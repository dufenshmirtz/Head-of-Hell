using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MenuStatsDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text statsDisplayText;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button backButton;
    [SerializeField] private MainMenuMusic sfx;  // Reference to your sound effect script

    private Button[] buttons;
    private int selectedIndex = 0;
    private bool notSelected = true;

    void Start()
    {
        UpdateStatsDisplay();
        buttons = new Button[] { resetButton, backButton };
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (notSelected)
            {
                AutoSelectButton();
                return;
            }
            Navigate(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (notSelected)
            {
                AutoSelectButton();
                return;
            }
            Navigate(1);
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ClickSelectedButton();
        }
    }

    void Navigate(int direction)
    {
        if (sfx != null)
            sfx.ButtonSound();

        // Deselect the current button (optional, can trigger OnDeselect callbacks)
        buttons[selectedIndex].OnDeselect(null);

        selectedIndex += direction;
        if (selectedIndex < 0) selectedIndex = buttons.Length - 1;
        else if (selectedIndex >= buttons.Length) selectedIndex = 0;

        EventSystem.current.SetSelectedGameObject(buttons[selectedIndex].gameObject);
        buttons[selectedIndex].Select();
    }

    void ClickSelectedButton()
    {
        var pointer = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(buttons[selectedIndex].gameObject, pointer, ExecuteEvents.pointerClickHandler);
    }

    void AutoSelectButton()
    {
        if (buttons.Length > 0)
        {
            if (sfx != null)
                sfx.ButtonSound();

            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
            buttons[0].Select();
            selectedIndex = 0;
            notSelected = false;
        }
    }

    private void UpdateStatsDisplay()
    {
        if (statsDisplayText == null)
        {
            Debug.LogError("StatsDisplayText is not assigned in the inspector.");
            return;
        }

        string statsText = "Character Win Ratios:\n";
        foreach (var character in CharacterStatsManager.Instance.GetCharacterStats())
        {
            float winRatio = character.Value.GetWinRatio();
            statsText += $"\n{character.Key}: {winRatio:P2} (Wins: {character.Value.wins}, Games: {character.Value.totalGames})\n";
        }

        statsDisplayText.text = statsText;
    }

    public void ResetStats()
    {
        CharacterStatsManager.Instance.ResetAllStats();
        UpdateStatsDisplay();
    }
}
