using UnityEngine;

public class Rager : Character
{
    //Spell
    float cooldown = 20f;
    int hit1Damage = 2, hit2Damage = 7, hit3Damage = 10;
    //Lightattack
    int lightDamage = 5;

    #region HeavyAttack
    override public void HeavyAttack()
    {
        if (usingAbility)
        {
            return;
        }
        animator.SetTrigger("RagerHeavy");
        audioManager.PlaySFX(audioManager.heavyswoosh, audioManager.heavySwooshVolume);
        ResetQuickPunch();
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);

        if (hitEnemy != null)
        {

            audioManager.PlaySFX(audioManager.heavyattack, 1f);
            enemy.TakeDamage(heavyDamage, true);

            if (!player.enemy.isBlocking)
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
        animator.SetTrigger("comboInit");
        player.UsingAbility(cooldown);
        ignoreDamage = true;
    }

    public void DealComboDmg()
    {

        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);

        if (hitEnemy != null)
        {
            enemy.StopPunching();
            enemy.BreakCharge();
            player.cdbarimage.sprite = player.activeSprite;
            //dmg and sound
            hitEnemy.GetComponent<PlayerScript>().TakeDamage(0, true); //--here
            audioManager.PlaySFX(audioManager.lightattack, audioManager.lightAttackVolume);

            //playerState
            player.stayStatic();
            player.canRotate = false;
            //enemystate
            enemy.stayStatic();
            enemy.blockBreaker();
            enemy.AbilityDisabled();
            enemy.GetCharacter().Grabbed();
            animator.SetBool("ComboReady", true);
        }
        else
        {
            audioManager.PlaySFX(audioManager.swoosh, audioManager.swooshVolume);
            animator.SetBool("isUsingAbility", false);
            ResetQuickPunch();
            player.OnCooldown(cooldown);
            ignoreDamage = false;
        }

    }

    public void Startcombo()
    {
        animator.SetTrigger("Combo");
    }
    public void FirstHit()
    {
        enemy.GetComponent<PlayerScript>().TakeDamage(hit1Damage, true); //--here
        audioManager.PlaySFX(audioManager.lightattack, audioManager.lightAttackVolume);
    }

    public void SecondHit()
    {
        enemy.GetComponent<PlayerScript>().TakeDamage(hit2Damage, true); //--here
        audioManager.PlaySFX(audioManager.heavyattack, audioManager.lightAttackVolume);
    }

    public void ThirdHit()
    {
        enemy.GetComponent<PlayerScript>().TakeDamage(hit3Damage,true); //--here
        audioManager.PlaySFX(audioManager.klong, audioManager.doubleVol);

        //player state reset
        player.stayDynamic();
        player.canRotate = true;


        //enemy state
        enemy.stayDynamic();
        enemy.AbilityEnabled();
        enemy.moveSpeed = player.OGMoveSpeed;
        enemy.Knockback(8f, .25f, false);

        //cd
        ResetQuickPunch();
        animator.SetBool("ComboReady", false);

        player.OnCooldown(cooldown);
        ignoreDamage = false;

    }
    #endregion

    #region LightAttack
    public override void LightAttack()
    {
        animator.SetTrigger("Punch2");
    }
    public void QuickPunchDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);

        if (hitEnemy != null)
        {

            //hitEnemy.GetComponent<PlayerScript>().TakeDamage(lightDMG); //--here
            enemy.TakeDamage(lightDamage, true);
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
        animator.SetTrigger("RagerCharge");
    }
    #endregion
}
