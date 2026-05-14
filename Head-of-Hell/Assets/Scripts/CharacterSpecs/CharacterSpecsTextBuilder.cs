using System.Text;

public static class CharacterSpecsTextBuilder
{
    public static string BuildInfo(string characterName, CharacterSpecsBase specs, Abilities fallback)
    {
        if (specs == null)
        {
            return BuildFallback(fallback);
        }

        StringBuilder builder = new StringBuilder();
        AppendPassive(builder, specs, fallback);
        AppendQuick(builder, specs, fallback);
        AppendSpell(builder, specs, fallback);
        return builder.ToString();
    }

    private static void AppendPassive(StringBuilder builder, CharacterSpecsBase specs, Abilities fallback)
    {
        string text = fallback != null ? fallback.passive : "";

        if (specs is FinSpecs fin)
        {
            text = $"Counter damage deals {fin.passiveDamage} extra damage.";
        }
        else if (specs is VanderSpecs)
        {
            text = "Vander heals from several attacks.";
        }
        else if (specs is ChibackSpecs chiback)
        {
            text = $"After Chiback is hit {chiback.enragingNum} times, the next damaging ability is empowered.";
        }
        else if (specs is SteelagerSpecs steelager)
        {
            text = $"Steelager's bomb can launch him, and heavy attacks can add {steelager.comboDamage} combo damage.";
        }
        else if (specs is LazyBigusSpecs bigus)
        {
            text = $"After enough poison stacks, Bigus applies poison for {bigus.passiveDamage} total damage.";
        }
        else if (specs is SkiplerSpecs)
        {
            text = "Every successful quick attack reduces special cooldown by 2 seconds.";
        }
        else if (specs is LithraSpecs)
        {
            text = "Lithra can stun with bell-based attacks.";
        }
        else if (specs is LupenSpecs lupen)
        {
            text = $"Lupen's transformation shockwave checks enemies within {lupen.passiveRange} units.";
        }
        else if (specs is VisviaSpecs visvia)
        {
            text = $"Visvia tracks blast use and can overheat for {Seconds(visvia.overheatDuration)}, taking {visvia.overheatDamage} damage ticks.";
        }

        AppendSection(builder, "PASSIVE", text);
    }

    private static void AppendQuick(StringBuilder builder, CharacterSpecsBase specs, Abilities fallback)
    {
        string text = fallback != null ? fallback.quick_attack : "";

        if (specs is FinSpecs fin)
        {
            text = $"Fin rolls with {fin.rollPower:0.#} force for {Seconds(fin.rollTime)}. Reset: {Seconds(fin.resetRoll)}.";
        }
        else if (specs is VanderSpecs vander)
        {
            text = $"Vander hits twice for {vander.katanaDmg} damage each; the second hit heals {vander.smallLifesteal}. Reset: {Seconds(vander.katanaCD)}.";
        }
        else if (specs is ChibackSpecs chiback)
        {
            text = $"Chiback creates fire bursts for {chiback.disableDamage} damage and briefly disables block and jump. Reset: {Seconds(chiback.resetDisable)}.";
        }
        else if (specs is SteelagerSpecs steelager)
        {
            text = $"Steelager throws bombs for {steelager.bombDamage} damage. Reset: {Seconds(steelager.resetBomb)}.";
        }
        else if (specs is LazyBigusSpecs bigus)
        {
            text = $"Bigus fires a projectile for {bigus.bulletDamage} damage. Reset: {Seconds(bigus.resetBullet)}.";
        }
        else if (specs is SkiplerSpecs skipler)
        {
            text = $"Skipler blinks with {skipler.blinkPower:0.#} force for {Seconds(skipler.blinkTime)}, dealing {skipler.blinkDamage} damage. Reset: {Seconds(skipler.blinkCD)}.";
        }
        else if (specs is RagerSpecs rager)
        {
            text = $"Rager punches for {rager.lightDamage} damage. Reset: {Seconds(rager.lightRatio)}.";
        }
        else if (specs is LithraSpecs lithra)
        {
            text = $"Lithra leaps forward for {Seconds(lithra.lightAttackDuration)}, dealing {lithra.airSpinDamage} damage and stunning. Reset: {Seconds(lithra.airSpinCD)}.";
        }
        else if (specs is LupenSpecs lupen)
        {
            text = $"Lupen whips for {lupen.wipDamage} damage and steals speed. Reset: {Seconds(lupen.resetWip)}.";
        }
        else if (specs is VisviaSpecs visvia)
        {
            text = $"Visvia blasts for {visvia.shotgunDamage} damage with {visvia.shotgunForce:0.#} recoil force. Reset: {Seconds(visvia.shotgunCooldown)}.";
        }

        AppendSection(builder, "QUICK ATTACK", text);
    }

    private static void AppendSpell(StringBuilder builder, CharacterSpecsBase specs, Abilities fallback)
    {
        string text = fallback != null ? fallback.spell : "";

        if (specs is FinSpecs fin)
        {
            text = $"Fin flashes a nearby enemy, stunning for {Seconds(fin.flashStunDuration)}. Cooldown: {Seconds(fin.cooldown)}.";
        }
        else if (specs is VanderSpecs vander)
        {
            text = $"Vander stabs for {vander.stabDamage} damage and heals {vander.stabHeal}. Cooldown: {Seconds(vander.cooldown)}.";
        }
        else if (specs is ChibackSpecs chiback)
        {
            text = $"Chiback leaps for up to {Seconds(chiback.jumpDuration)}, dealing {chiback.shortJumpDamage}, {chiback.MedJumpDamage}, or {chiback.wideJumpDamage} damage. Cooldown: {Seconds(chiback.cooldown)}.";
        }
        else if (specs is SteelagerSpecs steelager)
        {
            text = $"Steelager explodes for {steelager.damage} damage. Cooldown: {Seconds(steelager.cooldown)}.";
        }
        else if (specs is LazyBigusSpecs bigus)
        {
            text = $"Bigus fires a beam for {bigus.beamDamage} damage plus {bigus.beamPoisonDamage} poison damage. Cooldown: {Seconds(bigus.cooldown)}.";
        }
        else if (specs is SkiplerSpecs skipler)
        {
            text = $"Skipler dashes for {Seconds(skipler.dashingTime)}, dealing {skipler.damage} damage. Cooldown: {Seconds(skipler.cooldown)}.";
        }
        else if (specs is RagerSpecs rager)
        {
            text = $"Rager combos for {rager.spellDamage1 + rager.spellDamage2} total damage. Cooldown: {Seconds(rager.cooldown)}.";
        }
        else if (specs is LithraSpecs lithra)
        {
            text = $"Lithra's bell attack deals {lithra.damage} damage and can stun. Cooldown: {Seconds(lithra.cooldown)}.";
        }
        else if (specs is LupenSpecs lupen)
        {
            text = $"Lupen transforms after a shockwave check. Listed impact value: {lupen.damage}. Cooldown: {Seconds(lupen.cooldown)}.";
        }
        else if (specs is VisviaSpecs visvia)
        {
            text = $"Visvia grabs twice for {visvia.grabDamage} damage per hit. Cooldown: {Seconds(visvia.cooldown)}.";
        }

        AppendSection(builder, "SPELL", text);
    }

    private static void AppendSection(StringBuilder builder, string title, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        builder.Append('~').Append(title).Append("~\n");
        builder.Append(text).Append('\n');
    }

    private static string BuildFallback(Abilities fallback)
    {
        StringBuilder builder = new StringBuilder();
        if (fallback == null)
        {
            return "";
        }

        AppendSection(builder, "PASSIVE", fallback.passive);
        AppendSection(builder, "QUICK ATTACK", fallback.quick_attack);
        AppendSection(builder, "SPELL", fallback.spell);
        return builder.ToString();
    }

    private static string Seconds(float seconds)
    {
        return $"{seconds:0.##}s";
    }
}
