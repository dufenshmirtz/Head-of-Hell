using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
//using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class Chiback : Character
{
    float cooldown = 10f;
    float jumpHeight = 5f;
    float jumpSpeed = 10f;
    float jumpDuration = 1f;
    bool fireReady = true;
    int timesHit = 0;
    int enragingNum = 3;
    int shortJumpDamage = 5, MedJumpDamage = 10, wideJumpDamage = 15;
    bool roarPlayed = false;
    float resetDisable = 1f;

    float spellTime = 0.75f;

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
        TelemetryManager.Instance?.LogAction(PlayerId, "Heavy");
        animator.SetTrigger("HeavyAttack");
        audioManager.PlaySFX(audioManager.heavyswoosh, audioManager.heavySwooshVolume);
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle( attackPoint.position,  attackRange,  enemyLayer);
        Character target = ResolveTargetFromHit(hitEnemy);

        if (target != null)
        {
            audioManager.PlaySFX(audioManager.katanaHit, 1.8f);
            TelemetryManager.Instance?.LogHitAttempt(PlayerId, enemy.PlayerId, MoveType.Heavy);
            enemy.SetIncomingDamageContext(PlayerId, MoveType.Heavy, SourceType.Melee);
            enemy.TakeDamage(heavyDamage, true);

            if (! enemy.isBlocking)
            {
                enemy.Knockback(11f, 0.15f, true);
            }
        }
        else
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Heavy);
            audioManager.PlaySFX(audioManager.swoosh, 1f);
        }
    }
    #endregion

    #region Spell
    override public void Spell()
    {
        TelemetryManager.Instance?.LogAction(PlayerId, "Special");
        animator.SetTrigger("Spell");
        UsingAbility(cooldown);
        audioManager.PlaySFX(audioManager.sytheDash, audioManager.normalVol);
        audioManager.PlaySFX(audioManager.yeehaw, 3.5f);
        ignoreDamage = true;
        StartCoroutine(ScytheJump());
        StartCoroutine(SpellSafety(spellTime,cooldown));
    }

    private IEnumerator ScytheJump()
    {
        IgnoreUpdate(true);

        // Telemetry: track whether the special actually landed (for Miss logging)
        bool landed = false;

        // Calculate the movement direction (keyboard always, controller only if controller == true)
        float moveDirection = input.GetKey(left) ? -1f : (input.GetKey(right) ? 1f : (controller ? input.GetAxis("Horizontal" + playerString) : 0f));

        // Only proceed if a direction is given
        if (moveDirection == 0f)
        {
            IgnoreUpdate(false);
            OnCooldown(cooldown);
            yield break;
        }

        // Grounded vs air logic for keyboard/controller (keyboard always works, controller only when controller == true)
        if (!input.GetKey(KeyCode.W) && isGrounded || (controller && input.GetAxis("Vertical" + playerString) <= 0.5f && isGrounded))
        {
            rb.AddForce(new Vector2(moveDirection * jumpSpeed, jumpHeight), ForceMode2D.Impulse);
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(new Vector2(moveDirection * jumpSpeed, jumpHeight), ForceMode2D.Impulse);
        }

        // Duration of the attack
        float elapsedTime = 0f;

        while (elapsedTime < jumpDuration)
        {
            // Check for enemy collision
            Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);
            Character target = ResolveTargetFromHit(hitEnemy);
            if (target != null)
            {
                enemy.BreakCharge();
                animator.SetTrigger("SpellHit");
                audioManager.PlaySFX(audioManager.sytheHit, 1f);
                audioManager.PlaySFX(audioManager.sytheSlash, 1f);

                // Telemetry: HitAttempt + context (Special / Spell)
                TelemetryManager.Instance?.LogHitAttempt(PlayerId, enemy.PlayerId, MoveType.Special);
                enemy.SetIncomingDamageContext(PlayerId, MoveType.Special, SourceType.Spell);
                landed = true;

                // Apply damage based on elapsed time during the jump
                if (elapsedTime < 0.33f)
                {
                    // Telemetry: ensure context is set right before TakeDamage
                    enemy.SetIncomingDamageContext(PlayerId, MoveType.Special, SourceType.Spell);
                    enemy.TakeDamage(shortJumpDamage, true);
                    Enraged(enemy, shortJumpDamage);
                }
                else if (elapsedTime < 0.66f)
                {
                    enemy.SetIncomingDamageContext(PlayerId, MoveType.Special, SourceType.Spell);
                    enemy.TakeDamage(MedJumpDamage, true);
                    Enraged(enemy, MedJumpDamage);
                }
                else
                {
                    enemy.SetIncomingDamageContext(PlayerId, MoveType.Special, SourceType.Spell);
                    enemy.TakeDamage(wideJumpDamage, true);
                    Enraged(enemy, wideJumpDamage);
                }

                // Reset enraging hits if the threshold is reached
                if (timesHit >= enragingNum)
                {
                    timesHit = 0;
                }

                // Apply knockback to the enemy
                enemy.Knockback(11f, 0.25f, false);

                // Reset velocity after hitting the enemy
                rb.velocity = Vector2.zero;
                break; // Exit the loop after hitting an enemy
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Telemetry: if the special ended without landing, log Miss
        if (!landed)
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Special);
        }

        IgnoreUpdate(false);
        OnCooldown(cooldown);
        ignoreDamage = false;
    }
    #endregion

    #region LightAttack
    public override void LightAttack()
    {
        TelemetryManager.Instance?.LogAction(PlayerId, "Quick");
        if (fireReady)
        {
            QuickAttackIndicatorDisable();
            animator.SetTrigger("QuickAttack");
            fireReady = false;
            audioManager.PlaySFX(audioManager.sytheGround, 1f);
            StartCoroutine(ResetFire());
        }
    }

    public void DealFireDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle( mirrorFireAttackPoint.position,  attackRange,  enemyLayer);
        Collider2D hitEnemy2 = Physics2D.OverlapCircle( fireAttackPoint.position,  attackRange,  enemyLayer);
        Character target = ResolveTargetFromHit(hitEnemy, hitEnemy2);

        if (target != null)
        {
            audioManager.PlaySFX(audioManager.skiplaHeavyHit, 1f);
            TelemetryManager.Instance?.LogHitAttempt(PlayerId, enemy.PlayerId, MoveType.Quick);
            enemy.SetIncomingDamageContext(PlayerId, MoveType.Quick, SourceType.Melee);
            enemy.TakeDamage(5, true);
            if(!onCooldown){
                enemy.Knockback(15f, 0.8f, true);
            }
            else
            {
                enemy.Knockback(3f, 0.3f, true);
            }
            enemy.DisableBlock(true);
            enemy.DisableJump(true);
            StartCoroutine(ResetBlockability(enemy));
            
        }
        else
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Quick);
            audioManager.PlaySFX(audioManager.swoosh, 1f);
        }
    }

    IEnumerator ResetFire()
    {
        yield return new WaitForSeconds(resetDisable);
        audioManager.PlaySFX(audioManager.sworDashTada, audioManager.lessVol);
        fireReady = true;
        QuickAttackIndicatorEnable();
    }

    private IEnumerator ResetBlockability(Character target)
    {
        if (target == null)
        {
            yield break;
        }

        // Wait for 1.1 seconds before enabling the block
        yield return new WaitForSeconds(jumpDuration+0.1f);

        if (target == null || !target.isActiveAndEnabled)
        {
            yield break;
        }

        target.EnableBlock();
        target.DisableJump(false);
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
    override public void TakeDamage(int dmg, bool blockable, bool parryable=true, bool canCrit = true)
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

    void Enraged(Character target, int jumpDamage)
    {
        if (timesHit == enragingNum && target != null)
        {
            target.SetIncomingDamageContext(PlayerId, MoveType.Special, SourceType.Spell);
            target.TakeDamageNoAnimation(jumpDamage / 2,true);
            roarPlayed = false;
        }
    }
    #endregion
}
