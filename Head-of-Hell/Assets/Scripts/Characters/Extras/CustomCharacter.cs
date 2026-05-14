using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Custom : Character
{
    [SerializeField] private CustomCharacterData customData;

    private bool lightReady = true;
    private bool bombCharging;
    private bombScript activeBomb;

    private bool dashing;
    private readonly HashSet<Character> dashTargetsHit = new HashSet<Character>();
    private readonly HashSet<Character> beamTargetsHitThisCast = new HashSet<Character>();
    private readonly HashSet<Character> grabTargetsHitThisCast = new HashSet<Character>();

    public void Configure(CustomCharacterData data)
    {
        customData = data ?? CustomCharacterData.CreateDefault();
        customData.Sanitize();
    }

    public override void Start()
    {
        base.Start();
        Data.Sanitize();
    }

    private CustomCharacterData Data
    {
        get
        {
            if (customData == null)
            {
                customData = CustomCharacterData.CreateDefault();
            }

            customData.Sanitize();
            return customData;
        }
    }

    #region HeavyAttack
    public override void HeavyAttack()
    {
        TelemetryManager.Instance?.LogAction(PlayerId, "Heavy");
        animator.SetTrigger("HeavyAttack");
        audioManager.PlaySFX(audioManager.heavyswoosh, audioManager.heavySwooshVolume);
    }

    public override void DealHeavyDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        Character target = ResolveTargetFromHit(hitEnemies);

        if (target != null)
        {
            audioManager.PlaySFX(audioManager.explosion, audioManager.lessVol);
            ApplyTrackedHit(target, MoveType.Heavy, SourceType.Melee, heavyDamage, true);

            if (!target.isBlocking)
            {
                target.Knockback(11f, 0.15f, true);
            }
        }
        else
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Heavy);
            audioManager.PlaySFX(audioManager.explosion, audioManager.lessVol);
        }
    }
    #endregion

    #region Special
    public override void Spell()
    {
        string special = Data.specialAbilityId;

        switch (special)
        {
            case CustomCharacterAbilityIds.SpecialSteelagerExplosion:
                CastDelayedAreaSpecial(16f, 0.94f, GetResourcePoint(resources != null ? resources.explosionPoint : null), attackRange * 4f, 16, audioManager.bigExplosion);
                break;
            case CustomCharacterAbilityIds.SpecialVanderStab:
                StartCoroutine(VanderStabSpecial());
                break;
            case CustomCharacterAbilityIds.SpecialRagerCombo:
                StartCoroutine(RagerComboSpecial());
                break;
            case CustomCharacterAbilityIds.SpecialSkiplerDash:
                StartCoroutine(SkiplerDashSpecial());
                break;
            case CustomCharacterAbilityIds.SpecialFinFlash:
                StartCoroutine(FinFlashSpecial());
                break;
            case CustomCharacterAbilityIds.SpecialLazyBigusBeam:
                StartCoroutine(BigusBeamSpecial());
                break;
            case CustomCharacterAbilityIds.SpecialLithraBell:
                StartCoroutine(LithraBellSpecial());
                break;
            case CustomCharacterAbilityIds.SpecialChibackScytheJump:
                StartCoroutine(ChibackScytheJumpSpecial());
                break;
            case CustomCharacterAbilityIds.SpecialLupenShockwave:
                StartCoroutine(LupenShockwaveSpecial());
                break;
            case CustomCharacterAbilityIds.SpecialVisviaGrab:
                StartCoroutine(VisviaGrabSpecial());
                break;
            default:
                CastDelayedAreaSpecial(10f, 0.2f, attackPoint, attackRange * 1.5f, 12, audioManager.heavyattack);
                break;
        }
    }

    private void CastDelayedAreaSpecial(float cooldown, float delay, Transform point, float radius, int damage, AudioClip castSound)
    {
        TelemetryManager.Instance?.LogAction(PlayerId, "Special");
        UsingAbility(cooldown);
        animator.SetTrigger("Spell");
        if (castSound != null)
        {
            audioManager.PlaySFX(castSound, audioManager.doubleVol);
        }
        stayStatic();
        StartCoroutine(DelayedAreaDamage(cooldown, delay, point, radius, damage));
    }

    private IEnumerator DelayedAreaDamage(float cooldown, float delay, Transform point, float radius, int damage)
    {
        yield return new WaitForSeconds(delay);

        bool hit = DealAreaDamage(point.position, radius, MoveType.Special, SourceType.Spell, damage, true, true, 10f, 0.8f, false);
        if (!hit)
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Special);
        }

        FinishSpecial(cooldown);
    }

    private IEnumerator VanderStabSpecial()
    {
        const float cooldown = 12f;
        TelemetryManager.Instance?.LogAction(PlayerId, "Special");
        attackRange += 0.5f;
        UsingAbility(cooldown);
        animator.SetTrigger("Spell");

        yield return new WaitForSeconds(0.29f);

        List<Character> targets = TargetsInCircle(attackPoint.position, attackRange);
        if (targets.Count > 0)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                ApplyTrackedHit(targets[i], MoveType.Special, SourceType.Spell, 10, true, true, true, true, true);
                ApplySelfHeal(5);
            }
            audioManager.PlaySFX(audioManager.stabHit, audioManager.doubleVol);
        }
        else
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Special);
            audioManager.PlaySFX(audioManager.stab, audioManager.swooshVolume);
        }

        attackRange = ogRange;
        FinishSpecial(cooldown);
    }

    private IEnumerator RagerComboSpecial()
    {
        const float cooldown = 20f;
        TelemetryManager.Instance?.LogAction(PlayerId, "Special");
        UsingAbility(cooldown);
        animator.SetTrigger("Spell");

        yield return new WaitForSeconds(0.18f);

        Character target = ResolveTargetFromHit(Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer));
        if (target == null)
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Special);
            audioManager.PlaySFX(audioManager.swoosh, audioManager.swooshVolume);
            FinishSpecial(cooldown);
            yield break;
        }

        ApplyTrackedHit(target, MoveType.Special, SourceType.Spell, 0, true, true, false, true, true);
        target.BeginDeferredCritFeedback();
        stayStatic();
        ignoreUpdate = true;
        canRotate = false;
        target.stayStatic();
        target.blockBreaker();
        target.AbilityDisabled();
        target.Grabbed();

        int tickDamage = 15;
        float delayBetweenHits = 1.2f / tickDamage;
        for (int i = 0; i < tickDamage; i++)
        {
            if (target == null || !target.isActiveAndEnabled)
            {
                break;
            }

            target.SetIncomingDamageContext(PlayerId, MoveType.Special, SourceType.Spell);
            target.TakeDamageNoAnimation(1, false, false);
            yield return new WaitForSeconds(delayBetweenHits);
        }

        if (target != null && target.isActiveAndEnabled)
        {
            target.SetIncomingDamageContext(PlayerId, MoveType.Special, SourceType.Spell);
            target.TakeDamage(10, true);
            target.EndDeferredCritFeedback(true);
            target.stayDynamic();
            target.AbilityEnabled();
            target.moveSpeed = OGMoveSpeed;
            target.Knockback(8f, 0.25f, false);
        }

        FinishSpecial(cooldown);
    }

    private IEnumerator SkiplerDashSpecial()
    {
        const float cooldown = 8f;
        TelemetryManager.Instance?.LogAction(PlayerId, "Special");
        UsingAbility(cooldown);

        if (resources != null && resources.skiplerDouble != null && resources.skiplerPoint != null)
        {
            GameObject skipDouble = Instantiate(resources.skiplerDouble, resources.skiplerPoint.position, resources.skiplerPoint.rotation);
            Vector3 scale = skipDouble.transform.localScale;
            scale.x = transform.localScale.x;
            skipDouble.transform.localScale = scale;
        }

        IgnoreMovement(true);
        ignoreDamage = true;
        dashing = true;
        dashTargetsHit.Clear();

        float oldGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        Vector2 oldVelocity = rb.velocity;
        Collider2D[] colliders = GetColliders();
        bool[] colliderStates = CaptureColliderStates(colliders);
        ConfigureDashColliders(colliders);

        audioManager.PlaySFX(audioManager.dash, 1f);
        audioManager.PlaySFX(audioManager.dashGlitch, 1f);

        rb.velocity = new Vector2(GetMoveDirection(1f) * 40f, oldVelocity.y);
        animator.SetTrigger("Spell");

        yield return new WaitForSeconds(0.1f);

        if (dashTargetsHit.Count == 0)
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Special);
        }

        rb.velocity = oldVelocity;
        rb.gravityScale = oldGravity;
        RestoreColliderStates(colliders, colliderStates);
        dashing = false;
        FinishSpecial(cooldown);
    }

    private IEnumerator FinFlashSpecial()
    {
        const float cooldown = 8f;
        TelemetryManager.Instance?.LogAction(PlayerId, "Special");
        attackRange += 0.5f;
        audioManager.PlaySFX(audioManager.kalhaflash, 2f);
        UsingAbility(cooldown);
        animator.SetTrigger("Spell");

        yield return new WaitForSeconds(0.2f);

        Character target = ResolveTargetFromHit(Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer));
        if (target != null)
        {
            ApplyTrackedHit(target, MoveType.Special, SourceType.Spell, 1, true, true, false, true, true);
            target.Stun(0.7f);
        }
        else
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Special);
        }

        attackRange = ogRange;
        FinishSpecial(cooldown);
    }

    private IEnumerator BigusBeamSpecial()
    {
        const float cooldown = 20f;
        TelemetryManager.Instance?.LogAction(PlayerId, "Special");
        IgnoreUpdate(true);
        stayStatic();
        UsingAbility(cooldown);
        beamTargetsHitThisCast.Clear();
        animator.SetTrigger("Spell");
        audioManager.PlaySFX(audioManager.beam, audioManager.doubleVol);

        if (resources != null && resources.beam != null)
        {
            BeamScript beamScript = resources.beam.GetComponent<BeamScript>();
            if (beamScript != null)
            {
                beamScript.playa = this;
            }
            resources.beam.SetActive(true);
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
            DealCapsuleDamage((Vector2)attackPoint.position + Vector2.right * transform.localScale.x * 2f, new Vector2(5f, 0.8f), MoveType.Special, SourceType.Projectile, 10, true, true, 13f, 0.5f, true);
        }

        yield return new WaitForSeconds(1f);

        if (beamTargetsHitThisCast.Count == 0)
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Special);
        }

        beamTargetsHitThisCast.Clear();
        FinishSpecial(cooldown);
        IgnoreUpdate(false);
    }

    public void BeamHit(Character target)
    {
        if (target == null || !beamTargetsHitThisCast.Add(target))
        {
            return;
        }

        ApplyTrackedHit(target, MoveType.Projectile, SourceType.Projectile, 10, true, true, true, true, true);
        target.Knockback(13f, 0.5f, true);
        audioManager.PlaySFX(audioManager.beamHit, 1.8f);
        StartCoroutine(Poison(target, 2, 1f, 5));
    }

    private IEnumerator LithraBellSpecial()
    {
        const float cooldown = 10f;
        TelemetryManager.Instance?.LogAction(PlayerId, "Special");
        animator.SetTrigger("Spell");
        UsingAbility(cooldown);
        ignoreDamage = true;
        audioManager.PlaySFX(audioManager.swoosh, audioManager.doubleVol);

        yield return new WaitForSeconds(0.42f);

        Transform bellPoint = GetResourcePoint(resources != null ? resources.bellPoint : null);
        Transform stunPoint = GetResourcePoint(resources != null ? resources.bellStunPoint : null);
        List<Character> targets = TargetsInCircle(bellPoint.position, attackRange * 2f);
        List<Character> stunTargets = TargetsInCircle(stunPoint.position, attackRange / 3f);
        HashSet<Character> stunTargetSet = new HashSet<Character>(stunTargets);

        if (targets.Count > 0)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                Character target = targets[i];
                ApplyTrackedHit(target, MoveType.Special, SourceType.Spell, 10, true, true, true, true, true);
                if (stunTargetSet.Contains(target))
                {
                    target.Stun(0.8f);
                }
            }
            audioManager.PlaySFX(audioManager.heavyattack, audioManager.heavyAttackVolume);
            audioManager.PlaySFX(audioManager.bellSpell, 1.5f);
        }
        else
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Special);
            audioManager.PlaySFX(audioManager.bellPunch, audioManager.swooshVolume);
        }

        FinishSpecial(cooldown);
    }

    private IEnumerator ChibackScytheJumpSpecial()
    {
        const float cooldown = 10f;
        TelemetryManager.Instance?.LogAction(PlayerId, "Special");
        animator.SetTrigger("Spell");
        UsingAbility(cooldown);
        audioManager.PlaySFX(audioManager.sytheDash, audioManager.normalVol);
        audioManager.PlaySFX(audioManager.yeehaw, 3.5f);
        ignoreDamage = true;
        IgnoreUpdate(true);

        float moveDirection = GetMoveDirection(transform.localScale.x >= 0f ? 1f : -1f);
        rb.AddForce(new Vector2(moveDirection * 10f, 5f), ForceMode2D.Impulse);

        float elapsedTime = 0f;
        bool landed = false;
        while (elapsedTime < 1f)
        {
            Character target = ResolveTargetFromHit(Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer));
            if (target != null)
            {
                int damage = elapsedTime < 0.33f ? 5 : elapsedTime < 0.66f ? 10 : 15;
                ApplyTrackedHit(target, MoveType.Special, SourceType.Spell, damage, true, true, true, false, true);
                target.Knockback(11f, 0.25f, false);
                audioManager.PlaySFX(audioManager.sytheHit, 1f);
                audioManager.PlaySFX(audioManager.sytheSlash, 1f);
                animator.SetTrigger("SpellHit");
                rb.velocity = Vector2.zero;
                landed = true;
                break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (!landed)
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Special);
        }

        FinishSpecial(cooldown);
        IgnoreUpdate(false);
    }

    private IEnumerator LupenShockwaveSpecial()
    {
        const float cooldown = 15f;
        TelemetryManager.Instance?.LogAction(PlayerId, "Special");
        stayStatic();
        UsingAbility(cooldown);
        animator.SetTrigger("Spell");
        audioManager.PlaySFX(audioManager.transformation, 1.8f);

        yield return new WaitForSeconds(0.2f);

        List<Character> targets = TargetsInCircle(transform.position, 4f);
        if (targets.Count > 0)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                TelemetryManager.Instance?.LogHitAttempt(PlayerId, targets[i].PlayerId, MoveType.Special);
                targets[i].Knockback(9f, 0.5f, false);
            }
        }
        else
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Special);
        }

        FinishSpecial(cooldown);
    }

    private IEnumerator VisviaGrabSpecial()
    {
        const float cooldown = 10f;
        TelemetryManager.Instance?.LogAction(PlayerId, "Special");
        grabTargetsHitThisCast.Clear();
        UsingAbility(cooldown);
        animator.SetTrigger("Spell");

        yield return new WaitForSeconds(0.12f);
        DealGrabFrame();

        yield return new WaitForSeconds(0.47f);
        DealGrabFrame();

        if (grabTargetsHitThisCast.Count == 0)
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Special);
        }

        grabTargetsHitThisCast.Clear();
        FinishSpecial(cooldown);
    }

    private void DealGrabFrame()
    {
        Transform grabPoint = GetResourcePoint(resources != null ? resources.grabPoint : null);
        List<Character> targets = TargetsInCapsule(grabPoint.position, new Vector2(7f, 0.5f));

        for (int i = 0; i < targets.Count; i++)
        {
            Character target = targets[i];
            bool firstHit = grabTargetsHitThisCast.Add(target);
            ApplyTrackedHit(target, MoveType.Special, SourceType.Spell, 6, true, true, true, true, true, firstHit);
            target.Knockback(12f, 0.3333f, true);
            audioManager.PlaySFX(audioManager.stabHit, 2f);
        }
    }
    #endregion

    #region LightAttack
    public override void LightAttack()
    {
        string light = Data.lightAbilityId;

        if (light == CustomCharacterAbilityIds.LightSteelagerBomb && bombCharging)
        {
            if (activeBomb != null)
            {
                activeBomb.Explode();
            }
            return;
        }

        if (!lightReady)
        {
            return;
        }

        switch (light)
        {
            case CustomCharacterAbilityIds.LightSteelagerBomb:
                SteelagerBombLight();
                break;
            case CustomCharacterAbilityIds.LightVanderKatana:
                StartCoroutine(VanderKatanaLight());
                break;
            case CustomCharacterAbilityIds.LightRagerPunch:
                StartCoroutine(RagerPunchLight());
                break;
            case CustomCharacterAbilityIds.LightSkiplerBlink:
                StartCoroutine(SkiplerBlinkLight());
                break;
            case CustomCharacterAbilityIds.LightFinRoll:
                StartCoroutine(FinRollLight());
                break;
            case CustomCharacterAbilityIds.LightLazyBigusShot:
                BigusShotLight();
                break;
            case CustomCharacterAbilityIds.LightLithraAirSpin:
                StartCoroutine(LithraAirSpinLight());
                break;
            case CustomCharacterAbilityIds.LightChibackFire:
                StartCoroutine(ChibackFireLight());
                break;
            case CustomCharacterAbilityIds.LightLupenWhip:
                StartCoroutine(LupenWhipLight());
                break;
            case CustomCharacterAbilityIds.LightVisviaShotgun:
                StartCoroutine(VisviaShotgunLight());
                break;
            default:
                StartCoroutine(BasicQuickLight());
                break;
        }
    }

    private IEnumerator BasicQuickLight()
    {
        BeginLight("Quick");
        animator.SetTrigger("QuickAttack");
        yield return new WaitForSeconds(0.08f);
        Character target = ResolveTargetFromHit(Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer));
        if (target != null)
        {
            ApplyTrackedHit(target, MoveType.Quick, SourceType.Melee, 4, true);
            audioManager.PlaySFX(audioManager.lightattack, audioManager.lightAttackVolume);
        }
        else
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Quick);
            audioManager.PlaySFX(audioManager.swoosh, audioManager.swooshVolume);
        }
        StartCoroutine(ResetLight(1.5f));
    }

    private void SteelagerBombLight()
    {
        BeginLight("Quick");
        Transform spawnPoint = GetResourcePoint(resources != null ? resources.bombSpawner : null);
        Transform parent = resources != null ? resources.trash : null;
        Quaternion rotation = resources != null && resources.firePoint != null ? resources.firePoint.rotation : transform.rotation;

        if (resources == null || resources.bomb == null)
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Quick);
            StartCoroutine(ResetLight(2f));
            return;
        }

        bombCharging = true;
        audioManager.PlaySFX(audioManager.fuse, audioManager.normalVol);
        GameObject bomb = Instantiate(resources.bomb, spawnPoint.position, rotation);
        activeBomb = bomb.GetComponent<bombScript>();
        if (parent != null)
        {
            bomb.transform.SetParent(parent);
        }

        StartCoroutine(ResetBomb(2f));
    }

    private IEnumerator VanderKatanaLight()
    {
        BeginLight("Quick");
        animator.SetTrigger("QuickAttack");

        yield return new WaitForSeconds(0.08f);
        Character firstTarget = ResolveTargetFromHit(Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer));
        if (firstTarget != null)
        {
            ApplyTrackedHit(firstTarget, MoveType.Quick, SourceType.Melee, 3, true);
            audioManager.PlaySFX(audioManager.katanaHit, audioManager.lightAttackVolume);
        }
        else
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Quick);
            audioManager.PlaySFX(audioManager.katanaSwoosh, audioManager.swooshVolume);
        }

        yield return new WaitForSeconds(0.12f);
        Character secondTarget = ResolveTargetFromHit(Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer));
        if (secondTarget != null)
        {
            ApplyTrackedHit(secondTarget, MoveType.Quick, SourceType.Melee, 3, true);
            ApplySelfHeal(3);
            secondTarget.Knockback(10f, 0.15f, true);
            audioManager.PlaySFX(audioManager.katanaHit2, 1.5f);
        }

        StartCoroutine(ResetLight(2f, audioManager.katanaSeath));
    }

    private IEnumerator RagerPunchLight()
    {
        BeginLight("Quick");
        animator.SetTrigger("QuickAttack");
        yield return new WaitForSeconds(0.04f);

        Character target = ResolveTargetFromHit(Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer));
        if (target != null)
        {
            ApplyTrackedHit(target, MoveType.Quick, SourceType.Melee, 4, true, true, false, false, false, true);
            audioManager.PlaySFX(audioManager.lightattack, audioManager.lightAttackVolume);
        }
        else
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Quick);
            audioManager.PlaySFX(audioManager.swoosh, audioManager.swooshVolume);
        }

        StartCoroutine(ResetLight(0.1f, null));
    }

    private IEnumerator SkiplerBlinkLight()
    {
        BeginLight("Quick");
        IgnoreMovement(true);
        float oldGravity = rb.gravityScale;
        Vector2 oldVelocity = rb.velocity;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(GetMoveDirection(1f) * 10f, oldVelocity.y);
        audioManager.PlaySFX(audioManager.quickGlitch, 0.7f);
        animator.SetTrigger("QuickAttack");

        yield return new WaitForSeconds(0.14f);

        rb.velocity = oldVelocity;
        rb.gravityScale = oldGravity;
        IgnoreMovement(false);
        moveSpeed = OGMoveSpeed;

        Character target = ResolveTargetFromHit(Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer));
        if (target != null)
        {
            ApplyTrackedHit(target, MoveType.Quick, SourceType.Melee, 5, true);
            target.Knockback(10f, 0.15f, true);
            audioManager.PlaySFX(audioManager.dashHit, 0.8f);
            cdTimer -= 2f;
        }
        else
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Quick);
        }

        StartCoroutine(ResetLight(3f, audioManager.sworDashTada));
    }

    private IEnumerator FinRollLight()
    {
        BeginLight("Roll");
        IgnoreMovement(true);
        ignoreDamage = true;
        knockable = false;
        isRolling = true;
        float oldGravity = rb.gravityScale;
        Vector2 oldVelocity = rb.velocity;
        rb.gravityScale = 0f;

        Collider2D[] colliders = GetColliders();
        bool[] colliderStates = CaptureColliderStates(colliders);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
        if (colliders.Length > 5)
        {
            colliders[5].enabled = true;
        }

        audioManager.PlaySFX(audioManager.roll, 1f);
        rb.velocity = new Vector2(GetMoveDirection(1f) * 8f, oldVelocity.y);
        animator.SetTrigger("QuickAttack");

        yield return new WaitForSeconds(0.39f);

        rb.velocity = oldVelocity;
        RestoreColliderStates(colliders, colliderStates);
        rb.gravityScale = oldGravity;
        IgnoreMovement(false);
        ignoreDamage = false;
        knockable = true;
        isRolling = false;
        isBlocking = false;
        moveSpeed = OGMoveSpeed;

        StartCoroutine(ResetLight(2f, audioManager.rollReady));
    }

    private void BigusShotLight()
    {
        BeginLight("Projectile");

        if (resources == null || resources.bullet == null)
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Projectile);
            StartCoroutine(ResetLight(2f, audioManager.reload));
            return;
        }

        Transform firePoint = GetResourcePoint(resources.shootinPoint);
        Transform parent = resources.bulletParent;
        audioManager.PlaySFX(audioManager.volchSpit, audioManager.doubleVol);
        GameObject bullet = Instantiate(resources.bullet, firePoint.position, firePoint.rotation);

        Rigidbody2D bulletBody = bullet.GetComponent<Rigidbody2D>();
        if (bulletBody != null)
        {
            bulletBody.velocity = new Vector2(transform.localScale.x * 35f, 0f);
        }

        BulletScript bulletScript = bullet.GetComponent<BulletScript>();
        if (bulletScript != null)
        {
            bulletScript.Init(this);
        }

        if (parent != null)
        {
            bullet.transform.SetParent(parent);
        }

        Destroy(bullet, 2f);
        StartCoroutine(ResetLight(2f, audioManager.reload));
    }

    private IEnumerator LithraAirSpinLight()
    {
        BeginLight("Quick");
        IgnoreUpdate(true);
        audioManager.PlaySFX(audioManager.bellDash, 1f);

        float moveDirection = GetMoveDirection(transform.localScale.x >= 0f ? 1f : -1f);
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(new Vector2(moveDirection * 5f, 5f), ForceMode2D.Impulse);
        animator.SetTrigger("QuickAttack");

        float elapsedTime = 0f;
        bool landed = false;
        while (elapsedTime < 0.5f)
        {
            Character target = ResolveTargetFromHit(Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer));
            if (target != null)
            {
                ApplyTrackedHit(target, MoveType.Quick, SourceType.Melee, 3, true);
                target.Stun(0.5f);
                rb.velocity = Vector2.zero;
                rb.AddForce(new Vector2(-moveDirection * 2.5f, 5f), ForceMode2D.Impulse);
                animator.SetTrigger("Reverse");
                audioManager.PlaySFX(audioManager.bellDashHit, 1f);
                landed = true;
                break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (!landed)
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Quick);
        }

        while (!isGrounded)
        {
            yield return null;
        }

        IgnoreUpdate(false);
        StartCoroutine(ResetLight(3f, audioManager.sworDashTada));
    }

    private IEnumerator ChibackFireLight()
    {
        BeginLight("Quick");
        animator.SetTrigger("QuickAttack");
        audioManager.PlaySFX(audioManager.sytheGround, 1f);

        yield return new WaitForSeconds(0.1f);

        Transform backPoint = GetResourcePoint(resources != null ? resources.mirrorFireAttackPoint : null);
        Transform frontPoint = GetResourcePoint(resources != null ? resources.fireAttackPoint : null);
        List<Character> targets = ResolveTargetsFromHits(
            Physics2D.OverlapCircle(backPoint.position, attackRange, enemyLayer),
            Physics2D.OverlapCircle(frontPoint.position, attackRange, enemyLayer));

        if (targets.Count > 0)
        {
            audioManager.PlaySFX(audioManager.skiplaHeavyHit, 1f);
            for (int i = 0; i < targets.Count; i++)
            {
                Character target = targets[i];
                ApplyTrackedHit(target, MoveType.Quick, SourceType.Melee, 5, true);
                target.Knockback(onCooldown ? 3f : 15f, onCooldown ? 0.3f : 0.8f, true);
                target.DisableBlock(true);
                target.DisableJump(true);
                StartCoroutine(ResetBlockability(target, 1.1f));
            }
        }
        else
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Quick);
            audioManager.PlaySFX(audioManager.swoosh, 1f);
        }

        StartCoroutine(ResetLight(1f, audioManager.sworDashTada));
    }

    private IEnumerator LupenWhipLight()
    {
        BeginLight("Quick");
        animator.SetTrigger("QuickAttack");

        yield return new WaitForSeconds(0.1f);

        Transform whipPoint = GetResourcePoint(resources != null ? resources.wipPoint : null);
        Character target = ResolveTargetFromHit(Physics2D.OverlapCapsule(whipPoint.position, new Vector2(2f, 0.5f), CapsuleDirection2D.Horizontal, 0f, enemyLayer));
        audioManager.PlaySFX(audioManager.whip, audioManager.normalVol);

        if (target != null)
        {
            ApplyTrackedHit(target, MoveType.Quick, SourceType.Melee, 1, true);
            target.Slow(1.5f, 2f);
            StartCoroutine(SpeedUp(1.5f, 2f));
            audioManager.PlaySFX(audioManager.coinSound, audioManager.normalVol);
        }
        else
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Quick);
        }

        StartCoroutine(ResetLight(2f, audioManager.katanaSeath));
    }

    private IEnumerator VisviaShotgunLight()
    {
        BeginLight("Quick");
        animator.SetTrigger("QuickAttack");
        audioManager.PlaySFX(audioManager.shotgunBlast, 2f);
        canRotate = false;

        float direction = transform.localScale.x > 0f ? -1f : 1f;
        rb.velocity = new Vector2(direction * 15f, 6f);

        if (resources != null && resources.fart != null && resources.fartPoint != null)
        {
            Instantiate(resources.fart, resources.fartPoint.position, resources.fartPoint.rotation);
        }

        List<Character> targets = TargetsInCircle(attackPoint.position, 1f);
        if (targets.Count > 0)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                ApplyTrackedHit(targets[i], MoveType.Quick, SourceType.Melee, 6, true);
                if (!targets[i].isBlocking)
                {
                    targets[i].Knockback(10f, 0.2f, true);
                }
            }
        }
        else
        {
            TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Quick);
        }

        yield return new WaitForSeconds(0.2f);
        canRotate = true;

        yield return new WaitForSeconds(0.3f);
        rb.velocity = Vector2.zero;

        StartCoroutine(ResetLight(4f, audioManager.sworDashTada));
    }

    private IEnumerator ResetBomb(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        audioManager.PlaySFX(audioManager.lighter, audioManager.normalVol);
        bombCharging = false;
        activeBomb = null;
        lightReady = true;
        isLightAttacking = false;
        QuickAttackIndicatorEnable();
    }

    private IEnumerator ResetLight(float cooldown, AudioClip readySound = null)
    {
        yield return new WaitForSeconds(cooldown);
        if (readySound != null)
        {
            audioManager.PlaySFX(readySound, audioManager.lessVol);
        }
        lightReady = true;
        isLightAttacking = false;
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

    public override void TutorialRefreshQuickAttack()
    {
        if (dashing || ignoreMovement || isLightAttacking)
        {
            return;
        }

        lightReady = true;
        QuickAttackIndicatorEnable();
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        Character collidedCharacter = GetCharacterFromCollider(other);
        if (dashing
            && gameManager != null
            && gameManager.IsTwoVersusTwoMatch()
            && collidedCharacter != null
            && gameManager.ArePlayersTeammates(playerNum, collidedCharacter.GetPlayerNum()))
        {
            return;
        }

        Character target = ResolveTargetFromHit(other);
        if (dashing && target != null && dashTargetsHit.Add(target))
        {
            ApplyTrackedHit(target, MoveType.Special, SourceType.Spell, 10, true, true, true, true, true);
            audioManager.PlaySFX(audioManager.dashHit, 3f);
        }

        base.OnTriggerEnter2D(other);
    }

    private void BeginLight(string actionName)
    {
        Unblock();
        isLightAttacking = true;
        moveSpeed = OGMoveSpeed;
        lightReady = false;
        QuickAttackIndicatorDisable();
        TelemetryManager.Instance?.LogAction(PlayerId, actionName);
    }

    private void FinishSpecial(float cooldown)
    {
        attackRange = ogRange;
        ignoreDamage = false;
        ignoreUpdate = false;
        ignoreMovement = false;
        canRotate = true;
        rb.gravityScale = originalGravityScale;
        stayDynamic();

        if (!onCooldown)
        {
            OnCooldown(cooldown);
        }
    }

    private void ApplyTrackedHit(
        Character target,
        MoveType moveType,
        SourceType sourceType,
        int damage,
        bool blockable,
        bool parryable = true,
        bool canCrit = true,
        bool stopPunching = false,
        bool breakCharge = false,
        bool logHitAttempt = true)
    {
        if (target == null)
        {
            return;
        }

        SetEnemy(target);
        target.SetEnemy(this);

        if (logHitAttempt)
        {
            TelemetryManager.Instance?.LogHitAttempt(PlayerId, target.PlayerId, moveType);
        }

        if (stopPunching)
        {
            target.StopPunching();
        }

        if (breakCharge)
        {
            target.BreakCharge();
        }

        target.SetIncomingDamageContext(PlayerId, moveType, sourceType);
        target.TakeDamage(damage, blockable, parryable, canCrit);
    }

    private bool DealAreaDamage(
        Vector2 position,
        float radius,
        MoveType moveType,
        SourceType sourceType,
        int damage,
        bool blockable,
        bool breakCharge,
        float knockbackForce,
        float knockbackTime,
        bool xAxis)
    {
        List<Character> targets = TargetsInCircle(position, radius);
        if (targets.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            Character target = targets[i];
            ApplyTrackedHit(target, moveType, sourceType, damage, blockable, true, true, true, breakCharge);
            if (knockbackForce > 0f)
            {
                target.Knockback(knockbackForce, knockbackTime, xAxis);
            }
        }

        return true;
    }

    private bool DealCapsuleDamage(
        Vector2 position,
        Vector2 size,
        MoveType moveType,
        SourceType sourceType,
        int damage,
        bool blockable,
        bool breakCharge,
        float knockbackForce,
        float knockbackTime,
        bool xAxis)
    {
        List<Character> targets = TargetsInCapsule(position, size);
        if (targets.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            Character target = targets[i];
            ApplyTrackedHit(target, moveType, sourceType, damage, blockable, true, true, true, breakCharge);
            if (knockbackForce > 0f)
            {
                target.Knockback(knockbackForce, knockbackTime, xAxis);
            }
        }

        return true;
    }

    private List<Character> TargetsInCircle(Vector2 position, float radius)
    {
        return ResolveTargetsFromHits(Physics2D.OverlapCircleAll(position, radius, enemyLayer));
    }

    private List<Character> TargetsInCapsule(Vector2 position, Vector2 size)
    {
        return ResolveTargetsFromHits(Physics2D.OverlapCapsuleAll(position, size, CapsuleDirection2D.Horizontal, 0f, enemyLayer));
    }

    private Transform GetResourcePoint(Transform point)
    {
        return point != null ? point : attackPoint;
    }

    private float GetMoveDirection(float fallback)
    {
        float moveDirection = 0f;
        if (input.GetKey(left))
        {
            moveDirection = -1f;
        }
        else if (input.GetKey(right))
        {
            moveDirection = 1f;
        }
        else if (controller)
        {
            moveDirection = input.GetAxis("Horizontal" + playerString);
        }

        if (Mathf.Abs(moveDirection) < 0.01f)
        {
            moveDirection = fallback;
        }

        if (Mathf.Abs(moveDirection) < 0.01f)
        {
            return 0f;
        }

        return moveDirection > 0f ? 1f : -1f;
    }

    private bool[] CaptureColliderStates(Collider2D[] colliders)
    {
        bool[] states = new bool[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            states[i] = colliders[i].enabled;
        }

        return states;
    }

    private void ConfigureDashColliders(Collider2D[] colliders)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        for (int i = 3; i <= 5 && i < colliders.Length; i++)
        {
            colliders[i].enabled = true;
        }
    }

    private void RestoreColliderStates(Collider2D[] colliders, bool[] states)
    {
        int count = Mathf.Min(colliders.Length, states.Length);
        for (int i = 0; i < count; i++)
        {
            colliders[i].enabled = states[i];
        }
    }

    private void ApplySelfHeal(int amount)
    {
        currHealth = Mathf.Min(maxHealth, currHealth + amount);
        if (healthbar != null)
        {
            healthbar.SetHealth(currHealth);
        }
    }

    private IEnumerator Poison(Character target, int damageAmount, float interval, int times)
    {
        if (target == null)
        {
            yield break;
        }

        target.ActivatePoison(true);
        audioManager.PlaySFX(audioManager.poison, 2.5f);

        for (int i = 0; i < times; i++)
        {
            yield return new WaitForSeconds(interval);
            if (target == null || !target.isActiveAndEnabled)
            {
                yield break;
            }

            target.SetIncomingDamageContext(PlayerId, MoveType.PoisonTick, SourceType.Dot);
            target.TakeDamageNoAnimation(damageAmount, false, false);
        }

        if (target != null && target.isActiveAndEnabled)
        {
            target.ActivatePoison(false);
        }
    }

    private IEnumerator ResetBlockability(Character target, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (target == null || !target.isActiveAndEnabled)
        {
            yield break;
        }

        target.EnableBlock();
        target.DisableJump(false);
    }

    private IEnumerator SpeedUp(float time, float amount)
    {
        moveSpeed += amount;
        yield return new WaitForSeconds(time);
        moveSpeed = OGMoveSpeed;
    }
}
