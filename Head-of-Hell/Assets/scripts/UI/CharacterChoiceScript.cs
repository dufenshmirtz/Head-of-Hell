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

        // Automatically assign OnSelect event for each button
        foreach (Button btn in buttons)
        {
            EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = btn.gameObject.AddComponent<EventTrigger>();
            }

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Select;
            entry.callback.AddListener((eventData) => { OnButtonHighlighted(btn); });

            trigger.triggers.Add(entry);
        }
    }

    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow)) && notSelected)
        {
            AutoSelectButton();
        }
        
        // debug for deselect if (bothpicked)
        //{
            //Debug.Log("Both players have picked");
        //}

        // Check if both players have picked and Enter is pressed
        if (bothpicked && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            Debug.Log("Enter Key pressed, trying to start game");
            TriggerStartButton();
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

    public void OnButtonHighlighted(Button button)
    {
        if (!bothpicked)
        {
            sfx.ButtonSound();
            characterChoiceMenu.HoveringIn(button.name);

            // Hide border from the last highlighted button
            if (lastHighlightedButton != null && lastHighlightedButton != button)
            {
                Transform lastBorder = lastHighlightedButton.transform.Find("Border");
                if (lastBorder != null)
                {
                    lastBorder.gameObject.SetActive(false);
                }
            }

            // Show border on the currently highlighted button
            Transform border = button.transform.Find("Border");
            if (border != null)
            {
                border.gameObject.SetActive(true);
            }

            // Save the current button as last highlighted
            lastHighlightedButton = button;
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
