using UnityEngine;

public class SlotSelectionHandler : MonoBehaviour
{
    public RulesetManager rulesetManager;      // Reference to RulesetManager
    public GameObject customSettingsMenu;      // The panel with your "Custom Settings" screen
    public GameObject editSettingsPanel;       // The panel with your "Edit Settings" screen
    public CustomRulesetUI customRulesetUI;    // Reference to the UI script that controls Edit Settings

    // Called when clicking or pressing Enter on Edit buttons
    public void EditSlot(int slotNumber)
    {
        // Load the ruleset from the selected slot
        CustomRuleset ruleset = rulesetManager.LoadCustomRuleset(slotNumber);

        // Save the active slot info globally (optional, if needed later)
        CustomRulesetScreenManager.selectedSlot = slotNumber;
        CustomRulesetScreenManager.currentRuleset = ruleset;

        // Hide the Custom Settings panel
        if (customSettingsMenu != null)
            customSettingsMenu.SetActive(false);

        // Show the Edit Settings panel
        if (editSettingsPanel != null)
            editSettingsPanel.SetActive(true);

        // Initialize the edit panel with the loaded ruleset
        if (customRulesetUI != null)
            customRulesetUI.Initialize(slotNumber);
    }
}
