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
        // Clamp the number of cues between 0 and the total available cues
        numberOfCues = Mathf.Clamp(numberOfCues, 0, allCues.Length);

        if (numberOfCues == 0)
        {
            // Disable all cues if the number is zero
            foreach (var cue in allCues)
            {
                cue.SetActive(false);
            }
            return;
        }

        // Calculate the angle increment for even spacing
        float angleIncrement = 360f / numberOfCues;

        for (int i = 0; i < allCues.Length; i++)
        {
            if (i < numberOfCues)
            {
                // Activate the cue
                allCues[i].SetActive(true);

                // Calculate the angle for this cue
                float angle = i * angleIncrement;

                // Convert angle to radians for position calculation
                float rad = angle * Mathf.Deg2Rad;

                // Determine the position based on angle and radius
                float x = Mathf.Cos(rad) * radius;
                float z = Mathf.Sin(rad) * radius;

                Vector3 cuePosition = new Vector3(x, 0, z);

                // Set the cue's position
                allCues[i].transform.position = cuePosition;

                // Optionally, rotate the cue to face the center
                Vector3 directionToCenter = (- cuePosition).normalized;
                allCues[i].transform.rotation = Quaternion.LookRotation(directionToCenter);
            }
            else
            {
                // Deactivate the cue if it's not needed
                allCues[i].SetActive(false);
            }
        }
    }
}
