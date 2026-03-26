using TMPro;
using UnityEngine;

public class SlotSelectionHandler : MonoBehaviour
{
    public RulesetManager rulesetManager;
    public GameObject customSettingsMenu;
    public GameObject editSettingsPanel;
    public CustomRulesetUI customRulesetUI;
    public TextMeshProUGUI currentSettingDisplay;
    public SlotNameInitializer slotNameInitializer;

    public TMP_Text customMenuCurrentText;

    public void EditSlot(int slotNumber)
    {
        CustomRuleset ruleset = rulesetManager.LoadCustomRuleset(slotNumber);

        CustomRulesetScreenManager.selectedSlot = slotNumber;
        CustomRulesetScreenManager.currentRuleset = ruleset;

        if (customSettingsMenu != null)
            customSettingsMenu.SetActive(false);

        if (editSettingsPanel != null)
            editSettingsPanel.SetActive(true);

        if (customRulesetUI != null)
            customRulesetUI.Initialize(slotNumber);
    }

    public void PreviewSlot(int slotNumber)
    {
        CustomRuleset ruleset = rulesetManager.LoadCustomRuleset(slotNumber);

        string displayName = (ruleset != null && !string.IsNullOrWhiteSpace(ruleset.slotName))
            ? ruleset.slotName
            : "Empty";

        if (currentSettingDisplay != null)
            currentSettingDisplay.text = "Current Setting: " + displayName;

        if (customMenuCurrentText != null)
            customMenuCurrentText.text = displayName;

        CustomRulesetScreenManager.selectedSlot = slotNumber;
        CustomRulesetScreenManager.currentRuleset = ruleset;
        RulesetSelectionState.SelectSlot(slotNumber);
    }

    public void SelectSlot(int slotNumber)
    {
        RulesetSelectionState.SelectSlot(slotNumber);

        CustomRuleset loadedRuleset = RulesetManager.Instance.LoadCustomRuleset(slotNumber);
        CustomRulesetScreenManager.selectedSlot = slotNumber;
        CustomRulesetScreenManager.currentRuleset = loadedRuleset;

        Debug.Log("Selected Slot = " + slotNumber);
    }

    public void BackToCustomSettings()
    {
        if (editSettingsPanel != null) editSettingsPanel.SetActive(false);
        if (customSettingsMenu != null) customSettingsMenu.SetActive(true);

        if (slotNameInitializer != null)
            slotNameInitializer.InitializeSlotNames();

        if (RulesetSelectionState.SelectedSlot > 0)
        {
            CustomRuleset ruleset = RulesetManager.Instance.LoadCustomRuleset(RulesetSelectionState.SelectedSlot);

            string displayName = (ruleset != null && !string.IsNullOrWhiteSpace(ruleset.slotName))
                ? ruleset.slotName
                : "Empty";

            if (currentSettingDisplay != null)
                currentSettingDisplay.text = "Current Setting: " + displayName;

            if (customMenuCurrentText != null)
                customMenuCurrentText.text = displayName;
        }
    }
}