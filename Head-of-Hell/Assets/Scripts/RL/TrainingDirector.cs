using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.Barracuda;

public class TrainingOpponentDirector : MonoBehaviour
{
    [Header("Core References")]
    public GameManager gameManager;

    [Header("P1 References")]
    public FighterAgent agentP1;
    public BehaviorParameters behaviorP1;
    public DecisionRequester decisionP1;

    [Header("P2 References")]
    public FighterAgent agentP2;
    public BehaviorParameters behaviorP2;
    public DecisionRequester decisionP2;
    public SimpleBotController botP2;

    [Header("Models")]
    public NNModel oldOpponentModel;

    [Header("Selection Weights")]
    [Range(0f, 1f)] public float scriptedWeight = 0.50f;
    [Range(0f, 1f)] public float inferenceWeight = 0.25f;
    [Range(0f, 1f)] public float mirrorWeight = 0.25f;

    [Header("Scripted Curriculum")]
    [Range(0f, 1f)] public float minScriptedSkill = 0.35f;
    [Range(0f, 1f)] public float maxScriptedSkill = 0.85f;
    public int curriculumEpisodes = 5000;

    [Header("Debug")]
    public OpponentMode currentMode;
    public int episodeIndex = 0;
    public float currentScriptedSkill = 0.35f;

    public OpponentMode SelectNextMode()
    {
        float total = scriptedWeight + inferenceWeight + mirrorWeight;
        if (total <= 0f)
        {
            Debug.LogWarning("[TrainingOpponentDirector] Weights sum to 0. Falling back to ScriptedBot.");
            currentMode = OpponentMode.ScriptedBot;
            return currentMode;
        }

        float r = Random.value * total;

        if (r < scriptedWeight)
        {
            currentMode = OpponentMode.ScriptedBot;
            return currentMode;
        }

        r -= scriptedWeight;

        if (r < inferenceWeight)
        {
            currentMode = OpponentMode.InferenceModel;
            return currentMode;
        }

        currentMode = OpponentMode.MirrorSelfPlay;
        return currentMode;
    }

    public float EvaluateScriptedSkill()
    {
        float t = curriculumEpisodes <= 0 ? 1f : Mathf.Clamp01((float)episodeIndex / curriculumEpisodes);
        currentScriptedSkill = Mathf.Lerp(minScriptedSkill, maxScriptedSkill, t);
        return currentScriptedSkill;
    }

    public void PrepareNextEpisode()
    {
        episodeIndex++;

        SelectNextMode();
        EvaluateScriptedSkill();

        Debug.Log($"[TrainingOpponentDirector] Episode {episodeIndex} -> Mode: {currentMode}, ScriptedSkill: {currentScriptedSkill:F2}");
    }
}