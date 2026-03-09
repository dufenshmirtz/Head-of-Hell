using UnityEngine;

[System.Serializable]
public class CharacterMLStats
{
    public int characterID;
    public float damage;
    public float cooldown;
    public float utility;
}

public class CharacterMLDatabase : MonoBehaviour
{
    public static CharacterMLDatabase Instance;

    public CharacterMLStats[] stats;

    private void Awake()
    {
        Instance = this;
    }

    public CharacterMLStats GetStatsByID(int id)
    {
        foreach (var s in stats)
        {
            if (s.characterID == id)
                return s;
        }

        Debug.LogError($"No ML stats found for character ID {id}");
        return null;
    }
}