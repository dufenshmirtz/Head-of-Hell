using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterButtonManager : MonoBehaviour
{
    public GameObject lastClickedInfoButton;  // This will store the last clicked info button

    // Call this when an "info" button is clicked
    public void OnCharacterInfoButtonClicked(GameObject infoButton)
    {
        // Store the clicked button as the last clicked one
        lastClickedInfoButton = infoButton;

        // Activate the clicked info panel (in case it's not active)
        lastClickedInfoButton.SetActive(true);
    }

    // Call this when the back button is clicked
    public void OnBackButtonClicked()
    {
        if (lastClickedInfoButton != null)
        {
            // Deactivate the last clicked info button
            lastClickedInfoButton.SetActive(false);
        }
    }
}
