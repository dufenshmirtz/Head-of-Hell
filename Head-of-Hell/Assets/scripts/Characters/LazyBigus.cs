using System.Collections;
using UnityEngine;
using Transform = UnityEngine.Transform;

public class LazyBigus : Character
{
    public GameObject bulletPrefab; // The bullet prefab
    public Transform firePoint; // The point from where the bullet will be instantiated
    public Transform bulletParent; 
    public float bulletSpeed = 35f; // Speed of the bullet
    bool isShootin = false;
    public KeyCode shoot;
    float cooldown = 20f;
    float heal = 30f, healTime;

    #region HeavyAttack
    override public void HeavyAttack()
    {
        animator.SetTrigger("BigusHeavy");
        audioManager.PlaySFX(audioManager.heavyswoosh, audioManager.heavySwooshVolume);
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);

        if (hitEnemy != null)
        {

            audioManager.PlaySFX(audioManager.BigusHeavy, 1f);
            enemy.TakeDamage(heavyDamage, true);

            if (!player.enemy.isBlocking)
            {
                enemy.Knockback(11f, 0.15f, true);
            }

        }
        else
        {
            audioManager.PlaySFX(audioManager.swoosh, 1f);
        }

    }
    #endregion

    #region Spell
    override public void Spell()
    {
        player.IgnoreUpdate(true);
        player.stayStatic();
        player.UsingAbility(cooldown);
        player.beam.SetActive(true);
        animator.SetTrigger("Beam");
        audioManager.PlaySFX(audioManager.beam, audioManager.doubleVol);
    }

    public void BeamHitEnemy()
    {
        print("negada");
        enemy.BreakCharge();
        enemy.TakeDamage(10,true);
        enemy.Knockback(13f, 0.5f, true);
        audioManager.PlaySFX(audioManager.beamHit, 1.8f);
        enemy.poison.SetActive(true);
        StartCoroutine(Poison(2,2f,5));
    }

    private IEnumerator Poison(int damageAmount, float interval, int times)
    {
        for (int i = 0; i < times; i++)
        {
            yield return new WaitForSeconds(interval);

            // Deal damage to the enemy
            enemy.TakeDamage(damageAmount,false);
        }
        enemy.poison.SetActive(false);
    }

    public void BeamEnd()
    {
        player.OnCooldown(cooldown);
        player.IgnoreUpdate(false);
        player.stayDynamic();
    }
    
    #endregion

    #region LightAttack

    public override void LightAttack()
    {
        if (!isShootin)
        {
            QuickAttackIndicatorDisable();
            animator.SetTrigger("Shoot");
            StartCoroutine(ResetShooting());
        }
    }

    public void Shoot()
    {
        bulletPrefab = resources.bullet;
        firePoint = resources.shootinPoint;
        bulletParent = resources.bulletParent;

        audioManager.PlaySFX(audioManager.shoot, audioManager.normalVol);
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(transform.localScale.x * bulletSpeed, 0); // Shoots in the direction the character is facing

        Destroy(bullet, 2f);
    }

    public void firstShootFrame()
    {
        isShootin = true;
    }

    IEnumerator ResetShooting()
    {

        yield return new WaitForSeconds(2f);
        audioManager.PlaySFX(audioManager.reload, audioManager.normalVol);
        isShootin = false;
        QuickAttackIndicatorEnable();
    }

    #endregion

    #region ChargeAttack
    public override void ChargeAttack()
    {
        base.ChargeAttack();
        animator.SetTrigger("GunCharge");
    }
    #endregion
}
