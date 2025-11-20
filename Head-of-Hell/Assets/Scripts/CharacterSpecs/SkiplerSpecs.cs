using UnityEngine;

[CreateAssetMenu(fileName = "SkiplerSpecs", menuName = "Characters/Skipler Specs")]
public class SkiplerSpecs : ScriptableObject
{
    [Header("Spell")]
    public float cooldown = 8f;
    public int dashDamage = 10;

    [Header("Light Attack")]
    public int blinkDamage = 5;
    public float blinkCD = 3f;
}
