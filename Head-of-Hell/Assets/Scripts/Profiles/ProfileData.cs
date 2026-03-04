using System;

[Serializable]
public class ProfileData
{
    public string id;          // unique (GUID)
    public string profileName; // εμφανιζόμενο όνομα

    // TODO later: controls, stats, etc.

    public ProfileData(string name)
    {
        id = Guid.NewGuid().ToString();
        profileName = name;
    }
}