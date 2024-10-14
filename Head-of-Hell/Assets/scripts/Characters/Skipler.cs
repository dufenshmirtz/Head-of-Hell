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

    public Transform skiplerPoint;
    public GameObject skiplerDouble;

    #region HeavyAttack
    override public void HeavyAttack()
    {
        animator.SetTrigger("HeavyAttack");
        audioManager.PlaySFX(audioManager.skiplaHeavyCharge, audioManager.heavySwooshVolume);
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {

            audioManager.PlaySFX(audioManager.skiplaHeavyHit, 1f);
            enemy.TakeDamage(heavyDamage, true);

            if (!enemy.isBlocking)
            {
                enemy.Knockback(11f, 0.15f, true);
            }

        }
        else
        {
            audioManager.PlaySFX(audioManager.sworDashMiss, 1f);
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

        skiplerDouble=resources.skiplerDouble;
        skiplerPoint=resources.skiplerPoint;

        GameObject skipDouble = Instantiate(skiplerDouble, skiplerPoint.position,skiplerPoint.rotation);
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

        // Determine the dash direction based on the input
        float moveDirection = Input.GetKey(left) ? -1f : 1f;

        Collider2D[] colliders = GetColliders();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
        colliders[3].enabled = true;
        colliders[4].enabled = true;


        audioManager.PlaySFX(audioManager.dash, 1);
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

        dashHit = false;
        dashing = false;

        OnCooldown(cooldown);
        ignoreDamage = false;

    }

    public void DealDashDmg()
    {
        enemy.StopPunching();
        enemy.BreakCharge();
        enemy.TakeDamage(DashDamage, true);
        audioManager.PlaySFX(audioManager.dashHit, audioManager.heavyAttackVolume);
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

        // Determine the dash direction based on the input
        float moveDirection = Input.GetKey(left) ? -1f : 1f; ;


        audioManager.PlaySFX(audioManager.sworDashin, 1);
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
            audioManager.PlaySFX(audioManager.sworDashHit, 0.8f);
            ReduceCD();
        }
        else
        {
            audioManager.PlaySFX(audioManager.sworDashMiss, audioManager.swooshVolume);
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
}
