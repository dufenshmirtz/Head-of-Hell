using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using Transform = UnityEngine.Transform;

public class LazyBigus : Character
{
    public GameObject bulletPrefab; // The bullet prefab
    public Transform firePoint; // The point from where the bullet will be instantiated
    public float bulletSpeed = 35f; // Speed of the bullet
    bool isShootin = false;
    public KeyCode shoot;

    override public void HeavyAttack()
    {
        animator.SetTrigger("BigusHeavy");
        audioManager.PlaySFX(audioManager.heavyswoosh, audioManager.heavySwooshVolume);

        ResetQuickPunch();
    }

    override public void DealDmg()
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

        void Shoot()
        {
            audioManager.PlaySFX(audioManager.shoot, audioManager.normalVol);
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.velocity = new Vector2(transform.localScale.x * bulletSpeed, 0); // Shoots in the direction the character is facing

            Destroy(bullet, 2f);
        }

    }

    
}
