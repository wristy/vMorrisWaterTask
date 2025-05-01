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

        // Load the simulation scene
        SceneManager.LoadScene(simulationSceneName);
    }
}
