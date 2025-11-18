using TutorialSystem;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject m_menuUserSteps;

    [SerializeField]
    private GameObject m_menuMain;

    [SerializeField]
    private TutorialDisplay tutorialDisplay;

    [SerializeField]
    private TutorialData tutorialDataBatteryRecharge;

    [SerializeField]
    private TutorialData tutorialDataBatteryReplace;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_menuUserSteps.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("StartBatteryRechargeUserSteps");
            StartUserStepsBatteryRecharge();
        }
        */
    }

    public void StartUserStepsBatteryRecharge()
    {
        StartUserSteps(tutorialDataBatteryRecharge);
    }

    public void StartUserStepsBatteryReplace()
    {
        StartUserSteps(tutorialDataBatteryReplace);
    }

    public void StartUserSteps(TutorialData tutorialData)
    {
        m_menuMain.SetActive(false);
        m_menuUserSteps.SetActive(true);
        tutorialDisplay.tutorialData = tutorialData;
        tutorialDisplay.FirstStep();
    }

    /// <summary>
    /// Return to the main menu from tutorial steps.
    /// Closes the tutorial screen and opens the main menu with tutorial selection.
    /// </summary>
    public void ReturnToMainMenu()
    {
        // Stop any TTS audio that might be playing
        if (tutorialDisplay != null)
        {
            tutorialDisplay.StopSpeaking();
        }
        
        // Close tutorial screen and open main menu
        m_menuUserSteps.SetActive(false);
        m_menuMain.SetActive(true);
    }
}
