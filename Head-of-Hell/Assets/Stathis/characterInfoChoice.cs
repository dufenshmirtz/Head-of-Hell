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

    // Start is called before the first frame update
    void Start()
    {
        LoadCharacterData();
    }

    void LoadCharacterData()
    {
        string path = Path.Combine(Application.dataPath, "Stathis/characterData.json");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            characterList = JsonUtility.FromJson<CharacterList>(json);

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
            Debug.LogError("Error: characterData.json file not found in Stathis folder!");
        }
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
       
    }
}
