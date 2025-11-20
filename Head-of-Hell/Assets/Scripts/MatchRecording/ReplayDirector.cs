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
        if (ReplayRecorder.Instance == null || !ReplayRecorder.Instance.Recording) return;

        // sample axes from the same providers Characters use
        float p1H = in1.GetAxis(h1), p1V = in1.GetAxis(v1);
        float p2H = in2.GetAxis(h2), p2V = in2.GetAxis(v2);

        // make sure there is a frame shell for this tick with axes
        ReplayRecorder.Instance.StartFrameIfNeeded(p1H, p1V, p2H, p2V);
    }
}
