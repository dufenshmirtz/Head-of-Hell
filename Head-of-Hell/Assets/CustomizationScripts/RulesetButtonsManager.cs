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
        choice.text = ruleset.slotName; 
        // Load the CustomRuleset for the specified ruleNumber

        // Check if the ruleset exists for the given ruleNumber
        if (ruleset != null)
        {
            // Save the ruleset data to PlayerPrefs as JSON
            string json = JsonUtility.ToJson(ruleset);
            PlayerPrefs.SetString("SelectedRuleset", json);
            PlayerPrefs.Save(); // Make sure data is saved

            Debug.Log($"Ruleset for Slot {ruleNumber} saved to PlayerPrefs");
        }
        else
        {
            Debug.LogWarning($"No ruleset found for Slot {ruleNumber}");
        }
    }
}
