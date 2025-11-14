// ReplayUIHooks.cs
using UnityEngine;

public class ReplayUIHooks : MonoBehaviour
{
    public void OnSaveReplayClicked()
    {
        var path = ReplayRecorder.Instance?.SaveReplay();
        Debug.Log(path == null ? "No replay to save." : $"Replay saved: {path}");
    }

    public void OnDiscardReplay()
    {
        ReplayRecorder.Instance?.Discard();
    }
}
