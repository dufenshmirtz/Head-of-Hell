using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    protected PlayerScript player,enemy;
    protected Animator animator;
    protected AudioManager audioManager;
    public virtual void HeavyAttack() { }

    public void InitializeCharacter(PlayerScript playa,AudioManager audio)
    {
        player = playa;
        animator=player.animator;
        audioManager=audio;
        enemy= player.enemy;
    }

    public virtual void DealDmg() { }

    public void ResetQuickPunch()
    {
        player.animator.SetBool("QuickPunch", false);
    }

    public Collider2D HitEnemy()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);
        return hitEnemy;
    }
}
