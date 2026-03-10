using UnityEngine;

public static class HumanActionEncoder
{
    [System.Serializable]
    public struct DiscreteActionFrame
    {
        // Same branch layout as FighterAgent
        // 0: Move   (0=left, 1=idle, 2=right)
        // 1: Jump   (0/1)
        // 2: Drop   (0/1)
        // 3: Light  (0/1)
        // 4: Heavy  (0/1)
        // 5: Block  (0/1)
        // 6: Special(0/1)
        // 7: Charge (0=none, 1=hold, 2=release)
        // 8: Parry  (0/1)

        public int move;
        public int jump;
        public int drop;
        public int light;
        public int heavy;
        public int block;
        public int special;
        public int charge;
        public int parry;

        public int[] ToArray()
        {
            return new int[]
            {
                move, jump, drop, light, heavy, block, special, charge, parry
            };
        }
    }

    private static readonly int[] EmptyArray = new int[9] { 1, 0, 0, 0, 0, 0, 0, 0, 0 };

    public static DiscreteActionFrame Encode(Character self)
    {
        DiscreteActionFrame a = Default();

        if (self == null)
            return a;

        // --- movement keys from character setup ---
        KeyCode leftKey   = self.left;
        KeyCode rightKey  = self.right;
        KeyCode upKey     = self.up;
        KeyCode downKey   = self.down;
        KeyCode lightKey  = self.lightAttack;
        KeyCode heavyKey  = self.heavyAttack;
        KeyCode blockKey  = self.block;
        KeyCode abilityKey= self.ability;
        KeyCode chargeKey = self.charge;
        KeyCode parryKey  = self.parry;

        string playerString = self.playerString;

        // --------------------------------------------------
        // MOVE branch: 0=left, 1=idle, 2=right
        // Match the game's intent as closely as possible.
        // Character.Update uses input.GetAxis("Horizontal"+playerString),
        // but for human input we also check keyboard keys directly.
        // --------------------------------------------------
        bool leftHeld = Input.GetKey(leftKey);
        bool rightHeld = Input.GetKey(rightKey);

        float axisH = 0f;
        if (!string.IsNullOrEmpty(playerString))
            axisH = Input.GetAxis("Horizontal" + playerString);

        if (leftHeld && !rightHeld)
            a.move = 0;
        else if (rightHeld && !leftHeld)
            a.move = 2;
        else if (axisH < -0.5f)
            a.move = 0;
        else if (axisH > 0.5f)
            a.move = 2;
        else
            a.move = 1;

        // --------------------------------------------------
        // JUMP branch: edge-like action
        // Character uses:
        // input.GetKeyDown(up) || (axisUp && !jumpAxisHeld)
        // We cannot access jumpAxisHeld here, so for now:
        // - keyboard: GetKeyDown(up)
        // - controller axis up: detect when vertical axis > 0.5
        //   using previous axis state stored per player
        // --------------------------------------------------
        bool keyJumpDown = Input.GetKeyDown(upKey);

        float axisV = 0f;
        if (!string.IsNullOrEmpty(playerString))
            axisV = Input.GetAxis("Vertical" + playerString);

        bool axisUpNow = axisV > 0.5f;
        bool axisUpPressed = AxisMemory.GetUpPressed(playerString, axisUpNow);

        a.jump = (keyJumpDown || axisUpPressed) ? 1 : 0;

        // --------------------------------------------------
        // DROP branch
        // Character uses:
        // input.GetKeyDown(down) || (controller && axis < -0.5f)
        // For logging:
        // - keyboard edge on down key
        // - controller-style edge on vertical axis down
        // --------------------------------------------------
        bool keyDropDown = Input.GetKeyDown(downKey);
        bool axisDownNow = axisV < -0.5f;
        bool axisDownPressed = AxisMemory.GetDownPressed(playerString, axisDownNow);

        a.drop = (keyDropDown || axisDownPressed) ? 1 : 0;

        // --------------------------------------------------
        // LIGHT / HEAVY / SPECIAL / PARRY
        // edge actions
        // --------------------------------------------------
        a.light   = Input.GetKeyDown(lightKey)   ? 1 : 0;
        a.heavy   = Input.GetKeyDown(heavyKey)   ? 1 : 0;
        a.special = Input.GetKeyDown(abilityKey) ? 1 : 0;
        a.parry   = Input.GetKeyDown(parryKey)   ? 1 : 0;

        // --------------------------------------------------
        // BLOCK
        // hold action
        // --------------------------------------------------
        a.block = Input.GetKey(blockKey) ? 1 : 0;

        // --------------------------------------------------
        // CHARGE
        // branch: 0=none, 1=hold, 2=release
        //
        // Character uses:
        // - GetKeyDown(charge) to start
        // - GetKeyUp(charge) to release
        //
        // For imitation / RL compatibility, it is better to log:
        // - hold while held
        // - release on the release frame
        // --------------------------------------------------
        bool chargeHeld = Input.GetKey(chargeKey);
        bool chargeReleased = Input.GetKeyUp(chargeKey);

        if (chargeReleased)
            a.charge = 2;
        else if (chargeHeld)
            a.charge = 1;
        else
            a.charge = 0;

        return a;
    }

    public static DiscreteActionFrame Default()
    {
        return new DiscreteActionFrame
        {
            move = 1,
            jump = 0,
            drop = 0,
            light = 0,
            heavy = 0,
            block = 0,
            special = 0,
            charge = 0,
            parry = 0
        };
    }

    // Keeps minimal per-player axis edge memory for up/down detection.
    private static class AxisMemory
    {
        private static bool p1UpLast;
        private static bool p2UpLast;
        private static bool p1DownLast;
        private static bool p2DownLast;

        public static bool GetUpPressed(string playerString, bool current)
        {
            if (playerString == "_P1")
            {
                bool pressed = current && !p1UpLast;
                p1UpLast = current;
                return pressed;
            }

            if (playerString == "_P2")
            {
                bool pressed = current && !p2UpLast;
                p2UpLast = current;
                return pressed;
            }

            return false;
        }

        public static bool GetDownPressed(string playerString, bool current)
        {
            if (playerString == "_P1")
            {
                bool pressed = current && !p1DownLast;
                p1DownLast = current;
                return pressed;
            }

            if (playerString == "_P2")
            {
                bool pressed = current && !p2DownLast;
                p2DownLast = current;
                return pressed;
            }

            return false;
        }
    }
}