using UnityEngine;

[CreateAssetMenu(fileName = "LithraSpecs", menuName = "Characters/Lithra Specs")]
public class LithraSpecs : ScriptableObject
{
    [Header("Spell")]
    public float cooldown = 10f;
    int bellDamage = 10;

    [Header("Light Attack")]
    public int airSpinDamage = 3;
    public float airSpinCD = 3f;
    public float airSpinSpeed = 5f; // Speed of the air spin
    public float jumpBackForce = 5f; // Force for the jump back
    public float lightAttackDuration = 0.5f; // Duration of the air spin
}
