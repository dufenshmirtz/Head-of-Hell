using UnityEngine;

[CreateAssetMenu(fileName = "FinSpecs", menuName = "Characters/Fin Specs")]
public class FinSpecs : ScriptableObject
{
    [Header("Spell")]
    public float flashStunDuration = 0.8f;
    public float cooldown = 8f;

    [Header("Light Attack")]
    public float rollPower = 8f;
    public float rollTime = 0.39f;
    public float resetRoll = 2f;

    [Header("Passive")]
    public int passiveDamage = 8;
    
}