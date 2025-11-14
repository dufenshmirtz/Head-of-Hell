// ReplayBrowser.cs
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ReplayBrowser : MonoBehaviour
{
    public Transform listParent;     // a vertical layout group
    public Button itemPrefab;

    void OnEnable() { Refresh(); }

    public void Refresh()
    {
        foreach (Transform c in listParent) Destroy(c.gameObject);

        string dir = Path.Combine(Application.persistentDataPath, "Replays");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        foreach (var file in Directory.GetFiles(dir, "*.json"))
        {
            var b = Instantiate(itemPrefab, listParent);
            b.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = Path.GetFileName(file);
            b.onClick.AddListener(()=> ReplayPlayer.Instance.LoadFromPath(file));
        }
    }
}
