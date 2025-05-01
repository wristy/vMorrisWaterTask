using System.Collections.Generic;
using UnityEngine;

public class CueManager : MonoBehaviour
{
    [Header("Cue References")]
    [Tooltip("Assign all four distinct cue GameObjects here.")]
    public GameObject[] allCues; // Array to hold the four cues
    private float radius;

    void Start()
    {
        // Initialize cues based on the initial number of proximal cues
        UpdateCues(GameSettings.numberOfProximalCues);
        radius = GameSettings.circleRadius + 3;
    }

    /// <summary>
    /// Updates the active cues based on the specified number, positioning them evenly.
    /// </summary>
    /// <param name="numberOfCues">Number of cues to activate and position.</param>
    public void UpdateCues(int numberOfCues)
    {
        numberOfCues = Mathf.Clamp(numberOfCues, 0, allCues.Length);

        if (numberOfCues == 0)
        {
            foreach (var cue in allCues)
                cue.SetActive(false);
            return;
        }

        // Reference to current trial cue positions
        TrialDefinition td = GameSettings.allTrials[GameManager.CurrentTrialIndex]; // you'll need to add this static reference or pass the index in
        List<Vector3> customPositions = td.customCuePositions;

        for (int i = 0; i < allCues.Length; i++)
        {
            if (i < numberOfCues)
            {
                allCues[i].SetActive(true);

                Vector3 cuePosition;

                // Use custom cue position if available
                if (customPositions != null && i < customPositions.Count)
                {
                    cuePosition = customPositions[i];
                }
                else
                {
                    // fallback to evenly spaced
                    float angleIncrement = 360f / numberOfCues;
                    float angle = i * angleIncrement;
                    float rad = angle * Mathf.Deg2Rad;
                    cuePosition = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * (GameSettings.circleRadius + 3f);
                }

                allCues[i].transform.position = cuePosition;

                Vector3 directionToCenter = (-cuePosition).normalized;
                allCues[i].transform.rotation = Quaternion.LookRotation(directionToCenter);
            }
            else
            {
                allCues[i].SetActive(false);
            }
        }
    }

}
