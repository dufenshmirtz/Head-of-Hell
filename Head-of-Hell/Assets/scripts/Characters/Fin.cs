using System.Collections;
using UnityEngine;

public class Fin : Character
{
    float missCooldown = 10f, cooldown = 15f;
    bool ignoreCounterOff = false;
    int damage = 25;
    bool counterIsOn = false;
    bool counterDone = false;
    //Roll
    float rollPower = 8f;
    float rollTime = 0.39f;
    bool rollReady = true;

    #region HeavyAttack
    override public void HeavyAttack()
    {
        animator.SetTrigger("Punch");
        audioManager.PlaySFX(audioManager.heavyswoosh, audioManager.heavySwooshVolume);
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);

        if (hitEnemy != null)
        {

            audioManager.PlaySFX(audioManager.heavyattack, 1f);
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
    public override void Spell()
    {
        audioManager.PlaySFX(audioManager.counterScream, audioManager.counterVol);
        animator.SetTrigger("counter");
        counterIsOn = true;
        player.UsingAbility(cooldown);
    }

    public bool DetectCounter()
    {
        if (counterIsOn)
        {
            if (!counterDone)
            {
                ignoreCounterOff = true;
                Countered();
                counterDone = true;
                return true;
            }
            return true;
        }

        return false;
    }

    public void CounterOff()
    {
        if (!ignoreCounterOff)
        {
            // Start the cooldown timer
            player.OnCooldown(missCooldown);
            CounterVariablesOff();
        }
        else
        {
            ignoreCounterOff = false;
        }
        
    }

    public void CounterSuccessOff()
    {
        CounterVariablesOff();
        player.OnCooldown(cooldown);
    }

    public void Countered()
    {
        audioManager.PlaySFX(audioManager.counterSucces, audioManager.doubleVol);
        enemy.stayStatic();
        player.stayStatic();
        animator.SetTrigger("counterHit");
    }

    public void DealCounterDmg()
    {
        enemy.StopPunching();
        enemy.BreakCharge();

        audioManager.PlaySFX(audioManager.counterClong, audioManager.doubleVol);

        enemy.TakeDamage(damage);

        player.stayDynamic();
        enemy.stayDynamic();

        enemy.Knockback(10f, .3f, false);

    }

    public void CounterVariablesOff()
    {
        counterDone = false;
        counterIsOn = false;
    }

    override public void TakeDamage(int dmg)
    {
        if(DetectCounter()){
            return;
        }

        base.TakeDamage(dmg);
    }

    #endregion

    #region LightAttack
    override public void LightAttack() 
    {
        if (rollReady)
        {
            QuickAttackIndicatorDisable();
            rollReady = false;
            StartCoroutine(Roll());
        }
    }

    IEnumerator Roll()
    {
        player.IgnoreMovement(true);
        ignoreDamage=true;
        player.knockable = false;

        // Store the original gravity scale
        float ogGravityScale = rb.gravityScale;

        // Disable gravity while dashing
        rb.gravityScale = 0f;

        // Store the current velocity
        Vector2 currentVelocity = rb.velocity;

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
        colliders[3].enabled = true;
        colliders[4].enabled = true;

        // Determine the dash direction based on the input
        float moveDirection = Input.GetKey(player.left) ? -1f : 1f; ;


        audioManager.PlaySFX(audioManager.roll, 1);
        // Calculate the dash velocity
        Vector2 rollVelocity = new Vector2(moveDirection * rollPower, currentVelocity.y);

        // Apply the dash velocity
        rb.velocity = rollVelocity;

        // Trigger the dash animation
        animator.SetTrigger("roll");



        // Wait for the dash duration
        yield return new WaitForSeconds(rollTime);

        // Reset the velocity after the dash
        rb.velocity = currentVelocity;

        foreach (Collider2D collider in colliders)
        {
            if (collider != colliders[3])
            {
                collider.enabled = true;
            }
        }
        colliders[3].enabled = false;
        colliders[4].enabled = false;



        // Reset the gravity scale
        rb.gravityScale = ogGravityScale;


        player.IgnoreMovement(false);
        ignoreDamage=false;

        player.knockable = true;

        StartCoroutine(ResetRoll());


    }

    IEnumerator ResetRoll()
    {

        yield return new WaitForSeconds(2f);
        audioManager.PlaySFX(audioManager.rollReady, audioManager.lessVol);
        rollReady = true;
        QuickAttackIndicatorEnable();
    }
    #endregion

    #region ChargeAttack
    public override void ChargeAttack()
    {
        base.ChargeAttack();
        animator.SetTrigger("FinCharge");
    }
    #endregion
}
