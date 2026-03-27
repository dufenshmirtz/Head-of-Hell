using UnityEngine;
using UnityEngine.UI;

public class DeleteConfirmUI : MonoBehaviour
{
    public GameObject panel;
    public Button yesButton;
    public Button noButton;

    public ProfilesMenuUI menuUI;
    public ProfileAnalysisPanelUI analysisUI;

    // NEW
    public GameObject profilesMenuRoot;

    private int pendingIndex = -1;

    public void Open(int index)
    {
        pendingIndex = index;

        if (profilesMenuRoot != null)
            profilesMenuRoot.SetActive(false);

        if (panel != null)
            panel.SetActive(true);
    }

    public void Close(bool reopenProfilesMenu = true)
    {
        if (panel != null)
            panel.SetActive(false);

        if (reopenProfilesMenu && profilesMenuRoot != null)
            profilesMenuRoot.SetActive(true);

        pendingIndex = -1;
    }

    private void Start()
    {
        if (yesButton != null)
        {
            yesButton.onClick.AddListener(() =>
            {
                if (pendingIndex >= 0)
                {
                    Debug.Log($"CONFIRM DELETE slot {pendingIndex}");

                    ProfileManager.I.DeleteProfileAt(pendingIndex);

                    if (menuUI != null)
                        menuUI.Refresh();

                    if (analysisUI != null)
                        analysisUI.ClearProfileView();
                }

                // μετά το delete άνοιξε πάλι το profiles menu
                Close(true);
            });
        }

        if (noButton != null)
        {
            noButton.onClick.AddListener(() =>
            {
                // ακύρωση -> ξαναάνοιξε το profiles menu
                Close(true);
            });
        }
    }
}