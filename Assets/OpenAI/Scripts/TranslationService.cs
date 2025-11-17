using System;
using System.Collections;
using UnityEngine;

namespace OpenAI
{
    /// <summary>
    /// Service for translating text using OpenAI API
    /// </summary>
    public class TranslationService
    {
        private readonly OpenAIClient client;
        private readonly OpenAIConfig config;

        public TranslationService(OpenAIClient client, OpenAIConfig config)
        {
            this.client = client;
            this.config = config;
        }

        /// <summary>
        /// Translate text to target language
        /// </summary>
        /// <param name="text">Text to translate</param>
        /// <param name="targetLanguage">Target language (e.g., "Spanish", "French", "German", "Japanese")</param>
        /// <param name="sourceLanguage">Source language (optional, e.g., "English")</param>
        /// <param name="onSuccess">Callback with translated text</param>
        /// <param name="onError">Callback with error message</param>
        public IEnumerator TranslateText(string text, string targetLanguage, string sourceLanguage = "English", 
            Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                onError?.Invoke("Text to translate is empty");
                yield break;
            }

            string prompt = $"Translate the following text from {sourceLanguage} to {targetLanguage}. " +
                          $"Only return the translated text, nothing else:\n\n{text}";

            // Create JSON manually since Unity's JsonUtility doesn't handle nested arrays well
            string jsonBody = $"{{\"model\":\"{config.translationModel}\"," +
                            $"\"messages\":[{{\"role\":\"user\",\"content\":\"{EscapeJsonString(prompt)}\"}}]," +
                            $"\"temperature\":0.3," +
                            $"\"max_tokens\":1000}}";

            yield return client.PostRequest("chat/completions", jsonBody,
                response =>
                {
                    try
                    {
                        // Parse JSON response manually (Unity's JsonUtility doesn't handle nested arrays well)
                        var responseObj = ParseTranslationResponse(response);
                        if (responseObj != null && !string.IsNullOrEmpty(responseObj.translatedText))
                        {
                            onSuccess?.Invoke(responseObj.translatedText);
                        }
                        else
                        {
                            onError?.Invoke("Invalid response format from API");
                        }
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke($"Failed to parse response: {ex.Message}");
                    }
                },
                onError);
        }

        private TranslationResult ParseTranslationResponse(string json)
        {
            // Simple JSON parsing for OpenAI response
            // Looking for: "content": "translated text"
            int contentIndex = json.IndexOf("\"content\"");
            if (contentIndex == -1) return null;

            int colonIndex = json.IndexOf(':', contentIndex);
            int startQuote = json.IndexOf('"', colonIndex) + 1;
            int endQuote = json.IndexOf('"', startQuote);

            if (startQuote > 0 && endQuote > startQuote)
            {
                string content = json.Substring(startQuote, endQuote - startQuote);
                // Unescape JSON strings
                content = content.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");
                return new TranslationResult { translatedText = content.Trim() };
            }

            return null;
        }

        /// <summary>
        /// Escape JSON string for safe serialization
        /// </summary>
        private string EscapeJsonString(string input)
        {
            return input.Replace("\\", "\\\\")
                       .Replace("\"", "\\\"")
                       .Replace("\n", "\\n")
                       .Replace("\r", "\\r")
                       .Replace("\t", "\\t");
        }

        [Serializable]
        private class TranslationResult
        {
            public string translatedText;
        }
    }
}

