using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileAnalysisPanelUI : MonoBehaviour
{
    [Header("Roots")]
    public GameObject profilesMenuRoot;
    public GameObject profileAnalysisRoot;

    [Header("Data")]
    public ProfileAnalysisLoader loader;

    [Header("Texts")]
    public TMP_Text profileNameText;
    public TMP_Text styleLabelText;
    public TMP_Text matchesText;
    public TMP_Text winRateText;
    public TMP_Text hitRateText;
    public TMP_Text missRateText;
    public TMP_Text avgDamageDealtText;
    public TMP_Text avgDamageTakenText;
    public TMP_Text eloText;
    private string currentProfileName;
    private string currentProfileId;

    [Header("Chart")]
    public CombatSignatureChart combatChart;

    [Header("Buttons")]
    public Button backButton;

    private void Awake()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(ClosePanel);
        }
    }

    public void OpenForProfileName(string profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName))
        {
            Debug.LogWarning("ProfileAnalysisPanelUI: empty profile name.");
            return;
        }

        if (profileName == "Empty")
        {
            Debug.Log("ProfileAnalysisPanelUI: slot is empty, not opening analysis.");
            return;
        }

        if (loader == null || loader.Data == null || loader.Data.profiles == null)
        {
            Debug.LogError("ProfileAnalysisPanelUI: loader/data is null.");
            return;
        }

        var profile = loader.Data.profiles
            .FirstOrDefault(p => p.profile_name == profileName);

        if (profile == null)
        {
            Debug.LogWarning($"ProfileAnalysisPanelUI: profile '{profileName}' not found in JSON.");
            return;
        }

        ShowProfile(profile);

        if (profilesMenuRoot != null) profilesMenuRoot.SetActive(false);
        if (profileAnalysisRoot != null) profileAnalysisRoot.SetActive(true);
    }

    public void ClosePanel()
    {
        if (profileAnalysisRoot != null) profileAnalysisRoot.SetActive(false);
        if (profilesMenuRoot != null) profilesMenuRoot.SetActive(true);
    }

    public void RefreshAnalysis()
    {
        if (loader == null)
        {
            Debug.LogError("RefreshAnalysis: loader is NULL");
            return;
        }

        string profileIdToRestore = currentProfileId;
        string profileNameToRestore = currentProfileName;

        loader.Load();

        if (loader.Data == null || loader.Data.profiles == null || loader.Data.profiles.Count == 0)
        {
            Debug.LogWarning("RefreshAnalysis: no profiles found after reload");
            return;
        }

        ProfileAnalysisEntry foundProfile = null;

        if (!string.IsNullOrWhiteSpace(profileIdToRestore))
        {
            foundProfile = loader.Data.profiles
                .FirstOrDefault(p => p.profile_id == profileIdToRestore);
        }

        if (foundProfile == null && !string.IsNullOrWhiteSpace(profileNameToRestore))
        {
            foundProfile = loader.Data.profiles
                .FirstOrDefault(p => p.profile_name == profileNameToRestore);
        }

        if (foundProfile != null)
        {
            ShowProfile(foundProfile);
        }
        else
        {
            Debug.LogWarning($"RefreshAnalysis: could not restore profile id='{profileIdToRestore}' name='{profileNameToRestore}', showing first");
            ShowProfile(loader.Data.profiles[0]);
        }
    }
    private void ShowProfile(ProfileAnalysisEntry p)
    {
        currentProfileId = p.profile_id;
        currentProfileName = p.profile_name;
        Debug.Log("ShowProfile -> " + p.profile_name);
        Debug.Log("Style = " + p.style_label);
        Debug.Log("Elo = " + p.elo_rating);
        Debug.Log("WinRate = " + p.win_rate);
        Debug.Log("HitRate = " + p.hit_rate);
        Debug.Log("MissRate = " + p.miss_rate);
        Debug.Log("AvgDamageDealt = " + p.avg_damage_dealt);
        Debug.Log("AvgDamageTaken = " + p.avg_damage_taken);
        Debug.Log("AggressionRaw = " + p.aggression_raw);
        Debug.Log("MobilityRaw = " + p.mobility_raw);
        Debug.Log("DefenseRaw = " + p.defense_raw);
        Debug.Log("RiskRaw = " + p.risk_raw);
        
        if (profileNameText != null) profileNameText.text = p.profile_name;
        if (styleLabelText != null) styleLabelText.text = p.style_label;
        if (eloText != null)
            eloText.text = Mathf.RoundToInt(p.elo_rating).ToString();
        if (matchesText != null) matchesText.text = $"Matches: {p.matches_count}";
        if (winRateText != null) winRateText.text = $"Win Rate: {p.win_rate:P0}";
        if (hitRateText != null) hitRateText.text = $"Hit Rate: {p.hit_rate:P0}";
        if (missRateText != null) missRateText.text = $"Miss Rate: {p.miss_rate:P0}";
        if (avgDamageDealtText != null) avgDamageDealtText.text = $"Avg Damage Dealt: {p.avg_damage_dealt:F1}";
        if (avgDamageTakenText != null) avgDamageTakenText.text = $"Avg Damage Taken: {p.avg_damage_taken:F1}";
        if (combatChart != null)
        {
            float maxVal = Mathf.Max(
                p.aggression_raw,
                p.mobility_raw,
                p.defense_raw,
                p.risk_raw
            );

            if (maxVal <= 0f) maxVal = 1f;

            combatChart.SetValues(
                p.aggression_raw / maxVal,
                p.mobility_raw / maxVal,
                p.defense_raw / maxVal,
                p.risk_raw / maxVal
            );
        }
    }
}