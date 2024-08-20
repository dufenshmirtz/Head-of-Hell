using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Chiback : Character
{
    float cooldown = 10f;
    float jumpHeight = 5f;
    float jumpSpeed = 10f;
    float jumpDuration = 1f;
    bool fireReady = true;


    #region HeavyAttack
    override public void HeavyAttack()
    {
        animator.SetTrigger("AwsomeHeavy");
        audioManager.PlaySFX(audioManager.heavyswoosh, audioManager.heavySwooshVolume);
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);

        if (hitEnemy != null)
        {
            audioManager.PlaySFX(audioManager.katanaHit, 1.8f);
            enemy.TakeDamage(heavyDamage);

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
        animator.SetTrigger("AwsomeSpell");
        player.UsingAbility(cooldown);
        audioManager.PlaySFX(audioManager.sytheDash, audioManager.normalVol);
        StartCoroutine(ScytheJump());
    }

    private IEnumerator ScytheJump()
    {
        player.IgnoreUpdate(true);

        // Calculate the movement direction
        float moveDirection = Input.GetKey(player.left) ? -1f : (Input.GetKey(player.right) ? 1f : 0f);

        // Only proceed if a direction is given
        if (moveDirection == 0f)
        {
            player.IgnoreUpdate(false);
            player.OnCooldown(cooldown);
            yield break;
        }

        if (!Input.GetKey(KeyCode.W) && player.isGrounded)
        {
            player.rb.AddForce(new Vector2(moveDirection * jumpSpeed, jumpHeight), ForceMode2D.Impulse);
        }
        else
        {
            player.rb.velocity = new Vector2(player.rb.velocity.x, 0);
            player.rb.AddForce(new Vector2(moveDirection * jumpSpeed, jumpHeight), ForceMode2D.Impulse);
        }

        // Duration of the attack
        float elapsedTime = 0f;

        while (elapsedTime < jumpDuration)
        {
            // Perform the air spin movement
            //player.transform.Translate(Vector2.right * moveDirection * airSpinSpeed * Time.deltaTime);

            // Check for enemy collision
            Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);
            if (hitEnemy != null)
            {
                enemy.BreakCharge();
                animator.SetTrigger("SytheHit");
                audioManager.PlaySFX(audioManager.sytheHit, 1f);
                audioManager.PlaySFX(audioManager.sytheSlash, 1f);
                if(elapsedTime < 0.33f) {
                    enemy.TakeDamage(5);
                    print("5 demege $$");
                }
                if (0.33 <= elapsedTime && elapsedTime < 0.66)
                {
                    enemy.TakeDamage(10);
                    print("10 demege $$");
                }
                if (0.66 <= elapsedTime)
                {
                    enemy.TakeDamage(15);
                    print("15 demege $$");
                }

                enemy.Knockback(11f,0.25f,false);
               

                // Jump back after hitting the enemy
                player.rb.velocity = Vector2.zero; // Reset current velocity
                
                break; // Exit the loop after hitting an enemy
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        player.IgnoreUpdate(false);
        player.OnCooldown(cooldown);
    }
    #endregion

    #region LightAttack
    public override void LightAttack()
    {
        if(fireReady)
        {
            animator.SetTrigger("Fire");
            fireReady = false;
            audioManager.PlaySFX(audioManager.fireblast, 1f);
            StartCoroutine(ResetFire());
        }
    }

    public void DealFireDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.mirrorFireAttackPoint.position, player.attackRange, player.enemyLayer);
        Collider2D hitEnemy2 = Physics2D.OverlapCircle(player.fireAttackPoint.position, player.attackRange, player.enemyLayer);

        if (hitEnemy != null || hitEnemy2!=null)
        {
            audioManager.PlaySFX(audioManager.lightattack, 0.5f);
            enemy.TakeDamage(5);

            if (!player.enemy.isBlocking)
            {
                enemy.Knockback(15f, 0.5f, true);
            }
        }
        else
        {
            audioManager.PlaySFX(audioManager.swoosh, 1f);
        }
    }

    IEnumerator ResetFire()
    {
        yield return new WaitForSeconds(3f);
        audioManager.PlaySFX(audioManager.sworDashTada, audioManager.lessVol);
        fireReady = true;
    }

    #endregion

    #region ChargeAttack
    public override void ChargeAttack()
    {
        base.ChargeAttack();
        animator.SetTrigger("AwsomeCharge");
    }
    #endregion
}
