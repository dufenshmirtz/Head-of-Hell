using UnityEngine;

public class RulesetManager : MonoBehaviour
{
    public static RulesetManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Makes sure this object persists across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveCustomRuleset(int slot, CustomRuleset ruleset)
    {
        string json = JsonUtility.ToJson(ruleset);
        PlayerPrefs.SetString("CustomRuleset_" + slot, json);
        PlayerPrefs.Save();
    }

    public CustomRuleset LoadCustomRuleset(int slot)
    {
        string json = PlayerPrefs.GetString("CustomRuleset_" + slot, null);
        if (!string.IsNullOrEmpty(json))
        {
            return JsonUtility.FromJson<CustomRuleset>(json);
        }
        return null;
    }
}
