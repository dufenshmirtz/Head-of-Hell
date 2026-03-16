using UnityEngine;

[CreateAssetMenu(fileName = "SkiplerSpecs", menuName = "Characters/Skipler Specs")]
public class SkiplerSpecs : CharacterSpecsBase
{
    [Header("Spell")]
    public float dashingPower = 40f;
    public float dashingTime = 0.1f;
    
    public override float cooldown => 8f;
    public override int damage => 10;



    [Header("Light Attack")]
    public int blinkDamage = 5;
    public float blinkCD = 3f;

    public float blinkPower = 10f;
    public float blinkTime = 0.14f;
    public override float utility => blinkDamage / blinkCD;
}
