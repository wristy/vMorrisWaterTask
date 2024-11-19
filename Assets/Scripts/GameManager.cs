using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // References to other managers
    [Header("Managers")]
    public TreasureChestManager treasureChestManager;
    public DataCollector dataCollector;
    public PlayerController playerController;

    // Trial management
    private int currentTrial = 1;
    private bool trialInProgress = false;

    void Start()
    {
        if (treasureChestManager == null || dataCollector == null || playerController == null)
        {
            Debug.LogError("One or more manager references are missing in the GameManager.");
            return;
        }

        // Subscribe to the chest found event
        treasureChestManager.OnChestFound += OnChestFound;

        // Start the first trial
        StartNextTrial();
    }

    /// <summary>
    /// Initiates the next trial.
    /// </summary>
    void StartNextTrial()
    {
        if (currentTrial > GameSettings.numberOfTrials)
        {
            Debug.Log("All trials completed!");
            // Optional: Trigger end-of-experiment procedures
            return;
        }

        trialInProgress = true;
        Debug.Log($"Starting Trial {currentTrial}");

        // Reset DataCollector for the new trial
        dataCollector.StartNewTrial(currentTrial);

        if (currentTrial > 1)
        {
            // Reset Player Position if needed
            ResetPlayerPosition();

            // Reset the chest
            treasureChestManager.ResetChest();
        }

        // Unfreeze the player to allow movement
        playerController.UnfreezePlayer();

        // Optional: Update UI
        // uiManager.UpdateTrialDisplay(currentTrial);
    }
    void OnChestFound()
    {
        if (!trialInProgress)
            return;

        trialInProgress = false;
        Debug.Log($"Trial {currentTrial} completed!");

        // Freeze the player
        playerController.FreezePlayer();

        // Export data for the completed trial
        dataCollector.ExportData();
        dataCollector.ExportDistanceData();

        // Increment the trial counter
        currentTrial++;

        // Delay before starting the next trial
        StartCoroutine(StartNextTrialWithDelay(3f)); // 3-second delay
    }

    void ResetPlayerPosition()
    {
        // Define the start position (e.g., origin or a specific point)
        Vector3 startPosition = Vector3.zero; // Modify as needed
        Quaternion startRotation = Quaternion.identity; // Modify as needed

        playerController.transform.position = startPosition;
        playerController.transform.rotation = startRotation;

        // Reset player's CharacterController if necessary
        CharacterController controller = playerController.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            controller.enabled = true;
        }

        Debug.Log("Player position reset.");
    }
    IEnumerator StartNextTrialWithDelay(float delay)
    {
        // Optional: Show inter-trial interval UI
        // uiManager.ShowInterTrialInterval(currentTrial);

        yield return new WaitForSeconds(delay);

        StartNextTrial();
    }

    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        treasureChestManager.OnChestFound -= OnChestFound;
    }
}
