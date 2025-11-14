// ReplayDirector.cs
using UnityEngine;

[DefaultExecutionOrder(0)] // normal order, after ReplayPlayer but before most UI
public class ReplayDirector : MonoBehaviour
{
    Character p1, p2;
    IInputProvider in1, in2;
    string h1, v1, h2, v2;
    CharacterSetup s1, s2;

    void Start()
    {
        // find fighters
        foreach (var st in FindObjectsOfType<CharacterSetup>()) {
            var ch = st.GetComponent<Character>();
            if (!ch) continue;
            if (st.playerNum==1) { p1=ch; s1=st; }
            if (st.playerNum==2) { p2=ch; s2=st; }
        }
        if (!p1 || !p2) { enabled=false; Debug.LogWarning("Recorder: fighters not found."); return; }

        in1 = p1.GetInputProvider();
        in2 = p2.GetInputProvider();

        h1 = "Horizontal_P1"; v1 = "Vertical_P1";
        h2 = "Horizontal_P2"; v2 = "Vertical_P2";

        // init a round in the recorder
        var gm = FindObjectOfType<GameManager>();
        string stage = PlayerPrefs.GetString("SelectedStage");
        string p1Char = FindObjectOfType<CharacterManager>().GetCharacterName(1);
        string p2Char = FindObjectOfType<CharacterManager>().GetCharacterName(2);
        int rounds = 1, portals=0, maxH = (gm && gm.maxHealth!=-1) ? gm.maxHealth : 100;
        bool chanChan = false;
        if (PlayerPrefs.HasKey("SelectedRuleset")) {
            var r = JsonUtility.FromJson<CustomRuleset>(PlayerPrefs.GetString("SelectedRuleset"));
            rounds = r.rounds; portals = r.portals; chanChan = r.chanChan;
        }
        ReplayRecorder.Instance.BeginRound(
            stage, p1Char, p2Char, rounds, chanChan, maxH, portals,
            // pack key maps
            new[]{ s1.up,s1.down,s1.left,s1.right,s1.lightAttack,s1.heavyAttack,s1.block,s1.ability,s1.charge,s1.parry },
            new[]{ s2.up,s2.down,s2.left,s2.right,s2.lightAttack,s2.heavyAttack,s2.block,s2.ability,s2.charge,s2.parry }
        );
    }

    void Update()
    {
        if (!p1 || !p2) return;

        // sample axes
        float p1H = in1.GetAxis(h1), p1V = in1.GetAxis(v1);
        float p2H = in2.GetAxis(h2), p2V = in2.GetAxis(v2);

        // begin frame
        ReplayRecorder.Instance.StartFrameIfNeeded(p1H,p1V,p2H,p2V);

        // sample buttons
        ReplayData.ButtonSample b1 = SampleButtons(in1, s1);
        ReplayData.ButtonSample b2 = SampleButtons(in2, s2);

        ReplayRecorder.Instance.FillP1(b1);
        ReplayRecorder.Instance.FillP2(b2);
        ReplayRecorder.Instance.CommitFrameIfComplete();
        ReplayRecorder.Instance.AdvanceTick();
    }

    ReplayData.ButtonSample SampleButtons(IInputProvider inp, CharacterSetup s)
    {
        var b = new ReplayData.ButtonSample();
        // held
        b.up=inp.GetKey(s.up); b.down=inp.GetKey(s.down); b.left=inp.GetKey(s.left); b.right=inp.GetKey(s.right);
        b.light=inp.GetKey(s.lightAttack); b.heavy=inp.GetKey(s.heavyAttack); b.block=inp.GetKey(s.block);
        b.ability=inp.GetKey(s.ability); b.charge=inp.GetKey(s.charge); b.parry=inp.GetKey(s.parry);
        // edges
        b.upDown=inp.GetKeyDown(s.up); b.upUp=inp.GetKeyUp(s.up);
        b.downDown=inp.GetKeyDown(s.down); b.downUp=inp.GetKeyUp(s.down);
        b.leftDown=inp.GetKeyDown(s.left); b.leftUp=inp.GetKeyUp(s.left);
        b.rightDown=inp.GetKeyDown(s.right); b.rightUp=inp.GetKeyUp(s.right);
        b.lightDown=inp.GetKeyDown(s.lightAttack); b.lightUp=inp.GetKeyUp(s.lightAttack);
        b.heavyDown=inp.GetKeyDown(s.heavyAttack); b.heavyUp=inp.GetKeyUp(s.heavyAttack);
        b.blockDown=inp.GetKeyDown(s.block); b.blockUp=inp.GetKeyUp(s.block);
        b.abilityDown=inp.GetKeyDown(s.ability); b.abilityUp=inp.GetKeyUp(s.ability);
        b.chargeDown=inp.GetKeyDown(s.charge); b.chargeUp=inp.GetKeyUp(s.charge);
        b.parryDown=inp.GetKeyDown(s.parry); b.parryUp=inp.GetKeyUp(s.parry);
        return b;
    }
}
