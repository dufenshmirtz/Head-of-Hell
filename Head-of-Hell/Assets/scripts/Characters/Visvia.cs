using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visvia : Character
{
    int currentForm = 0;
    int maxForm = 2;
    Transform fartPoint;
    GameObject fartPrefab;
    bool fartin = true;
    bool trasformin = false;
    int cd1=5, cd2=5, cd3 = 5;
    Transform grabPoint;
    List<FartManager> activeFarts = new List<FartManager>(); // Track all farts
    bool explosionDamaged = false;

    float fartDuration = 1f;
    public int farted = 0;
    public bool fartcd = false;
    public int fartDamage = 2;
    float fartRate = 0.1f;

    public override void Start()
    {
        base.Start();

        fartPoint = resources.fartPoint;
        fartPrefab = resources.fart;
        grabPoint = resources.bellPoint;
        StartFarting();
    }

    #region HeavyAttack
    override public void HeavyAttack()
    {
        switch (currentForm)
        {
            case 0:
                animator.SetTrigger("HeavyAttack");
                audioManager.PlaySFX(audioManager.heavyswoosh, audioManager.heavySwooshVolume);
                break;
            case 1:
                animator.SetTrigger("Spell");
                break;
            case 2:
                animator.SetTrigger("QuickAttack");
                break;
        }
    }

    override public void DealHeavyDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {

            audioManager.PlaySFX(audioManager.katanaHit, 1f);
            enemy.TakeDamage(heavyDamage, true);
            if (!enemy.isBlocking)
            {
                enemy.Knockback(11f, 0.15f, true);
            }

        }
        else
        {
            audioManager.PlaySFX(audioManager.katanaSwoosh, 1f);
        }

    }

    public void GrabDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(grabPoint.position, attackRange * 3, enemyLayer);

        if (hitEnemy != null)
        {
            enemy.StopPunching();
            enemy.BreakCharge();
            enemy.TakeDamage(3, true);
            enemy.Knockback(8f, 0.3333f, true);

        }
        else
        {
            audioManager.PlaySFX(audioManager.bellPunch, audioManager.swooshVolume);
        }
    }

    public void DealQuickDamage()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {

            audioManager.PlaySFX(audioManager.katanaHit, 1f);
            enemy.TakeDamage(5, true);
            if (!enemy.isBlocking)
            {
                enemy.Knockback(20f, 0.3f, true);
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
        switch (currentForm)
        {
            case 0:
                StartCoroutine(FartBoost());
                break;
            case 1:
                StartCoroutine(PersistantFart());
                break;
            case 2:
                ExplodeAllFarts();
                break;
        }
    }

    IEnumerator FartBoost()
    {
        fartDamage = fartDamage * 3;
        moveSpeed= moveSpeed + 2;
        canAlterSpeed = false;

        yield return new WaitForSeconds(5f); // Wait for 1 second

        fartDamage = fartDamage / 2;
        moveSpeed= OGMoveSpeed;
        canAlterSpeed = true;
        OnCooldown(cd2);
    }

    void ExplodeAllFarts()
    {
        foreach (var fart in activeFarts)
        {
            if (fart != null)
            {
                fart.Explode(); // Trigger explosion
            }
        }
        activeFarts.Clear(); // Clear the list after explosion
    }

    public void FartExploded()
    {
        if (!explosionDamaged)
        {
            enemy.TakeDamage(10,true);
            explosionDamaged = true;
            OnCooldown(cd3);
            StartCoroutine(ResetExplosionCouroutine());
        }
    }

    IEnumerator ResetExplosionCouroutine()
    {
        yield return new WaitForSeconds((float)cd3); // Wait a bit before dealing damage

        explosionDamaged= false;
    }


    IEnumerator PersistantFart()
    {
        fartDuration=fartDuration*10;

        yield return new WaitForSeconds(3f); // Wait for 1 second

        fartDuration = fartDuration / 10;
        OnCooldown(cd2);
    }

    #endregion

    #region LightAttack
    public override void LightAttack()
    {
        if (!trasformin)
        {
            if (currentForm < maxForm)
            {
                currentForm++;
            }
            else
            {
                currentForm = 0;
            }

            // Change color based on the current form
            if (currentForm == 0)
            {
                StartCoroutine(ChangeColor(Color.blue));
            }
            if (currentForm == 1)
            {
                StartCoroutine(ChangeColor(Color.red));
            }
            if (currentForm == 2)
            {
                StartCoroutine(ChangeColor(Color.green));
            }
        }
    }

    // Coroutine to change color for 1 second
    IEnumerator ChangeColor(Color newColor)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color originalColor = sr.color;
        trasformin = true;
        sr.color = newColor; // Change to the new color

        yield return new WaitForSeconds(0.5f); // Wait for 1 second
        sr.color = originalColor; // Revert back to the original color
        trasformin = false;
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
    
    void StartFarting()
    {
        StartCoroutine(FartCoroutine());
    }

    void StopFarting()
    {
        fartin = false; // Stop loop
    }

    IEnumerator FartCoroutine()
    {
        while (fartin) // Infinite loop
        {
            GameObject fartObject = Instantiate(fartPrefab, fartPoint.position, Quaternion.identity);
            FartManager fartInstance = fartObject.GetComponent<FartManager>();
            fartObject.transform.SetParent(resources.trash);

            if (fartInstance != null)
            {
                fartInstance.owner = this; // Set owner
                fartInstance.duration = fartDuration;
                activeFarts.Add(fartInstance); // Store in list
            }

            yield return new WaitForSeconds(fartRate);
        }
    }

    //asss
    private Coroutine poisonCoroutine;
    public void FartDamage()
    {
        if (poisonCoroutine == null && !fartcd) // Ensure only one coroutine per enemy
        {
            poisonCoroutine = StartCoroutine(PoisonEnemy());
        }
    }

    public void FartEscape()
    {
        if (poisonCoroutine != null)
        {
            StopCoroutine(poisonCoroutine);
            StartCoroutine(FartCD());
            poisonCoroutine = null;
        }
    }

    IEnumerator PoisonEnemy()
    {
        while (true)
        {
            enemy.TakeDamageNoAnimation(fartDamage, false);
            yield return new WaitForSeconds(1f); // Apply damage every second
        }

    }

    IEnumerator FartCD()
    {
        fartcd = true;
        yield return new WaitForSeconds(0.8f); // Apply damage every second
        fartcd = false;
    }

    #endregion

}
