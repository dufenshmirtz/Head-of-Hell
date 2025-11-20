using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Chiback : Character
{
    float cooldown = 10f;
    float jumpHeight = 5f;
    float jumpSpeed = 10f;
    float jumpDuration = 1f;
    bool fireReady = true;
    int timesHit = 0;
    int enragingNum = 3;
    int shortJumpDamage = 5, MedJumpDamage = 10, wideJumpDamage = 15;
    bool roarPlayed = false;

    public Transform mirrorFireAttackPoint;
    public Transform fireAttackPoint;

    public override void Start()
    {
        base.Start();
        mirrorFireAttackPoint = resources.mirrorFireAttackPoint;
        fireAttackPoint = resources.fireAttackPoint;
    }

    #region HeavyAttack
    override public void HeavyAttack()
    {
        animator.SetTrigger("HeavyAttack");
        audioManager.PlaySFX(audioManager.heavyswoosh, audioManager.heavySwooshVolume);
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {
            audioManager.PlaySFX(audioManager.katanaHit, 1.8f);

            enemy.photonView.RPC("RPC_TakeDamage", RpcTarget.All, heavyDamage, true, true);

            if (!enemy.isBlocking)
            {
                enemy.photonView.RPC("RPC_Knockback", RpcTarget.All, 11f, 0.15f, true);
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
        animator.SetTrigger("Spell");
        UsingAbility(cooldown);
        audioManager.PlaySFX(audioManager.sytheDash, audioManager.normalVol);
        ignoreDamage = true;
        StartCoroutine(ScytheJump());
    }

    private IEnumerator ScytheJump()
    {
        IgnoreUpdate(true);

        float moveDirection = Input.GetKey(left) ? -1f : (Input.GetKey(right) ? 1f : (controller ? Input.GetAxis("Horizontal" + playerString) : 0f));

        if (moveDirection == 0f)
        {
            IgnoreUpdate(false);
            OnCooldown(cooldown);
            yield break;
        }

        if (!Input.GetKey(KeyCode.W) && isGrounded || (controller && Input.GetAxis("Vertical" + playerString) <= 0.5f && isGrounded))
        {
            rb.AddForce(new Vector2(moveDirection * jumpSpeed, jumpHeight), ForceMode2D.Impulse);
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(new Vector2(moveDirection * jumpSpeed, jumpHeight), ForceMode2D.Impulse);
        }

        float elapsedTime = 0f;

        while (elapsedTime < jumpDuration)
        {
            Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

            if (hitEnemy != null)
            {
                enemy.photonView.RPC("RPC_BreakCharge", RpcTarget.All);

                animator.SetTrigger("SpellHit");
                audioManager.PlaySFX(audioManager.sytheHit, 1f);
                audioManager.PlaySFX(audioManager.sytheSlash, 1f);

                if (elapsedTime < 0.33f)
                {
                    enemy.photonView.RPC("RPC_TakeDamage", RpcTarget.All, shortJumpDamage, true, true);
                    Enraged(shortJumpDamage);
                }
                else if (elapsedTime < 0.66f)
                {
                    enemy.photonView.RPC("RPC_TakeDamage", RpcTarget.All, MedJumpDamage, true, true);
                    Enraged(MedJumpDamage);
                }
                else
                {
                    enemy.photonView.RPC("RPC_TakeDamage", RpcTarget.All, wideJumpDamage, true, true);
                    Enraged(wideJumpDamage);
                }

                if (timesHit >= enragingNum)
                {
                    timesHit = 0;
                }

                enemy.photonView.RPC("RPC_Knockback", RpcTarget.All, 11f, 0.25f, false);

                rb.velocity = Vector2.zero;
                break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        IgnoreUpdate(false);
        OnCooldown(cooldown);
        ignoreDamage = false;
    }
    #endregion

    #region LightAttack
    public override void LightAttack()
    {
        if (fireReady)
        {
            QuickAttackIndicatorDisable();
            animator.SetTrigger("QuickAttack");
            fireReady = false;
            audioManager.PlaySFX(audioManager.fireblast, 1f);
            StartCoroutine(ResetFire());
        }
    }

    public void DealFireDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(mirrorFireAttackPoint.position, attackRange, enemyLayer);
        Collider2D hitEnemy2 = Physics2D.OverlapCircle(fireAttackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null || hitEnemy2 != null)
        {
            audioManager.PlaySFX(audioManager.lightattack, 0.5f);

            enemy.photonView.RPC("RPC_TakeDamage", RpcTarget.All, 5, true, true);

            if (!onCooldown)
            {
                enemy.photonView.RPC("RPC_Knockback", RpcTarget.All, 15f, 0.8f, true);
            }

            enemy.photonView.RPC("RPC_DisableBlock", RpcTarget.All, true);
            enemy.photonView.RPC("RPC_DisableJump", RpcTarget.All, true);

            StartCoroutine(ResetBlockability());
        }
        else
        {
            audioManager.PlaySFX(audioManager.swoosh, 1f);
        }
    }

    IEnumerator ResetFire()
    {
        yield return new WaitForSeconds(3f);
        audioManager.PlaySFX(audioManager.sworDashTada, audioManager.lessVol);
        fireReady = true;
        QuickAttackIndicatorEnable();
    }

    private IEnumerator ResetBlockability()
    {
        yield return new WaitForSeconds(jumpDuration + 0.1f);

        enemy.photonView.RPC("RPC_EnableBlock", RpcTarget.All);
        enemy.photonView.RPC("RPC_DisableJump", RpcTarget.All, false);
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
    override public void TakeDamage(int dmg, bool blockable, bool parryable = true)
    {
        if (timesHit < enragingNum && !isBlocking)
        {
            timesHit++;
        }

        if (timesHit == enragingNum && !roarPlayed)
        {
            audioManager.PlaySFX(audioManager.growl, 0.5f);
            roarPlayed = true;
        }

        base.TakeDamage(dmg, blockable);
    }

    void Enraged(int jumpDamage)
    {
        if (timesHit == enragingNum)
        {
            enemy.photonView.RPC("RPC_TakeDamage", RpcTarget.All, jumpDamage / 2, true, true);
            roarPlayed = false;
        }
    }
    #endregion
}
