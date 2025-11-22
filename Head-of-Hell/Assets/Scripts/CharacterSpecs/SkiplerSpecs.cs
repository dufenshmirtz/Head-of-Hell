using UnityEngine;

[CreateAssetMenu(fileName = "SkiplerSpecs", menuName = "Characters/Skipler Specs")]
public class SkiplerSpecs : ScriptableObject
{
    [Header("Spell")]
    public float cooldown = 8f;
    public int dashDamage = 10;

    public float dashingPower = 40f;
    public float dashingTime = 0.1f;

    [Header("Light Attack")]
    public int blinkDamage = 5;
    public float blinkCD = 3f;

    public float blinkPower = 10f;
    public float blinkTime = 0.14f;
}
