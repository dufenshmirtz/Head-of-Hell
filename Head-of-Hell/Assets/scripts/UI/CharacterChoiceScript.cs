using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterChoiceScript : MonoBehaviour
{
    public Button[] buttons;  // Assign your buttons in the Inspector
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
        }
    }

    public void BothPicked(bool didThey)
    {
        bothpicked = didThey;
    }
}
