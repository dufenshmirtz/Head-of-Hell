using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Unity.MLAgents;

public class TrainingSafety : MonoBehaviour
{
    [Header("Enable")]
    public bool enable = true;

    [Tooltip("If set, safety runs only when a communicator is connected (mlagents-learn).")]
    public bool onlyWhenTrainingConnected = true;

    [Header("References")]
    public GameManager gameManager;               // drag GameManager (or leave null if this is on GM)
    public CharacterManager p1Manager;            // drag
    public CharacterManager p2Manager;            // drag
    public FighterAgent agentP1;                  // optional (can be null)
    public FighterAgent agentP2;                  // optional (can be null)

    [Tooltip("A collider that represents the playable arena bounds.")]
    public Collider2D arenaBounds;                // drag your ready collider here

    [Header("Out of bounds")]
    public float oobPadding = 0.5f;
    public float oobSeconds = 1.0f;

    [Header("Stall / Freeze")]
    [Tooltip("If BOTH fighters are basically not moving for this long -> reset.")]
    public float stallSecondsBoth = 10f;

    [Tooltip("If ONE fighter is basically not moving for this long (while the other is alive) -> reset.")]
    public float stallSecondsSingle = 20f;

    [Tooltip("Velocity magnitude below this counts as 'not moving'.")]
    public float velEps = 0.05f;

    [Tooltip("Position change below this counts as 'not moving'.")]
    public float posEps = 0.02f;

    [Header("Persistent invalid state")]
    [Tooltip("If gravityScale == 0 (or body static / colliders off) persists this long -> reset.")]
    public float invalidStateSeconds = 10.0f;

    [Header("Episode hard timeout (sim seconds)")]
    public float episodeTimeoutSeconds = 500f;

    [Header("Time base")]
    [Tooltip("If true, timers use scaled time (affected by Time.timeScale). This makes thresholds 'scale' when you speed up training.")]
    public bool useScaledTime = true;

    public float minX = -9.5f;
    public float maxX = 9.5f;
    public float minY = -3f;
    public float maxY = 6f;

    // --- internal timers ---
    float oobTimer;
    float stallTimerBoth;
    float stallTimerP1;
    float stallTimerP2;
    float invalidTimerP1;
    float invalidTimerP2;
    float episodeTimer;

    Vector2 lastPos1, lastPos2;

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
            // If you're not running via mlagents-learn, don't interfere.
            if (Academy.Instance == null || !Academy.Instance.IsCommunicatorOn) return;
        }

        if (gameManager == null || !gameManager.trainingMode) return;
        if (arenaBounds == null) return;

        var c1 = p1Manager ? p1Manager.GetCurrentCharacter() : null;
        var c2 = p2Manager ? p2Manager.GetCurrentCharacter() : null;
        if (c1 == null || c2 == null) return;

        float dt = useScaledTime ? Time.deltaTime : Time.unscaledDeltaTime;
        episodeTimer += dt;

        // 1) OOB
        if (IsOutOfBounds(c1.transform.position) || IsOutOfBounds(c2.transform.position))
            oobTimer += dt;
        else
            oobTimer = 0f;

        if (oobTimer >= oobSeconds)
        {
            SafetyReset("OOB");
            return;
        }

        // 2) Persistent invalid physics/state (with grace time)
        invalidTimerP1 = UpdateInvalidTimer(c1, invalidTimerP1, dt);
        invalidTimerP2 = UpdateInvalidTimer(c2, invalidTimerP2, dt);

        if (invalidTimerP1 >= invalidStateSeconds || invalidTimerP2 >= invalidStateSeconds)
        {
            SafetyReset("INVALID_STATE");
            return;
        }

        // 3) Stall detection
        UpdateStallTimers(c1, c2, dt);

        if (stallTimerBoth >= stallSecondsBoth)
        {
            SafetyReset("STALL_BOTH");
            return;
        }

        // single-fighter stall (only if the other one is alive-ish)
        if (stallTimerP1 >= stallSecondsSingle || stallTimerP2 >= stallSecondsSingle)
        {
            SafetyReset("STALL_SINGLE");
            return;
        }

        // 4) Hard timeout
        if (episodeTimer >= episodeTimeoutSeconds)
        {
            SafetyReset("TIMEOUT");
            return;
        }

        // update last positions
        lastPos1 = c1.transform.position;
        lastPos2 = c2.transform.position;
    }

    bool IsOutOfBounds(Vector3 p)
    {
        return (p.x < minX + oobPadding ||
                p.x > maxX - oobPadding ||
                p.y < minY + oobPadding ||
                p.y > maxY - oobPadding);
    }

    float UpdateInvalidTimer(Character c, float timer, float dt)
    {
        // We treat as invalid if it looks "bugged" AND NOT just a momentary animation moment.
        // So: we only accumulate timer when the condition persists.

        var rb = c.GetComponent<Rigidbody2D>();
        var cols = c.GetComponents<Collider2D>();

        bool anyColliderEnabled = cols != null && cols.Length > 0 && cols.Any(col => col != null && col.enabled);

        bool isDead = GetBool(c, "IsDead", false);
        if (!isDead)
        {
            // common animator bool fallback
            var anim = c.GetComponent<Animator>();
            if (anim != null)
            {
                try { isDead = anim.GetBool("isDead"); } catch { /* ignore */ }
            }
        }

        bool isCasting  = GetBool(c, "IsCasting", false);
        bool isCharging = GetBool(c, "IsCharging", false);
        bool isStunned  = GetBool(c, "IsStunned", false);
        bool isKnocked  = GetBool(c, "IsKnocked", false);

        // during death/certain states you may intentionally disable stuff
        bool allowedToBeWeird = isDead || isCasting || isCharging || isStunned || isKnocked;

        bool invalid =
            !allowedToBeWeird &&
            (
                (rb != null && rb.bodyType == RigidbodyType2D.Static) ||
                (rb != null && Mathf.Abs(rb.gravityScale) < 0.001f) ||
                (!anyColliderEnabled)
            );

        if (invalid) return timer + dt;
        return 0f;
    }

    void UpdateStallTimers(Character c1, Character c2, float dt)
    {
        var rb1 = c1.GetComponent<Rigidbody2D>();
        var rb2 = c2.GetComponent<Rigidbody2D>();

        Vector2 p1 = c1.transform.position;
        Vector2 p2 = c2.transform.position;

        float v1 = rb1 ? rb1.velocity.magnitude : 0f;
        float v2 = rb2 ? rb2.velocity.magnitude : 0f;

        float dp1 = (p1 - lastPos1).magnitude;
        float dp2 = (p2 - lastPos2).magnitude;

        bool p1Still = (v1 < velEps) && (dp1 < posEps);
        bool p2Still = (v2 < velEps) && (dp2 < posEps);

        bool p1Dead = GetBool(c1, "IsDead", false);
        bool p2Dead = GetBool(c2, "IsDead", false);

        bool p1Locked = GetBool(c1, "IsCasting", false) || GetBool(c1, "IsStunned", false) || GetBool(c1, "IsKnocked", false) || GetBool(c1, "IsCharging", false);
        bool p2Locked = GetBool(c2, "IsCasting", false) || GetBool(c2, "IsStunned", false) || GetBool(c2, "IsKnocked", false) || GetBool(c2, "IsCharging", false);

        // If they are "still" BUT they're locked by legit states, don't count it as stall
        if (p1Still && !p1Locked && !p1Dead) stallTimerP1 += dt; else stallTimerP1 = 0f;
        if (p2Still && !p2Locked && !p2Dead) stallTimerP2 += dt; else stallTimerP2 = 0f;

        if (p1Still && p2Still && !p1Locked && !p2Locked && !p1Dead && !p2Dead) stallTimerBoth += dt;
        else stallTimerBoth = 0f;
    }

    void SafetyReset(string reason)
    {
        Debug.LogWarning($"[TRAINING SAFETY] Reset: {reason}");

        // Optional: small penalty so policy avoids states that trigger safety reset
        // (If you want it, uncomment; otherwise keep it clean.)
        // if (agentP1) agentP1.AddReward(-0.05f);
        // if (agentP2) agentP2.AddReward(-0.05f);

        // Reset timers
        oobTimer = 0f;
        stallTimerBoth = 0f;
        stallTimerP1 = 0f;
        stallTimerP2 = 0f;
        invalidTimerP1 = 0f;
        invalidTimerP2 = 0f;
        episodeTimer = 0f;

        // Call your existing reset path
        if (gameManager != null)
            gameManager.SoftResetRound(0);

        CacheLastPositions();
    }

    void CacheLastPositions()
    {
        var c1 = p1Manager ? p1Manager.GetCurrentCharacter() : null;
        var c2 = p2Manager ? p2Manager.GetCurrentCharacter() : null;
        if (c1) lastPos1 = c1.transform.position;
        if (c2) lastPos2 = c2.transform.position;
    }

    // Reflection helper: won't break compile if properties don't exist.
    bool GetBool(object obj, string propName, bool fallback)
    {
        if (obj == null) return fallback;
        try
        {
            var t = obj.GetType();
            var p = t.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            if (p != null && p.PropertyType == typeof(bool))
                return (bool)p.GetValue(obj);

            var f = t.GetField(propName, BindingFlags.Public | BindingFlags.Instance);
            if (f != null && f.FieldType == typeof(bool))
                return (bool)f.GetValue(obj);
        }
        catch { }
        return fallback;
    }
}