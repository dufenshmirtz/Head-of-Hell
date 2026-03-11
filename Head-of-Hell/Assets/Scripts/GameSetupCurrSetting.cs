using TMPro;
using UnityEngine;

public class GameSetupCurrentSetting : MonoBehaviour
{
    public TextMeshProUGUI currentSettingText;

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (currentSettingText == null) return;

        if (RulesetSelectionState.SelectedSlot == -1)
        {
            currentSettingText.text = "Default";
            return;
        }

        CustomRuleset ruleset = RulesetManager.Instance.LoadCustomRuleset(RulesetSelectionState.SelectedSlot);
        string name = (ruleset != null && !string.IsNullOrEmpty(ruleset.slotName)) ? ruleset.slotName : "Empty";
        currentSettingText.text = name;
    }
}