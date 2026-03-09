using UnityEngine;

[CreateAssetMenu(fileName = "ChibackSpecs", menuName = "Characters/Chiback Specs")]
public class ChibackSpecs : CharacterSpecsBase
{
    [Header("Spell")]
    public float jumpHeight = 5f;
    public float jumpSpeed = 10f;
    public float jumpDuration = 1f;
    public int shortJumpDamage = 5, MedJumpDamage = 10, wideJumpDamage = 15;

    public override float cooldown => 10f;
    public override int damage => (shortJumpDamage+MedJumpDamage+wideJumpDamage)/3;

    [Header("Light Attack")]
    public int disableDamage = 5;
    public float resetDisable = 1f;
    public override float utility => disableDamage/resetDisable;

    [Header("Passive")]
     public int enragingNum = 3;
    
}