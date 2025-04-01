using System.Collections;
using UnityEngine;

public class Rager : Character
{
    //Spell
    float cooldown = 20f;
    int hit1Damage = 2, hit2Damage = 7, hit3Damage = 10;
    //Lightattack
    int lightDamage = 3;
    bool spellHit = false;

    #region HeavyAttack
    override public void HeavyAttack()
    {
        if (usingAbility)
        {
            return;
        }
        animator.SetTrigger("HeavyAttack");
        audioManager.PlaySFX(audioManager.heavyswoosh, audioManager.heavySwooshVolume);
        ResetQuickPunch();
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle( attackPoint.position,  attackRange,  enemyLayer);

        if (hitEnemy != null)
        {

            audioManager.PlaySFX(audioManager.heavyattack, 1f);
            enemy.TakeDamage(heavyDamage, true);

            if (! enemy.isBlocking)
            {
                enemy.Knockback(11f, 0.15f, true);
            }

        }
        else
        {
            audioManager.PlaySFX(audioManager.swoosh, 1f);
        }

        ResetQuickPunch();

    }
    #endregion

    #region Spell
    override public void Spell()
    {
        animator.SetTrigger("Spell");
        UsingAbility(cooldown);
    }

    public void DealComboDmg()
    {

        Collider2D hitEnemy = Physics2D.OverlapCircle( attackPoint.position,  attackRange,  enemyLayer);

        if (hitEnemy != null)
        {
            enemy.StopPunching();
            enemy.BreakCharge();
            cdbarimage.sprite =  activeSprite;
            //dmg and sound
            hitEnemy.GetComponent<Character>().TakeDamage(0, true); 
            audioManager.PlaySFX(audioManager.lightattack, audioManager.lightAttackVolume);

            //playerState
            stayStatic();
            canRotate = false;
            //enemystate
            enemy.stayStatic();
            enemy.blockBreaker();
            enemy.AbilityDisabled();
            enemy.Grabbed();
            animator.SetBool("ComboReady", true);
            spellHit = true;
        }
        else
        {
            audioManager.PlaySFX(audioManager.swoosh, audioManager.swooshVolume);
            animator.SetBool("isUsingAbility", false);
            ResetQuickPunch();
            OnCooldown(cooldown);
        }

    }

    public void Startcombo()
    {
        if(spellHit)
        {
            animator.SetTrigger("Combo");

            StartCoroutine(DealComboDamageOverTime(2f, 15));
        }
    }

    private IEnumerator DealComboDamageOverTime(float totalDuration, int totalHits)
    {
        float delayBetweenHits = totalDuration / totalHits; // Calculate delay between hits

        for (int i = 0; i < totalHits; i++)
        {
            if (enemy != null) // Ensure enemy is not null
            {
                enemy.TakeDamage(1, false);
            }
            yield return new WaitForSeconds(delayBetweenHits); // Wait before the next hit
        }
    }
    public void FirstHit() //old and useless remove
    {
        enemy.GetComponent<Character>().TakeDamage(hit1Damage, true); //--here
        audioManager.PlaySFX(audioManager.lightattack, audioManager.lightAttackVolume);
    }

    public void SecondHit() //old and useless remove
    {
        enemy.GetComponent<Character>().TakeDamage(hit2Damage, true); //--here
        audioManager.PlaySFX(audioManager.heavyattack, audioManager.lightAttackVolume);
    }

    public void ThirdHit()
    {
        enemy.GetComponent<Character>().TakeDamage(hit3Damage,true); //--here
        audioManager.PlaySFX(audioManager.klong, audioManager.doubleVol);

        //player state reset
         stayDynamic();
         canRotate = true;


        //enemy state
        enemy.stayDynamic();
        enemy.AbilityEnabled();
        enemy.moveSpeed =  OGMoveSpeed;
        enemy.Knockback(8f, .25f, false);

        //cd
        spellHit = false;
        ResetQuickPunch();
        animator.SetBool("ComboReady", false);

        OnCooldown(cooldown);

    }
    #endregion

    #region LightAttack
    public override void LightAttack()
    {
        animator.SetTrigger("QuickAttack");
    }
    public void QuickPunchDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle( attackPoint.position,  attackRange,  enemyLayer);

        if (hitEnemy != null)
        {
            enemy.TakeDamageNoAnimation(lightDamage, true);
            audioManager.PlaySFX(audioManager.lightattack, audioManager.lightAttackVolume);
        }
        else
        {
            audioManager.PlaySFX(audioManager.swoosh, audioManager.swooshVolume);
        }

        animator.SetBool("QuickPunch", false);
    }

    public void QuickPunchStart()
    {
        animator.SetBool("QuickPunch", true);

    }
    #endregion

    #region ChargeAttack
    public override void ChargeAttack()
    {
        base.ChargeAttack();
        animator.SetTrigger("Charge");
    }
    #endregion
}
