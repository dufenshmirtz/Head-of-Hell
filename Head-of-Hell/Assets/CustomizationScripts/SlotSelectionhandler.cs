using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SlotSelectionHandler : MonoBehaviour
{
    public RulesetManager rulesetManager; // Reference to RulesetManager
    public Text currentSettingDisplay;    // Text to display the current setting

    private int selectedSlot = -1;        // Variable to track the selected slot

    // Method to handle slot selection and open the customization screen
    public void EditSlot(int slotNumber)
    {
        // Load the ruleset from the saved slot
        CustomRuleset ruleset = rulesetManager.LoadCustomRuleset(slotNumber);

        // Pass the selected slot and ruleset to the next scene
        CustomRulesetScreenManager.selectedSlot = slotNumber;
        CustomRulesetScreenManager.currentRuleset = ruleset;

        // Load the custom ruleset UI scene
        SceneManager.LoadScene("CustomRulesetScene"); // Adjust to your scene name
    }
}
