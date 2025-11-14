using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.IO;

[System.Serializable]
public class Abilities
{
    public string passive;
    public string quick_attack;
    public string spell;
}

[System.Serializable]
public class GameCharacter
{
    public string name;
    public string lore; // To pass on the characterLore script
    public Abilities abilities;
}

[System.Serializable]
public class CharacterList
{
    public List<GameCharacter> characters;
}

public class characterInfoChoice : MonoBehaviour
{
    public TextMeshProUGUI info;
    public TextMeshProUGUI title;
    private string parentName;
    private CharacterList characterList;
    private CharacterLore characterLoreScript; // Reference to the CharacterLore script

    // Start is called before the first frame update
    void Start()
    {
        LoadCharacterData();
    }

    void LoadCharacterData()
    {
        // Load the JSON file from the Resources folder
        TextAsset jsonFile = Resources.Load<TextAsset>("characterData");
        if (jsonFile != null)
        {
            // Deserialize the JSON data into the CharacterList object
            characterList = JsonUtility.FromJson<CharacterList>(jsonFile.text);

            // Check if characterList is null
            if (characterList == null || characterList.characters == null)
            {
                Debug.LogError("Error: characterList is null after loading JSON!");
            }
            else
            {
                // Debugging: Print character names
                foreach (var character in characterList.characters)
                {
                    Debug.Log("Character Loaded: " + character.name);
                }
            }
        }
        else
        {
            Debug.LogError("Error: characterData.json not found in Resources!");
        }
    }

    // Get lore text for the specified character name
    public string GetCharacterLore(string characterName)
    {
        GameCharacter character = characterList?.characters.Find(c => c.name == characterName);
        if (character != null)
        {
            return character.lore; // Return the lore for the character
        }
        return "No lore found.";
    }

    public void InfoProceedure()
    {
        // Get the parent object name
        GameObject parentObject = transform.parent.gameObject;
        parentName = parentObject.name;

        // Debugging: Log the parent name
        Debug.Log("Parent Name: " + parentName);

        // Check if title and info are assigned
        if (title == null || info == null)
        {
            Debug.LogError("Error: title or info is not assigned in the Inspector!");
            return; // Exit the method early
        }

        // Set the title text to the character's name
        title.text = parentName;

        // Find the character in the list based on the parent name
        GameCharacter character = characterList?.characters.Find(c => c.name == parentName);

        if (character != null)
        {
            // Build the info string using the character's abilities
            string characterInfo = $"~PASSIVE~\n{character.abilities.passive}\n";

            if (!string.IsNullOrEmpty(character.abilities.quick_attack))
            {
                characterInfo += $"~QUICK ATTACK~\n{character.abilities.quick_attack}\n";
            }

            if (!string.IsNullOrEmpty(character.abilities.spell))
            {
                characterInfo += $"~SPELL~\n{character.abilities.spell}\n";
            }

            // Set the info text
            info.text = characterInfo;

            // Find the CharacterLore script and pass the lore
            characterLoreScript = FindObjectOfType<CharacterLore>();
            if (characterLoreScript != null)
            {
                characterLoreScript.SetLore(character.lore); // Pass the lore to CharacterLore
            }
            else
            {
                Debug.LogError("CharacterLore script not found in the scene!");
            }

            // Debugging: Log the character info
            Debug.Log("Character Info: " + characterInfo);
        }
        else
        {
            // Display an error if the character is not found
            info.text = "Error: Character info not found.";
            Debug.LogError("Error: Character info not found for " + parentName);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // You can add update logic here if needed
    }
}