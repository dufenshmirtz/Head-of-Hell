public enum PvEBotType
{
    MLAgent,
    ScriptedBot
}

public enum PvEDifficulty
{
    Easy,
    Medium,
    Hard
}

public static class PvESelectionState
{
    public static bool IsPvE = false;
    public static PvEBotType SelectedBotType = PvEBotType.ScriptedBot;
    public static PvEDifficulty SelectedDifficulty = PvEDifficulty.Medium;

    public static void ResetToDefaults()
    {
        IsPvE = false;
        SelectedBotType = PvEBotType.ScriptedBot;
        SelectedDifficulty = PvEDifficulty.Medium;
    }
}