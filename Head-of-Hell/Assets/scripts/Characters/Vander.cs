using System.Collections;
using UnityEngine;

public class Vander : Character
{

    int stabDamage = 10, stabHeal = 5;
    float cooldown = 12f;
    //katana
    int katanaDmg = 3;
    bool katanaready = true;

    #region HeavyAttack
    override public void HeavyAttack()
    {
        animator.SetTrigger("VanderHeavy");
        audioManager.PlaySFX(audioManager.heavyswoosh, audioManager.heavySwooshVolume);
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);

        if (hitEnemy != null)
        {

            audioManager.PlaySFX(audioManager.katanaHit, 1f);
            enemy.TakeDamage(heavyDamage);

            if (!player.enemy.isBlocking)
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
        player.attackRange += 0.5f;
        animator.SetTrigger("Stab");
        player.UsingAbility(cooldown);
    }

    public void DealStabDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);

        if (hitEnemy != null)
        {
            enemy.StopPunching();
            enemy.BreakCharge();
            enemy.TakeDamage(stabDamage);
            player.currHealth += stabHeal;

            if (player.currHealth > player.maxHealth)
            {
                player.currHealth = player.maxHealth;
            }
            player.healthbar.SetHealth(player.currHealth);
            audioManager.PlaySFX(audioManager.stabHit, audioManager.doubleVol);
        }
        else
        {
            audioManager.PlaySFX(audioManager.stab, audioManager.swooshVolume);
        }

        player.attackRange = player.ogRange;

        player.OnCooldown(cooldown);

    }
    #endregion

    #region LightAttack
    public override void LightAttack()
    {
        if (katanaready)
        {
            QuickAttackIndicatorDisable();
            animator.SetTrigger("katana");
            katanaready = false;
            StartCoroutine(ResetKatana());
        }
    }

    public void DealKatanaDmg1()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);

        if (hitEnemy != null)
        {
            enemy.TakeDamage(katanaDmg);
            audioManager.PlaySFX(audioManager.katanaHit, audioManager.lightAttackVolume);
        }
        else
        {
            audioManager.PlaySFX(audioManager.katanaSwoosh, audioManager.swooshVolume);
        }

    }

    public void DealKatanaDmg2()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);

        if (hitEnemy != null)
        {
            enemy.TakeDamage(katanaDmg);
            player.currHealth += katanaDmg;
            if (player.currHealth > player.maxHealth)
            {
                player.currHealth = player.maxHealth;
            }
            player.healthbar.SetHealth(player.currHealth);
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
        animator.SetTrigger("VanderCharge");
    }
    #endregion
}
