using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Import SceneManager

public class CharacterChoiceScript : MonoBehaviour
{
    private Button lastHighlightedButton = null;

    public Button[] buttons;  // Assign your buttons in the Inspector
    public Button startButton; // Assign your Start button in the Inspector
    public MainMenuMusic sfx;
    public CharacterChoiceMenu characterChoiceMenu;  // Reference to the CharacterChoiceMenu script
    private bool notSelected = true;
    public bool bothpicked = false;

    void Start()
    {
        notSelected = true;

        foreach (Button btn in buttons)
        {
            // Hide all borders at start
            Transform border = btn.transform.Find("Border");
            if (border != null)
            {
                border.gameObject.SetActive(false);
            }

            EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = btn.gameObject.AddComponent<EventTrigger>();
            }

            // SELECT event
            EventTrigger.Entry selectEntry = new EventTrigger.Entry();
            selectEntry.eventID = EventTriggerType.Select;
            selectEntry.callback.AddListener((eventData) => { OnButtonHighlighted(btn); });
            trigger.triggers.Add(selectEntry);

            // DESELECT event
            EventTrigger.Entry deselectEntry = new EventTrigger.Entry();
            deselectEntry.eventID = EventTriggerType.Deselect;
            deselectEntry.callback.AddListener((eventData) => { OnButtonUnhighlighted(btn); });
            trigger.triggers.Add(deselectEntry);
        }
    }

    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow)) && notSelected)
        {
            AutoSelectButton();
        }

        if (bothpicked && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            Debug.Log("Enter Key pressed, trying to start game");
            TriggerStartButton();
        }
    }

    void AutoSelectButton()
    {
        if (buttons.Length > 0)
        {
            sfx.ButtonSound();
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
            buttons[0].Select();
            notSelected = false;

            characterChoiceMenu.HoveringIn(buttons[0].name);
        }
    }

    public void OnButtonHighlighted(Button button)
    {
        if (!bothpicked)
        {
            sfx.ButtonSound();
            characterChoiceMenu.HoveringIn(button.name);

            // Show border on the currently highlighted button
            Transform border = button.transform.Find("Border");
            if (border != null)
            {
                border.gameObject.SetActive(true);
            }

            lastHighlightedButton = button;
        }
    }

    public void OnButtonUnhighlighted(Button button)
    {
        // Hide border when button is no longer selected
        Transform border = button.transform.Find("Border");
        if (border != null)
        {
            border.gameObject.SetActive(false);
        }

        // Clear last highlighted if needed
        if (lastHighlightedButton == button)
        {
            lastHighlightedButton = null;
        }
    }

    public void BothPicked(bool didThey)
    {
        bothpicked = didThey;
    }

    void TriggerStartButton()
    {
        if (startButton != null)
        {
            startButton.onClick.Invoke(); // Simulates a button click
        }
    }
}
