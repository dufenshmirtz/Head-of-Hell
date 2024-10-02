using System.Collections;
using UnityEditor.Build;
using UnityEngine;

public class Steelager : Character
{
    float cooldown = 16f;
    int damage = 20;
    float speedSaver = 4f;
    //bombin
    public GameObject bombPrefab; // The bullet prefab
    public Transform bombPoint; // The point from where the bullet will be instantiated
    public Transform bombsParent;
    bool bombCharging = false;
    bombScript bomba;

    bool ignoreSlow=false;

    #region HeavyAttack
    override public void HeavyAttack()
    {
        animator.SetTrigger("HeavyAttack");
        audioManager.PlaySFX(audioManager.heavyswoosh, audioManager.heavySwooshVolume);
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);

        if (hitEnemy != null)
        {

            audioManager.PlaySFX(audioManager.explosion, audioManager.lessVol);
            enemy.TakeDamage(heavyDamage, true);

            if (!player.enemy.isBlocking)
            {
                enemy.Knockback(11f, 0.15f, true);
            }

        }
        else
        {
            audioManager.PlaySFX(audioManager.explosion, audioManager.lessVol);
        }

    }
    #endregion

    #region Spell
    public override void Spell()
    {
        ignoreDamage=true;
        animator.SetTrigger("Spell");
        audioManager.PlaySFX(audioManager.bigExplosion, audioManager.doubleVol);
        player.UsingAbility(cooldown);
        player.stayStatic();
    }

    public void DealExplosionDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.explosionPoint.position, player.attackRange*4, player.enemyLayer);

        if (hitEnemy != null)
        {
            enemy.BreakCharge();
            enemy.TakeDamage(damage, true);
            enemy.Knockback(10f, 0.8f, false);
        }
    }

    public void ExplosionReset()
    {
        player.OnCooldown(cooldown);
        player.stayDynamic();
        ignoreDamage = false;
    }

    #endregion

    #region LightAttack
    public override void LightAttack()
    {
        if (!bombCharging)
        {
            QuickAttackIndicatorDisable();
            ThrowBomb();
        }
        else
        {
            bomba.Explode();
        }
    }
    void ThrowBomb()
    {
        bombPrefab = resources.bomb;
        bombPoint=resources.bombSpawner;
        bombsParent = resources.bombParent;

        bombCharging = true;
        audioManager.PlaySFX(audioManager.fuse, audioManager.normalVol);
        GameObject bomb = Instantiate(bombPrefab, bombPoint.position, player.firePoint.rotation);
        bomba=bomb.GetComponent<bombScript>();
        Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();
        bomb.transform.SetParent(bombsParent);
        StartCoroutine(ResetBomb());
    }

    IEnumerator ResetBomb()
    {
        yield return new WaitForSeconds(2f);
        audioManager.PlaySFX(audioManager.lighter, audioManager.normalVol);
        bombCharging = false;
        QuickAttackIndicatorEnable();
    }

    public void FuseSound()
    {
        audioManager.PlaySFX(audioManager.fuse, 1f); 
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
