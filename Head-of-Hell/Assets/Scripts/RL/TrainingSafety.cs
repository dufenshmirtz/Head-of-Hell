using System.Linq;
using UnityEngine;
using Unity.MLAgents;

public class TrainingSafety : MonoBehaviour
{
    [Header("Enable")]
    public bool enable = true;

    [Tooltip("If true, safety runs only when a communicator is connected (mlagents-learn).")]
    public bool onlyWhenTrainingConnected = true;

    [Header("References")]
    public GameManager gameManager;       // drag or leave null if on same GO
    public CharacterManager p1Manager;    // drag
    public CharacterManager p2Manager;    // drag

    [Header("Time base")]
    [Tooltip("Use unscaled time so thresholds stay consistent even if you change Time.timeScale.")]
    public bool useUnscaledTime = true;

    [Header("Manual Arena Bounds (world space)")]
    public float minX = -9.5f;
    public float maxX = 9.5f;
    public float minY = -3.0f;
    public float maxY = 6.0f;
    public float oobSeconds = 1.0f;

    [Header("Hard episode timeout (seconds)")]
    public float episodeTimeoutSeconds = 500f;

    [Header("Stall / Same-position")]
    [Tooltip("If BOTH fighters are basically in the same spot for this long -> reset.")]
    public float stallSecondsBoth = 10f;

    [Tooltip("If ONE fighter is basically in the same spot for this long -> reset.")]
    public float stallSecondsSingle = 20f;

    [Tooltip("Counts as not moving if velocity magnitude is below this.")]
    public float velEps = 0.05f;

    [Tooltip("Counts as not moving if position change since last frame is below this.")]
    public float posEps = 0.02f;

    [Header("Weird-state timeouts (seconds)")]
    [Tooltip("If gravityScale~0 OR body Static OR all colliders off OR animator isDead stays true for this long -> reset.")]
    public float weirdStateSeconds = 10f;

    [Tooltip("If IsCasting stays true continuously for this long -> reset.")]
    public float castingSeconds = 10f;

    [Tooltip("If IsCharging stays true continuously for this long -> reset (prevents charge-hold exploits/stucks).")]
    public float chargingSeconds = 10f;

    [Header("Optional penalties (apply before reset)")]
    public bool applyPenaltyOnSafetyReset = false;
    public float penaltyValue = -0.02f;
    public FighterAgent agentP1;
    public FighterAgent agentP2;

    // Timers
    float episodeTimer;
    float oobTimer;

    float stallTimerBoth;
    float stallTimerP1;
    float stallTimerP2;

    float weirdTimerP1;
    float weirdTimerP2;

    float castTimerP1;
    float castTimerP2;

    float chargeTimerP1;
    float chargeTimerP2;

    Vector2 lastPos1;
    Vector2 lastPos2;
    bool hasLastPos;

    bool oob=false;

    void Awake()
    {
        if (gameManager == null) gameManager = GetComponent<GameManager>();
    }

    void Start()
    {
        CacheLastPositions();
    }

    void Update()
    {
        if (!enable) return;

        if (onlyWhenTrainingConnected)
        {
            if (Academy.Instance == null || !Academy.Instance.IsCommunicatorOn) return;
        }

        if (gameManager == null || !gameManager.trainingMode) return;

        var c1 = p1Manager ? p1Manager.GetCurrentCharacter() : null;
        var c2 = p2Manager ? p2Manager.GetCurrentCharacter() : null;
        if (c1 == null || c2 == null) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (dt <= 0f) return;

        episodeTimer += dt;

        // 1) OOB (manual bounds)
        if (IsOutOfBounds(c1.transform.position) || IsOutOfBounds(c2.transform.position))
            oobTimer += dt;
        else
            oobTimer = 0f;

        if (oobTimer >= oobSeconds)
        {
            SafetyReset("OOB");
            return;
        }

        // 2) Weird-state (physics / colliders / dead animator stuck)
        weirdTimerP1 = UpdateWeirdTimer(c1, weirdTimerP1, dt);
        weirdTimerP2 = UpdateWeirdTimer(c2, weirdTimerP2, dt);

        if (weirdTimerP1 >= weirdStateSeconds || weirdTimerP2 >= weirdStateSeconds)
        {
            SafetyReset("WEIRD_STATE_TIMEOUT");
            return;
        }

        // 3) Casting / Charging long holds (continuous)
        castTimerP1 = UpdateContinuousTimer(c1, castTimerP1, dt, cond: IsCastingLong);
        castTimerP2 = UpdateContinuousTimer(c2, castTimerP2, dt, cond: IsCastingLong);

        if (castTimerP1 >= castingSeconds || castTimerP2 >= castingSeconds)
        {
            SafetyReset("CASTING_TIMEOUT");
            return;
        }

        chargeTimerP1 = UpdateContinuousTimer(c1, chargeTimerP1, dt, cond: IsChargingLong);
        chargeTimerP2 = UpdateContinuousTimer(c2, chargeTimerP2, dt, cond: IsChargingLong);

        if (chargeTimerP1 >= chargingSeconds || chargeTimerP2 >= chargingSeconds)
        {
            SafetyReset("CHARGE_HOLD_TIMEOUT");
            return;
        }

        // 4) Stall / same-position (per fighter + both)
        UpdateStallTimers(c1, c2, dt);

        if (stallTimerBoth >= stallSecondsBoth)
        {
            SafetyReset("STALL_BOTH");
            return;
        }

        if (stallTimerP1 >= stallSecondsSingle || stallTimerP2 >= stallSecondsSingle)
        {
            SafetyReset("STALL_SINGLE");
            return;
        }

        // 5) Hard timeout
        if (episodeTimer >= episodeTimeoutSeconds)
        {
            SafetyReset("TIMEOUT");
            return;
        }

        // Update last positions
        lastPos1 = c1.transform.position;
        lastPos2 = c2.transform.position;
        hasLastPos = true;
    }

    bool IsOutOfBounds(Vector3 p)
    {
        oob=p.x < minX  || p.x > maxX || p.y < minY || p.y > maxY;
        print(p.x+" ?? "+p.y+" "+ oob);
        return oob;
    }

    float UpdateWeirdTimer(Character c, float timer, float dt)
    {
        if (c == null) return 0f;

        // If dead by HP, don't count weird; normal death/reset flow will handle it
        if (c.GetCurrentHealth() <= 0) return 0f;

        var rb = c.GetComponent<Rigidbody2D>();
        var cols = c.GetComponents<Collider2D>();
        bool anyColliderEnabled = cols != null && cols.Length > 0 && cols.Any(col => col != null && col.enabled);

        bool gravityZero = (rb != null && Mathf.Abs(rb.gravityScale) < 0.001f);
        bool staticBody  = (rb != null && rb.bodyType == RigidbodyType2D.Static);
        bool noColliders = !anyColliderEnabled;

        bool animDead = false;
        var anim = c.GetComponent<Animator>();
        if (anim != null)
        {
            try { animDead = anim.GetBool("isDead"); } catch { animDead = false; }
        }

        bool weird = gravityZero || staticBody || noColliders || animDead;

        return weird ? (timer + dt) : 0f;
    }

    // Generic "continuous condition" timer
    float UpdateContinuousTimer(Character c, float timer, float dt, System.Func<Character, bool> cond)
    {
        if (c == null) return 0f;
        if (c.GetCurrentHealth() <= 0) return 0f;

        bool on = false;
        try { on = cond(c); } catch { on = false; }

        return on ? (timer + dt) : 0f;
    }

    bool IsCastingLong(Character c)
    {
        // Uses your public property
        return c.IsCasting;
    }

    bool IsChargingLong(Character c)
    {
        return c.IsCharging;
    }

    void UpdateStallTimers(Character c1, Character c2, float dt)
    {
        var rb1 = c1.GetComponent<Rigidbody2D>();
        var rb2 = c2.GetComponent<Rigidbody2D>();

        Vector2 p1 = c1.transform.position;
        Vector2 p2 = c2.transform.position;

        float v1 = rb1 ? rb1.velocity.magnitude : 0f;
        float v2 = rb2 ? rb2.velocity.magnitude : 0f;

        float dp1 = hasLastPos ? (p1 - lastPos1).magnitude : 0f;
        float dp2 = hasLastPos ? (p2 - lastPos2).magnitude : 0f;

        bool p1Still = (v1 < velEps) && (dp1 < posEps);
        bool p2Still = (v2 < velEps) && (dp2 < posEps);

        bool p1Alive = c1.GetCurrentHealth() > 0;
        bool p2Alive = c2.GetCurrentHealth() > 0;

        // Treat locked states as "still" too — if they last too long, reset will happen via casting/charging timers anyway.
        if (p1Still && p1Alive) stallTimerP1 += dt; else stallTimerP1 = 0f;
        if (p2Still && p2Alive) stallTimerP2 += dt; else stallTimerP2 = 0f;

        if (p1Still && p2Still && p1Alive && p2Alive) stallTimerBoth += dt;
        else stallTimerBoth = 0f;
    }

    void SafetyReset(string reason)
    {
        Debug.LogWarning($"[TRAINING SAFETY] Reset: {reason}");

        if (applyPenaltyOnSafetyReset)
        {
            if (agentP1) agentP1.AddReward(penaltyValue);
            if (agentP2) agentP2.AddReward(penaltyValue);
        }

        ResetTimers();

        if (gameManager != null)
            gameManager.SoftResetRound(0);

        CacheLastPositions();
    }

    void ResetTimers()
    {
        episodeTimer = 0f;
        oobTimer = 0f;

        stallTimerBoth = 0f;
        stallTimerP1 = 0f;
        stallTimerP2 = 0f;

        weirdTimerP1 = 0f;
        weirdTimerP2 = 0f;

        castTimerP1 = 0f;
        castTimerP2 = 0f;

        chargeTimerP1 = 0f;
        chargeTimerP2 = 0f;
    }

    void CacheLastPositions()
    {
        var c1 = p1Manager ? p1Manager.GetCurrentCharacter() : null;
        var c2 = p2Manager ? p2Manager.GetCurrentCharacter() : null;

        if (c1) lastPos1 = c1.transform.position;
        if (c2) lastPos2 = c2.transform.position;

        hasLastPos = (c1 != null && c2 != null);
    }
}