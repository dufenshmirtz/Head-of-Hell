using UnityEngine;
using Unity.MLAgents.Policies;

public class PvEMLBotRuntimeSetup : MonoBehaviour
{
    void Start()
    {
        if (!PvESelectionState.IsPvE) return;
        if (PvESelectionState.SelectedBotType != PvEBotType.MLAgent) return;

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
            Debug.LogError("PvE ML Setup: Could not find both players.");
            return;
        }

        CharacterManager botManager =
            PvESelectionState.SelectedBotSide == PvEBotSide.Player1 ? p1 : p2;

        CharacterManager humanManager =
            PvESelectionState.SelectedBotSide == PvEBotSide.Player1 ? p2 : p1;

        SetupMLBot(botManager, humanManager);
    }

    void SetupMLBot(CharacterManager botManager, CharacterManager enemyManager)
    {
        GameObject botObj = botManager.gameObject;
        GameObject humanObj = enemyManager.gameObject;

        // BOT SIDE
        var ml = botObj.GetComponent<FighterAgent>();
        if (ml == null)
        {
            Debug.LogError($"PvE ML Setup: No FighterAgent found on Player {botManager.playerNum}.");
            return;
        }

        var decision = botObj.GetComponent<Unity.MLAgents.DecisionRequester>();
        var behavior = botObj.GetComponent<BehaviorParameters>();
        var scripted = botObj.GetComponent<SimpleBotController>();

        if (scripted != null)
            scripted.enabled = false;

        ml.selfManager = botManager;
        ml.enemyManager = enemyManager;
        ml.enabled = true;

        if (decision != null)
            decision.enabled = true;

        if (behavior != null)
        {
            behavior.BehaviorType = BehaviorType.Default;
            // model δεν το πειράζουμε εδώ unless εσύ θες inference-only setup
        }

        ml.ForceRebind();
        ml.ClearInput();

        // HUMAN SIDE
        var humanML = humanObj.GetComponent<FighterAgent>();
        if (humanML != null)
        {
            humanML.ClearInput();
            humanML.enabled = false;
        }

        var humanDecision = humanObj.GetComponent<Unity.MLAgents.DecisionRequester>();
        if (humanDecision != null)
            humanDecision.enabled = false;

        var humanScripted = humanObj.GetComponent<SimpleBotController>();
        if (humanScripted != null)
            humanScripted.enabled = false;

        Debug.Log($"ML Bot Enabled on Player {botManager.playerNum}");
    }
}