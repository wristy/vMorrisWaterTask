// using UnityEngine;
// using System.Collections.Generic;

// public class BuildConsoleOverlay : MonoBehaviour
// {
//     private static readonly List<string> logMessages = new List<string>();
//     private Vector2 scrollPosition = Vector2.zero;

//     // Settings
//     public int maxLines = 100;
//     public int fontSize = 14;
//     public Color logColor = Color.white;
//     public Color warningColor = Color.yellow;
//     public Color errorColor = Color.red;

//     void OnEnable()
//     {
//         Application.logMessageReceived += HandleLog;
//     }

//     void OnDisable()
//     {
//         Application.logMessageReceived -= HandleLog;
//     }

//     void HandleLog(string logString, string stackTrace, LogType type)
//     {
//         string prefix = $"[{type}] ";
//         string colorHex = type switch
//         {
//             LogType.Warning => ColorUtility.ToHtmlStringRGB(warningColor),
//             LogType.Error or LogType.Exception => ColorUtility.ToHtmlStringRGB(errorColor),
//             _ => ColorUtility.ToHtmlStringRGB(logColor)
//         };

//         string formatted = $"<color=#{colorHex}>{prefix}{logString}</color>";
//         logMessages.Add(formatted);

//         if (logMessages.Count > maxLines)
//             logMessages.RemoveAt(0);
//     }

//     void OnGUI()
//     {
//         GUI.skin.label.fontSize = fontSize;
//         GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height / 2));
//         scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width - 20), GUILayout.Height(Screen.height / 2));
//         foreach (string message in logMessages)
//         {
//             GUILayout.Label(message);
//         }
//         GUILayout.EndScrollView();
//         GUILayout.EndArea();
//     }
// }
