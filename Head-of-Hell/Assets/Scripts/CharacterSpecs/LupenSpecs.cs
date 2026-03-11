using UnityEngine;

[CreateAssetMenu(fileName = "LupenSpecs", menuName = "Characters/Lupen Specs")]
public class LupenSpecs : CharacterSpecsBase
{
    //[Header("Spell")]
    public override float cooldown => 15f;
    public override int damage => 20;

    [Header("Light Attack")]
    public int wipDamage = 6;
    public float resetWip = 2f;
    public override float utility => wipDamage/resetWip;

    [Header("Passive")]
     public int passiveRange = 4;
    
}