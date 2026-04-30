using System.Collections.Generic;
using UnityEngine;

public class ProfileSelectorsUI : MonoBehaviour
{
    public ProfileCarouselSelector p1Selector;
    public ProfileCarouselSelector p2Selector;
    public ProfileCarouselSelector p3Selector;
    public GameObject p3SelectionRoot;

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
        if (p3Selector != null)
            p3Selector.OnIndexChanged -= OnP3Changed;

        p1Selector.OnIndexChanged += OnP1Changed;
        p2Selector.OnIndexChanged += OnP2Changed;
        if (p3Selector != null)
            p3Selector.OnIndexChanged += OnP3Changed;
    }

    public void RefreshSelectors()
    {
        if (ProfileManager.I == null) return;

        bool showP3 = GameModeSelectionState.RequiresThirdPlayerSelection;
        if (p3SelectionRoot != null)
            p3SelectionRoot.SetActive(showP3);

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
        if (p3Selector != null)
            p3Selector.SetOptions(names);

        // Sync with current selection
        p1Selector.SetIndexWithoutNotify(FindOptionForPlayer(1));
        p2Selector.SetIndexWithoutNotify(FindOptionForPlayer(2));
        if (showP3 && p3Selector != null)
            p3Selector.SetIndexWithoutNotify(FindOptionForPlayer(3));
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
    private void OnP3Changed(int index) => OnChanged(3, index);

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
