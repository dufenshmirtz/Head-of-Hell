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
    

    //Charge
    protected bool charged = false;
    protected bool charging = false;
    private Coroutine chargeCoroutine;
    float chargeTime = 0.5f;
    protected int chargeDmg = 34;

    //Flags
    public bool isBlocking = false;
    public bool ignoreDamage = false;
  
    public bool isStatic = false;
    public bool casting = false;
    public bool canCast = true;
    public bool knocked = false;
    public bool canRotate = true;
    public bool usingAbility;
    public int currHealth;
    public bool isGrounded;

    //knockback

    protected float KBForce;
    protected float KBCounter;
    protected float KBTotalTime;
    protected bool knockfromright;
    protected bool knockbackXaxis;
    public bool knockable = true;

    //parry
    public bool canParry = true;
    protected bool safety = true;
    protected bool ignoreCounterOff = false;
    protected int parryDamage = 16;
    public bool counterIsOn = false;
    protected bool counterDone = false;

    //cd
    public bool onCooldown = false;
    public float cdTimer = 0f;

    public bool ignoreUpdate = false;

    //ps------------------------------------------
    protected TextMeshProUGUI P1Name, winner;
    protected string P2Name;
    string playerEnemysString;
    protected GameObject playAgainButton;
    protected GameObject mainMenuButton;
    protected GameObject saveReplayButton;
    protected Slider cooldownSlider;
    protected TextMeshProUGUI damageCounter;
    bool damageCounterReseted = true;
    protected helthbarscript healthbar;

    //basic stats
    public int characterID;
    public float moveSpeed = 4f; // Initialize moveSpeed
    protected float heavySpeed;
    protected float OGMoveSpeed;
    protected float jumpForce = 10f;
    public int maxHealth = 100;
    protected int heavyDamage = 14;
    protected int playerNum;
    protected float attackRange = 0.5f;
    protected float ogRange = 0.5f;

    //Stages
    string stageName;
    protected GameObject[] stages;

    private bool jumpAxisHeld;

    protected Transform attackPoint;



    protected LayerMask enemyLayer;

    protected bool ignoreMovement = false;
    protected bool ignoreSlow = false;
    public bool blockDisabled;
    public bool canAlterSpeed = true;

    //bar images
    protected Image cdbarimage;
    protected Sprite activeSprite, ogSprite;

    //Indicators
    protected GameObject blockDisabledIndicator;
    protected GameObject poison;
    protected GameObject Stack1Poison;
    protected GameObject Stack2Poison;
    protected GameObject Stack3Poison;
    protected GameObject quickAttackIndicator;
    protected GameObject stun;
    protected GameObject shield;

    protected TextMeshPro robberyCountIndicator;
    protected bool stunned = false;

    //movement keys
    public KeyCode up;
    public KeyCode down;
    public KeyCode left;
    public KeyCode right;
    public KeyCode lightAttack;
    public KeyCode heavyAttack;
    public KeyCode block;
    public KeyCode ability;
    public KeyCode charge;
    public KeyCode parry;

    protected CharacterSetup characterSetup;
    protected CharacterManager characterChoiceHandler;
    protected GameManager gameManager;

    bool preserveJump = false;

    protected bool damageShield = false;

    protected AudioClip blockSound;
    protected AudioClip characterJump;
    protected AudioClip chargeHitSound;
    protected AudioClip winQuip;


    public string playerString;

    protected bool quickDisable = false;
    protected bool heavyDisable = false;
    protected bool blockDisable = false;
    protected bool specialDisable = false;
    public bool chargeDisable = false;
    bool ignoreStats = false;


    public bool overrideDeath = false;

    //handling variables
    public int grounds = 0;
    int isonpad = 0;

    int controllerCount = 0;

    protected bool controller = false;
    protected bool chargeReset = false;
    protected bool chargeAttackActive = false;

    protected bool jumpDisabled = false;

    public bool chanChan;

    //teleport
    public bool justTeleported = false;

    public Transform spawn;

    protected float originalGravityScale;

    private bool debugControllers = false;

    // --- Episode spawn ---
    private Vector3 _spawnPos;
    public void SetSpawnPosition(Vector3 pos) => _spawnPos = pos;


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

            chanChan = loadedRuleset.chanChan;

            if (chanChan)
            {
                StartCoroutine(WaitForMaxHealth());
            }
            else
            {
                maxHealth = loadedRuleset.health;
                currHealth = maxHealth;
                healthbar.SetMaxHealth(maxHealth);
            }


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

        winner.gameObject.SetActive(false);

        playAgainButton.SetActive(false);
        mainMenuButton.SetActive(false);
        saveReplayButton.SetActive(false);

        OGMoveSpeed = moveSpeed;
        heavySpeed = moveSpeed / 2;

        cooldownSlider.maxValue = 1f;

        _spawnPos = transform.position;

        originalGravityScale = rb.gravityScale;


        //Disable Indicators
        shield.gameObject.SetActive(false);
        poison.gameObject.SetActive(false);
        Stack1Poison.gameObject.SetActive(false);
        Stack2Poison.gameObject.SetActive(false);
        Stack3Poison.gameObject.SetActive(false);
        stun.gameObject.SetActive(false);
        blockDisabledIndicator.gameObject.SetActive(false);
        robberyCountIndicator.gameObject.SetActive(false);

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
        parry = characterSetup.parry;
        //stages = characterSetup.stages;
        attackPoint = characterSetup.attackPoint;
        blockDisabledIndicator = characterSetup.blockDisabledIndicator;
        poison = characterSetup.poison;
        Stack1Poison = characterSetup.Stack1Poison;
        Stack2Poison = characterSetup.Stack2Poison;
        Stack3Poison = characterSetup.Stack3Poison;
        robberyCountIndicator = characterSetup.robberyCountIndicator;
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
        saveReplayButton = characterSetup.saveReplayButton;
        cooldownSlider = characterSetup.cooldownSlider;
        damageCounter = characterSetup.damageCounter;
        audioManager = characterSetup.audioManager;
        quickAttackIndicator = characterSetup.quickAttackIndicator;


        P1Name.text = characterChoiceHandler.GetCharacterName(1);
        P2Name = characterChoiceHandler.GetCharacterName(2);
        enemy = characterChoiceHandler.CharacterChoice(2);


        if (playerNum == 1)
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

        if (gameManager != null && gameManager.trainingMode)
        {
            animator.updateMode = AnimatorUpdateMode.AnimatePhysics; // δένει με FixedUpdate
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate; // ΔΕΝ σταματάει όταν δεν φαίνεται
        }
    }

    // inside Character (fields)
    protected IInputProvider input = new KeyboardInputProvider();

    // optional: allow swapping to AI later
    public void SetInput(IInputProvider provider)
    {
        input = provider ?? new KeyboardInputProvider();
    }

    // inside Character
    public IInputProvider GetInputProvider() => input;

    int ControllerNum(int pNum)
    {
        if (pNum == 1)
        {
            return 2;
        }
        else
        {
            return 1;
        }
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

        #if UNITY_EDITOR
        if (debugControllers)
        {
            for (int i = 1; i <= 4; i++)
            {
                for (int button = 0; button <= 19; button++)
                {
                    if (Input.GetKeyDown("joystick " + i + " button " + button))
                    {
                        Debug.Log("Joystick " + i + " Button " + button + " is pressed");
                    }
                }
            }
        }
        #endif

        float moveDirection = input.GetAxis("Horizontal" + playerString);
        // Running animations...
        if (Mathf.Abs(moveDirection) > 0.1f && !isStatic)
        {
            if (ignoreMovement || knocked) return;

            if (isGrounded)
            {
                animator.SetBool("IsRunning", true);
            }
            else
            {
                animator.SetBool("IsRunning", false);
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

        float v = input.GetAxis("Vertical" + playerString);
        bool axisUp = v > 0.5f;

        // Jumping
        if (input.GetKeyDown(up) || (axisUp && !jumpAxisHeld))
        {
            if (isGrounded && !jumpDisabled && !casting)
            {
                Jump();
            }
        }
        jumpAxisHeld = axisUp;

        // Heavy Punching
        if (input.GetKeyDown(heavyAttack) || (controller && Input.GetKeyDown("joystick "+ControllerNum(playerNum)+" button 2")))
        {
            if (!heavyDisable && !casting)
            {
                HeavyAttack();
            }
        }

        //Blocking
        if (input.GetKeyDown(block) || (controller && Input.GetKeyDown("joystick "+ControllerNum(playerNum)+" button 5")))
        {
            if (!blockDisable && !casting)
            {
                Block();
            }
        }
        else if (input.GetKeyUp(block) || (controller && Input.GetKeyUp("joystick "+ControllerNum(playerNum)+" button 5")))
        {
            if (!blockDisable && !casting)
            {
                Unblock();
            }
        }

        //ChargeAttack
        if (input.GetKeyDown(charge) || (controller && Input.GetKeyDown("joystick "+ControllerNum(playerNum)+" button 1")))
        {
            if (isGrounded && !chargeDisable && !casting)
            {
                ChargeAttack();
            }

        }

        //Get down from pad
        if (input.GetKeyDown(down) || (controller && input.GetAxis("Vertical" + playerString) < -0.5f))
        {
            Collider2D[] colliders = GetComponents<Collider2D>();

            colliders[3].enabled = false;
        }

        //LightAttack
        if (input.GetKeyDown(lightAttack) || (controller && Input.GetKeyDown("joystick "+ControllerNum(playerNum)+" button 0")))
        {
            if (!quickDisable && !casting)
            {
                moveSpeed = OGMoveSpeed;
                LightAttack();
            }
        }

        //Spells
        if (input.GetKeyDown(ability) || (controller && Input.GetKeyDown("joystick "+ControllerNum(playerNum)+" button 3")))
        {
            if (!onCooldown && canCast && !casting && !specialDisable)
            {
                Spell();
            }
        }

        //Parry
        if (input.GetKeyDown(parry) || (controller && Input.GetKeyDown("joystick "+ControllerNum(playerNum)+" button 4")))
        {
            if (canParry && canCast && !casting)
            {
                Parry();
            }
        }

        // Animation control for jumping, falling, and landing
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VerticalSpeed", rb.velocity.y);

    }

    IEnumerator WaitForMaxHealth()
    {
        // Wait until gameManager.maxHealth is no longer -1
        yield return new WaitUntil(() => gameManager.maxHealth != -1);

        // Now it's safe to assign the value
        maxHealth = gameManager.maxHealth;
        currHealth = maxHealth;
        healthbar.SetMaxHealth(maxHealth);
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
            grounds++;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {

        if (other.CompareTag("Ground"))
        {
            grounds--;

            if (grounds <= 0)
            {
                grounds=0;
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

            if (grounds <= 0)
            {
                grounds=0;
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
        colliders[5].enabled = false;
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
        chargeAttackActive = false;
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
    // cdTimer already set in OnCooldown()
    while (cdTimer > 0f)
    {
        cdTimer -= Time.deltaTime;
        UpdateCooldownSlider(duration);
        yield return null; // next frame
    }

    onCooldown = false;
    cdTimer = 0f;
    UpdateCooldownSlider(duration);
    }

    void UpdateCooldownSlider(float duration)
    {
        float progress = Mathf.Clamp01(1f - cdTimer / duration);
        cooldownSlider.value = progress;
    }

    public void EnemyAbilityBlock()
    {
        if (enemy == null) return;
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
        isBlocking = false;
        UpdateCooldownSlider(cd);

        lastAbilityCD = cd; //ML
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
                knocked = true;
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
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {
            enemy.StopPunching();
            if (!enemy.counterIsOn) {
                enemy.BreakCharge();
            }
            enemy.TakeDamage(chargeDmg, false);
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

    public virtual bool ChargeCheck(KeyCode charge)
    {
        if (charging)
        {
            chargeAttackActive = true;
            stayStatic();
            ignoreMovement = true;
            if (charged)
            {
                if (input.GetKeyUp(charge) || (controller && Input.GetKeyUp("joystick "+ControllerNum(playerNum)+" button 1")))
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
                if (input.GetKeyUp(charge) || (controller && Input.GetKeyUp("joystick "+ControllerNum(playerNum)+" button 1")))
                {
                    stayDynamic();
                    ignoreMovement = false;
                    animator.SetBool("Charging", false);
                    charging = false;
                    knockable = true;
                    animator.ResetTrigger("tookDmg");
                    chargeAttackActive = false;
                }
                return true;
            }
        }
        return false;
    }

    public void StopCHarge()
    {
        if (!casting)
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
        isBlocking = true;
        ResetQuickPunch();
    }
    public void Unblock()
    {
        animator.SetBool("cWalk", false);
        animator.SetBool("Crouch", false);
        isBlocking = false;

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
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        animator.SetBool("Jump", true);
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
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);
        return hitEnemy;
    }

    virtual public void HeavyAttackStart()
    {
        if (canAlterSpeed)
        {
            moveSpeed = heavySpeed;
            StartCoroutine(WaitAndSetSpeed());
        }
        animator.SetBool("IsHeavyAttacking", true);
    }

    public void HeavyAttackEnd()
    {
        if (canAlterSpeed)
        {
            moveSpeed = OGMoveSpeed;
        }
        animator.SetBool("IsHeavyAttacking", false);
    }

    private IEnumerator WaitAndSetSpeed()
    {

        yield return new WaitForSeconds(0.49f);  // Waits for 0.49 seconds
        moveSpeed = OGMoveSpeed;

    }

    void Parry() {
        counterIsOn = true;
        safety = true;
        canParry = false;
        knockable = false;
        stayStatic();
        StartCoroutine(ResetParry());
        StartCoroutine(CounterOffSafety());
        audioManager.PlaySFX(audioManager.counterScream, 2.5f);
        animator.SetTrigger("Parry");
    }

    IEnumerator ResetParry()
    {

        yield return new WaitForSeconds(5f);
        audioManager.PlaySFX(audioManager.rollReady, audioManager.lessVol);
        canParry = true;
    }

    public bool DetectCounter()
    {
        if (counterIsOn)
        {
            if (!counterDone)
            {
                Countered();
                return true;
            }
            return true;
        }

        return false;
    }

    public void CounterOff()
    {
        if (!ignoreCounterOff)
        {
            CounterVariablesOff();
            safety = false;
        }
        else
        {
            ignoreCounterOff = false;
        }

    }

    private IEnumerator CounterOffSafety()
    {
        yield return new WaitForSeconds(0.43f);
        if (!counterDone && safety)
        {
            CounterVariablesOff();
        }
    }

    public void CounterSuccessOff()
    {
        CounterVariablesOff();
    }

    public void Countered()
    {
        animator.SetTrigger("counterHit");
        audioManager.PlaySFX(audioManager.counterSucces, 1.5f);
        enemy.stayStatic();
        stayStatic();
        ignoreCounterOff = true;
        counterDone = true;
    }

    virtual public void DealCounterDmg()
    {
        enemy.StopPunching();
        enemy.BreakCharge();

        audioManager.PlaySFX(audioManager.counterClong, 0.5f);

        enemy.TakeDamage(parryDamage, true);

        stayDynamic();
        enemy.stayDynamic();

        enemy.Knockback(10f, .3f, false);

    }

    public void CounterVariablesOff()
    {
        counterDone = false;
        counterIsOn = false;
        knockable = true;
        stayDynamic();
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
        enemy = changeEnemy;
    }

    public bool AmICasting()
    {
        return casting;
    }

    public IEnumerator TeleportCooldown()
    {
        justTeleported = true;
        yield return new WaitForSeconds(2f);
        justTeleported = false;
    }

    public bool IsCooldownBarActiveSprite
    {
        get
        {
            return cdbarimage != null && activeSprite != null && cdbarimage.sprite == activeSprite;
        }
    }

    #endregion

    #region Passive and Damage

    virtual public void TakeDamage(int dmg, bool blockable, bool parryable = true)
    {
        if (parryable)
        {
            if (DetectCounter())
            {
                return;
            }
        }

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
                stayDynamic();
                ignoreMovement = false;
                chargeReset = false;
            }
            else
            {
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

                StartCoroutine(TriggerDamageCounter(5));
            }
            //if its light attack take no dmg

            if (dmg == chargeDmg)
            {
                currHealth -= dmg;
                Debug.Log("Took " + dmg + " damage.");
                healthbar.SetHealth(currHealth);
                moveSpeed = OGMoveSpeed;

                StartCoroutine(TriggerDamageCounter(dmg));
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

            StartCoroutine(TriggerDamageCounter(dmg));

            Debug.Log("Took " + dmg + " damage.");
        }

        if (currHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (overrideDeath) {
            return;
        }
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
            KeepStats(P2Name, P1Name.text);
        }
        else if (enemy.currHealth <= 0)
        {
            gameManager.RoundEndTie(playerNum);
        }
        else
        {
            gameManager.RoundEnd(playerNum, P2Name);
            KeepStats(P2Name, P1Name.text);
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

    virtual public void TakeDamageNoAnimation(int dmg, bool blockable, bool parryable = true)
    {
        if (parryable)
        {
            if (DetectCounter())
            {
                return;
            }
        }


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

            StartCoroutine(TriggerDamageCounter(dmg));

            Debug.Log("Took " + dmg + " damage.");
        }

        if (currHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator TriggerDamageCounter(int damage) {

        if (damageCounter.gameObject.activeSelf) {
            damage += int.Parse(damageCounter.text);
        }
        damageCounter.text = damage.ToString();
        damageCounter.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        damageCounter.gameObject.SetActive(false);

    }

    public void DealDamageToEnemy(int amount)
    {
        enemy.TakeDamageNoAnimation(amount, false);
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

    public void Slow(float time, float amount)
    {
        if (canAlterSpeed)
        {
            StartCoroutine(SlowCoroutine(time, amount));
        }
    }

    public IEnumerator SlowCoroutine(float time, float amount)
    {

        stun.gameObject.SetActive(true);
        moveSpeed = moveSpeed - amount;

        yield return new WaitForSeconds(time);

        moveSpeed = OGMoveSpeed;
        stun.gameObject.SetActive(false);
    }


    public void DisableBlock(bool whileKnocked)
    {
        Unblock();
        moveSpeed = OGMoveSpeed;
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
        animator.SetBool("IsHeavyAttacking", false);
    }

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    public bool IsEnemyClose()
    {
        return Vector3.Distance(this.transform.position, enemy.transform.position) <= 4f;
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
        if(this is Rager)
        {
            animator.SetBool("QuickPunch", false);
        }       
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
        if (!animator.GetBool("isDead")) {

            currHealth += amount;
            if (currHealth > maxHealth)
            {
                currHealth = maxHealth;
            }
            healthbar.SetHealth(currHealth);
        }

    }

    public void ChangeEnemy(Character newEnemy)
    {
        enemy = newEnemy;
    }

    #endregion

    #region PowerUps
    public void SpeedBoost()
    {
        moveSpeed = moveSpeed + 2;

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

    public void KeepStats(string winner, string loser)
    {
        if (winner == loser || ignoreStats)
        {
            return;
        }

        if (CharacterStatsManager.Instance != null)
        {
            CharacterStatsManager.Instance.KeepStats(winner, loser);
        }
        else
        {
            Debug.Log("Error.StatsManager not loaded properly.");
        }
    }

    #endregion

    #region RL
    // --- Public read-only state for RL ---
    public bool IsGrounded => isGrounded;
    public bool IsBlocking => isBlocking;
    public bool IsCasting => casting;
    public bool IsStunned => stunned;
    public bool IsKnocked => knocked;
    public bool IsCharging => charging;
    public bool IsCharged => charged;
    public bool OnAbilityCD => onCooldown;
    public bool CanCast => canCast;
    public bool CanParry => canParry;

    public bool QuickDisabled => quickDisable;
    public bool HeavyDisabled => heavyDisable;
    public bool BlockDisabled => blockDisable;
    public bool SpecialDisabled => specialDisable;
    public bool ChargeDisabled => chargeDisable;
    public bool JumpDisabled => jumpDisabled;

    // Optional: normalized ability cooldown (0=ready, 1=just used).
    // Store last used cooldown length so we can normalize.
    private float lastAbilityCD = 0f;
    public float AbilityCooldown01
    {
        get
        {
            if (!onCooldown || lastAbilityCD <= 0f) return 0f;
            // cdTimer counts down each second; normalize remaining
            return Mathf.Clamp01(cdTimer / lastAbilityCD);
        }
    }

    public virtual void ResetForEpisode()
    {
        // Position & physics
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.position = _spawnPos;
        rb.gravityScale = originalGravityScale;

        // Core flags
        ignoreUpdate = false;
        isBlocking = false;
        casting = false;
        stunned = false;
        knocked = false;
        knockable = true;
        justTeleported = false;

        // Charges / counters
        charging = false;
        charged = false;
        chargeAttackActive = false;
        counterIsOn = false;
        counterDone = false;
        canParry = true;

        // Movement & stats
        moveSpeed = OGMoveSpeed;
        currHealth = maxHealth;
        healthbar?.SetHealth(currHealth);
        jumpDisabled = false;
        blockDisabled = false;
        damageShield = false;

        // Cooldowns / UI bits
        // Stop any running coroutines that control timing (prevents “ghost timers”)
        StopAllCoroutines();
        onCooldown = false;
        cdTimer = 0f;
        cooldownSlider?.SetValueWithoutNotify(0f);
        cdbarimage.sprite = ogSprite;

        // Indicators off
        shield?.SetActive(false);
        poison?.SetActive(false);
        Stack1Poison?.SetActive(false);
        Stack2Poison?.SetActive(false);
        Stack3Poison?.SetActive(false);
        stun?.SetActive(false);
        blockDisabledIndicator?.SetActive(false);
        quickAttackIndicator?.SetActive(false);

        // Animator sanity
        if (animator == null) animator = GetComponent<Animator>();
        animator.Rebind();
        animator.Update(0f);
        animator.SetBool("isDead", false);
        animator.ResetTrigger("tookDmg");
        animator.ResetTrigger("ChargedHit");
        animator.SetBool("Charging", false);
        animator.SetBool("Casting", false);
        animator.SetBool("IsRunning", false);
        animator.SetBool("Crouch", false);

        // Re-enable gameplay
        stayDynamic();

        ActivateColliders();
        stayDynamic();
    }

    public virtual void ResetForEpisode2()
    {
        StopAllCoroutines();

        // Position & physics
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 1.8f;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (playerNum == 1)
        {
            transform.position=new Vector3(-7.3f,-2.50f,0f);
        }
        else
        {
            transform.position=new Vector3(7.4f,-2.50f,0f);
        }
        
        // Animator sanity
        if (animator == null) animator = GetComponent<Animator>();
        animator.Rebind();
        animator.Update(0f);
        animator.SetBool("isDead", false);
        animator.ResetTrigger("tookDmg");
        animator.ResetTrigger("ChargedHit");
        animator.SetBool("Charging", false);
        animator.SetBool("Casting", false);
        animator.SetBool("IsRunning", false);
        animator.SetBool("Crouch", false);
        //animator.SetBool("Jump", false);
        //animator.SetBool("isGrounded", true);

        // Core flags
        ignoreUpdate = false;
        isBlocking = false;
        casting = false;
        stunned = false;
        knocked = false;
        knockable = true;
        justTeleported = false;
        isonpad=0;
        onCooldown = false;
        ActivateColliders();
    }

    public void ClearDynamicScripts()
    {
        // Remove LupenSpirit if it exists
        LupenSpirit spirit = GetComponent<LupenSpirit>();
        if (spirit != null)
        {
            Destroy(spirit);
        }
        // Remove Lupen if it exists
        Lupen lup = GetComponent<Lupen>();
        if (lup != null)
        {
            Destroy(lup);
        }
    }

    #endregion
}
