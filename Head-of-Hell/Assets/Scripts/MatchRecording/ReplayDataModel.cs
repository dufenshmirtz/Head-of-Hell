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
}
