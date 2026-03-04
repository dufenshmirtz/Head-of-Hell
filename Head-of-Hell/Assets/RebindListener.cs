using System;
using TMPro;
using UnityEngine;

public class RebindListener : MonoBehaviour
{
    public TextMeshProUGUI hintText; // optional

    private ControlsRebindRow current;

    public bool IsListening => current != null;

    public void BeginRebind(ControlsRebindRow row)
    {
        if (current != null) return;   // already rebinding
        current = row;
        if (hintText != null) hintText.text = "Press a key... (Esc to cancel)";
    }

    private void Update()
    {
        if (current == null) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            current = null;
            if (hintText != null) hintText.text = "";
            return;
        }

        if (TryGetAnyKeyDown(out var pressed))
        {
            current.SetKey(pressed);
            current = null;
            if (hintText != null) hintText.text = "";
        }
    }

    private bool TryGetAnyKeyDown(out KeyCode code)
    {
        foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
        {
            if (!Input.GetKeyDown(k)) continue;
            if (k == KeyCode.Escape) continue;

            code = k;
            return true;
        }

        code = KeyCode.None;
        return false;
    }
}