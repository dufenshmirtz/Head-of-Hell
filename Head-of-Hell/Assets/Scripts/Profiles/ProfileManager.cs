using System.IO;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager I { get; private set; }

    [Header("Runtime")]
    [SerializeField] private ProfilesDatabase db = new ProfilesDatabase();
    [SerializeField] private int selectedIndexP1 = -1;
    [SerializeField] private int selectedIndexP2 = -1;

    private string SavePath => Path.Combine(Application.persistentDataPath, "profiles.json");

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }

    // --- Public API (θα το καλέσει το UI στο επόμενο βήμα) ---

    public ProfilesDatabase GetDatabase() => db;

    public int GetSelectedIndex(int playerNum) => (playerNum == 1) ? selectedIndexP1 : selectedIndexP2;


    public ProfileData GetSelectedProfile(int playerNum)
    {
        int idx = GetSelectedIndex(playerNum);
        return db.GetAt(idx);
    }

    public void SelectProfile(int playerNum, int index)
    {
        if (!db.HasAt(index)) return;

        if (playerNum == 1) selectedIndexP1 = index;
        else selectedIndexP2 = index;

        Save();
    }

    public ProfileData CreateOrRenameProfileAt(int index, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) return null;

        var existing = db.GetAt(index);
        if (existing == null)
        {
            var p = new ProfileData(newName.Trim());
            db.SetAt(index, p);
        }
        else
        {
            existing.profileName = newName.Trim();
            db.SetAt(index, existing);
        }

        Save();
        return db.GetAt(index);
    }

    public void DeleteProfileAt(int index)
    {
        db.DeleteAt(index);

        if (selectedIndexP1 == index) selectedIndexP1 = -1;
        if (selectedIndexP2 == index) selectedIndexP2 = -1;
        Save();
    }

    // --- Persistence ---

    public void Save()
    {
        var json = JsonUtility.ToJson(db, prettyPrint: true);
        File.WriteAllText(SavePath, json);
    }

    public void Load()
    {
        if (!File.Exists(SavePath))
        {
            db = new ProfilesDatabase();
            return;
        }

        var json = File.ReadAllText(SavePath);
        db = JsonUtility.FromJson<ProfilesDatabase>(json) ?? new ProfilesDatabase();
    }
}