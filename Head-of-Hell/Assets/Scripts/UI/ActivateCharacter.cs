using UnityEngine;

public class CharacterPositionManager : MonoBehaviour
{
    public string characterName;            // Name of the character to move
    public GameObject characterInfoScreen;  // Reference to the Character Info screen (set in the Inspector)
    public GameObject chooseYourFighterScreen; // Reference to the Choose Your Fighter screen (set in the Inspector)
    
    private GameObject character;           // Reference to the chosen character GameObject
    private Vector3 originalPosition;       // Stores the original position of the character
    public Vector3 infoScreenPosition;      // The position to move the character to in the Character Info screen

    void Start()
    {
        // Find the character by name in the hierarchy
        character = GameObject.Find(characterName);

        if (character != null)
        {
            // Store the original position of the character
            originalPosition = character.transform.position;

            // Initially hide the character
            character.SetActive(false);
        }
        else
        {
            Debug.LogError("Character with name " + characterName + " not found!");
        }
    }

    // Call this when the info button is clicked
    public void OnInfoButtonClicked()
    {
        if (character != null)
        {
            // Activate the character and move it to the Character Info screen
            character.SetActive(true);
            character.transform.position = infoScreenPosition;

            // Switch screens: Deactivate "Choose Your Fighter" and activate "Character Info"
            chooseYourFighterScreen.SetActive(false);
            characterInfoScreen.SetActive(true);
        }
    }

    // Call this when the back button is clicked
    public void OnBackButtonClicked()
    {
        if (character != null)
        {
            // Return the character to its original position
            character.transform.position = originalPosition;

            // Hide the character again
            character.SetActive(false);

            // Switch screens: Activate "Choose Your Fighter" and deactivate "Character Info"
            chooseYourFighterScreen.SetActive(true);
            characterInfoScreen.SetActive(false);
        }
    }
}
