using UnityEngine;
using System.Text;

public class FighterAgentRewardDebugger : MonoBehaviour
{
    [Header("Debug Toggles")]
    [SerializeField] private bool debugRewardBreakdown = false;
    [SerializeField] private bool debugProfileLogging = false;
    [SerializeField] private int debugLogEveryNEpisodes = 10;

    private int localEpisodeCounter = 0;

    // totals
    private float epStepPenaltyTotal;
    private float epDamageDealtRewardTotal;
    private float epDamageTakenRewardTotal;
    private float epSpacingRewardTotal;
    private float epStackPenaltyTotal;
    private float epMashPenaltyTotal;
    private float epAirJumpPenaltyTotal;
    private float epEdgeCampPenaltyTotal;
    private float epApproachRewardTotal;
    private float epForwardPressureRewardTotal;
    private float epClosePressureRewardTotal;
    private float epRetreatFromClosePenaltyTotal;
    private float epUsefulIntentRewardTotal;
    private float epPassiveNearPenaltyTotal;
    private float epBodyPushPenaltyTotal;
    private float epParryOpportunityRewardTotal;
    private float epUnsafeParryPenaltyTotal;
    private float epObviousPunishRewardTotal;
    private float epMissedPunishPenaltyTotal;
    private float epChargePunishRewardTotal;
    private float epMissedChargeSpecialPenaltyTotal;
    private float epChargeRunawayPenaltyTotal;
    private float epLowHealthPanicPenaltyTotal;
    private float epExtremeFarHeavyPenaltyTotal;
    private float epExtremeFarChargePenaltyTotal;
    private float epFarMeleeLightPenaltyTotal;
    private float epFarMeleeSpecialPenaltyTotal;
    private float epDashNoDirectionPenaltyTotal;
    private float epWrongFacingSpecialPenaltyTotal;
    private float epBlockHoldPenaltyTotal;
    private float epRepeatSameMovePenaltyTotal;
    private float epChargeSpamPenaltyTotal;
    private float epEmptyChargeReleasePenaltyTotal;
    private float epSafetyResetPenaltyTotal;
    private float epWinRewardTotal;
    private float epLossRewardTotal;

    // counts
    private int epStepPenaltyCount;
    private int epDamageDealtCount;
    private int epDamageTakenCount;
    private int epSpacingCount;
    private int epStackPenaltyCount;
    private int epMashPenaltyCount;
    private int epAirJumpPenaltyCount;
    private int epEdgeCampPenaltyCount;
    private int epApproachRewardCount;
    private int epForwardPressureRewardCount;
    private int epClosePressureRewardCount;
    private int epRetreatFromClosePenaltyCount;
    private int epUsefulIntentRewardCount;
    private int epPassiveNearPenaltyCount;
    private int epBodyPushPenaltyCount;
    private int epParryOpportunityRewardCount;
    private int epUnsafeParryPenaltyCount;
    private int epObviousPunishRewardCount;
    private int epMissedPunishPenaltyCount;
    private int epChargePunishRewardCount;
    private int epMissedChargeSpecialPenaltyCount;
    private int epChargeRunawayPenaltyCount;
    private int epLowHealthPanicPenaltyCount;
    private int epExtremeFarHeavyPenaltyCount;
    private int epExtremeFarChargePenaltyCount;
    private int epFarMeleeLightPenaltyCount;
    private int epFarMeleeSpecialPenaltyCount;
    private int epDashNoDirectionPenaltyCount;
    private int epWrongFacingSpecialPenaltyCount;
    private int epBlockHoldPenaltyCount;
    private int epRepeatSameMovePenaltyCount;
    private int epChargeSpamPenaltyCount;
    private int epEmptyChargeReleasePenaltyCount;
    private int epSafetyResetPenaltyCount;
    private int epWinRewardCount;
    private int epLossRewardCount;

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

    public void LogStepPenalty(float value)
    {
        epStepPenaltyTotal += value;
        epStepPenaltyCount++;
    }

    public void LogDamageDealt(float value)
    {
        epDamageDealtRewardTotal += value;
        if (value != 0f)
        {
            epDamageDealtCount++;
        }
    }

    public void LogDamageTaken(float value)
    {
        epDamageTakenRewardTotal += value;
        if (value != 0f)
        {
            epDamageTakenCount++;
        }
    }

    public void LogSpacing(float value)
    {
        epSpacingRewardTotal += value;
        epSpacingCount++;
    }

    public void LogStackPenalty(float value)
    {
        epStackPenaltyTotal += value;
        epStackPenaltyCount++;
    }

    public void LogMashPenalty(float value)
    {
        epMashPenaltyTotal += value;
        epMashPenaltyCount++;
    }

    public void LogAirJumpPenalty(float value)
    {
        epAirJumpPenaltyTotal += value;
        epAirJumpPenaltyCount++;
    }

    public void LogEdgeCampPenalty(float value)
    {
        epEdgeCampPenaltyTotal += value;
        epEdgeCampPenaltyCount++;
    }

    public void LogApproachReward(float value)
    {
        epApproachRewardTotal += value;
        epApproachRewardCount++;
    }

    public void LogForwardPressureReward(float value)
    {
        epForwardPressureRewardTotal += value;
        epForwardPressureRewardCount++;
    }

    public void LogClosePressureReward(float value)
    {
        epClosePressureRewardTotal += value;
        epClosePressureRewardCount++;
    }

    public void LogRetreatFromClosePenalty(float value)
    {
        epRetreatFromClosePenaltyTotal += value;
        epRetreatFromClosePenaltyCount++;
    }

    public void LogUsefulIntentReward(float value)
    {
        epUsefulIntentRewardTotal += value;
        epUsefulIntentRewardCount++;
    }

    public void LogPassiveNearPenalty(float value)
    {
        epPassiveNearPenaltyTotal += value;
        epPassiveNearPenaltyCount++;
    }

    public void LogBodyPushPenalty(float value)
    {
        epBodyPushPenaltyTotal += value;
        epBodyPushPenaltyCount++;
    }

    public void LogParryOpportunityReward(float value)
    {
        epParryOpportunityRewardTotal += value;
        epParryOpportunityRewardCount++;
    }

    public void LogUnsafeParryPenalty(float value)
    {
        epUnsafeParryPenaltyTotal += value;
        epUnsafeParryPenaltyCount++;
    }

    public void LogObviousPunishReward(float value)
    {
        epObviousPunishRewardTotal += value;
        epObviousPunishRewardCount++;
    }

    public void LogMissedPunishPenalty(float value)
    {
        epMissedPunishPenaltyTotal += value;
        epMissedPunishPenaltyCount++;
    }

    public void LogChargePunishReward(float value)
    {
        epChargePunishRewardTotal += value;
        epChargePunishRewardCount++;
    }

    public void LogMissedChargeSpecialPenalty(float value)
    {
        epMissedChargeSpecialPenaltyTotal += value;
        epMissedChargeSpecialPenaltyCount++;
    }

    public void LogChargeRunawayPenalty(float value)
    {
        epChargeRunawayPenaltyTotal += value;
        epChargeRunawayPenaltyCount++;
    }

    public void LogLowHealthPanicPenalty(float value)
    {
        epLowHealthPanicPenaltyTotal += value;
        epLowHealthPanicPenaltyCount++;
    }

    public void LogExtremeFarHeavyPenalty(float value)
    {
        epExtremeFarHeavyPenaltyTotal += value;
        epExtremeFarHeavyPenaltyCount++;
    }

    public void LogExtremeFarChargePenalty(float value)
    {
        epExtremeFarChargePenaltyTotal += value;
        epExtremeFarChargePenaltyCount++;
    }

    public void LogFarMeleeLightPenalty(float value)
    {
        epFarMeleeLightPenaltyTotal += value;
        epFarMeleeLightPenaltyCount++;
    }

    public void LogFarMeleeSpecialPenalty(float value)
    {
        epFarMeleeSpecialPenaltyTotal += value;
        epFarMeleeSpecialPenaltyCount++;
    }

    public void LogDashNoDirectionPenalty(float value)
    {
        epDashNoDirectionPenaltyTotal += value;
        epDashNoDirectionPenaltyCount++;
    }

    public void LogWrongFacingSpecialPenalty(float value)
    {
        epWrongFacingSpecialPenaltyTotal += value;
        epWrongFacingSpecialPenaltyCount++;
    }

    public void LogBlockHoldPenalty(float value)
    {
        epBlockHoldPenaltyTotal += value;
        epBlockHoldPenaltyCount++;
    }

    public void LogRepeatSameMovePenalty(float value)
    {
        epRepeatSameMovePenaltyTotal += value;
        epRepeatSameMovePenaltyCount++;
    }

    public void LogChargeSpamPenalty(float value)
    {
        epChargeSpamPenaltyTotal += value;
        epChargeSpamPenaltyCount++;
    }

    public void LogEmptyChargeReleasePenalty(float value)
    {
        epEmptyChargeReleasePenaltyTotal += value;
        epEmptyChargeReleasePenaltyCount++;
    }

    public void LogSafetyResetPenalty(float value)
    {
        epSafetyResetPenaltyTotal += value;
        epSafetyResetPenaltyCount++;
    }

    public void LogWinReward(float value)
    {
        epWinRewardTotal += value;
        epWinRewardCount++;
    }

    public void LogLossReward(float value)
    {
        epLossRewardTotal += value;
        epLossRewardCount++;
    }

    public void EndEpisode(
        string playerSuffix,
        string endReason,
        int? characterId,
        ReachType lightReach,
        ReachType specialReach,
        bool profileLoaded)
    {
        if (!debugRewardBreakdown)
        {
            return;
        }

        if (debugLogEveryNEpisodes <= 0)
        {
            debugLogEveryNEpisodes = 1;
        }

        if (localEpisodeCounter % debugLogEveryNEpisodes != 0)
        {
            return;
        }

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
            epForwardPressureRewardTotal +
            epClosePressureRewardTotal +
            epRetreatFromClosePenaltyTotal +
            epUsefulIntentRewardTotal +
            epPassiveNearPenaltyTotal +
            epBodyPushPenaltyTotal +
            epParryOpportunityRewardTotal +
            epUnsafeParryPenaltyTotal +
            epObviousPunishRewardTotal +
            epMissedPunishPenaltyTotal +
            epChargePunishRewardTotal +
            epMissedChargeSpecialPenaltyTotal +
            epChargeRunawayPenaltyTotal +
            epLowHealthPanicPenaltyTotal +
            epExtremeFarHeavyPenaltyTotal +
            epExtremeFarChargePenaltyTotal +
            epFarMeleeLightPenaltyTotal +
            epFarMeleeSpecialPenaltyTotal +
            epDashNoDirectionPenaltyTotal +
            epWrongFacingSpecialPenaltyTotal +
            epBlockHoldPenaltyTotal +
            epRepeatSameMovePenaltyTotal +
            epChargeSpamPenaltyTotal +
            epEmptyChargeReleasePenaltyTotal +
            epSafetyResetPenaltyTotal +
            epWinRewardTotal +
            epLossRewardTotal;

        StringBuilder sb = new StringBuilder();

        sb.AppendLine(
            $"[Agent {playerSuffix}] Episode {localEpisodeCounter} END={endReason} | " +
            $"CharID={(characterId.HasValue ? characterId.Value.ToString() : "?")} | " +
            $"LightReach={lightReach} | SpecialReach={specialReach} | ProfileLoaded={profileLoaded}"
        );

        sb.AppendLine($"Total={total:F4}");

        AppendLine(sb, "DamageDealt", epDamageDealtRewardTotal, epDamageDealtCount);
        AppendLine(sb, "DamageTaken", epDamageTakenRewardTotal, epDamageTakenCount);
        AppendLine(sb, "Win", epWinRewardTotal, epWinRewardCount);
        AppendLine(sb, "Loss", epLossRewardTotal, epLossRewardCount);
        AppendLine(sb, "Step", epStepPenaltyTotal, epStepPenaltyCount);
        AppendLine(sb, "Spacing", epSpacingRewardTotal, epSpacingCount);
        AppendLine(sb, "VerticalCheese", epStackPenaltyTotal, epStackPenaltyCount);
        AppendLine(sb, "Mash", epMashPenaltyTotal, epMashPenaltyCount);
        AppendLine(sb, "AirJump", epAirJumpPenaltyTotal, epAirJumpPenaltyCount);
        AppendLine(sb, "EdgeCamp", epEdgeCampPenaltyTotal, epEdgeCampPenaltyCount);
        AppendLine(sb, "Approach", epApproachRewardTotal, epApproachRewardCount);
        AppendLine(sb, "ForwardPressure", epForwardPressureRewardTotal, epForwardPressureRewardCount);
        AppendLine(sb, "ClosePressure", epClosePressureRewardTotal, epClosePressureRewardCount);
        AppendLine(sb, "RetreatFromClose", epRetreatFromClosePenaltyTotal, epRetreatFromClosePenaltyCount);
        AppendLine(sb, "UsefulIntent", epUsefulIntentRewardTotal, epUsefulIntentRewardCount);
        AppendLine(sb, "PassiveNear", epPassiveNearPenaltyTotal, epPassiveNearPenaltyCount);
        AppendLine(sb, "BodyPush", epBodyPushPenaltyTotal, epBodyPushPenaltyCount);
        AppendLine(sb, "ParryOpportunity", epParryOpportunityRewardTotal, epParryOpportunityRewardCount);
        AppendLine(sb, "UnsafeParry", epUnsafeParryPenaltyTotal, epUnsafeParryPenaltyCount);
        AppendLine(sb, "ObviousPunish", epObviousPunishRewardTotal, epObviousPunishRewardCount);
        AppendLine(sb, "MissedPunish", epMissedPunishPenaltyTotal, epMissedPunishPenaltyCount);
        AppendLine(sb, "ChargePunish", epChargePunishRewardTotal, epChargePunishRewardCount);
        AppendLine(sb, "MissedChargeSpecial", epMissedChargeSpecialPenaltyTotal, epMissedChargeSpecialPenaltyCount);
        AppendLine(sb, "ChargeRunaway", epChargeRunawayPenaltyTotal, epChargeRunawayPenaltyCount);
        AppendLine(sb, "LowHealthPanic", epLowHealthPanicPenaltyTotal, epLowHealthPanicPenaltyCount);
        AppendLine(sb, "ExtremeFarHeavy", epExtremeFarHeavyPenaltyTotal, epExtremeFarHeavyPenaltyCount);
        AppendLine(sb, "ExtremeFarCharge", epExtremeFarChargePenaltyTotal, epExtremeFarChargePenaltyCount);
        AppendLine(sb, "FarMeleeLight", epFarMeleeLightPenaltyTotal, epFarMeleeLightPenaltyCount);
        AppendLine(sb, "FarMeleeSpecial", epFarMeleeSpecialPenaltyTotal, epFarMeleeSpecialPenaltyCount);
        AppendLine(sb, "DashNoDirection", epDashNoDirectionPenaltyTotal, epDashNoDirectionPenaltyCount);
        AppendLine(sb, "WrongFacingSpecial", epWrongFacingSpecialPenaltyTotal, epWrongFacingSpecialPenaltyCount);
        AppendLine(sb, "BlockHold", epBlockHoldPenaltyTotal, epBlockHoldPenaltyCount);
        AppendLine(sb, "RepeatSameMove", epRepeatSameMovePenaltyTotal, epRepeatSameMovePenaltyCount);
        AppendLine(sb, "ChargeSpam", epChargeSpamPenaltyTotal, epChargeSpamPenaltyCount);
        AppendLine(sb, "EmptyChargeRelease", epEmptyChargeReleasePenaltyTotal, epEmptyChargeReleasePenaltyCount);
        AppendLine(sb, "SafetyReset", epSafetyResetPenaltyTotal, epSafetyResetPenaltyCount);

        Debug.Log(sb.ToString());
    }

    private void AppendLine(StringBuilder sb, string label, float total, int count)
    {
        if (count <= 0 && Mathf.Approximately(total, 0f))
        {
            return;
        }

        sb.AppendLine($"  {label}: total={total:F4}, count={count}");
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
        epForwardPressureRewardTotal = 0f;
        epClosePressureRewardTotal = 0f;
        epRetreatFromClosePenaltyTotal = 0f;
        epUsefulIntentRewardTotal = 0f;
        epPassiveNearPenaltyTotal = 0f;
        epBodyPushPenaltyTotal = 0f;
        epParryOpportunityRewardTotal = 0f;
        epUnsafeParryPenaltyTotal = 0f;
        epObviousPunishRewardTotal = 0f;
        epMissedPunishPenaltyTotal = 0f;
        epChargePunishRewardTotal = 0f;
        epMissedChargeSpecialPenaltyTotal = 0f;
        epChargeRunawayPenaltyTotal = 0f;
        epLowHealthPanicPenaltyTotal = 0f;
        epExtremeFarHeavyPenaltyTotal = 0f;
        epExtremeFarChargePenaltyTotal = 0f;
        epFarMeleeLightPenaltyTotal = 0f;
        epFarMeleeSpecialPenaltyTotal = 0f;
        epDashNoDirectionPenaltyTotal = 0f;
        epWrongFacingSpecialPenaltyTotal = 0f;
        epBlockHoldPenaltyTotal = 0f;
        epRepeatSameMovePenaltyTotal = 0f;
        epChargeSpamPenaltyTotal = 0f;
        epEmptyChargeReleasePenaltyTotal = 0f;
        epSafetyResetPenaltyTotal = 0f;
        epWinRewardTotal = 0f;
        epLossRewardTotal = 0f;

        epStepPenaltyCount = 0;
        epDamageDealtCount = 0;
        epDamageTakenCount = 0;
        epSpacingCount = 0;
        epStackPenaltyCount = 0;
        epMashPenaltyCount = 0;
        epAirJumpPenaltyCount = 0;
        epEdgeCampPenaltyCount = 0;
        epApproachRewardCount = 0;
        epForwardPressureRewardCount = 0;
        epClosePressureRewardCount = 0;
        epRetreatFromClosePenaltyCount = 0;
        epUsefulIntentRewardCount = 0;
        epPassiveNearPenaltyCount = 0;
        epBodyPushPenaltyCount = 0;
        epParryOpportunityRewardCount = 0;
        epUnsafeParryPenaltyCount = 0;
        epObviousPunishRewardCount = 0;
        epMissedPunishPenaltyCount = 0;
        epChargePunishRewardCount = 0;
        epMissedChargeSpecialPenaltyCount = 0;
        epChargeRunawayPenaltyCount = 0;
        epLowHealthPanicPenaltyCount = 0;
        epExtremeFarHeavyPenaltyCount = 0;
        epExtremeFarChargePenaltyCount = 0;
        epFarMeleeLightPenaltyCount = 0;
        epFarMeleeSpecialPenaltyCount = 0;
        epDashNoDirectionPenaltyCount = 0;
        epWrongFacingSpecialPenaltyCount = 0;
        epBlockHoldPenaltyCount = 0;
        epRepeatSameMovePenaltyCount = 0;
        epChargeSpamPenaltyCount = 0;
        epEmptyChargeReleasePenaltyCount = 0;
        epSafetyResetPenaltyCount = 0;
        epWinRewardCount = 0;
        epLossRewardCount = 0;
    }
}
