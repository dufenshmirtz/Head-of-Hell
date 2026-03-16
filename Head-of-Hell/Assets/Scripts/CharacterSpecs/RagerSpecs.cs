using UnityEngine;

[CreateAssetMenu(fileName = "RagerSpecs", menuName = "Characters/Rager Specs")]
public class RagerSpecs : CharacterSpecsBase
{
    [Header("Spell")]
    public int spellDamage1 = 15;
    public int spellDamage2 = 10;

    public override float cooldown => 20f;
    public override int damage => spellDamage1 + spellDamage2;

    [Header("Light Attack")]
    public int lightDamage = 4;
    public float lightRatio = 0.1f;
    public override float utility => lightDamage/lightRatio;
    
}