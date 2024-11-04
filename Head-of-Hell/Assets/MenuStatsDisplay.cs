using UnityEngine;
using TMPro;

public class MenuStatsDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text statsDisplayText; // Reference to TextMeshPro component

    void Start()
    {
        UpdateStatsDisplay();
    }

    private void UpdateStatsDisplay()
    {
        if (statsDisplayText == null)
        {
            Debug.LogError("StatsDisplayText is not assigned in the inspector.");
            return;
        }

        // Access the character stats from CharacterStatsManager
        string statsText = "Character Win Ratios:\n";
        foreach (var character in CharacterStatsManager.Instance.GetCharacterStats())
        {
            float winRatio = character.Value.GetWinRatio();
            statsText += $"\n{character.Key}: {winRatio:P2} (Wins: {character.Value.wins}, Games: {character.Value.totalGames})\n";
        }

        statsDisplayText.text = statsText;
    }
}
