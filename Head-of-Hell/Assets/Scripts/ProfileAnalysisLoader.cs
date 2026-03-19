using System.IO;
using UnityEngine;

public class ProfileAnalysisLoader : MonoBehaviour
{
    public ProfileAnalysisCollection Data { get; private set; }

    void Awake()
    {
        Load();
    }

    public void Load()
    {
        string persistentPath = Path.Combine(
            Application.persistentDataPath,
            "ProfileAnalysis",
            "profile_analysis.json"
        );

        string streamingPath = Path.Combine(
            Application.streamingAssetsPath,
            "ProfileAnalysis",
            "profile_analysis.json"
        );

        string path = File.Exists(persistentPath) ? persistentPath : streamingPath;

        if (!File.Exists(path))
        {
            return;
        }

        string json = File.ReadAllText(path);

        Data = JsonUtility.FromJson<ProfileAnalysisCollection>(json);

        if (Data == null || Data.profiles == null)
        {
            return;
        }
    }
}