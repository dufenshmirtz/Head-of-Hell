using System.Collections.Generic;
using UnityEngine;

public class ProfileSelectorsUI : MonoBehaviour
{
    public ProfileCarouselSelector p1Selector;
    public ProfileCarouselSelector p2Selector;

    // mapping: UI index -> profileIndex (-2 guest, >=0 slot index)
    private readonly List<int> optionMap = new List<int>();

    private const int GUEST = -2;

    private void OnEnable()
    {
        StartCoroutine(InitNextFrame());
    }

    private System.Collections.IEnumerator InitNextFrame()
    {
        yield return null;

        RefreshSelectors();

        p1Selector.OnIndexChanged -= OnP1Changed;
        p2Selector.OnIndexChanged -= OnP2Changed;

        p1Selector.OnIndexChanged += OnP1Changed;
        p2Selector.OnIndexChanged += OnP2Changed;
    }

    public void RefreshSelectors()
    {
        if (ProfileManager.I == null) return;

        optionMap.Clear();
        var names = new List<string>();

        // 0) Guest
        optionMap.Add(GUEST);
        names.Add("Guest");

        // profiles
        var db = ProfileManager.I.GetDatabase();
        for (int i = 0; i < ProfilesDatabase.MaxProfiles; i++)
        {
            var p = db.GetAt(i);
            if (p == null) continue;

            optionMap.Add(i);
            names.Add(p.profileName);
        }

        // Apply to selectors
        p1Selector.SetOptions(names);
        p2Selector.SetOptions(names);

        // Sync with current selection
        p1Selector.SetIndexWithoutNotify(FindOptionForPlayer(1));
        p2Selector.SetIndexWithoutNotify(FindOptionForPlayer(2));
    }

    private int FindOptionForPlayer(int playerNum)
    {
        if (ProfileManager.I.IsGuestSelected(playerNum)) return 0;

        var selected = ProfileManager.I.GetSelectedProfile(playerNum);
        if (selected == null) return 0;

        for (int opt = 0; opt < optionMap.Count; opt++)
        {
            int idx = optionMap[opt];
            if (idx >= 0)
            {
                var p = ProfileManager.I.GetDatabase().GetAt(idx);
                if (p != null && p.id == selected.id)
                    return opt;
            }
        }

        return 0;
    }

    private void OnP1Changed(int index) => OnChanged(1, index);
    private void OnP2Changed(int index) => OnChanged(2, index);

    private void OnChanged(int playerNum, int optionIndex)
    {
        if (ProfileManager.I == null) return;
        if (optionIndex < 0 || optionIndex >= optionMap.Count) return;

        int mapped = optionMap[optionIndex];

        if (mapped == GUEST)
            ProfileManager.I.SelectGuest(playerNum);
        else
            ProfileManager.I.SelectProfile(playerNum, mapped);
    }
}