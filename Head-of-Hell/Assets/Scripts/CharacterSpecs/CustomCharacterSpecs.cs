using UnityEngine;

[CreateAssetMenu(fileName = "CustomCharacterSpecs", menuName = "Characters/Custom Character Specs")]
public class CustomCharacterSpecs : CharacterSpecsBase
{
    public int listedDamage = 12;
    public float listedCooldown = 10f;
    public float listedUtility = 2f;

    public override int damage => listedDamage;
    public override float cooldown => listedCooldown;
    public override float utility => listedUtility;
}
