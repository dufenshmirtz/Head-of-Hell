using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public enum ReachType
{
    Melee,
    Dash,
    Ranged,
    Global
}

[RequireComponent(typeof(CharacterManager))]
public class FighterAgent : Agent
{
    [Header("Managers")]
    public CharacterManager selfManager;
    public CharacterManager enemyManager;

    [Header("Input")]
    public string playerSuffix = "_P1";
    private AIInputProvider aiInput;

    // live pointers
    private Character self;
    private Character opp;

    // cache keys for Heuristic
    KeyCode upK, downK, leftK, rightK, lightK, heavyK, blockK, abilityK, chargeK, parryK;

    [Header("Main Rewards")]
    [SerializeField] float rewardDamageDealt = +0.02f;
    [SerializeField] float rewardDamageTaken = -0.02f;
    [SerializeField] float rewardWin = +1.0f;
    [SerializeField] float rewardLoss = -1.0f;
    [SerializeField] float stepPenalty = -0.0001f;

    [Header("Minimal Spacing Shaping")]
    [SerializeField] float spacingBonus = +0.0003f;

    [Tooltip("Useful horizontal spacing for common melee attacks.")]
    [SerializeField] float usefulRangeMinX = 0.4f;

    [Tooltip("Useful horizontal spacing for common melee attacks.")]
    [SerializeField] float usefulRangeMaxX = 0.90f;

    [Tooltip("Useful vertical spacing for common melee attacks.")]
    [SerializeField] float usefulRangeMaxY = 0.50f;

    [Tooltip("Penalty when agents end up in degenerate stacked states.")]
    [SerializeField] float stackPenalty = -0.00015f;

    [Tooltip("Very small horizontal gap -> likely overlap/stack exploit.")]
    [SerializeField] float stackBadX = 0.2f;

    [Tooltip("Minimum vertical offset for bad head-stack detection.")]
    [SerializeField] float stackBadMinY = 0.8f;

    [Tooltip("Maximum vertical offset for bad head-stack detection.")]
    [SerializeField] float stackBadMaxY = 1.2f;

    [Header("Observation scales")]
    [SerializeField] float relXScale = 9f;
    [SerializeField] float relYScale = 5f;
    [SerializeField] float velScale = 10f;

    [SerializeField] int totalCharacterCount = 10;

    [Header("Behavior Hygiene")]
    [SerializeField] float mashPenalty = -0.0006f;
    [SerializeField] float airJumpPenalty = -0.0008f;
    [SerializeField] float edgeCampPenalty = -0.0007f;

    [Tooltip("How many consecutive action changes before we start punishing noisy mashing.")]
    [SerializeField] int mashChangeThreshold = 3;

    [Tooltip("World X beyond which we consider the fighter near the edge.")]
    [SerializeField] float edgeZoneX = 8f;

    [Tooltip("How long (seconds) the fighter can stay near the edge before mild penalty starts.")]
    [SerializeField] float edgeGraceTime = 1.75f;

    [Tooltip("Small x movement range considered 'camping in place'.")]
    [SerializeField] float edgeSmallMoveThreshold = 0.35f;

    [Header("Move Semantics")]
    [SerializeField] private ReachType lightReachType = ReachType.Melee;
    [SerializeField] private ReachType specialReachType = ReachType.Melee;

    [Header("Range Logic")]
    [SerializeField] float extremeFarThreshold = 8.5f;

    [Tooltip("Tiny penalty for using heavy from absurdly far away.")]
    [SerializeField] float extremeFarHeavyPenalty = -0.0007f;

    [Tooltip("Tiny penalty for using charge from absurdly far away.")]
    [SerializeField] float extremeFarChargePenalty = -0.0006f;

    [Tooltip("Reward for reducing distance when clearly outside melee threat range.")]
    [SerializeField] float approachBonus = +0.00025f;

    [Tooltip("Extra margin beyond useful melee range before approach shaping starts.")]
    [SerializeField] float approachStartMargin = 0.75f;

    [Tooltip("Tiny penalty for using clearly melee light from absurdly far away.")]
    [SerializeField] float farMeleeLightPenalty = -0.00035f;

    [Tooltip("Tiny penalty for using clearly melee special from absurdly far away.")]
    [SerializeField] float farMeleeSpecialPenalty = -0.00035f;

    [Header("Directional Hygiene")]
    [Tooltip("Tiny penalty for using a Dash-type move without horizontal direction input.")]
    [SerializeField] float dashNoDirectionPenalty = -0.00005f;

    [Tooltip("Tiny penalty for using special while not facing the opponent.")]
    [SerializeField] float wrongFacingSpecialPenalty = -0.0005f;

    private FighterAgentRewardDebugger rewardDebugger;

    // bookkeeping
    int lastSelfHP, lastOppHP;
    int lastMoveX = 0;

    int lastActionIntent = 0;
    int consecutiveActionChanges = 0;

    float edgeStayTimer = 0f;
    float edgeAnchorX = 0f;
    bool edgeAnchorInitialized = false;
    float lastAbsDx = 0f;
    bool profileLoaded = false;

    int lastLightAction = 0;
    int lastSpecialAction = 0;
    int lastJumpAction = 0;

    // optional
    FighterAgent oppAgent;

    void Start()
    {
        TryBindNow();
    }

    void Update()
    {
        if (self == null || opp == null)
        {
            TryBindNow();
        }

        if (self != null && !profileLoaded)
        {
            RefreshCharacterProfile();
        }
    }

    void TryBindNow()
    {
        if (self == null && selfManager != null)
        {
            var c = selfManager.CharacterChoice(1);
            if (c != null)
            {
                BindSelf(c);
                Debug.Log($"[Agent {playerSuffix}] Bound SELF: {c.name}");
            }
        }

        if (opp == null && enemyManager != null)
        {
            var e = enemyManager.CharacterChoice(1);
            if (e != null)
            {
                BindEnemy(e);
                Debug.Log($"[Agent {playerSuffix}] Bound OPP: {e.name}");
            }
        }
    }

    public override void Initialize()
    {
        if (!selfManager)
        {
            selfManager = GetComponent<CharacterManager>();
        }

        aiInput = new AIInputProvider(playerSuffix);

        selfManager.OnCharacterReady += BindSelf;
        selfManager.OnCharacterChanged += BindSelf;

        if (enemyManager != null)
        {
            enemyManager.OnCharacterReady += BindEnemy;
            enemyManager.OnCharacterChanged += BindEnemy;
        }

        rewardDebugger = GetComponent<FighterAgentRewardDebugger>();
    }

    private void BindSelf(Character c)
    {
        self = c;
        if (self == null)
        {
            return;
        }

        var setup = self.GetComponent<CharacterSetup>();

        upK = setup.up;
        downK = setup.down;
        leftK = setup.left;
        rightK = setup.right;
        lightK = setup.lightAttack;
        heavyK = setup.heavyAttack;
        blockK = setup.block;
        abilityK = setup.ability;
        chargeK = setup.charge;
        parryK = setup.parry;

        if (aiInput == null)
        {
            aiInput = new AIInputProvider(playerSuffix);
        }

        aiInput.SetKeys(leftK, rightK, upK, downK, lightK, heavyK, blockK, abilityK, chargeK, parryK);

        self.SetInput(aiInput);

        lastSelfHP = self.GetCurrentHealth();

        RefreshCharacterProfile();
    }

    private void BindEnemy(Character c)
    {
        opp = c;

        if (opp != null)
        {
            lastOppHP = opp.GetCurrentHealth();
        }

        if (self != null && opp != null)
        {
            lastAbsDx = Mathf.Abs(opp.transform.position.x - self.transform.position.x);
        }

        if (enemyManager != null)
        {
            oppAgent = enemyManager.GetComponent<FighterAgent>();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (!self || !opp)
        {
            for (int i = 0; i < 67; i++)
            {
                sensor.AddObservation(0f);
            }
            return;
        }

        var rb = self.GetComponent<Rigidbody2D>();
        var orb = opp.GetComponent<Rigidbody2D>();

        Vector2 vel = rb ? rb.velocity : Vector2.zero;
        Vector2 ovel = orb ? orb.velocity : Vector2.zero;
        Vector2 rel = (Vector2)(opp.transform.position - self.transform.position);

        float nx = Mathf.Clamp(rel.x / relXScale, -1f, 1f);
        float ny = Mathf.Clamp(rel.y / relYScale, -1f, 1f);
        float absDxNorm = Mathf.Clamp(Mathf.Abs(rel.x) / relXScale, 0f, 1f);
        float absDyNorm = Mathf.Clamp(Mathf.Abs(rel.y) / relYScale, 0f, 1f);

        float facingSign = Mathf.Sign(self.transform.localScale.x);
        float oppDirSign = Mathf.Sign(rel.x);
        float facingCorrectly = (facingSign == oppDirSign) ? 1f : 0f;

        sensor.AddObservation(nx);
        sensor.AddObservation(ny);

        sensor.AddObservation(absDxNorm);
        sensor.AddObservation(absDyNorm);

        sensor.AddObservation(Mathf.Clamp(vel.x / velScale, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(vel.y / velScale, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(ovel.x / velScale, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(ovel.y / velScale, -1f, 1f));

        sensor.AddObservation(self.GetCurrentHealth() / 100f);
        sensor.AddObservation(opp.GetCurrentHealth() / 100f);

        sensor.AddOneHotObservation(self.characterID, totalCharacterCount);
        sensor.AddOneHotObservation(opp.characterID, totalCharacterCount);

        sensor.AddObservation(self.IsGrounded);
        sensor.AddObservation(self.IsBlocking);
        sensor.AddObservation(self.IsCasting);
        sensor.AddObservation(self.IsStunned);
        sensor.AddObservation(self.IsKnocked);
        sensor.AddObservation(self.IsCharging);
        sensor.AddObservation(self.IsCharged);
        sensor.AddObservation(self.OnAbilityCD);
        sensor.AddObservation(self.AbilityCooldown01);
        sensor.AddObservation(self.CanCast);
        sensor.AddObservation(self.CanParry);
        sensor.AddObservation(self.LightAttacking);
        sensor.AddObservation(self.HeavyAttacking);
        sensor.AddObservation(self.Parrying);

        sensor.AddObservation(self.QuickDisabled);
        sensor.AddObservation(self.HeavyDisabled);
        sensor.AddObservation(self.BlockDisabled);
        sensor.AddObservation(self.SpecialDisabled);
        sensor.AddObservation(self.ChargeDisabled);
        sensor.AddObservation(self.JumpDisabled);

        sensor.AddObservation(opp.IsGrounded);
        sensor.AddObservation(opp.IsBlocking);
        sensor.AddObservation(opp.IsCasting);
        sensor.AddObservation(opp.IsStunned);
        sensor.AddObservation(opp.IsKnocked);
        sensor.AddObservation(opp.IsCharging);
        sensor.AddObservation(opp.IsCharged);
        sensor.AddObservation(opp.OnAbilityCD);
        sensor.AddObservation(opp.AbilityCooldown01);
        sensor.AddObservation(opp.CanCast);
        sensor.AddObservation(opp.CanParry);
        sensor.AddObservation(opp.LightAttacking);
        sensor.AddObservation(opp.HeavyAttacking);
        sensor.AddObservation(opp.Parrying);

        sensor.AddObservation(oppDirSign);
        sensor.AddObservation(facingSign);
        sensor.AddObservation(facingCorrectly);
    }

    void ShapingRewards()
    {
        if (self == null || opp == null)
        {
            return;
        }

        if (GameManager.instance == null || !GameManager.instance.roundOn)
        {
            return;
        }

        float absDx = Mathf.Abs(opp.transform.position.x - self.transform.position.x);
        float absDy = Mathf.Abs(opp.transform.position.y - self.transform.position.y);

        bool inUsefulRange =
            absDx >= usefulRangeMinX &&
            absDx <= usefulRangeMaxX &&
            absDy <= usefulRangeMaxY;

        if (inUsefulRange)
        {
            AddReward(spacingBonus);
            rewardDebugger?.LogSpacing(spacingBonus);
        }

        bool badStack =
            absDx < stackBadX &&
            absDy > stackBadMinY &&
            absDy < stackBadMaxY;

        if (badStack)
        {
            AddReward(stackPenalty);
            rewardDebugger?.LogStackPenalty(stackPenalty);
        }
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        if (self == null)
        {
            return;
        }

        bool locked = self.IsStunned || self.IsCasting;

        if (self.IsCharging)
        {
            actionMask.SetActionEnabled(1, 1, false);
            actionMask.SetActionEnabled(2, 1, false);
            actionMask.SetActionEnabled(3, 1, false);
            actionMask.SetActionEnabled(4, 1, false);
            actionMask.SetActionEnabled(5, 1, false);
            actionMask.SetActionEnabled(6, 1, false);
            actionMask.SetActionEnabled(8, 1, false);

            actionMask.SetActionEnabled(0, 0, false);
            actionMask.SetActionEnabled(0, 2, false);

            if (self.IsCharged)
            {
                actionMask.SetActionEnabled(7, 0, false);
                actionMask.SetActionEnabled(7, 1, false);
            }
            else
            {
                actionMask.SetActionEnabled(7, 0, false);
            }

            return;
        }

        bool canJumpNow = self.IsGrounded;
        if (!canJumpNow || locked || self.JumpDisabled)
        {
            actionMask.SetActionEnabled(1, 1, false);
        }

        if (!self.IsGrounded || locked)
        {
            actionMask.SetActionEnabled(2, 1, false);
        }

        if (locked || self.QuickDisabled)
        {
            actionMask.SetActionEnabled(3, 1, false);
        }

        if (locked || self.HeavyDisabled)
        {
            actionMask.SetActionEnabled(4, 1, false);
        }

        if (locked || self.BlockDisabled)
        {
            actionMask.SetActionEnabled(5, 1, false);
        }

        if (locked || self.OnAbilityCD || !self.CanCast || self.SpecialDisabled)
        {
            actionMask.SetActionEnabled(6, 1, false);
        }

        bool canChargeNow = !locked && !self.ChargeDisabled;
        if (!canChargeNow)
        {
            actionMask.SetActionEnabled(7, 1, false);
            actionMask.SetActionEnabled(7, 2, false);
        }

        if (locked || !self.CanParry)
        {
            actionMask.SetActionEnabled(8, 1, false);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!self)
        {
            return;
        }

        if (GameManager.instance == null || !GameManager.instance.roundOn)
        {
            return;
        }

        int moveBranch = actions.DiscreteActions[0];
        int jump = actions.DiscreteActions[1];
        int drop = actions.DiscreteActions[2];
        int light = actions.DiscreteActions[3];
        int heavy = actions.DiscreteActions[4];
        int blockHold = actions.DiscreteActions[5];
        int special = actions.DiscreteActions[6];
        int chargeMode = actions.DiscreteActions[7];
        int parry = actions.DiscreteActions[8];

        int moveX = moveBranch == 0 ? -1 : (moveBranch == 1 ? 0 : 1);
        lastMoveX = moveX;

        var cmd = new AIInputProvider.Command
        {
            moveX = moveX,
            jump = (jump == 1),
            drop = (drop == 1),
            light = (light == 1),
            heavy = (heavy == 1),
            blockHold = (blockHold == 1),
            special = (special == 1),
            chargeHold = (chargeMode == 1),
            chargeRelease = (chargeMode == 2),
            parry = (parry == 1)
        };

        aiInput.Apply(cmd);

        AddReward(stepPenalty);
        rewardDebugger?.LogStepPenalty(stepPenalty);

        int selfHP = self.GetCurrentHealth();
        int oppHP = (opp != null) ? opp.GetCurrentHealth() : lastOppHP;

        int dealt = Mathf.Max(0, lastOppHP - oppHP);
        int taken = Mathf.Max(0, lastSelfHP - selfHP);

        float dealtReward = dealt * rewardDamageDealt;
        float takenReward = taken * rewardDamageTaken;

        AddReward(dealtReward);
        AddReward(takenReward);

        rewardDebugger?.LogDamageDealt(dealtReward);
        rewardDebugger?.LogDamageTaken(takenReward);

        lastSelfHP = selfHP;
        lastOppHP = oppHP;

        ShapingRewards();
        BehaviorHygieneRewards(jump, drop, light, heavy, blockHold, special, chargeMode, parry);
        TacticalRangeRewards(light, heavy, special, chargeMode);
        DirectionalHygieneRewards(moveX, light, special);

        if (opp != null && oppHP <= 0)
        {
            AddReward(rewardWin);
            rewardDebugger?.LogWinReward(rewardWin);
            return;
        }

        if (selfHP <= 0)
        {
            AddReward(rewardLoss);
            rewardDebugger?.LogLossReward(rewardLoss);
            return;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var d = actionsOut.DiscreteActions;

        d[0] = 1;
        if (Input.GetKey(leftK))
        {
            d[0] = 0;
        }
        if (Input.GetKey(rightK))
        {
            d[0] = 2;
        }

        d[1] = Input.GetKey(upK) ? 1 : 0;
        d[2] = Input.GetKey(downK) ? 1 : 0;

        d[3] = Input.GetKey(lightK) ? 1 : 0;
        d[4] = Input.GetKey(heavyK) ? 1 : 0;
        d[5] = Input.GetKey(blockK) ? 1 : 0;
        d[6] = Input.GetKey(abilityK) ? 1 : 0;

        if (Input.GetKeyUp(chargeK))
        {
            d[7] = 2;
        }
        else if (Input.GetKey(chargeK))
        {
            d[7] = 1;
        }
        else
        {
            d[7] = 0;
        }

        d[8] = Input.GetKey(parryK) ? 1 : 0;
    }

    public override void OnEpisodeBegin()
    {
        TryBindNow();

        if (self == null || opp == null)
        {
            return;
        }

        rewardDebugger?.BeginEpisode(
            playerSuffix,
            CurrentCharacterIdOrNull(),
            lightReachType,
            specialReachType,
            profileLoaded
        );

        lastSelfHP = self.GetCurrentHealth();
        lastOppHP = opp.GetCurrentHealth();
        lastMoveX = 0;

        if (self != null && opp != null)
        {
            lastAbsDx = Mathf.Abs(opp.transform.position.x - self.transform.position.x);
        }
        else
        {
            lastAbsDx = 0f;
        }

        lastActionIntent = 0;
        consecutiveActionChanges = 0;

        edgeStayTimer = 0f;
        edgeAnchorX = 0f;
        edgeAnchorInitialized = false;

        if (aiInput != null)
        {
            aiInput.Apply(new AIInputProvider.Command
            {
                moveX = 0,
                jump = false,
                drop = false,
                light = false,
                heavy = false,
                blockHold = false,
                special = false,
                chargeHold = false,
                chargeRelease = false,
                parry = false
            });
        }

        lastLightAction = 0;
        lastSpecialAction = 0;
        lastJumpAction = 0;
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
    }

    void BehaviorHygieneRewards(int jump, int drop, int light, int heavy, int blockHold, int special, int chargeMode, int parry)
    {
        if (self == null || opp == null)
        {
            return;
        }

        if (GameManager.instance == null || !GameManager.instance.roundOn)
        {
            return;
        }

        int currentIntent = GetActionIntent(jump, drop, light, heavy, blockHold, special, chargeMode, parry);

        if (currentIntent != 0 && lastActionIntent != 0 && currentIntent != lastActionIntent)
        {
            consecutiveActionChanges++;
        }
        else if (currentIntent == 0 || currentIntent == lastActionIntent)
        {
            consecutiveActionChanges = 0;
        }

        if (consecutiveActionChanges >= mashChangeThreshold)
        {
            AddReward(mashPenalty);
            rewardDebugger?.LogMashPenalty(mashPenalty);
        }

        lastActionIntent = currentIntent;

        bool jumpPressedNow = (jump == 1 && lastJumpAction == 0);
        if (jumpPressedNow && !self.IsGrounded)
        {
            AddReward(airJumpPenalty);
            rewardDebugger?.LogAirJumpPenalty(airJumpPenalty);
        }

        float x = self.transform.position.x;
        bool nearEdge = Mathf.Abs(x) >= edgeZoneX;

        if (nearEdge)
        {
            if (!edgeAnchorInitialized)
            {
                edgeAnchorInitialized = true;
                edgeAnchorX = x;
                edgeStayTimer = 0f;
            }

            float movedFromAnchor = Mathf.Abs(x - edgeAnchorX);

            if (movedFromAnchor <= edgeSmallMoveThreshold)
            {
                edgeStayTimer += Time.fixedDeltaTime;

                if (edgeStayTimer > edgeGraceTime)
                {
                    AddReward(edgeCampPenalty);
                    rewardDebugger?.LogEdgeCampPenalty(edgeCampPenalty);
                }
            }
            else
            {
                edgeAnchorX = x;
                edgeStayTimer = 0f;
            }
        }
        else
        {
            edgeAnchorInitialized = false;
            edgeStayTimer = 0f;
        }

        lastJumpAction = jump;
    }

    int GetActionIntent(int jump, int drop, int light, int heavy, int blockHold, int special, int chargeMode, int parry)
    {
        if (parry == 1)
        {
            return 7;
        }

        if (chargeMode != 0)
        {
            return 6;
        }

        if (special == 1)
        {
            return 5;
        }

        if (blockHold == 1)
        {
            return 4;
        }

        if (heavy == 1)
        {
            return 3;
        }

        if (light == 1)
        {
            return 2;
        }

        if (jump == 1 || drop == 1)
        {
            return 1;
        }

        return 0;
    }

    bool IsStrictMelee(ReachType reachType)
    {
        return reachType == ReachType.Melee;
    }

    void TacticalRangeRewards(int light, int heavy, int special, int chargeMode)
    {
        if (self == null || opp == null)
        {
            return;
        }

        if (GameManager.instance == null || !GameManager.instance.roundOn)
        {
            return;
        }

        float absDx = Mathf.Abs(opp.transform.position.x - self.transform.position.x);
        float absDy = Mathf.Abs(opp.transform.position.y - self.transform.position.y);

        float approachStartDistance = usefulRangeMaxX + approachStartMargin;
        bool farFromOpponent = absDx > approachStartDistance;

        if (farFromOpponent && absDx < lastAbsDx)
        {
            AddReward(approachBonus);
            rewardDebugger?.LogApproachReward(approachBonus);
        }

        bool absurdlyFar = absDx >= extremeFarThreshold;

        if (absurdlyFar)
        {
            if (heavy == 1)
            {
                AddReward(extremeFarHeavyPenalty);
                rewardDebugger?.LogExtremeFarHeavyPenalty(extremeFarHeavyPenalty);
            }

            if (chargeMode == 1)
            {
                AddReward(extremeFarChargePenalty);
                rewardDebugger?.LogExtremeFarChargePenalty(extremeFarChargePenalty);
            }

            if (light == 1 && IsStrictMelee(lightReachType))
            {
                Debug.Log($"[FarMeleeLight TRIGGER] CharID={self.characterID} LightReach={lightReachType} absDx={absDx}");
                AddReward(farMeleeLightPenalty);
                rewardDebugger?.LogFarMeleeLightPenalty(farMeleeLightPenalty);
            }

            if (special == 1 && IsStrictMelee(specialReachType))
            {
                Debug.Log($"[FarMeleeSpecial TRIGGER] CharID={self.characterID} SpecialReach={specialReachType} absDx={absDx}");
                AddReward(farMeleeSpecialPenalty);
                rewardDebugger?.LogFarMeleeSpecialPenalty(farMeleeSpecialPenalty);
            }
        }

        lastAbsDx = absDx;
    }

    void RefreshCharacterProfile()
    {
        lightReachType = ReachType.Melee;
        specialReachType = ReachType.Melee;
        profileLoaded = false;

        if (self == null)
        {
            return;
        }

        if (self.characterID < 0)
        {
            return;
        }

        if (CharacterMLProfileDatabase.Instance == null)
        {
            return;
        }

        var profile = CharacterMLProfileDatabase.Instance.GetProfileByID(self.characterID);
        if (profile == null)
        {
            return;
        }

        lightReachType = profile.lightReachType;
        specialReachType = profile.specialReachType;
        profileLoaded = true;
    }

    bool IsDashType(ReachType reachType)
    {
        return reachType == ReachType.Dash;
    }

    bool IsFacingOpponent()
    {
        if (self == null || opp == null)
        {
            return true;
        }

        float relX = opp.transform.position.x - self.transform.position.x;
        float oppDirSign = Mathf.Sign(relX);
        float facingSign = Mathf.Sign(self.transform.localScale.x);

        return facingSign == oppDirSign;
    }

    void DirectionalHygieneRewards(int moveX, int light, int special)
    {
        if (self == null || opp == null)
        {
            return;
        }

        if (GameManager.instance == null || !GameManager.instance.roundOn)
        {
            return;
        }

        bool lightPressedNow = (light == 1 && lastLightAction == 0);
        bool specialPressedNow = (special == 1 && lastSpecialAction == 0);

        if (moveX == 0)
        {
            if (lightPressedNow && IsDashType(lightReachType))
            {
                AddReward(dashNoDirectionPenalty);
                rewardDebugger?.LogDashNoDirectionPenalty(dashNoDirectionPenalty);
            }

            if (specialPressedNow && IsDashType(specialReachType))
            {
                AddReward(dashNoDirectionPenalty);
                rewardDebugger?.LogDashNoDirectionPenalty(dashNoDirectionPenalty);
            }
        }

        bool facingOpponent = IsFacingOpponent();

        if (specialPressedNow && !facingOpponent && specialReachType != ReachType.Global)
        {
            AddReward(wrongFacingSpecialPenalty);
            rewardDebugger?.LogWrongFacingSpecialPenalty(wrongFacingSpecialPenalty);
        }

        lastLightAction = light;
        lastSpecialAction = special;
    }

    public void ClearInput()
    {
        if (aiInput != null)
        {
            aiInput.Apply(new AIInputProvider.Command
            {
                moveX = 0,
                jump = false,
                drop = false,
                light = false,
                heavy = false,
                blockHold = false,
                special = false,
                chargeHold = false,
                chargeRelease = false,
                parry = false
            });
        }
    }

    public void ForceRebind()
    {
        self = null;
        opp = null;
        profileLoaded = false;
        TryBindNow();
    }

    int? CurrentCharacterIdOrNull()
    {
        return self != null ? self.characterID : (int?)null;
    }

    public void DebugEndEpisode(string reason)
    {
        rewardDebugger?.EndEpisode(
            playerSuffix,
            reason,
            CurrentCharacterIdOrNull(),
            lightReachType,
            specialReachType,
            profileLoaded
        );
    }
}