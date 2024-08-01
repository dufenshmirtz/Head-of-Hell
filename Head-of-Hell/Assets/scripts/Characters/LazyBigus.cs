using System.Collections;
using UnityEngine;
using Transform = UnityEngine.Transform;

public class LazyBigus : Character
{
    public GameObject bulletPrefab; // The bullet prefab
    public Transform firePoint; // The point from where the bullet will be instantiated
    public float bulletSpeed = 35f; // Speed of the bullet
    bool isShootin = false;
    public KeyCode shoot;
    float cooldown = 60f;
    float heal = 30f, healTime;

    #region HeavyAttack
    override public void HeavyAttack()
    {
        animator.SetTrigger("BigusHeavy");
        audioManager.PlaySFX(audioManager.heavyswoosh, audioManager.heavySwooshVolume);
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);

        if (hitEnemy != null)
        {

            audioManager.PlaySFX(audioManager.BigusHeavy, 1f);
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
        player.IgnoreUpdate(true);
        player.UsingAbility(cooldown);

        healTime = heal / 5f;
        //Activate abilty function

        audioManager.PauseMusic();
        audioManager.PlaySFX(audioManager.lullaby, audioManager.lullaVol);

        StartCoroutine(Sleeping(healTime));
    }

    IEnumerator Sleeping(float duration)
    {
        player.DeactivateColliders();
        player.stayStatic();

        // Set the player as dead
        animator.SetBool("isDead", true);


        // Loop for the specified duration
        for (int i = 0; i < duration; i++)
        {

            // Regenerate health
            player.currHealth += 5;
            player.healthbar.SetHealth(player.currHealth);


            yield return new WaitForSeconds(1f);
        }

        if (player.currHealth > player.maxHealth)
        {
            player.currHealth = player.maxHealth;
        }

        animator.SetBool("isDead", false);

        animator.SetBool("permanentDeath", false);

        player.enabled = true;

        audioManager.StartMusic();

        animator.SetTrigger("Angel");

    }

    public void Rejuvenation()
    {

        player.ActivateColliders();
        player.stayDynamic();

        player.moveSpeed = player.OGMoveSpeed;
        player.IgnoreUpdate(false);

        // Start the cooldown timer
        player.OnCooldown(cooldown);

    }

    public void BigusKnock()
    {
        if (IsEnemyClose())
        {
            enemy.StopCHarge();
            enemy.Knockback(10f, .3f, false);
        }
    }
    #endregion

    void Shoot()
    {
        audioManager.PlaySFX(audioManager.shoot, audioManager.normalVol);
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(transform.localScale.x * bulletSpeed, 0); // Shoots in the direction the character is facing

        Destroy(bullet, 2f);
    }


}
