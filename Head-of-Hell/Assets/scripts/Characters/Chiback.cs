using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class Chiback : Character
{
    float cooldown = 10f;
    float jumpHeight = 5f;
    float jumpSpeed = 10f;
    float jumpDuration = 1f;
    bool fireReady = true;
    int timesHit = 0;
    int enragingNum = 4;
    int shortJumpDamage = 5, MedJumpDamage = 10, wideJumpDamage = 15;
    bool roarPlayed = false;

    public Transform mirrorFireAttackPoint;
    public Transform fireAttackPoint;

    public override void Start()
    {
        base.Start();

        mirrorFireAttackPoint=resources.mirrorFireAttackPoint;
        fireAttackPoint=resources.fireAttackPoint;
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
            audioManager.PlaySFX(audioManager.katanaHit, 1.8f);
            enemy.TakeDamage(heavyDamage,true);

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
        audioManager.PlaySFX(audioManager.sytheDash, audioManager.normalVol);
        ignoreDamage = true;
        StartCoroutine(ScytheJump());
    }

    private IEnumerator ScytheJump()
    {
         IgnoreUpdate(true);

        // Calculate the movement direction
        float moveDirection = Input.GetKey( left) ? -1f : (Input.GetKey( right) ? 1f : 0f);

        // Only proceed if a direction is given
        if (moveDirection == 0f)
        {
             IgnoreUpdate(false);
             OnCooldown(cooldown);
            yield break;
        }

        if (!Input.GetKey(KeyCode.W) &&  isGrounded)
        {
             rb.AddForce(new Vector2(moveDirection * jumpSpeed, jumpHeight), ForceMode2D.Impulse);
        }
        else
        {
             rb.velocity = new Vector2( rb.velocity.x, 0);
             rb.AddForce(new Vector2(moveDirection * jumpSpeed, jumpHeight), ForceMode2D.Impulse);
        }

        // Duration of the attack
        float elapsedTime = 0f;

        while (elapsedTime < jumpDuration)
        {
            // Perform the air spin movement
            // transform.Translate(Vector2.right * moveDirection * airSpinSpeed * Time.deltaTime);

            // Check for enemy collision
            Collider2D hitEnemy = Physics2D.OverlapCircle( attackPoint.position,  attackRange,  enemyLayer);
            if (hitEnemy != null)
            {
                enemy.BreakCharge();
                animator.SetTrigger("SpellHit");
                audioManager.PlaySFX(audioManager.sytheHit, 1f);
                audioManager.PlaySFX(audioManager.sytheSlash, 1f);
                if(elapsedTime < 0.33f) {
                    enemy.TakeDamage(shortJumpDamage,true);
                    Enraged(shortJumpDamage);
                }
                if (0.33 <= elapsedTime && elapsedTime < 0.66)
                {
                    enemy.TakeDamage(MedJumpDamage,true);
                    Enraged(MedJumpDamage);
                }
                if (0.66 <= elapsedTime)
                {
                    enemy.TakeDamage(wideJumpDamage, true);
                    Enraged(wideJumpDamage);
                }

                if(timesHit>=enragingNum)
                {
                    timesHit = 0;
                }

                enemy.Knockback(11f,0.25f,false);
               

                // Jump back after hitting the enemy
                 rb.velocity = Vector2.zero; // Reset current velocity
                
                break; // Exit the loop after hitting an enemy
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
         IgnoreUpdate(false);
         OnCooldown(cooldown);
        ignoreDamage = false;
    }
    #endregion

    #region LightAttack
    public override void LightAttack()
    {
        if(fireReady)
        {
            QuickAttackIndicatorDisable();
            animator.SetTrigger("QuickAttack");
            fireReady = false;
            audioManager.PlaySFX(audioManager.fireblast, 1f);
            StartCoroutine(ResetFire());
        }
    }

    public void DealFireDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle( mirrorFireAttackPoint.position,  attackRange,  enemyLayer);
        Collider2D hitEnemy2 = Physics2D.OverlapCircle( fireAttackPoint.position,  attackRange,  enemyLayer);

        if (hitEnemy != null || hitEnemy2!=null)
        {
            audioManager.PlaySFX(audioManager.lightattack, 0.5f);
            enemy.TakeDamage(5, true);

            if (! enemy.isBlocking)
            {
                enemy.Knockback(15f, 0.8f, true);
                enemy.DisableBlock(true);
                StartCoroutine(ResetBlockability());
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
        QuickAttackIndicatorEnable();
    }

    private IEnumerator ResetBlockability()
    {

        // Wait for 1.1 seconds before enabling the block
        yield return new WaitForSeconds(jumpDuration+0.1f);

        enemy.EnableBlock();
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
    override public void TakeDamage(int dmg, bool blockable)
    {
        if (timesHit < enragingNum && !isBlocking)
        {
            timesHit++;
        }

        if(timesHit == enragingNum && !roarPlayed)
        {
            audioManager.PlaySFX(audioManager.growl, 0.5f);
            roarPlayed = true;
        }
        base.TakeDamage(dmg, blockable);
    }

    void Enraged(int jumpDamage)
    {
        if (timesHit == enragingNum)
        {
            enemy.TakeDamage(jumpDamage / 2,true);
            roarPlayed = false;
        }
    }
    #endregion
}
