using System.Collections;
using UnityEngine;

public class Lithra : Character
{
    public float airSpinSpeed = 5f; // Speed of the air spin
    public float jumpBackForce = 5f; // Force for the jump back
    public float lightAttackDuration = 0.5f; // Duration of the air spin
    public float jumpHeight = 5f; // Initial jump height
    private bool isAttacking = false;

    #region HeavyAttack
    override public void HeavyAttack()
    {

    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);

        if (hitEnemy != null)
        {
            audioManager.PlaySFX(audioManager.katanaHit, 1f);
            enemy.TakeDamage(21);

            if (!player.enemy.isBlocking)
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

    }
    #endregion

    #region LightAttack
    public override void LightAttack()
    {
        if (!isAttacking && (Input.GetKey(player.left) || Input.GetKey(player.right)))
        {
            StartCoroutine(PerformLightAttack());
        }
    }

    private IEnumerator PerformLightAttack()
    {
        isAttacking = true;
        player.IgnoreUpdate(true);

        // Calculate the movement direction
        float moveDirection = Input.GetKey(player.left) ? -1f : (Input.GetKey(player.right) ? 1f : 0f);

        // Only proceed if a direction is given
        if (moveDirection == 0f)
        {
            player.IgnoreUpdate(false);
            isAttacking = false;
            yield break;
        }

        if (!Input.GetKey(KeyCode.W) && player.isGrounded)
        {
            player.rb.AddForce(new Vector2(moveDirection * airSpinSpeed / 4, jumpHeight), ForceMode2D.Impulse);
        }
        else
        {   
            player.rb.velocity = new Vector2(player.rb.velocity.x, 0);
            player.rb.AddForce(new Vector2(moveDirection * airSpinSpeed / 4, jumpHeight), ForceMode2D.Impulse);
        }

        animator.SetTrigger("AirSpin");

        // Duration of the attack
        float elapsedTime = 0f;

        while (elapsedTime < lightAttackDuration)
        {
            // Perform the air spin movement
            player.transform.Translate(Vector2.right * moveDirection * airSpinSpeed * Time.deltaTime);

            // Check for enemy collision
            Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);
            if (hitEnemy != null)
            {
                audioManager.PlaySFX(audioManager.katanaHit, 1f);
                enemy.TakeDamage(3);
                StartCoroutine(enemy.InterruptMovement(0.5f));
                float moveDirection2 = Input.GetKey(player.left) ? -1f : (Input.GetKey(player.right) ? 1f : 0f);
                // Jump back after hitting the enemy
                player.rb.velocity = Vector2.zero; // Reset current velocity
                if(moveDirection2 == moveDirection) {
                    player.rb.AddForce(new Vector2(-moveDirection2 * airSpinSpeed, jumpHeight), ForceMode2D.Impulse);
                }
                else
                {
                    player.rb.AddForce(new Vector2(moveDirection2 * airSpinSpeed, jumpHeight), ForceMode2D.Impulse);
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
        isAttacking = false;
    }
    #endregion

    #region ChargeAttack
    public override void ChargeAttack()
    {

    }
    #endregion
}
