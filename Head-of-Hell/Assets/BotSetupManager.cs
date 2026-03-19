using UnityEngine;

public enum BotType
{
    MLAgent,
    Scripted
}

public enum Difficulty
{
    Easy,
    Medium,
    Hard
}

public class BotSetupManager : MonoBehaviour
{
    public static BotType SelectedBot = BotType.Scripted;
    public static Difficulty SelectedDifficulty = Difficulty.Medium;

    // BOT TYPE
    public void SelectMLAgent()
    {
        SelectedBot = BotType.MLAgent;
        Debug.Log("Selected ML Agent");
    }

    public void SelectScripted()
    {
        SelectedBot = BotType.Scripted;
        Debug.Log("Selected Scripted Bot");
    }

    // DIFFICULTY
    public void SelectEasy()
    {
        SelectedDifficulty = Difficulty.Easy;
    }

    public void SelectMedium()
    {
        SelectedDifficulty = Difficulty.Medium;
    }

    public void SelectHard()
    {
        SelectedDifficulty = Difficulty.Hard;
    }
}