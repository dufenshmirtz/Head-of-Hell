using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VictoryScreenNavigation : MonoBehaviour
{
    public Button PlayAgainButton;
    public Button ChangeProfilesButton;
    public Button BackToMenuButton;
    public Button SaveReplayButton;

    private bool isVictoryScreenActive = false;

    void OnEnable()
    {
        isVictoryScreenActive = true;
        SetupButtonNavigation();
    }

    void OnDisable()
    {
        isVictoryScreenActive = false;
    }

    private void SetupButtonNavigation()
    {
        EventSystem.current.SetSelectedGameObject(PlayAgainButton.gameObject);

        Navigation navPlayAgain = PlayAgainButton.navigation;
        navPlayAgain.mode = Navigation.Mode.Explicit;
        navPlayAgain.selectOnRight = SaveReplayButton;
        navPlayAgain.selectOnDown = ChangeProfilesButton;
        PlayAgainButton.navigation = navPlayAgain;

        Navigation navSaveReplay = SaveReplayButton.navigation;
        navSaveReplay.mode = Navigation.Mode.Explicit;
        navSaveReplay.selectOnLeft = PlayAgainButton;
        navSaveReplay.selectOnDown = BackToMenuButton;
        SaveReplayButton.navigation = navSaveReplay;

        Navigation navChangeProfiles = ChangeProfilesButton.navigation;
        navChangeProfiles.mode = Navigation.Mode.Explicit;
        navChangeProfiles.selectOnUp = PlayAgainButton;
        navChangeProfiles.selectOnRight = BackToMenuButton;
        ChangeProfilesButton.navigation = navChangeProfiles;

        Navigation navBackToMenu = BackToMenuButton.navigation;
        navBackToMenu.mode = Navigation.Mode.Explicit;
        navBackToMenu.selectOnUp = SaveReplayButton;
        navBackToMenu.selectOnLeft = ChangeProfilesButton;
        BackToMenuButton.navigation = navBackToMenu;
    }

    void Update()
    {
        if (!isVictoryScreenActive) return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null && selected.TryGetComponent(out Button button))
            {
                button.onClick.Invoke();
            }
        }
    }
}