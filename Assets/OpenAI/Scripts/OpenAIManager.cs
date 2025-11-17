using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace OpenAI
{
    /// <summary>
    /// Main manager for OpenAI services (Translation and Text-to-Speech)
    /// Attach this to a GameObject in your scene or use as a singleton
    /// </summary>
    public class OpenAIManager : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("OpenAI Configuration asset with API key")]
        public OpenAIConfig config;

        [Header("Settings")]
        [Tooltip("Default target language for translations")]
        public string defaultTargetLanguage = "Spanish";

        [Tooltip("Default source language for translations")]
        public string defaultSourceLanguage = "English";

        private OpenAIClient client;
        private TranslationService translationService;
        private TextToSpeechService ttsService;
        private bool isInitialized = false;

        public static OpenAIManager Instance { get; private set; }
        
        /// <summary>
        /// Check if the manager is properly initialized
        /// </summary>
        public bool IsInitialized => isInitialized;

        /// <summary>
        /// Manually re-initialize the manager (useful if config was changed at runtime)
        /// </summary>
        [ContextMenu("Re-initialize")]
        public void Reinitialize()
        {
            Initialize();
        }

        private void Awake()
        {
            // If Instance is already set and it's not this object, destroy this one
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple OpenAIManager instances detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            // Set this as the instance
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void OnDestroy()
        {
            // Only clear Instance if this is the current instance
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Initialize()
        {
            isInitialized = false;

            Debug.Log("Starting OpenAI Manager initialization...");

            if (config == null)
            {
                Debug.LogError("[OpenAI] Config is not assigned! Please assign the OpenAI Config asset in the Inspector.");
                return;
            }

            Debug.Log($"[OpenAI] Config assigned: {config.name}");

            if (!config.IsConfigured())
            {
                Debug.LogError("[OpenAI] API key is not configured in the config asset! Please enter your API key.");
                return;
            }

            Debug.Log("[OpenAI] API key is configured.");

            try
            {
                client = new OpenAIClient(config);
                translationService = new TranslationService(client, config);
                ttsService = new TextToSpeechService(client, config);
                isInitialized = true;

                Debug.Log("[OpenAI] ✅ Manager initialized successfully!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[OpenAI] ❌ Failed to initialize: {ex.Message}\nStack trace: {ex.StackTrace}");
                isInitialized = false;
            }
        }

        /// <summary>
        /// Translate text to target language
        /// </summary>
        public void Translate(string text, Action<string> onSuccess, Action<string> onError = null, 
            string targetLanguage = null, string sourceLanguage = null)
        {
            if (!isInitialized || translationService == null)
            {
                string errorMsg = "OpenAI Manager not initialized. ";
                if (config == null)
                    errorMsg += "Config is not assigned.";
                else if (!config.IsConfigured())
                    errorMsg += "API key is not configured.";
                else
                    errorMsg += "Initialization failed. Check Console for details.";
                    
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
                return;
            }

            string target = string.IsNullOrEmpty(targetLanguage) ? defaultTargetLanguage : targetLanguage;
            string source = string.IsNullOrEmpty(sourceLanguage) ? defaultSourceLanguage : sourceLanguage;

            StartCoroutine(translationService.TranslateText(text, target, source, onSuccess, onError));
        }

        /// <summary>
        /// Convert text to speech
        /// </summary>
        public void Speak(string text, Action<AudioClip> onSuccess, Action<string> onError = null, string voice = null)
        {
            if (!isInitialized || ttsService == null)
            {
                string errorMsg = "OpenAI Manager not initialized. ";
                if (config == null)
                    errorMsg += "Config is not assigned.";
                else if (!config.IsConfigured())
                    errorMsg += "API key is not configured.";
                else
                    errorMsg += "Initialization failed. Check Console for details.";
                    
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
                return;
            }

            StartCoroutine(ttsService.TextToSpeech(text, voice, onSuccess, onError));
        }

        /// <summary>
        /// Translate and then speak the translated text
        /// </summary>
        public void TranslateAndSpeak(string text, Action<AudioClip> onSuccess, Action<string> onError = null,
            string targetLanguage = null, string sourceLanguage = null, string voice = null)
        {
            Translate(text,
                translatedText =>
                {
                    Speak(translatedText, onSuccess, onError, voice);
                },
                onError,
                targetLanguage,
                sourceLanguage);
        }
    }
}

