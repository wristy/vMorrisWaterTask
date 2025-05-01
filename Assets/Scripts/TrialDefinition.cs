using UnityEngine;
using System.Collections.Generic;

public enum StartingLocationOption
{
    Randomize,
    Auto,   // Automatically choose a location with restrictions
    Circle  // Choose a location on a circle
}

public enum CuePlacementOption
{
    Auto,         // evenly spaced, or from code
    CircleManual // user picks positions on circle
}

public enum ChestPlacementOption
{
    Randomize,
    CircleManual
}


[System.Serializable]
public class TrialDefinition
{
    public float circleRadius = 20f;
    public int numberOfProximalCues = 4;
    public GameSettings.TrialType trialType = GameSettings.TrialType.Visible;
    public int timeLimit = -999;

    // --- Starting location ---
    public StartingLocationOption startingLocationOption = StartingLocationOption.Randomize;
    public Vector3 customStartingPosition;

    // --- Proximal cues ---
    public CuePlacementOption cuePlacementOption = CuePlacementOption.Auto;
    public List<Vector3> customCuePositions = new List<Vector3>();

    // --- Treasure chest ---
    public ChestPlacementOption chestPlacementOption = ChestPlacementOption.Randomize;
    public Vector3 customTreasureChestPosition;
}
