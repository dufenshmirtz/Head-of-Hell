using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VictoryProfileSelection : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject changeProfilesPanel;

    [Header("Buttons")]
    [SerializeField] private Button changeProfilesButton;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button cancelButton;

    [Header("Selectors")]
    [SerializeField] private ProfileCarouselSelector p1Selector;
    [SerializeField] private ProfileCarouselSelector p2Selector;

    [Header("Config")]
    [SerializeField] private int maxProfileSlotsToScan = 8;

    private readonly List<int> availableProfileIndices = new List<int>();
    private readonly List<string> availableProfileNames = new List<string>();

    private void Awake()
    {
        if (changeProfilesButton != null)
            changeProfilesButton.onClick.AddListener(OpenPanel);

        if (applyButton != null)
            applyButton.onClick.AddListener(ApplySelection);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(ClosePanel);
    }

    private void Start()
    {
        if (changeProfilesPanel != null)
            changeProfilesPanel.SetActive(false);

        PrepareSelectors();
    }

    private void OnEnable()
    {
        PrepareSelectors();
    }

    private void PrepareSelectors()
    {
        LoadAvailableProfiles();

        if (p1Selector == null || p2Selector == null)
            return;

        p1Selector.SetOptions(new List<string>(availableProfileNames));
        p2Selector.SetOptions(new List<string>(availableProfileNames));

        if (availableProfileIndices.Count == 0)
            return;

        int selectedP1RealIndex = ProfileManager.I != null ? ProfileManager.I.GetSelectedIndex(1) : -1;
        int selectedP2RealIndex = ProfileManager.I != null ? ProfileManager.I.GetSelectedIndex(2) : -1;

        int selectedP1LocalIndex = FindLocalIndexByRealIndex(selectedP1RealIndex);
        int selectedP2LocalIndex = FindLocalIndexByRealIndex(selectedP2RealIndex);

        if (selectedP1LocalIndex < 0) selectedP1LocalIndex = 0;
        if (selectedP2LocalIndex < 0) selectedP2LocalIndex = 0;

        p1Selector.SetIndexWithoutNotify(selectedP1LocalIndex);
        p2Selector.SetIndexWithoutNotify(selectedP2LocalIndex);
    }

    private void LoadAvailableProfiles()
    {
        availableProfileIndices.Clear();
        availableProfileNames.Clear();

        if (ProfileManager.I == null)
            return;

        ProfilesDatabase db = ProfileManager.I.GetDatabase();
        if (db == null)
            return;

        for (int i = 0; i < maxProfileSlotsToScan; i++)
        {
            ProfileData p = db.GetAt(i);

            if (p == null)
                continue;

            if (string.IsNullOrWhiteSpace(p.profileName))
                continue;

            availableProfileIndices.Add(i);
            availableProfileNames.Add(p.profileName);
        }
    }

    private int FindLocalIndexByRealIndex(int realIndex)
    {
        for (int i = 0; i < availableProfileIndices.Count; i++)
        {
            if (availableProfileIndices[i] == realIndex)
                return i;
        }

        return -1;
    }

    public void OpenPanel()
    {
        PrepareSelectors();

        if (changeProfilesPanel != null)
            changeProfilesPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        if (changeProfilesPanel != null)
            changeProfilesPanel.SetActive(false);
    }

    public void ApplySelection()
    {
        if (ProfileManager.I == null)
            return;

        if (availableProfileIndices.Count == 0)
            return;

        int p1LocalIndex = p1Selector != null ? p1Selector.GetCurrentIndex() : -1;
        int p2LocalIndex = p2Selector != null ? p2Selector.GetCurrentIndex() : -1;

        if (p1LocalIndex < 0 || p1LocalIndex >= availableProfileIndices.Count)
            return;

        if (p2LocalIndex < 0 || p2LocalIndex >= availableProfileIndices.Count)
            return;

        int p1RealIndex = availableProfileIndices[p1LocalIndex];
        int p2RealIndex = availableProfileIndices[p2LocalIndex];

        ProfileManager.I.SelectProfile(1, p1RealIndex);
        ProfileManager.I.SelectProfile(2, p2RealIndex);

        Debug.Log($"VictoryProfileSelection applied: P1 slot={p1RealIndex}, P2 slot={p2RealIndex}");

        ClosePanel();
    }
}