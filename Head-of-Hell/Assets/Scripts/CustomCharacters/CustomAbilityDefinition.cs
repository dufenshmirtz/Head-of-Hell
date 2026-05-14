using System;

public enum CustomAbilitySlot
{
    Light,
    Special,
    Passive
}

[Serializable]
public class CustomAbilityDefinition
{
    public string id;
    public string displayName;
    public CustomAbilitySlot slot;
    public string sourceCharacter;
    public int damage;
    public float cooldown;
    public string description;

    public CustomAbilityDefinition(
        string id,
        string displayName,
        CustomAbilitySlot slot,
        string sourceCharacter,
        int damage,
        float cooldown,
        string description)
    {
        this.id = id;
        this.displayName = displayName;
        this.slot = slot;
        this.sourceCharacter = sourceCharacter;
        this.damage = damage;
        this.cooldown = cooldown;
        this.description = description;
    }
}
