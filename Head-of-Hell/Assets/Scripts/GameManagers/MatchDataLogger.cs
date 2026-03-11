using System.IO;
using System;
using UnityEngine;

public class MatchDataLogger : MonoBehaviour
{
    public static MatchDataLogger Instance;

    private string filePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        string folder = Path.Combine(
            documentsPath,
            "My Games",
            "Head of Hell",
            "BalanceLogs"
        );

        Directory.CreateDirectory(folder);

        filePath = Path.Combine(folder, "ml_data.csv");

        Debug.Log("Balance ML data path: " + filePath);

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath,
                "char_A,char_B,damage_A,cooldown_A,utility_A,damage_B,cooldown_B,utility_B,A_wins\n");
        }
    }

    public void LogMatchRow(
        int charA, int charB,
        float damageA, float cooldownA, float utilityA,
        float damageB, float cooldownB, float utilityB,
        int aWins)
    {
        string row = $"{charA},{charB},{damageA},{cooldownA},{utilityA},{damageB},{cooldownB},{utilityB},{aWins}\n";
        File.AppendAllText(filePath, row);
    }

    public string GetFilePath()
    {
        return filePath;
    }
}