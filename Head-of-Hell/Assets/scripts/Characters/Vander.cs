using System.Collections;
using UnityEngine;

public class Vander : Character
{

    int stabDamage = 10, stabHeal = 5;
    float cooldown = 12f;
    //katana
    int katanaDmg = 3;
    int smallLifesteal = 3;
    bool katanaready = true;

    int chargeLifesteal=8;

    float katanaCD=2f;

    // --- Flying passive ---
    bool isFlying = false;
    float flyUpSpeed = 6f;        // how fast he ascends while holding jump

    // How quickly we cancel falling (units: velocity per second)
    [SerializeField] float fallBrakePerSec = 20f;

    // How quickly we ramp upward (units: velocity per second)
    [SerializeField] float ascendAccelPerSec = 18f;

    // Hard cap for upward speed
    [SerializeField] float maxAscendSpeed = 4f;

    // --- Flying passive tuning ---
    [SerializeField] float fallStartEpsilon = 0.02f; // tiny buffer to detect start of fall


    // Keep the gravity to restore on release
    float baseGravity = 1f;


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

            audioManager.PlaySFX(audioManager.katanaHit, 1f);
            enemy.TakeDamage(heavyDamage, true);
            Lifesteal(smallLifesteal);
            if (!enemy.isBlocking)
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
        attackRange += 0.5f;
        animator.SetTrigger("Spell");
        UsingAbility(cooldown);
        ignoreDamage = true;
    }

    public void DealStabDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {
            enemy.StopPunching();
            enemy.BreakCharge();
            enemy.TakeDamage(stabDamage, true);
            Lifesteal(stabHeal);
            healthbar.SetHealth(currHealth);
            audioManager.PlaySFX(audioManager.stabHit, audioManager.doubleVol);
        }
        else
        {
            audioManager.PlaySFX(audioManager.stab, audioManager.swooshVolume);
        }

        attackRange = ogRange;

        OnCooldown(cooldown);
        ignoreDamage = false;

    }
    #endregion

    #region LightAttack
    public override void LightAttack()
    {
        if (katanaready)
        {
            QuickAttackIndicatorDisable();
            animator.SetTrigger("QuickAttack");
            katanaready = false;
            StartCoroutine(ResetKatana());
        }
    }

    public void DealKatanaDmg1()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {
            enemy.TakeDamage(katanaDmg, true);
            audioManager.PlaySFX(audioManager.katanaHit, audioManager.lightAttackVolume);
        }
        else
        {
            audioManager.PlaySFX(audioManager.katanaSwoosh, audioManager.swooshVolume);
        }

    }

    public void DealKatanaDmg2()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {
            enemy.TakeDamage(katanaDmg, true);
            Lifesteal(smallLifesteal);
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

        yield return new WaitForSeconds(katanaCD);
        audioManager.PlaySFX(audioManager.katanaSeath, audioManager.doubleVol);
        katanaready = true;
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

    public override void DealChargeDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {
            enemy.StopPunching();
            enemy.BreakCharge();
            enemy.TakeDamage(chargeDmg, false);
            Lifesteal(chargeLifesteal);
            enemy.Knockback(13f, 0.4f, false);
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
                audioManager.PlaySFX(audioManager.swoosh, audioManager.swooshVolume);
            }

        }
        chargeReset = true;
        knockable = true;
        charging = false;
        animator.SetBool("Casting", false);
        animator.SetBool("Charging", false);
    }

    public override void DealCounterDmg()
    {
        base.DealCounterDmg();

        Lifesteal(smallLifesteal);
    }


    void Lifesteal(int amount)
    {
        currHealth += amount;
        if (currHealth > maxHealth)
        {
            currHealth = maxHealth;
        }
        healthbar.SetHealth(currHealth);
    }

    public override void Update()
    {
        // Run the base Character logic first (movement, jump, etc.)
        base.Update();

        // Then apply the flying passive on top
        HandleFlightPassive();
    }

    void HandleFlightPassive()
    {
        // Don’t interfere if gameplay is locked or grounded
        if (ignoreUpdate || casting || stunned || knocked || charging || isStatic || isGrounded)
        {
            StopFlightIfActive();
            return;
        }

        // Use same "jump" input you already use
        bool holdingJump =
            input.GetKey(up) ||
           (controller && input.GetAxis("Vertical" + playerString) > 0.5f);

        if (!holdingJump)
        {
            StopFlightIfActive();
            return;
        }

        float vy = rb.velocity.y;

        // Only allow flight to ENGAGE once we start falling.
        // This prevents the initial ground jump from helping you reach max speed.
        if (!isFlying)
        {
            if (vy > -fallStartEpsilon)
            {
                // Still rising (or almost zero)? Wait until we’re truly falling.
                return;
            }

            // Engage flight at the *moment of falling* and zero vertical speed.
            isFlying = true;
            rb.gravityScale = 0f;
            animator.SetBool("IsFlying", true); // optional
            vy = 0f; // start from rest for that floaty feel
        }

        // We’re flying: accelerate upward quickly but respect a hard cap.
        vy = Mathf.MoveTowards(vy, maxAscendSpeed, ascendAccelPerSec * Time.deltaTime);

        rb.velocity = new Vector2(rb.velocity.x, vy);
    }

    void StopFlightIfActive()
    {
        if (!isFlying) return;
        isFlying = false;
        rb.gravityScale = originalGravityScale; // from Character.Start()
        animator.SetBool("IsFlying", false); // optional
    }



    #endregion
}
