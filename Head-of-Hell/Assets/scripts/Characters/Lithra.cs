using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Lithra : Character
{
    public float airSpinSpeed = 5f; // Speed of the air spin
    public float jumpBackForce = 5f; // Force for the jump back
    public float lightAttackDuration = 0.5f; // Duration of the air spin
    public float jumpHeight = 5f; // Initial jump height
    private bool airSpinready = true;
    float cooldown = 10f;
    int bellDamage = 10;
    public Transform bellPoint;
    public Transform bellStunPointTransf;

    public override void Start()
    {
        base.Start();

        bellPoint=resources.bellPoint;
        bellStunPointTransf = resources.bellStunPoint;

        blockSound = audioManager.bellPunch;
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
            audioManager.PlaySFX(audioManager.bellPunch, 1.8f);
            audioManager.PlaySFX(audioManager.lightattack, 0.5f);
            enemy.TakeDamage(heavyDamage, true);
            LuckyBell();
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
        animator.SetTrigger("Spell");
        UsingAbility(cooldown);
        ignoreDamage = true;
        audioManager.PlaySFX(audioManager.swoosh, audioManager.doubleVol);
    }

    public void DealBellDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle( bellPoint.position,  attackRange*2,  enemyLayer);
        Collider2D bellStunPoint = Physics2D.OverlapCircle(bellStunPointTransf.position,  attackRange / 3,  enemyLayer);

        if (hitEnemy != null || bellStunPoint!=null)
        {
            enemy.StopPunching();
            enemy.BreakCharge();
            enemy.TakeDamage(bellDamage, true);
            
            if (bellStunPoint != null)
            {
                enemy.Stun(0.8f);
            }
            audioManager.PlaySFX(audioManager.heavyattack, audioManager.heavyAttackVolume);
            audioManager.PlaySFX(audioManager.bellSpell, 1.5f);
        }
        else
        {
            audioManager.PlaySFX(audioManager.bellPunch, audioManager.swooshVolume);
        }

         attackRange =  ogRange;

         OnCooldown(cooldown);
        ignoreDamage = false;

    }
    #endregion

    #region LightAttack
    public override void LightAttack()
    {
        // Check if air spin is ready and the player is moving either left or right (controller or keyboard)
        if (airSpinready && (Input.GetKey(left) || Input.GetKey(right) || (controller && Input.GetAxis("Horizontal" + playerString) != 0)))
        {
            QuickAttackIndicatorDisable();
            StartCoroutine(PerformLightAttack());
        }
    }


    private IEnumerator PerformLightAttack()
    {
        airSpinready = false;
        IgnoreUpdate(true);

        audioManager.PlaySFX(audioManager.bellDash, 1f);

        // Calculate the movement direction (keyboard or controller)
        float moveDirection = Input.GetKey(left) ? -1f : (Input.GetKey(right) ? 1f : Input.GetAxis("Horizontal" + playerString));

        // Only proceed if a direction is given (controller or keyboard)
        if (moveDirection == 0f)
        {
            IgnoreUpdate(false);
            airSpinready = true;
            yield break;
        }

        // Grounded vs. air logic for keyboard/controller
        if (!Input.GetKey(up) && isGrounded && (controller && Input.GetAxis("Vertical" + playerString) <= 0.5f))
        {
            rb.AddForce(new Vector2(moveDirection * airSpinSpeed, jumpHeight), ForceMode2D.Impulse);
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(new Vector2(moveDirection * airSpinSpeed, jumpHeight), ForceMode2D.Impulse);
        }

        animator.SetTrigger("QuickAttack");

        // Duration of the attack
        float elapsedTime = 0f;

        while (elapsedTime < lightAttackDuration)
        {
            // Perform the air spin movement
            // transform.Translate(Vector2.right * moveDirection * airSpinSpeed * Time.deltaTime);

            // Check for enemy collision
            Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);
            if (hitEnemy != null)
            {
                audioManager.PlaySFX(audioManager.bellDashHit, 1f);
                enemy.TakeDamage(3, true);
                enemy.Stun(0.5f);

                // Calculate move direction again after hitting (keyboard or controller)
                float moveDirection2 = Input.GetKey(left) ? -1f : (Input.GetKey(right) ? 1f : Input.GetAxis("Horizontal" + playerString));

                // Jump back after hitting the enemy
                rb.velocity = Vector2.zero; // Reset current velocity
                if (moveDirection2 == moveDirection)
                {
                    rb.AddForce(new Vector2(-moveDirection2 * airSpinSpeed / 2, jumpHeight), ForceMode2D.Impulse);
                }
                else
                {
                    rb.AddForce(new Vector2(moveDirection2 * airSpinSpeed / 2, jumpHeight), ForceMode2D.Impulse);
                }

                animator.SetTrigger("Reverse");
                break; // Exit the loop after hitting an enemy
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Wait until the player is grounded
        while (!isGrounded)
        {
            yield return null;
        }

        IgnoreUpdate(false);
        StartCoroutine(ResetAirSpin());
    }


    IEnumerator ResetAirSpin()
    {
        yield return new WaitForSeconds(3f);
        audioManager.PlaySFX(audioManager.sworDashTada, audioManager.lessVol);
        airSpinready = true;
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
        Collider2D hitEnemy = Physics2D.OverlapCircle( attackPoint.position,  attackRange,  enemyLayer);

        if (hitEnemy != null)
        {
            enemy.StopPunching();
            enemy.BreakCharge();
            enemy.TakeDamage(chargeDmg, false);
            LuckyBell();
            enemy.Knockback(13f, 0.4f, false);
            audioManager.PlaySFX(audioManager.smash, audioManager.doubleVol);
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

    void LuckyBell()
    {
        if (StunChance())
        {
            enemy.Stun(0.8f);
        }
    }
    bool StunChance()
    {
        return Random.value < 0.20f;
    }
    #endregion

    #region Extra
    public void Sip()
    {
        audioManager.PlaySFX(audioManager.sip, 2f);
    }

    #endregion
}
