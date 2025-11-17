using UnityEngine;

namespace OpenAI
{
    /// <summary>
    /// Configuration ScriptableObject for OpenAI API settings.
    /// Create this asset and store your API key here (keep it secure, don't commit to git).
    /// </summary>
    [CreateAssetMenu(fileName = "OpenAI Config", menuName = "OpenAI/Configuration", order = 1)]
    public class OpenAIConfig : ScriptableObject
    {
        [Header("API Configuration")]
        [Tooltip("Your OpenAI API key. Keep this secure and don't commit to version control!")]
        [SerializeField] private string apiKey = "";

        [Tooltip("Base URL for OpenAI API (default: https://api.openai.com/v1)")]
        public string apiBaseUrl = "https://api.openai.com/v1";

        [Header("Default Settings")]
        [Tooltip("Default model for translations. Recommended: gpt-4o-mini (best balance), gpt-3.5-turbo (cheaper), gpt-4o (best quality)")]
        public string translationModel = "gpt-4o-mini";

        [Tooltip("Default model for text-to-speech (e.g., tts-1, tts-1-hd)")]
        public string ttsModel = "tts-1";

        [Tooltip("Default voice for text-to-speech (alloy, echo, fable, onyx, nova, shimmer)")]
        public string ttsVoice = "alloy";

        /// <summary>
        /// Get the API key (with validation)
        /// </summary>
        public string GetApiKey()
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogError("OpenAI API key is not set! Please configure it in the OpenAI Config asset.");
                return null;
            }
            return apiKey;
        }

        /// <summary>
        /// Set the API key (for runtime configuration if needed)
        /// </summary>
        public void SetApiKey(string key)
        {
            apiKey = key;
        }

        /// <summary>
        /// Check if API key is configured
        /// </summary>
        public bool IsConfigured()
        {
            return !string.IsNullOrEmpty(apiKey);
        }
    }
}

