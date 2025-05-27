using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StageChoiceManager : MonoBehaviour
{
    public Button[] buttons;
    private int selectedIndex = 0;
    public MainMenuMusic sfx;
    bool notSelected;

    private int rows = 2;
    private int cols = 3;

    private bool clickProcessed = false;
    public StageChoiceButtons stageChoiceButtons;

    public bool picked = false;

    public Button proceedButton;
    private bool stageSelected = false;
    private bool gameModeSelected = false;

    void Start()
    {
        notSelected = true;
        proceedButton.interactable = false;
    }

    void Update()
    {
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

        if (picked)
        {
            // Navigate between Back (index 4), Random (5), and Proceed (6)
            if (selectedIndex < 4) selectedIndex = 4;

            selectedIndex += direction;

            if (selectedIndex > 6) selectedIndex = 4;
            if (selectedIndex < 4) selectedIndex = 6;
        }
        else
        {
            int newIndex = selectedIndex + direction * cols;
            if (newIndex >= buttons.Length) newIndex %= buttons.Length;
            if (newIndex < 0) newIndex += buttons.Length;
            if (newIndex > 3) newIndex = 0; // keep only on stage buttons before selection

            selectedIndex = newIndex;
        }

        EventSystem.current.SetSelectedGameObject(buttons[selectedIndex].gameObject);
        buttons[selectedIndex].Select();
    }

    void NavigateHorizontal(int direction)
    {
        sfx.ButtonSound();
        buttons[selectedIndex].OnDeselect(null);

        if (picked)
        {
            if (selectedIndex < 4) selectedIndex = 4;

            selectedIndex += direction;
            if (selectedIndex > 6) selectedIndex = 4;
            if (selectedIndex < 4) selectedIndex = 6;
        }
        else
        {
            int newIndex = selectedIndex + direction;
            int currentRow = selectedIndex / cols;

            if (newIndex >= (currentRow + 1) * cols) newIndex = currentRow * cols;
            if (newIndex < currentRow * cols) newIndex = (currentRow + 1) * cols - 1;
            if (newIndex > 3) newIndex = 0;

            selectedIndex = newIndex;
        }

        EventSystem.current.SetSelectedGameObject(buttons[selectedIndex].gameObject);
        buttons[selectedIndex].Select();
    }

    void AutoSelectButton()
    {
        if (buttons.Length > 0)
        {
            sfx.ButtonSound();
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
            buttons[0].Select();
            notSelected = false;
        }
    }

    public void OnStageSelected()
    {
        stageSelected = true;
        CheckReadyToProceed();
    }

    public void OnGameModeSelected()
    {
        gameModeSelected = true;
        CheckReadyToProceed();
    }

    void CheckReadyToProceed()
    {
        if (stageSelected && gameModeSelected)
        {
            picked = true;
            proceedButton.interactable = true;
        }
    }
}
