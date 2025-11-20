using UnityEngine;

[CreateAssetMenu(fileName = "VanderSpecs", menuName = "Characters/Vander Specs")]
public class VanderSpecs : ScriptableObject
{
    [Header("Spell")]
    int stabDamage = 10, stabHeal = 5;
    float cooldown = 12f;

    [Header("Light Attack")]
    
    int katanaDmg = 3;
    int smallLifesteal = 3;
    float katanaCD=2f;

    [Header("Charge")]
    int chargeLifesteal=8;
}