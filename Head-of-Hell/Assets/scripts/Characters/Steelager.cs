using System.Collections;
//using UnityEditor.Build;
using UnityEngine;

public class Steelager : Character
{
    float cooldown = 16f;
    int damage = 20;
    float speedSaver = 4f;
    float resetBombs = 2f;
    //bombin
    public GameObject bombPrefab; // The bullet prefab
    public Transform bombPoint; // The point from where the bullet will be instantiated
    public Transform bombsParent;
    bool bombCharging = false;
    bombScript bomba;
    public Transform firePoint;
    public Transform explosionPoint;

    public override void Start()
    {
        base.Start();

        firePoint=resources.firePoint;
        explosionPoint=resources.explosionPoint;
    }

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

            if(knocked){
                enemy.TakeDamageNoAnimation(3,false);
            }else{
                print(moveSpeed);
            }

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
        TelemetryManager.Instance?.LogAction(PlayerId, "Special");
        ignoreDamage = true;
        animator.SetTrigger("Spell");
        audioManager.PlaySFX(audioManager.bigExplosion, audioManager.doubleVol);
        UsingAbility(cooldown);
        stayStatic();
    }

    public void DealExplosionDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(explosionPoint.position, attackRange * 4, enemyLayer);

        if (hitEnemy != null)
        {
            // Telemetry: successful special hit + context before damage
            TelemetryManager.Instance?.LogHitAttempt(PlayerId, enemy.PlayerId, MoveType.Special);
            enemy.SetIncomingDamageContext(PlayerId, MoveType.Special, SourceType.Spell);

            enemy.BreakCharge();
            enemy.TakeDamage(damage, true);
            enemy.Knockback(10f, 0.8f, false);
        }
        else
        {
            // Telemetry: special whiff (no target in AoE)
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Special);
        }
    }

    public void ExplosionReset()
    {
        OnCooldown(cooldown);
        stayDynamic();
        ignoreDamage = false;
    }
    #endregion

    #region LightAttack
    public override void LightAttack()
    {
        if (!bombCharging)
        {
            TelemetryManager.Instance?.LogAction(PlayerId, "Quick");
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
        bombsParent = resources.trash;

        bombCharging = true;
        audioManager.PlaySFX(audioManager.fuse, audioManager.normalVol);
        GameObject bomb = Instantiate(bombPrefab, bombPoint.position,  firePoint.rotation);
        bomba=bomb.GetComponent<bombScript>();
        Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();
        bomb.transform.SetParent(bombsParent);
        StartCoroutine(ResetBomb());
    }

    IEnumerator ResetBomb()
    {
        yield return new WaitForSeconds(resetBombs);
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
