using UnityEngine;

namespace OpenAI
{
    /// <summary>
    /// Helper to test if Unity's audio system is working
    /// </summary>
    public class AudioTestHelper : MonoBehaviour
    {
        [ContextMenu("Test Audio System")]
        public void TestAudioSystem()
        {
            Debug.Log("=== Testing Unity Audio System ===");
            
            // Check if audio is disabled
            if (AudioSettings.GetConfiguration().dspBufferSize == 0)
            {
                Debug.LogWarning("⚠️ Audio might be disabled or misconfigured");
            }
            else
            {
                Debug.Log($"✅ Audio system is active. DSP Buffer Size: {AudioSettings.GetConfiguration().dspBufferSize}");
            }
            
            // Try to create a simple test tone
            AudioSource testSource = GetComponent<AudioSource>();
            if (testSource == null)
            {
                testSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Create a simple sine wave tone (440Hz for 0.5 seconds)
            int sampleRate = 44100;
            float duration = 0.5f;
            float frequency = 440f; // A4 note
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples * 2]; // Stereo
            
            for (int i = 0; i < samples; i++)
            {
                float sample = Mathf.Sin(2f * Mathf.PI * frequency * i / sampleRate);
                data[i * 2] = sample * 0.5f; // Left channel
                data[i * 2 + 1] = sample * 0.5f; // Right channel
            }
            
            AudioClip testClip = AudioClip.Create("TestTone", samples, 2, sampleRate, false);
            testClip.SetData(data, 0);
            
            testSource.clip = testClip;
            testSource.volume = 1.0f;
            testSource.spatialBlend = 0f; // 2D
            testSource.Play();
            
            if (testSource.isPlaying)
            {
                Debug.Log("✅ Test tone is playing! You should hear a beep. If not, check:");
                Debug.Log("   1. System volume");
                Debug.Log("   2. Unity's Audio settings (Edit > Project Settings > Audio)");
                Debug.Log("   3. Audio output device");
            }
            else
            {
                Debug.LogError("❌ Test tone failed to play. Audio system might be disabled.");
            }
            
            Debug.Log("=== End Audio Test ===");
        }
    }
}

