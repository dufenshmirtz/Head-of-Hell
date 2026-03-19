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

    private BotInputProvider botInput;
    private float thinkTimer;
    private float actionTimer;
    private float blockHoldTimer;

    private float forcedVerticalTimer;
    private float forcedVerticalValue;
    private bool isDroppingThroughPlatform;

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

        initialized = false;
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

        UpdateForcedVertical();

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

        // ⭐ Anti-top-camp
        if (targetBelow && dx < 0.6f && !isDroppingThroughPlatform)
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

        // --- HORIZONTAL ---
        string suffix = GetAxisSuffix();

        botInput.SetAxis("Horizontal" + suffix, move);
        botInput.SetAxis("Horizontal" + setup.playerNum, move);
        botInput.SetAxis("Horizontal", move);

        // --- VERTICAL (important for drop) ---
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
    // PLATFORM DROP
    // =========================================================

    private void TriggerDropPlatform()
    {
        isDroppingThroughPlatform = true;

        // ⭐ SEND DOWN KEY PRESS (edge trigger)
        PressOneFrame(setup.down);

        // Κράτα και vertical για σιγουριά
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
}