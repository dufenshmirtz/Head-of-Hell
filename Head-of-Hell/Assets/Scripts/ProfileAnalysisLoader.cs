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
        string path = Path.Combine(
            Application.streamingAssetsPath,
            "ProfileAnalysis",
            "profile_analysis.json"
        );
       
        if (!File.Exists(path))
        {
            Debug.LogError("Profile analysis JSON not found: " + path);
            return;
        }

        string json = File.ReadAllText(path);

        Data = JsonUtility.FromJson<ProfileAnalysisCollection>(json);

        if (Data == null || Data.profiles == null)
        {
            Debug.LogError("Failed to parse profile_analysis.json");
            return;
        }

        Debug.Log($"Loaded {Data.profiles.Count} profiles");

        foreach (var p in Data.profiles)
        {
            Debug.Log($"Profile: {p.profile_name} | Style: {p.style_label}");
        }
    }
}