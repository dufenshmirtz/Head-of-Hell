using System.IO;
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

        filePath = Path.Combine(Application.persistentDataPath, "ml_data.csv");

        print("ass: "+Application.persistentDataPath);

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