using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SlotNameInitializer : MonoBehaviour
{
    // Public list of five TMP Text elements (assign in Unity Inspector)
    public List<TMP_Text> slotNameTexts = new List<TMP_Text>(5);

    void OnEnable()
    {
        InitializeSlotNames();
    }

    // Method to initialize the TMP text fields with the slot names
    public void InitializeSlotNames()
    {
        for (int i = 0; i < slotNameTexts.Count; i++)  // Loop through all TMP Texts
        {
            // Load the CustomRuleset for this slot (slots are 1-indexed, so i + 1)
            CustomRuleset ruleset = RulesetManager.Instance.LoadCustomRuleset(i);

            // Check if a ruleset exists for this slot
            if (ruleset != null)
            {
                // Set the corresponding TMP text field to the slot's name
                slotNameTexts[i].text = ruleset.slotName;
            }
            else
            {
                // If no ruleset exists, mark the slot as empty
                slotNameTexts[i].text = "Empty";
            }
        }
    }
}
