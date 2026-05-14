using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomCharactersMenuUI : MonoBehaviour
{
    [System.Serializable]
    public class SlotUI
    {
        public Button slotButton;
        public TMP_Text slotText;
        public Button editButton;
        public Button deleteButton;
    }

    [Header("Slots")]
    public SlotUI[] slots = new SlotUI[5];

    [Header("Panels")]
    public GameObject customCharactersMenuRoot;
    public GameObject customCharacterEditorRoot;
    public CustomCharacterEditorUI editorUI;

    private void OnEnable()
    {
        StartCoroutine(InitNextFrame());
    }

    private IEnumerator InitNextFrame()
    {
        yield return null;
        Refresh();
        WireButtons();
    }

    public void Refresh()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            CustomCharacterData data = CustomCharacterStore.GetAt(i);
            bool hasCharacter = data != null;

            if (slots[i].slotText != null)
            {
                slots[i].slotText.text = hasCharacter ? data.displayName : "";
            }

            if (slots[i].deleteButton != null)
            {
                slots[i].deleteButton.gameObject.SetActive(hasCharacter);
            }
        }
    }

    private void WireButtons()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int index = i;

            if (slots[index].slotButton != null)
            {
                slots[index].slotButton.onClick.RemoveAllListeners();
                slots[index].slotButton.onClick.AddListener(() => OpenEditor(index));
            }

            if (slots[index].editButton != null)
            {
                slots[index].editButton.onClick.RemoveAllListeners();
                slots[index].editButton.onClick.AddListener(() => OpenEditor(index));
            }

            if (slots[index].deleteButton != null)
            {
                slots[index].deleteButton.onClick.RemoveAllListeners();
                slots[index].deleteButton.onClick.AddListener(() =>
                {
                    CustomCharacterStore.DeleteAt(index);
                    Refresh();
                });
            }
        }
    }

    private void OpenEditor(int index)
    {
        CustomCharacterEditContext.EditingIndex = index;

        if (editorUI != null)
        {
            editorUI.Open(index);
        }

        if (customCharactersMenuRoot != null)
        {
            customCharactersMenuRoot.SetActive(false);
        }

        if (customCharacterEditorRoot != null)
        {
            customCharacterEditorRoot.SetActive(true);
        }
    }
}
