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
    int poisonCounter = 0;

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

            audioManager.PlaySFX(audioManager.BigusHeavy, 1f);
            enemy.TakeDamage(heavyDamage, true);
            ToxicTouch();

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
        animator.SetTrigger("Spell");
        audioManager.PlaySFX(audioManager.beam, audioManager.doubleVol);
        enemy.BreakCharge();
        ignoreDamage = true;
    }

    public void BeamHitEnemy()
    {
        enemy.TakeDamage(10,true);
        enemy.Knockback(13f, 0.5f, true);
        audioManager.PlaySFX(audioManager.beamHit, 1.8f);
        StartCoroutine(Poison(2,2f,5));
    }

    private IEnumerator Poison(int damageAmount, float interval, int times)
    {
        ResetPoisonStacks();
        enemy.poison.SetActive(true);

        audioManager.PlaySFX(audioManager.poison, 2.5f);
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
        ignoreDamage=false;
    }
    
    #endregion

    #region LightAttack

    public override void LightAttack()
    {
        if (!isShootin)
        {
            QuickAttackIndicatorDisable();
            animator.SetTrigger("QuickAttack");
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
        animator.SetTrigger("Charge");
    }
    #endregion

    #region Passive

    void ToxicTouch()
    {
        if(poisonCounter == 3)
        {
            StartCoroutine(Poison(2,2f,2));
            poisonCounter = 0;
            return;
        }

        AddPoison();
    }

    public void AddPoison()
    {
        if (!enemy.poison.gameObject.activeSelf)
        {
            if(poisonCounter < 3)
            {
                if(poisonCounter==0)
                {
                    enemy.Stack1Poison.gameObject.SetActive(true);
                }
                if (poisonCounter == 1)
                {
                    enemy.Stack1Poison.gameObject.SetActive(false);
                    enemy.Stack2Poison.gameObject.SetActive(true);
                }
                if (poisonCounter == 2)
                {
                    enemy.Stack2Poison.gameObject.SetActive(false);
                    enemy.Stack3Poison.gameObject.SetActive(true);
                }
                poisonCounter++;
            }
        }
    }

    override public void DealChargeDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);

        if (hitEnemy != null)
        {
            enemy.StopPunching();
            enemy.BreakCharge();
            enemy.TakeDamage(chargeDmg, false);
            enemy.Knockback(13f, 0.4f, false);
            audioManager.PlaySFX(audioManager.smash, audioManager.doubleVol);
            ToxicTouch();
        }
        else
        {
            audioManager.PlaySFX(audioManager.swoosh, audioManager.swooshVolume);
        }
        player.knockable = true;
        charging = false;
        animator.SetBool("Casting", false);
        animator.SetBool("Charging", false);
        player.stayDynamic();
    }

    private void ResetPoisonStacks()
    {
        enemy.Stack1Poison.SetActive(false);
        enemy.Stack2Poison.SetActive(false);
        enemy.Stack3Poison.SetActive(false);
    }
    #endregion
}
