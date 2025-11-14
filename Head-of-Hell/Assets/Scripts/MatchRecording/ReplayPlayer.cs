// ReplayPlayer.cs (revision)
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-500)] // run very early each frame
public class ReplayPlayer : MonoBehaviour
{
    public static ReplayPlayer Instance;

    ReplayData data;
    int tick;
    ReplayInputProvider p1Provider, p2Provider;

    [SerializeField] string gameplaySceneName = "GamePlayScene";

    void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void LoadFromPath(string path)
    {
        var json = File.ReadAllText(path);
        data = JsonUtility.FromJson<ReplayData>(json);
        SceneManager.LoadScene(gameplaySceneName);
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        if (data == null) return;
        StartCoroutine(BindFightersWhenReady()); // wait one frame if needed
    }

    IEnumerator BindFightersWhenReady()
    {
        Character p1=null, p2=null;
        // wait until fighters exist
        for (int i=0;i<120 && (p1==null || p2==null); i++) {
            var setups = FindObjectsOfType<CharacterSetup>();
            foreach (var st in setups) {
                var ch = st.GetComponent<Character>();
                if (!ch) continue;
                if (st.playerNum == 1) p1 = ch;
                if (st.playerNum == 2) p2 = ch;
            }
            if (p1!=null && p2!=null) break;
            yield return null; // next frame
        }

        if (p1==null || p2==null) {
            Debug.LogError("Replay: fighters not found in scene.");
            yield break;
        }

        p1Provider = new ReplayInputProvider(data, 1);
        p2Provider = new ReplayInputProvider(data, 2);
        p1.SetInput(p1Provider);
        p2.SetInput(p2Provider);

        tick = 0;
        Time.fixedDeltaTime = data.fixedDeltaTime;
    }

    void Update()
    {
        if (data == null || p1Provider == null || p2Provider == null) return;
        if (tick >= data.frames.Count) return;

        // Set the frame for this render/update step
        p1Provider.SetIndex(tick);
        p2Provider.SetIndex(tick);
        tick++;
    }
}
