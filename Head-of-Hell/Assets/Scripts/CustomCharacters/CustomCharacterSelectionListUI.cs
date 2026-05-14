using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomCharacterSelectionListUI : MonoBehaviour
{
    public CharacterChoiceMenu characterChoiceMenu;
    public Transform listRoot;
    public Button buttonPrefab;
    public TMP_Text emptyStateText;

    private readonly List<Button> spawnedButtons = new List<Button>();

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        ClearSpawnedButtons();

        CustomCharacterDatabase database = CustomCharacterStore.GetDatabase();
        bool hasAny = false;

        for (int i = 0; i < database.characters.Count; i++)
        {
            CustomCharacterData data = database.characters[i];
            if (data == null)
            {
                continue;
            }

            hasAny = true;
            SpawnButton(data);
        }

        if (emptyStateText != null)
        {
            emptyStateText.gameObject.SetActive(!hasAny);
        }
    }

    private void SpawnButton(CustomCharacterData data)
    {
        if (buttonPrefab == null || listRoot == null)
        {
            return;
        }

        Button button = Instantiate(buttonPrefab, listRoot);
        button.gameObject.SetActive(true);
        spawnedButtons.Add(button);

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.text = data.displayName;
        }

        string id = data.id;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            if (characterChoiceMenu != null)
            {
                characterChoiceMenu.AssignCustomCharacterToCurrentPlayer(id);
            }
        });
    }

    private void ClearSpawnedButtons()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (spawnedButtons[i] != null)
            {
                Destroy(spawnedButtons[i].gameObject);
            }
        }

        spawnedButtons.Clear();
    }
}
