using UnityEngine;

[CreateAssetMenu(fileName = "VanderSpecs", menuName = "Characters/Vander Specs")]
public class VanderSpecs : ScriptableObject
{
    [Header("Spell")]
    public int stabDamage = 10, stabHeal = 5;
    public float cooldown = 12f;

    [Header("Light Attack")]
    
    public int katanaDmg = 3;
    public int smallLifesteal = 3;
    public float katanaCD=2f;

    [Header("Charge")]
    public int chargeLifesteal=8;

    [Header("Flying")]
    public float fallBrakePerSec = 20f;

    // How quickly we ramp upward (units: velocity per second)
    public float ascendAccelPerSec = 18f;

    // Hard cap for upward speed
    public float maxAscendSpeed = 4f;
}