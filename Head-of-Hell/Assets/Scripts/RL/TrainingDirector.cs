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

    [Header("Progressive Mixing Phases")]
    [SerializeField] private int phase2StartEpisode = 3000;
    [SerializeField] private int phase3StartEpisode = 5000;
    [SerializeField] private int phase4StartEpisode = 7000;

    [Header("Current Weights (debug)")]
    [Range(0f, 1f)] public float scriptedWeight = 0.70f;
    [Range(0f, 1f)] public float inferenceWeight = 0.20f;
    [Range(0f, 1f)] public float mirrorWeight = 0.10f;

    [Header("Scripted Curriculum")]
    [Range(0f, 1f)] public float minScriptedSkill = 0.35f;
    [Range(0f, 1f)] public float maxScriptedSkill = 0.85f;
    public int curriculumEpisodes = 5000;

    [Header("Debug")]
    public OpponentMode currentMode;
    [SerializeField] private int episodeIndex = 0;
    public float currentScriptedSkill = 0.35f;

    private void Awake()
    {
        episodeIndex = 0;
        ValidateReferences();
    }
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

        UpdateProgressiveWeights();
        SelectNextMode();
        EvaluateScriptedSkill();

        Debug.Log($"[TrainingOpponentDirector] Episode {episodeIndex} | OpponentMode={currentMode} | Weights=({scriptedWeight:F2}, {inferenceWeight:F2}, {mirrorWeight:F2}) | ScriptedSkill={currentScriptedSkill:F2}");

        ApplyCurrentMode();
    }

    private void ApplyScriptedBotMode()
    {
        if (botP2 != null)
        {
            botP2.enabled = true;
            botP2.SetSkill(currentScriptedSkill);
        }

        if (agentP2 != null)
        {
            agentP2.ClearInput();
            agentP2.enabled = false;
        }

        if (decisionP2 != null)
            decisionP2.enabled = false;

        if (behaviorP2 != null)
        {
            behaviorP2.BehaviorType = BehaviorType.Default;
            behaviorP2.Model = null;
        }

        Debug.Log("[TrainingOpponentDirector] Applied ScriptedBot mode on P2.");
    }

    public void ApplyCurrentMode()
    {
        switch (currentMode)
        {
            case OpponentMode.ScriptedBot:
                if (CanApplyScriptedMode())
                {
                    ApplyScriptedBotMode();
                }
                else
                {
                    Debug.LogWarning("[TrainingOpponentDirector] Scripted mode unavailable. Falling back to MirrorSelfPlay.");
                    currentMode = OpponentMode.MirrorSelfPlay;
                    ApplyMirrorMode();
                }
                break;

            case OpponentMode.InferenceModel:
                if (CanApplyInferenceMode())
                {
                    ApplyInferenceMode();
                }
                else
                {
                    Debug.LogWarning("[TrainingOpponentDirector] Inference mode unavailable. Falling back to ScriptedBot.");
                    currentMode = OpponentMode.ScriptedBot;
                    ApplyScriptedBotMode();
                }
                break;

            case OpponentMode.MirrorSelfPlay:
                if (CanApplyMirrorMode())
                {
                    ApplyMirrorMode();
                }
                else
                {
                    Debug.LogWarning("[TrainingOpponentDirector] Mirror mode unavailable. Falling back to ScriptedBot.");
                    currentMode = OpponentMode.ScriptedBot;
                    ApplyScriptedBotMode();
                }
                break;
        }
    }

    public void RebindAfterCharacterSwap()
    {
        Character p1 = gameManager != null && gameManager.p1Manager != null
            ? gameManager.p1Manager.GetCurrentCharacter()
            : null;

        Character p2 = gameManager != null && gameManager.p2Manager != null
            ? gameManager.p2Manager.GetCurrentCharacter()
            : null;

        if (currentMode == OpponentMode.ScriptedBot)
        {
            if (botP2 != null && p2 != null && p1 != null)
                botP2.Rebind(p2, p1);
        }

        if (agentP1 != null)
            agentP1.ForceRebind();

        if (agentP2 != null && agentP2.enabled)
            agentP2.ForceRebind();
    }

    private void ApplyMirrorMode()
    {
        if (botP2 != null)
            botP2.enabled = false;

        if (agentP2 != null)
        {
            agentP2.ClearInput();
            agentP2.enabled = true;
        }

        if (decisionP2 != null)
            decisionP2.enabled = true;

        if (behaviorP2 != null)
        {
            behaviorP2.BehaviorType = BehaviorType.Default;
            behaviorP2.Model = null;
        }

        Debug.Log("[TrainingOpponentDirector] Applied MirrorSelfPlay mode on P2.");
    }

    private void ApplyInferenceMode()
    {
        if (botP2 != null)
            botP2.enabled = false;

        if (agentP2 != null)
        {
            agentP2.ClearInput();
            agentP2.enabled = true;
        }

        if (decisionP2 != null)
            decisionP2.enabled = true;

        if (behaviorP2 != null)
        {
            behaviorP2.Model = oldOpponentModel;
            behaviorP2.BehaviorType = BehaviorType.InferenceOnly;
        }

        Debug.Log("[TrainingOpponentDirector] Applied InferenceModel mode on P2.");
    }

    private void UpdateProgressiveWeights()
    {
        if (episodeIndex >= phase4StartEpisode)
        {
            // Phase 4: advanced
            scriptedWeight = 0.10f;
            inferenceWeight = 0.30f;
            mirrorWeight = 0.60f;
        }
        else if (episodeIndex >= phase3StartEpisode)
        {
            // Phase 3: strong self-play emphasis
            scriptedWeight = 0.15f;
            inferenceWeight = 0.25f;
            mirrorWeight = 0.60f;
        }
        else if (episodeIndex >= phase2StartEpisode)
        {
            // Phase 2: balanced transition
            scriptedWeight = 0.40f;
            inferenceWeight = 0.30f;
            mirrorWeight = 0.30f;
        }
        else
        {
            // Phase 1: fundamentals
            scriptedWeight = 0.70f;
            inferenceWeight = 0.20f;
            mirrorWeight = 0.10f;
        }
    }

    private void ValidateReferences()
    {
        if (gameManager == null)
            Debug.LogWarning("[TrainingOpponentDirector] Missing GameManager reference.");

        if (agentP1 == null)
            Debug.LogWarning("[TrainingOpponentDirector] Missing agentP1 reference.");

        if (behaviorP1 == null)
            Debug.LogWarning("[TrainingOpponentDirector] Missing behaviorP1 reference.");

        if (decisionP1 == null)
            Debug.LogWarning("[TrainingOpponentDirector] Missing decisionP1 reference.");

        if (agentP2 == null)
            Debug.LogWarning("[TrainingOpponentDirector] Missing agentP2 reference.");

        if (behaviorP2 == null)
            Debug.LogWarning("[TrainingOpponentDirector] Missing behaviorP2 reference.");

        if (decisionP2 == null)
            Debug.LogWarning("[TrainingOpponentDirector] Missing decisionP2 reference.");

        if (botP2 == null)
            Debug.LogWarning("[TrainingOpponentDirector] Missing botP2 reference.");

        if (oldOpponentModel == null)
            Debug.LogWarning("[TrainingOpponentDirector] Missing oldOpponentModel reference.");
    }

    private bool CanApplyScriptedMode()
    {
        return botP2 != null;
    }

    private bool CanApplyInferenceMode()
    {
        return agentP2 != null && decisionP2 != null && behaviorP2 != null && oldOpponentModel != null;
    }

    private bool CanApplyMirrorMode()
    {
        return agentP2 != null && decisionP2 != null && behaviorP2 != null;
    }

    public OpponentMode GetCurrentMode()
    {
        return currentMode;
    }

    public string GetCurrentModeName()
    {
        return currentMode.ToString();
    }

    public bool IsScriptedMode()
    {
        return currentMode == OpponentMode.ScriptedBot;
    }

    public bool IsInferenceMode()
    {
        return currentMode == OpponentMode.InferenceModel;
    }

    public bool IsMirrorMode()
    {
        return currentMode == OpponentMode.MirrorSelfPlay;
    }
}