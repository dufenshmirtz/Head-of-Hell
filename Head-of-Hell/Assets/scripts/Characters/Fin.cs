using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Fin : Character
{

    override public void HeavyAttack()
    {
        animator.SetTrigger("Punch");
        audioManager.PlaySFX(audioManager.heavyswoosh, audioManager.heavySwooshVolume);

        ResetQuickPunch();
    }

    override public void DealDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);

        if (hitEnemy != null)
        {

            audioManager.PlaySFX(audioManager.heavyattack, 1f);
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
}
