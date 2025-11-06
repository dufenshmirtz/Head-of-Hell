using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RulesetButtonsManager : MonoBehaviour
{
    public int ruleNumber;
    public TMPro.TMP_Text choice;

    public void SaveRuleToPlayerPrefs()
    {
        CustomRuleset ruleset = RulesetManager.Instance.LoadCustomRuleset(ruleNumber);

        // Check if the ruleset exists for the given ruleNumber
        if (ruleset != null)
        {
            // Save the ruleset data to PlayerPrefs as JSON
            string json = JsonUtility.ToJson(ruleset);
            PlayerPrefs.SetString("SelectedRuleset", json);
            PlayerPrefs.Save(); // Make sure data is saved
            RulesetManager.Instance.SetRulesetNum(ruleNumber);

            // ✅ Update the current setting name so StageChoiceManager shows it
            CurrentSettingsData.currentRulesetName = ruleset.slotName;

            // ✅ (Optional) persist it between sessions
            PlayerPrefs.SetString("LastUsedRulesetName", ruleset.slotName);
            PlayerPrefs.Save();

            // ✅ Update the UI text in this menu
            if (choice != null)
                choice.text = ruleset.slotName;

            Debug.Log($"Ruleset for Slot {ruleNumber} saved to PlayerPrefs and set as current.");
        }
        else
        {
            Debug.LogWarning($"No ruleset found for Slot {ruleNumber}");
        }
    }
}
