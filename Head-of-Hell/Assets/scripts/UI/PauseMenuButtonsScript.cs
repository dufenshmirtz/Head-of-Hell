using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuNavigation : MonoBehaviour
{
    public Button[] buttons;  // Assign your buttons in the Inspector
    private int selectedIndex = 0;
    public AudioManager sfx;
    bool notSelected;

    void Start()
    {
        notSelected = true;
    }

    void Update()
    {
        // Navigate through the buttons with the arrow keys
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (notSelected)
            {
                AutoSelectButton();
                return;
            }
            Navigate(1);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (notSelected)
            {
                AutoSelectButton();
                return;
            }
            Navigate(-1);
        }

        // Press Enter to click the selected button
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ClickSelectedButton();
        }
    }

    void Navigate(int direction)
    {
        sfx.ButtonSound();
        // Deselect the current button
        buttons[selectedIndex].OnDeselect(null);

        // Update the selected index
        selectedIndex += direction;
        if (selectedIndex >= buttons.Length) selectedIndex = 0;
        if (selectedIndex < 0) selectedIndex = buttons.Length - 1;

        // Select the new button
        EventSystem.current.SetSelectedGameObject(buttons[selectedIndex].gameObject);
        buttons[selectedIndex].Select();
    }

    void ClickSelectedButton()
    {
        // Click the currently selected button
        var pointer = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(buttons[selectedIndex].gameObject, pointer, ExecuteEvents.pointerClickHandler);
    }

    void AutoSelectButton()
    {
        // Ensure the first button is selected by default
        if (buttons.Length > 0)
        {
            sfx.ButtonSound();
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
            buttons[0].Select();
            notSelected = false;
        }
    }
}
