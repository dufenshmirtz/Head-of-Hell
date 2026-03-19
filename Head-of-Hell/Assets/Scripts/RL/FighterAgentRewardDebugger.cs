using UnityEngine;

public class FighterAgentRewardDebugger : MonoBehaviour
{
    [Header("Debug Toggles")]
    [SerializeField] private bool debugRewardBreakdown = false;
    [SerializeField] private bool debugProfileLogging = false;
    [SerializeField] private int debugLogEveryNEpisodes = 10;

    // episode counters
    private int localEpisodeCounter = 0;

    // reward buckets
    private float epStepPenaltyTotal;
    private float epDamageDealtRewardTotal;
    private float epDamageTakenRewardTotal;
    private float epSpacingRewardTotal;
    private float epStackPenaltyTotal;
    private float epMashPenaltyTotal;
    private float epAirJumpPenaltyTotal;
    private float epEdgeCampPenaltyTotal;
    private float epApproachRewardTotal;
    private float epExtremeFarHeavyPenaltyTotal;
    private float epExtremeFarChargePenaltyTotal;
    private float epFarMeleeLightPenaltyTotal;
    private float epFarMeleeSpecialPenaltyTotal;
    private float epDashNoDirectionPenaltyTotal;
    private float epWrongFacingSpecialPenaltyTotal;
    private float epWinRewardTotal;
    private float epLossRewardTotal;

    public void BeginEpisode(string playerSuffix, int? characterId, ReachType lightReach, ReachType specialReach, bool profileLoaded)
    {
        localEpisodeCounter++;
        ResetEpisodeRewardDebug();

        if (debugProfileLogging)
        {
            Debug.Log(
                $"[Agent {playerSuffix}] Episode {localEpisodeCounter} BEGIN | " +
                $"CharacterID={(characterId.HasValue ? characterId.Value.ToString() : "?")} | " +
                $"LightReach={lightReach} | SpecialReach={specialReach} | ProfileLoaded={profileLoaded}"
            );
        }
    }

    public void LogStepPenalty(float value) => epStepPenaltyTotal += value;
    public void LogDamageDealt(float value) => epDamageDealtRewardTotal += value;
    public void LogDamageTaken(float value) => epDamageTakenRewardTotal += value;
    public void LogSpacing(float value) => epSpacingRewardTotal += value;
    public void LogStackPenalty(float value) => epStackPenaltyTotal += value;
    public void LogMashPenalty(float value) => epMashPenaltyTotal += value;
    public void LogAirJumpPenalty(float value) => epAirJumpPenaltyTotal += value;
    public void LogEdgeCampPenalty(float value) => epEdgeCampPenaltyTotal += value;
    public void LogApproachReward(float value) => epApproachRewardTotal += value;
    public void LogExtremeFarHeavyPenalty(float value) => epExtremeFarHeavyPenaltyTotal += value;
    public void LogExtremeFarChargePenalty(float value) => epExtremeFarChargePenaltyTotal += value;
    public void LogFarMeleeLightPenalty(float value) => epFarMeleeLightPenaltyTotal += value;
    public void LogFarMeleeSpecialPenalty(float value) => epFarMeleeSpecialPenaltyTotal += value;
    public void LogDashNoDirectionPenalty(float value) => epDashNoDirectionPenaltyTotal += value;
    public void LogWrongFacingSpecialPenalty(float value) => epWrongFacingSpecialPenaltyTotal += value;
    public void LogWinReward(float value) => epWinRewardTotal += value;
    public void LogLossReward(float value) => epLossRewardTotal += value;

    public void EndEpisode(
        string playerSuffix,
        string endReason,
        int? characterId,
        ReachType lightReach,
        ReachType specialReach,
        bool profileLoaded)
    {
        if (!debugRewardBreakdown) return;
        if (debugLogEveryNEpisodes <= 0) debugLogEveryNEpisodes = 1;
        if (localEpisodeCounter % debugLogEveryNEpisodes != 0) return;

        float total =
            epStepPenaltyTotal +
            epDamageDealtRewardTotal +
            epDamageTakenRewardTotal +
            epSpacingRewardTotal +
            epStackPenaltyTotal +
            epMashPenaltyTotal +
            epAirJumpPenaltyTotal +
            epEdgeCampPenaltyTotal +
            epApproachRewardTotal +
            epExtremeFarHeavyPenaltyTotal +
            epExtremeFarChargePenaltyTotal +
            epFarMeleeLightPenaltyTotal +
            epFarMeleeSpecialPenaltyTotal +
            epDashNoDirectionPenaltyTotal +
            epWrongFacingSpecialPenaltyTotal +
            epWinRewardTotal +
            epLossRewardTotal;

        Debug.Log(
            $"[Agent {playerSuffix}] Episode {localEpisodeCounter} END={endReason} | " +
            $"CharID={(characterId.HasValue ? characterId.Value.ToString() : "?")}, " +
            $"LightReach={lightReach}, SpecialReach={specialReach}, ProfileLoaded={profileLoaded}\n" +
            $"Total={total:F4}\n" +
            $"  DamageDealt={epDamageDealtRewardTotal:F4}\n" +
            $"  DamageTaken={epDamageTakenRewardTotal:F4}\n" +
            $"  Win={epWinRewardTotal:F4}\n" +
            $"  Loss={epLossRewardTotal:F4}\n" +
            $"  Step={epStepPenaltyTotal:F4}\n" +
            $"  Spacing={epSpacingRewardTotal:F4}\n" +
            $"  Stack={epStackPenaltyTotal:F4}\n" +
            $"  Mash={epMashPenaltyTotal:F4}\n" +
            $"  AirJump={epAirJumpPenaltyTotal:F4}\n" +
            $"  EdgeCamp={epEdgeCampPenaltyTotal:F4}\n" +
            $"  Approach={epApproachRewardTotal:F4}\n" +
            $"  ExtremeFarHeavy={epExtremeFarHeavyPenaltyTotal:F4}\n" +
            $"  ExtremeFarCharge={epExtremeFarChargePenaltyTotal:F4}\n" +
            $"  FarMeleeLight={epFarMeleeLightPenaltyTotal:F4}\n" +
            $"  FarMeleeSpecial={epFarMeleeSpecialPenaltyTotal:F4}\n" +
            $"  DashNoDirection={epDashNoDirectionPenaltyTotal:F4}\n" +
            $"  WrongFacingSpecial={epWrongFacingSpecialPenaltyTotal:F4}"
        );
    }

    private void ResetEpisodeRewardDebug()
    {
        epStepPenaltyTotal = 0f;
        epDamageDealtRewardTotal = 0f;
        epDamageTakenRewardTotal = 0f;
        epSpacingRewardTotal = 0f;
        epStackPenaltyTotal = 0f;
        epMashPenaltyTotal = 0f;
        epAirJumpPenaltyTotal = 0f;
        epEdgeCampPenaltyTotal = 0f;
        epApproachRewardTotal = 0f;
        epExtremeFarHeavyPenaltyTotal = 0f;
        epExtremeFarChargePenaltyTotal = 0f;
        epFarMeleeLightPenaltyTotal = 0f;
        epFarMeleeSpecialPenaltyTotal = 0f;
        epDashNoDirectionPenaltyTotal = 0f;
        epWrongFacingSpecialPenaltyTotal = 0f;
        epWinRewardTotal = 0f;
        epLossRewardTotal = 0f;
    }
}