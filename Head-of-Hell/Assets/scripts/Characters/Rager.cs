using System.Collections;
using UnityEngine;

public class Rager : Character
{
    //Spell
    public float cooldown = 20f;
    int hit1Damage = 2, hit2Damage = 7; //25 actual dmg these variables are ass
    int spellDamage2 = 10;
    int spellDamage1 = 15;
    //Lightattack
    int lightDamage = 4;
    bool spellHit = false;

    float spellTime = 2.84f;


    #region HeavyAttack
    override public void HeavyAttack()
    {
        if (usingAbility)
        {
            return;
        }
        TelemetryManager.Instance?.LogAction(PlayerId, "Heavy");
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
            audioManager.PlaySFX(audioManager.explosion, audioManager.lessVol);
            enemy.TakeDamage(heavyDamage, true);

            if (! enemy.isBlocking)
            {
                enemy.Knockback(11f, 0.15f, true);
            }

        }
        else
        {
            audioManager.PlaySFX(audioManager.explosion, audioManager.lessVol);
        }

        ResetQuickPunch();

    }
    #endregion

    #region Spell
    override public void Spell()
    {
        TelemetryManager.Instance?.LogAction(PlayerId, "Special");
        UsingAbility(cooldown);
        animator.SetTrigger("Spell");
        StartCoroutine(SpellSafety(spellTime,cooldown));
    }

    public void DealComboDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {
            // Telemetry: combo special successfully connected (log once here)
            TelemetryManager.Instance?.LogHitAttempt(PlayerId, enemy.PlayerId, MoveType.Special);

            enemy.StopPunching();
            enemy.BreakCharge();
            cdbarimage.sprite = activeSprite;

            // dmg and sound (0 damage "confirm" hit)
            hitEnemy.GetComponent<Character>().SetIncomingDamageContext(PlayerId, MoveType.Special, SourceType.Spell);
            hitEnemy.GetComponent<Character>().TakeDamage(0, true);
            audioManager.PlaySFX(audioManager.lightattack, audioManager.lightAttackVolume);

            // playerState
            stayStatic();
            ignoreUpdate = true;
            canRotate = false;

            // enemy state
            enemy.stayStatic();
            enemy.blockBreaker();
            enemy.AbilityDisabled();
            enemy.Grabbed();

            animator.SetBool("ComboReady", true);
            spellHit = true;
        }
        else
        {
            // Telemetry: special whiff (didn't grab/connect)
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Special);

            audioManager.PlaySFX(audioManager.swoosh, audioManager.swooshVolume);
            animator.SetBool("isUsingAbility", false);
            ResetQuickPunch();
            OnCooldown(cooldown);
        }
    }

    public void Startcombo()
    {
        if (spellHit)
        {
            animator.SetTrigger("Combo");

            StartCoroutine(DealComboDamageOverTime(2f, spellDamage1));
        }
    }

    private IEnumerator DealComboDamageOverTime(float totalDuration, int totalHits)
    {
        audioManager.PlaySFX(audioManager.chainsaw, 1f);

        float delayBetweenHits = totalDuration / totalHits; // Calculate delay between hits

        for (int i = 0; i < totalHits; i++)
        {
            if (enemy != null) // Ensure enemy is not null
            {
                // Telemetry: context before each tick (no extra HitAttempt spam)
                enemy.SetIncomingDamageContext(PlayerId, MoveType.Special, SourceType.Spell);
                enemy.TakeDamage(1, false, false, false);
            }
            yield return new WaitForSeconds(delayBetweenHits); // Wait before the next hit
        }
    }

    public void FirstHit() // old and useless remove
    {
        enemy.GetComponent<Character>().SetIncomingDamageContext(PlayerId, MoveType.Special, SourceType.Spell);
        enemy.GetComponent<Character>().TakeDamage(hit1Damage, true); //--here
        audioManager.PlaySFX(audioManager.lightattack, audioManager.lightAttackVolume);
    }

    public void SecondHit() // old and useless remove
    {
        enemy.GetComponent<Character>().SetIncomingDamageContext(PlayerId, MoveType.Special, SourceType.Spell);
        enemy.GetComponent<Character>().TakeDamage(hit2Damage, true); //--here
        audioManager.PlaySFX(audioManager.heavyattack, audioManager.lightAttackVolume);
    }

    public void ThirdHit()
    {
        enemy.GetComponent<Character>().TakeDamage(spellDamage2,true); //--here
        audioManager.PlaySFX(audioManager.klong, audioManager.doubleVol);

        // player state reset
        stayDynamic();
        ignoreUpdate = false;
        canRotate = true;

        // enemy state
        enemy.stayDynamic();
        enemy.AbilityEnabled();
        enemy.moveSpeed = OGMoveSpeed;
        enemy.Knockback(8f, .25f, false);

        // cd
        spellHit = false;
        ResetQuickPunch();
        animator.SetBool("ComboReady", false);

        OnCooldown(cooldown);
    }
    #endregion

    #region LightAttack
    public override void LightAttack()
    {
        TelemetryManager.Instance?.LogAction(PlayerId, "Quick");
        Unblock();
        isLightAttacking=true;
        animator.SetTrigger("QuickAttack");
    }
    public void QuickPunchDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle( attackPoint.position,  attackRange,  enemyLayer);

        if (hitEnemy != null)
        {
            TelemetryManager.Instance?.LogHitAttempt(PlayerId, enemy.PlayerId, MoveType.Quick);
            enemy.SetIncomingDamageContext(PlayerId, MoveType.Quick, SourceType.Melee);
            enemy.TakeDamageNoAnimation(lightDamage, true, false);
            audioManager.PlaySFX(audioManager.lightattack, audioManager.lightAttackVolume);
        }
        else
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Quick);
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
