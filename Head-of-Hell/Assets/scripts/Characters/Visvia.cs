using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visvia : Character
{

    bool shotgunReady = true;
    float shotgunForce = 15f;
    float upwardsForce = 6f;
    float shotgunCooldown = 2f;
    float dashDuration = 0.3f;
    float shotgunRange = 1f;
    int shotgunDamage = 5;

    public int blastCounter=0;
    float overheatDuration = 8f;
    float elapsed = 0f;
    float overheatFrequency=0.5f;
    int overheatDamage=3;

    int cooldown = 10;
    int grabDamage = 12;

    Transform grabPoint;

    public override void Start()
    {
        base.Start();

        grabPoint = resources.bellPoint;
    }

    #region HeavyAttack
    override public void HeavyAttack()
    {
        animator.SetTrigger("HeavyAttack");
        audioManager.PlaySFX(audioManager.heavyswoosh, audioManager.heavySwooshVolume);
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle( attackPoint.position,  attackRange,  enemyLayer);

        if (hitEnemy != null)
        {

            audioManager.PlaySFX(audioManager.katanaHit, 1f);
            enemy.TakeDamage(heavyDamage, true);
            if (! enemy.isBlocking)
            {
                enemy.Knockback(11f, 0.15f, true);
            }

        }
        else
        {
            audioManager.PlaySFX(audioManager.katanaSwoosh, 1f);
        }

    }
    #endregion

    #region Spell
    override public void Spell()
    {
        animator.SetTrigger("Spell");
        blastCounter++;
        UsingAbility(cooldown);
        StartCoroutine(GrabEnd());
    }

    public void GrabDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(grabPoint.position, attackRange * 3, enemyLayer);

        if (hitEnemy != null)
        {
            enemy.StopPunching();
            enemy.TakeDamage(grabDamage, true);
            enemy.Knockback(8f, 0.3333f, true);

        }
        else
        {
            audioManager.PlaySFX(audioManager.bellPunch, audioManager.swooshVolume);
        }
    }

    IEnumerator GrabEnd(){
        yield return new WaitForSeconds(0.59f);
        OnCooldown(cooldown);
        OverheatCheck();
    }

    

    #endregion

    #region LightAttack
    public override void LightAttack()
    {
        if (shotgunReady)
        {
            blastCounter++;
            shotgunReady = false;
            QuickAttackIndicatorDisable();
            StartCoroutine(ShotgunBlast());
        }
    }

    private IEnumerator ShotgunBlast()
    {
        // Lock orientation
        canRotate = false;

        // Determine direction to dash (opposite of facing)
        float direction = transform.localScale.x > 0 ? -1f : 1f;

        // Apply recoil force (backward)
        rb.velocity = new Vector2(direction * shotgunForce, upwardsForce);

        // Deal damage in front of the player
        Vector2 attackPosition = attackPoint.position;
        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, shotgunRange, enemyLayer);
        if (hit != null)
        {
            enemy.TakeDamage(shotgunDamage, false);
            if (!enemy.isBlocking)
            {
                enemy.Knockback(10f, 0.2f, true);
            }
            audioManager.PlaySFX(audioManager.counterSucces, 1f);
        }

        // Wait during dash
        yield return new WaitForSeconds(dashDuration);

        // Unlock orientation and reset velocity
        canRotate = true;
        rb.velocity = Vector2.zero;
        OverheatCheck();

        // Cooldown before you can shotgun again
        yield return new WaitForSeconds(shotgunCooldown);
        shotgunReady = true;
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

    #region Passive
    
    public void OverheatCheck(){
        if(blastCounter >= 5){
            chargeDisable = true;
            heavyDisable = true;
            specialDisable = true;
            blockDisable = true;
            quickDisable = true;
            moveSpeed= moveSpeed + 2;
            canAlterSpeed = false;
            StartCoroutine(Overheat());
        }
    }

    IEnumerator Overheat(){

        while (elapsed < overheatDuration)
        {
            Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, shotgunRange, enemyLayer);
            if (hit != null)
            {
                Character target = hit.GetComponent<Character>();
                if (target != null && target != this)
                {
                    target.TakeDamage(overheatDamage, false);
                }
            }

            yield return new WaitForSeconds(overheatFrequency);
            elapsed += overheatFrequency;
        }

        elapsed = 0;
        moveSpeed = OGMoveSpeed;
        canAlterSpeed = true;
        blastCounter = 0;
        chargeDisable = false;
        heavyDisable = false;
        specialDisable = false;
        blockDisable = false;
        quickDisable = false;
    }

    #endregion

}
