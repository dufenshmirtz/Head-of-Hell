using System.Collections;
using UnityEngine;

public class Vander : Character
{

    int stabDamage = 10, stabHeal = 5;
    float cooldown = 12f;
    //katana
    int katanaDmg = 3;
    int smallLifesteal = 3;
    bool katanaready = true;

    #region HeavyAttack
    override public void HeavyAttack()
    {
        animator.SetTrigger("HeavyAttack");
        audioManager.PlaySFX(audioManager.heavyswoosh, audioManager.heavySwooshVolume);
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle( attackPoint.position,  attackRange,  enemyLayer);

        if (hitEnemy != null)
        {

            audioManager.PlaySFX(audioManager.katanaHit, 1f);
            enemy.TakeDamage(heavyDamage, true);
            Lifesteal(smallLifesteal);
            if (! enemy.isBlocking)
            {
                enemy.Knockback(11f, 0.15f, true);
            }

        }
        else
        {
            audioManager.PlaySFX(audioManager.katanaSwoosh, 1f);
        }

    }
    #endregion

    #region Spell
    override public void Spell()
    {
        attackRange += 0.5f;
        animator.SetTrigger("Spell");
        UsingAbility(cooldown);
        ignoreDamage = true;
    }

    public void DealStabDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle( attackPoint.position,  attackRange,  enemyLayer);

        if (hitEnemy != null)
        {
            enemy.StopPunching();
            enemy.BreakCharge();
            enemy.TakeDamage(stabDamage, true);
            Lifesteal(stabHeal);
             healthbar.SetHealth( currHealth);
            audioManager.PlaySFX(audioManager.stabHit, audioManager.doubleVol);
        }
        else
        {
            audioManager.PlaySFX(audioManager.stab, audioManager.swooshVolume);
        }

         attackRange =  ogRange;

         OnCooldown(cooldown);
        ignoreDamage = false;

    }
    #endregion

    #region LightAttack
    public override void LightAttack()
    {
        if (katanaready)
        {
            QuickAttackIndicatorDisable();
            animator.SetTrigger("QuickAttack");
            katanaready = false;
            StartCoroutine(ResetKatana());
        }
    }

    public void DealKatanaDmg1()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle( attackPoint.position,  attackRange,  enemyLayer);

        if (hitEnemy != null)
        {
            enemy.TakeDamage(katanaDmg, true);
            audioManager.PlaySFX(audioManager.katanaHit, audioManager.lightAttackVolume);
        }
        else
        {
            audioManager.PlaySFX(audioManager.katanaSwoosh, audioManager.swooshVolume);
        }

    }

    public void DealKatanaDmg2()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle( attackPoint.position,  attackRange,  enemyLayer);

        if (hitEnemy != null)
        {
            enemy.TakeDamage(katanaDmg, true);
            Lifesteal(smallLifesteal);
            enemy.Knockback(10f, .15f, true);
            audioManager.PlaySFX(audioManager.katanaHit2, 1.5f);
        }
        else
        {
            audioManager.PlaySFX(audioManager.katanaSwoosh, audioManager.swooshVolume);
        }

    }

    IEnumerator ResetKatana()
    {

        yield return new WaitForSeconds(2f);
        audioManager.PlaySFX(audioManager.katanaSeath, audioManager.doubleVol);
        katanaready = true;
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

    public override void DealChargeDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle( attackPoint.position,  attackRange,  enemyLayer);

        if (hitEnemy != null)
        {
            enemy.StopPunching();
            enemy.BreakCharge();
            enemy.TakeDamage(chargeDmg,false);
            Lifesteal(8);
            enemy.Knockback(13f, 0.4f, false);
            audioManager.PlaySFX(audioManager.smash, audioManager.doubleVol);
            if (chargeHitSound != null)
            {
                audioManager.PlaySFX(chargeHitSound, 1.5f);
            }
        }
        else
        {
            if (chargeHitSound != null)
            {
                audioManager.PlaySFX(chargeHitSound, 1.5f);
            }
            else
            {
                audioManager.PlaySFX(audioManager.swoosh, audioManager.swooshVolume);
            }
            
        }
        chargeReset = true;
        knockable = true;
        charging = false;
        animator.SetBool("Casting", false);
        animator.SetBool("Charging", false);
    }


    void Lifesteal(int amount)
    {
         currHealth += amount;
        if ( currHealth >  maxHealth)
        {
             currHealth =  maxHealth;
        }
         healthbar.SetHealth( currHealth);
    }
    #endregion
}
