using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VictoryScreenNavigation : MonoBehaviour
{
    public Button PlayAgainButton;
    public Button BackToMenuButton;

    void Start()
    {
        // Set the first selected button
        EventSystem.current.SetSelectedGameObject(PlayAgainButton.gameObject);

        // Explicitly set navigation
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
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null)
            {
                Button button = selected.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.Invoke();
                }
            }
        }
    }
}
