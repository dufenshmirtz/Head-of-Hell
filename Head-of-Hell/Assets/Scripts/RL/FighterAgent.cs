using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

[RequireComponent(typeof(CharacterManager))]
public class FighterAgent : Agent
{
    [Header("Managers")]
    public CharacterManager selfManager;     // assign in Inspector (same GO is fine)
    public CharacterManager enemyManager;    // assign to the opponent's manager

    [Header("Input")]
    public string playerSuffix = "_P1";      // matches your input suffixes
    private AIInputProvider aiInput;

    // live pointers
    private Character self;
    private Character opp;

    // cache keys for Heuristic
    KeyCode upK, downK, leftK, rightK, lightK, heavyK, blockK, abilityK, chargeK, parryK;

    [Header("Rewards")]
    [SerializeField] float rewardDamageDealt = +0.02f;
    [SerializeField] float rewardDamageTaken = -0.02f;
    [SerializeField] float rewardWin         = +1.0f;
    [SerializeField] float rewardLoss        = -1.0f;
    [SerializeField] float stepPenalty       = -0.0001f;

    public float buggedPenalty = -0.02f;
    public float staticPenalty = -0.01f;

    [Header("Shaping (keep it light)")]
    [SerializeField] float engageDist = 2f;            // “in range”
    [SerializeField] float dirReward = 0.0020f;          // reward when moving towards opponent (when far)
    [SerializeField] float dirPenalty = -0.0020f;        // penalty when moving away (when far)
    [SerializeField] float farIdlePenalty = -0.0030f;    // penalty when idle while far
    [SerializeField] float inRangeBonus = +0.0010f;      // small bonus for being within engageDist
    [SerializeField] float farDistancePenaltyScale = -0.0003f; // small continuous penalty for being far

    [Header("Observation scales")]
    [SerializeField] float relXScale = 9f;   // arena half-width approx
    [SerializeField] float relYScale = 5f;   // arena height approx
    [SerializeField] float velScale  = 10f;

    [SerializeField] int totalCharacterCount = 10;

    // bookkeeping
    int lastSelfHP, lastOppHP;
    int lastMoveX = 0;   // -1/0/+1 from last action

    // opponent agent (optional)
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

        upK = setup.up; downK = setup.down; leftK = setup.left; rightK = setup.right;
        lightK = setup.lightAttack; heavyK = setup.heavyAttack; blockK = setup.block;
        abilityK = setup.ability; chargeK = setup.charge; parryK = setup.parry;

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
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            return;
        }

        var rb  = self.GetComponent<Rigidbody2D>();
        var orb = opp.GetComponent<Rigidbody2D>();
        Vector2 vel  = rb  ? rb.velocity  : Vector2.zero;
        Vector2 ovel = orb ? orb.velocity : Vector2.zero;

        Vector2 rel = (Vector2)(opp.transform.position - self.transform.position);

        // REL position (normalized)
        float nx = Mathf.Clamp(rel.x / relXScale, -1f, 1f);
        float ny = Mathf.Clamp(rel.y / relYScale, -1f, 1f);
        sensor.AddObservation(nx);
        sensor.AddObservation(ny);

        // velocities (normalized)
        sensor.AddObservation(Mathf.Clamp(vel.x / velScale,  -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(vel.y / velScale,  -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(ovel.x / velScale, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(ovel.y / velScale, -1f, 1f));

        // health (keep as you had)
        sensor.AddObservation(self.GetCurrentHealth() / 100f);
        sensor.AddObservation(opp.GetCurrentHealth() / 100f);

        // character id one-hot
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

        // disabled flags
        sensor.AddObservation(self.QuickDisabled);
        sensor.AddObservation(self.HeavyDisabled);
        sensor.AddObservation(self.BlockDisabled);
        sensor.AddObservation(self.SpecialDisabled);
        sensor.AddObservation(self.ChargeDisabled);
        sensor.AddObservation(self.JumpDisabled);

        // opponent state (minimal but useful)
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

        // facing hint (cheap, helps approach)
        sensor.AddObservation(Mathf.Sign(rel.x));                  // where opponent is
        sensor.AddObservation(Mathf.Sign(self.transform.localScale.x)); // where i'm facing
    }

    void ShapingRewards()
    {
        if (self == null || opp == null) return;

        Vector2 rel = opp.transform.position - self.transform.position;
        float dist = rel.magnitude;

        if (dist > engageDist)
        {
            int desired = rel.x > 0 ? +1 : -1;

            if (lastMoveX == desired) AddReward(dirReward);
            else if (lastMoveX == -desired) AddReward(dirPenalty);
            else AddReward(farIdlePenalty);
        }
        else
        {
            AddReward(inRangeBonus);
        }

        // small continuous distance penalty (so camping far is never optimal)
        AddReward(farDistancePenaltyScale * Mathf.Clamp01(dist / 10f));
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        if (self == null) return;

        // Branch layout:
        // 0: Move (3)
        // 1: Jump (2)
        // 2: Drop (2)
        // 3: Light (2)
        // 4: Heavy (2)
        // 5: Block (2)
        // 6: Special (2)
        // 7: Charge (3)
        // 8: Parry (2)

        bool locked = self.IsStunned || self.IsCasting;

        // CHARGE lock logic
        if (self.IsCharging)
        {
            // Disable everything except Charge (and optionally Move idle)
            actionMask.SetActionEnabled(1, 1, false); // jump
            actionMask.SetActionEnabled(2, 1, false); // drop
            actionMask.SetActionEnabled(3, 1, false); // light
            actionMask.SetActionEnabled(4, 1, false); // heavy
            actionMask.SetActionEnabled(5, 1, false); // block
            actionMask.SetActionEnabled(6, 1, false); // special
            actionMask.SetActionEnabled(8, 1, false); // parry

            // Move -> only idle
            actionMask.SetActionEnabled(0, 0, false); // left
            actionMask.SetActionEnabled(0, 2, false); // right

            if (self.IsCharged)
            {
                // When fully charged: ONLY release
                actionMask.SetActionEnabled(7, 0, false); // none
                actionMask.SetActionEnabled(7, 1, false); // hold
                // (7,2) release stays enabled
            }
            else
            {
                // While charging (not yet charged): allow hold OR release
                // Disable "none" so it doesn't drop charge by accident
                actionMask.SetActionEnabled(7, 0, false); // none
                // keep (7,1) hold enabled
                // keep (7,2) release enabled
            }

            return;
        }

        // -------------------
        // JUMP
        // -------------------
        bool canJumpNow = self.IsGrounded;   // αν έχεις flying character θα το επεκτείνουμε
        if (!canJumpNow || locked || self.JumpDisabled)
            actionMask.SetActionEnabled(1, 1, false);

        // -------------------
        // DROP (μόνο αν grounded – βελτίωσε αν έχεις IsOnPlatform)
        // -------------------
        if (!self.IsGrounded || locked)
            actionMask.SetActionEnabled(2, 1, false);

        // -------------------
        // LIGHT (quick)
        // -------------------
        if (locked || self.QuickDisabled)
            actionMask.SetActionEnabled(3, 1, false);

        // -------------------
        // HEAVY
        // -------------------
        if (locked || self.HeavyDisabled)
            actionMask.SetActionEnabled(4, 1, false);

        // -------------------
        // BLOCK
        // -------------------
        if (self.BlockDisabled)
            actionMask.SetActionEnabled(5, 1, false);

        // -------------------
        // SPECIAL
        // -------------------
        if (locked || self.OnAbilityCD || !self.CanCast || self.SpecialDisabled)
            actionMask.SetActionEnabled(6, 1, false);

        // -------------------
        // CHARGE
        // -------------------
        bool canChargeNow = !locked && !self.ChargeDisabled;
        if (!canChargeNow)
        {
            actionMask.SetActionEnabled(7, 1, false); // hold
            actionMask.SetActionEnabled(7, 2, false); // release
        }

        // -------------------
        // PARRY
        // -------------------
        if (locked || !self.CanParry)
            actionMask.SetActionEnabled(8, 1, false);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!self) return;

        int moveBranch = actions.DiscreteActions[0];
        int jump       = actions.DiscreteActions[1];
        int drop       = actions.DiscreteActions[2];
        int light      = actions.DiscreteActions[3];
        int heavy      = actions.DiscreteActions[4];
        int blockHold  = actions.DiscreteActions[5];
        int special    = actions.DiscreteActions[6];
        int chargeMode = actions.DiscreteActions[7];
        int parry      = actions.DiscreteActions[8];

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
        int oppHP  = (opp != null) ? opp.GetCurrentHealth() : lastOppHP;

        int dealt = Mathf.Max(0, lastOppHP - oppHP);
        int taken = Mathf.Max(0, lastSelfHP - selfHP);

        AddReward(dealt * rewardDamageDealt);
        AddReward(taken * rewardDamageTaken);

        lastSelfHP = selfHP;
        lastOppHP  = oppHP;

        ShapingRewards();

        // terminal
        if (opp != null && oppHP <= 0)
        {
            AddReward(rewardWin);
            //EndEpisode();
            return;
        }

        if (selfHP <= 0)
        {
            AddReward(rewardLoss);
            //EndEpisode();
            return;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var d = actionsOut.DiscreteActions;

        d[0] = 1;
        if (Input.GetKey(leftK))  d[0] = 0;
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
        lastOppHP  = opp.GetCurrentHealth();
        lastMoveX  = 0;

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