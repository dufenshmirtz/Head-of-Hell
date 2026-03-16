using UnityEngine;

[CreateAssetMenu(fileName = "LithraSpecs", menuName = "Characters/Lithra Specs")]
public class LithraSpecs : CharacterSpecsBase
{
    //[Header("Spell")]
    public override float cooldown => 10f;
    public override int damage => 10;

    [Header("Light Attack")]
    public int airSpinDamage = 3;
    public float airSpinCD = 3f;
    public float airSpinSpeed = 5f; // Speed of the air spin
    public float jumpBackForce = 5f; // Force for the jump back
    public float lightAttackDuration = 0.5f; // Duration of the air spin
    public override float utility => airSpinDamage / airSpinCD;
}
