using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace OpenAI
{
    /// <summary>
    /// Simple test component for OpenAI integration.
    /// Add this to a GameObject in a test scene to quickly test translation and TTS.
    /// </summary>
    public class OpenAITest : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("OpenAI Manager (will use Instance if not assigned)")]
        public OpenAIManager openAIManager;

        [Header("UI References (Optional)")]
        [Tooltip("Input field for text to translate")]
        public TMP_InputField inputField;

        [Tooltip("Text to display translated result")]
        public TextMeshProUGUI resultText;

        [Tooltip("Button to trigger translation")]
        public Button translateButton;

        [Tooltip("Button to trigger text-to-speech")]
        public Button speakButton;

        [Tooltip("Button to trigger translate and speak")]
        public Button translateAndSpeakButton;

        [Tooltip("Dropdown for target language")]
        public TMP_Dropdown languageDropdown;

        [Header("Test Settings")]
        [Tooltip("Default test text")]
        public string testText = "Hello, how are you today?";

        [Tooltip("Default target language")]
        public string targetLanguage = "Spanish";

        [Header("Audio")]
        [Tooltip("AudioSource to play TTS audio. If not assigned, will be created automatically.")]
        public AudioSource audioSource;

        private void Start()
        {
            // Get or create audio source for TTS
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                // Configure for maximum volume and best compatibility
                audioSource.volume = 1.0f;
                audioSource.spatialBlend = 0f; // 2D sound
                audioSource.priority = 0; // Highest priority
                audioSource.mute = false;
                audioSource.bypassEffects = true;
                audioSource.bypassListenerEffects = true;
                audioSource.bypassReverbZones = true;
                Debug.Log("Created AudioSource component automatically for TTS playback (configured for maximum volume).");
            }

            // Wait a frame to ensure OpenAIManager has initialized
            StartCoroutine(InitializeAfterFrame());
        }

        private System.Collections.IEnumerator InitializeAfterFrame()
        {
            yield return null; // Wait one frame for Awake to complete
            
            // Get manager instance
            if (openAIManager == null)
            {
                openAIManager = OpenAIManager.Instance;
            }

            if (openAIManager == null)
            {
                Debug.LogWarning("OpenAIManager.Instance is still null after waiting. Trying to find it in scene...");
                openAIManager = FindObjectOfType<OpenAIManager>();
                if (openAIManager != null)
                {
                    Debug.Log("Found OpenAIManager in scene!");
                }
            }

            // Setup UI buttons if assigned
            if (translateButton != null)
            {
                translateButton.onClick.AddListener(OnTranslateButtonClicked);
            }

            if (speakButton != null)
            {
                speakButton.onClick.AddListener(OnSpeakButtonClicked);
            }

            if (translateAndSpeakButton != null)
            {
                translateAndSpeakButton.onClick.AddListener(OnTranslateAndSpeakButtonClicked);
            }

            // Setup language dropdown
            if (languageDropdown != null)
            {
                languageDropdown.options.Clear();
                languageDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "Spanish", "French", "German", "Italian", "Portuguese",
                    "Japanese", "Chinese", "Korean", "Arabic", "Hindi",
                    "Russian", "Dutch", "Swedish", "Norwegian"
                });
                languageDropdown.value = 0; // Spanish
            }

            // Auto-test on start (optional - comment out if you don't want this)
            // StartCoroutine(AutoTest());
        }

        /// <summary>
        /// Test translation
        /// </summary>
        public void TestTranslation()
        {
            // Try multiple ways to get the manager
            if (openAIManager == null)
            {
                openAIManager = OpenAIManager.Instance;
            }

            if (openAIManager == null)
            {
                openAIManager = FindObjectOfType<OpenAIManager>();
            }

            if (openAIManager == null)
            {
                Debug.LogError("OpenAI Manager is not assigned or not found in scene! " +
                             "Make sure you have a GameObject with OpenAIManager component in the scene.");
                return;
            }

            // Check if initialized
            if (!openAIManager.IsInitialized)
            {
                Debug.LogWarning("OpenAI Manager exists but is not initialized. Attempting to re-initialize...");
                openAIManager.Reinitialize();
                
                // Wait a moment and check again
                if (!openAIManager.IsInitialized)
                {
                    Debug.LogError("OpenAI Manager failed to initialize. Check Console for details.");
                    return;
                }
            }

            string textToTranslate = inputField != null ? inputField.text : testText;
            string language = languageDropdown != null ? 
                languageDropdown.options[languageDropdown.value].text : targetLanguage;

            Debug.Log($"Testing translation: '{textToTranslate}' to {language}");

            openAIManager.Translate(
                textToTranslate,
                translatedText =>
                {
                    Debug.Log($"✅ Translation successful: {translatedText}");
                    if (resultText != null)
                    {
                        resultText.text = translatedText;
                    }
                },
                error =>
                {
                    Debug.LogError($"❌ Translation failed: {error}");
                    if (resultText != null)
                    {
                        resultText.text = $"Error: {error}";
                    }
                },
                language
            );
        }

        /// <summary>
        /// Test text-to-speech
        /// </summary>
        public void TestTextToSpeech()
        {
            // Try multiple ways to get the manager
            if (openAIManager == null)
            {
                openAIManager = OpenAIManager.Instance;
            }

            if (openAIManager == null)
            {
                openAIManager = FindObjectOfType<OpenAIManager>();
            }

            if (openAIManager == null)
            {
                Debug.LogError("OpenAI Manager is not assigned or not found in scene! " +
                             "Make sure you have a GameObject with OpenAIManager component in the scene.");
                return;
            }

            // Check if initialized
            if (!openAIManager.IsInitialized)
            {
                Debug.LogWarning("OpenAI Manager exists but is not initialized. Attempting to re-initialize...");
                openAIManager.Reinitialize();
                
                if (!openAIManager.IsInitialized)
                {
                    Debug.LogError("OpenAI Manager failed to initialize. Check Console for details.");
                    return;
                }
            }

            string textToSpeak = inputField != null ? inputField.text : testText;

            Debug.Log($"Testing TTS: '{textToSpeak}'");

            openAIManager.Speak(
                textToSpeak,
                audioClip =>
                {
                    if (audioClip != null)
                    {
                        Debug.Log($"✅ TTS successful! Playing audio...");
                        
                        // Ensure audioSource is available
                        if (audioSource == null)
                        {
                            audioSource = GetComponent<AudioSource>();
                            if (audioSource == null)
                            {
                                audioSource = gameObject.AddComponent<AudioSource>();
                            }
                        }
                        
                        // Configure AudioSource for best playback - MAXIMUM VOLUME
                        audioSource.clip = audioClip;
                        audioSource.volume = 1.0f; // Maximum volume
                        audioSource.spatialBlend = 0f; // 2D sound (not 3D)
                        audioSource.playOnAwake = false;
                        audioSource.loop = false;
                        audioSource.outputAudioMixerGroup = null; // Use default mixer (bypass any mixer that might reduce volume)
                        audioSource.priority = 0; // Highest priority
                        audioSource.mute = false; // Make sure not muted
                        audioSource.bypassEffects = true; // Bypass any effects that might reduce volume
                        audioSource.bypassListenerEffects = true;
                        audioSource.bypassReverbZones = true;
                        
                        // Verify clip has data before playing
                        if (audioClip.samples == 0)
                        {
                            Debug.LogError("❌ Audio clip has 0 samples! Cannot play empty audio.");
                            return;
                        }
                        
                        Debug.Log($"🎵 Audio clip info: {audioClip.length:F2}s, {audioClip.samples} samples, {audioClip.channels} channels, {audioClip.frequency}Hz");
                        
                        audioSource.Play();
                        Debug.Log($"🎵 Playing audio clip: {audioClip.name} at volume {audioSource.volume}");
                        
                        // Wait a frame and verify it's actually playing
                        StartCoroutine(VerifyAudioPlaying(audioSource));
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ TTS returned null audio clip. Check MP3 support.");
                    }
                },
                error =>
                {
                    Debug.LogError($"❌ TTS failed: {error}");
                }
            );
        }

        /// <summary>
        /// Test translate and speak
        /// </summary>
        public void TestTranslateAndSpeak()
        {
            // Try multiple ways to get the manager
            if (openAIManager == null)
            {
                openAIManager = OpenAIManager.Instance;
            }

            if (openAIManager == null)
            {
                openAIManager = FindObjectOfType<OpenAIManager>();
            }

            if (openAIManager == null)
            {
                Debug.LogError("OpenAI Manager is not assigned or not found in scene! " +
                             "Make sure you have a GameObject with OpenAIManager component in the scene.");
                return;
            }

            // Check if initialized
            if (!openAIManager.IsInitialized)
            {
                Debug.LogWarning("OpenAI Manager exists but is not initialized. Attempting to re-initialize...");
                openAIManager.Reinitialize();
                
                if (!openAIManager.IsInitialized)
                {
                    Debug.LogError("OpenAI Manager failed to initialize. Check Console for details.");
                    return;
                }
            }

            string textToTranslate = inputField != null ? inputField.text : testText;
            string language = languageDropdown != null ? 
                languageDropdown.options[languageDropdown.value].text : targetLanguage;

            Debug.Log($"Testing translate and speak: '{textToTranslate}' to {language}");

            openAIManager.TranslateAndSpeak(
                textToTranslate,
                audioClip =>
                {
                    if (audioClip != null)
                    {
                        Debug.Log($"✅ Translate and speak successful! Playing audio...");
                        
                        // Ensure audioSource is available
                        if (audioSource == null)
                        {
                            audioSource = GetComponent<AudioSource>();
                            if (audioSource == null)
                            {
                                audioSource = gameObject.AddComponent<AudioSource>();
                            }
                        }
                        
                        // Configure AudioSource for best playback - MAXIMUM VOLUME
                        audioSource.clip = audioClip;
                        audioSource.volume = 1.0f; // Maximum volume
                        audioSource.spatialBlend = 0f; // 2D sound (not 3D)
                        audioSource.playOnAwake = false;
                        audioSource.loop = false;
                        audioSource.outputAudioMixerGroup = null; // Use default mixer (bypass any mixer that might reduce volume)
                        audioSource.priority = 0; // Highest priority
                        audioSource.mute = false; // Make sure not muted
                        audioSource.bypassEffects = true; // Bypass any effects that might reduce volume
                        audioSource.bypassListenerEffects = true;
                        audioSource.bypassReverbZones = true;
                        
                        // Verify clip has data before playing
                        if (audioClip.samples == 0)
                        {
                            Debug.LogError("❌ Audio clip has 0 samples! Cannot play empty audio.");
                            return;
                        }
                        
                        Debug.Log($"🎵 Audio clip info: {audioClip.length:F2}s, {audioClip.samples} samples, {audioClip.channels} channels, {audioClip.frequency}Hz");
                        
                        audioSource.Play();
                        Debug.Log($"🎵 Playing translated audio clip: {audioClip.name} at volume {audioSource.volume}");
                        
                        // Wait a frame and verify it's actually playing
                        StartCoroutine(VerifyAudioPlaying(audioSource));
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ TTS returned null audio clip. Check MP3 support.");
                    }
                },
                error =>
                {
                    Debug.LogError($"❌ Translate and speak failed: {error}");
                },
                language
            );
        }

        // Button click handlers
        private void OnTranslateButtonClicked()
        {
            TestTranslation();
        }

        private void OnSpeakButtonClicked()
        {
            TestTextToSpeech();
        }

        private void OnTranslateAndSpeakButtonClicked()
        {
            TestTranslateAndSpeak();
        }

        // Quick test methods (can be called from Inspector or code)
        [ContextMenu("Quick Test - Translate")]
        private void QuickTestTranslate()
        {
            TestTranslation();
        }

        [ContextMenu("Quick Test - TTS")]
        private void QuickTestTTS()
        {
            TestTextToSpeech();
        }

        [ContextMenu("Quick Test - Translate & Speak")]
        private void QuickTestTranslateAndSpeak()
        {
            TestTranslateAndSpeak();
        }

        /// <summary>
        /// Verify audio is actually playing after a frame
        /// </summary>
        private System.Collections.IEnumerator VerifyAudioPlaying(AudioSource source)
        {
            yield return null; // Wait one frame
            
            if (source != null && source.clip != null)
            {
                if (source.isPlaying)
                {
                    Debug.Log($"✅ AudioSource is playing! Clip: {source.clip.name}, Time: {source.time:F2}s / {source.clip.length:F2}s");
                }
                else
                {
                    Debug.LogWarning("⚠️ AudioSource.Play() was called but isPlaying is false after one frame.");
                    Debug.LogWarning("   This usually means:");
                    Debug.LogWarning("   1. Audio clip has no data (0 samples)");
                    Debug.LogWarning("   2. Unity Audio is disabled");
                    Debug.LogWarning("   3. Audio output device issue");
                    Debug.LogWarning($"   Clip samples: {source.clip.samples}, Length: {source.clip.length:F2}s");
                }
            }
        }
    }
}

