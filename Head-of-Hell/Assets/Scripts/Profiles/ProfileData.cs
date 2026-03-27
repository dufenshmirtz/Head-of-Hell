using System;
using System.Collections.Generic;

[Serializable]
public class ProfileData
{
    public string id;          // unique (GUID)
    public string profileName; // εμφανιζόμενο όνομα
    public List<string> legacyIds = new List<string>();

    public ProfileData(string name)
    {
        id = Guid.NewGuid().ToString();
        profileName = name;
        legacyIds = new List<string>();
    }

    public ProfileData()
    {
        legacyIds = new List<string>();
    }

    public static ProfileData CreateGuest()
    {
        return new ProfileData
        {
            id = "GUEST",
            profileName = "Guest",
            legacyIds = new List<string>()
        };
    }
}