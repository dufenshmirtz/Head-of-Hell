using UnityEngine;

public class SimpleBotController : MonoBehaviour
{
    [Header("Runtime References")]
    [SerializeField] private Character character;
    [SerializeField] private CharacterSetup setup;
    [SerializeField] private Character target;

    [Header("Movement")]
    [SerializeField] private float preferredRange = 1.25f;
    [SerializeField] private float tooCloseRange = 0.55f;
    [SerializeField] private float randomRetreatChance = 0.02f;

    [Header("Decision Timing")]
    [SerializeField] private float thinkIntervalMin = 0.18f;
    [SerializeField] private float thinkIntervalMax = 0.28f;
    [SerializeField] private float actionCooldownMin = 0.35f;
    [SerializeField] private float actionCooldownMax = 0.80f;

    [Header("Action Probabilities")]
    [SerializeField] private float quickChance = 0.62f;
    [SerializeField] private float heavyChance = 0.20f;
    [SerializeField] private float specialChance = 0.08f;
    [SerializeField] private float chargeChance = 0.00f;   // άστο 0 για τώρα
    [SerializeField] private float jumpChance = 0.04f;
    [SerializeField] private float blockChance = 0.05f;
    [SerializeField] private float parryChance = 0.00f;    // άστο 0 για τώρα

    [Header("Level Awareness")]
    [SerializeField] private float sameLevelTolerance = 1.5f;
    [SerializeField] private float jumpToTargetChance = 0.15f;
    [SerializeField] private float engageRange = 1.35f;
    [SerializeField] private float dropToTargetChance = 0.45f;
    [SerializeField] private float underTargetDeadzone = 0.60f;
    [SerializeField] private float sideStepWhenBelow = 1f;
    [SerializeField] private float forcedDropChance = 1.0f;

    [Header("Drop Platform")]
    [SerializeField] private float dropHoldDuration = 0.20f;   // πόσο κρατάμε vertical = -1
    [SerializeField] private float dropMinXAlign = 0.90f;      // πόσο κοντά σε X πρέπει να είμαστε για drop attempt

    [Header("Combat")]
    [SerializeField] private float faceToFaceAttackBias = 0.80f;
    [SerializeField] private float idleInEngageChance = 0.08f;

    [Header("Stacked Platform Fix")]
    [SerializeField] private float stuckUnderTargetMoveTime = 0.18f;
    [SerializeField] private float stuckUnderTargetSpeed = 1f;
    [SerializeField] private float forcedDropIfStackedChance = 1.0f;

    [Header("Anti Camp")]
    [SerializeField] private float stackedDropDelay = 1.25f;
    [SerializeField] private float stackedXThreshold = 0.60f;

    [Header("Debug")]
    [SerializeField] private bool logDebug = false;

    private BotInputProvider botInput;
    private float thinkTimer;
    private float actionTimer;
    private float blockHoldTimer;
    private float forcedVerticalTimer;
    private float forcedVerticalValue;
    private bool initialized;
    private float forcedHorizontalTimer;
    private float forcedHorizontalValue;
    private bool isDroppingThroughPlatform;
    private float stackedOnPlatformTimer;

    private void Start()
    {
        TryInitialize();
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
        forcedHorizontalTimer -= Time.deltaTime;

        UpdateForcedVertical();
        UpdateAntiCamp();

        if (forcedVerticalTimer <= 0f)
            isDroppingThroughPlatform = false;

        if (blockHoldTimer <= 0f)
            botInput.SetKey(setup.block, false);

        HandleMovement();

        if (thinkTimer <= 0f)
        {
            thinkTimer = Random.Range(thinkIntervalMin, thinkIntervalMax);
            DecideAction();
        }
    }

    private void TryInitialize()
    {
        if (setup == null)
            setup = GetComponent<CharacterSetup>();

        if (character == null)
            character = GetComponent<Character>();

        if (setup == null || character == null)
            return;

        if (botInput == null)
        {
            botInput = new BotInputProvider();
            character.SetInput(botInput);
        }

        if (target == null)
            target = FindOpponent();

        if (target != null)
        {
            initialized = true;

            if (logDebug)
                Debug.Log($"{name} bot initialized. Target = {target.name}");
        }
    }

    private void HandleMovement()
    {
        float dxSigned = target.transform.position.x - transform.position.x;
        float dySigned = target.transform.position.y - transform.position.y;

        float dx = Mathf.Abs(dxSigned);
        float dy = Mathf.Abs(dySigned);

        bool sameLevel = dy <= sameLevelTolerance;
        bool targetAbove = dySigned > sameLevelTolerance;
        bool targetBelow = dySigned < -sameLevelTolerance;
        bool inEngageRange = dx <= engageRange && sameLevel;

        float move = 0f;

        // Όταν κάνει drop, μηδένισε horizontal για να μην "σέρνεται" πάνω στην platform
        if (isDroppingThroughPlatform)
        {
            move = 0f;
        }
        // Αν είμαστε face-to-face, κόψε το "χορό" δεξιά-αριστερά
        else if (inEngageRange)
        {
            move = 0f;
        }
        // Αν ο άλλος είναι πιο πάνω, πλησίασε για jump alignment
        else if (targetAbove)
        {
            if (dx > 0.60f)
                move = Mathf.Sign(dxSigned);
            else
                move = 0f;
        }
        // Αν ο άλλος είναι πιο κάτω, πλησίασε για drop alignment
        else if (targetBelow)
        {
            if (dx > dropMinXAlign)
                move = Mathf.Sign(dxSigned);
            else
                move = 0f;
        }
        // Ίδιο περίπου ύψος
        else
        {
            if (dx > preferredRange)
            {
                move = Mathf.Sign(dxSigned);
            }
            else if (dx < tooCloseRange)
            {
                move = -Mathf.Sign(dxSigned);
            }
            else if (Random.value < randomRetreatChance)
            {
                move = -Mathf.Sign(dxSigned);
            }
            else
            {
                move = 0f;
            }
        }

        if (forcedHorizontalTimer > 0f)
        {
            move = forcedHorizontalValue;
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

    private void ForceSideStepAwayFromTarget()
    {
        float dir = transform.position.x <= target.transform.position.x ? -1f : 1f;

        if (Mathf.Abs(target.transform.position.x - transform.position.x) < 0.05f)
            dir = Random.value < 0.5f ? -1f : 1f;

        forcedHorizontalValue = dir * stuckUnderTargetSpeed;
        forcedHorizontalTimer = stuckUnderTargetMoveTime;
    }

    private void DecideAction()
    {
        if (actionTimer > 0f || isDroppingThroughPlatform)
            return;

        float dxSigned = target.transform.position.x - transform.position.x;
        float dySigned = target.transform.position.y - transform.position.y;

        float dx = Mathf.Abs(dxSigned);
        float dy = Mathf.Abs(dySigned);

        bool sameLevel = dy <= sameLevelTolerance;
        bool targetAbove = dySigned > sameLevelTolerance;
        bool targetBelow = dySigned < -sameLevelTolerance;
        bool inEngageRange = dx <= engageRange && sameLevel;

        // -----------------------------
        // LEVEL ALIGNMENT
        // -----------------------------

        // Ο αντίπαλος είναι πιο πάνω
        if (!sameLevel && targetAbove)
        {
            if (dx < underTargetDeadzone)
            {
                ForceSideStepAwayFromTarget();
                actionTimer = Random.Range(0.10f, 0.16f);

                if (logDebug)
                    Debug.Log($"{name} -> FORCE SIDE-STEP UNDER TARGET");

                return;
            }

            if (dx <= preferredRange + 0.35f && Random.value < jumpToTargetChance)
            {
                PressOneFrame(setup.up);
                actionTimer = Random.Range(0.20f, 0.40f);

                if (logDebug)
                    Debug.Log($"{name} -> JUMP TO ALIGN LEVEL");

                return;
            }

            if (Random.value < 0.20f)
            {
                PressOneFrame(setup.ability);
                actionTimer = Random.Range(0.30f, 0.50f);

                if (logDebug)
                    Debug.Log($"{name} -> SPECIAL (TARGET ABOVE FALLBACK)");

                return;
            }

            actionTimer = Random.Range(0.08f, 0.15f);
            return;
        }

        // Ο αντίπαλος είναι πιο κάτω
        if (!sameLevel && targetBelow)
        {
            // Αν είναι σχεδόν ακριβώς από κάτω -> κάνε drop
            if (dx < underTargetDeadzone)
            {
                TriggerDropPlatform();
                actionTimer = Random.Range(0.20f, 0.35f);

                if (logDebug)
                    Debug.Log($"{name} -> FORCED DROP (STACKED)");

                return;
            }

            // Αν είναι αρκετά κοντά σε X -> πιθανό drop
            if (dx <= dropMinXAlign && Random.value < forcedDropChance)
            {
                TriggerDropPlatform();
                actionTimer = Random.Range(0.20f, 0.35f);

                if (logDebug)
                    Debug.Log($"{name} -> DROP TO ALIGN LEVEL");

                return;
            }

            // fallback attack
            if (dx <= preferredRange + 0.30f && Random.value < specialChance)
            {
                PressOneFrame(setup.ability);
                actionTimer = Random.Range(actionCooldownMin, actionCooldownMax);

                if (logDebug)
                    Debug.Log($"{name} -> SPECIAL (TARGET BELOW)");

                return;
            }

            actionTimer = Random.Range(0.08f, 0.15f);
            return;
        }

        // -----------------------------
        // FAR APPROACH
        // -----------------------------
        if (!inEngageRange && dx > preferredRange + 0.15f)
        {
            float farRoll = Random.value;

            if (farRoll < jumpChance)
            {
                PressOneFrame(setup.up);
                actionTimer = Random.Range(actionCooldownMin, actionCooldownMax);

                if (logDebug)
                    Debug.Log($"{name} -> JUMP (FAR)");

                return;
            }

            if (farRoll < jumpChance + specialChance)
            {
                PressOneFrame(setup.ability);
                actionTimer = Random.Range(actionCooldownMin, actionCooldownMax);

                if (logDebug)
                    Debug.Log($"{name} -> SPECIAL (FAR)");

                return;
            }

            return;
        }

        // -----------------------------
        // ENGAGE MODE (face-to-face)
        // -----------------------------
        if (inEngageRange)
        {
            if (Random.value < idleInEngageChance)
            {
                actionTimer = Random.Range(0.08f, 0.15f);
                return;
            }

            if (Random.value < blockChance)
            {
                botInput.SetKey(setup.block, true);
                blockHoldTimer = Random.Range(0.10f, 0.22f);
                actionTimer = Random.Range(0.18f, 0.30f);

                if (logDebug)
                    Debug.Log($"{name} -> BLOCK (ENGAGE)");

                return;
            }

            if (parryChance > 0f && Random.value < parryChance)
            {
                PressOneFrame(setup.parry);
                actionTimer = Random.Range(0.25f, 0.40f);

                if (logDebug)
                    Debug.Log($"{name} -> PARRY (ENGAGE)");

                return;
            }

            float attackBoost = Mathf.Clamp(faceToFaceAttackBias, 0.1f, 1.5f);
            float quickWeight = quickChance * attackBoost;
            float heavyWeight = heavyChance * attackBoost;
            float specialWeight = specialChance * 0.85f;
            float jumpWeight = jumpChance * 0.25f;
            float chargeWeight = chargeChance * 0.25f;

            float total =
                quickWeight +
                heavyWeight +
                specialWeight +
                jumpWeight +
                chargeWeight;

            if (total <= 0f)
                total = 1f;

            float r = Random.value * total;
            float cumulative = 0f;

            cumulative += quickWeight;
            if (r < cumulative)
            {
                PressOneFrame(setup.lightAttack);
                actionTimer = Random.Range(0.22f, 0.40f);

                if (logDebug)
                    Debug.Log($"{name} -> QUICK (ENGAGE)");

                return;
            }

            cumulative += heavyWeight;
            if (r < cumulative)
            {
                PressOneFrame(setup.heavyAttack);
                actionTimer = Random.Range(0.28f, 0.48f);

                if (logDebug)
                    Debug.Log($"{name} -> HEAVY (ENGAGE)");

                return;
            }

            cumulative += specialWeight;
            if (r < cumulative)
            {
                PressOneFrame(setup.ability);
                actionTimer = Random.Range(0.35f, 0.60f);

                if (logDebug)
                    Debug.Log($"{name} -> SPECIAL (ENGAGE)");

                return;
            }

            cumulative += jumpWeight;
            if (r < cumulative)
            {
                PressOneFrame(setup.up);
                actionTimer = Random.Range(0.30f, 0.50f);

                if (logDebug)
                    Debug.Log($"{name} -> JUMP (ENGAGE)");

                return;
            }

            cumulative += chargeWeight;
            if (r < cumulative)
            {
                PressOneFrame(setup.charge);
                actionTimer = Random.Range(0.40f, 0.65f);

                if (logDebug)
                    Debug.Log($"{name} -> CHARGE (ENGAGE)");

                return;
            }

            PressOneFrame(setup.lightAttack);
            actionTimer = Random.Range(0.22f, 0.40f);

            if (logDebug)
                Debug.Log($"{name} -> QUICK (ENGAGE FALLBACK)");

            return;
        }

        // -----------------------------
        // CLOSE BUT NOT PERFECT ENGAGE
        // -----------------------------
        float nearR = Random.value;
        float nearCum = 0f;

        nearCum += quickChance;
        if (nearR < nearCum)
        {
            PressOneFrame(setup.lightAttack);
            actionTimer = Random.Range(actionCooldownMin, actionCooldownMax);

            if (logDebug)
                Debug.Log($"{name} -> QUICK");

            return;
        }

        nearCum += heavyChance;
        if (nearR < nearCum)
        {
            PressOneFrame(setup.heavyAttack);
            actionTimer = Random.Range(actionCooldownMin, actionCooldownMax);

            if (logDebug)
                Debug.Log($"{name} -> HEAVY");

            return;
        }

        nearCum += specialChance;
        if (nearR < nearCum)
        {
            PressOneFrame(setup.ability);
            actionTimer = Random.Range(actionCooldownMin, actionCooldownMax);

            if (logDebug)
                Debug.Log($"{name} -> SPECIAL");

            return;
        }

        nearCum += jumpChance;
        if (nearR < nearCum)
        {
            PressOneFrame(setup.up);
            actionTimer = Random.Range(actionCooldownMin, actionCooldownMax);

            if (logDebug)
                Debug.Log($"{name} -> JUMP (NEAR)");

            return;
        }

        if (chargeChance > 0f)
        {
            nearCum += chargeChance;
            if (nearR < nearCum)
            {
                PressOneFrame(setup.charge);
                actionTimer = Random.Range(actionCooldownMin, actionCooldownMax);

                if (logDebug)
                    Debug.Log($"{name} -> CHARGE");

                return;
            }
        }
    }

    private void TriggerDropPlatform()
    {
        isDroppingThroughPlatform = true;

        forcedVerticalValue = -1f;
        forcedVerticalTimer = dropHoldDuration;

        forcedHorizontalValue = 0f;
        forcedHorizontalTimer = dropHoldDuration;
    }

    private void UpdateForcedVertical()
    {
        if (forcedVerticalTimer > 0f)
        {
            forcedVerticalTimer -= Time.deltaTime;

            if (forcedVerticalTimer <= 0f)
            {
                forcedVerticalTimer = 0f;
                forcedVerticalValue = 0f;
            }
        }
    }

    private void UpdateAntiCamp()
    {
        if (!initialized || target == null)
        {
            stackedOnPlatformTimer = 0f;
            return;
        }

        if (isDroppingThroughPlatform)
        {
            stackedOnPlatformTimer = 0f;
            return;
        }

        float dx = Mathf.Abs(target.transform.position.x - transform.position.x);
        float dySigned = target.transform.position.y - transform.position.y;

        bool targetBelow = dySigned < -sameLevelTolerance;
        bool stackedX = dx <= stackedXThreshold;

        if (targetBelow && stackedX)
        {
            stackedOnPlatformTimer += Time.deltaTime;

            if (stackedOnPlatformTimer >= stackedDropDelay)
            {
                TriggerDropPlatform();
                actionTimer = Random.Range(0.22f, 0.35f);
                stackedOnPlatformTimer = 0f;

                if (logDebug)
                    Debug.Log($"{name} -> ANTI-CAMP DROP");
            }
        }
        else
        {
            stackedOnPlatformTimer = 0f;
        }
    }

    private string GetAxisSuffix()
    {
        if (character != null && !string.IsNullOrEmpty(character.playerString))
            return character.playerString;

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
        {
            if (c != null && c != character)
                return c;
        }

        return null;
    }

    public void SetTarget(Character newTarget)
    {
        target = newTarget;
    }
}