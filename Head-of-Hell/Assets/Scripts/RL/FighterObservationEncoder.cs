using System.Collections.Generic;
using UnityEngine;

public static class FighterObservationEncoder
{
    public static float[] Encode(
        Character self,
        Character opp,
        float relXScale,
        float relYScale,
        float velScale,
        int totalCharacterCount)
    {
        List<float> obs = new List<float>(64);

        if (self == null || opp == null)
        {
            // same spirit as FighterAgent fallback
            obs.Add(0f);
            obs.Add(0f);
            obs.Add(0f);
            obs.Add(0f);
            obs.Add(0f);
            obs.Add(0f);
            return obs.ToArray();
        }

        var rb = self.GetComponent<Rigidbody2D>();
        var orb = opp.GetComponent<Rigidbody2D>();

        Vector2 vel = rb ? rb.velocity : Vector2.zero;
        Vector2 ovel = orb ? orb.velocity : Vector2.zero;

        Vector2 rel = (Vector2)(opp.transform.position - self.transform.position);

        // Relative position (normalized)
        float nx = Mathf.Clamp(rel.x / relXScale, -1f, 1f);
        float ny = Mathf.Clamp(rel.y / relYScale, -1f, 1f);
        obs.Add(nx);
        obs.Add(ny);

        // Velocities (normalized)
        obs.Add(Mathf.Clamp(vel.x / velScale, -1f, 1f));
        obs.Add(Mathf.Clamp(vel.y / velScale, -1f, 1f));
        obs.Add(Mathf.Clamp(ovel.x / velScale, -1f, 1f));
        obs.Add(Mathf.Clamp(ovel.y / velScale, -1f, 1f));

        // Health
        obs.Add(self.GetCurrentHealth() / 100f);
        obs.Add(opp.GetCurrentHealth() / 100f);

        // Character id one-hot
        AddOneHot(obs, self.characterID, totalCharacterCount);
        AddOneHot(obs, opp.characterID, totalCharacterCount);

        // Self state
        obs.Add(B(self.IsGrounded));
        obs.Add(B(self.IsBlocking));
        obs.Add(B(self.IsCasting));
        obs.Add(B(self.IsStunned));
        obs.Add(B(self.IsKnocked));
        obs.Add(B(self.IsCharging));
        obs.Add(B(self.IsCharged));
        obs.Add(B(self.OnAbilityCD));
        obs.Add(self.AbilityCooldown01);
        obs.Add(B(self.CanCast));
        obs.Add(B(self.CanParry));
        obs.Add(B(self.LightAttacking));
        obs.Add(B(self.HeavyAttacking));
        obs.Add(B(self.Parrying));

        // Self disabled flags
        obs.Add(B(self.QuickDisabled));
        obs.Add(B(self.HeavyDisabled));
        obs.Add(B(self.BlockDisabled));
        obs.Add(B(self.SpecialDisabled));
        obs.Add(B(self.ChargeDisabled));
        obs.Add(B(self.JumpDisabled));

        // Opponent state
        obs.Add(B(opp.IsGrounded));
        obs.Add(B(opp.IsBlocking));
        obs.Add(B(opp.IsCasting));
        obs.Add(B(opp.IsStunned));
        obs.Add(B(opp.IsKnocked));
        obs.Add(B(opp.IsCharging));
        obs.Add(B(opp.IsCharged));
        obs.Add(B(opp.OnAbilityCD));
        obs.Add(opp.AbilityCooldown01);
        obs.Add(B(opp.CanCast));
        obs.Add(B(opp.CanParry));
        obs.Add(B(opp.LightAttacking));
        obs.Add(B(opp.HeavyAttacking));
        obs.Add(B(opp.Parrying));

        // Facing hint
        obs.Add(Mathf.Sign(rel.x));
        obs.Add(Mathf.Sign(self.transform.localScale.x));

        return obs.ToArray();
    }

    public static int GetObservationSize(int totalCharacterCount)
    {
        return 2 + 4 + 2 + totalCharacterCount + totalCharacterCount + 14 + 6 + 14 + 2;
    }

    private static void AddOneHot(List<float> obs, int index, int count)
    {
        for (int i = 0; i < count; i++)
            obs.Add(i == index ? 1f : 0f);
    }

    private static float B(bool value) => value ? 1f : 0f;
}