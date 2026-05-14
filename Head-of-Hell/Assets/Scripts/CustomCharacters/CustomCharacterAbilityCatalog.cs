using System.Collections.Generic;

public static class CustomCharacterAbilityCatalog
{
    private static readonly List<CustomAbilityDefinition> lightAbilities = new List<CustomAbilityDefinition>
    {
        new CustomAbilityDefinition(CustomCharacterAbilityIds.LightBasicQuick, "Basic Quick", CustomAbilitySlot.Light, "Custom", 4, 1.5f, "A simple close-range strike."),
        new CustomAbilityDefinition(CustomCharacterAbilityIds.LightSteelagerBomb, "Steelager Bomb", CustomAbilitySlot.Light, "Steelager", 6, 2f, "Throw a bomb that can be detonated early."),
        new CustomAbilityDefinition(CustomCharacterAbilityIds.LightVanderKatana, "Vander Katana", CustomAbilitySlot.Light, "Vander", 6, 2f, "Two fast katana hits with minor lifesteal."),
        new CustomAbilityDefinition(CustomCharacterAbilityIds.LightRagerPunch, "Rager Punch", CustomAbilitySlot.Light, "Rager", 4, 0.1f, "A very fast close-range punch."),
        new CustomAbilityDefinition(CustomCharacterAbilityIds.LightSkiplerBlink, "Skipler Blink", CustomAbilitySlot.Light, "Skipler", 5, 3f, "A short directional blink that can hit in front."),
        new CustomAbilityDefinition(CustomCharacterAbilityIds.LightFinRoll, "Fin Roll", CustomAbilitySlot.Light, "Fin", 0, 2f, "A brief invulnerable roll."),
        new CustomAbilityDefinition(CustomCharacterAbilityIds.LightLazyBigusShot, "Bigus Shot", CustomAbilitySlot.Light, "Lazy Bigus", 3, 2f, "Fire a projectile forward."),
        new CustomAbilityDefinition(CustomCharacterAbilityIds.LightLithraAirSpin, "Lithra Air Spin", CustomAbilitySlot.Light, "Lithra", 3, 3f, "Leap forward, damage, stun, then bounce back."),
        new CustomAbilityDefinition(CustomCharacterAbilityIds.LightChibackFire, "Chiback Fire", CustomAbilitySlot.Light, "Chiback", 5, 1f, "Fire bursts on both sides that disable block and jump briefly."),
        new CustomAbilityDefinition(CustomCharacterAbilityIds.LightLupenWhip, "Lupen Whip", CustomAbilitySlot.Light, "Lupen", 1, 2f, "Whip forward, slow the enemy, and speed yourself up."),
        new CustomAbilityDefinition(CustomCharacterAbilityIds.LightVisviaShotgun, "Visvia Shotgun", CustomAbilitySlot.Light, "Visvia", 6, 4f, "Blast forward and recoil backward.")
    };

    private static readonly List<CustomAbilityDefinition> specialAbilities = new List<CustomAbilityDefinition>
    {
        new CustomAbilityDefinition(CustomCharacterAbilityIds.SpecialBasicBurst, "Basic Burst", CustomAbilitySlot.Special, "Custom", 12, 10f, "A reliable close-range burst."),
        new CustomAbilityDefinition(CustomCharacterAbilityIds.SpecialSteelagerExplosion, "Steelager Explosion", CustomAbilitySlot.Special, "Steelager", 16, 16f, "Explode in a large area around the body."),
        new CustomAbilityDefinition(CustomCharacterAbilityIds.SpecialVanderStab, "Vander Stab", CustomAbilitySlot.Special, "Vander", 10, 12f, "A forward stab that also heals."),
        new CustomAbilityDefinition(CustomCharacterAbilityIds.SpecialRagerCombo, "Rager Combo", CustomAbilitySlot.Special, "Rager", 25, 20f, "Grab and damage a nearby target over a short combo."),
        new CustomAbilityDefinition(CustomCharacterAbilityIds.SpecialSkiplerDash, "Skipler Dash", CustomAbilitySlot.Special, "Skipler", 10, 8f, "Dash through enemies and damage each target once."),
        new CustomAbilityDefinition(CustomCharacterAbilityIds.SpecialFinFlash, "Fin Flash", CustomAbilitySlot.Special, "Fin", 1, 8f, "Flash a nearby enemy and stun them."),
        new CustomAbilityDefinition(CustomCharacterAbilityIds.SpecialLazyBigusBeam, "Bigus Beam", CustomAbilitySlot.Special, "Lazy Bigus", 20, 20f, "Fire a beam that damages and poisons enemies."),
        new CustomAbilityDefinition(CustomCharacterAbilityIds.SpecialLithraBell, "Lithra Bell", CustomAbilitySlot.Special, "Lithra", 10, 10f, "Strike with a large bell area and stun the sweet spot."),
        new CustomAbilityDefinition(CustomCharacterAbilityIds.SpecialChibackScytheJump, "Chiback Scythe Jump", CustomAbilitySlot.Special, "Chiback", 10, 10f, "Leap forward and deal more damage the later it lands."),
        new CustomAbilityDefinition(CustomCharacterAbilityIds.SpecialLupenShockwave, "Lupen Shockwave", CustomAbilitySlot.Special, "Lupen", 0, 15f, "Knock nearby enemies away."),
        new CustomAbilityDefinition(CustomCharacterAbilityIds.SpecialVisviaGrab, "Visvia Grab", CustomAbilitySlot.Special, "Visvia", 12, 10f, "Long horizontal grab that damages and knocks back.")
    };

    private static readonly List<CustomAbilityDefinition> passiveAbilities = new List<CustomAbilityDefinition>
    {
        new CustomAbilityDefinition(CustomCharacterAbilityIds.PassiveNone, "No Passive", CustomAbilitySlot.Passive, "Custom", 0, 0f, "No passive effect.")
    };

    public static IReadOnlyList<CustomAbilityDefinition> LightAbilities => lightAbilities;
    public static IReadOnlyList<CustomAbilityDefinition> SpecialAbilities => specialAbilities;
    public static IReadOnlyList<CustomAbilityDefinition> PassiveAbilities => passiveAbilities;

    public static IReadOnlyList<CustomAbilityDefinition> GetAbilities(CustomAbilitySlot slot)
    {
        switch (slot)
        {
            case CustomAbilitySlot.Light:
                return lightAbilities;
            case CustomAbilitySlot.Special:
                return specialAbilities;
            case CustomAbilitySlot.Passive:
                return passiveAbilities;
            default:
                return lightAbilities;
        }
    }

    public static bool HasAbility(string id, CustomAbilitySlot slot)
    {
        return FindAbility(id, slot) != null;
    }

    public static CustomAbilityDefinition GetAbility(string id, CustomAbilitySlot slot)
    {
        CustomAbilityDefinition ability = FindAbility(id, slot);
        if (ability != null)
        {
            return ability;
        }

        IReadOnlyList<CustomAbilityDefinition> abilities = GetAbilities(slot);
        return abilities.Count > 0 ? abilities[0] : null;
    }

    private static CustomAbilityDefinition FindAbility(string id, CustomAbilitySlot slot)
    {
        IReadOnlyList<CustomAbilityDefinition> abilities = GetAbilities(slot);
        for (int i = 0; i < abilities.Count; i++)
        {
            if (abilities[i].id == id)
            {
                return abilities[i];
            }
        }

        return null;
    }
}
