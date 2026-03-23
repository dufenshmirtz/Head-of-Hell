using System;
using System.Collections;
using UnityEngine;

public class Custom : Character
{
    float cooldown = 16f;
    int damage = 18;
    float spellTime = 0.94f;

    String lightName;

    String spellName;


    #region HeavyAttack
    override public void HeavyAttack()
    {
        TelemetryManager.Instance?.LogAction(PlayerId, "Heavy");
        animator.SetTrigger("HeavyAttack");
        audioManager.PlaySFX(audioManager.heavyswoosh, audioManager.heavySwooshVolume);
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle( attackPoint.position,  attackRange,  enemyLayer);

        if (hitEnemy != null)
        {
            audioManager.PlaySFX(audioManager.explosion, audioManager.lessVol);
            TelemetryManager.Instance?.LogHitAttempt(PlayerId, enemy.PlayerId, MoveType.Heavy);
            enemy.SetIncomingDamageContext(PlayerId, MoveType.Heavy, SourceType.Melee);
            enemy.TakeDamage(heavyDamage, true);

            if (!enemy.isBlocking)
            {
                enemy.Knockback(11f, 0.15f, true);
            }

        }
        else
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Heavy);
            audioManager.PlaySFX(audioManager.explosion, audioManager.lessVol);
        }

    }
    #endregion

    #region Spell
    public override void Spell()
    {
        SelectSpell(spellName);
        TelemetryManager.Instance?.LogAction(PlayerId, "Special");
        ignoreDamage = true;
        animator.SetTrigger("Spell");
        audioManager.PlaySFX(audioManager.bigExplosion, audioManager.doubleVol);
        UsingAbility(cooldown);
        stayStatic();
        StartCoroutine(SpellSafety(spellTime,cooldown));
    }

    public void SelectSpell(String name)
    {
        
    }

    #endregion

    #region LightAttack
    public override void LightAttack()
    {
        SelectLight(lightName);
    }

    public void SelectLight(String name)
    {
        
    }
    
    #endregion

    #region ChargeAttack
    public override void ChargeAttack()
    {
        base.ChargeAttack();
        animator.SetTrigger("Charge");
    }
    #endregion

}
