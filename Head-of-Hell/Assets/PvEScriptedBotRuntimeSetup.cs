using UnityEngine;

public class PvEScriptedBotRuntimeSetup : MonoBehaviour
{
    void Start()
    {
        if (!PvESelectionState.IsPvE) return;
        if (PvESelectionState.SelectedBotType != PvEBotType.ScriptedBot) return;

        // Βρες όλους τους CharacterManagers (P1 & P2)
        CharacterManager[] managers = FindObjectsOfType<CharacterManager>();

        CharacterManager p1 = null;
        CharacterManager p2 = null;

        foreach (var m in managers)
        {
            if (m.playerNum == 1) p1 = m;
            if (m.playerNum == 2) p2 = m;
        }

        if (p1 == null || p2 == null)
        {
            Debug.LogError("PvE Bot Setup: Could not find both players.");
            return;
        }

        // Ποιος γίνεται bot;
        CharacterManager botManager =
            PvESelectionState.SelectedBotSide == PvEBotSide.Player1 ? p1 : p2;

        CharacterManager humanManager =
            PvESelectionState.SelectedBotSide == PvEBotSide.Player1 ? p2 : p1;

        SetupScriptedBot(botManager, humanManager);
    }

    void SetupScriptedBot(CharacterManager botManager, CharacterManager enemyManager)
    {
        GameObject botObj = botManager.gameObject;

        // 1. Disable ML Agent (αν υπάρχει)
        var ml = botObj.GetComponent<FighterAgent>();
        if (ml != null) ml.enabled = false;

        // 2. Add Scripted Bot
        var bot = botObj.GetComponent<SimpleBotController>();
        if (bot == null)
            bot = botObj.AddComponent<SimpleBotController>();

        // 3. Set references
        var selfChar = botManager.GetCurrentCharacter();
        var enemyChar = enemyManager.GetCurrentCharacter();

        bot.Rebind(selfChar, enemyChar);

        // 4. Difficulty → skill
        float skill = GetSkillFromDifficulty(PvESelectionState.SelectedDifficulty);
        bot.SetSkill(skill);

        Debug.Log($"Scripted Bot Enabled on Player {botManager.playerNum} with skill {skill}");
    }

    float GetSkillFromDifficulty(PvEDifficulty difficulty)
    {
        switch (difficulty)
        {
            case PvEDifficulty.Easy: return 0.35f;
            case PvEDifficulty.Medium: return 0.60f;
            case PvEDifficulty.Hard: return 0.90f;
        }

        return 0.6f;
    }
}