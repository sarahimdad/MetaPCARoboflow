using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;
using OpenAI;

namespace TutorialSystem
{
    /// <summary>
    /// Component to display tutorial data on a UI prefab.
    /// Attach this to your tutorial UI prefab and assign the UI elements.
    /// </summary>
    public class TutorialDisplay : MonoBehaviour
    {
        [Header("Tutorial Data")]
        [Tooltip("The tutorial data to display")]
        public TutorialData tutorialData;

        [Header("UI References - Title")]
        [Tooltip("TextMeshPro component for the tutorial title (optional)")]
        public TextMeshProUGUI titleTextMeshPro;

        [Tooltip("Unity Text component for the tutorial title (optional, if not using TextMeshPro)")]
        public Text titleText;

        [Header("UI References - Current Step")]
        [Tooltip("TextMeshPro component for the current step number (optional)")]
        public TextMeshProUGUI stepNumberTextMeshPro;

        [Tooltip("Unity Text component for the current step number (optional, if not using TextMeshPro)")]
        public Text stepNumberText;

        [Tooltip("TextMeshPro component for the current step title (optional)")]
        public TextMeshProUGUI stepTitleTextMeshPro;

        [Tooltip("Unity Text component for the current step title (optional, if not using TextMeshPro)")]
        public Text stepTitleText;

        [Tooltip("TextMeshPro component for the current step text (optional)")]
        public TextMeshProUGUI stepTextTextMeshPro;

        [Tooltip("Unity Text component for the current step text (optional, if not using TextMeshPro)")]
        public Text stepTextText;

        [Header("UI References - Step Media")]
        [Tooltip("Unity Image component to display step image/sprite (optional)")]
        public Image stepImage;

        [Tooltip("Unity VideoPlayer component to display step video (optional)")]
        public VideoPlayer stepVideoPlayer;

        [Tooltip("GameObject containing the image display (will be shown/hidden based on media type)")]
        public GameObject imageDisplayObject;

        [Tooltip("GameObject containing the video player (will be shown/hidden based on media type)")]
        public GameObject videoDisplayObject;

        [Header("Text-to-Speech Settings")]
        [Tooltip("Enable automatic text-to-speech for tutorial steps")]
        public bool enableTTS = true;

        [Tooltip("What to speak: Title only, Text only, or Both")]
        public TTSSpeakMode ttsSpeakMode = TTSSpeakMode.TextOnly;

        [Tooltip("AudioSource to play the TTS audio. If not assigned, will try to get or create one.")]
        public AudioSource audioSource;

        [Tooltip("OpenAI voice to use (alloy, echo, fable, onyx, nova, shimmer). Leave empty to use config default.")]
        public string ttsVoice = "";

        [Header("Navigation")]
        [Tooltip("Current step index (0-based). Use DisplayStep() to change.")]
        [SerializeField] private int currentStepIndex = 0;

        /// <summary>
        /// What content to speak when displaying a step
        /// </summary>
        public enum TTSSpeakMode
        {
            TitleOnly,
            TextOnly,
            Both
        }

        /// <summary>
        /// Current step index (0-based)
        /// </summary>
        public int CurrentStepIndex
        {
            get => currentStepIndex;
            private set => currentStepIndex = value;
        }

        /// <summary>
        /// Total number of steps in the current tutorial
        /// </summary>
        public int TotalSteps => tutorialData != null ? tutorialData.StepCount : 0;

        /// <summary>
        /// Check if there's a next step
        /// </summary>
        public bool HasNextStep => CurrentStepIndex < TotalSteps - 1;

        /// <summary>
        /// Check if there's a previous step
        /// </summary>
        public bool HasPreviousStep => CurrentStepIndex > 0;

        private AudioClip currentAudioClip;
        private bool isSpeaking = false;

        private void Start()
        {
            // Set up AudioSource if not assigned
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            // Configure AudioSource for TTS (non-spatial, 2D sound)
            if (audioSource != null)
            {
                audioSource.spatialBlend = 0f; // 2D sound (not 3D)
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                audioSource.volume = 1f;
                Debug.Log("TutorialDisplay: AudioSource configured for TTS");
            }

            // Check for AudioListener in scene
            AudioListener listener = FindObjectOfType<AudioListener>();
            if (listener == null)
            {
                Debug.LogWarning("TutorialDisplay: ⚠️ No AudioListener found in scene! Audio will not be heard. " +
                    "Add an AudioListener component to your Main Camera or OVRCameraRig.");
            }
            else
            {
                Debug.Log($"TutorialDisplay: ✅ AudioListener found on: {listener.gameObject.name}");
            }

            if (tutorialData != null)
            {
                DisplayTutorial();
            }
        }

        private void OnDestroy()
        {
            // Stop any playing audio when destroyed
            StopSpeaking();
        }

        /// <summary>
        /// Load and display a tutorial data asset
        /// </summary>
        public void LoadTutorial(TutorialData data)
        {
            tutorialData = data;
            currentStepIndex = 0;
            DisplayTutorial();
        }

        /// <summary>
        /// Display the tutorial title and show the first step
        /// </summary>
        public void DisplayTutorial()
        {
            if (tutorialData == null)
            {
                Debug.LogWarning("TutorialDisplay: No tutorial data assigned!");
                return;
            }

            // Display tutorial title
            SetText(titleTextMeshPro, titleText, tutorialData.tutorialTitle);

            // Display first step
            if (tutorialData.StepCount > 0)
            {
                DisplayStep(0);
            }
        }

        /// <summary>
        /// Display a specific step by index (0-based)
        /// </summary>
        public void DisplayStep(int stepIndex)
        {
            if (tutorialData == null)
            {
                Debug.LogWarning("TutorialDisplay: No tutorial data assigned!");
                return;
            }

            var step = tutorialData.GetStep(stepIndex);
            if (step == null)
            {
                Debug.LogWarning($"TutorialDisplay: Step {stepIndex} not found!");
                return;
            }

            CurrentStepIndex = stepIndex;

            // Display step number in "Step X/Y" format
            string stepNumberDisplay = $"Step {CurrentStepIndex + 1}/{TotalSteps}";
            SetText(stepNumberTextMeshPro, stepNumberText, stepNumberDisplay);

            // Display step title
            SetText(stepTitleTextMeshPro, stepTitleText, step.stepTitle);

            // Display step text
            SetText(stepTextTextMeshPro, stepTextText, step.stepText);

            // Display step media (image or video)
            DisplayStepMedia(step);

            // Speak the step text using TTS
            if (enableTTS)
            {
                SpeakStep(step);
            }
        }

        /// <summary>
        /// Display the media (image or video) for a step
        /// Safe to use just image, just video, both, or neither - all cases are handled
        /// </summary>
        private void DisplayStepMedia(TutorialStep step)
        {
            bool hasImage = step.stepImage != null;
            bool hasVideo = step.stepVideo != null;

            // Handle image display
            if (hasImage && stepImage != null)
            {
                // Convert Texture2D to Sprite if needed
                Texture2D texture = step.stepImage;
                if (texture != null)
                {
                    // Create a sprite from the texture
                    Sprite sprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),
                        100f // pixels per unit
                    );
                    stepImage.sprite = sprite;
                    stepImage.enabled = true;
                }
            }
            else if (stepImage != null)
            {
                stepImage.enabled = false;
            }

            // Handle video display
            if (hasVideo && stepVideoPlayer != null)
            {
                stepVideoPlayer.clip = step.stepVideo;
                stepVideoPlayer.isLooping = true; // Enable looping
                stepVideoPlayer.Play(); // Play automatically
            }
            else if (stepVideoPlayer != null)
            {
                stepVideoPlayer.Stop();
            }

            // Show/hide display objects if assigned
            if (imageDisplayObject != null)
            {
                imageDisplayObject.SetActive(hasImage);
            }

            if (videoDisplayObject != null)
            {
                videoDisplayObject.SetActive(hasVideo);
            }
        }

        /// <summary>
        /// Display the next step
        /// </summary>
        public void NextStep()
        {
            if (HasNextStep)
            {
                // Stop current audio immediately when navigating
                StopSpeaking();
                DisplayStep(CurrentStepIndex + 1);
            }
        }

        /// <summary>
        /// Display the previous step
        /// </summary>
        public void PreviousStep()
        {
            if (HasPreviousStep)
            {
                // Stop current audio immediately when navigating
                StopSpeaking();
                DisplayStep(CurrentStepIndex - 1);
            }
        }

        /// <summary>
        /// Display the first step
        /// </summary>
        public void FirstStep()
        {
            if (tutorialData != null && tutorialData.StepCount > 0)
            {
                // Stop current audio immediately when navigating
                StopSpeaking();
                DisplayStep(0);
            }
        }

        /// <summary>
        /// Display the last step
        /// </summary>
        public void LastStep()
        {
            if (tutorialData != null && tutorialData.StepCount > 0)
            {
                // Stop current audio immediately when navigating
                StopSpeaking();
                DisplayStep(tutorialData.StepCount - 1);
            }
        }

        /// <summary>
        /// Helper method to set text on either TextMeshPro or Unity Text component
        /// </summary>
        private void SetText(TextMeshProUGUI tmpComponent, Text textComponent, string text)
        {
            if (tmpComponent != null)
            {
                tmpComponent.text = text;
            }
            else if (textComponent != null)
            {
                textComponent.text = text;
            }
        }

        /// <summary>
        /// Get the current step data
        /// </summary>
        public TutorialStep GetCurrentStep()
        {
            if (tutorialData == null)
                return null;

            return tutorialData.GetStep(CurrentStepIndex);
        }

        /// <summary>
        /// Speak the step content using OpenAI TTS
        /// </summary>
        private void SpeakStep(TutorialStep step)
        {
            if (step == null)
                return;

            // Stop any currently playing audio immediately
            StopSpeaking();

            // Build the text to speak based on mode
            string textToSpeak = "";
            switch (ttsSpeakMode)
            {
                case TTSSpeakMode.TitleOnly:
                    textToSpeak = step.stepTitle;
                    break;
                case TTSSpeakMode.TextOnly:
                    textToSpeak = step.stepText;
                    break;
                case TTSSpeakMode.Both:
                    textToSpeak = $"{step.stepTitle}. {step.stepText}";
                    break;
            }

            // Skip if text is empty
            if (string.IsNullOrEmpty(textToSpeak))
            {
                Debug.LogWarning("TutorialDisplay: No text to speak for this step.");
                return;
            }

            // Check if OpenAI Manager is available
            if (OpenAIManager.Instance == null)
            {
                Debug.LogWarning("TutorialDisplay: OpenAIManager.Instance is not available. TTS will not work.");
                return;
            }

            if (!OpenAIManager.Instance.IsInitialized)
            {
                Debug.LogWarning("TutorialDisplay: OpenAIManager is not initialized. TTS will not work.");
                return;
            }

            // Use TTS
            isSpeaking = true;
            string voiceToUse = string.IsNullOrEmpty(ttsVoice) ? null : ttsVoice;

            OpenAIManager.Instance.Speak(
                textToSpeak,
                audioClip =>
                {
                    if (audioClip != null && audioSource != null)
                    {
                        currentAudioClip = audioClip;
                        audioSource.clip = audioClip;
                        
                        // Ensure AudioSource is configured correctly before playing
                        audioSource.spatialBlend = 0f; // 2D sound
                        audioSource.volume = 1f;
                        
                        audioSource.Play();
                        
                        Debug.Log($"TutorialDisplay: ✅ Speaking step {CurrentStepIndex + 1} - '{textToSpeak.Substring(0, Mathf.Min(50, textToSpeak.Length))}...'");
                        Debug.Log($"TutorialDisplay: AudioClip - Length: {audioClip.length:F2}s, Channels: {audioClip.channels}, Frequency: {audioClip.frequency}Hz, Samples: {audioClip.samples}");
                        Debug.Log($"TutorialDisplay: AudioSource - IsPlaying: {audioSource.isPlaying}, Volume: {audioSource.volume}, SpatialBlend: {audioSource.spatialBlend}");
                        
                        // Verify AudioListener exists
                        AudioListener listener = FindObjectOfType<AudioListener>();
                        if (listener == null)
                        {
                            Debug.LogError("TutorialDisplay: ❌ No AudioListener in scene! Audio will not be heard.");
                        }
                    }
                    else
                    {
                        Debug.LogError($"TutorialDisplay: ❌ Failed to get audio clip or AudioSource is null. AudioClip: {audioClip != null}, AudioSource: {audioSource != null}");
                        isSpeaking = false;
                    }
                },
                error =>
                {
                    Debug.LogError($"TutorialDisplay: ❌ TTS Error: {error}");
                    isSpeaking = false;
                },
                voiceToUse
            );
        }

        /// <summary>
        /// Stop speaking the current step immediately
        /// </summary>
        public void StopSpeaking()
        {
            // Stop audio immediately
            if (audioSource != null)
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
                // Clear the clip to ensure it stops completely
                audioSource.clip = null;
            }

            // Clean up current audio clip
            if (currentAudioClip != null)
            {
                // Note: We don't destroy the clip here as it might be reused
                // The clip will be replaced when a new one is loaded
                currentAudioClip = null;
            }

            isSpeaking = false;
        }

        /// <summary>
        /// Check if TTS is currently speaking
        /// </summary>
        public bool IsSpeaking => isSpeaking || (audioSource != null && audioSource.isPlaying);

        /// <summary>
        /// Manually trigger TTS for the current step
        /// </summary>
        public void SpeakCurrentStep()
        {
            var step = GetCurrentStep();
            if (step != null && enableTTS)
            {
                SpeakStep(step);
            }
        }
    }
}

