using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlsRebindRow : MonoBehaviour
{
    [Header("Unique id for saving (example: P1_Jump)")]
    public string actionId;

    [Header("Drag the TMP text inside this KeyButton")]
    public TextMeshProUGUI keyText;

    [Header("Drag RebindSystem here")]
    public RebindListener listener;

    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(OnClick);

        RefreshFromSaved();
    }

    private void OnClick()
    {
        if (listener == null || keyText == null) return;
        if (listener.IsListening) return;

        listener.BeginRebind(this);
    }

    public void SetKey(KeyCode key)
    {
        PlayerPrefs.SetInt("BIND_" + actionId, (int)key);
        PlayerPrefs.Save();
        keyText.text = PrettyKey(key);
    }

    public void RefreshFromSaved()
    {
        string k = "BIND_" + actionId;
        if (PlayerPrefs.HasKey(k))
        {
            var saved = (KeyCode)PlayerPrefs.GetInt(k);
            keyText.text = PrettyKey(saved);
        }
        // else leave whatever text is already set in the scene (your defaults)
    }

    private string PrettyKey(KeyCode k)
    {
        return k switch
        {
            KeyCode.Mouse0 => "LMB",
            KeyCode.Mouse1 => "RMB",
            KeyCode.Mouse2 => "MMB",
            KeyCode.Minus => "-",
            _ => k.ToString()
        };
    }
}