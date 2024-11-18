using System.Collections;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements.Experimental;

public class Lupen : Character
{
    //Spell
    float cooldown = 15f; //Mesos oros twn allwn +- ligo
    CharacterAnimationEvents cEvents;
    LupenSpirit spirit;
    string randomCharacter;

    public override void Start()
    {
        base.Start();

        cEvents = this.GetComponent<CharacterAnimationEvents>();

        spirit=this.gameObject.AddComponent<LupenSpirit>();
        spirit.lupen = this;
        spirit.ability=ability;
        spirit.playerString = playerString;
        spirit.controller = controller;
        spirit.characterChoiceHandler= characterChoiceHandler;
        spirit.cEvents = cEvents;
    }

    #region HeavyAttack
    override public void HeavyAttack()
    {
        if (usingAbility)
        {
            return;
        }
        animator.SetTrigger("HeavyAttack");
        audioManager.PlaySFX(audioManager.heavyswoosh, audioManager.heavySwooshVolume);
        ResetQuickPunch();
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {

            audioManager.PlaySFX(audioManager.heavyattack, 1f);
            enemy.TakeDamage(heavyDamage, true);

            if (!enemy.isBlocking)
            {
                enemy.Knockback(11f, 0.15f, true);
            }

        }
        else
        {
            audioManager.PlaySFX(audioManager.swoosh, 1f);
        }

        ResetQuickPunch();

    }
    #endregion

    #region Spell
    override public void Spell()
    {
        stayStatic();
        animator.SetTrigger("Spell");
    }

    public void Transformation()
    {
        do
        {
            randomCharacter = characterChoiceHandler.PickRandomCharacter();
        } while (randomCharacter=="Lupen");
        
        characterChoiceHandler.ChangeCharacter(randomCharacter);
        cEvents.ChangeCharacterEvents(1);
        stayDynamic();
        spirit.Action();
    }

    public void ReturnToLupen()
    {
        RemoveLastAttachedScript();
        characterChoiceHandler.ChangeCharacter("Lupen");
        cEvents.ChangeCharacterEvents(2);
        OnCooldown(cooldown);
    }

    public void RemoveLastAttachedScript()
    {
        // Get all components attached to the GameObject
        Component[] components = this.GetComponents<Component>();

        // Ensure the GameObject has components beyond the Transform
        if (components.Length > 1)
        {
            // Get the last component (excluding Transform, which is always first)
            Component lastComponent = components[components.Length - 1];

            // Destroy the last component
            Destroy(lastComponent);

            Debug.Log($"Removed component: {lastComponent.GetType().Name}");
        }
        else
        {
            Debug.LogWarning("No scripts to remove on this GameObject.");
        }
    }

    #endregion

    #region LightAttack
    public override void LightAttack()
    {
    }
    
    #endregion
}
