using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

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
    [SerializeField] float usefulRangeMinX = 0.35f;

    [Tooltip("Useful horizontal spacing for common melee attacks.")]
    [SerializeField] float usefulRangeMaxX = 0.90f;

    [Tooltip("Useful vertical spacing for common melee attacks.")]
    [SerializeField] float usefulRangeMaxY = 0.50f;

    [Tooltip("Penalty when agents end up in degenerate stacked states.")]
    [SerializeField] float stackPenalty = -0.0015f;

    [Tooltip("Very small horizontal gap -> likely overlap/stack exploit.")]
    [SerializeField] float stackBadX = 0.20f;

    [Tooltip("Minimum vertical offset for bad head-stack detection.")]
    [SerializeField] float stackBadMinY = 0.30f;

    [Tooltip("Maximum vertical offset for bad head-stack detection.")]
    [SerializeField] float stackBadMaxY = 1.20f;

    [Header("Observation scales")]
    [SerializeField] float relXScale = 9f;
    [SerializeField] float relYScale = 5f;
    [SerializeField] float velScale = 10f;

    [SerializeField] int totalCharacterCount = 10;

    // bookkeeping
    int lastSelfHP, lastOppHP;
    int lastMoveX = 0;

    // optional
    FighterAgent oppAgent;

    void Start()
    {
        TryBindNow();
    }

    void Update()
    {
        if (self == null || opp == null)
            TryBindNow();
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
            var e = enemyManager.CharacterChoice(2);
            if (e != null)
            {
                BindEnemy(e);
                Debug.Log($"[Agent {playerSuffix}] Bound OPP: {e.name}");
            }
        }
    }

    public override void Initialize()
    {
        if (!selfManager) selfManager = GetComponent<CharacterManager>();

        aiInput = new AIInputProvider(playerSuffix);

        selfManager.OnCharacterReady += BindSelf;
        selfManager.OnCharacterChanged += BindSelf;

        if (enemyManager != null)
        {
            enemyManager.OnCharacterReady += BindEnemy;
            enemyManager.OnCharacterChanged += BindEnemy;
        }
    }

    private void BindSelf(Character c)
    {
        self = c;
        if (self == null) return;

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

        if (aiInput == null) aiInput = new AIInputProvider(playerSuffix);
        aiInput.SetKeys(leftK, rightK, upK, downK, lightK, heavyK, blockK, abilityK, chargeK, parryK);

        self.SetInput(aiInput);

        lastSelfHP = self.GetCurrentHealth();
    }

    private void BindEnemy(Character c)
    {
        opp = c;
        if (opp != null) lastOppHP = opp.GetCurrentHealth();

        if (enemyManager != null)
            oppAgent = enemyManager.GetComponent<FighterAgent>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (!self || !opp)
        {
            // keep count stable
            for (int i = 0; i < 61; i++)
                sensor.AddObservation(0f);
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

        // relative position
        sensor.AddObservation(nx);
        sensor.AddObservation(ny);

        // absolute spacing helpers
        sensor.AddObservation(absDxNorm);
        sensor.AddObservation(absDyNorm);

        // velocities
        sensor.AddObservation(Mathf.Clamp(vel.x / velScale, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(vel.y / velScale, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(ovel.x / velScale, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(ovel.y / velScale, -1f, 1f));

        // health
        sensor.AddObservation(self.GetCurrentHealth() / 100f);
        sensor.AddObservation(opp.GetCurrentHealth() / 100f);

        // one-hot character ids
        sensor.AddOneHotObservation(self.characterID, totalCharacterCount);
        sensor.AddOneHotObservation(opp.characterID, totalCharacterCount);

        // self state
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

        // self disabled flags
        sensor.AddObservation(self.QuickDisabled);
        sensor.AddObservation(self.HeavyDisabled);
        sensor.AddObservation(self.BlockDisabled);
        sensor.AddObservation(self.SpecialDisabled);
        sensor.AddObservation(self.ChargeDisabled);
        sensor.AddObservation(self.JumpDisabled);

        // opponent state
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

        // facing hints
        sensor.AddObservation(oppDirSign);
        sensor.AddObservation(facingSign);
        sensor.AddObservation(facingCorrectly);
    }

    void ShapingRewards()
    {
        if (self == null || opp == null) return;

        float absDx = Mathf.Abs(opp.transform.position.x - self.transform.position.x);
        float absDy = Mathf.Abs(opp.transform.position.y - self.transform.position.y);

        // tiny reward for useful melee spacing
        bool inUsefulRange =
            absDx >= usefulRangeMinX &&
            absDx <= usefulRangeMaxX &&
            absDy <= usefulRangeMaxY;

        if (inUsefulRange)
            AddReward(spacingBonus);

        // punish standing on top of opponent / head-stacking exploit
        bool badStack =
            absDx < stackBadX &&
            absDy > stackBadMinY &&
            absDy < stackBadMaxY;

        if (badStack)
            AddReward(stackPenalty);
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        if (self == null) return;

        // Branch layout:
        // 0: Move (3)      left / idle / right
        // 1: Jump (2)
        // 2: Drop (2)
        // 3: Light (2)
        // 4: Heavy (2)
        // 5: Block (2)
        // 6: Special (2)
        // 7: Charge (3)    none / hold / release
        // 8: Parry (2)

        bool locked = self.IsStunned || self.IsCasting;

        // CHARGE lock logic
        if (self.IsCharging)
        {
            actionMask.SetActionEnabled(1, 1, false); // jump
            actionMask.SetActionEnabled(2, 1, false); // drop
            actionMask.SetActionEnabled(3, 1, false); // light
            actionMask.SetActionEnabled(4, 1, false); // heavy
            actionMask.SetActionEnabled(5, 1, false); // block
            actionMask.SetActionEnabled(6, 1, false); // special
            actionMask.SetActionEnabled(8, 1, false); // parry

            // move -> only idle
            actionMask.SetActionEnabled(0, 0, false); // left
            actionMask.SetActionEnabled(0, 2, false); // right

            if (self.IsCharged)
            {
                // only release
                actionMask.SetActionEnabled(7, 0, false); // none
                actionMask.SetActionEnabled(7, 1, false); // hold
            }
            else
            {
                // allow hold or release, disable none
                actionMask.SetActionEnabled(7, 0, false);
            }

            return;
        }

        // jump
        bool canJumpNow = self.IsGrounded;
        if (!canJumpNow || locked || self.JumpDisabled)
            actionMask.SetActionEnabled(1, 1, false);

        // drop
        if (!self.IsGrounded || locked)
            actionMask.SetActionEnabled(2, 1, false);

        // light
        if (locked || self.QuickDisabled)
            actionMask.SetActionEnabled(3, 1, false);

        // heavy
        if (locked || self.HeavyDisabled)
            actionMask.SetActionEnabled(4, 1, false);

        // block
        if (locked || self.BlockDisabled)
            actionMask.SetActionEnabled(5, 1, false);

        // special
        if (locked || self.OnAbilityCD || !self.CanCast || self.SpecialDisabled)
            actionMask.SetActionEnabled(6, 1, false);

        // charge
        bool canChargeNow = !locked && !self.ChargeDisabled;
        if (!canChargeNow)
        {
            actionMask.SetActionEnabled(7, 1, false); // hold
            actionMask.SetActionEnabled(7, 2, false); // release
        }

        // parry
        if (locked || !self.CanParry)
            actionMask.SetActionEnabled(8, 1, false);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!self) return;

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

        // time pressure
        AddReward(stepPenalty);

        // damage deltas
        int selfHP = self.GetCurrentHealth();
        int oppHP = (opp != null) ? opp.GetCurrentHealth() : lastOppHP;

        int dealt = Mathf.Max(0, lastOppHP - oppHP);
        int taken = Mathf.Max(0, lastSelfHP - selfHP);

        AddReward(dealt * rewardDamageDealt);
        AddReward(taken * rewardDamageTaken);

        lastSelfHP = selfHP;
        lastOppHP = oppHP;

        ShapingRewards();

        // terminal
        if (opp != null && oppHP <= 0)
        {
            AddReward(rewardWin);
            // EndEpisode();
            return;
        }

        if (selfHP <= 0)
        {
            AddReward(rewardLoss);
            // EndEpisode();
            return;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var d = actionsOut.DiscreteActions;

        d[0] = 1;
        if (Input.GetKey(leftK)) d[0] = 0;
        if (Input.GetKey(rightK)) d[0] = 2;

        d[1] = Input.GetKey(upK) ? 1 : 0;
        d[2] = Input.GetKey(downK) ? 1 : 0;

        d[3] = Input.GetKey(lightK) ? 1 : 0;
        d[4] = Input.GetKey(heavyK) ? 1 : 0;
        d[5] = Input.GetKey(blockK) ? 1 : 0;
        d[6] = Input.GetKey(abilityK) ? 1 : 0;

        d[7] = Input.GetKey(chargeK) ? 1 : 0; // hold only
        d[8] = Input.GetKey(parryK) ? 1 : 0;
    }

    public override void OnEpisodeBegin()
    {
        TryBindNow();
        if (self == null || opp == null) return;

        lastSelfHP = self.GetCurrentHealth();
        lastOppHP = opp.GetCurrentHealth();
        lastMoveX = 0;

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
}