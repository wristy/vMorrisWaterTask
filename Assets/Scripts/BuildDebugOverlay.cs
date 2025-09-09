// using UnityEngine;

// public class BuildDebugOverlay : MonoBehaviour
// {
//     private float logUpdateInterval = 1f;
//     private float nextUpdateTime = 0f;

//     private string debugText = "";

//     void Update()
//     {
//         if (Time.time >= nextUpdateTime)
//         {
//             UpdateDebugInfo();
//             nextUpdateTime = Time.time + logUpdateInterval;
//         }
//     }

//     void UpdateDebugInfo()
//     {
//         debugText = $"[DEBUG INFO]\n" +
//                     $"Time: {Time.time:F1}s\n" +
//                     $"allTrials == null: {GameSettings.allTrials == null}\n" +
//                     $"Trial count: {(GameSettings.allTrials != null ? GameSettings.allTrials.Length.ToString() : "N/A")}\n" +
//                     $"CurrentTrialIndex: {GameManager.CurrentTrialIndex}\n" +
//                     $"participantID: {GameSettings.participantID}\n" +
//                     $"Player position: {GetPlayerPosition()}\n" +
//                     $"TrialType: {GameSettings.trialType}\n";

//         Camera cam = Camera.main;
//         if (cam != null)
//         {
//             debugText += $"MainCamera pos: {cam.transform.position:F2}\n";
//             debugText += $"MainCamera parent: {(cam.transform.parent != null ? cam.transform.parent.name : "none")}\n";
//         }
//         else
//         {
//             debugText += "MainCamera not found!\n";
//         }


//     }

//     string GetPlayerPosition()
//     {
//         GameObject player = GameObject.FindWithTag("Player");
//         if (player != null)
//         {
//             return player.transform.position.ToString("F2");
//         }
//         return "Not Found";
//     }

//     void OnGUI()
//     {
//         GUI.color = Color.green;
//         GUI.Label(new Rect(10, 10, 500, 200), debugText);
//     }
// }
