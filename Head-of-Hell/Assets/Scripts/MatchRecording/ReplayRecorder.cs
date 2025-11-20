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

    bool haveP1ThisFrame, haveP2ThisFrame;

    ReplayData.Frame pendingFrame;

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

        haveP1ThisFrame = haveP2ThisFrame = false;
        pendingFrame = null;
    }

    public void RegisterP1() => p1Registered = true;
    public void RegisterP2() => p2Registered = true;

    // Called once per Update by ReplayTap on one GameObject (see below)
    public void AdvanceTick() {}

    public void StartFrameIfNeeded(float p1H, float p1V, float p2H, float p2V)
    {
        if (pendingFrame == null)
            pendingFrame = new ReplayData.Frame { tick = Tick, p1H = p1H, p1V = p1V, p2H = p2H, p2V = p2V, p1 = new(), p2 = new() };
    }

    public void FillP1(ReplayData.ButtonSample b) {
        if (!Recording || pendingFrame == null) return;
        pendingFrame.p1 = b;
        haveP1ThisFrame = true;
    }
    public void FillP2(ReplayData.ButtonSample b) {
        if (!Recording || pendingFrame == null) return;
        pendingFrame.p2 = b;
        haveP2ThisFrame = true; 
    }

    public void CommitFrameIfComplete()
    {
        if (!Recording || pendingFrame == null) return;

        // What players do we expect this round?
        bool needP1 = p1Registered;
        bool needP2 = p2Registered;

        // Frame is ready when every existing player has written this frame.
        bool ready =
            (!needP1 || haveP1ThisFrame) &&
            (!needP2 || haveP2ThisFrame);

        if (ready)
        {
            data.frames.Add(pendingFrame);

            pendingFrame = null;
            haveP1ThisFrame = haveP2ThisFrame = false;

            Tick++;   // <-- tick now advances exactly once per committed frame
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
