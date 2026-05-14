public static class CustomCharacterSelection
{
    public const string ChoicePrefix = "Custom:";

    public static string EncodeChoice(string customCharacterId)
    {
        return ChoicePrefix + customCharacterId;
    }

    public static bool TryParseChoice(string choice, out string customCharacterId)
    {
        customCharacterId = null;
        if (string.IsNullOrWhiteSpace(choice) || !choice.StartsWith(ChoicePrefix))
        {
            return false;
        }

        customCharacterId = choice.Substring(ChoicePrefix.Length);
        return !string.IsNullOrWhiteSpace(customCharacterId);
    }

    public static string GetDisplayName(string choice)
    {
        if (!TryParseChoice(choice, out string id))
        {
            return choice;
        }

        CustomCharacterData data = CustomCharacterStore.GetById(id);
        return data != null ? data.displayName : "Custom Fighter";
    }
}
