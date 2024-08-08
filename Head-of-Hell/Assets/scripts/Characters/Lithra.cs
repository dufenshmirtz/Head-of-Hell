using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Lithra : Character
{
    public float airSpinSpeed = 5f; // Speed of the air spin
    public float jumpBackForce = 5f; // Force for the jump back
    public float lightAttackDuration = 0.5f; // Duration of the air spin
    public float jumpHeight = 5f; // Initial jump height
    private bool airSpinready = true;
    float cooldown = 10f;
    int bellDamage = 5;


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
            enemy.TakeDamage(21);

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
        audioManager.PlaySFX(audioManager.swoosh, audioManager.doubleVol);
    }

    public void DealBellDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.bellPoint.position, player.attackRange*2, player.enemyLayer);

        if (hitEnemy != null)
        {
            enemy.StopPunching();
            enemy.BreakCharge();
            enemy.TakeDamage(bellDamage);
            Collider2D bellStunPoint = Physics2D.OverlapCircle(player.bellStunPoint.position, player.attackRange/3, player.enemyLayer);
            if (bellStunPoint != null)
            {
                StartCoroutine(enemy.Stun(0.8f));
            }
            audioManager.PlaySFX(audioManager.stabHit, audioManager.doubleVol);
            audioManager.PlaySFX(audioManager.bellPunch, audioManager.doubleVol);
        }
        else
        {
            audioManager.PlaySFX(audioManager.bellPunch, audioManager.swooshVolume);
        }

        player.attackRange = player.ogRange;

        player.OnCooldown(cooldown);

    }
    #endregion

    #region LightAttack
    public override void LightAttack()
    {
        if (airSpinready && (Input.GetKey(player.left) || Input.GetKey(player.right)))
        {
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
                enemy.TakeDamage(3);
                StartCoroutine(enemy.InterruptMovement(0.5f));
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
    }
    #endregion

    #region ChargeAttack
    public override void ChargeAttack()
    {
        base.ChargeAttack();
        animator.SetTrigger("bellCharge");
    }
    #endregion
}
