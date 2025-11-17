using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace OpenAI
{
    /// <summary>
    /// Service for converting text to speech using OpenAI API
    /// </summary>
    public class TextToSpeechService
    {
        private readonly OpenAIClient client;
        private readonly OpenAIConfig config;

        public TextToSpeechService(OpenAIClient client, OpenAIConfig config)
        {
            this.client = client;
            this.config = config;
        }

        /// <summary>
        /// Convert text to speech audio
        /// </summary>
        /// <param name="text">Text to convert to speech</param>
        /// <param name="voice">Voice to use (alloy, echo, fable, onyx, nova, shimmer). If null, uses config default.</param>
        /// <param name="onSuccess">Callback with audio clip</param>
        /// <param name="onError">Callback with error message</param>
        public IEnumerator TextToSpeech(string text, string voice = null, 
            Action<AudioClip> onSuccess = null, Action<string> onError = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                onError?.Invoke("Text is empty");
                yield break;
            }

            string selectedVoice = string.IsNullOrEmpty(voice) ? config.ttsVoice : voice;
            string model = config.ttsModel;

            // Create JSON manually for TTS request
            string jsonBody = $"{{\"model\":\"{model}\",\"input\":\"{EscapeJsonString(text)}\",\"voice\":\"{selectedVoice}\"}}";
            string url = $"{config.apiBaseUrl}/audio/speech";

            string apiKey = config.GetApiKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                onError?.Invoke("API key is not configured");
                yield break;
            }

            // Use UnityWebRequest with proper setup for POST request
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    // OpenAI TTS returns audio as MP3 data
                    byte[] audioData = request.downloadHandler.data;
                    AudioClip clip = LoadAudioFromBytes(audioData);
                    if (clip != null)
                    {
                        onSuccess?.Invoke(clip);
                    }
                    else
                    {
                        onError?.Invoke("Failed to create audio clip from response");
                    }
                }
                else
                {
                    string errorMessage = $"Error: {request.error}\nResponse: {request.downloadHandler.text}";
                    Debug.LogError($"OpenAI TTS Error: {errorMessage}");
                    onError?.Invoke(errorMessage);
                }
            }
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

        /// <summary>
        /// Load audio clip from MP3 bytes
        /// Note: OpenAI TTS returns MP3, but Unity doesn't natively support MP3.
        /// This method attempts to use UnityWebRequestMultimedia to load it.
        /// For better support, consider using a MP3 decoder plugin.
        /// </summary>
        private AudioClip LoadAudioFromBytes(byte[] audioData)
        {
            if (audioData == null || audioData.Length == 0)
            {
                Debug.LogError("Audio data is null or empty!");
                return null;
            }

            Debug.Log($"Loading audio from {audioData.Length} bytes...");

            // Try to load as audio using UnityWebRequestMultimedia
            // This works for some formats but MP3 support varies by platform
            string tempPath = System.IO.Path.Combine(Application.temporaryCachePath, "tts_audio.mp3");
            
            try
            {
                System.IO.File.WriteAllBytes(tempPath, audioData);
                Debug.Log($"Saved audio to: {tempPath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to save audio file: {ex.Message}");
                return null;
            }

            // Use UnityWebRequestMultimedia to load audio
            // Note: MP3 support depends on platform and Unity version
            // For Quest/Android, you may need a MP3 decoder plugin
            string fileUrl = "file://" + tempPath;
            Debug.Log($"Loading audio from: {fileUrl}");
            
            // Try MPEG first, then fallback to UNKNOWN if needed
            AudioType audioType = AudioType.MPEG;
            
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileUrl, audioType))
            {
                var operation = www.SendWebRequest();
                while (!operation.isDone) { }

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    
                    if (clip != null)
                    {
                        Debug.Log($"✅ Audio clip loaded successfully! Name: {clip.name}, Length: {clip.length:F2}s, Channels: {clip.channels}, Frequency: {clip.frequency}Hz, Samples: {clip.samples}");
                        
                        // Verify clip has data
                        if (clip.samples == 0)
                        {
                            Debug.LogError("⚠️ Audio clip has 0 samples! The MP3 might not have loaded correctly.");
                        }
                    }
                    else
                    {
                        Debug.LogError("⚠️ DownloadHandlerAudioClip.GetContent returned null!");
                    }
                    
                    // Clean up temp file
                    try { System.IO.File.Delete(tempPath); } catch { }
                    return clip;
                }
                else
                {
                    Debug.LogWarning($"⚠️ Failed to load as MPEG: {www.error}. Trying UNKNOWN format...");
                    
                    // Try again with UNKNOWN type (sometimes works better)
                    using (UnityWebRequest www2 = UnityWebRequestMultimedia.GetAudioClip(fileUrl, AudioType.UNKNOWN))
                    {
                        var operation2 = www2.SendWebRequest();
                        while (!operation2.isDone) { }
                        
                        if (www2.result == UnityWebRequest.Result.Success)
                        {
                            AudioClip clip = DownloadHandlerAudioClip.GetContent(www2);
                            if (clip != null)
                            {
                                Debug.Log($"✅ Audio clip loaded with UNKNOWN type! Name: {clip.name}, Length: {clip.length:F2}s, Samples: {clip.samples}");
                                try { System.IO.File.Delete(tempPath); } catch { }
                                return clip;
                            }
                        }
                    }
                    
                    Debug.LogError($"❌ Failed to load MP3 audio with both MPEG and UNKNOWN types.");
                    Debug.LogError($"   Error: {www.error}");
                    Debug.LogWarning("💡 Solutions:");
                    Debug.LogWarning("   1. Check Unity's Audio settings (Edit > Project Settings > Audio)");
                    Debug.LogWarning("   2. Make sure 'Disable Unity Audio' is UNCHECKED");
                    Debug.LogWarning("   3. Try using a MP3 decoder plugin (e.g., NAudio, FFmpeg)");
                    Debug.LogWarning("   4. Or convert MP3 to WAV/OGG server-side");
                    Debug.LogWarning($"   5. Audio file saved at: {tempPath} (you can test it manually)");
                    
                    // Don't delete temp file so user can test it
                    return null;
                }
            }
        }
    }
}

