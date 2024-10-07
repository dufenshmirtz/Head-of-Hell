using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageChoiceButtons : MonoBehaviour
{
    public Button[] stageButtons;  // Buttons representing stages

    public TextMeshProUGUI stageNameText;  // Text to display the selected stage name

    public Button startButton;  // Button to confirm the stage selection

    private bool stagePicked = false;

    public MainMenuMusic audiomanager;

    void Start()
    {
        stagePicked = false;
        foreach (Button button in stageButtons)
        {
            button.onClick.AddListener(() => OnStageButtonClicked(button));
        }
        startButton.gameObject.SetActive(false);  // Hide the start button initially
    }

    public void OnStageButtonClicked(Button button)
    {
        if (stagePicked)
        {
            return;
        }

        // Get the stage index or identifier from the button
        int stageIndex = System.Array.IndexOf(stageButtons, button);

        // Update the UI to show the selected stage
        stageNameText.text = button.name;

        // Store the selected stage name for later use (e.g., in PlayerPrefs)
        PlayerPrefs.SetString("SelectedStage", button.name);

        // Disable other buttons to prevent changing the selection
        foreach (Button stageButton in stageButtons)
        {
            if (stageButton != button)
            {
                stageButton.interactable = false;
            }
        }

        // Play sound associated with the selected stage
        PlayStageSound(button.name);

        stagePicked = true;

        // Show the start button once a stage is selected
        startButton.gameObject.SetActive(true);
    }

    public void ResetStageSelection()
    {
        // Reactivate all stage buttons
        foreach (Button button in stageButtons)
        {
            button.interactable = true;
        }

        // Reset stage selection
        stagePicked = false;
        stageNameText.text = "";

        // Hide the start button
        startButton.gameObject.SetActive(false);
    }

    public void HoveringIn(string name)
    {
        if (stagePicked)
        {
            return;
        }

        stageNameText.text = name;
    }

    public void HoveringOut()
    {
        if (stagePicked)
        {
            return;
        }

        stageNameText.text = "";
    }

    void PlayStageSound(string name)
    {
        audiomanager.PlaySFX(audiomanager.stageSound, 1f);
    }
}
