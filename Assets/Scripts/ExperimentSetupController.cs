using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class ExperimentSetupController : MonoBehaviour
{
    [Header("Mode Toggles")]
    public Toggle newExperimentToggle;     // “Set up New Experiment”
    public Toggle runParticipantToggle;    // “Run a Participant”

    [Header("New Experiment UI (shown only when New is selected)")]
    public TMP_InputField numTrialsInput;  // Number of Trials

    [Header("Run Participant UI (shown only when Run is selected)")]
    public TMP_InputField participantIdInput;  // Participant ID (Run only)
    public TMP_Dropdown savedFilesDropdown;    // List *.json from persistentDataPath

    [Header("Buttons")]
    public Button nextButton;
    public TMP_Text nextButtonLabel;   // <- assign the label (child TMP_Text of the button)

    [Header("Scene Names")]
    public string trialEditorSceneName = "TrialEditorScene"; // Configure settings
    public string runSceneName = "TrialScene";               // Run sequence

    void Start()
    {
        // Default to New Experiment
        if (newExperimentToggle != null) newExperimentToggle.isOn = true;
        if (runParticipantToggle != null) runParticipantToggle.isOn = false;

        // Wire events
        if (newExperimentToggle != null) newExperimentToggle.onValueChanged.AddListener(_ => UpdatePanels());
        if (runParticipantToggle != null) runParticipantToggle.onValueChanged.AddListener(_ => UpdatePanels());
        if (nextButton != null) nextButton.onClick.AddListener(OnClickNext);

        PopulateSavedFilesDropdown();
        UpdatePanels();
    }

    void UpdatePanels()
    {
        bool isNew = newExperimentToggle == null || newExperimentToggle.isOn;

        // New Experiment section
        if (numTrialsInput != null)
            numTrialsInput.transform.parent.gameObject.SetActive(isNew);

        // Run Participant section
        bool showRun = !isNew;
        if (participantIdInput != null)
            participantIdInput.transform.parent.gameObject.SetActive(showRun);
        if (savedFilesDropdown != null)
            savedFilesDropdown.transform.parent.gameObject.SetActive(showRun);

        // Update button label
        if (nextButtonLabel != null)
            nextButtonLabel.text = isNew ? "Edit Trials..." : "Run...";
    }


    void PopulateSavedFilesDropdown()
    {
        if (savedFilesDropdown == null) return;

        savedFilesDropdown.ClearOptions();
        var files = Directory.GetFiles(Application.persistentDataPath, "*.json");
        var options = new List<string>();
        foreach (var f in files) options.Add(Path.GetFileName(f));
        if (options.Count == 0) options.Add("(no saved experiments found)");
        savedFilesDropdown.AddOptions(options);
        savedFilesDropdown.value = 0;
        savedFilesDropdown.RefreshShownValue();
    }

    public void OnClickNext()
    {
        bool isNew = newExperimentToggle == null || newExperimentToggle.isOn;

        if (isNew)
        {
            // === New Experiment → define #trials and go configure ===
            if (!int.TryParse(numTrialsInput != null ? numTrialsInput.text : "1", out int nTrials))
                nTrials = 1;

            GameSettings.numberOfTrials = Mathf.Max(1, nTrials);
            GameSettings.allTrials = new TrialDefinition[GameSettings.numberOfTrials];
            for (int i = 0; i < GameSettings.numberOfTrials; i++)
                GameSettings.allTrials[i] = new TrialDefinition();

            // Seed some defaults for the editor preview if you want
            if (GameSettings.allTrials.Length > 0)
            {
                var first = GameSettings.allTrials[0];
                GameSettings.circleRadius = first.circleRadius;
                GameSettings.numberOfProximalCues = first.numberOfProximalCues;
                GameSettings.trialType = first.trialType;
                GameSettings.timeLimit = first.timeLimit;
            }

            SceneManager.LoadScene(trialEditorSceneName);
        }
        else
        {
            // === Run Participant → set participant ID, load saved experiment, jump to run ===
            if (!int.TryParse(participantIdInput != null ? participantIdInput.text : "0", out int pid))
                pid = 0;
            GameSettings.participantID = pid;

            if (savedFilesDropdown == null || savedFilesDropdown.options.Count == 0)
            {
                Debug.LogWarning("No saved file selected.");
                return;
            }

            string chosen = savedFilesDropdown.options[savedFilesDropdown.value].text;
            if (string.IsNullOrEmpty(chosen) || chosen.StartsWith("("))
            {
                Debug.LogWarning("No valid saved experiment found.");
                return;
            }

            string path = Path.Combine(Application.persistentDataPath, chosen);
            if (!File.Exists(path))
            {
                Debug.LogWarning("Saved experiment file not found: " + path);
                return;
            }

            var data = JsonUtility.FromJson<ExperimentSettingsData>(File.ReadAllText(path));

            // Load trials/settings from file
            GameSettings.allTrials = data.allTrials ?? new TrialDefinition[0];
            GameSettings.numberOfTrials = GameSettings.allTrials.Length > 0 ? GameSettings.allTrials.Length : 1;
            GameSettings.enableDistalCues = data.enableDistalCues;

            // Seed global defaults from first trial (for runtime components that consult GameSettings)
            if (GameSettings.allTrials.Length > 0)
            {
                var first = GameSettings.allTrials[0];
                GameSettings.circleRadius = first.circleRadius;
                GameSettings.numberOfProximalCues = first.numberOfProximalCues;
                GameSettings.trialType = first.trialType;
                GameSettings.timeLimit = first.timeLimit;
            }

            SceneManager.LoadScene(runSceneName);
        }
    }
}
