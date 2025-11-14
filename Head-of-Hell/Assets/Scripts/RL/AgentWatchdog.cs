using UnityEngine;
using Unity.MLAgents;

[DisallowMultipleComponent]
public class AgentWatchdog : MonoBehaviour
{
    // Autowired
    FighterAgent agent;
    CharacterManager cm;
    Character character;
    Rigidbody2D rb;
    GameManager gm;

    [Header("Stall Detection")]
    public float idleSpeedEps = 0.02f;   // consider “not moving” if slower than this
    public float noChangeSeconds = 3f;   // how long of stagnation before action

    // Internals
    Vector2 lastPos;
    int lastHP;
    float t;

    [Header("Cooldown Stuck Detection")]
    public float activeColorStuckSeconds = 5f; // πόσο να περιμένει όταν βλέπει κόκκινο
    private float redStuckTimer = 0f;

    [Header("Out-of-Bounds (OOB)")]
    [Tooltip("If set, this BoxCollider2D defines the playable area (recommended).")]
    public BoxCollider2D arenaBounds;

    [Tooltip("Alternatively, define bounds via 4 transforms (x from left to right, y from floor to ceiling).")]
    public Transform boundLeft;
    public Transform boundRight;
    public Transform boundFloor;
    public Transform boundCeiling;

    [Tooltip("If neither collider nor transforms exist, use these constants.")]
    public Vector2 fallbackMin = new(-9f, -3f);
    public Vector2 fallbackMax = new( 9f,  5f);

    [Tooltip("Allow a tiny tolerance outside the box before acting.")]
    public float oobPadding = 0.25f;

    [Tooltip("Seconds outside before we intervene (debounce).")]
    public float oobGraceSeconds = 0.25f;

    [Tooltip("Reward penalty when going OOB (training only).")]
    public float oobPenalty = -0.02f;

    float oobTimer;


    void Awake()
    {
        agent = GetComponent<FighterAgent>();
        cm    = GetComponent<CharacterManager>();
        rb    = GetComponent<Rigidbody2D>();
        gm    = FindObjectOfType<GameManager>();

        if (cm != null)
        {
            cm.OnCharacterReady   += BindCharacter;
            cm.OnCharacterChanged += BindCharacter;
        }
    }

    void OnDestroy()
    {
        if (cm != null)
        {
            cm.OnCharacterReady   -= BindCharacter;
            cm.OnCharacterChanged -= BindCharacter;
        }
    }

    void BindCharacter(Character c)
    {
        character = c;
        if (character != null)
        {
            // reset watchdog baselines
            lastPos = character.transform.position;
            lastHP = character.GetCurrentHealth();
            t = 0f;
        }
    }
    
    void FindCharacter(Character c)
    {
        character = c;
        if (character != null)
        {
            // reset watchdog baselines
            lastPos = character.transform.position;
            lastHP  = character.GetCurrentHealth();
            t = 0f;
        }
    }

    void Update()
    {
        if (character == null || rb == null)
        {
            character = GetComponent<Character>();
            print("[wd5]");
            return;
        }


        // “frozen” heuristics (tweak as you like)
        bool stagnantPos = Vector2.Distance(character.transform.position, lastPos) < 0.02f;
        bool stagnantVel = rb.velocity.magnitude < idleSpeedEps;
        bool lockedState = character.IsStunned || character.IsCasting || character.IsKnocked;

        bool frozen = stagnantPos && stagnantVel && !lockedState;

        t = frozen ? t + Time.deltaTime : 0f;

        if (t >= noChangeSeconds)
        {
            Debug.LogWarning(
                $"[Watchdog] Stall on {name}. vel={rb.velocity.magnitude:0.000} " +
                $"grounded={character.IsGrounded} blocking={character.IsBlocking}");
            // Only intervene during training
            if (gm != null && gm.trainingMode)
            {
                if (agent != null)
                {
                    print("[wd10static]");
                    agent.AddReward(agent.staticPenalty);
                    gm.SoftResetRound();
                }
                else character.ResetForEpisode();
            }
            t = 0f;
        }

        // Update baselines
        lastPos = character.transform.position;
        lastHP = character.GetCurrentHealth();

        if (character != null)
        {
            bool redActive = character.IsCooldownBarActiveSprite;
            //bool legitState = character.IsCasting || character.OnAbilityCD; // τότε επιτρέπεται να είναι κόκκινο

            if (redActive /*&& !legitState*/)
            {
                redStuckTimer += Time.unscaledDeltaTime;
                t = frozen ? t + Time.unscaledDeltaTime : 0f;
                if (redStuckTimer >= activeColorStuckSeconds)
                {
                    Debug.LogWarning($"[Watchdog] Cooldown bar stuck RED on {name} for {activeColorStuckSeconds:0.0}s. Ending episode.");
                    if (gm != null && gm.trainingMode)
                    {
                        if (agent != null)
                        {
                            agent.AddReward(agent.buggedPenalty);
                            gm.SoftResetRound();
                            print("[wd1spellstuck]");
                        }
                        else
                        {
                            character.ResetForEpisode();
                        }
                    }

                    redStuckTimer = 0f; // reset το timer για να μην σπαμάρει
                }
            }
            else
            {
                redStuckTimer = 0f; // οτιδήποτε “νόμιμο” ή όχι-κόκκινο, μηδενίζει
            }
        }
        //out of bounds
        if (character != null)
        {
            if (IsOutOfBounds(character.transform.position))
            {
                oobTimer += Time.unscaledDeltaTime;

                Debug.LogWarning($"[Watchdog] {name} went out of bounds. Handling…");

                if (gm != null && gm.trainingMode)
                {
                    if (agent != null) agent.AddReward(oobPenalty);

                    // Choose ONE of the following:
                    // A) Soft reset the round (hardest sanction during training):
                    gm.SoftResetRound();
                    TeleportInsideBounds(character);

                    print("[wd22oob]");

                    // B) OR gently teleport back inside without ending episode:
                    //TeleportInsideBounds(character);
                }
                else
                {
                    // In non-training playtests, just put them back inside.
                    TeleportInsideBounds(character);
                }

                oobTimer = 0f;
            }
            else
            {
                oobTimer = 0f;
            }
        }

    }
    
     bool IsOutOfBounds(Vector2 p)
    {
        GetBounds(out Vector2 min, out Vector2 max);
        // Pad with tolerance
        min -= Vector2.one * oobPadding;
        max += Vector2.one * oobPadding;

        return (p.x < min.x || p.x > max.x || p.y < min.y || p.y > max.y);
    }

    void TeleportInsideBounds(Character ch)
    {
        if (ch == null) return;

        GetBounds(out Vector2 min, out Vector2 max);

        // Clamp to inside (minus tiny epsilon so we don’t land exactly on the wall)
        Vector3 pos = ch.transform.position;
        pos.x = Mathf.Clamp(pos.x, min.x + 0.05f, max.x - 0.05f);
        pos.y = Mathf.Clamp(pos.y, min.y + 0.05f, max.y - 0.05f);
        ch.transform.position = pos;

        // zero velocity to avoid immediately flying out again
        if (rb) rb.velocity = Vector2.zero;

        // Optional: clear “falling through platform” by re-enabling colliders
        ch.ActivateColliders();
        ch.stayDynamic();
    }

    void GetBounds(out Vector2 min, out Vector2 max)
    {
        if (arenaBounds != null)
        {
            var b = arenaBounds.bounds;
            min = b.min;
            max = b.max;
            return;
        }

        if (boundLeft && boundRight && boundFloor && boundCeiling)
        {
            min = new Vector2(boundLeft.position.x,  boundFloor.position.y);
            max = new Vector2(boundRight.position.x, boundCeiling.position.y);
            // Ensure min <= max even if markers are swapped
            if (min.x > max.x) (min.x, max.x) = (max.x, min.x);
            if (min.y > max.y) (min.y, max.y) = (max.y, min.y);
            return;
        }

        // Fallback constants
        min = fallbackMin;
        max = fallbackMax;
    }
    
}
