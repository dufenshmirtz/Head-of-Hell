using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterChoiceScript : MonoBehaviour
{
    public Button[] buttons;  // Assign your buttons in the Inspector
    private int selectedIndex = 0;
    public MainMenuMusic sfx;
    bool notSelected;

    private int rows = 3;  // Number of rows in your button grid
    private int cols = 4;  // Number of columns in your button grid

    private bool clickProcessed = false;

    public CharacterChoiceMenu characterChoiceMenu;  // Reference to the CharacterChoiceMenu script

    public bool bothpicked = false;

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
            NavigateVertical(1);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (notSelected)
            {
                AutoSelectButton();
                return;
            }
            NavigateVertical(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (notSelected)
            {
                AutoSelectButton();
                return;
            }
            NavigateHorizontal(1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (notSelected)
            {
                AutoSelectButton();
                return;
            }
            NavigateHorizontal(-1);
        }

        
    }

    void NavigateVertical(int direction)
    {
        sfx.ButtonSound();
        buttons[selectedIndex].OnDeselect(null);

        if (bothpicked)
        {
            // When both players picked, only navigate between "Back" (index 6) and "Start" (index 7)
            if (direction > 0)
            {
                selectedIndex = selectedIndex == 9 ? 10 : 9;
            }
            else
            {
                selectedIndex = selectedIndex == 10 ? 9 : 10;
            }
        }
        else
        {
            int newIndex = selectedIndex + direction * cols;
            if (newIndex >= buttons.Length) newIndex %= buttons.Length;
            if (newIndex < 0) newIndex += buttons.Length;

            selectedIndex = newIndex;

            if (selectedIndex > 9)
            {
                selectedIndex = 0;
            }
        }

        

        EventSystem.current.SetSelectedGameObject(buttons[selectedIndex].gameObject);
        buttons[selectedIndex].Select();

        if (!bothpicked && selectedIndex != 9 && selectedIndex != 10)
        {
            characterChoiceMenu.HoveringIn(buttons[selectedIndex].name);
        }

    }

    void NavigateHorizontal(int direction)
    {
        sfx.ButtonSound();
        buttons[selectedIndex].OnDeselect(null);

        if (bothpicked)
        {
            // When both players picked, only navigate between "Back" (index 6) and "Start" (index 7)
            if (direction > 0)
            {
                selectedIndex = selectedIndex == 9 ? 10 : 9;
            }
            else
            {
                selectedIndex = selectedIndex == 10 ? 9 : 10;
            }
        }
        else
        {
            int newIndex = selectedIndex + direction;
            int currentRow = selectedIndex / cols;

            // Ensure new index is within the same row
            if (newIndex >= (currentRow + 1) * cols) newIndex = currentRow * cols;
            if (newIndex < currentRow * cols) newIndex = (currentRow + 1) * cols - 1;

            selectedIndex = newIndex;
            if (selectedIndex > 9)
            {
                selectedIndex = 0;
            }
        }
        
        EventSystem.current.SetSelectedGameObject(buttons[selectedIndex].gameObject);
        buttons[selectedIndex].Select();

        if (!bothpicked && selectedIndex != 9 && selectedIndex != 10)
        {
            characterChoiceMenu.HoveringIn(buttons[selectedIndex].name);
        }

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

            // Update character name display
            characterChoiceMenu.HoveringIn(buttons[0].name);
        }
    }

    public void BothPicked(bool didthey)
    {
        bothpicked = didthey;
    }
}
