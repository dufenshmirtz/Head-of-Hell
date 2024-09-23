using System.Collections;
using UnityEngine;

public class Steelager : Character
{
    float cooldown = 30f;
    float speedSaver = 4f;
    //bombin
    public GameObject bombPrefab; // The bullet prefab
    public Transform bombPoint; // The point from where the bullet will be instantiated
    public Transform bombsParent;
    bool bombCharging = false;

    bool ignoreSlow=false;

    #region HeavyAttack
    override public void HeavyAttack()
    {
        animator.SetTrigger("BombPunch");
        audioManager.PlaySFX(audioManager.heavyswoosh, audioManager.heavySwooshVolume);
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);

        if (hitEnemy != null)
        {

            audioManager.PlaySFX(audioManager.explosion, audioManager.lessVol);
            enemy.TakeDamage(heavyDamage);

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

    override public void HeavyAttackStart()
    {
        if(ignoreSlow){
            animator.SetBool("isHeavypunching", true);
            return;
        }

        base.HeavyAttackStart();
    }
    #endregion

    #region Spell
    public override void Spell()
    {
        ignoreDamage=true;
        player.IgnoreSlow(true);
        ignoreSlow=true;

        player.ChangeSpeed(player.moveSpeed + 2f);
        speedSaver = player.OGMoveSpeed;
        player.ChangeOGSpeed(player.moveSpeed);

        animator.SetTrigger("FullGeros");
        audioManager.PlaySFX(audioManager.growl, audioManager.heavyAttackVolume);

        player.UsingAbility(cooldown);

        player.stayStatic();
        StartCoroutine(Invulnerable(10));
    }

    public void ReActivate()
    {
        player.ActivateColliders();
        player.stayDynamic();
        animator.SetTrigger("animationOver");
    }

    IEnumerator Invulnerable(float delay)
    {
        yield return new WaitForSeconds(delay);
        ignoreDamage=false;
        player.IgnoreSlow(false);
        ignoreSlow=false;

        player.ChangeOGSpeed(speedSaver);
        player.ChangeSpeed(speedSaver);

        player.OnCooldown(cooldown);
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
    }
    void ThrowBomb()
    {
        bombPrefab = resources.bomb;
        bombPoint=resources.bombSpawner;
        bombsParent = resources.bombParent;

        bombCharging = true;
        audioManager.PlaySFX(audioManager.fuse, audioManager.normalVol);
        GameObject bomb = Instantiate(bombPrefab, bombPoint.position, player.firePoint.rotation);
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
        animator.SetTrigger("TNTCharge");
    }
    #endregion
}
