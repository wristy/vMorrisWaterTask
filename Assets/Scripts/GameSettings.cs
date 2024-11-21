public static class GameSettings
{
    public static float circleRadius = 20f; // Default value
    public static int numberOfTrials = 5; // Default value
    public static int numberOfProximalCues = 4; // Default value
    public static int participantID = 1234; // Default value

    public enum TrialType
    {
        Visible,
        Invisible,
        Absent
    }

    public static TrialType trialType = TrialType.Visible; // Default value
    public static int timeLimit = 10; // Default value
}
