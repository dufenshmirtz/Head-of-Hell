// ReplayRecorder.cs
using System.IO;
using UnityEngine;

public class ReplayRecorder : MonoBehaviour
{
    public static ReplayRecorder Instance { get; private set; }

    public bool Recording => data != null;
    public int Tick { get; private set; }

    ReplayData data;
    string replaysDir;
    bool p1Registered, p2Registered;

    void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        replaysDir = Path.Combine(Application.persistentDataPath, "Replays");
        if (!Directory.Exists(replaysDir)) Directory.CreateDirectory(replaysDir);
    }

    public void BeginRound(
        string sceneName, string p1Char, string p2Char,
        int roundCount, bool chanChan, int maxHealth, int portalNumber,
        KeyCode[] p1Keys, KeyCode[] p2Keys)
    {
        Tick = 0;
        data = new ReplayData {
            gameVersion    = Application.version,
            sceneName      = sceneName,
            p1CharacterId  = p1Char,
            p2CharacterId  = p2Char,
            roundCount     = roundCount,
            chanChan       = chanChan,
            maxHealth      = maxHealth,
            portalNumber   = portalNumber,
            fixedDeltaTime = Time.fixedDeltaTime,
            startedAtUnix  = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            p1Keys = new ReplayData.KeyMap {
                up=(int)p1Keys[0], down=(int)p1Keys[1], left=(int)p1Keys[2], right=(int)p1Keys[3],
                light=(int)p1Keys[4], heavy=(int)p1Keys[5], block=(int)p1Keys[6],
                ability=(int)p1Keys[7], charge=(int)p1Keys[8], parry=(int)p1Keys[9]
            },
            p2Keys = new ReplayData.KeyMap {
                up=(int)p2Keys[0], down=(int)p2Keys[1], left=(int)p2Keys[2], right=(int)p2Keys[3],
                light=(int)p2Keys[4], heavy=(int)p2Keys[5], block=(int)p2Keys[6],
                ability=(int)p2Keys[7], charge=(int)p2Keys[8], parry=(int)p2Keys[9]
            }
        };
        p1Registered = p2Registered = false;
    }

    public void RegisterP1() => p1Registered = true;
    public void RegisterP2() => p2Registered = true;

    // Called once per Update by ReplayTap on one GameObject (see below)
    public void AdvanceTick() { if (Recording) Tick++; }

    // Called by each character’s ReplayTap every Update with that player’s sample.
    ReplayData.Frame pendingFrame;

    public void StartFrameIfNeeded(float p1H, float p1V, float p2H, float p2V)
    {
        if (pendingFrame == null)
            pendingFrame = new ReplayData.Frame { tick = Tick, p1H = p1H, p1V = p1V, p2H = p2H, p2V = p2V, p1 = new(), p2 = new() };
    }

    public void FillP1(ReplayData.ButtonSample b) { if (Recording) pendingFrame.p1 = b; }
    public void FillP2(ReplayData.ButtonSample b) { if (Recording) pendingFrame.p2 = b; }

    public void CommitFrameIfComplete()
    {
        if (!Recording || pendingFrame == null) return;
        // store when both sides have registered or if it’s a 1P training
        if ((p1Registered && p2Registered) || (p1Registered ^ p2Registered)) {
            data.frames.Add(pendingFrame);
            pendingFrame = null;
        }
    }

    public string SaveReplay()
    {
        if (!Recording) return null;
        string name = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + $"_{data.sceneName}.json";
        string path = Path.Combine(replaysDir, name);
        File.WriteAllText(path, JsonUtility.ToJson(data));
        data = null; // clear buffer after saving
        return path;
    }

    public void Discard() { data = null; pendingFrame = null; }
}
