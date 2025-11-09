using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

[RequireComponent(typeof(CharacterManager))]
public class FighterAgent : Agent
{
    [Header("Managers")]
    public CharacterManager selfManager;   // assign in Inspector (same GO is fine)
    public CharacterManager enemyManager;  // assign to the opponent's manager

    [Header("Input")]
    public string playerSuffix = "_P1";    // matches your input suffixes
    private AIInputProvider aiInput;

    // live pointers (change when forms swap)
    private Character self;
    private Character opp;

    // cache the actual keybinds so Heuristic matches CharacterSetup
    KeyCode upK, downK, leftK, rightK, lightK, heavyK, blockK, abilityK, chargeK, parryK;

    // ---- Reward tuning (simple, we can tweak later)
    [SerializeField] float rewardDamageDealt = +0.02f;
    [SerializeField] float rewardDamageTaken = -0.02f;
    [SerializeField] float rewardWin = +1.0f;
    [SerializeField] float rewardLoss = -1.0f;
    [SerializeField] float stepPenalty = -0.0005f;
    public float buggedPenalty = -0.02f;
    public float staticPenalty = -0.01f;
    [SerializeField] int totalCharacterCount = 10; // adjust if you add more


    [SerializeField] float rewardCloseToOpponent = +0.0008f;
    [SerializeField] float penaltyFarFromOpponent = -0.0008f;
    [SerializeField] float centerStageBonus = +0.0006f;

    [SerializeField] Vector2 stageMin = new(-9f, -3f);
    [SerializeField] Vector2 stageMax = new(9f, 5f);
    [SerializeField] float engageDist = 3.5f; // “in range”



    // Health bookkeeping
    int lastSelfHP, lastOppHP;

    // Optional: cache opponent agent so we can end both episodes together
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
        // Bind self if missing
        if (self == null && selfManager != null)
        {
            var c = selfManager.CharacterChoice(1);   // your API that returns active char for P1
            if (c != null)
            {
                BindSelf(c);
                Debug.Log($"[Agent {playerSuffix}] Bound SELF: {c.name}");
            }
        }

        // Bind opponent if missing
        if (opp == null && enemyManager != null)
        {
            var e = enemyManager.CharacterChoice(2);   // active char for P2
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

        // create the provider once; we’ll inject it into each new Character
        aiInput = new AIInputProvider(playerSuffix);

        // Subscribe to lifecycle events
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

        // cache keys for Heuristic()
        upK = setup.up; downK = setup.down; leftK = setup.left; rightK = setup.right;
        lightK = setup.lightAttack; heavyK = setup.heavyAttack; blockK = setup.block;
        abilityK = setup.ability; chargeK = setup.charge; parryK = setup.parry;

        // make sure the provider knows them too (so input.GetKeyDown/Up works)
        if (aiInput == null) aiInput = new AIInputProvider(playerSuffix);
        aiInput.SetKeys(leftK, rightK, upK, downK, lightK, heavyK, blockK, abilityK, chargeK, parryK);

        self.SetInput(aiInput);

        lastSelfHP = self.GetCurrentHealth();
    }


    private void BindEnemy(Character c)
    {
        opp = c;
        if (opp != null) lastOppHP = opp.GetCurrentHealth();

        // try to find the opponent's FighterAgent
        if (enemyManager != null)
            oppAgent = enemyManager.GetComponent<FighterAgent>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {

        if (!self || !opp)
        {
            // keep obs small until bound
            sensor.AddObservation(Vector3.zero); // pad minimal
            sensor.AddObservation(0f);
            return;
        }

        var rb = self.GetComponent<Rigidbody2D>();
        var orb = opp.GetComponent<Rigidbody2D>();
        Vector2 vel = rb ? rb.velocity : Vector2.zero;
        Vector2 ovel = orb ? orb.velocity : Vector2.zero;

        Vector2 rel = (Vector2)(opp.transform.position - self.transform.position);

        sensor.AddObservation(self.transform.position.x);
        sensor.AddObservation(self.transform.position.y);
        sensor.AddObservation(vel.x);
        sensor.AddObservation(vel.y);
        sensor.AddObservation(rel.x);
        sensor.AddObservation(rel.y);
        sensor.AddObservation(ovel.x);
        sensor.AddObservation(ovel.y);
        sensor.AddObservation(self.GetCurrentHealth() / 100f);
        sensor.AddObservation(opp.GetCurrentHealth() / 100f);

        // Example: encode self character type
        sensor.AddOneHotObservation(self.characterID, totalCharacterCount);

        // Same for opponent
        sensor.AddOneHotObservation(opp.characterID, totalCharacterCount);


        // --- Move & ability state: self ---
        sensor.AddObservation(self.IsGrounded);
        sensor.AddObservation(self.IsBlocking);
        sensor.AddObservation(self.IsCasting);
        sensor.AddObservation(self.IsStunned);
        sensor.AddObservation(self.IsKnocked);
        sensor.AddObservation(self.IsCharging);
        sensor.AddObservation(self.IsCharged);
        sensor.AddObservation(self.OnAbilityCD);
        sensor.AddObservation(self.AbilityCooldown01);  // float
        sensor.AddObservation(self.CanCast);
        sensor.AddObservation(self.CanParry);

        // Availability / disabled flags (so the policy doesn’t try unusable moves)
        sensor.AddObservation(self.QuickDisabled);
        sensor.AddObservation(self.HeavyDisabled);
        sensor.AddObservation(self.BlockDisabled);
        sensor.AddObservation(self.SpecialDisabled);
        sensor.AddObservation(self.ChargeDisabled);
        sensor.AddObservation(self.JumpDisabled);

        // --- For opponent too (helps with reads & punishes) ---
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

        // (You can also add opp disabled flags if useful)


    }

    void ShapingRewards()
    {
        if (self == null || opp == null) return;

        // Encourage engagement
        float dist = Vector2.Distance(self.transform.position, opp.transform.position);
        AddReward(dist < engageDist ? rewardCloseToOpponent : penaltyFarFromOpponent);

    }


    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        if (self == null) return;

        // Branch indices (your layout):
        // 0: Move (3) | 1: Jump (2) | 2: Drop (2) | 3: Light (2) | 4: Heavy (2)
        // 5: Block (2) | 6: Special (2) | 7: Charge (3) | 8: Parry (2)

        bool locked = self.IsStunned || self.IsCasting;

        // Jump only if grounded and not locked
        if (!self.IsGrounded || locked)
            actionMask.SetActionEnabled(1, 1, false); // disable "jump = 1"

        // Drop only when on a platform and not locked
        if (!self.IsGrounded || locked) // tweak if you have an "onPlatform" flag
            actionMask.SetActionEnabled(2, 1, false);

        // Attacks disabled while locked
        if (locked)
        {
            actionMask.SetActionEnabled(3, 1, false); // light
            actionMask.SetActionEnabled(4, 1, false); // heavy
            actionMask.SetActionEnabled(6, 1, false); // special
            actionMask.SetActionEnabled(8, 1, false); // parry
        }

        // Respect gameplay locks
        if (self.QuickDisabled) actionMask.SetActionEnabled(3, 1, false);
        if (self.HeavyDisabled) actionMask.SetActionEnabled(4, 1, false);
        if (self.SpecialDisabled) actionMask.SetActionEnabled(6, 1, false);
        if (self.BlockDisabled) actionMask.SetActionEnabled(5, 1, false);
        if (self.JumpDisabled) actionMask.SetActionEnabled(1, 1, false);
        if (self.ChargeDisabled)
        {
            actionMask.SetActionEnabled(7, 1, false); // hold
            actionMask.SetActionEnabled(7, 2, false); // release
        }
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!self) return;

        // Branches: 3,2,2,2,2,2,2,3,2  (same as before)
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


        // --- tiny time pressure so it doesn't turtle forever
        AddReward(stepPenalty);

        // --- reward for damage dealt / taken (health deltas)
        int selfHP = self.GetCurrentHealth();
        int oppHP = (opp != null) ? opp.GetCurrentHealth() : lastOppHP;

        int dealt = Mathf.Max(0, lastOppHP - oppHP);   // damage we did
        int taken = Mathf.Max(0, lastSelfHP - selfHP); // damage we took

        AddReward(dealt * rewardDamageDealt);
        AddReward(taken * rewardDamageTaken);

        lastSelfHP = selfHP;
        lastOppHP = oppHP;

        ShapingRewards();

        // --- terminal conditions (round end)
        if (opp != null && oppHP <= 0)
        {
            // we won
            AddReward(rewardWin);
            if (oppAgent) oppAgent.AddReward(rewardLoss);
            // end both episodes
            EndEpisode();
            if (oppAgent) oppAgent.EndEpisode();
            return;
        }

        if (selfHP <= 0)
        {
            // we lost
            AddReward(rewardLoss);
            if (oppAgent) oppAgent.AddReward(rewardWin);
            EndEpisode();
            if (oppAgent) oppAgent.EndEpisode();
            return;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var d = actionsOut.DiscreteActions;

        // Move: 0 left, 1 idle, 2 right
        d[0] = 1;
        if (Input.GetKey(leftK)) d[0] = 0;
        if (Input.GetKey(rightK)) d[0] = 2;

        d[1] = Input.GetKey(upK) ? 1 : 0; // jump
        d[2] = Input.GetKey(downK) ? 1 : 0; // drop

        d[3] = Input.GetKey(lightK) ? 1 : 0; // light
        d[4] = Input.GetKey(heavyK) ? 1 : 0; // heavy
        d[5] = Input.GetKey(blockK) ? 1 : 0; // block (hold)
        d[6] = Input.GetKey(abilityK) ? 1 : 0; // special

        // Before:
        //d[7] = Input.GetKey(chargeK) ? 1 : (Input.GetKeyUp(chargeK) ? 2 : 0);

        // After (simpler, safer):
        d[7] = Input.GetKey(chargeK) ? 1 : 0;   // 1 = hold, 0 = not holding

        d[8] = Input.GetKey(parryK) ? 1 : 0; // parry
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
