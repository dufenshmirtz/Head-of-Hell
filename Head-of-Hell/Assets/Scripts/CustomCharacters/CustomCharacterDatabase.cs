using System;
using System.Collections.Generic;

[Serializable]
public class CustomCharacterDatabase
{
    public const int MaxCustomCharacters = 20;

    public List<CustomCharacterData> characters = new List<CustomCharacterData>();

    public CustomCharacterData GetAt(int index)
    {
        if (index < 0 || index >= characters.Count)
        {
            return null;
        }

        return characters[index];
    }

    public bool HasAt(int index)
    {
        return GetAt(index) != null;
    }

    public void SetAt(int index, CustomCharacterData data)
    {
        if (index < 0 || index >= MaxCustomCharacters || data == null)
        {
            return;
        }

        data.Sanitize();
        while (characters.Count <= index)
        {
            characters.Add(null);
        }

        characters[index] = data;
    }

    public void DeleteAt(int index)
    {
        if (index < 0 || index >= characters.Count)
        {
            return;
        }

        characters[index] = null;
    }

    public CustomCharacterData GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        for (int i = 0; i < characters.Count; i++)
        {
            CustomCharacterData data = characters[i];
            if (data != null && data.id == id)
            {
                return data;
            }
        }

        return null;
    }
}
