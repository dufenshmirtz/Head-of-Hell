using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfilesMenuUI : MonoBehaviour
{
    [System.Serializable]
    public class SlotUI
    {
        public Button slotButton;     // το κουμπί που πατάς για select/view
        public TMP_Text slotText;     // το κείμενο "Empty" ή profile name
        public Button editButton;     // το edit κουμπί
    }

    [Header("Slots (size=5)")]
    public SlotUI[] slots = new SlotUI[5];

    [Header("Panels")]
    public GameObject profilesMenuRoot;   // το panel της λίστας
    public GameObject profileEditorRoot;  // το panel του editor
    public int playerNum = 1; // 1 = P1, 2 = P2

    [Header("Analysis")]
    public ProfileAnalysisPanelUI profileAnalysisPanelUI;

    private void OnEnable()
    {
        StartCoroutine(InitNextFrame());
    }

    private System.Collections.IEnumerator InitNextFrame()
    {
        yield return null;

        if (ProfileManager.I == null) yield break;

        Refresh();
        WireButtons();
    }

    public void Refresh()
    {
        var db = ProfileManager.I.GetDatabase();

        for (int i = 0; i < slots.Length; i++)
        {
            var p = db.GetAt(i);
            slots[i].slotText.text = (p == null) ? "Empty" : p.profileName;
        }
    }

    private void WireButtons()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int index = i;

            slots[index].slotButton.onClick.RemoveAllListeners();
            slots[index].editButton.onClick.RemoveAllListeners();

            // Slot click -> open analysis screen if profile exists
            slots[index].slotButton.onClick.AddListener(() =>
            {
                var db = ProfileManager.I.GetDatabase();
                var profileData = db != null ? db.GetAt(index) : null;

                if (profileData == null || string.IsNullOrWhiteSpace(profileData.id))
                {
                    Debug.Log($"Slot {index} is empty. Clearing analysis view.");

                    if (profileAnalysisPanelUI != null)
                        profileAnalysisPanelUI.ClearProfileView();

                    return;
                }

                Debug.Log($"ProfilesMenuUI: slot {index} clicked -> id='{profileData.id}', name='{profileData.profileName}'");

                ProfileManager.I.SelectProfile(playerNum, index);
                Refresh();

                if (profileAnalysisPanelUI != null)
                {
                    profileAnalysisPanelUI.OpenForProfileId(profileData.id);
                }
                else
                {
                    Debug.LogWarning("ProfilesMenuUI: profileAnalysisPanelUI is not assigned.");
                }
            });

            // Edit button -> keep current behavior
            slots[index].editButton.onClick.AddListener(() =>
            {
                ProfileEditContext.EditingIndex = index;
                OpenEditor(index);
            });
        }
    }

    private void OpenEditor(int index)
    {
        ProfileEditContext.EditingIndex = index;

        if (profilesMenuRoot != null) profilesMenuRoot.SetActive(false);
        if (profileEditorRoot != null) profileEditorRoot.SetActive(true);
    }


}