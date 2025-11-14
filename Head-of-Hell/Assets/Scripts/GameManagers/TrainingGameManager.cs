using UnityEngine;

public class TrainingGameManager : MonoBehaviour
{
    [Header("Arena bindings")]
    public CharacterManager p1Manager, p2Manager;
    public FighterAgent agentP1, agentP2;
    public Transform p1Spawn, p2Spawn;

    [Header("Options")]
    public int maxHealth = 150;   // if you want to override per-episode
    public bool randomizeHealth = false;
    public Vector2Int healthRange = new Vector2Int(100, 200);

    // Always true in this scene
    public bool trainingMode => true;

    void Awake()
    {
        // Make sure nothing persists between arenas
        // (no DontDestroyOnLoad here)
        // Also no statics.
    }

    // --- API compatible with your current calls ---

    public void RoundEnd(int loserPlayerNum, string winnerName)
    {
        SoftResetRound(loserPlayerNum);
    }

    public void RoundEndTie(int playerNum)
    {
        SoftResetRound(0);
    }

    public void RoundEndFlawless(int loserPlayerNum, string winnerName)
    {
        SoftResetRound(loserPlayerNum);
    }

    public void EnableGamePlay()
    {
        p1Manager?.Resume();
        p2Manager?.Resume();
    }

    public void DisableGamePlay()
    {
        p1Manager?.Pause();
        p2Manager?.Pause();
    }

    public void SoftResetRound(int winnerPlayerNum = 0)
    {
        // End episodes (don’t recurse into OnEpisodeBegin)
        if (agentP1) agentP1.EndEpisode();
        if (agentP2) agentP2.EndEpisode();

        // Reset characters to spawns
        var c1 = p1Manager?.CharacterChoice(1);
        var c2 = p2Manager?.CharacterChoice(2);

        if (randomizeHealth)
        {
            var hp = Random.Range(healthRange.x, healthRange.y + 1);
            if (c1) { c1.SetCurrentHealth(hp); }  // optional – or use your ResetForEpisode overload
            if (c2) { c2.SetCurrentHealth(hp); }
        }

        if (c1) c1.ResetForEpisode();
        if (c2) c2.ResetForEpisode();

        EnableGamePlay();
    }
}
