using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Import TextMeshPro for input fields

public class CustomRulesetUI : MonoBehaviour
{
    public TMP_InputField slotNameInput;     // TextMeshPro InputField for slot name
    public Button rounds1Button, rounds2Button, rounds3Button;  // Buttons for rounds
    public Button powerupsYesButton, powerupsNoButton;          // Buttons for powerups
    public TMP_InputField healthInput;       // TextMeshPro InputField for health
    public Button saveButton;                // Save button

    private int rounds = 1;
    private bool powerupsEnabled = true;
    private int health = 100;
    private int selectedSlot; // The currently selected slot

    void Start()
    {
        // Set up the button listeners for round buttons
        rounds1Button.onClick.AddListener(() => SetRounds(1));
        rounds2Button.onClick.AddListener(() => SetRounds(3));
        rounds3Button.onClick.AddListener(() => SetRounds(5));

        // Set up the button listeners for powerup buttons
        powerupsYesButton.onClick.AddListener(() => SetPowerups(true));
        powerupsNoButton.onClick.AddListener(() => SetPowerups(false));

        // Restrict health input to only integers
        healthInput.contentType = TMP_InputField.ContentType.IntegerNumber;

        // Set up the listener for save button
        saveButton.onClick.AddListener(SaveCustomRuleset);
    }

    public void Initialize(int slotNumber)
    {
        // Load the current slot settings into the UI if a ruleset exists
        selectedSlot = slotNumber;
        CustomRuleset ruleset=  RulesetManager.Instance.LoadCustomRuleset(slotNumber);

        print(slotNumber + "___init");

        if (ruleset != null)
        {
            slotNameInput.text = ruleset.slotName;
            healthInput.text = ruleset.health.ToString();
            SetRounds(ruleset.rounds);
            SetPowerups(ruleset.powerupsEnabled);
        }
        else
        {
            // Default values
            slotNameInput.text = "";
            healthInput.text = "100";
            SetRounds(1);
            SetPowerups(true);
        }
    }

    // Method to set rounds and update UI
    private void SetRounds(int roundValue)
    {
        rounds = roundValue;
        // Update button visuals if needed
        rounds1Button.interactable = rounds != 1;
        rounds2Button.interactable = rounds != 3;
        rounds3Button.interactable = rounds != 5;
    }

    // Method to set powerups and update UI
    private void SetPowerups(bool isEnabled)
    {
        powerupsEnabled = isEnabled;
        // Update button visuals if needed
        powerupsYesButton.interactable = !powerupsEnabled;
        powerupsNoButton.interactable = powerupsEnabled;
    }

    // Method to save the current ruleset
    private void SaveCustomRuleset()
    {
        // Parse health value
        int.TryParse(healthInput.text, out health);

        // Create new ruleset
        CustomRuleset newRuleset = new CustomRuleset()
        {
            slotName = slotNameInput.text,
            rounds = rounds,
            powerupsEnabled = powerupsEnabled,
            health = health
        };

        print(selectedSlot + "___");
        // Save the ruleset to the selected slot
        RulesetManager.Instance.SaveCustomRuleset(selectedSlot, newRuleset);

        // Print the results to verify the save
        PrintCustomRuleset(newRuleset);
    }

    // Method to print the ruleset details for debugging
    private void PrintCustomRuleset(CustomRuleset ruleset)
    {
        Debug.Log($"Slot Name: {ruleset.slotName}");
        Debug.Log($"Rounds: {ruleset.rounds}");
        Debug.Log($"Powerups Enabled: {ruleset.powerupsEnabled}");
        Debug.Log($"Health: {ruleset.health}");
    }
}
