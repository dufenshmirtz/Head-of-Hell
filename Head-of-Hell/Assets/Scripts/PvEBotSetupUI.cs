using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PvEBotSetupUI : MonoBehaviour
{
    [Header("Scene Navigation")]
    public string nextSceneName = "GameSetup";
    public string backSceneName = "MainMenu";

    [Header("Bot Type Buttons")]
    public Button mlAgentButton;
    public Button scriptedBotButton;

    [Header("Difficulty Buttons")]
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;

    [Header("Labels")]
    public TMP_Text selectedBotText;
    public TMP_Text selectedDifficultyText;

    private void Start()
    {
        // default μόνο αν δεν μπήκαμε ήδη σαν PvE
        if (!PvESelectionState.IsPvE)
            PvESelectionState.IsPvE = true;

        RefreshUI();
    }

    public void SelectMLAgent()
    {
        PvESelectionState.SelectedBotType = PvEBotType.MLAgent;
        RefreshUI();
    }

    public void SelectScriptedBot()
    {
        PvESelectionState.SelectedBotType = PvEBotType.ScriptedBot;
        RefreshUI();
    }

    public void SelectEasy()
    {
        PvESelectionState.SelectedDifficulty = PvEDifficulty.Easy;
        RefreshUI();
    }

    public void SelectMedium()
    {
        PvESelectionState.SelectedDifficulty = PvEDifficulty.Medium;
        RefreshUI();
    }

    public void SelectHard()
    {
        PvESelectionState.SelectedDifficulty = PvEDifficulty.Hard;
        RefreshUI();
    }

    public void ContinueToGameSetup()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    public void BackToMainMenu()
    {
        PvESelectionState.ResetToDefaults();
        SceneManager.LoadScene(backSceneName);
    }

    private void RefreshUI()
    {
        if (selectedBotText != null)
            selectedBotText.text = "Bot: " + GetBotTypeLabel(PvESelectionState.SelectedBotType);

        if (selectedDifficultyText != null)
            selectedDifficultyText.text = "Difficulty: " + GetDifficultyLabel(PvESelectionState.SelectedDifficulty);

        SetButtonVisual(mlAgentButton, PvESelectionState.SelectedBotType == PvEBotType.MLAgent);
        SetButtonVisual(scriptedBotButton, PvESelectionState.SelectedBotType == PvEBotType.ScriptedBot);

        SetButtonVisual(easyButton, PvESelectionState.SelectedDifficulty == PvEDifficulty.Easy);
        SetButtonVisual(mediumButton, PvESelectionState.SelectedDifficulty == PvEDifficulty.Medium);
        SetButtonVisual(hardButton, PvESelectionState.SelectedDifficulty == PvEDifficulty.Hard);
    }

    private string GetBotTypeLabel(PvEBotType type)
    {
        switch (type)
        {
            case PvEBotType.MLAgent: return "ML Agent";
            case PvEBotType.ScriptedBot: return "Scripted Bot";
            default: return "Unknown";
        }
    }

    private string GetDifficultyLabel(PvEDifficulty difficulty)
    {
        switch (difficulty)
        {
            case PvEDifficulty.Easy: return "Easy";
            case PvEDifficulty.Medium: return "Medium";
            case PvEDifficulty.Hard: return "Hard";
            default: return "Unknown";
        }
    }

    private void SetButtonVisual(Button button, bool selected)
    {
        if (button == null) return;

        ColorBlock colors = button.colors;
        colors.normalColor = selected ? new Color(0.75f, 0.75f, 0.75f, 1f) : Color.white;
        button.colors = colors;
    }
}