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
    int wipDamage=1;
    bool wipReady = true;
    Transform wipPoint;
    int robberyCountter=0;
    Character stolenCharacter;

    public float hitBoxWidth = 2f; // Length in the X-axis
    public float hitBoxHeight = 0.5f; // Thinness in the Y-axi
    Vector2 size;

    public override void Start()
    {
        base.Start();

        cEvents = this.GetComponent<CharacterAnimationEvents>();
        wipPoint = resources.wipPoint;

        spirit=this.gameObject.AddComponent<LupenSpirit>();
        spirit.lupen = this;
        spirit.ability=ability;
        spirit.playerString = playerString;
        spirit.controller = controller;
        spirit.characterChoiceHandler= characterChoiceHandler;
        spirit.cEvents = cEvents;
        spirit.enemy = enemy;
        spirit.healthbar = healthbar;
        spirit.maxHealth=maxHealth;

        size = new Vector2(hitBoxWidth, hitBoxHeight);
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
            Robbed();

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
        UsingAbility(cooldown);
    }

    public void Transformation()
    {
        do
        {
            randomCharacter = characterChoiceHandler.PickRandomCharacter();
        } while (randomCharacter=="Lupen");
        
        characterChoiceHandler.ChangeCharacter(randomCharacter);
        cEvents.ChangeCharacterEvents(1);
        stolenCharacter = characterChoiceHandler.CharacterChoice(1);
        stayDynamic();
        enemy.ChangeEnemy(stolenCharacter);
        //SaveValues and change form
        spirit.stolenCharacter = stolenCharacter;
        spirit.currentHealth = currHealth;
        spirit.whipDamage = wipDamage;
        spirit.robberyCounter=robberyCountter;
        spirit.Action();
    }

    public void ReturnToLupen(int wdmg,int rc,int currentHealth)
    {
        wipDamage=wdmg;
        robberyCountter=rc;
        currHealth = currentHealth;
        RemoveLastAttachedScript();
        characterChoiceHandler.ChangeCharacter("Lupen");
        cEvents.ChangeCharacterEvents(2);
        enemy.ChangeEnemy(characterChoiceHandler.CharacterChoice(1));
        P1Name.text = "Lupen";
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
        if (wipReady)
        {
            QuickAttackIndicatorDisable();
            animator.SetTrigger("QuickAttack");
            wipReady = false;
            StartCoroutine(ResetWip());
        }
    }

    public void DealWipDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCapsule(wipPoint.position,size, CapsuleDirection2D.Horizontal,0f,enemyLayer);

        if (hitEnemy != null)
        {
            enemy.TakeDamage(wipDamage, true);
            Robbed();
            enemy.Slow(1.5f,2f);
            StartCoroutine(SpeedUpCoroutine(1.5f,2f));
            audioManager.PlaySFX(audioManager.counterSucces, audioManager.doubleVol);
        }
        else
        {
            audioManager.PlaySFX(audioManager.stab, audioManager.swooshVolume);
        }

    }

    public IEnumerator SpeedUpCoroutine(float time, float amount)
    {

        moveSpeed = moveSpeed + amount;

        yield return new WaitForSeconds(time);

        moveSpeed = OGMoveSpeed;
    }

    IEnumerator ResetWip()
    {
        yield return new WaitForSeconds(2f);
        audioManager.PlaySFX(audioManager.katanaSeath, audioManager.doubleVol);
        wipReady = true;
        QuickAttackIndicatorEnable();
    }

    #endregion

    #region ChargeAttack
    public override void ChargeAttack()
    {
        base.ChargeAttack();
        animator.SetTrigger("Charge");
    }
    #endregion

    #region Passive
    public void Robbed()
    {
        robberyCountter++;
        if (robberyCountter == 3)
        {
            if (currHealth > enemy.GetCurrentHealth())
            {
                wipDamage++;
                print("wipDamage: " + wipDamage);
            }
            else
            {
                Heal(5);
            }
            robberyCountter = 0;
        }
    }
    #endregion

    public override void TakeDamage(int dmg, bool blockable)
    {
        base.TakeDamage(dmg, blockable);       

        animator.SetTrigger(characterChoiceHandler.PickRandomCharacter());
    }
}
