using UnityEngine;

public class SimpleBotController : MonoBehaviour
{
    [Header("Runtime References")]
    [SerializeField] private Character character;
    [SerializeField] private CharacterSetup setup;
    [SerializeField] private Character target;

    [Header("Skill")]
    [Range(0f, 1f)]
    [SerializeField] private float skill = 0.6f;
    [SerializeField] private bool randomizePersonality = true;

    [Header("Movement")]
    [SerializeField] private float preferredRange = 1.25f;
    [SerializeField] private float tooCloseRange = 0.55f;

    [Header("Timing")]
    [SerializeField] private float thinkIntervalMin = 0.18f;
    [SerializeField] private float thinkIntervalMax = 0.28f;
    [SerializeField] private float actionCooldownMin = 0.35f;
    [SerializeField] private float actionCooldownMax = 0.80f;

    [Header("Action Chances")]
    [SerializeField] private float quickChance = 0.62f;
    [SerializeField] private float heavyChance = 0.20f;
    [SerializeField] private float specialChance = 0.08f;
    [SerializeField] private float jumpChance = 0.04f;
    [SerializeField] private float blockChance = 0.05f;

    [Header("Level")]
    [SerializeField] private float sameLevelTolerance = 1.5f;
    [SerializeField] private float engageRange = 1.35f;

    [Header("Platform Drop")]
    [SerializeField] private float dropHoldDuration = 0.20f;

    [Header("Anti-Charge Reaction")]
    [SerializeField] private bool antiChargeEnabled = true;
    [SerializeField] private float antiChargeRange = 2.0f;
    [SerializeField] private float antiChargeHeightTolerance = 1.6f;
    [SerializeField] private float antiChargeReactionCooldown = 0.45f;
    [SerializeField] private float antiChargeBaseChance = 0.35f;
    [SerializeField] private float antiChargeSkillBonus = 0.50f;
    [SerializeField] private float antiChargeParryWeight = 0.42f;
    [SerializeField] private float antiChargeJumpWeight = 0.26f;
    [SerializeField] private float antiChargeRetreatWeight = 0.22f;
    [SerializeField] private float antiChargeBlockWeight = 0.10f;
    [SerializeField] private float retreatDuration = 0.22f;

    [SerializeField] private float antiChargeSpecialChance = 0.92f;
    [SerializeField] private float antiChargeSpecialRange = 2f;
    [SerializeField] private float antiChargeSpecialCooldown = 0.30f;

    [SerializeField] private float antiChargeParryDelay = 0.5f;

    private bool trackingEnemyCharge;
    private float enemyChargeTimer;

    private BotInputProvider botInput;
    private float thinkTimer;
    private float actionTimer;
    private float blockHoldTimer;

    private float forcedVerticalTimer;
    private float forcedVerticalValue;
    private bool isDroppingThroughPlatform;

    private float antiChargeReactionTimer;

    private float forcedHorizontalTimer;
    private float forcedHorizontalValue;

    private bool initialized;

    // =========================================================
    // INIT
    // =========================================================

    private void Start()
    {
        TryInitialize();
        RandomizePersonality();
    }

    private void OnEnable()
    {
        thinkTimer = 0f;
        actionTimer = 0f;
        blockHoldTimer = 0f;

        forcedVerticalTimer = 0f;
        forcedVerticalValue = 0f;
        isDroppingThroughPlatform = false;

        antiChargeReactionTimer = 0f;
        forcedHorizontalTimer = 0f;
        forcedHorizontalValue = 0f;

        initialized = false;

        trackingEnemyCharge = false;
        enemyChargeTimer = 0f;
    }

    private void OnDisable()
    {
        if (botInput != null)
            botInput.ClearFrameState();
    }

    private void Update()
    {
        if (!initialized)
        {
            TryInitialize();
            return;
        }

        if (character == null || setup == null || target == null)
            return;

        botInput.ClearFrameState();

        thinkTimer -= Time.deltaTime;
        actionTimer -= Time.deltaTime;
        blockHoldTimer -= Time.deltaTime;
        antiChargeReactionTimer -= Time.deltaTime;

        UpdateForcedVertical();
        UpdateForcedHorizontal();

        if (blockHoldTimer <= 0f)
            botInput.SetKey(setup.block, false);

        // πρώτα reactions, μετά movement/normal decisions
        if (TryReactToCharge())
        {
            HandleMovement();
            return;
        }

        HandleMovement();

        if (thinkTimer <= 0f)
        {
            thinkTimer = Random.Range(thinkIntervalMin, thinkIntervalMax);
            DecideAction();
        }

        UpdateEnemyChargeTracking();
    }

    private void TryInitialize()
    {
        if (setup == null) setup = GetComponent<CharacterSetup>();
        if (character == null) character = GetComponent<Character>();

        if (setup == null || character == null) return;

        if (botInput == null)
        {
            botInput = new BotInputProvider();
            character.SetInput(botInput);
        }

        if (target == null) target = FindOpponent();

        if (target != null)
            initialized = true;
    }

    // =========================================================
    // PERSONALITY
    // =========================================================

    private void RandomizePersonality()
    {
        if (!randomizePersonality) return;

        float f = Random.Range(0.8f, 1.2f);

        quickChance *= f;
        heavyChance *= f;
        specialChance *= f;
        jumpChance *= f;
        blockChance *= f;
    }

    // =========================================================
    // MOVEMENT
    // =========================================================

    private void HandleMovement()
    {
        float dxSigned = target.transform.position.x - transform.position.x;
        float dySigned = target.transform.position.y - transform.position.y;

        float dx = Mathf.Abs(dxSigned);
        float dy = Mathf.Abs(dySigned);

        bool sameLevel = dy <= sameLevelTolerance;
        bool targetBelow = dySigned < -sameLevelTolerance;
        bool inEngageRange = dx <= engageRange && sameLevel;

        float move = 0f;

        // forced retreat / burst movement
        if (forcedHorizontalTimer > 0f)
        {
            move = forcedHorizontalValue;
        }
        // ⭐ Anti-top-camp
        else if (targetBelow && dx < 0.6f && !isDroppingThroughPlatform)
        {
            float dropChance = Mathf.Lerp(0.4f, 0.75f, skill);

            if (Random.value < dropChance)
            {
                TriggerDropPlatform();
            }
            else
            {
                move = Random.value < 0.5f ? -1f : 1f;
            }
        }
        else if (!inEngageRange)
        {
            if (dx > preferredRange)
                move = Mathf.Sign(dxSigned);
            else if (dx < tooCloseRange)
                move = -Mathf.Sign(dxSigned);
        }

        string suffix = GetAxisSuffix();

        botInput.SetAxis("Horizontal" + suffix, move);
        botInput.SetAxis("Horizontal" + setup.playerNum, move);
        botInput.SetAxis("Horizontal", move);

        float vertical = forcedVerticalTimer > 0f ? forcedVerticalValue : 0f;

        botInput.SetAxis("Vertical" + suffix, vertical);
        botInput.SetAxis("Vertical" + setup.playerNum, vertical);
        botInput.SetAxis("Vertical", vertical);
    }

    // =========================================================
    // ACTIONS
    // =========================================================

    private void DecideAction()
    {
        if (actionTimer > 0f || isDroppingThroughPlatform)
            return;

        float dx = Mathf.Abs(target.transform.position.x - transform.position.x);
        float dy = Mathf.Abs(target.transform.position.y - transform.position.y);

        bool sameLevel = dy <= sameLevelTolerance;
        bool inEngageRange = dx <= engageRange && sameLevel;

        // Defensive reaction
        if (inEngageRange && Random.value < Mathf.Lerp(0.2f, 0.7f, skill))
        {
            botInput.SetKey(setup.block, true);
            blockHoldTimer = Random.Range(0.12f, 0.28f);
            actionTimer = 0.25f;
            return;
        }

        // Far approach
        if (!inEngageRange)
        {
            if (Random.value < jumpChance)
            {
                PressOneFrame(setup.up);
                actionTimer = Random.Range(actionCooldownMin, actionCooldownMax);
            }

            return;
        }

        // Engage attacks
        float r = Random.value;

        if (r < quickChance)
        {
            PressOneFrame(setup.lightAttack);
            actionTimer = Random.Range(0.22f, 0.40f);
            return;
        }

        r -= quickChance;

        if (r < heavyChance)
        {
            PressOneFrame(setup.heavyAttack);
            actionTimer = Random.Range(0.28f, 0.48f);
            return;
        }

        r -= heavyChance;

        if (r < specialChance)
        {
            PressOneFrame(setup.ability);
            actionTimer = Random.Range(0.35f, 0.60f);
            return;
        }
    }

    // =========================================================
    // ANTI-CHARGE
    // =========================================================

    private bool TryReactToCharge()
    {
        if (!antiChargeEnabled) return false;
        if (antiChargeReactionTimer > 0f) return false;
        if (actionTimer > 0f) return false;
        if (character == null || target == null) return false;

        if (character.IsCasting || character.IsStunned || character.IsKnocked)
            return false;

        float dxSigned = target.transform.position.x - transform.position.x;
        float dySigned = target.transform.position.y - transform.position.y;

        float dx = Mathf.Abs(dxSigned);
        float dy = Mathf.Abs(dySigned);

        bool sameLevel = dy <= antiChargeHeightTolerance;
        bool closeEnough = dx <= antiChargeRange;
        bool opponentCharging = target.IsCharging;

        if (!opponentCharging || !closeEnough || !sameLevel)
            return false;

        bool chargingTowardsMe = IsTargetChargingTowardsMe(dxSigned);

        // =========================================================
        // PRIORITY: SPECIAL σχεδόν πάντα όταν το charge έρχεται πάνω μας
        // =========================================================
        if (chargingTowardsMe &&
            dx <= antiChargeSpecialRange &&
            CanUseSpecialNow())
        {
            if (Random.value < antiChargeSpecialChance)
            {
                PressOneFrame(setup.ability);
                antiChargeReactionTimer = antiChargeSpecialCooldown;
                actionTimer = 0.20f;
                return true;
            }
        }

        float reactChance = antiChargeBaseChance + antiChargeSkillBonus * skill;

        if (target.IsCharged)
            reactChance += 0.15f;

        reactChance = Mathf.Clamp01(reactChance);

        if (Random.value > reactChance)
            return false;

        antiChargeReactionTimer = antiChargeReactionCooldown;

        float totalWeight =
            antiChargeParryWeight +
            antiChargeJumpWeight +
            antiChargeRetreatWeight +
            antiChargeBlockWeight;

        float roll = Random.value * totalWeight;

        // 1) PARRY
        if (roll < antiChargeParryWeight)
        {
            bool parryTimingReady = trackingEnemyCharge && enemyChargeTimer >= antiChargeParryDelay;

            if (parryTimingReady && character.CanParry && !character.IsCasting)
            {
                PressOneFrame(setup.parry);
                actionTimer = 0.22f;
                return true;
            }
        }
        else
        {
            roll -= antiChargeParryWeight;
        }

        // 2) JUMP
        if (roll < antiChargeJumpWeight)
        {
            if (character.IsGrounded && !character.JumpDisabled && !character.IsCasting)
            {
                PressOneFrame(setup.up);
                ForceRetreatFromTarget(dxSigned, retreatDuration * 0.75f);
                actionTimer = 0.25f;
                return true;
            }
        }
        else
        {
            roll -= antiChargeJumpWeight;
        }

        // 3) RETREAT
        if (roll < antiChargeRetreatWeight)
        {
            ForceRetreatFromTarget(dxSigned, retreatDuration);
            actionTimer = 0.18f;
            return true;
        }
        else
        {
            roll -= antiChargeRetreatWeight;
        }

        // 4) BLOCK fallback
        botInput.SetKey(setup.block, true);
        blockHoldTimer = Random.Range(0.18f, 0.32f);
        actionTimer = 0.22f;
        return true;
    }

    private void ForceRetreatFromTarget(float dxSigned, float duration)
    {
        // αν ο αντίπαλος είναι δεξιά, φύγε αριστερά, και το αντίστροφο
        forcedHorizontalValue = -Mathf.Sign(dxSigned);

        // αν για κάποιο λόγο είναι ακριβώς πάνω μας
        if (Mathf.Abs(forcedHorizontalValue) < 0.01f)
            forcedHorizontalValue = Random.value < 0.5f ? -1f : 1f;

        forcedHorizontalTimer = duration;
    }

    private void UpdateForcedHorizontal()
    {
        if (forcedHorizontalTimer > 0f)
        {
            forcedHorizontalTimer -= Time.deltaTime;
            if (forcedHorizontalTimer <= 0f)
            {
                forcedHorizontalTimer = 0f;
                forcedHorizontalValue = 0f;
            }
        }
    }

    // =========================================================
    // PLATFORM DROP
    // =========================================================

    private void TriggerDropPlatform()
    {
        isDroppingThroughPlatform = true;

        PressOneFrame(setup.down);

        forcedVerticalValue = -1f;
        forcedVerticalTimer = dropHoldDuration;
    }

    private void UpdateForcedVertical()
    {
        if (forcedVerticalTimer > 0f)
        {
            forcedVerticalTimer -= Time.deltaTime;

            if (forcedVerticalTimer <= 0f)
            {
                forcedVerticalValue = 0f;
                isDroppingThroughPlatform = false;
            }
        }
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private string GetAxisSuffix()
    {
        return setup.playerNum == 1 ? "_P1" : "_P2";
    }

    private void PressOneFrame(KeyCode key)
    {
        botInput.PressKeyOneFrame(key);
    }

    private Character FindOpponent()
    {
        Character[] all = FindObjectsOfType<Character>();

        foreach (Character c in all)
            if (c != character)
                return c;

        return null;
    }

    public void SetTarget(Character newTarget)
    {
        target = newTarget;
    }

    public void SetSkill(float newSkill)
    {
        skill = Mathf.Clamp01(newSkill);
    }

    public void Rebind(Character self, Character enemy)
    {
        character = self;
        target = enemy;
        setup = GetComponent<CharacterSetup>();

        if (botInput == null)
            botInput = new BotInputProvider();

        if (character != null)
            character.SetInput(botInput);

        initialized = (character != null && setup != null && target != null);
    }

    private bool IsTargetChargingTowardsMe(float dxSigned)
    {
        if (target == null) return false;

        float targetFacing = Mathf.Sign(target.transform.localScale.x);

        // αν ο αντίπαλος είναι δεξιά μας, για να έρχεται προς εμάς πρέπει να κοιτάει αριστερά (-1)
        // αν είναι αριστερά μας, για να έρχεται προς εμάς πρέπει να κοιτάει δεξιά (+1)
        float dirToMe = -Mathf.Sign(dxSigned);

        // fallback αν είναι ακριβώς πάνω μας
        if (Mathf.Abs(dirToMe) < 0.01f)
            return true;

        return targetFacing == dirToMe;
    }

    private bool CanUseSpecialNow()
    {
        if (character == null) return false;

        return
            !character.IsCasting &&
            !character.IsStunned &&
            !character.IsKnocked &&
            character.CanCast &&
            !character.OnAbilityCD &&
            !character.SpecialDisabled;
    }

    private void UpdateEnemyChargeTracking()
    {
        if (target == null)
        {
            trackingEnemyCharge = false;
            enemyChargeTimer = 0f;
            return;
        }

        if (target.IsCharging)
        {
            if (!trackingEnemyCharge)
            {
                trackingEnemyCharge = true;
                enemyChargeTimer = 0f;
            }
            else
            {
                enemyChargeTimer += Time.deltaTime;
            }
        }
        else
        {
            trackingEnemyCharge = false;
            enemyChargeTimer = 0f;
        }
    }
}