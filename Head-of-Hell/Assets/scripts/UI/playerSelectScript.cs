using UnityEngine;
using UnityEngine.UI;

public class playerSelectScript : MonoBehaviour
{
    public Button[] characterButtons;
    private int currentPlayer = 1;

    void Start()
    {
        foreach (Button button in characterButtons)
        {
            button.onClick.AddListener(() => OnCharacterButtonClicked(button));
        }
    }

    void OnCharacterButtonClicked(Button button)
    {
        // Get the character index or identifier from the button
        int characterIndex = System.Array.IndexOf(characterButtons, button);

        // Assign the character to the current player
        if (currentPlayer == 1)
        {
            Debug.Log("Player 1 chose character " + characterIndex);
            // TODO: Assign character to Player 1
            // TODO: Update UI to indicate Player 1's choice
        }
        else if (currentPlayer == 2)
        {
            Debug.Log("Player 2 chose character " + characterIndex);
            // TODO: Assign character to Player 2
            // TODO: Update UI to indicate Player 2's choice
        }

        // Disable the button after selection
        button.interactable = false;

        // Switch to the next player
        currentPlayer = currentPlayer == 1 ? 2 : 1;
    }
}
