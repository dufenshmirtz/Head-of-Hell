using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class EvaluationMatchLogger : MonoBehaviour
{
    public static EvaluationMatchLogger Instance;

    [Header("Enable")]
    [SerializeField] private bool enableLogging = true;
    [SerializeField] private bool onlyInTestMode = true;

    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CharacterManager p1Manager;
    [SerializeField] private CharacterManager p2Manager;

    [Header("Settings")]
    [Tooltip("Which side is considered the evaluated agent.")]
    [SerializeField] private int agentPlayerNum = 1;

    [Tooltip("Optional manual label for the opponent type. Leave empty to use opponentDirector if available.")]
    [SerializeField] private string manualOpponentLabel = "";

    [Tooltip("Folder inside Application.persistentDataPath")]
    [SerializeField] private string outputFolderName = "EvaluationLogs";

    private string sessionId;
    private string folderPath;
    private string matchCsvPath;
    private string byAgentCsvPath;
    private string byMatchupCsvPath;

    private int matchIndex = 0;
    private float currentMatchStartTime = 0f;
    private bool matchActive = false;

    private readonly Dictionary<int, EvalAggregate> byAgentCharacter = new Dictionary<int, EvalAggregate>();
    private readonly Dictionary<string, MatchupAggregate> byMatchup = new Dictionary<string, MatchupAggregate>();

    [Serializable]
    private class EvalAggregate
    {
        public int characterId;
        public string characterName;
        public int matches;
        public int wins;
        public int losses;
        public int ties;
        public int flawlessWins;
        public float totalHpDiff;
        public float totalDuration;

        public float WinRate => matches <= 0 ? 0f : (float)wins / matches;
        public float AvgHpDiff => matches <= 0 ? 0f : totalHpDiff / matches;
        public float AvgDuration => matches <= 0 ? 0f : totalDuration / matches;
    }

    [Serializable]
    private class MatchupAggregate
    {
        public int agentCharId;
        public string agentCharName;
        public int oppCharId;
        public string oppCharName;
        public string opponentMode;

        public int matches;
        public int wins;
        public int losses;
        public int ties;
        public int flawlessWins;

        public float totalHpDiff;
        public float totalDuration;

        public float WinRate => matches <= 0 ? 0f : (float)wins / matches;
        public float AvgHpDiff => matches <= 0 ? 0f : totalHpDiff / matches;
        public float AvgDuration => matches <= 0 ? 0f : totalDuration / matches;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
        if (p1Manager == null && gameManager != null) p1Manager = gameManager.p1Manager;
        if (p2Manager == null && gameManager != null) p2Manager = gameManager.p2Manager;

        sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        folderPath = Path.Combine(Application.persistentDataPath, outputFolderName, sessionId);
        Directory.CreateDirectory(folderPath);

        matchCsvPath = Path.Combine(folderPath, "matches.csv");
        byAgentCsvPath = Path.Combine(folderPath, "summary_by_agent_character.csv");
        byMatchupCsvPath = Path.Combine(folderPath, "summary_by_matchup.csv");

        WriteMatchCsvHeaderIfNeeded();
    }

    private bool ShouldLog()
    {
        if (!enableLogging) return false;
        if (gameManager == null) return false;
        if (onlyInTestMode && !gameManager.testMode) return false;
        return true;
    }

    public void BeginMatch()
    {
        if (!ShouldLog()) return;

        matchIndex++;
        currentMatchStartTime = Time.unscaledTime;
        matchActive = true;

        Debug.Log($"[EvaluationMatchLogger] BeginMatch #{matchIndex}");
    }

    public void RecordMatchEnd(int winnerPlayerNum, bool isTie = false, bool isFlawless = false)
    {
        if (!ShouldLog()) return;

        if (!matchActive)
        {
            BeginMatch();
        }

        Character p1 = p1Manager != null ? p1Manager.GetCurrentCharacter() : null;
        Character p2 = p2Manager != null ? p2Manager.GetCurrentCharacter() : null;

        if (p1 == null || p2 == null)
        {
            Debug.LogWarning("[EvaluationMatchLogger] Could not record match end because one or both characters are null.");
            return;
        }

        Character agentChar = agentPlayerNum == 1 ? p1 : p2;
        Character oppChar = agentPlayerNum == 1 ? p2 : p1;

        string agentName = agentPlayerNum == 1 ? p1Manager.GetCharacterName(1) : p2Manager.GetCharacterName(1);
        string oppName = agentPlayerNum == 1 ? p2Manager.GetCharacterName(1) : p1Manager.GetCharacterName(1);

        int agentHp = agentChar.GetCurrentHealth();
        int oppHp = oppChar.GetCurrentHealth();
        int hpDiff = agentHp - oppHp;

        bool agentWon = !isTie && winnerPlayerNum == agentPlayerNum;
        bool agentLost = !isTie && winnerPlayerNum != 0 && winnerPlayerNum != agentPlayerNum;

        float matchLength = Mathf.Max(0f, Time.unscaledTime - currentMatchStartTime);
        string stage = PlayerPrefs.GetString("SelectedStage", "Stage 1");
        string opponentMode = ResolveOpponentMode();
        string timestampUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

        string row =
            timestampUtc + "," +
            matchIndex + "," +
            Escape(stage) + "," +
            Escape(opponentMode) + "," +
            agentPlayerNum + "," +
            agentChar.characterID + "," +
            Escape(agentName) + "," +
            oppChar.characterID + "," +
            Escape(oppName) + "," +
            winnerPlayerNum + "," +
            Bool01(isTie) + "," +
            Bool01(isFlawless) + "," +
            Bool01(agentWon) + "," +
            agentHp + "," +
            oppHp + "," +
            hpDiff + "," +
            matchLength.ToString("F3", CultureInfo.InvariantCulture);

        File.AppendAllText(matchCsvPath, row + Environment.NewLine);

        UpdateByAgentCharacter(agentChar.characterID, agentName, agentWon, agentLost, isTie, isFlawless, hpDiff, matchLength);
        UpdateByMatchup(agentChar.characterID, agentName, oppChar.characterID, oppName, opponentMode, agentWon, agentLost, isTie, isFlawless, hpDiff, matchLength);

        ExportSummaries();

        Debug.Log(
            $"[EvaluationMatchLogger] Match #{matchIndex} | OpponentMode={opponentMode} | " +
            $"Agent={agentName}({agentChar.characterID}) vs {oppName}({oppChar.characterID}) | " +
            $"Winner={(isTie ? "Tie" : "P" + winnerPlayerNum)} | HPDiff={hpDiff} | Length={matchLength:F2}s"
        );

        matchActive = false;
    }

    private void UpdateByAgentCharacter(
        int characterId,
        string characterName,
        bool won,
        bool lost,
        bool tied,
        bool flawless,
        int hpDiff,
        float duration)
    {
        if (!byAgentCharacter.TryGetValue(characterId, out var agg))
        {
            agg = new EvalAggregate
            {
                characterId = characterId,
                characterName = characterName
            };
            byAgentCharacter.Add(characterId, agg);
        }

        agg.matches++;
        if (won) agg.wins++;
        if (lost) agg.losses++;
        if (tied) agg.ties++;
        if (won && flawless) agg.flawlessWins++;

        agg.totalHpDiff += hpDiff;
        agg.totalDuration += duration;
    }

    private void UpdateByMatchup(
        int agentCharId,
        string agentCharName,
        int oppCharId,
        string oppCharName,
        string opponentMode,
        bool won,
        bool lost,
        bool tied,
        bool flawless,
        int hpDiff,
        float duration)
    {
        string key = agentCharId + "|" + oppCharId + "|" + opponentMode;

        if (!byMatchup.TryGetValue(key, out var agg))
        {
            agg = new MatchupAggregate
            {
                agentCharId = agentCharId,
                agentCharName = agentCharName,
                oppCharId = oppCharId,
                oppCharName = oppCharName,
                opponentMode = opponentMode
            };
            byMatchup.Add(key, agg);
        }

        agg.matches++;
        if (won) agg.wins++;
        if (lost) agg.losses++;
        if (tied) agg.ties++;
        if (won && flawless) agg.flawlessWins++;

        agg.totalHpDiff += hpDiff;
        agg.totalDuration += duration;
    }

    private void ExportSummaries()
    {
        ExportByAgentCharacterSummary();
        ExportByMatchupSummary();
    }

    private void ExportByAgentCharacterSummary()
    {
        using (StreamWriter sw = new StreamWriter(byAgentCsvPath, false))
        {
            sw.WriteLine("agent_char_id,agent_char_name,matches,wins,losses,ties,winrate,avg_hp_diff,avg_duration_s,flawless_wins");

            foreach (var kvp in byAgentCharacter)
            {
                var a = kvp.Value;
                sw.WriteLine(
                    a.characterId + "," +
                    Escape(a.characterName) + "," +
                    a.matches + "," +
                    a.wins + "," +
                    a.losses + "," +
                    a.ties + "," +
                    a.WinRate.ToString("F4", CultureInfo.InvariantCulture) + "," +
                    a.AvgHpDiff.ToString("F3", CultureInfo.InvariantCulture) + "," +
                    a.AvgDuration.ToString("F3", CultureInfo.InvariantCulture) + "," +
                    a.flawlessWins
                );
            }
        }
    }

    private void ExportByMatchupSummary()
    {
        using (StreamWriter sw = new StreamWriter(byMatchupCsvPath, false))
        {
            sw.WriteLine("agent_char_id,agent_char_name,opp_char_id,opp_char_name,opponent_mode,matches,wins,losses,ties,winrate,avg_hp_diff,avg_duration_s,flawless_wins");

            foreach (var kvp in byMatchup)
            {
                var a = kvp.Value;
                sw.WriteLine(
                    a.agentCharId + "," +
                    Escape(a.agentCharName) + "," +
                    a.oppCharId + "," +
                    Escape(a.oppCharName) + "," +
                    Escape(a.opponentMode) + "," +
                    a.matches + "," +
                    a.wins + "," +
                    a.losses + "," +
                    a.ties + "," +
                    a.WinRate.ToString("F4", CultureInfo.InvariantCulture) + "," +
                    a.AvgHpDiff.ToString("F3", CultureInfo.InvariantCulture) + "," +
                    a.AvgDuration.ToString("F3", CultureInfo.InvariantCulture) + "," +
                    a.flawlessWins
                );
            }
        }
    }

    private string ResolveOpponentMode()
    {
        if (!string.IsNullOrWhiteSpace(manualOpponentLabel))
            return manualOpponentLabel;

        if (gameManager != null && gameManager.opponentDirector != null)
            return gameManager.opponentDirector.GetCurrentModeName();

        return "Unknown";
    }

    private void WriteMatchCsvHeaderIfNeeded()
    {
        if (File.Exists(matchCsvPath)) return;

        File.WriteAllText(
            matchCsvPath,
            "timestamp_utc,match_index,stage,opponent_mode,agent_player,agent_char_id,agent_char_name,opp_char_id,opp_char_name,winner_player,is_tie,is_flawless,agent_won,agent_hp,opp_hp,hp_diff,match_length_s" +
            Environment.NewLine
        );
    }

    private string Escape(string value)
    {
        if (string.IsNullOrEmpty(value)) return "\"\"";
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    private string Bool01(bool value) => value ? "1" : "0";

    private void OnApplicationQuit()
    {
        if (!ShouldLog()) return;
        ExportSummaries();
    }
}