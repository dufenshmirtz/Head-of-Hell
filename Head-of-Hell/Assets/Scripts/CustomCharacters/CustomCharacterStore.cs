using System.IO;
using UnityEngine;

public static class CustomCharacterStore
{
    private const string FileName = "custom_characters.json";

    private static CustomCharacterDatabase cache;

    private static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    public static CustomCharacterDatabase GetDatabase()
    {
        if (cache == null)
        {
            Load();
        }

        return cache;
    }

    public static void Load()
    {
        if (!File.Exists(SavePath))
        {
            cache = new CustomCharacterDatabase();
            return;
        }

        string json = File.ReadAllText(SavePath);
        cache = JsonUtility.FromJson<CustomCharacterDatabase>(json) ?? new CustomCharacterDatabase();
        Sanitize(cache);
    }

    public static void Save()
    {
        CustomCharacterDatabase database = GetDatabase();
        Sanitize(database);
        string json = JsonUtility.ToJson(database, true);
        File.WriteAllText(SavePath, json);
    }

    public static CustomCharacterData GetAt(int index)
    {
        return GetDatabase().GetAt(index);
    }

    public static CustomCharacterData GetById(string id)
    {
        return GetDatabase().GetById(id);
    }

    public static CustomCharacterData GetOrCreate(string id)
    {
        CustomCharacterData existing = GetById(id);
        if (existing != null)
        {
            return existing;
        }

        CustomCharacterData fallback = CustomCharacterData.CreateDefault();
        fallback.id = id;
        return fallback;
    }

    public static void SetAt(int index, CustomCharacterData data)
    {
        GetDatabase().SetAt(index, data);
        Save();
    }

    public static void DeleteAt(int index)
    {
        GetDatabase().DeleteAt(index);
        Save();
    }

    private static void Sanitize(CustomCharacterDatabase database)
    {
        if (database == null || database.characters == null)
        {
            return;
        }

        for (int i = 0; i < database.characters.Count; i++)
        {
            if (database.characters[i] != null)
            {
                database.characters[i].Sanitize();
            }
        }
    }
}
