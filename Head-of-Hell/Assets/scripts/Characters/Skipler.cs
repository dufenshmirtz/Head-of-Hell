using System.Collections;
using UnityEngine;

public class Skipler : Character
{
    protected float cooldown = 8f;
    //Spell
    protected float dashingPower = 40f;
    protected float dashingTime = 0.1f;
    protected int DashDamage = 10;
    bool dashing = false;
    bool dashHit = false;
    //LightAttack
    bool lightReady = true;
    protected float swordDashPower = 10f;
    protected float swordDashTime = 0.14f;
    protected int swordDashDmg = 5;
    bool isIdlePlaying = false;

    public Transform skiplerPoint;
    public GameObject skiplerDouble;

    public override void Start()
    {
        base.Start();

        characterJump = audioManager.jumpGlitch;
        chargeHitSound= audioManager.chargeGlitch;
    }

    override public void Update()
    {
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);

        // Check if the current state is not idle
        if (!currentState.IsName("Idle"))
        {
            if (isIdlePlaying)
            {
                StopIdleGlitch();
            }
        }

        base.Update();
    }
    #region HeavyAttack
    override public void HeavyAttack()
    {
        animator.SetTrigger("HeavyAttack");
        audioManager.PlaySFX(audioManager.heavyGlitch, 0.8f);
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {

            audioManager.PlaySFX(audioManager.heavyGlitchHit, 1.3f);
            enemy.TakeDamage(heavyDamage, true);

            if (!enemy.isBlocking)
            {
                enemy.Knockback(11f, 0.15f, true);
            }

        }
        else
        {
            audioManager.PlaySFX(audioManager.nothitGlitch, 1.8f);
        }

    }
    #endregion

    #region Spell
    override public void Spell()
    {
        UsingAbility(cooldown);
        ignoreDamage = true;
        StartCoroutine(Dash());
    }

    IEnumerator Dash()
    {
        skiplerDouble = resources.skiplerDouble;
        skiplerPoint = resources.skiplerPoint;

        // Instantiate the skipler double
        GameObject skipDouble = Instantiate(skiplerDouble, skiplerPoint.position, skiplerPoint.rotation);
        Vector3 newScale = skipDouble.transform.localScale;  // Get the current scale
        newScale.x = this.transform.localScale.x;            // Set the x scale to match the player's x scale
        skipDouble.transform.localScale = newScale;

        ignoreMovement = true;
        ignoreDamage = true;
        dashing = true;

        // Store the original gravity scale
        float ogGravityScale = rb.gravityScale;

        // Disable gravity while dashing
        rb.gravityScale = 0f;

        // Store the current velocity
        Vector2 currentVelocity = rb.velocity;

        // Determine the dash direction based on input (keyboard always, controller only if controller == true)
        float moveDirection = Input.GetKey(left) ? -1f : (Input.GetKey(right) ? 1f : (controller ? Input.GetAxis("Horizontal" + playerString) : 0f));

        // If no direction input was given, default to right
        if (moveDirection == 0f)
        {
            moveDirection = 1f; // Default direction in case no input
        }

        // Disable colliders temporarily during the dash
        Collider2D[] colliders = GetColliders();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
        colliders[3].enabled = true;  // Keep specific colliders enabled
        colliders[4].enabled = true;

        // Play dash sound effects
        audioManager.PlaySFX(audioManager.dash, 1);
        audioManager.PlaySFX(audioManager.dashGlitch, 1f);

        // Calculate the dash velocity
        Vector2 dashVelocity = new Vector2(moveDirection * dashingPower, currentVelocity.y);

        // Apply the dash velocity
        rb.velocity = dashVelocity;

        // Trigger the dash animation
        animator.SetTrigger("Spell");

        // Wait for the dash duration
        yield return new WaitForSeconds(dashingTime);

        // Reset the velocity after the dash
        rb.velocity = currentVelocity;

        // Reset the gravity scale
        rb.gravityScale = ogGravityScale;

        // Re-enable damage and colliders
        ignoreDamage = false;
        foreach (Collider2D collider in colliders)
        {
            if (collider != colliders[3])
            {
                collider.enabled = true;
            }
        }
        colliders[3].enabled = false;
        colliders[4].enabled = false;

        // Reset dash state
        dashHit = false;
        dashing = false;

        // Trigger cooldown
        OnCooldown(cooldown);
        ignoreDamage = false;
    }


    public void DealDashDmg()
    {
        enemy.StopPunching();
        enemy.BreakCharge();
        enemy.TakeDamage(DashDamage, true);
        audioManager.PlaySFX(audioManager.dashHit, 3f);
    }

    override protected void OnTriggerEnter2D(Collider2D other)
    {
        if (dashing && other.CompareTag("Player") && !dashHit)  //--here
        {
            DealDashDmg();
            dashHit = true;
        }

        base.OnTriggerEnter2D(other);
    }
    #endregion

    #region LightAttack
    override public void LightAttack()
    {
        if (lightReady)
        {
            QuickAttackIndicatorDisable();
            StartCoroutine(SwordDash());
        }
    }

    IEnumerator SwordDash()
    {
        IgnoreMovement(true);

        // Store the original gravity scale
        float ogGravityScale = rb.gravityScale;

        // Disable gravity while dashing
        rb.gravityScale = 0f;

        // Store the current velocity
        Vector2 currentVelocity = rb.velocity;

        // Determine the dash direction based on input (keyboard always, controller only if controller == true)
        float moveDirection = Input.GetKey(left) ? -1f : (Input.GetKey(right) ? 1f : (controller ? Input.GetAxis("Horizontal" + playerString) : 0f));

        // If no direction input was given, default to right
        if (moveDirection == 0f)
        {
            moveDirection = 1f; // Default direction in case no input
        }

        audioManager.PlaySFX(audioManager.quickGlitch, 0.7f);

        // Calculate the dash velocity
        Vector2 sworddashVelocity = new Vector2(moveDirection * swordDashPower, currentVelocity.y);

        // Apply the dash velocity
        rb.velocity = sworddashVelocity;

        // Trigger the dash animation
        animator.SetTrigger("QuickAttack");

        // Wait for the dash duration
        yield return new WaitForSeconds(swordDashTime);

        // Reset the velocity after the dash
        rb.velocity = currentVelocity;

        // Reset the gravity scale
        rb.gravityScale = ogGravityScale;

        lightReady = false;

        IgnoreMovement(false);

        StartCoroutine(ResetSwordDash());
    }


    IEnumerator ResetSwordDash()
    {
        yield return new WaitForSeconds(3f);
        audioManager.PlaySFX(audioManager.sworDashTada, audioManager.lessVol);
        lightReady = true;
        QuickAttackIndicatorEnable();
    }

    public void DealSwordDashDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {
            enemy.TakeDamage(swordDashDmg, true);
            enemy.Knockback(10f, .15f, true);
            audioManager.PlaySFX(audioManager.dashHit, 0.8f);
            ReduceCD();
        }
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
    void ReduceCD()
    {
        cdTimer -= 1f;
    }
    #endregion

    public void IdleGlitch()
    {
        audioManager.PlayAndStoreSFX(audioManager.idleGlitch, audioManager.normalVol);
        isIdlePlaying = true;
    }

    public void StopIdleGlitch()
    {
        audioManager.StopAndStoreSFX(audioManager.idleGlitch);  
        isIdlePlaying = false;
    }
}
