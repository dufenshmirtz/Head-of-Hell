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
    public float cooldown = 20f;
    int poisonCounter = 0;
    public GameObject beam;
    public BeamScript bScript;
    private Coroutine poisonResetCoroutine;
    public BulletScript bulletScript;
    bool beamHit=false;
    int beamDamage = 10;
    int beamPoisonDamage = 10; 
    int passiveDamage = 4;
    float resetBullet=2f;
    Character poisonTarget;

    float spellTime = 1f;

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
        audioManager.PlaySFX(audioManager.volchBite, 1.5f);
        audioManager.PlaySFX(audioManager.volchBiteExtra, 1.5f);
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle( attackPoint.position,  attackRange,  enemyLayer);
        Character target = ResolveTargetFromHit(hitEnemy);

        if (target != null)
        {

            audioManager.PlaySFX(audioManager.volchBiteSuccess, 1.5f);
            enemy.SetIncomingDamageContext(PlayerId, MoveType.Heavy, SourceType.Melee);
            enemy.TakeDamage(heavyDamage, true);
            ToxicTouch();

            if (! enemy.isBlocking)
            {
                enemy.Knockback(11f, 0.15f, true);
            }

        }
        else
        {
            //audioManager.PlaySFX(audioManager.swoosh, 1f);
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
        ignoreDamage = true;
        StartCoroutine(SpellSafety(spellTime,cooldown));
    }

    public void BeamHitEnemy()
    {
        if(!beamHit){
            poisonTarget = enemy;
            enemy.SetIncomingDamageContext(PlayerId, MoveType.Projectile, SourceType.Projectile);
            enemy.TakeDamage(beamDamage,true);
            enemy.StopPunching();
            enemy.BreakCharge();
            enemy.Knockback(13f, 0.5f, true);
            audioManager.PlaySFX(audioManager.beamHit, 1.8f);
            StartCoroutine(Poison(poisonTarget, beamPoisonDamage/5,1f,5));
            StartCoroutine(BeamDetectorReset());
        }
    }

    private IEnumerator BeamDetectorReset(){

        beamHit=true;

        yield return new WaitForSeconds(1f);

        beamHit=false;
    }

    private IEnumerator Poison(Character target, int damageAmount, float interval, int times)
    {
        if (target == null)
        {
            yield break;
        }

        ResetPoisonStacks(target);
        target.ActivatePoison(true);

        audioManager.PlaySFX(audioManager.poison, 2.5f);
        for (int i = 0; i < times; i++)
        {
            yield return new WaitForSeconds(interval);

            // Deal damage to the enemy
            if (target == null || !target.isActiveAndEnabled)
            {
                yield break;
            }

            target.SetIncomingDamageContext(PlayerId, MoveType.PoisonTick, SourceType.Dot);
            target.TakeDamageNoAnimation(damageAmount,false,false);
        }
        if (target != null && target.isActiveAndEnabled)
        {
            target.ActivatePoison(false);
        }
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
            Unblock();
            isLightAttacking=true;
            moveSpeed = OGMoveSpeed;
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

        audioManager.PlaySFX(audioManager.volchSpit, audioManager.doubleVol);
        TelemetryManager.Instance?.LogAction(PlayerId,"Projectile");
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(transform.localScale.x * bulletSpeed, 0); // Shoots in the direction the character is facing

        bulletScript = bullet.GetComponent<BulletScript>();
        bulletScript.Init(this);

        Destroy(bullet, 2f);
    }

    public void firstShootFrame()
    {
        isShootin = true;
    }

    IEnumerator ResetShooting()
    {

        yield return new WaitForSeconds(resetBullet);
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

    public override void DealChargeDmg()
    {
        TelemetryManager.Instance?.LogAction(PlayerId, "ChargeRelease");
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);
        Character target = ResolveTargetFromHit(hitEnemy);

        if (target != null)
        {
            enemy.StopPunching();
            if (!enemy.counterIsOn)
            {
                enemy.BreakCharge();
            }
            TelemetryManager.Instance?.LogHitAttempt(PlayerId, enemy.PlayerId, MoveType.Charge);
            enemy.SetIncomingDamageContext(PlayerId, MoveType.Charge, SourceType.Melee);
            enemy.TakeDamage(chargeDmg, false);
            enemy.Knockback(13f, 0.4f, false);
            ToxicTouch();
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
                TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Charge);
                audioManager.PlaySFX(audioManager.swoosh, audioManager.swooshVolume);
            }

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
            StartCoroutine(Poison(enemy, passiveDamage/4,1f,4));
            poisonCounter = 0;
            return;
        }

        AddPoison(enemy);
    }

    public void AddPoison(Character target)
    {
        if (target == null)
        {
            return;
        }

        poisonTarget = target;

        if (!target.IsPoisoned())
        {
            if(poisonCounter < 3)
            {
                if(poisonCounter==0)
                {
                    target.StackPoison1(true);
                }
                if (poisonCounter == 1)
                {
                    target.StackPoison1(false);
                    target.StackPoison2(true);
                }
                if (poisonCounter == 2)
                {
                    target.StackPoison2(false);
                    target.StackPoison3(true);
                }
                poisonCounter++;
            }
        }
        // Restart the poison reset coroutine
        if (poisonResetCoroutine != null)
        {
            StopCoroutine(poisonResetCoroutine);
        }
        poisonResetCoroutine = StartCoroutine(ResetPoisonAfterDelay(target));
    }

    private IEnumerator ResetPoisonAfterDelay(Character target)
    {
        yield return new WaitForSeconds(10f); // Wait for 5 seconds
        ResetPoisonStacks(target);
        poisonCounter = 0; // Reset the poison counter
    }

    private void ResetPoisonStacks(Character target)
    {
        if (target == null)
        {
            return;
        }

        target.StackPoison1(false);
        target.StackPoison2(false);
        target.StackPoison3(false);
    }

    public void BeamHit()
    {
        BeamHitEnemy();
    }

    public void StackPoison()
    {

        AddPoison(enemy);

    }
    #endregion
}
