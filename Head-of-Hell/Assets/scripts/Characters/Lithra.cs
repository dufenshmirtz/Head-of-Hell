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


    #region HeavyAttack
    override public void HeavyAttack()
    {
        animator.SetTrigger("bellPunch");
        audioManager.PlaySFX(audioManager.heavyswoosh, audioManager.heavySwooshVolume);
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);

        if (hitEnemy != null)
        {
            audioManager.PlaySFX(audioManager.bellPunch, 1.8f);
            audioManager.PlaySFX(audioManager.lightattack, 0.5f);
            enemy.TakeDamage(heavyDamage, true);
            LuckyBell();
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
        animator.SetTrigger("BellSpell");
        player.UsingAbility(cooldown);
        ignoreDamage = true;
        audioManager.PlaySFX(audioManager.swoosh, audioManager.doubleVol);
    }

    public void DealBellDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.bellPoint.position, player.attackRange*2, player.enemyLayer);

        if (hitEnemy != null)
        {
            enemy.StopPunching();
            enemy.BreakCharge();
            enemy.TakeDamage(bellDamage, true);
            Collider2D bellStunPoint = Physics2D.OverlapCircle(player.bellStunPoint.position, player.attackRange/3, player.enemyLayer);
            if (bellStunPoint != null)
            {
                StartCoroutine(enemy.Stun(0.8f));
            }
            audioManager.PlaySFX(audioManager.heavyattack, audioManager.heavyAttackVolume);
            audioManager.PlaySFX(audioManager.bellPunch, audioManager.doubleVol);
        }
        else
        {
            audioManager.PlaySFX(audioManager.bellPunch, audioManager.swooshVolume);
        }

        player.attackRange = player.ogRange;

        player.OnCooldown(cooldown);
        ignoreDamage = false;

    }
    #endregion

    #region LightAttack
    public override void LightAttack()
    {
        if (airSpinready && (Input.GetKey(player.left) || Input.GetKey(player.right)))
        {
            QuickAttackIndicatorDisable();
            StartCoroutine(PerformLightAttack());
        }
    }

    private IEnumerator PerformLightAttack()
    {
        airSpinready=false;
        player.IgnoreUpdate(true);

        audioManager.PlaySFX(audioManager.bellDash, 1f);
        // Calculate the movement direction
        float moveDirection = Input.GetKey(player.left) ? -1f : (Input.GetKey(player.right) ? 1f : 0f);

        // Only proceed if a direction is given
        if (moveDirection == 0f)
        {
            player.IgnoreUpdate(false);
            airSpinready=true;
            yield break;
        }

        if (!Input.GetKey(KeyCode.W) && player.isGrounded)
        {
            player.rb.AddForce(new Vector2(moveDirection * airSpinSpeed , jumpHeight), ForceMode2D.Impulse);
        }
        else
        {   
            player.rb.velocity = new Vector2(player.rb.velocity.x, 0);
            player.rb.AddForce(new Vector2(moveDirection * airSpinSpeed , jumpHeight), ForceMode2D.Impulse);
        }

        animator.SetTrigger("AirSpin");

        // Duration of the attack
        float elapsedTime = 0f;

        while (elapsedTime < lightAttackDuration)
        {
            // Perform the air spin movement
            //player.transform.Translate(Vector2.right * moveDirection * airSpinSpeed * Time.deltaTime);

            // Check for enemy collision
            Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);
            if (hitEnemy != null)
            {
                audioManager.PlaySFX(audioManager.bellDashHit, 1f);
                enemy.TakeDamage(3,true);
                StartCoroutine(enemy.Stun(0.5f));
                float moveDirection2 = Input.GetKey(player.left) ? -1f : (Input.GetKey(player.right) ? 1f : 0f);

                // Jump back after hitting the enemy
                player.rb.velocity = Vector2.zero; // Reset current velocity
                if(moveDirection2 == moveDirection) {
                    player.rb.AddForce(new Vector2(-moveDirection2 * airSpinSpeed/2, jumpHeight), ForceMode2D.Impulse);
                }
                else
                {
                    player.rb.AddForce(new Vector2(moveDirection2 * airSpinSpeed/2, jumpHeight), ForceMode2D.Impulse);
                }
                
                animator.SetTrigger("reverseTime");
                break; // Exit the loop after hitting an enemy
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // Wait until the player is grounded
        while (!player.isGrounded)
        {
            yield return null;
        }
        player.IgnoreUpdate(false);
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
        animator.SetTrigger("bellCharge");
    }

    public override void DealChargeDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);

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
        player.knockable = true;
        charging = false;
        animator.SetBool("Casting", false);
        animator.SetBool("Charging", false);
        player.stayDynamic();
    }
    #endregion

    #region Passive

    void LuckyBell()
    {
        if (StunChance())
        {
            StartCoroutine(enemy.Stun(0.8f));
        }
    }
    bool StunChance()
    {
        return Random.value < 0.20f;
    }
    #endregion
}
