using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
    {
    public TMP_InputField radiusInputField;
    public TMP_InputField numberOfTrialsInputField;
    public TMP_InputField numberOfProximalCuesInputField;
    public TMP_InputField participantIDInputField;
    public TMP_Dropdown trialTypeDropdown;
    public TMP_InputField timeLimitInputField;
    public string simulationSceneName = "TrialScene";
    

    public void Start()
    {
        numberOfProximalCuesInputField.text = GameSettings.numberOfProximalCues.ToString();
    }

    public void StartSimulation()
    {
        string radiusText = radiusInputField.text;
        if (float.TryParse(radiusText, out float radius))
        {
            GameSettings.circleRadius = radius;
        }
        else
        {
            GameSettings.circleRadius = 20f;
        }

        string numberOfTrialsText = numberOfTrialsInputField.text;
        if (int.TryParse(numberOfTrialsText, out int numberOfTrials))
        {
            GameSettings.numberOfTrials = numberOfTrials;
        }
        else
        {
            GameSettings.numberOfTrials = 5;
        }

        string numberOfProximalCuesText = numberOfProximalCuesInputField.text;
        if (int.TryParse(numberOfProximalCuesText, out int numberOfProximalCues))
        {
            GameSettings.numberOfProximalCues = numberOfProximalCues;
        }

        string participantIDText = participantIDInputField.text;
        if (int.TryParse(participantIDText, out int participantID))
        {
            GameSettings.participantID = participantID;
        }

        // Parse the trial type selection
        if (trialTypeDropdown != null)
        {
            GameSettings.trialType = (GameSettings.TrialType)trialTypeDropdown.value;
            Debug.Log($"Trial type set to: {GameSettings.trialType}");
        }
        else
        {
            GameSettings.trialType = GameSettings.TrialType.Visible; // Default value
            Debug.Log($"Trial type set to: {GameSettings.trialType}");
        }
        
        string timeLimitText = timeLimitInputField.text;
        if (int.TryParse(timeLimitText, out int timeLimit))
        {
            GameSettings.timeLimit = timeLimit;
        }

        // Load the simulation scene
        SceneManager.LoadScene(simulationSceneName);
    }
}
