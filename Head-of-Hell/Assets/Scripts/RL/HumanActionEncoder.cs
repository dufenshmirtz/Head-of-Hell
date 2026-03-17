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

    public static DiscreteActionFrame Encode(Character self)
    {
        DiscreteActionFrame a = Default();

        if (self == null)
            return a;

        KeyCode leftKey    = self.left;
        KeyCode rightKey   = self.right;
        KeyCode upKey      = self.up;
        KeyCode downKey    = self.down;
        KeyCode lightKey   = self.lightAttack;
        KeyCode heavyKey   = self.heavyAttack;
        KeyCode blockKey   = self.block;
        KeyCode abilityKey = self.ability;
        KeyCode chargeKey  = self.charge;
        KeyCode parryKey   = self.parry;

        string playerString = self.playerString;

        bool controllerEnabled = IsControllerEnabled(self);
        int joystickNum = GetJoystickNumber(self);

        // -----------------------------
        // MOVE branch: 0=left, 1=idle, 2=right
        // -----------------------------
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

        // -----------------------------
        // Vertical axis
        // -----------------------------
        float axisV = 0f;
        if (!string.IsNullOrEmpty(playerString))
            axisV = Input.GetAxis("Vertical" + playerString);

        bool axisUpNow = axisV > 0.5f;
        bool axisDownNow = axisV < -0.5f;

        // -----------------------------
        // JUMP
        // Character uses:
        // input.GetKeyDown(up) || (axisUp && !jumpAxisHeld)
        // -----------------------------
        bool keyJumpDown = Input.GetKeyDown(upKey);
        bool axisUpPressed = AxisMemory.GetUpPressed(playerString, axisUpNow);

        a.jump = (keyJumpDown || axisUpPressed) ? 1 : 0;

        // -----------------------------
        // DROP
        // Character uses:
        // input.GetKeyDown(down) || (controller && axis < -0.5f)
        // We log edge on axis down to avoid spam every frame.
        // -----------------------------
        bool keyDropDown = Input.GetKeyDown(downKey);
        bool axisDownPressed = AxisMemory.GetDownPressed(playerString, axisDownNow);

        a.drop = (keyDropDown || (controllerEnabled && axisDownPressed)) ? 1 : 0;

        // -----------------------------
        // LIGHT
        // Character:
        // keyboard -> key
        // controller -> joystick button 0
        // -----------------------------
        bool lightPressed =
            Input.GetKeyDown(lightKey) ||
            (controllerEnabled && Input.GetKeyDown(GetJoystickButtonString(joystickNum, 0)));

        a.light = lightPressed ? 1 : 0;

        // -----------------------------
        // HEAVY
        // controller -> joystick button 2
        // -----------------------------
        bool heavyPressed =
            Input.GetKeyDown(heavyKey) ||
            (controllerEnabled && Input.GetKeyDown(GetJoystickButtonString(joystickNum, 2)));

        a.heavy = heavyPressed ? 1 : 0;

        // -----------------------------
        // SPECIAL
        // controller -> joystick button 3
        // -----------------------------
        bool specialPressed =
            Input.GetKeyDown(abilityKey) ||
            (controllerEnabled && Input.GetKeyDown(GetJoystickButtonString(joystickNum, 3)));

        a.special = specialPressed ? 1 : 0;

        // -----------------------------
        // PARRY
        // controller -> joystick button 4
        // -----------------------------
        bool parryPressed =
            Input.GetKeyDown(parryKey) ||
            (controllerEnabled && Input.GetKeyDown(GetJoystickButtonString(joystickNum, 4)));

        a.parry = parryPressed ? 1 : 0;

        // -----------------------------
        // BLOCK (hold)
        // controller -> joystick button 5
        // -----------------------------
        bool blockHeld =
            Input.GetKey(blockKey) ||
            (controllerEnabled && Input.GetKey(GetJoystickButtonString(joystickNum, 5)));

        a.block = blockHeld ? 1 : 0;

        // -----------------------------
        // CHARGE
        // controller -> joystick button 1
        // 0=none, 1=hold, 2=release
        // -----------------------------
        bool chargeHeld =
            Input.GetKey(chargeKey) ||
            (controllerEnabled && Input.GetKey(GetJoystickButtonString(joystickNum, 1)));

        bool chargeReleased =
            Input.GetKeyUp(chargeKey) ||
            (controllerEnabled && Input.GetKeyUp(GetJoystickButtonString(joystickNum, 1)));

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

    private static bool IsControllerEnabled(Character self)
    {
        if (self == null)
            return false;

        string[] names = Input.GetJoystickNames();
        int connectedCount = 0;

        foreach (string n in names)
        {
            if (!string.IsNullOrEmpty(n))
                connectedCount++;
        }

        if (self.PlayerId == "P1")
            return connectedCount >= 2;

        if (self.PlayerId == "P2")
            return connectedCount >= 1;

        return false;
    }

    private static int GetJoystickNumber(Character self)
    {
        if (self == null)
            return 1;

        // Match Character.ControllerNum(playerNum)
        return self.PlayerId == "P1" ? 2 : 1;
    }

    private static string GetJoystickButtonString(int joystickNum, int buttonNum)
    {
        return "joystick " + joystickNum + " button " + buttonNum;
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