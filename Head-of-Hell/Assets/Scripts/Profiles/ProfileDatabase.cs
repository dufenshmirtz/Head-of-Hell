using System;
using System.Collections.Generic;

[Serializable]
public class ProfilesDatabase
{
    public List<ProfileData> profiles = new List<ProfileData>();

    // 0..4 slots (5 profiles max όπως έχεις UI)
    public const int MaxProfiles = 5;

    public ProfileData GetAt(int index)
    {
        if (index < 0 || index >= profiles.Count) return null;
        return profiles[index];
    }

    public bool HasAt(int index) => GetAt(index) != null;

    public void SetAt(int index, ProfileData data)
    {
        if (index < 0 || index >= MaxProfiles) return;

        // φρόντισε η λίστα να έχει μέγεθος index+1
        while (profiles.Count <= index) profiles.Add(null);

        profiles[index] = data;
    }

    public void DeleteAt(int index)
    {
        if (index < 0 || index >= profiles.Count) return;
        profiles[index] = null;
    }
}