using UnityEngine;

public class PvPModeSelectionMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject localOnlineMenu;
    [SerializeField] private GameObject pvpModeSelectionMenu;
    [SerializeField] private GameObject gameSetupMenu;

    public void OpenFromPvPButton()
    {
        PvESelectionState.SelectPvPMode();
        GameModeSelectionState.SelectPvP1v1();

        if (localOnlineMenu != null)
            localOnlineMenu.SetActive(false);

        if (pvpModeSelectionMenu != null)
            pvpModeSelectionMenu.SetActive(true);
    }

    public void Select1v1()
    {
        PvESelectionState.SelectPvPMode();
        GameModeSelectionState.SelectPvP1v1();
        OpenGameSetup();
    }

    public void Select1v1v1()
    {
        PvESelectionState.SelectPvPMode();
        GameModeSelectionState.SelectPvP1v1v1();
        OpenGameSetup();
    }

    public void Back()
    {
        GameModeSelectionState.SelectPvP1v1();

        if (pvpModeSelectionMenu != null)
            pvpModeSelectionMenu.SetActive(false);

        if (localOnlineMenu != null)
            localOnlineMenu.SetActive(true);
    }

    private void OpenGameSetup()
    {
        if (pvpModeSelectionMenu != null)
            pvpModeSelectionMenu.SetActive(false);

        if (gameSetupMenu != null)
            gameSetupMenu.SetActive(true);
    }
}
