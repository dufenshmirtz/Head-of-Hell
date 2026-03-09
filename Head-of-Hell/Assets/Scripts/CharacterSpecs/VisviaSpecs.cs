using UnityEngine;

[CreateAssetMenu(fileName = "VisviaSpecs", menuName = "Characters/Visvia Specs")]
public class VisviaSpecs : CharacterSpecsBase
{
    [Header("Spell")]
    public int grabDamage = 6;

    public override float cooldown => 10;
    public override int damage => grabDamage*2;

    [Header("Light Attack")]
    public float shotgunForce = 15f;
    public float upwardsForce = 6f;
    public float shotgunCooldown = 4f;
    public float dashDuration = 0.3f;
    public float shotgunRange = 1f;
    public int shotgunDamage = 6;
    public override float utility => shotgunDamage/shotgunCooldown;

    [Header("Passive")]
    public int blastCounter=0;
    public float overheatDuration = 8f;
    public float elapsed = 0f;
    public float overheatFrequency=0.2f;
    public int overheatDamage=1;
    
}