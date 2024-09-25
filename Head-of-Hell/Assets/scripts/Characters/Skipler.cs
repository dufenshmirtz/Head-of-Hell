using System.Collections;
using UnityEngine;

public class Skipler : Character
{
    public float cooldown = 8f;
    //Spell
    protected float dashingPower = 40f;
    protected float dashingTime = 0.1f;
    public int DashDamage = 10;
    bool dashing = false;
    bool dashHit = false;
    //LightAttack
    bool lightReady = true;
    public float swordDashPower = 10f;
    public float swordDashTime = 0.14f;
    public int swordDashDmg = 5;

    #region HeavyAttack
    override public void HeavyAttack()
    {
        animator.SetTrigger("skiplaHeavy");
        audioManager.PlaySFX(audioManager.skiplaHeavyCharge, audioManager.heavySwooshVolume);
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);

        if (hitEnemy != null)
        {

            audioManager.PlaySFX(audioManager.skiplaHeavyHit, 1f);
            enemy.TakeDamage(heavyDamage, true);

            if (!player.enemy.isBlocking)
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
        player.UsingAbility(cooldown);
        ignoreDamage = true;
        StartCoroutine(Dash());
    }

    IEnumerator Dash()
    {

        player.ignoreMovement = true;
        ignoreDamage=true;

        dashing = true;

        // Store the original gravity scale
        float ogGravityScale = rb.gravityScale;

        // Disable gravity while dashing
        rb.gravityScale = 0f;

        // Store the current velocity
        Vector2 currentVelocity = rb.velocity;

        // Determine the dash direction based on the input
        float moveDirection = Input.GetKey(player.left) ? -1f : 1f; 

        Collider2D[] colliders = player.GetColliders();
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
        animator.SetTrigger("Dash");



        // Wait for the dash duration
        yield return new WaitForSeconds(dashingTime);

        // Reset the velocity after the dash
        rb.velocity = currentVelocity;

        // Reset the gravity scale
        rb.gravityScale = ogGravityScale;

        ignoreDamage=false;


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

        player.OnCooldown(cooldown);
        ignoreDamage = false;

    }

    public void DealDashDmg()
    {
        enemy.StopPunching();
        enemy.BreakCharge();
        enemy.TakeDamage(DashDamage, true);
        audioManager.PlaySFX(audioManager.dashHit, audioManager.heavyAttackVolume);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (dashing && other.CompareTag("Player") && !dashHit)  //--here
        {
            DealDashDmg();
            dashHit = true;
        }
    }
    #endregion

    #region LightAttack
    override public void LightAttack() {
        if (lightReady)
        {
            QuickAttackIndicatorDisable();
            StartCoroutine(SwordDash());
        }
    }

    IEnumerator SwordDash()
    {
        player.IgnoreMovement(true);

        // Store the original gravity scale
        float ogGravityScale = rb.gravityScale;

        // Disable gravity while dashing
        rb.gravityScale = 0f;

        // Store the current velocity
        Vector2 currentVelocity = rb.velocity;

        // Determine the dash direction based on the input
        float moveDirection = Input.GetKey(player.left) ? -1f : 1f; ;


        audioManager.PlaySFX(audioManager.sworDashin, 1);
        // Calculate the dash velocity
        Vector2 sworddashVelocity = new Vector2(moveDirection * swordDashPower, currentVelocity.y);

        // Apply the dash velocity
        rb.velocity = sworddashVelocity;

        // Trigger the dash animation
        animator.SetTrigger("swordDash");


        // Wait for the dash duration
        yield return new WaitForSeconds(swordDashTime);

        // Reset the velocity after the dash
        rb.velocity = currentVelocity;

        // Reset the gravity scale
        rb.gravityScale = ogGravityScale;

        lightReady = false;

        player.IgnoreMovement(false);

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
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);

        if (hitEnemy != null)
        {
            enemy.TakeDamage(swordDashDmg, true);
            enemy.Knockback(10f, .15f, true);
            audioManager.PlaySFX(audioManager.sworDashHit, 0.8f);
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
        animator.SetTrigger("SkiplerCharge");
    }
    #endregion
}
