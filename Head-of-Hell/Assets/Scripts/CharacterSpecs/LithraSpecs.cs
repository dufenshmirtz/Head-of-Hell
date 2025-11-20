using UnityEngine;

[CreateAssetMenu(fileName = "LithraSpecs", menuName = "Characters/Lithra Specs")]
public class LithraSpecs : ScriptableObject
{
    [Header("Spell")]
    float cooldown = 10f;
    int bellDamage = 10;

    [Header("Light Attack")]
    int airSpinDamage = 3;
    float airSpinCD = 3f;
}
