using UnityEngine;

[CreateAssetMenu(fileName = "SteelagerSpecs", menuName = "Characters/Steelager Specs")]
public class SteelagerSpecs : ScriptableObject
{
    [Header("Spell")]
    public float cooldown = 16f;
    public int damage = 20;

    [Header("Light Attack")]
    public float rollPower = 8f;
    public float rollTime = 0.39f;
    public float resetRoll = 2f;

    [Header("Passive")]
    public int passiveDamage = 8;
    
}