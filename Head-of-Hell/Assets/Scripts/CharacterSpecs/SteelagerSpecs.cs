using UnityEngine;

[CreateAssetMenu(fileName = "SteelagerSpecs", menuName = "Characters/Steelager Specs")]
public class SteelagerSpecs : CharacterSpecsBase
{
    //[Header("Spell")]
    public override float cooldown => 16f;
    public override int damage => 16;

    [Header("Light Attack")]
    public int bombDamage = 6;
    public float resetBomb = 2f;
    public override float utility => bombDamage/resetBomb;

    [Header("Passive")]
     public int comboDamage = 3;
    
}