using UnityEngine;

[CreateAssetMenu(fileName = "LazyBigusSpecs", menuName = "Characters/LazyBigus Specs")]
public class LazyBigusSpecs : CharacterSpecsBase
{
    [Header("Spell")]
    public int beamDamage = 10;
    public int beamPoisonDamage = 10;
    
    public override float cooldown => 20f;
    public override int damage => beamDamage + beamPoisonDamage;

    [Header("Light Attack")]
    public int bulletDamage = 3;
    public float resetBullet = 2f;
    public override float utility => bulletDamage/resetBullet;

    [Header("Passive")]
    public int passiveDamage = 4;
    
}