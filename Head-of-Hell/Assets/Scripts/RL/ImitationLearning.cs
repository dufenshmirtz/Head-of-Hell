using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ImitationLogger : MonoBehaviour
{
    [Header("Bindings")]
    public CharacterManager selfManager;
    public CharacterManager enemyManager;

    [Header("Logging")]
    public bool enableLogging = true;
    public bool logOnlyWhenGameplayActive = true;
    public bool autoStart = true;
    public bool prettyDebug = false;

    [Header("Observation Settings")]
    [SerializeField] private float relXScale = 9f;
    [SerializeField] private float relYScale = 5f;
    [SerializeField] private float velScale = 10f;
    [SerializeField] private int totalCharacterCount = 10;

    [Header("Output")]
    [SerializeField] private string folderName = "ILLogs";
    [SerializeField] private string filePrefix = "imitation";

    [Header("Metadata")]
    [SerializeField] private string controlledPlayerLabel = "P1";

    private Character self;
    private Character opp;
    private GameManager gameManager;

    private string currentFilePath;
    private StreamWriter writer;

    private int episodeIndex = 0;
    private int stepIndex = 0;
    private bool episodeActive = false;
    private bool terminalLoggedThisEpisode = false;

    private void Start()
    {
        if (selfManager == null)
            selfManager = GetComponent<CharacterManager>();

        if (selfManager != null)
        {
            selfManager.OnCharacterReady += BindSelf;
            selfManager.OnCharacterChanged += BindSelf;
        }

        if (enemyManager != null)
        {
            enemyManager.OnCharacterReady += BindEnemy;
            enemyManager.OnCharacterChanged += BindEnemy;
        }

        TryBindNow();

        if (self != null)
            gameManager = self.GetComponent<CharacterSetup>()?.gameManager;

        if (autoStart && enableLogging)
        {
            OpenLogFile();
            StartNewEpisode();
        }
    }

    private void Update()
    {
        if (!enableLogging || writer == null)
            return;

        if (self == null || opp == null)
        {
            TryBindNow();
            return;
        }

        if (gameManager == null)
        {
            gameManager = self.GetComponent<CharacterSetup>()?.gameManager;
        }

        bool done = IsTerminal();

        if (logOnlyWhenGameplayActive && !done && !IsGameplayActive())
            return;

        if (!episodeActive)
            StartNewEpisode();

        float[] obs = FighterObservationEncoder.Encode(
            self,
            opp,
            relXScale,
            relYScale,
            velScale,
            totalCharacterCount
        );

        HumanActionEncoder.DiscreteActionFrame act = HumanActionEncoder.Encode(self);

        string outcome = "none";
        if (done)
        {
            outcome = GetOutcome();
        }

        FrameRecord record = new FrameRecord
        {
            session_id = Path.GetFileNameWithoutExtension(currentFilePath),
            episode = episodeIndex,
            step = stepIndex,
            timestamp_utc = DateTime.UtcNow.ToString("o"),

            controlled_player = controlledPlayerLabel,
            self_character_id = self != null ? self.characterID : -1,
            opp_character_id = opp != null ? opp.characterID : -1,

            obs = obs,
            act = act.ToArray(),

            self_hp = self != null ? self.GetCurrentHealth() : -1,
            opp_hp = opp != null ? opp.GetCurrentHealth() : -1,

            done = done,
            outcome = outcome
        };

        WriteRecord(record);

        stepIndex++;

        if (done && !terminalLoggedThisEpisode)
        {
            terminalLoggedThisEpisode = true;
            episodeActive = false;
        }
    }

    private void TryBindNow()
    {
        if (self == null && selfManager != null)
        {
            var c = selfManager.CharacterChoice(1);
            if (c != null)
                BindSelf(c);
        }

        if (opp == null && enemyManager != null)
        {
            var e = enemyManager.CharacterChoice(1);
            if (e != null)
                BindEnemy(e);
        }
    }

    private void BindSelf(Character c)
    {
        self = c;
        if (self != null && gameManager == null)
            gameManager = self.GetComponent<CharacterSetup>()?.gameManager;
    }

    private void BindEnemy(Character c)
    {
        opp = c;
    }

    private bool IsGameplayActive()
    {
        if (self == null || opp == null)
            return false;

        if (self.ignoreUpdate || opp.ignoreUpdate)
            return false;

        return true;
    }

    private bool IsTerminal()
    {
        if (self == null || opp == null)
            return false;

        return self.GetCurrentHealth() <= 0 || opp.GetCurrentHealth() <= 0;
    }

    private string GetOutcome()
    {
        if (self == null || opp == null)
            return "unknown";

        int selfHp = self.GetCurrentHealth();
        int oppHp = opp.GetCurrentHealth();

        if (selfHp <= 0 && oppHp <= 0)
            return "draw";
        if (oppHp <= 0)
            return "win";
        if (selfHp <= 0)
            return "loss";

        return "none";
    }

    private void StartNewEpisode()
    {
        episodeIndex++;
        stepIndex = 0;
        episodeActive = true;
        terminalLoggedThisEpisode = false;

        if (prettyDebug)
            Debug.Log($"[ImitationLogger] New episode {episodeIndex}");
    }

    private void OpenLogFile()
    {
        string dir = GetLogsDirectory();

        Directory.CreateDirectory(dir);

        string safePlayer = string.IsNullOrWhiteSpace(controlledPlayerLabel)
            ? "player"
            : controlledPlayerLabel;

        string fileName = $"{filePrefix}_{safePlayer}_{DateTime.Now:yyyyMMdd_HHmmss}.jsonl";

        currentFilePath = Path.Combine(dir, fileName);

        writer = new StreamWriter(currentFilePath, false, Encoding.UTF8);
        writer.AutoFlush = true;

        Debug.Log($"[ImitationLogger] Logging to: {currentFilePath}");
    }

    private string GetLogsDirectory()
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

    #if UNITY_EDITOR
        string modeFolder = "editor_logs";
    #else
        string modeFolder = "game_logs";
    #endif

        string logsPath = Path.Combine(
            documentsPath,
            "My Games",
            "Head of Hell",
            "ImitationLogs",
            modeFolder,
            DateTime.Now.ToString("yyyy-MM-dd")
        );

        return logsPath;
    }

    private void WriteRecord(FrameRecord record)
    {
        string json = JsonUtility.ToJson(record);
        writer.WriteLine(json);

        if (prettyDebug)
            Debug.Log(json);
    }

    public void StopLogging()
    {
        if (writer != null)
        {
            writer.Flush();
            writer.Close();
            writer = null;
        }
    }

    public string GetCurrentFilePath()
    {
        return currentFilePath;
    }

    private void OnDestroy()
    {
        if (selfManager != null)
        {
            selfManager.OnCharacterReady -= BindSelf;
            selfManager.OnCharacterChanged -= BindSelf;
        }

        if (enemyManager != null)
        {
            enemyManager.OnCharacterReady -= BindEnemy;
            enemyManager.OnCharacterChanged -= BindEnemy;
        }

        StopLogging();
    }

    private void OnApplicationQuit()
    {
        StopLogging();
    }

    [Serializable]
    private class FrameRecord
    {
        public string session_id;
        public int episode;
        public int step;
        public string timestamp_utc;

        public string controlled_player;
        public int self_character_id;
        public int opp_character_id;

        public float[] obs;
        public int[] act;

        public int self_hp;
        public int opp_hp;

        public bool done;
        public string outcome;
    }
}