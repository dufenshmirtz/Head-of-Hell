using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfilesMenuUI : MonoBehaviour
{
    [System.Serializable]
    public class SlotUI
    {
        public Button slotButton;     // το κουμπί που πατάς για select/view
        public TMP_Text slotText;     // το κείμενο "Empty" ή "Profile X"
        public Button editButton;     // το edit κουμπί
    }

    [Header("Slots (size=5)")]
    public SlotUI[] slots = new SlotUI[5];

    [Header("Panels")]
    public GameObject profilesMenuRoot;   // το panel της λίστας
    public GameObject profileEditorRoot;  // το panel του editor (Page1)
    public int playerNum = 1; // 1 = P1, 2 = P2

    private void OnEnable()
    {
        StartCoroutine(InitNextFrame());
    }

    private System.Collections.IEnumerator InitNextFrame()
    {
        yield return null; // περιμένει 1 frame

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

            // καθάρισμα παλιών listeners (για να μη διπλομπαίνουν)
            slots[index].slotButton.onClick.RemoveAllListeners();
            slots[index].editButton.onClick.RemoveAllListeners();

            // Select/View
            slots[index].slotButton.onClick.AddListener(() =>
            {
                ProfileManager.I.SelectProfile(playerNum,index);
                Refresh();
            });

            // Edit -> θα ανοίξει editor στο Βήμα 3
            slots[index].editButton.onClick.AddListener(() =>
            {
                ProfileEditContext.EditingIndex = index;
            });
        }
    }

    private void OpenEditor(int index)
    {
        ProfileEditContext.EditingIndex = index;
        // Για τώρα: απλά αλλάζουμε panel
        // στο Βήμα 3 θα περάσουμε το index στον Editor UI.
        profilesMenuRoot.SetActive(false);
        profileEditorRoot.SetActive(true);

        // προσωρινά: αποθήκευσε ποιο slot κάνεις edit μέσω selectedIndex
        // (εύκολο hack μέχρι να φτιάξουμε σωστό Editor script)
        // Αν είναι empty, το SelectProfile δεν θα κάνει τίποτα, οπότε:
        // Θα κρατήσουμε "selectedIndex" αλλιώς. Θα το λύσουμε στο Βήμα 3.
    }
}