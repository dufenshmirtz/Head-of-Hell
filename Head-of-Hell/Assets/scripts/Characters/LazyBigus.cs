using System.Collections;
using UnityEngine;
using Transform = UnityEngine.Transform;

public class LazyBigus : Character
{
    public GameObject bulletPrefab; // The bullet prefab
    public Transform firePoint; // The point from where the bullet will be instantiated
    Transform bulletParent; 
    public float bulletSpeed = 35f; // Speed of the bullet
    bool isShootin = false;
    float cooldown = 20f;
    int poisonCounter = 0;
    public GameObject beam;
    public BeamScript bScript;

    public override void Start()
    {
        base.Start();

        firePoint = resources.firePoint;
        beam = resources.beam;
    }

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

            audioManager.PlaySFX(audioManager.BigusHeavy, 1f);
            enemy.TakeDamage(heavyDamage, true);
            ToxicTouch();

            if (! enemy.isBlocking)
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
        IgnoreUpdate(true);
        stayStatic();
        UsingAbility(cooldown);
        beam.SetActive(true);
        bScript = beam.GetComponent<BeamScript>();
        bScript.playa = this;
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
        enemy.ActivatePoison(true);

        audioManager.PlaySFX(audioManager.poison, 2.5f);
        for (int i = 0; i < times; i++)
        {
            yield return new WaitForSeconds(interval);

            // Deal damage to the enemy
            enemy.TakeDamage(damageAmount,false);
        }
        enemy.ActivatePoison(false);
    }

    public void BeamEnd()
    {
         OnCooldown(cooldown);
         IgnoreUpdate(false);
         stayDynamic();
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

    override public void DealChargeDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

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
        chargeReset = true;
        knockable = true;
        charging = false;
        animator.SetBool("Casting", false);
        animator.SetBool("Charging", false);
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
        if (!enemy.IsPoisoned())
        {
            if(poisonCounter < 3)
            {
                if(poisonCounter==0)
                {
                    enemy.StackPoison1(true);
                }
                if (poisonCounter == 1)
                {
                    enemy.StackPoison1(false);
                    enemy.StackPoison2(true);
                }
                if (poisonCounter == 2)
                {
                    enemy.StackPoison2(false);
                    enemy.StackPoison3(true);
                }
                poisonCounter++;
            }
        }
    }

    

    private void ResetPoisonStacks()
    {
        enemy.StackPoison1(false);
        enemy.StackPoison2(false);
        enemy.StackPoison3(false);
    }

    public void BeamHit()
    {
        BeamHitEnemy();
    }

    public void StackPoison()
    {
        if (!enemy.isBlocking)
        {
            AddPoison();
        }

    }
    #endregion
}
