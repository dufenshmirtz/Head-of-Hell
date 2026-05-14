using System;

[Serializable]
public class CustomCharacterData
{
    public string id;
    public string displayName;
    public string lightAbilityId;
    public string specialAbilityId;
    public string passiveId;

    public static CustomCharacterData CreateDefault(string displayName = "Custom Fighter")
    {
        return new CustomCharacterData
        {
            id = Guid.NewGuid().ToString("N"),
            displayName = displayName,
            lightAbilityId = CustomCharacterAbilityIds.LightSkiplerBlink,
            specialAbilityId = CustomCharacterAbilityIds.SpecialSteelagerExplosion,
            passiveId = CustomCharacterAbilityIds.PassiveNone
        };
    }

    public void Sanitize()
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            id = Guid.NewGuid().ToString("N");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = "Custom Fighter";
        }

        if (!CustomCharacterAbilityCatalog.HasAbility(lightAbilityId, CustomAbilitySlot.Light))
        {
            lightAbilityId = CustomCharacterAbilityIds.LightBasicQuick;
        }

        if (!CustomCharacterAbilityCatalog.HasAbility(specialAbilityId, CustomAbilitySlot.Special))
        {
            specialAbilityId = CustomCharacterAbilityIds.SpecialBasicBurst;
        }

        if (!CustomCharacterAbilityCatalog.HasAbility(passiveId, CustomAbilitySlot.Passive))
        {
            passiveId = CustomCharacterAbilityIds.PassiveNone;
        }
    }
}
