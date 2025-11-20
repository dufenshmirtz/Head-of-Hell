// ReplayTap.cs
using UnityEngine;

[DefaultExecutionOrder(200)] // after Character.Update has read input is also fine
public class ReplayTap : MonoBehaviour
{
    public enum Side { P1, P2 }
    public Side side;

    Character character;
    IInputProvider input;
    CharacterSetup setup;

    string hAxis, vAxis;
    bool registered;

    void Start()
    {
        character = GetComponent<Character>();
        setup     = GetComponent<CharacterSetup>();
        input     = character.GetInputProvider();

        bool isP1 = (setup.playerNum == 1) || (side == Side.P1);
        hAxis = "Horizontal" + (isP1 ? "_P1" : "_P2");
        vAxis = "Vertical"   + (isP1 ? "_P1" : "_P2");
    }

    void Update()
    {
        // not recording? nothing to do
        if (ReplayRecorder.Instance == null || !ReplayRecorder.Instance.Recording) return;

        // first Update for this tap: tell recorder this side exists
        if (!registered)
        {
            if (setup.playerNum == 1) ReplayRecorder.Instance.RegisterP1();
            else                      ReplayRecorder.Instance.RegisterP2();
            registered = true;
        }

        // Gather keys for this side
        var b = new ReplayData.ButtonSample {
            up     = input.GetKey(setup.up),
            down   = input.GetKey(setup.down),
            left   = input.GetKey(setup.left),
            right  = input.GetKey(setup.right),
            light  = input.GetKey(setup.lightAttack),
            heavy  = input.GetKey(setup.heavyAttack),
            block  = input.GetKey(setup.block),
            ability= input.GetKey(setup.ability),
            charge = input.GetKey(setup.charge),
            parry  = input.GetKey(setup.parry),

            upDown     = input.GetKeyDown(setup.up),      upUp     = input.GetKeyUp(setup.up),
            downDown   = input.GetKeyDown(setup.down),    downUp   = input.GetKeyUp(setup.down),
            leftDown   = input.GetKeyDown(setup.left),    leftUp   = input.GetKeyUp(setup.left),
            rightDown  = input.GetKeyDown(setup.right),   rightUp  = input.GetKeyUp(setup.right),
            lightDown  = input.GetKeyDown(setup.lightAttack),  lightUp  = input.GetKeyUp(setup.lightAttack),
            heavyDown  = input.GetKeyDown(setup.heavyAttack),  heavyUp  = input.GetKeyUp(setup.heavyAttack),
            blockDown  = input.GetKeyDown(setup.block),   blockUp  = input.GetKeyUp(setup.block),
            abilityDown= input.GetKeyDown(setup.ability), abilityUp= input.GetKeyUp(setup.ability),
            chargeDown = input.GetKeyDown(setup.charge),  chargeUp = input.GetKeyUp(setup.charge),
            parryDown  = input.GetKeyDown(setup.parry),   parryUp  = input.GetKeyUp(setup.parry),
        };

        // Fill this side’s buttons
        if (setup.playerNum == 1) ReplayRecorder.Instance.FillP1(b);
        else                       ReplayRecorder.Instance.FillP2(b);

        // Commit once both sides have filled (or single-player)
        ReplayRecorder.Instance.CommitFrameIfComplete();
    }
}
