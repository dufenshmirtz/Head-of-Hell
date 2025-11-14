using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VictoryScreenNavigation : MonoBehaviour
{
    public Button PlayAgainButton;
    public Button BackToMenuButton;
    public Button SaveReplayButton;
    private bool isVictoryScreenActive = false;

    // Initialize the script as disabled (via Inspector checkbox)
    void Start()
    {
        // No need for 'enabled = false' here if you uncheck the script in Inspector
    }

    // Called when the script is enabled (manually or via code)
    void OnEnable()
    {
        isVictoryScreenActive = true;
        SetupButtonNavigation();
    }

    // Called when the script is disabled
    void OnDisable()
    {
        isVictoryScreenActive = false;
    }

    // Set up button navigation (called in OnEnable)
    private void SetupButtonNavigation()
    {
        // Set default selected button
        EventSystem.current.SetSelectedGameObject(PlayAgainButton.gameObject);

        // Configure explicit navigation
        Navigation navPlayAgain = PlayAgainButton.navigation;
        navPlayAgain.mode = Navigation.Mode.Explicit;
        navPlayAgain.selectOnRight = BackToMenuButton;
        navPlayAgain.selectOnDown = BackToMenuButton;
        PlayAgainButton.navigation = navPlayAgain;

        Navigation navBackToMenu = BackToMenuButton.navigation;
        navBackToMenu.mode = Navigation.Mode.Explicit;
        navBackToMenu.selectOnLeft = PlayAgainButton;
        navBackToMenu.selectOnUp = PlayAgainButton;
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