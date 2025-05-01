using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExperimentSetupController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField participantIDInput;
    public TMP_InputField numTrialsInput;

    public Button nextButton;

    // Name of the next scene (the multi‐page trial editor scene)
    public string trialEditorSceneName = "TrialEditorScene";

    void Start()
    {
        nextButton.onClick.AddListener(OnClickNext);
    }


    public void OnClickNext()
    {
        // Read participant ID
        if (int.TryParse(participantIDInput.text, out int pid))
        {
            GameSettings.participantID = pid;
        }

        // Read number of trials
        if (int.TryParse(numTrialsInput.text, out int nTrials))
        {
            GameSettings.numberOfTrials = nTrials;
        }
        else
        {
            GameSettings.numberOfTrials = 1;
        }

        // Allocate array for storing each trial’s settings
        // so the next scene can fill it in.
        GameSettings.allTrials = new TrialDefinition[GameSettings.numberOfTrials];
        for (int i = 0; i < GameSettings.numberOfTrials; i++)
        {
            // Optionally initialize defaults
            GameSettings.allTrials[i] = new TrialDefinition();
        }

        // Now load the multi‐page trial editor
        SceneManager.LoadScene(trialEditorSceneName);
    }
}
