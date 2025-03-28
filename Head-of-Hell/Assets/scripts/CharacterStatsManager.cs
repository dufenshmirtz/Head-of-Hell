using System.Collections.Generic;
using UnityEngine;

public class CharacterStatsManager : MonoBehaviour
{
    public static CharacterStatsManager Instance { get; private set; }

    private Dictionary<string, CharacterStats> characterStatsDict = new Dictionary<string, CharacterStats>();

    private List<string> characterNames = new List<string>
    {
        "Fin", "Skipler", "Lithra", "Lazy Bigus", "Rager", "Vander", "Chiback", "Steelager","Lupen","Visvia"
    };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }

        LoadCharacterStats();
    }

    public void KeepStats(string winner, string loser)
    {
        if (characterStatsDict.ContainsKey(winner))
        {
            characterStatsDict[winner].wins++;
            characterStatsDict[winner].totalGames++;
        }
        else
        {
            characterStatsDict[winner] = new CharacterStats { characterName = winner, wins = 1, totalGames = 1 };
        }

        if (characterStatsDict.ContainsKey(loser))
        {
            characterStatsDict[loser].totalGames++;
        }
        else
        {
            characterStatsDict[loser] = new CharacterStats { characterName = loser, wins = 0, totalGames = 1 };
        }

        SaveCharacterStats();
    }

    public Dictionary<string, CharacterStats> GetCharacterStats()
    {
        return characterStatsDict;
    }

    private void SaveCharacterStats()
    {
        foreach (var character in characterStatsDict)
        {
            string json = JsonUtility.ToJson(character.Value);
            PlayerPrefs.SetString(character.Key, json);
        }
        PlayerPrefs.Save();
    }

    private void LoadCharacterStats()
    {
        characterStatsDict.Clear();
        foreach (string name in characterNames)
        {
            if (PlayerPrefs.HasKey(name))
            {
                string json = PlayerPrefs.GetString(name);
                CharacterStats stats = JsonUtility.FromJson<CharacterStats>(json);
                characterStatsDict[name] = stats;
            }
            else
            {
                characterStatsDict[name] = new CharacterStats { characterName = name, wins = 0, totalGames = 0 };
            }
        }
    }
}

[System.Serializable]
public class CharacterStats
{
    public string characterName;
    public int wins;
    public int totalGames;

    public float GetWinRatio()
    {
        return totalGames > 0 ? (float)wins / totalGames : 0f;
    }
}
