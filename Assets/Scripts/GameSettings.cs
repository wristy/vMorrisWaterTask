public static class GameSettings
{
    // Participant ID or other global info
    public static int participantID = 0;
    public static int numberOfTrials = 1;

    // “Default” trial fields if you still want them 
    // for quick single‐trial usage:
    public static float circleRadius = 20f;
    public static int numberOfProximalCues = 4;
    public static bool enableDistalCues = true;
    public static TrialType trialType = TrialType.Visible;
    public static int timeLimit = 99999;

    public enum TrialType
    {
        Visible,
        Invisible,
        Absent
    }

    public static TrialDefinition[] allTrials;
}
