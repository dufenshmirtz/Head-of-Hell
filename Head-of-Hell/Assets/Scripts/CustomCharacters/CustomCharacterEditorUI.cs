using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomCharacterEditorUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField nameInput;
    public TMP_Dropdown lightDropdown;
    public TMP_Dropdown specialDropdown;
    public TMP_Dropdown passiveDropdown;
    public TMP_Text statusText;
    public Button saveButton;
    public Button backButton;

    [Header("Panels")]
    public GameObject customCharactersMenuRoot;
    public GameObject customCharacterEditorRoot;

    private readonly List<string> lightIds = new List<string>();
    private readonly List<string> specialIds = new List<string>();
    private readonly List<string> passiveIds = new List<string>();

    private void Awake()
    {
        if (saveButton != null)
        {
            saveButton.onClick.RemoveListener(Save);
            saveButton.onClick.AddListener(Save);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(Back);
            backButton.onClick.AddListener(Back);
        }
    }

    private void OnEnable()
    {
        StartCoroutine(LoadNextFrame());
    }

    private IEnumerator LoadNextFrame()
    {
        yield return null;
        LoadFromCurrentIndex();
    }

    public void Open(int index)
    {
        CustomCharacterEditContext.EditingIndex = index;
        LoadFromCurrentIndex();
    }

    public void Save()
    {
        int index = CustomCharacterEditContext.EditingIndex;
        if (index < 0)
        {
            Back();
            return;
        }

        CustomCharacterData data = CustomCharacterStore.GetAt(index) ?? CustomCharacterData.CreateDefault();
        data.displayName = nameInput != null && !string.IsNullOrWhiteSpace(nameInput.text)
            ? nameInput.text.Trim()
            : "Custom Fighter";
        data.lightAbilityId = GetSelectedId(lightIds, lightDropdown, CustomCharacterAbilityIds.LightBasicQuick);
        data.specialAbilityId = GetSelectedId(specialIds, specialDropdown, CustomCharacterAbilityIds.SpecialBasicBurst);
        data.passiveId = GetSelectedId(passiveIds, passiveDropdown, CustomCharacterAbilityIds.PassiveNone);

        CustomCharacterStore.SetAt(index, data);

        if (statusText != null)
        {
            statusText.text = "Saved";
        }
    }

    public void Back()
    {
        CustomCharactersMenuUI menu = customCharactersMenuRoot != null
            ? customCharactersMenuRoot.GetComponent<CustomCharactersMenuUI>()
            : null;

        if (menu != null)
        {
            menu.Refresh();
        }

        if (customCharacterEditorRoot != null)
        {
            customCharacterEditorRoot.SetActive(false);
        }

        if (customCharactersMenuRoot != null)
        {
            customCharactersMenuRoot.SetActive(true);
        }
    }

    private void LoadFromCurrentIndex()
    {
        int index = CustomCharacterEditContext.EditingIndex;
        CustomCharacterData data = CustomCharacterStore.GetAt(index);

        if (data == null)
        {
            data = CustomCharacterData.CreateDefault($"Custom Fighter {index + 1}");
        }

        if (nameInput != null)
        {
            nameInput.text = data.displayName;
            nameInput.Select();
            nameInput.ActivateInputField();
        }

        PopulateDropdown(lightDropdown, CustomAbilitySlot.Light, data.lightAbilityId, lightIds);
        PopulateDropdown(specialDropdown, CustomAbilitySlot.Special, data.specialAbilityId, specialIds);
        PopulateDropdown(passiveDropdown, CustomAbilitySlot.Passive, data.passiveId, passiveIds);

        if (statusText != null)
        {
            statusText.text = "";
        }
    }

    private void PopulateDropdown(TMP_Dropdown dropdown, CustomAbilitySlot slot, string selectedId, List<string> ids)
    {
        ids.Clear();
        if (dropdown == null)
        {
            return;
        }

        dropdown.ClearOptions();
        List<string> labels = new List<string>();
        IReadOnlyList<CustomAbilityDefinition> abilities = CustomCharacterAbilityCatalog.GetAbilities(slot);
        int selectedIndex = 0;

        for (int i = 0; i < abilities.Count; i++)
        {
            CustomAbilityDefinition ability = abilities[i];
            ids.Add(ability.id);
            labels.Add(ability.displayName);

            if (ability.id == selectedId)
            {
                selectedIndex = i;
            }
        }

        dropdown.AddOptions(labels);
        dropdown.value = selectedIndex;
        dropdown.RefreshShownValue();
    }

    private string GetSelectedId(List<string> ids, TMP_Dropdown dropdown, string fallback)
    {
        if (dropdown == null || dropdown.value < 0 || dropdown.value >= ids.Count)
        {
            return fallback;
        }

        return ids[dropdown.value];
    }
}
