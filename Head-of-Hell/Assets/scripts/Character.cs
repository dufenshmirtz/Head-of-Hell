using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Playables;
//using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static Unity.Collections.AllocatorManager;

public abstract class Character : MonoBehaviour
{
    protected Character enemy;
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
    public bool isBlocking=false;
    protected bool ignoreDamage=false;
    protected int heavyDamage = 14;

    //ps------------------------------------------
    protected TextMeshProUGUI P1Name, winner;
    protected string P2Name;
    string playerEnemysString;
    protected GameObject playAgainButton;
    protected GameObject mainMenuButton;
    protected Slider cooldownSlider;

    //basic stats
    public float moveSpeed = 4f; // Initialize moveSpeed
    protected float heavySpeed;
    protected float OGMoveSpeed;
    protected float jumpForce = 10f;
    protected int maxHealth = 100;
    protected int currHealth;
    protected bool isGrounded;

    protected int playerNum;

    protected helthbarscript healthbar;

    string stageName;
    protected GameObject[] stages;

    //additional
    protected bool isStatic = false;
    protected bool casting = false;
    protected bool canCast = true;
    protected bool knocked = false;
    protected bool canRotate = true;

    //knockback

    protected float KBForce;
    protected float KBCounter;
    protected float KBTotalTime;
    protected bool knockfromright;
    protected bool knockbackXaxis;
    protected bool knockable = true;

    protected Transform attackPoint;

    protected float attackRange = 0.5f;
    protected float ogRange = 0.5f;

    protected LayerMask enemyLayer;

    protected bool ignoreMovement = false;
    protected bool ignoreSlow = false;
    public bool blockDisabled;
    public bool canAlterSpeed = true;

    //bar images
    protected Image cdbarimage;
    protected Sprite activeSprite, ogSprite;

    //cd
    protected bool onCooldown = false;
    protected float cdTimer = 0f;

    public bool ignoreUpdate = false;

    //Indicators
    protected GameObject blockDisabledIndicator;
    protected GameObject poison;
    protected GameObject Stack1Poison;
    protected GameObject Stack2Poison;
    protected GameObject Stack3Poison;
    protected GameObject quickAttackIndicator;
    protected GameObject stun;
    protected GameObject shield;
    protected bool stunned = false;

    //movement keys
    protected KeyCode up;
    protected KeyCode down;
    protected KeyCode left;
    protected KeyCode right;
    protected KeyCode lightAttack;
    protected KeyCode heavyAttack;
    protected KeyCode block;
    protected KeyCode ability;
    protected KeyCode charge;

    protected CharacterSetup characterSetup;
    protected CharacterManager characterChoiceHandler;
    protected GameManager gameManager;

    bool preserveJump = false;

    protected bool damageShield = false;

    protected AudioClip blockSound;
    protected AudioClip characterJump;
    protected AudioClip chargeHitSound;
    protected AudioClip winQuip;


    protected string playerString;

    protected bool quickDisable = false;
    protected bool heavyDisable = false;
    protected bool blockDisable = false;
    protected bool specialDisable = false;
    protected bool chargeDisable = false;

    //handling variables
    int grounds = 0;
    int isonpad = 0;

    int controllerCount = 0;

    protected bool controller = false;
    protected bool chargeReset = false;
    bool chargeAttackActive = false;

    protected bool jumpDisabled = false;
    


    #region Base
    public virtual void Start()
    {
        characterSetup = GetComponent<CharacterSetup>();
        characterChoiceHandler = GetComponent<CharacterManager>();

        string[] connectedControllers = Input.GetJoystickNames();

        // Count how many controllers are connected (non-empty entries in the array)
        foreach (string controller in connectedControllers)
        {
            if (!string.IsNullOrEmpty(controller))
            {
                controllerCount++;
            }
        }

        InitializeCharacter();

        string json = PlayerPrefs.GetString("SelectedRuleset", null);

        if (!string.IsNullOrEmpty(json))
        {
            // Convert the JSON string back to a CustomRuleset object
            CustomRuleset loadedRuleset = JsonUtility.FromJson<CustomRuleset>(json);

            maxHealth =loadedRuleset.health;

            moveSpeed = loadedRuleset.playerSpeed;

            quickDisable = loadedRuleset.quickDisabled;
            heavyDisable = loadedRuleset.heavyDisabled;
            blockDisable = loadedRuleset.blockDisabled;
            specialDisable = loadedRuleset.specialDisabled;
            chargeDisable = loadedRuleset.chargeDisabled;

            if (loadedRuleset.hideHealth)
            {
                healthbar.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("No ruleset found in PlayerPrefs.");
        }

        //basic variables assignment
        rb = GetComponent<Rigidbody2D>();
        resources = GetComponent<CharacterResources>();
        currHealth = maxHealth;
        healthbar.SetMaxHealth(maxHealth);

        winner.gameObject.SetActive(false);

        playAgainButton.SetActive(false);
        mainMenuButton.SetActive(false);

        OGMoveSpeed = moveSpeed;
        heavySpeed = moveSpeed / 2;

        cooldownSlider.maxValue = 1f;

        //Disable Indicators
        shield.gameObject.SetActive(false);
        poison.gameObject.SetActive(false);
        Stack1Poison.gameObject.SetActive(false);
        Stack2Poison.gameObject.SetActive(false);
        Stack3Poison.gameObject.SetActive(false);
        stun.gameObject.SetActive(false);
        blockDisabledIndicator.gameObject.SetActive(false);
        
    }

    public void InitializeCharacter()
    {
        up = characterSetup.up;
        down = characterSetup.down;
        left = characterSetup.left;
        right = characterSetup.right;
        lightAttack = characterSetup.lightAttack;
        heavyAttack = characterSetup.heavyAttack;
        block = characterSetup.block;
        ability = characterSetup.ability;
        charge = characterSetup.charge;
        //stages = characterSetup.stages;
        attackPoint = characterSetup.attackPoint;
        blockDisabledIndicator = characterSetup.blockDisabledIndicator;
        poison = characterSetup.poison;
        Stack1Poison = characterSetup.Stack1Poison;
        Stack2Poison = characterSetup.Stack2Poison;
        Stack3Poison = characterSetup.Stack3Poison;
        stun = characterSetup.stun;
        enemyLayer = characterSetup.enemyLayer;
        shield = characterSetup.shield;
        gameManager = characterSetup.gameManager;
        cdbarimage = characterSetup.cdbarimage;
        activeSprite = characterSetup.activeSprite;
        ogSprite = characterSetup.ogSprite;
        playerNum = characterSetup.playerNum;
        healthbar = characterSetup.healthbar;
        P1Name = characterSetup.P1Name;
        winner = characterSetup.winner;
        playAgainButton = characterSetup.playAgainButton;
        mainMenuButton = characterSetup.mainMenuButton;
        cooldownSlider = characterSetup.cooldownSlider;
        audioManager = characterSetup.audioManager;
        quickAttackIndicator = characterSetup.quickAttackIndicator;


        P1Name.text = characterChoiceHandler.GetCharacterName(1);
        P2Name = characterChoiceHandler.GetCharacterName(2);
        enemy = characterChoiceHandler.CharacterChoice(2);

        if(playerNum==1)
        {
            playerString = "_P1";
            if (controllerCount >= 2)
            {
                controller = true;
            }
        }
        else if (playerNum == 2)
        {
            playerString = "_P2";
            if (controllerCount >= 1)
            {
                controller = true;
            }
        }

        animator = GetComponent<Animator>();
    }

    public virtual void Update()
    {
        //self knockback mechanic
        if (knockable)
        {
            if (KBCounter > 0)
            {
                if (knockfromright == true)
                {
                    if (!knockbackXaxis)
                    {
                        rb.velocity = new Vector2(-KBForce, KBForce);
                    }
                    else
                    {
                        rb.velocity = new Vector2(-KBForce, rb.velocity.y);
                    }
                }
                else
                {
                    if (!knockbackXaxis)
                    {
                        rb.velocity = new Vector2(KBForce, KBForce);
                    }
                    else
                    {
                        rb.velocity = new Vector2(KBForce, rb.velocity.y);
                    }
                }

                KBCounter -= Time.deltaTime;
                return;
            }
        }
        //animator.SetBool("knocked", false);  oldKnocked*

        if (ignoreUpdate)
        {
            return;
        }

        if (stunned)
        {
            animator.SetBool("IsRunning", false);
            rb.velocity = new Vector2(0, rb.velocity.y);

            animator.SetBool("cWalk", false);
            animator.SetTrigger("tookDmg");
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Hurt"))
            {
                // If not, reset the trigger so it doesn't stay set
                animator.ResetTrigger("tookDmg");
            }
            return;
        }

        if (!canRotate)
        {
            return;
        }
        //charge attack specifics
        if (ChargeCheck(charge))
        {
            return;
        }

        if (isStatic)
        {
            return;
        }

        for (int i = 1; i <= 4; i++)
        {
            // Check if any button on joystick i was pressed
            for (int button = 0; button <= 19; button++) // Joystick buttons range from 0 to 19
            {
                if (Input.GetKeyDown("joystick " + i + " button " + button))
                {
                    Debug.Log("Joystick " + i + " Button " + button + " is pressed");
                }
            }
        }

        float moveDirection = Input.GetAxis("Horizontal"+playerString);
        // Running animations...
        if (Mathf.Abs(moveDirection) > 0.1f)
        {
            if (ignoreMovement || knocked) return;

            if (isGrounded)
            {
                animator.SetBool("IsRunning", true);
            }
            else
            {
                animator.SetBool("IsRunning", false);
                animator.SetBool("IsJumping", false);
                animator.SetTrigger("Jump");
            }

            //float moveDirection = Input.GetKey(left) ? -1f : 1f; // -1 for A, 1 for D

            if (isBlocking)
            {
                animator.SetBool("cWalk", true);
                rb.velocity = new Vector2(moveDirection * heavySpeed, rb.velocity.y);
                transform.localScale = new Vector3(Mathf.Sign(moveDirection), 1, 1); // Flip sprite according to movement direction

            }
            else
            {
                rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);
                transform.localScale = new Vector3(Mathf.Sign(moveDirection), 1, 1); // Flip sprite according to movement direction
            }
        }
        else
        {
            animator.SetBool("IsRunning", false);
            rb.velocity = new Vector2(0, rb.velocity.y);

            animator.SetBool("cWalk", false);
        }

        // Jumping
        if (Input.GetKeyDown(up) || Input.GetAxis("Vertical"+playerString) > 0.5f)
        {
            if (isGrounded && !jumpDisabled)
            {
                Jump();
            }
        }

        // Heavy Punching
        if (Input.GetKeyDown(heavyAttack) || (controller && Input.GetButtonDown("HeavyAttack" + playerString)))
        {
            if(!heavyDisable)
            {
                HeavyAttack();
            }           
        }

        //Blocking
        if (Input.GetKeyDown(block) && !casting || (controller && Input.GetButtonDown("Block" + playerString)))
        {
            if (!blockDisable)
            {
                Block();
            }            
        }
        else if (Input.GetKeyUp(block) || (controller && Input.GetButtonUp("Block" + playerString)))
        {
            if (!blockDisable)
            {
                Unblock();
            }
        }

        //ChargeAttack
        if (Input.GetKeyDown(charge)|| (controller && Input.GetButtonDown("ChargeAttack" + playerString)))
        {
            if(isGrounded && !chargeDisable)
            {
                ChargeAttack();
            }
             
        }

        //Get down from pad
        if (Input.GetKeyDown(down) || (controller && Input.GetAxis("Vertical" + playerString) < -0.5f))
        {
            Collider2D[] colliders = GetComponents<Collider2D>();

            colliders[3].enabled = false;
        }

        //LightAttack
        if (Input.GetKeyDown(lightAttack) || (controller && Input.GetButtonDown("QuickAttack" + playerString)))
        {
            if (!quickDisable)
            {
                moveSpeed = OGMoveSpeed;
                LightAttack();
            }          
        }

        //Spells
        if (Input.GetKeyDown(ability) || (controller && controller && Input.GetButtonDown("Spell" + playerString)))
        {
            if (!onCooldown && canCast && !casting &&!specialDisable)
            {
                Spell();
            }           
        }

        // Animation control for jumping, falling, and landing
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VerticalSpeed", rb.velocity.y);

        /*if(rb.velocity.y != 0)
        {
            preserveJump = true;
            animator.SetBool("IsGrounded", !preserveJump);
        }
        else
        {
            preserveJump = false;
        }*/

    }

    #endregion

    #region Colliders
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.SetBool("Jump", false);
            grounds++;
        }

        if (other.CompareTag("Platform"))
        {
            isGrounded = true;
            animator.SetBool("Jump", false);
            isonpad++;
            grounds++;
            Collider2D[] colliders = GetComponents<Collider2D>();

            colliders[3].enabled = true;
        }

        if (other.CompareTag("Player"))  //--here
        {
            isGrounded = true;
            animator.SetBool("Jump", false);
            animator.SetBool("PlayerGrounded",true);
            grounds++;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {

        if (other.CompareTag("Ground"))
        {
            grounds--;

            if (grounds == 0)
            {
                isGrounded = false;
            }
        }

        if (other.CompareTag("Platform"))
        {
            isonpad--;
            grounds--;

            if (isonpad == 0)
            {
                isGrounded = false;
                Collider2D[] colliders = GetComponents<Collider2D>();

                colliders[3].enabled = false;
            }
        }

        if (other.CompareTag("Player"))  //--here
        {
            grounds--;
            animator.SetBool("PlayerGrounded", false);

            if (grounds == 0)
            {
                isGrounded = false;
            }
        }
    }
    public void ActivateColliders()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();

        foreach (Collider2D collider in colliders)
        {
            if (collider != colliders[3])
            {
                collider.enabled = true;
            }
        }

        colliders[4].enabled = false;
    }

    public void DeactivateColliders()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
    }

    public void stayStatic()
    {
        isStatic = true;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }
    }

    public void stayDynamic()
    {
        isStatic = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        chargeReset = false;
    }

    public Collider2D[] GetColliders()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        return colliders;
    }
    #endregion

    #region Abilities and Cooldowns
    public void OnCooldown(float cd)
    {
        ignoreDamage = false;
        ignoreMovement = false;

        EnemyAbilityEnable();
        knockable = true;
        cdbarimage.sprite = ogSprite;
        animator.SetBool("isUsingAbility", false);
        animator.SetBool("Casting", false);
        casting = false;

        // Start the cooldown timer
        cdTimer = cd;
        onCooldown = true;
        StartCoroutine(AbilityCooldown(cd));
    }

    public IEnumerator AbilityCooldown(float duration)
    {

        while (cdTimer > 0)
        {
            yield return new WaitForSeconds(1f);
            cdTimer -= 1f;
            UpdateCooldownSlider(duration); // Update the cooldown slider every second
        }

        // Reset the cooldown flag
        onCooldown = false;
        cdTimer = 0f;
    }

    void UpdateCooldownSlider(float duration)
    {
        float progress = Mathf.Clamp01(1f - cdTimer / duration);
        cooldownSlider.value = progress;
    }

    public void EnemyAbilityBlock()
    {
        enemy.AbilityDisabled();
    }

    public void EnemyAbilityEnable()
    {
        enemy.AbilityEnabled();
    }

    public void AbilityDisabled()
    {
        canCast = false;
    }

    public void AbilityEnabled()
    {
        canCast = true;
    }

    public void UsingAbility(float cd)
    {
        casting = true;
        ignoreDamage = true;
        knockable = false;
        animator.SetBool("Casting", true);
        EnemyAbilityBlock();
        animator.SetBool("isUsingAbility", true);
        cdbarimage.sprite = activeSprite;
        UpdateCooldownSlider(cd);
    }

    public void Casting(bool castin)
    {
        casting = castin;
    }
    #endregion

    #region Knockback
    public void Knockback(float force, float time, bool axis)
    {
        if (knockable)
        {
            knockbackXaxis = axis;
            audioManager.PlaySFX(audioManager.knockback, audioManager.lessVol);
            bool enemyOnRight = enemy.transform.position.x > this.transform.position.x;
            //This if must be removed when knockback tranfers to playerscript, its used for a Stellger Passive Function
            if (time == 0.3333f)
            {
                enemyOnRight = !enemyOnRight;
                knocked=true;
                StartCoroutine(ResetKnockedAfterDelay(0.3333f));
            }
            else
            {
                //animator.SetBool("knocked", true); oldKnocked*
                animator.SetTrigger("tookDmg");
            }
            KBForce = force;
            KBCounter = time;
            knockfromright = enemyOnRight;
        }
    }

    private IEnumerator ResetKnockedAfterDelay(float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Reset the knocked variable
        print("l");
        knocked = false;
    }

    public void Knockable(bool update)
    {
        knockable = update;
    }
    #endregion

    #region Purely Virtual
    public virtual void HeavyAttack() { }

    public virtual void DealHeavyDamage() { }

    public virtual void Spell() { }

    public virtual void LightAttack() { }
    #endregion

    #region ChargeAttack
    public virtual void ChargeAttack() {
        knockable = false;
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
            animator.SetTrigger("Charged");
        }
    }

    public virtual void DealChargeDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle( attackPoint.position,  attackRange,  enemyLayer);

        if (hitEnemy != null)
        {
            enemy.StopPunching();
            enemy.BreakCharge();
            enemy.TakeDamage(chargeDmg,false);
            enemy.Knockback(13f, 0.4f, false);
            audioManager.PlaySFX(audioManager.smash, audioManager.doubleVol);
            if (chargeHitSound != null)
            {
                audioManager.PlaySFX(chargeHitSound, 1.5f);
            }
        }
        else
        {
            if (chargeHitSound != null)
            {
                audioManager.PlaySFX(chargeHitSound, 1.5f);
            }
            else
            {
                audioManager.PlaySFX(audioManager.swoosh, audioManager.swooshVolume);
            }
            
        }
        chargeReset = true;
        knockable = true;
        charging = false;
        animator.SetBool("Casting", false);
        animator.SetBool("Charging", false);
    }

    public bool ChargeCheck(KeyCode charge)
    {
        if (charging)
        {
            chargeAttackActive = true;
            stayStatic();
            ignoreMovement = true;
            if (charged)
            {
                if (Input.GetKeyUp(charge) || (controller && Input.GetButtonUp("ChargeAttack" + playerString)))
                {
                    //stayDynamic();
                    animator.SetTrigger("ChargedHit");
                    charged = false;
                    animator.SetBool("Casting", true);
                    animator.ResetTrigger("tookDmg");
                }
                return true;
            }
            else
            {
                if (Input.GetKeyUp(charge) || (controller && Input.GetButtonUp("ChargeAttack" + playerString)))
                {
                    stayDynamic();
                    ignoreMovement = false;
                    animator.SetBool("Charging", false);
                    charging = false;
                    knockable = true;
                    animator.ResetTrigger("tookDmg");
                }
                return true;
            } 
        }
        return false;
    }

    public void StopCHarge()
    {
        stayDynamic();
        chargeAttackActive = false;
        ignoreMovement = false;
        animator.SetBool("Charging", false);
        charging = false;
        knockable = true;
        charged = false;
        animator.SetBool("Casting", false);
        animator.ResetTrigger("ChargedHit");
    }
    public void BreakCharge()
    {
        StopCHarge();
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
         PlayerBlock(true);
        isBlocking=true;
        ResetQuickPunch();
    }
    public void Unblock()
    {
        animator.SetBool("cWalk", false);
        animator.SetBool("Crouch", false);
         PlayerBlock(false);
        isBlocking=false;

        ResetQuickPunch();
    }

    public void blockBreaker()
    {
        isBlocking = false;
    } 

    public void PlayerBlock(bool blck)
    {
        isBlocking = blck;
    }
 
    #endregion

    #region General
    public void Jump()
    {
        rb.velocity = new Vector2( rb.velocity.x,  jumpForce);
        animator.SetBool("Jump",true);
        if (characterJump != null)
        {
            audioManager.PlaySFX(characterJump, audioManager.normalVol);
        }
        else
        {
            audioManager.PlaySFX(audioManager.jump, audioManager.jumpVolume);
        }
        

        ResetQuickPunch();
    }

    public Collider2D HitEnemy()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle( attackPoint.position,  attackRange,  enemyLayer);
        return hitEnemy;
    }

    virtual public void HeavyAttackStart()
    {
        if (canAlterSpeed)
        {
            moveSpeed = heavySpeed;
            StartCoroutine(WaitAndSetSpeed());
        }
        animator.SetBool("isHeavyAttacking", true);
    }

    public void HeavyAttackEnd()
    {
        if (canAlterSpeed)
        {
            moveSpeed = OGMoveSpeed;
        }       
        animator.SetBool("isHeavyAttacking", false);
    }

    private IEnumerator WaitAndSetSpeed()
    {

        yield return new WaitForSeconds(0.49f);  // Waits for 0.49 seconds
         moveSpeed =  OGMoveSpeed;

    }

    

    protected void QuickAttackIndicatorEnable()
    {
         quickAttackIndicator.SetActive(true);
    }

    protected void QuickAttackIndicatorDisable()
    {
         quickAttackIndicator?.SetActive(false);
    }

    public Character GetEnemy()
    {
        return enemy;
    }

    public void SetEnemy(Character changeEnemy)
    {
        enemy=changeEnemy;
    }

    public bool AmICasting()
    {
        return casting;
    }
    #endregion

    #region Passive and Damage

    virtual public void TakeDamage(int dmg, bool blockable)
    {

        if (ignoreDamage)
        {
            return;
        }


        if (dmg == chargeDmg)
        {
            StopCHarge();
        }

        if (chargeAttackActive)
        {
            if (chargeReset)
            {
                print("kolok1");
                stayDynamic();
                ignoreMovement = false;
                chargeReset = false;
            }
            else
            {
                print("kolok2");
                TakeDamageNoAnimation(dmg, blockable);
                return;
            }
            
        }

        ResetQuickPunch();

        if (isBlocking && blockable)
        {
            if (blockSound != null)
            {
                audioManager.PlaySFX(blockSound, audioManager.normalVol);
            }
            if (dmg == heavyDamage) //if its heavy attack take half the damage
            {
                currHealth -= 5;
                Debug.Log("Took 5 damage.");
                healthbar.SetHealth(currHealth);
            }
            //if its light attack take no dmg

            if (dmg == chargeDmg)
            {
                currHealth -= dmg;
                Debug.Log("Took " + dmg + " damage.");
                healthbar.SetHealth(currHealth);
                moveSpeed = OGMoveSpeed;
            }
        }
        else
        {
            if (damageShield)
            {
                damageShield = false;
                shield.gameObject.SetActive(false);
                return;
            }
            currHealth -= dmg;

            animator.SetTrigger("tookDmg");

            healthbar.SetHealth(currHealth);

            Debug.Log("Took " + dmg + " damage.");
        }

        if (currHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        animator.SetBool("isDead", true);
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }

        ignoreDamage = true;

        ActivateHealthBars(); //In case they are hidden

        enemy.Win();
        enemy.stayStatic();

        audioManager.StopMusic();
        audioManager.PlaySFX(audioManager.dearth, audioManager.doubleVol);
        if (enemy.currHealth == maxHealth)
        {
            gameManager.RoundEndFlawless(playerNum, P2Name);
            KeepStats(P2Name,P1Name.text);
        }
        else if (enemy.currHealth <= 0)
        {
            gameManager.RoundEndTie(playerNum);
        }
        else
        {
            gameManager.RoundEnd(playerNum, P2Name);
            KeepStats(P2Name,P1Name.text);
        }

    }

    public void PermaDeath()
    {
        animator.SetBool("permanentDeath", true);
        this.enabled = false;
    }

    public void Win()
    {
        if (winQuip != null)
        {
            audioManager.PlaySFX(winQuip, 2);
        }
        animator.SetTrigger("Win");
    }

    public void ActivateHealthBars()
    {
        healthbar.gameObject.SetActive(true);
        enemy.healthbar.gameObject.SetActive(true);
    }

    virtual public void TakeDamageNoAnimation(int dmg, bool blockable)
    {

        if (ignoreDamage)
        {
            return;
        }

        if (isBlocking && blockable)
        {
            if (blockSound != null)
            {
                audioManager.PlaySFX(blockSound, audioManager.normalVol);
            }           
        }
        else
        {
            if (damageShield)
            {
                damageShield = false;
                shield.gameObject.SetActive(false);
                return;
            }
            currHealth -= dmg;

            healthbar.SetHealth(currHealth);

            Debug.Log("Took " + dmg + " damage.");
        }

        if (currHealth <= 0)
        {
            Die();
        }
    }

    public void DealDamageToEnemy(int amount)
    {
        enemy.TakeDamageNoAnimation(amount,false);
    }

    public IEnumerator InterruptMovement(float time)
    {
        rb.velocity = Vector2.zero; // Stop the enemy's movement
        ignoreMovement = true;

        yield return new WaitForSeconds(time);

        ignoreMovement = false;
    }

    public void Stun(float time)
    {
        StartCoroutine(StunCoroutine(time));
    }

    public IEnumerator StunCoroutine(float time)
    {
        StopCHarge();

        stun.gameObject.SetActive(true);
        rb.velocity = Vector2.zero; // Stop the enemy's movement
        stunned = true;

        yield return new WaitForSeconds(time);

        stunned = false;
        stun.gameObject.SetActive(false);
    }

    public void Slow(float time,float amount)
    {
        if (canAlterSpeed)
        {
            StartCoroutine(SlowCoroutine(time, amount));
        }
    }

    public IEnumerator SlowCoroutine(float time, float amount)
    {

        stun.gameObject.SetActive(true);
        moveSpeed = moveSpeed-amount;

        yield return new WaitForSeconds(time);

        moveSpeed = OGMoveSpeed;
        stun.gameObject.SetActive(false);
    }


    public void DisableBlock(bool whileKnocked)
    {
        blockDisabled = true;
        blockDisabledIndicator.gameObject.SetActive(true);
    }

    public void DisableJump(bool choice)
    {
        jumpDisabled = choice;
    }

    public void EnableBlock()
    {
        blockDisabled = false;
        blockDisabledIndicator.gameObject.SetActive(false);
    }

    public void IgnoreMovement(bool boolean)
    {
        ignoreMovement = boolean;
    }

    public void IgnoreSlow(bool boolean)
    {
        ignoreSlow = boolean;
    }

    public void IgnoreUpdate(bool boolean)
    {
        ignoreUpdate = boolean;
    }

    public void ChangeSpeed(float speed)
    {
        moveSpeed = speed;
    }

    public void ChangeOGSpeed(float speed)
    {
        OGMoveSpeed = speed;
    }

    public void StopPunching()
    {
        animator.SetBool("isHeavyAttacking", false);
    }

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    public bool IsEnemyClose()
    {
        return Vector3.Distance(this.transform.position, enemy.transform.position) <= 3f;
    }

    public int GetCurrentHealth()
    {
        return currHealth;
    }

    public void SetCurrentHealth(int value)
    {
        currHealth = value;
        healthbar.SetHealth(value);
    }
    #endregion

    #region Special Functions
    //Rager
    public void ResetQuickPunch()
    {
        animator.ResetTrigger("punch2");
         animator.SetBool("QuickPunch", false);
    }

    public void Grabbed()
    {
        audioManager.PlaySFX(audioManager.grab, audioManager.heavyAttackVolume);
        animator.SetTrigger("grabbed");
    }

    public void StackPoison1(bool on)
    {
        Stack1Poison.gameObject.SetActive(on);
    }
    public void StackPoison2(bool on)
    {
        Stack2Poison.gameObject.SetActive(on);
    }
    public void StackPoison3(bool on)
    {
        Stack3Poison.gameObject.SetActive(on);
    }

    public bool IsPoisoned()
    {
        return poison.gameObject.activeSelf;
    }

    public void ActivatePoison(bool on)
    {
        poison.gameObject.SetActive(on);
    }

    public void ActivateStun(bool on)
    {
        poison.gameObject.SetActive(on);
    }
    public void ActivateblockBreaker(bool on)
    {
        blockDisabledIndicator.gameObject.SetActive(on);
    }

    public void Heal(int amount)
    {
        currHealth += amount;
        if (currHealth > maxHealth)
        {
            currHealth = maxHealth;
        }
        healthbar.SetHealth(currHealth);
    }

    public void ChangeEnemy(Character newEnemy)
    {
        enemy = newEnemy;
    }

    #endregion

    #region PowerUps
    public void SpeedBoost()
    {
        moveSpeed = moveSpeed +2;

        StartCoroutine(SpeedBoostCoroutine());
    }

    private IEnumerator SpeedBoostCoroutine()
    {
        yield return new WaitForSeconds(5f);

        moveSpeed = OGMoveSpeed;
    }

    public void DamageShield()
    {
        damageShield = true;
        shield.gameObject.SetActive(true);
    }

    public void RefreshCD()
    {
        cdTimer -= 5f;
    }

    public void HealUp()
    {
        StartCoroutine(HealCoroutine(2, 2f, 5));
    }

    private IEnumerator HealCoroutine(int amount, float interval, int times)
    {

        for (int i = 0; i < times; i++)
        {
            yield return new WaitForSeconds(interval);

            Heal(amount);
        }
        
    }

    public void KeepStats(string winner,string loser)
    {
        if (CharacterStatsManager.Instance!=null)
        {
            CharacterStatsManager.Instance.KeepStats(winner, loser);
        }
        else
        {
            Debug.Log("Error.StatsManager not loaded properly.");
        } 
    }
    #endregion
}
