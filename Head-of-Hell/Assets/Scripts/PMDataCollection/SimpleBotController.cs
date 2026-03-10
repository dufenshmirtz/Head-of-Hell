using UnityEngine;

public class SimpleBotController : MonoBehaviour
{
    [Header("Runtime References")]
    [SerializeField] private Character character;
    [SerializeField] private CharacterSetup setup;
    [SerializeField] private Character target;

    [Header("Movement")]
    [SerializeField] private float preferredRange = 2.2f;
    [SerializeField] private float tooCloseRange = 1.0f;
    [SerializeField] private float randomRetreatChance = 0.15f;

    [Header("Decision Timing")]
    [SerializeField] private float thinkIntervalMin = 0.12f;
    [SerializeField] private float thinkIntervalMax = 0.30f;
    [SerializeField] private float actionCooldownMin = 0.35f;
    [SerializeField] private float actionCooldownMax = 0.90f;

    [Header("Action Probabilities")]
    [SerializeField] private float quickChance = 0.45f;
    [SerializeField] private float heavyChance = 0.22f;
    [SerializeField] private float specialChance = 0.10f;
    [SerializeField] private float chargeChance = 0.08f;
    [SerializeField] private float jumpChance = 0.08f;
    [SerializeField] private float blockChance = 0.05f;
    [SerializeField] private float parryChance = 0.02f;

    [Header("Level Awareness")]
    [SerializeField] private float sameLevelTolerance = 0.9f;
    [SerializeField] private float jumpToTargetChance = 0.30f;

    [Header("Debug")]
    [SerializeField] private bool logDebug = false;

    private BotInputProvider botInput;
    private float thinkTimer;
    private float actionTimer;
    private float blockHoldTimer;
    private bool initialized;

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
        float dx = target.transform.position.x - transform.position.x;
        float absDx = Mathf.Abs(dx);

        float move = 0f;

        if (absDx > preferredRange)
        {
            move = Mathf.Sign(dx);
        }
        else if (absDx < tooCloseRange)
        {
            move = -Mathf.Sign(dx);
        }
        else if (Random.value < randomRetreatChance)
        {
            move = -Mathf.Sign(dx);
        }

        string suffix = "";

        if (character != null)
        {
            // π.χ. "_P1" ή "_P2"
            suffix = character.playerString;
        }

        botInput.SetAxis("Horizontal" + suffix, move);
        botInput.SetAxis("Vertical" + suffix, 0f);

        // fallbacks για ασφάλεια
        botInput.SetAxis("Horizontal" + setup.playerNum, move);
        botInput.SetAxis("Vertical" + setup.playerNum, 0f);

        botInput.SetAxis("Horizontal", move);
        botInput.SetAxis("Vertical", 0f);
    }

    private void DecideAction()
    {
        if (actionTimer > 0f)
            return;

        float dxSigned = target.transform.position.x - transform.position.x;
        float dySigned = target.transform.position.y - transform.position.y;

        float dx = Mathf.Abs(dxSigned);
        float dy = Mathf.Abs(dySigned);

        bool sameLevel = dy <= sameLevelTolerance;

        // Μικρή πιθανότητα άμυνας μόνο όταν είμαστε περίπου στο ίδιο ύψος
        if (sameLevel && Random.value < blockChance)
        {
            botInput.SetKey(setup.block, true);
            blockHoldTimer = Random.Range(0.10f, 0.25f);
            actionTimer = Random.Range(0.20f, 0.35f);

            if (logDebug)
                Debug.Log($"{name} -> BLOCK");

            return;
        }

        // Αν είμαστε κοντά οριζόντια αλλά όχι στο ίδιο ύψος,
        // μη σπαμάρεις melee attacks.
        if (dx <= preferredRange + 0.3f && !sameLevel)
        {
            // Αν ο αντίπαλος είναι πιο πάνω, προσπάθησε να ανέβεις
            if (target.transform.position.y > transform.position.y && Random.value < jumpToTargetChance)
            {
                PressOneFrame(setup.up);
                actionTimer = Random.Range(0.25f, 0.45f);

                if (logDebug)
                    Debug.Log($"{name} -> JUMP TO TARGET LEVEL");

                return;
            }

            // Αν δεν είμαστε στο ίδιο ύψος, special έχει πιο πολύ νόημα από heavy whiff
            if (Random.value < specialChance)
            {
                PressOneFrame(setup.ability);
                actionTimer = Random.Range(actionCooldownMin, actionCooldownMax);

                if (logDebug)
                    Debug.Log($"{name} -> SPECIAL (DIFFERENT HEIGHT)");

                return;
            }

            // Αλλιώς μην κάνει τίποτα αυτό το think cycle
            return;
        }

        // Αν είναι πολύ μακριά, μη βαράς στον αέρα
        if (dx > preferredRange + 0.15f)
        {
            float farRoll = Random.value;

            if (farRoll < jumpChance)
            {
                PressOneFrame(setup.up);
                actionTimer = Random.Range(actionCooldownMin, actionCooldownMax);

                if (logDebug)
                    Debug.Log($"{name} -> JUMP");

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

        // Από εδώ και κάτω: κοντά ΚΑΙ στο ίδιο ύψος
        float r = Random.value;
        float cumulative = 0f;

        cumulative += quickChance;
        if (r < cumulative)
        {
            PressOneFrame(setup.lightAttack);
            actionTimer = Random.Range(actionCooldownMin, actionCooldownMax);

            if (logDebug)
                Debug.Log($"{name} -> QUICK");

            return;
        }

        cumulative += heavyChance;
        if (r < cumulative)
        {
            PressOneFrame(setup.heavyAttack);
            actionTimer = Random.Range(actionCooldownMin, actionCooldownMax);

            if (logDebug)
                Debug.Log($"{name} -> HEAVY");

            return;
        }

        cumulative += specialChance;
        if (r < cumulative)
        {
            PressOneFrame(setup.ability);
            actionTimer = Random.Range(actionCooldownMin, actionCooldownMax);

            if (logDebug)
                Debug.Log($"{name} -> SPECIAL");

            return;
        }

        cumulative += jumpChance;
        if (r < cumulative)
        {
            PressOneFrame(setup.up);
            actionTimer = Random.Range(actionCooldownMin, actionCooldownMax);

            if (logDebug)
                Debug.Log($"{name} -> JUMP (NEAR)");

            return;
        }
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