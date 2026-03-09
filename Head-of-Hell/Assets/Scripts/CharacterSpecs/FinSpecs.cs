using UnityEngine;

[CreateAssetMenu(fileName = "FinSpecs", menuName = "Characters/Fin Specs")]
public class FinSpecs : CharacterSpecsBase
{
    [Header("Spell")]
    public float flashStunDuration = 0.8f;
    public override float cooldown => 8f;
    public override int damage => 0;

    [Header("Light Attack")]
    public float rollPower = 8f;
    public float rollTime = 0.39f;
    public float resetRoll = 2f;
    public override float utility => 0f;

    [Header("Passive")]
    public int passiveDamage = 8;
    
}