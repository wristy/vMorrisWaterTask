using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TreasureChestManager : MonoBehaviour
{
    // Chest parameters
    [Header("Chest Settings")]
    public GameObject treasureChestPrefab;         // Reference to the chest prefab
    public float chestSpawnRadius = 20.0f;        // Radius within which to spawn the chest
    public float chestDetectionDistance = 2.0f;   // Distance at which the chest is "found"

    // Player reference
    [Header("Player Settings")]
    public PlayerController playerController;      // Reference to the PlayerController script

    // Private variables
    private Vector3 chestPosition;                 // Position where the chest will appear
    private bool chestFound = false;               // Flag to check if the chest has been found

    public event Action OnChestFound;

    public GameObject chestMarkerPrefab; // Assign via Inspector
    private GameObject chestMarkerInstance;

    void Start()
    {
        // Spawn the chest position
        SpawnChestPosition();
    }

    void Update()
    {
        if (!chestFound)
        {
            CheckChestProximity();
        }
    }

    void SpawnChestPosition()
    {
        // Generate a random direction within a circle on the horizontal plane
        Vector2 randomDirection = UnityEngine.Random.insideUnitCircle * chestSpawnRadius;
        Vector3 spawnPosition = new Vector3(randomDirection.x, 0, randomDirection.y) + playerController.transform.position;

        // Raycast to find the ground height at the chest position
        RaycastHit hit;
        if (Physics.Raycast(spawnPosition + Vector3.up * 10, Vector3.down, out hit, 20))
        {
            chestPosition = hit.point;
        }
        else
        {
            // If no ground is found, default to player's Y position
            chestPosition = new Vector3(spawnPosition.x, playerController.transform.position.y, spawnPosition.z);
        }

        Debug.Log($"Chest spawned at: {chestPosition}");

        if (chestMarkerPrefab != null)
        {
            chestMarkerInstance = Instantiate(chestMarkerPrefab, chestPosition, Quaternion.identity);
        }

    }
    void CheckChestProximity()
    {
        float distance = Vector3.Distance(playerController.transform.position, chestPosition);
        if (distance <= chestDetectionDistance)
        {
            FindChest();
        }
    }

    void FindChest()
    {
        chestFound = true;

        // Notify subscribers that the chest has been found
        OnChestFound?.Invoke();

        // Spawn the chest, facing the player, but still parallel to the ground
        GameObject chest = Instantiate(treasureChestPrefab, chestPosition, Quaternion.identity);

        Debug.Log($"Chest found at: {chestPosition}");

        // Freeze the player
        playerController.FreezePlayer();

        // Optional: Provide feedback to the player
        Debug.Log("Chest found!");
    }

    public void ResetChest()
    {
        chestFound = false;

        // Destroy existing chest instances to prevent duplicates
        foreach (GameObject chest in GameObject.FindGameObjectsWithTag("TreasureChest"))
        {
            Destroy(chest);
        }

        // Re-instantiate the chest at the original position
        // Instantiate(treasureChestPrefab, chestPosition, Quaternion.identity);

        Debug.Log($"Chest reset at: {chestPosition}");

        // Optional: Provide feedback
        Debug.Log("Chest reset for the next trial.");
    }
}

