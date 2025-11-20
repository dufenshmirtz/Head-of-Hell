using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Fin : Character
{
    float missCooldown = 10f, cooldown = 15f;
    float rollPower = 8f;
    float rollTime = 0.39f;
    bool rollReady = true;
    int passiveDamage = 4;

    Transform escapePoint;
    GameObject freeBaby;

    public override void Start()
    {
        base.Start();

        escapePoint = resources.escapeRoute;
        freeBaby = resources.freebaby;
    }

    #region HeavyAttack
    override public void HeavyAttack()
    {
        animator.SetTrigger("HeavyAttack");
        audioManager.PlaySFX(audioManager.incense, audioManager.doubleVol);
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {
            audioManager.PlaySFX(audioManager.heavyattack, 1f);

            // main hit
            enemy.photonView.RPC("RPC_TakeDamage", RpcTarget.All, heavyDamage, true, true);

            // passive extra no-anim damage
            enemy.photonView.RPC("RPC_TakeDamageNoAnimation", RpcTarget.All, passiveDamage, true, false);

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

        float ogGravityScale = rb.gravityScale;
        rb.gravityScale = 0f;
        Vector2 currentVelocity = rb.velocity;

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
        colliders[5].enabled = true;

        float moveDirection = Input.GetKey(left)
            ? -1f
            : (Input.GetKey(right) ? 1f : (controller ? Input.GetAxis("Horizontal" + playerString) : 0f));

        if (moveDirection == 0f)
        {
            moveDirection = 1f;
        }

        audioManager.PlaySFX(audioManager.roll, 1);

        Vector2 rollVelocity = new Vector2(moveDirection * rollPower, currentVelocity.y);
        rb.velocity = rollVelocity;

        animator.SetTrigger("QuickAttack");

        yield return new WaitForSeconds(rollTime);

        rb.velocity = currentVelocity;

        foreach (Collider2D collider in colliders)
        {
            if (collider != colliders[3])
            {
                collider.enabled = true;
            }
        }
        colliders[5].enabled = false;
        colliders[4].enabled = false;

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
        audioManager.PlaySFX(audioManager.counterScream, audioManager.normalVol);
    }

    public void ThreePointBaptism()
    {
        audioManager.PlaySFX(audioManager.waterSplash, audioManager.normalVol);
    }

    public void ThreePointBall()
    {
        audioManager.PlaySFX(audioManager.fireblast, audioManager.normalVol);
    }

    public void FreeExit()
    {
        StartCoroutine(SpawnBabiesWithDelay());
    }

    [PunRPC]
    void RPC_SpawnBaby(float x, float y, int direction)
    {
        GameObject baby = Instantiate(freeBaby, new Vector3(x, y, 0f), Quaternion.identity);

        BabyRun mover = baby.GetComponent<BabyRun>();
        if (mover != null)
        {
            mover.SetDirection(direction);
        }
        else
        {
            Debug.LogWarning("BabyRun script is missing on the prefab.");
        }
    }

    IEnumerator SpawnBabiesWithDelay()
    {
        for (int i = 0; i < 3; i++)
        {
            int direction = transform.localScale.x > 0 ? -1 : 1;

            if (PhotonNetwork.InRoom)
            {
                photonView.RPC(
                    "RPC_SpawnBaby",
                    RpcTarget.All,
                    escapePoint.position.x,
                    escapePoint.position.y,
                    direction
                );
            }
            else
            {
                GameObject baby = Instantiate(freeBaby, escapePoint.position, Quaternion.identity);

                BabyRun mover = baby.GetComponent<BabyRun>();
                if (mover != null)
                {
                    mover.SetDirection(direction);
                }
                else
                {
                    Debug.LogWarning("BabyRun script is missing on the prefab.");
                }
            }

            yield return new WaitForSeconds(1.5f);
        }
    }
}
