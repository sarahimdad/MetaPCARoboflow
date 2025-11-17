using UnityEngine;
using TutorialSystem;

public class UserStepManager : MonoBehaviour
{
    [SerializeField]
    TutorialDisplay tutorialDisplay;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Period))
        {
            Debug.Log("Next");
            tutorialDisplay.NextStep();
        }

        if (Input.GetKeyDown(KeyCode.Comma))
        {
            Debug.Log("Previous");
            tutorialDisplay.PreviousStep();
        }

        // Replay current step audio (TTS)
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Replay TTS");
            tutorialDisplay.SpeakCurrentStep();
        }

        // Stop speaking
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Stop TTS");
            tutorialDisplay.StopSpeaking();
        }
    }
}
