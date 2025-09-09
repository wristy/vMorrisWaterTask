// ExperimentSettingsData.cs
using UnityEngine; // Required for TrialDefinition which uses Vector3

[System.Serializable]
public class ExperimentSettingsData
{
    public int participantID;
    public int numberOfTrials;
    public bool enableDistalCues; // Global setting for distal cues
    public TrialDefinition[] allTrials;

    // Default constructor for JsonUtility
    public ExperimentSettingsData() { }
}