using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProfileDropdownsUI : MonoBehaviour
{
    public TMP_Dropdown p1Dropdown;
    public TMP_Dropdown p2Dropdown;
    public TMP_Dropdown p3Dropdown;
    public GameObject p3SelectionRoot;

    // mapping: dropdown option index -> profileIndex (-2 guest, >=0 slot index)
    private readonly List<int> optionMap = new List<int>();

    private const int GUEST = -2;

    private void OnEnable()
    {
        StartCoroutine(InitNextFrame());
    }
    private System.Collections.IEnumerator InitNextFrame()
    {
        yield return null;
        RefreshDropdowns();

        p1Dropdown.onValueChanged.RemoveAllListeners();
        p2Dropdown.onValueChanged.RemoveAllListeners();
        if (p3Dropdown != null)
            p3Dropdown.onValueChanged.RemoveAllListeners();

        p1Dropdown.onValueChanged.AddListener(v => OnChanged(1, v));
        p2Dropdown.onValueChanged.AddListener(v => OnChanged(2, v));
        if (p3Dropdown != null)
            p3Dropdown.onValueChanged.AddListener(v => OnChanged(3, v));
    }
    public void RefreshDropdowns()
    {
        if (ProfileManager.I == null) return;

        bool showP3 = GameModeSelectionState.RequiresThirdPlayerSelection;
        if (p3SelectionRoot != null)
            p3SelectionRoot.SetActive(showP3);

        // Build options once and apply to both dropdowns
        optionMap.Clear();
        var options = new List<TMP_Dropdown.OptionData>();

        // 0) Guest
        optionMap.Add(GUEST);
        options.Add(new TMP_Dropdown.OptionData("Guest"));

        // 1..n) saved profiles
        var db = ProfileManager.I.GetDatabase();
        for (int i = 0; i < ProfilesDatabase.MaxProfiles; i++)
        {
            var p = db.GetAt(i);
            if (p == null) continue;

            optionMap.Add(i);
            options.Add(new TMP_Dropdown.OptionData(p.profileName));
        }

        ApplyOptions(p1Dropdown, options);
        ApplyOptions(p2Dropdown, options);
        if (p3Dropdown != null)
            ApplyOptions(p3Dropdown, options);

        // set dropdown values based on current selections
        p1Dropdown.SetValueWithoutNotify(FindOptionForPlayer(1));
        p2Dropdown.SetValueWithoutNotify(FindOptionForPlayer(2));
        if (showP3 && p3Dropdown != null)
            p3Dropdown.SetValueWithoutNotify(FindOptionForPlayer(3));
    }

    private void ApplyOptions(TMP_Dropdown dd, List<TMP_Dropdown.OptionData> options)
    {
        dd.ClearOptions();
        dd.AddOptions(options);
    }

    private int FindOptionForPlayer(int playerNum)
    {
        // default = Guest
        if (ProfileManager.I.IsGuestSelected(playerNum)) return 0;

        var selected = ProfileManager.I.GetSelectedProfile(playerNum);
        if (selected == null) return 0;

        // find matching slot by id in map
        for (int opt = 0; opt < optionMap.Count; opt++)
        {
            int idx = optionMap[opt];
            if (idx >= 0)
            {
                var p = ProfileManager.I.GetDatabase().GetAt(idx);
                if (p != null && p.id == selected.id) return opt;
            }
        }
        return 0;
    }

    private void OnChanged(int playerNum, int optionIndex)
    {
        if (ProfileManager.I == null) return;
        if (optionIndex < 0 || optionIndex >= optionMap.Count) return;

        int mapped = optionMap[optionIndex];

        if (mapped == GUEST) ProfileManager.I.SelectGuest(playerNum);
        else ProfileManager.I.SelectProfile(playerNum, mapped);
    }
}
