using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Import TextMeshPro for input fields

public class CustomRulesetUI : MonoBehaviour
{
    //interface
    public TMP_InputField slotNameInput;     // TextMeshPro InputField for slot name
    public Button rounds1Button, rounds2Button, rounds3Button;  // Buttons for rounds
    public Button powerupsYesButton, powerupsNoButton;          // Buttons for powerups
    public TMP_InputField healthInput;       // TextMeshPro InputField for health
    public Button saveButton;                // Save button
    public Button speedButtonSlow, speedButtonNormal, speedButtonFast, speedButtonDoped;
    public Button quick, heavy, block, special, charge;
    public Button hideHealthButton;
    public Button devToolsButton;

    //values
    private int rounds = 1;
    private bool powerupsEnabled = true;
    private int health = 100;
    private int selectedSlot; // The currently selected slot
    private int playerSpeed = 4; // Default to "Slow"
    private bool quickDisabled = false, heavyDisabled = false, blockDisabled = false, specialDisabled = false, chargeDisabled = false;
    private bool hideHealth = false;
    private bool devTools = false;

    void Start()
    {
        // Set up the button listeners for round buttons
        rounds1Button.onClick.AddListener(() => SetRounds(1));
        rounds2Button.onClick.AddListener(() => SetRounds(3));
        rounds3Button.onClick.AddListener(() => SetRounds(5));

        // Set up the button listeners for powerup buttons
        powerupsYesButton.onClick.AddListener(() => SetPowerups(true));
        powerupsNoButton.onClick.AddListener(() => SetPowerups(false));

        // Set up the button listeners for speed buttons
        speedButtonSlow.onClick.AddListener(() => SetSpeed(3));
        speedButtonNormal.onClick.AddListener(() => SetSpeed(4));
        speedButtonFast.onClick.AddListener(() => SetSpeed(5));
        speedButtonDoped.onClick.AddListener(() => SetSpeed(6));

        // Set up the button listeners for ability buttons
        quick.onClick.AddListener(() => ToggleAbility(ref quickDisabled, quick));
        heavy.onClick.AddListener(() => ToggleAbility(ref heavyDisabled, heavy));
        block.onClick.AddListener(() => ToggleAbility(ref blockDisabled, block));
        special.onClick.AddListener(() => ToggleAbility(ref specialDisabled, special));
        charge.onClick.AddListener(() => ToggleAbility(ref chargeDisabled, charge));

        // Restrict health input to only integers
        healthInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        hideHealthButton.onClick.AddListener(() => ToggleButton(ref hideHealth,hideHealthButton));

        devToolsButton.onClick.AddListener(() => ToggleButton(ref devTools, devToolsButton));

        // Set up the listener for save button
        saveButton.onClick.AddListener(SaveCustomRuleset);

    }

    public void Initialize(int slotNumber)
    {
        // Load the current slot settings into the UI if a ruleset exists
        selectedSlot = slotNumber;
        CustomRuleset ruleset = RulesetManager.Instance.LoadCustomRuleset(slotNumber);

        if (ruleset != null)
        {
            slotNameInput.text = ruleset.slotName;
            healthInput.text = ruleset.health.ToString();
            SetRounds(ruleset.rounds);
            SetPowerups(ruleset.powerupsEnabled);
            SetHideHealth(ruleset.hideHealth);
            // Load speed and ability states (if they are part of your CustomRuleset)
            SetSpeed(ruleset.playerSpeed);
            SetAbilityStates(ruleset.quickDisabled, ruleset.heavyDisabled, ruleset.blockDisabled, ruleset.specialDisabled, ruleset.chargeDisabled);
            SetDevTools(ruleset.devTools);
        }
        else
        {
            // Default values
            slotNameInput.text = "";
            healthInput.text = "100";
            SetRounds(1);
            SetPowerups(true);
            SetHideHealth(false);
            SetSpeed(4);
            SetAbilityStates(false, false, false, false, false);
            SetDevTools(false);
        }
    }

    // Method to set rounds and update UI
    private void SetRounds(int roundValue)
    {
        rounds = roundValue;
        rounds1Button.interactable = rounds != 1;
        rounds2Button.interactable = rounds != 3;
        rounds3Button.interactable = rounds != 5;
    }

    // Method to set powerups and update UI
    private void SetPowerups(bool isEnabled)
    {
        powerupsEnabled = isEnabled;
        powerupsYesButton.interactable = !powerupsEnabled;
        powerupsNoButton.interactable = powerupsEnabled;
    }

    private void SetHideHealth(bool hide)
    {
        hideHealth = hide;
        UpdateButtonVisual(hideHealthButton,hideHealth);
    }

    private void SetDevTools(bool dev)
    {
        devTools = dev;
        UpdateButtonVisual(devToolsButton, devTools);
    }

    // Method to set player speed and update UI
    private void SetSpeed(int speedValue)
    {
        playerSpeed = speedValue;
        speedButtonSlow.interactable = playerSpeed != 3;
        speedButtonNormal.interactable = playerSpeed != 4;
        speedButtonFast.interactable = playerSpeed != 5;
        speedButtonDoped.interactable = playerSpeed != 6;
    }

    // Method to toggle abilities on and off and update UI
    private void ToggleAbility(ref bool abilityDisabled, Button button)
    {
        abilityDisabled = !abilityDisabled; // Toggle the state
        UpdateButtonVisual(button, abilityDisabled); // Update the button's visual state
    }

    private void ToggleButton(ref bool hide, Button button)
    {
        hide = !hide; // Toggle the state
        UpdateButtonVisual(button, hide); // Update the button's visual state
    }

    // Method to update the button's visual state based on abilityDisabled
    private void UpdateButtonVisual(Button button, bool abilityDisabled)
    {
        Transform firstChild = button.transform.GetChild(0);

        if (firstChild != null)
        {
            // Enable or disable the first child based on abilityDisabled
            firstChild.gameObject.SetActive(abilityDisabled);
        }
        else
        {
            Debug.Log("Error possible wrong button setup.");
        }
    }

    // Method to set initial ability states and update button visuals
    private void SetAbilityStates(bool quickState, bool heavyState, bool blockState, bool specialState, bool chargeState)
    {
        quickDisabled = quickState;
        heavyDisabled = heavyState;
        blockDisabled = blockState;
        specialDisabled = specialState;
        chargeDisabled = chargeState;

        UpdateButtonVisual(quick, quickDisabled);
        UpdateButtonVisual(heavy, heavyDisabled);
        UpdateButtonVisual(block, blockDisabled);
        UpdateButtonVisual(special, specialDisabled);
        UpdateButtonVisual(charge, chargeDisabled);
    }

    // Method to save the current ruleset
    private void SaveCustomRuleset()
    {
        int.TryParse(healthInput.text, out health);

        CustomRuleset newRuleset = new CustomRuleset()
        {
            slotName = slotNameInput.text,
            rounds = rounds,
            powerupsEnabled = powerupsEnabled,
            health = health,
            hideHealth = hideHealth,
            playerSpeed = playerSpeed,
            quickDisabled = quickDisabled,
            heavyDisabled = heavyDisabled,
            blockDisabled = blockDisabled,
            specialDisabled = specialDisabled,
            chargeDisabled = chargeDisabled,
            devTools=devTools
        };

        RulesetManager.Instance.SaveCustomRuleset(selectedSlot, newRuleset);
        PrintCustomRuleset(newRuleset);
    }

    // Method to print the ruleset details for debugging
    private void PrintCustomRuleset(CustomRuleset ruleset)
    {
        Debug.Log($"Slot Name: {ruleset.slotName}");
        Debug.Log($"Rounds: {ruleset.rounds}");
        Debug.Log($"Powerups Enabled: {ruleset.powerupsEnabled}");
        Debug.Log($"Health: {ruleset.health}");
        Debug.Log($"Player Speed: {ruleset.playerSpeed}");
        Debug.Log($"Quick Disabled: {ruleset.quickDisabled}");
        Debug.Log($"Heavy Disabled: {ruleset.heavyDisabled}");
        Debug.Log($"Block Disabled: {ruleset.blockDisabled}");
        Debug.Log($"Special Disabled: {ruleset.specialDisabled}");
        Debug.Log($"Charge Disabled: {ruleset.chargeDisabled}");
    }
}
