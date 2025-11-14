using UnityEngine;

public static class DebugPlayerPrefs
{
    public static void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        Debug.Log($"[PlayerPrefs] SetFloat called → key='{key}', value={value}\n{StackTrace()}");
    }

    public static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        Debug.Log($"[PlayerPrefs] SetInt called → key='{key}', value={value}\n{StackTrace()}");
    }

    public static void SetString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
        Debug.Log($"[PlayerPrefs] SetString called → key='{key}', value={value}\n{StackTrace()}");
    }

    public static float GetFloat(string key, float defaultValue = 0f)
    {
        float result = PlayerPrefs.GetFloat(key, defaultValue);
        Debug.Log($"[PlayerPrefs] GetFloat called → key='{key}', returned={result}\n{StackTrace()}");
        return result;
    }

    private static string StackTrace()
    {
        return new System.Diagnostics.StackTrace(2, true).ToString(); // skip the first 2 frames
    }
}
