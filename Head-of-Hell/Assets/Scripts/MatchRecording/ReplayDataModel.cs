// ReplayData.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ReplayData
{
    public string gameVersion;
    public string sceneName;                 // e.g., "Stage 1"
    public string p1CharacterId, p2CharacterId;
    public int roundCount;                   // ruleset.rounds
    public bool chanChan;
    public int maxHealth;                    // effective value used in this round (even if random)
    public int portalNumber;                 // effective value used
    public float fixedDeltaTime;             // for safety
    public long startedAtUnix;               // timestamp

    // Per-player key maps so we can answer GetKey/GetKeyDown correctly on playback
    [Serializable] public class KeyMap {
        public int up, down, left, right, light, heavy, block, ability, charge, parry;
    }
    public KeyMap p1Keys = new();
    public KeyMap p2Keys = new();

    // One frame per Update tick (not FixedUpdate) to match your input polling timing
    [Serializable] public class Frame
    {
        public int tick;
        // axes are named by your code "Horizontal_P1/Vertical_P1", etc — store raw values:
        public float p1H, p1V, p2H, p2V;

        // Logical buttons (held / down / up)
        public ButtonSample p1, p2;
    }

    [Serializable] public class ButtonSample
    {
        public bool up, down, left, right;
        public bool light, heavy, block, ability, charge, parry;

        public bool upDown, upUp, downDown, downUp, leftDown, leftUp, rightDown, rightUp;
        public bool lightDown, lightUp, heavyDown, heavyUp, blockDown, blockUp, abilityDown, abilityUp, chargeDown, chargeUp, parryDown, parryUp;
    }

    public List<Frame> frames = new();

    public List<CombatEvent> combatEvents = new();
    public List<StateSnapshot> snapshots = new();

    public enum CombatKind { DamageApplied, KnockbackApplied, BlockStart, BlockEnd, ParryAttempt, CounterTriggered, RoundEnd }

    [Serializable]
    public class CombatEvent
    {
        public int tick;
        public CombatKind kind;

        public string attackerId;
        public string victimId;

        public string moveType;    // ή enum αν θες
        public string sourceType;

        public int hpBefore;
        public int hpAfter;

        public int damageApplied;  // actualDamage
        public bool blocked;
        public bool ignored;       // i-frames / shield negation κλπ

        // Knockback info (για το event KnockbackApplied)
        public bool kbAxisXOnly;
        public float kbForce;
        public float kbTime;
        public bool kbFromRight;
    }

    [Serializable]
    public class FighterSnapshot
    {
        public string playerId;
        public float px, py;
        public float vx, vy;
        public int hp;
        public bool isBlocking;
        public bool isGrounded;
        public bool stunned;
        public bool casting;
        public bool knocked;
        public bool ignoreDamage;
        public float facing; // +1 / -1
    }

    [Serializable]
    public class StateSnapshot
    {
        public int tick;
        public FighterSnapshot p1;
        public FighterSnapshot p2;

        // προαιρετικά: context του event που το προκάλεσε
        public int combatEventIndex;
    }
}
