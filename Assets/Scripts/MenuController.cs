using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
    {
    public TMP_InputField radiusInputField;
    public TMP_InputField numberOfTrialsInputField;
    public string simulationSceneName = "TrialScene";

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

        // Load the simulation scene
        SceneManager.LoadScene(simulationSceneName);
    }
}
