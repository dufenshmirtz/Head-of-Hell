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
    int passiveDamage = 4;
    bool safety = true;

    #region HeavyAttack
    override public void HeavyAttack()
    {
        animator.SetTrigger("HeavyAttack");
        audioManager.PlaySFX(audioManager.incense, audioManager.doubleVol);
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle( attackPoint.position,  attackRange,  enemyLayer);

        if (hitEnemy != null)
        {

            audioManager.PlaySFX(audioManager.heavyattack, 1f);
            enemy.TakeDamage(heavyDamage, true);
            enemy.TakeDamageNoAnimation(passiveDamage, true);

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
    public override void Spell()
    {
        audioManager.PlaySFX(audioManager.counterScream, audioManager.counterVol);
        animator.SetTrigger("Spell");
        counterIsOn = true;
        safety = true;
        UsingAbility(cooldown);
        StartCoroutine(CounterOffSafety());
    }

    public bool DetectCounter()
    {
        if (counterIsOn)
        {
            if (!counterDone)
            {
                Countered();
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
            OnCooldown(missCooldown);
            CounterVariablesOff();
            safety = false;
        }
        else
        {
            ignoreCounterOff = false;
        }
        
    }

    private IEnumerator CounterOffSafety()
    {
        yield return new WaitForSeconds(0.41f);
        if (!counterDone && safety)
        {
            OnCooldown(missCooldown);
            CounterVariablesOff();
        }
    }

    public void CounterSuccessOff()
    {
        CounterVariablesOff();
        OnCooldown(cooldown);
    }

    public void Countered()
    {
        animator.SetTrigger("counterHit");
        audioManager.PlaySFX(audioManager.counterSucces, audioManager.doubleVol);
        enemy.stayStatic();
        stayStatic();
        ignoreCounterOff = true;
        counterDone = true;
    }

    public void DealCounterDmg()
    {
        enemy.StopPunching();
        enemy.BreakCharge();

        audioManager.PlaySFX(audioManager.counterClong, audioManager.doubleVol);

        enemy.TakeDamage(damage,true);

         stayDynamic();
        enemy.stayDynamic();

        enemy.Knockback(10f, .3f, false);

    }

    public void CounterVariablesOff()
    {
        counterDone = false;
        counterIsOn = false;
    }

    override public void TakeDamage(int dmg, bool blockable)
    {
        if(DetectCounter()){
            return;
        }

        base.TakeDamage(dmg, blockable);
    }

    override public void TakeDamageNoAnimation(int dmg, bool blockable)
    {
        if(DetectCounter()){
            return;
        }

        base.TakeDamageNoAnimation(dmg, blockable);
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
        IgnoreMovement(true);
        ignoreDamage = true;
        knockable = false;

        // Store the original gravity scale
        float ogGravityScale = rb.gravityScale;

        // Disable gravity while rolling
        rb.gravityScale = 0f;

        // Store the current velocity
        Vector2 currentVelocity = rb.velocity;

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
        colliders[5].enabled = true;


        // Determine the roll direction based on the input (keyboard always, controller only if controller == true)
        float moveDirection = Input.GetKey(left) ? -1f : (Input.GetKey(right) ? 1f : (controller ? Input.GetAxis("Horizontal" + playerString) : 0f));

        // If no direction input was given, default to right
        if (moveDirection == 0f)
        {
            moveDirection = 1f; // Default direction in case no input
        }

        audioManager.PlaySFX(audioManager.roll, 1);

        // Calculate the roll velocity
        Vector2 rollVelocity = new Vector2(moveDirection * rollPower, currentVelocity.y);

        // Apply the roll velocity
        rb.velocity = rollVelocity;

        // Trigger the roll animation
        animator.SetTrigger("QuickAttack");

        // Wait for the roll duration
        yield return new WaitForSeconds(rollTime);

        // Reset the velocity after the roll
        rb.velocity = currentVelocity;

        // Re-enable colliders after rolling
        foreach (Collider2D collider in colliders)
        {
            if (collider != colliders[3])
            {
                collider.enabled = true;
            }
        }
        colliders[5].enabled = false;
        colliders[4].enabled = false;

        // Reset the gravity scale
        rb.gravityScale = ogGravityScale;

        IgnoreMovement(false);
        ignoreDamage = false;
        knockable = true;

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
        animator.SetTrigger("Charge");
    }
    #endregion

    public void ThreePointThrow()
    {
        audioManager.PlaySFX(audioManager.swoosh, audioManager.normalVol);
    }
    public void ThreePointBaptism()
    {
        audioManager.PlaySFX(audioManager.waterSplash, audioManager.normalVol);
    }

    public void ThreePointBall()
    {
        audioManager.PlaySFX(audioManager.fireblast, audioManager.normalVol);
    }
}
