using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    protected PlayerScript player, enemy;
    protected Animator animator;
    protected AudioManager audioManager;
    protected Rigidbody2D rb;
    protected CharacterResources resources;
    protected bool usingAbility;
    //Charge
    bool charged = false;
    protected bool charging = false;
    private Coroutine chargeCoroutine;
    float chargeTime = 0.5f;
    protected int chargeDmg = 34;
    protected bool isBlocking=false;
    protected bool ignoreDamage=false;
    protected int heavyDamage = 14;
    public bool blockDisabled = false;

    public void InitializeCharacter(PlayerScript playa, AudioManager audio, CharacterResources res)
    {
        player = playa;
        animator = player.animator;
        audioManager = audio;
        enemy = player.enemy;
        rb=player.rb;
        resources = res;
        usingAbility = false;
    }

    #region Purely Virtual
    public virtual void HeavyAttack() { }

    public virtual void DealHeavyDamage() { }

    public virtual void Spell() { }

    public virtual void LightAttack() { }
    #endregion

    #region ChargeAttack
    public virtual void ChargeAttack() {
        player.knockable = false;
        charging = true;
        animator.SetBool("Charging", true);
        StartCharge();
    }

    private void StartCharge()
    {
        // If there is an existing charge coroutine, stop it
        if (chargeCoroutine != null)
        {
            StopCoroutine(chargeCoroutine);
        }

        // Start a new charge coroutine
        chargeCoroutine = StartCoroutine(Charge());
    }

    private IEnumerator Charge()
    {
        yield return new WaitForSeconds(chargeTime);  // Waits for 2 seconds
        if (charging)
        {
            charged = true;
            audioManager.PlaySFX(audioManager.charged, 0.7f);
        }
    }

    public virtual void DealChargeDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);

        if (hitEnemy != null)
        {
            enemy.StopPunching();
            enemy.BreakCharge();
            enemy.TakeDamage(chargeDmg,false);
            enemy.Knockback(13f, 0.4f, false);
            audioManager.PlaySFX(audioManager.smash, audioManager.doubleVol);
        }
        else
        {
            audioManager.PlaySFX(audioManager.swoosh, audioManager.swooshVolume);
        }
        player.knockable = true;
        charging = false;
        animator.SetBool("Casting", false);
        animator.SetBool("Charging", false);
        player.stayDynamic();
    }

    public bool ChargeCheck(KeyCode charge)
    {
        if (charging)
        {
            player.stayStatic();
            if (charged)
            {
                if (Input.GetKeyUp(charge))
                {
                    player.stayDynamic();
                    animator.SetTrigger("ChargedHit");
                    charged = false;
                    animator.SetBool("Casting", true);

                }
                return true;
            }
            if (Input.GetKeyUp(charge))
            {
                player.stayDynamic();
                animator.SetBool("Charging", false);
                charging = false;
                player.knockable = true;
            }
            return true;
        }
        return false;
    }

    public void StopCHarge()
    {
        player.stayDynamic();
        animator.SetBool("Charging", false);
        charging = false;
        player.knockable = true;
        charged = false;
        animator.SetBool("Casting", false);
        animator.ResetTrigger("ChargedHit");
    }
    #endregion

    #region Block
    public void Block()
    {
        if (blockDisabled)
        {
            return;
        }
        animator.SetTrigger("critsi");
        animator.SetBool("Crouch", true);
        player.PlayerBlock(true);
        isBlocking=true;
        ResetQuickPunch();
    }
    public void Unblock()
    {
        animator.SetBool("cWalk", false);
        animator.SetBool("Crouch", false);
        player.PlayerBlock(false);
        isBlocking=false;

        ResetQuickPunch();
    }
    #endregion

    #region General
    public void Jump()
    {
        player.rb.velocity = new Vector2(player.rb.velocity.x, player.jumpForce);
        animator.SetTrigger("Jump");
        audioManager.PlaySFX(audioManager.jump, audioManager.jumpVolume);

        ResetQuickPunch();
    }

    public Collider2D HitEnemy()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);
        return hitEnemy;
    }

    public bool IsEnemyClose()
    {
        return Vector3.Distance(this.transform.position, enemy.transform.position) <= 3f;
    }

    virtual public void HeavyAttackStart()
    {
        player.moveSpeed = player.heavySpeed;
        StartCoroutine(WaitAndSetSpeed());
        animator.SetBool("isHeavypunching", true);
    }

    public void HeavyAttackEnd()
    {
        player.moveSpeed = player.OGMoveSpeed;
        animator.SetBool("isHeavypunching", false);
    }

    private IEnumerator WaitAndSetSpeed()
    {

        yield return new WaitForSeconds(0.49f);  // Waits for 0.49 seconds
        player.moveSpeed = player.OGMoveSpeed;

    }

    virtual public void TakeDamage(int dmg,bool blockable)
    {
        if (ignoreDamage)
        {
            return;
        }

        ResetQuickPunch();

        if (dmg == chargeDmg)
        {
            StopCHarge();
        }

        if (isBlocking && blockable)
        {
            if (dmg == heavyDamage) //if its heavy attack take half the damage
            {
                player.currHealth -= 5;
                print("Took 5 damage");
                player.healthbar.SetHealth(player.currHealth);
            }
            //if its light attack take no dmg

            if (dmg == chargeDmg)
            {
                player.currHealth -= dmg;

                player.healthbar.SetHealth(player.currHealth);
                player.moveSpeed = player.OGMoveSpeed;
            }
        }
        else
        {
            player.currHealth -= dmg;

            animator.SetTrigger("tookDmg");

            print("Took " + dmg + " damage");

            player.healthbar.SetHealth(player.currHealth);
        }

        if (player.currHealth <= 0)
        {
            player.Die();
        }
    }

    protected void QuickAttackIndicatorEnable()
    {
        player.quickAttackIndicator.SetActive(true);
    }

    protected void QuickAttackIndicatorDisable()
    {
        player.quickAttackIndicator?.SetActive(false);
    }
    #endregion

    #region Special Functions
    //Rager
    public void ResetQuickPunch()
    {
        animator.ResetTrigger("punch2");
        player.animator.SetBool("QuickPunch", false);
    }

    public void Grabbed()
    {
        audioManager.PlaySFX(audioManager.grab, audioManager.heavyAttackVolume);
        animator.SetTrigger("grabbed");
    }

    #endregion
}
