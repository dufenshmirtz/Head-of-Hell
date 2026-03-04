using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileEditorUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField nameInput;   // το input που γράφεις Name
    public Button saveButton;
    public Button backButton;

    [Header("Panels")]
    public GameObject profilesMenuRoot;
    public GameObject profileEditorRoot;

    private void OnEnable()
    {
        saveButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();

        saveButton.onClick.AddListener(Save);
        backButton.onClick.AddListener(Back);

        StartCoroutine(LoadNextFrame());
    }

    private System.Collections.IEnumerator LoadNextFrame()
    {
        yield return null; // αφήνει να ολοκληρωθούν όλα τα OnClick/SetActive πρώτα

        int idx = ProfileEditContext.EditingIndex;
        var p = ProfileManager.I.GetDatabase().GetAt(idx);

        nameInput.text = (p == null) ? "" : p.profileName;
        nameInput.Select();
        nameInput.ActivateInputField();
    }

    private void Save()
    {
        int idx = ProfileEditContext.EditingIndex;
        if (idx < 0) { Back(); return; }

        ProfileManager.I.CreateOrRenameProfileAt(idx, nameInput.text);

    }

    private void Back()
    {
        profileEditorRoot.SetActive(false);
        profilesMenuRoot.SetActive(true);

        // refresh τη λίστα, αν υπάρχει ProfilesMenuUI πάνω στο menu root
        var menu = profilesMenuRoot.GetComponent<ProfilesMenuUI>();
        if (menu != null) menu.Refresh();
    }
}