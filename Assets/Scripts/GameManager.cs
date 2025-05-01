using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    // References to other managers
    [Header("Managers")]
    public TreasureChestManager treasureChestManager;
    public DataCollector dataCollector;
    public PlayerController playerController;
    public CueManager cueManager;
    public DistalCueManager distalCueManager;

    [Header("UI Elements")]
    public GameObject timeUpPanel;
    public TextMeshProUGUI instructionText;


    // Trial management
    private int currentTrial = 1;
    private bool trialInProgress = false;

    private float trialTimer = 0f;
    private TrialDefinition currentTrialDefinition;

    public GameObject trialEndPlatformPrefab; // A small platform prefab with a collider.
    public Light trialEndLight;               // A light that indicates success (initially disabled).
    public float trialEndDuration = 8f;       // Duration to stay in the trial-end sequence.
    public float elevationOffset = 1f;      // How much to raise the player (in Unity units).
    public float platformSafeRadius = 10.0f;   // The safe radius on the platform where the player can roam.
    public static int CurrentTrialIndex => instance.currentTrial - 1;
    private static GameManager instance;

    void Awake()
    {
        instance = this;
    }


    void Start()
    {
        if (treasureChestManager == null || dataCollector == null || playerController == null || cueManager == null)
        {
            Debug.LogError("One or more manager references are missing in the GameManager.");
            return;
        }

        if (timeUpPanel != null)
        {
            timeUpPanel.SetActive(false);
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

        // Retrieve the current trial definition from the game settings
        currentTrialDefinition = GameSettings.allTrials[currentTrial - 1];

        GameSettings.circleRadius = currentTrialDefinition.circleRadius;
        GameSettings.numberOfProximalCues = currentTrialDefinition.numberOfProximalCues;
        GameSettings.trialType = currentTrialDefinition.trialType;
        GameSettings.timeLimit = currentTrialDefinition.timeLimit;

        if (distalCueManager != null)
        {
            distalCueManager.SetDistalCuesActive(GameSettings.enableDistalCues);
        }



        // Reset DataCollector for the new trial
        dataCollector.StartNewTrial(currentTrial);


        // Reset Player Position if needed
        ResetPlayerPosition();

        // Reset the chest
        treasureChestManager.ResetChest();


        trialTimer = 0f;

        // Unfreeze the player to allow movement
        playerController.UnfreezePlayer();

        cueManager.UpdateCues(currentTrialDefinition.numberOfProximalCues);

        // Optional: Update UI
        // uiManager.UpdateTrialDisplay(currentTrial);
    }

    void Update()
    {
        if (trialInProgress)
        {
            // Increment trial timer
            trialTimer += Time.deltaTime;

            // Check if time limit is reached
            // Check time limit using the current trial's timeLimit setting (if set to a positive value)
            if (currentTrialDefinition.timeLimit > 0 && trialTimer >= currentTrialDefinition.timeLimit)
            {
                OnTrialTimeout();
            }
        }
    }
    void OnChestFound()
    {
        if (!trialInProgress)
            return;

        trialInProgress = false;
        Debug.Log($"Trial {currentTrial} completed!");

        // Freeze the player

        StartCoroutine(TrialEndSequence());
    }

    IEnumerator TrialEndSequence()
    {
        // Get the player's current position.
        Vector3 playerPosition = playerController.transform.position;

        // Calculate the platform's spawn position.
        Vector3 platformPosition = new Vector3(playerPosition.x, playerPosition.y - elevationOffset, playerPosition.z);

        // Instantiate the platform prefab.
        GameObject platformInstance = Instantiate(trialEndPlatformPrefab, platformPosition, Quaternion.identity);

        // Raise the player by the elevation offset.
        playerController.transform.position = new Vector3(playerPosition.x, playerPosition.y + elevationOffset, playerPosition.z);

        // Record the fixed position so the player remains stationary.
        Vector3 fixedPosition = playerController.transform.position;

        // Activate the trial-end light and position it above the player.
        trialEndLight.transform.position = new Vector3(playerPosition.x, trialEndLight.transform.position.y, playerPosition.z);
        trialEndLight.gameObject.SetActive(true);

        // Display the instruction text at the same time as the platform.
        if (instructionText != null)
        {
            instructionText.text = "Good job! Moving on to the next trial...";
            instructionText.gameObject.SetActive(true);
        }

        // Wait for 6 seconds (trial end duration) while keeping the player fixed.
        float elapsedTime = 0f;
        while (elapsedTime < 6f)
        {
            // Freeze the player's position but allow them to look around.
            playerController.transform.position = fixedPosition;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Hide the instruction text.
        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(false);
        }

        // Turn off the trial-end light.
        trialEndLight.gameObject.SetActive(false);

        // Remove the platform.
        Destroy(platformInstance);

        // Export trial data.
        dataCollector.ExportData();
        dataCollector.ExportDistanceData();

        // Proceed to the next trial.
        currentTrial++;
        StartCoroutine(StartNextTrialWithDelay(0.001f));
    }



    /// <summary>
    /// Called when the trial timer runs out (timeout).
    /// </summary>
    void OnTrialTimeout()
    {
        Debug.Log($"Trial {currentTrial} ended due to timeout.");
        trialInProgress = false;

        // Freeze the player
        playerController.FreezePlayer();

        // Display "time is up" message (if you have assigned a panel or text)
        if (timeUpPanel != null)
        {
            timeUpPanel.SetActive(true);
        }

        // Export data just like the trial ended
        dataCollector.ExportData();
        dataCollector.ExportDistanceData();

        // Move on to the next trial after a delay
        currentTrial++;
        StartCoroutine(StartNextTrialWithDelay(3f)); // Adjust as desired
    }



    void ResetPlayerPosition()
    {
        // Determine starting position based on the trial definition’s starting location option
        Vector3 newStartPosition = GetStartingPosition(currentTrialDefinition);
        playerController.transform.position = newStartPosition;

        // Set rotation based on starting location option
        if (currentTrialDefinition.startingLocationOption == StartingLocationOption.Auto)
        {
            // Use the chest’s position as the reference
            Vector3 chestPos = treasureChestManager.ChestPosition;
            Vector3 directionToChest = (chestPos - newStartPosition).normalized;

            // Offset the angle so the player does not face the chest directly.
            float offsetAngle = 90f; // degrees
            float sign = Random.value > 0.5f ? 1f : -1f;
            Vector3 rotatedDirection = Quaternion.Euler(0, offsetAngle * sign, 0) * new Vector3(directionToChest.x, 0, directionToChest.z);
            playerController.transform.rotation = Quaternion.LookRotation(rotatedDirection);
        }
        else
        {
            // For Randomize and Circle options, use a default orientation.
            playerController.transform.rotation = Quaternion.identity;
        }

        // Reset player's CharacterController if necessary
        CharacterController controller = playerController.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            controller.enabled = true;
        }

        Debug.Log("Player position reset using starting location option: " + currentTrialDefinition.startingLocationOption);
    }

    // Helper method to determine the starting position based on trial settings.
    Vector3 GetStartingPosition(TrialDefinition trialDef)
    {
        switch (trialDef.startingLocationOption)
        {
            case StartingLocationOption.Randomize:
                {
                    // Choose any random point within a circle of radius equal to circleRadius.
                    Vector2 randomPoint = Random.insideUnitCircle * trialDef.circleRadius;
                    return new Vector3(randomPoint.x, 1, randomPoint.y); // Offset of to avoid being underground
                }
            case StartingLocationOption.Auto:
                {
                    // For the auto option, avoid the quadrant containing the chest.
                    Vector3 chestPos = treasureChestManager.ChestPosition;
                    if (chestPos == Vector3.zero)
                    {
                        // Fallback: choose a random angle if the chest position is not available.
                        Debug.LogWarning("Chest position not available. Choosing random starting position.");
                        float randomAngle = Random.Range(0, 360f);
                        float rad = randomAngle * Mathf.Deg2Rad;
                        return new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * trialDef.circleRadius;
                    }
                    else
                    {
                        // Determine the quadrant of the chest (using X and Z coordinates)
                        int chestQuadrant;
                        if (chestPos.x >= 0 && chestPos.z >= 0)
                            chestQuadrant = 1;
                        else if (chestPos.x < 0 && chestPos.z >= 0)
                            chestQuadrant = 2;
                        else if (chestPos.x < 0 && chestPos.z < 0)
                            chestQuadrant = 3;
                        else
                            chestQuadrant = 4;

                        // Allowed quadrants are all except the one that contains the chest.
                        List<int> allowedQuadrants = new List<int> { 1, 2, 3, 4 };
                        allowedQuadrants.Remove(chestQuadrant);

                        // Randomly choose one of the allowed quadrants.
                        int chosenQuadrant = allowedQuadrants[Random.Range(0, allowedQuadrants.Count)];

                        // Define quadrant angle ranges:
                        float angleMin = 0f, angleMax = 0f;
                        switch (chosenQuadrant)
                        {
                            case 1: angleMin = 0f; angleMax = 90f; break;
                            case 2: angleMin = 90f; angleMax = 180f; break;
                            case 3: angleMin = 180f; angleMax = 270f; break;
                            case 4: angleMin = 270f; angleMax = 360f; break;
                        }
                        float chosenAngle = Random.Range(angleMin, angleMax);
                        float rad = chosenAngle * Mathf.Deg2Rad;
                        Vector3 spawnPosition = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * (trialDef.circleRadius);
                        spawnPosition.y = 1f; // Offset to avoid being underground
                        spawnPosition.x = Mathf.Clamp(spawnPosition.x, -trialDef.circleRadius * 0.7f, trialDef.circleRadius * 0.7f);
                        spawnPosition.z = Mathf.Clamp(spawnPosition.z, -trialDef.circleRadius * 0.7f, trialDef.circleRadius * 0.7f);
                        return spawnPosition;
                    }
                }
            case StartingLocationOption.Circle:
                {
                    // For the circle option, use a custom starting position if provided;
                    // otherwise, default to a point on the circle (e.g., at (circleRadius, 0, 0)).
                    if (trialDef.customStartingPosition != Vector3.zero)
                        return trialDef.customStartingPosition;
                    else
                        return new Vector3(trialDef.circleRadius, 0, 0);
                }
            default:
                return Vector3.zero;
        }
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
