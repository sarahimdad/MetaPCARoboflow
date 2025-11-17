using UnityEngine;

namespace OpenAI
{
    /// <summary>
    /// Diagnostic tool to check OpenAI setup
    /// Add this component to any GameObject and it will check the setup
    /// </summary>
    public class OpenAIDiagnostics : MonoBehaviour
    {
        [ContextMenu("Check Setup")]
        public void CheckSetup()
        {
            Debug.Log("=== OpenAI Setup Diagnostics ===");
            
            // Check if manager exists
            if (OpenAIManager.Instance == null)
            {
                Debug.LogError("❌ OpenAIManager.Instance is NULL");
                Debug.LogError("   → Make sure you have a GameObject with OpenAIManager component in the scene");
            }
            else
            {
                Debug.Log("✅ OpenAIManager.Instance exists");
                
                // Check if initialized
                if (OpenAIManager.Instance.IsInitialized)
                {
                    Debug.Log("✅ OpenAIManager is initialized");
                }
                else
                {
                    Debug.LogError("❌ OpenAIManager is NOT initialized");
                    
                    // Try to get the manager component to check config
                    var manager = OpenAIManager.Instance;
                    if (manager.config == null)
                    {
                        Debug.LogError("   → Config is not assigned!");
                    }
                    else
                    {
                        Debug.Log($"   → Config assigned: {manager.config.name}");
                        
                        if (!manager.config.IsConfigured())
                        {
                            Debug.LogError("   → API key is not configured in the config asset!");
                        }
                        else
                        {
                            Debug.Log("   → API key is configured");
                            Debug.LogWarning("   → But initialization still failed. Check for exceptions in Console.");
                        }
                    }
                }
            }
            
            Debug.Log("=== End Diagnostics ===");
        }

        private void Start()
        {
            // Auto-check on start
            CheckSetup();
        }
    }
}

