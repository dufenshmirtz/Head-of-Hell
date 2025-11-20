using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Steelager : Character
{
    float cooldown = 16f;
    int damage = 20;
    float speedSaver = 4f;

    public GameObject bombPrefab;
    public Transform bombPoint;
    public Transform bombsParent;
    bool bombCharging = false;
    bombScript bomba;
    public Transform firePoint;
    public Transform explosionPoint;

    public override void Start()
    {
        base.Start();
        firePoint = resources.firePoint;
        explosionPoint = resources.explosionPoint;
    }

    #region HeavyAttack
    override public void HeavyAttack()
    {
        animator.SetTrigger("HeavyAttack");
        audioManager.PlaySFX(audioManager.heavyswoosh, audioManager.heavySwooshVolume);
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {
            audioManager.PlaySFX(audioManager.explosion, audioManager.lessVol);

            // MAIN DAMAGE
            enemy.photonView.RPC("RPC_TakeDamage", RpcTarget.All, heavyDamage, true, true);

            // EXTRA DAMAGE IF KNOCKED
            if (knocked)
            {
                enemy.photonView.RPC("RPC_TakeDamageNoAnimation", RpcTarget.All, 3, false, false);
            }

            // KNOCKBACK
            if (!enemy.isBlocking)
            {
                enemy.photonView.RPC("RPC_Knockback", RpcTarget.All, 11f, 0.15f, true);
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
            // RPC BreakCharge
            enemy.photonView.RPC("RPC_BreakCharge", RpcTarget.All);

            // RPC Damage
            enemy.photonView.RPC("RPC_TakeDamage", RpcTarget.All, damage, true, true);

            // RPC Knockback
            enemy.photonView.RPC("RPC_Knockback", RpcTarget.All, 10f, 0.8f, false);
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
            QuickAttackIndicatorDisable();
            ThrowBomb();
        }
        else
        {
            bomba.Explode(); // NOTE: explosion damage must be RPC inside bombScript
        }
    }

    void ThrowBomb()
    {
        bombPrefab = resources.bomb;
        bombPoint = resources.bombSpawner;
        bombsParent = resources.trash;

        bombCharging = true;
        audioManager.PlaySFX(audioManager.fuse, audioManager.normalVol);

        GameObject bomb = Instantiate(bombPrefab, bombPoint.position, firePoint.rotation);
        bomba = bomb.GetComponent<bombScript>();
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
