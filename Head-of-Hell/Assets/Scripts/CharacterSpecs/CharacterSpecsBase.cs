using UnityEngine;

public abstract class CharacterSpecsBase : ScriptableObject
{
    public abstract int damage { get; }
    public abstract float cooldown { get; }
    public abstract float utility { get; }
}