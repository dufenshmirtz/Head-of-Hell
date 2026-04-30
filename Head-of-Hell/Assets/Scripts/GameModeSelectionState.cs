using UnityEngine;
public enum SelectedGameMode
{
    PvP_1v1,
    PvP_1v1v1,
    PvE
}

public static class GameModeSelectionState
{
    public static SelectedGameMode CurrentMode { get; private set; } = SelectedGameMode.PvP_1v1;

    public static int PlayerCount => CurrentMode == SelectedGameMode.PvP_1v1v1 ? 3 : 2;

    public static bool RequiresThirdPlayerSelection => CurrentMode == SelectedGameMode.PvP_1v1v1;

    public static void SelectPvP1v1()
    {
        CurrentMode = SelectedGameMode.PvP_1v1;
        Debug.Log("[GameModeSelectionState] SelectPvP1v1 called");
    }

    public static void SelectPvP1v1v1()
    {
        CurrentMode = SelectedGameMode.PvP_1v1v1;
        Debug.Log("[GameModeSelectionState] SelectPvP1v1v1 called");
    }

    public static void SelectPvE()
    {
        CurrentMode = SelectedGameMode.PvE;
        Debug.Log("[GameModeSelectionState] SelectPvE called");
    }
}