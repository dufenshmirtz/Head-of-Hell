using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomRulesetUI : MonoBehaviour
{
    public TMP_InputField slotNameInput;
    public Button rounds1Button, rounds2Button, rounds3Button;
    public Button powerupsYesButton, powerupsNoButton;
    public TMP_InputField healthInput;
    public Button saveButton;
    public Button speedButtonSlow, speedButtonNormal, speedButtonFast, speedButtonDoped;
    public Button quick, heavy, block, special, charge;
    public Button hideHealthButton;
    public Button devToolsButton;
    public Button ppButton0, ppButton1, ppButton2, ppButton1non, ppButton2non;
    public Button ccYesButton, ccNoButton;

    public SlotNameInitializer slotNameInitializer;
    public TextMeshProUGUI currentSettingDisplay;

    private int rounds = 1;
    private bool powerupsEnabled = true;
    private int health = 100;
    private int selectedSlot;
    private int playerSpeed = 4;
    private bool quickDisabled = false, heavyDisabled = false, blockDisabled = false, specialDisabled = false, chargeDisabled = false;
    private bool hideHealth = false;
    private bool devTools = false;
    private int portals = 0;
    private bool chanChanEnabled = true;

    void Start()
    {
        rounds1Button.onClick.AddListener(() => SetRounds(1));
        rounds2Button.onClick.AddListener(() => SetRounds(3));
        rounds3Button.onClick.AddListener(() => SetRounds(5));

        powerupsYesButton.onClick.AddListener(() => SetPowerups(true));
        powerupsNoButton.onClick.AddListener(() => SetPowerups(false));

        speedButtonSlow.onClick.AddListener(() => SetSpeed(3));
        speedButtonNormal.onClick.AddListener(() => SetSpeed(4));
        speedButtonFast.onClick.AddListener(() => SetSpeed(5));
        speedButtonDoped.onClick.AddListener(() => SetSpeed(6));

        quick.onClick.AddListener(() => ToggleAbility(ref quickDisabled, quick));
        heavy.onClick.AddListener(() => ToggleAbility(ref heavyDisabled, heavy));
        block.onClick.AddListener(() => ToggleAbility(ref blockDisabled, block));
        special.onClick.AddListener(() => ToggleAbility(ref specialDisabled, special));
        charge.onClick.AddListener(() => ToggleAbility(ref chargeDisabled, charge));

        ppButton0.onClick.AddListener(() => Setportals(0));
        ppButton1.onClick.AddListener(() => Setportals(1));
        ppButton2.onClick.AddListener(() => Setportals(2));
        ppButton1non.onClick.AddListener(() => Setportals(3));
        ppButton2non.onClick.AddListener(() => Setportals(4));

        healthInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        hideHealthButton.onClick.AddListener(() => ToggleButton(ref hideHealth, hideHealthButton));
        devToolsButton.onClick.AddListener(() => ToggleButton(ref devTools, devToolsButton));

        saveButton.onClick.AddListener(SaveCustomRuleset);

        ccYesButton.onClick.AddListener(() => SetChanChanMode(true));
        ccNoButton.onClick.AddListener(() => SetChanChanMode(false));
    }

    public void Initialize(int slotNumber)
    {
        selectedSlot = slotNumber;
        CustomRuleset ruleset = RulesetManager.Instance.LoadCustomRuleset(slotNumber);

        if (ruleset != null)
        {
            slotNameInput.text = ruleset.slotName;
            healthInput.text = ruleset.health.ToString();
            SetRounds(ruleset.rounds);
            SetPowerups(ruleset.powerupsEnabled);
            SetHideHealth(ruleset.hideHealth);
            SetSpeed(ruleset.playerSpeed);
            SetAbilityStates(ruleset.quickDisabled, ruleset.heavyDisabled, ruleset.blockDisabled, ruleset.specialDisabled, ruleset.chargeDisabled);
            SetDevTools(ruleset.devTools);
            Setportals(ruleset.portals);
            SetChanChanMode(ruleset.chanChan);
        }
        else
        {
            slotNameInput.text = "";
            healthInput.text = "100";
            SetRounds(1);
            SetPowerups(true);
            SetHideHealth(false);
            SetSpeed(4);
            SetAbilityStates(false, false, false, false, false);
            SetDevTools(false);
            Setportals(0);
            SetChanChanMode(false);
        }
    }

    private void SetRounds(int roundValue)
    {
        rounds = roundValue;
        rounds1Button.interactable = rounds != 1;
        rounds2Button.interactable = rounds != 3;
        rounds3Button.interactable = rounds != 5;
    }

    private void SetPowerups(bool isEnabled)
    {
        powerupsEnabled = isEnabled;
        powerupsYesButton.interactable = !powerupsEnabled;
        powerupsNoButton.interactable = powerupsEnabled;
    }

    private void SetChanChanMode(bool isEnabled)
    {
        chanChanEnabled = isEnabled;
        ccYesButton.interactable = !chanChanEnabled;
        ccNoButton.interactable = chanChanEnabled;
    }

    private void Setportals(int roundValue)
    {
        portals = roundValue;
        ppButton0.interactable = portals != 0;
        ppButton1.interactable = portals != 1;
        ppButton2.interactable = portals != 2;
        ppButton1non.interactable = portals != 3;
        ppButton2non.interactable = portals != 4;
    }

    private void SetHideHealth(bool hide)
    {
        hideHealth = hide;
        UpdateButtonVisual(hideHealthButton, hideHealth);
    }

    private void SetDevTools(bool dev)
    {
        devTools = dev;
        UpdateButtonVisual(devToolsButton, devTools);
    }

    private void SetSpeed(int speedValue)
    {
        playerSpeed = speedValue;
        speedButtonSlow.interactable = playerSpeed != 3;
        speedButtonNormal.interactable = playerSpeed != 4;
        speedButtonFast.interactable = playerSpeed != 5;
        speedButtonDoped.interactable = playerSpeed != 6;
    }

    private void ToggleAbility(ref bool abilityDisabled, Button button)
    {
        abilityDisabled = !abilityDisabled;
        UpdateButtonVisual(button, abilityDisabled);
    }

    private void ToggleButton(ref bool hide, Button button)
    {
        hide = !hide;
        UpdateButtonVisual(button, hide);
    }

    private void UpdateButtonVisual(Button button, bool abilityDisabled)
    {
        if (button == null || button.transform.childCount == 0)
        {
            Debug.LogWarning("Button visual setup missing child.");
            return;
        }

        Transform firstChild = button.transform.GetChild(0);
        firstChild.gameObject.SetActive(abilityDisabled);
    }

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

    private void SaveCustomRuleset()
    {
        int.TryParse(healthInput.text, out health);

        if (health <= 0)
            health = 100;

        string cleanSlotName = string.IsNullOrWhiteSpace(slotNameInput.text)
            ? "Empty"
            : slotNameInput.text.Trim();

        CustomRuleset newRuleset = new CustomRuleset()
        {
            slotName = cleanSlotName,
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
            devTools = devTools,
            portals = portals,
            chanChan = chanChanEnabled
        };

        RulesetManager.Instance.SaveCustomRuleset(selectedSlot, newRuleset);

        CustomRulesetScreenManager.selectedSlot = selectedSlot;
        CustomRulesetScreenManager.currentRuleset = newRuleset;
        RulesetSelectionState.SelectSlot(selectedSlot);

        if (slotNameInitializer != null)
            slotNameInitializer.UpdateSlotName(selectedSlot);

        if (currentSettingDisplay != null)
            currentSettingDisplay.text = "Current Setting: " + cleanSlotName;

        PrintCustomRuleset(newRuleset);
    }

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