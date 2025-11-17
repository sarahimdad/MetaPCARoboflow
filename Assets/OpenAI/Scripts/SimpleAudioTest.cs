using UnityEngine;

namespace OpenAI
{
    /// <summary>
    /// Simple test to verify audio works - plays a loud beep
    /// </summary>
    public class SimpleAudioTest : MonoBehaviour
    {
        [ContextMenu("Play Loud Beep Test")]
        public void PlayLoudBeep()
        {
            AudioSource source = GetComponent<AudioSource>();
            if (source == null)
            {
                source = gameObject.AddComponent<AudioSource>();
            }

            // Create a very loud test tone
            int sampleRate = 44100;
            float duration = 1.0f;
            float frequency = 440f; // A4 note
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples * 2]; // Stereo
            
            for (int i = 0; i < samples; i++)
            {
                float sample = Mathf.Sin(2f * Mathf.PI * frequency * i / sampleRate) * 0.8f; // 80% volume
                data[i * 2] = sample; // Left channel
                data[i * 2 + 1] = sample; // Right channel
            }
            
            AudioClip testClip = AudioClip.Create("LoudBeep", samples, 2, sampleRate, false);
            testClip.SetData(data, 0);
            
            // Configure for MAXIMUM volume
            source.clip = testClip;
            source.volume = 1.0f;
            source.spatialBlend = 0f; // 2D
            source.priority = 0; // Highest
            source.mute = false;
            source.bypassEffects = true;
            source.bypassListenerEffects = true;
            source.bypassReverbZones = true;
            source.outputAudioMixerGroup = null;
            
            source.Play();
            
            Debug.Log($"🔊 Playing LOUD beep test at volume {source.volume} (should be very audible!)");
            Debug.Log($"   If you can't hear this, check:");
            Debug.Log($"   1. System volume");
            Debug.Log($"   2. Headphones/speakers are connected");
            Debug.Log($"   3. Unity Audio settings (Edit > Project Settings > Audio)");
            Debug.Log($"   4. AudioSource isPlaying: {source.isPlaying}");
        }
    }
}

