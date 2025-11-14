// ReplayInputProvider.cs
using UnityEngine;

public class ReplayInputProvider : IInputProvider
{
    readonly ReplayData data;
    readonly int side; // 1 for P1, 2 for P2
    int index;         // current frame index
    ReplayData.Frame cur;

    public ReplayInputProvider(ReplayData d, int playerSide) { data = d; side = playerSide; cur = data.frames.Count>0 ? data.frames[0] : null; }

    public void SetIndex(int i) { index = Mathf.Clamp(i, 0, data.frames.Count - 1); cur = data.frames[index]; }

    public float GetAxis(string name)
    {
        if (cur == null) return 0f;
        if (side==1) { if (name.Contains("Horizontal")) return cur.p1H; if (name.Contains("Vertical")) return cur.p1V; }
        else         { if (name.Contains("Horizontal")) return cur.p2H; if (name.Contains("Vertical")) return cur.p2V; }
        return 0f;
    }

    // We answer Key/Down/Up by mapping the recorded logical state, independent of actual keycode:
    bool GetAction(KeyCode key, System.Func<ReplayData.ButtonSample, bool> f1, System.Func<ReplayData.ButtonSample, bool> f2)
    {
        var b = side==1 ? cur.p1 : cur.p2;
        // map the requested KeyCode to the correct logical flag using stored KeyMaps
        var km = side==1 ? data.p1Keys : data.p2Keys;
        if      ((int)key == km.up)    return f1(b);
        else if ((int)key == km.down)  return f2 == null ? f1(b) : f2(b); // the caller provides right selector
        // (we’ll handle below with switch helpers for clarity)
        return false;
    }

    public bool GetKeyDown(KeyCode key) => KeyQuery(key, type:0);
    public bool GetKeyUp(KeyCode key)    => KeyQuery(key, type:1);
    public bool GetKey(KeyCode key)      => KeyQuery(key, type:2);

    bool KeyQuery(KeyCode key, int type)
    {
        var b = side==1 ? cur.p1 : cur.p2;
        var km = side==1 ? data.p1Keys : data.p2Keys;

        if ((int)key == km.up)     return type==0? b.upDown : type==1? b.upUp : b.up;
        if ((int)key == km.down)   return type==0? b.downDown : type==1? b.downUp : b.down;
        if ((int)key == km.left)   return type==0? b.leftDown : type==1? b.leftUp : b.left;
        if ((int)key == km.right)  return type==0? b.rightDown : type==1? b.rightUp : b.right;
        if ((int)key == km.light)  return type==0? b.lightDown : type==1? b.lightUp : b.light;
        if ((int)key == km.heavy)  return type==0? b.heavyDown : type==1? b.heavyUp : b.heavy;
        if ((int)key == km.block)  return type==0? b.blockDown : type==1? b.blockUp : b.block;
        if ((int)key == km.ability)return type==0? b.abilityDown : type==1? b.abilityUp : b.ability;
        if ((int)key == km.charge) return type==0? b.chargeDown : type==1? b.chargeUp : b.charge;
        if ((int)key == km.parry)  return type==0? b.parryDown : type==1? b.parryUp : b.parry;
        return false;
    }

    // Not used in your Character but required by the interface
    public bool GetButtonDown(string name) => false;
    public bool GetButtonUp(string name) => false;
}
