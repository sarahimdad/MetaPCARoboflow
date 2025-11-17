using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace OpenAI
{
    /// <summary>
    /// HTTP client for making requests to OpenAI API
    /// </summary>
    public class OpenAIClient
    {
        private readonly OpenAIConfig config;

        public OpenAIClient(OpenAIConfig config)
        {
            this.config = config;
        }

        /// <summary>
        /// Make a POST request to OpenAI API
        /// </summary>
        public IEnumerator PostRequest(string endpoint, string jsonBody, Action<string> onSuccess, Action<string> onError)
        {
            string apiKey = config.GetApiKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                onError?.Invoke("API key is not configured");
                yield break;
            }

            string url = $"{config.apiBaseUrl}/{endpoint}";
            
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    onSuccess?.Invoke(request.downloadHandler.text);
                }
                else
                {
                    string errorMessage = $"Error: {request.error}\nResponse: {request.downloadHandler.text}";
                    Debug.LogError($"OpenAI API Error: {errorMessage}");
                    onError?.Invoke(errorMessage);
                }
            }
        }
    }
}

