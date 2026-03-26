using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SlotNameInitializer : MonoBehaviour
{
    public List<TMP_Text> slotNameTexts = new List<TMP_Text>(5);

    void OnEnable()
    {
        InitializeSlotNames();
    }

    public void InitializeSlotNames()
    {
        for (int i = 0; i < slotNameTexts.Count; i++)
        {
            CustomRuleset ruleset = RulesetManager.Instance.LoadCustomRuleset(i + 1);

            if (ruleset != null && !string.IsNullOrWhiteSpace(ruleset.slotName))
                slotNameTexts[i].text = ruleset.slotName;
            else
                slotNameTexts[i].text = "Empty";
        }
    }

    public void UpdateSlotName(int slotIndex)
    {
        int uiIndex = slotIndex - 1;

        if (uiIndex < 0 || uiIndex >= slotNameTexts.Count)
        {
            Debug.LogError("Invalid slot index for UI: " + slotIndex);
            return;
        }

        CustomRuleset ruleset = RulesetManager.Instance.LoadCustomRuleset(slotIndex);

        if (ruleset != null && !string.IsNullOrWhiteSpace(ruleset.slotName))
            slotNameTexts[uiIndex].text = ruleset.slotName;
        else
            slotNameTexts[uiIndex].text = "Empty";
    }
}