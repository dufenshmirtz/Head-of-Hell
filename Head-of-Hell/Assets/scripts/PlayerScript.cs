using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    //basic stats
    public float moveSpeed = 4f; // Initialize moveSpeed
    public float heavySpeed;
    public float OGMoveSpeed;
    public float jumpForce = 10f;
    public int maxHealth = 100;
    public int currHealth;

    public Animator animator;
    public Rigidbody2D rb;

    public Transform attackPoint;
    public float attackRange = 0.5f;
    public float ogRange = 0.5f;

    public Transform bellPoint;
    public Transform bellStunPoint;
    public Transform mirrorFireAttackPoint;
    public Transform fireAttackPoint;
    public Transform explosionPoint;

    public LayerMask enemyLayer;
    public bool isBlocking = false;
    public helthbarscript healthbar;
    public int heavyDMG = 14;
    public int lightDMG = 5;
    AudioManager audiomngr;
    public TextMeshProUGUI P1Name, winner;
    public string P2Name;
    public GameObject playAgainButton;
    public GameObject mainMenuButton;


    //handling variables
    int grounds = 0;
    public bool isGrounded;
    private bool isCrouching;
    int isonpad = 0;

    //cd
    protected bool onCooldown = false;
    public Slider cooldownSlider;
    protected float cdTimer = 0f;

    //bar images
    public Image cdbarimage;
    public Sprite activeSprite, ogSprite;

    public PlayerScript enemy;
   

    //player colors
    public Color SteelagerColor;
    public Color FinColor;
    public Color RagerColor;
    public Color SkiplerColor;
    public Color VanderColor;
    public Color LazyBigusColor;
    public Color lithraColor;
    public Color chibackColor;

    //player number
    public int player;

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

    //additional
    bool isStatic = false;
    bool casting = false;
    bool canCast = true;
    bool knocked = false;
    public bool canRotate = true;

    //knockback

    public float KBForce;
    public float KBCounter;
    public float KBTotalTime;
    public bool knockfromright;
    bool knockbackXaxis;
    public bool knockable = true;

    //charge attack
    public bool charged = false;
    public bool charging = false;
    private Coroutine chargeCoroutine;
    float chargeTime = 0.5f;
    int chargeDmg = 34;

    public bool ignoreUpdate = false;

    //shootin
    public GameObject bulletPrefab; // The bullet prefab
    public Transform firePoint; // The point from where the bullet will be instantiated
    public float bulletSpeed = 35f; // Speed of the bullet
    bool isShootin = false;
    public KeyCode shoot;

    Character character;
    Skipler skipler;
    Rager rager;
    Fin fin;
    Steelager steelager;
    LazyBigus bigus;
    Vander vander;
    Lithra lithra;
    Chiback chiback;

    public bool ignoreMovement = false;
    public bool ignoreDamage = false;
    public bool ignoreSlow = false;

    public CharacterAnimationEvents animEvents;
    public CharacterResources resources;
    private bool stunned=false;

    public GameObject[] stages;
    string stageName;

    public GameObject beam;
    //Indicators
    public GameObject blockDisabled;
    public GameObject poison;
    public GameObject quickAttackIndicator;
    public GameObject stun;

    void Start()
    {
        //basic variables assignment
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currHealth = maxHealth;
        healthbar.SetMaxHealth(maxHealth);

        stageName=PlayerPrefs.GetString("SelectedStage");
        if(stageName=="Stage 1"){
            stages[0].SetActive(true);
        }else if(stageName=="Stage 2"){
            stages[1].SetActive(true);
        }else if(stageName=="Stage 3"){
            stages[2].SetActive(true);
        }

        if (player == 1)  //player name assignment
        {
            P1Name.text = PlayerPrefs.GetString("Player1Choice");
        }
        else
        {
            P1Name.text = PlayerPrefs.GetString("Player2Choice");
        }

        winner.gameObject.SetActive(false);

        playAgainButton.SetActive(false);
        mainMenuButton.SetActive(false);

        OGMoveSpeed = moveSpeed;
        heavySpeed = moveSpeed / 2;

        cooldownSlider.maxValue = 1f;

        //character choice
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (P1Name.text == "Random")
        {
            PickRandomCharacter(spriteRenderer);
        }
        if (P1Name.text == "Steelager")
        {
            steelager = this.gameObject.AddComponent<Steelager>();
            character = steelager;
            spriteRenderer.color = SteelagerColor;
        }
        if (P1Name.text == "Vander")
        {
            vander = this.gameObject.AddComponent<Vander>();
            character = vander;
            spriteRenderer.color = VanderColor;
        }
        if (P1Name.text == "Rager")
        {
            rager = this.gameObject.AddComponent<Rager>();
            character = rager;
            spriteRenderer.color = RagerColor;
        }
        if (P1Name.text == "Skipler")
        {
            skipler = this.gameObject.AddComponent<Skipler>();
            character = skipler;
            spriteRenderer.color = SkiplerColor;
        }
        if (P1Name.text == "Fin")
        {
            fin = this.gameObject.AddComponent<Fin>();
            character = fin;
            spriteRenderer.color = FinColor;
        }
        if (P1Name.text == "Lazy Bigus")
        {
            bigus = this.gameObject.AddComponent<LazyBigus>();
            character = bigus;
            spriteRenderer.color = LazyBigusColor;
        }
        if (P1Name.text == "Lithra")
        {
            lithra = this.gameObject.AddComponent<Lithra>();
            character = lithra;
            spriteRenderer.color = lithraColor;
        }
        if (P1Name.text == "Chiback")
        {
            chiback = this.gameObject.AddComponent<Chiback>();
            character = chiback;
            spriteRenderer.color = chibackColor;
        }
        enemy.P2Name=P1Name.text;

        character.InitializeCharacter(this, audiomngr, resources);

        animEvents.SetCharacter(character);
    }

    void Update()
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
            animator.SetBool("knocked", false);
        }

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
            return;
        }

        if (!canRotate)
        {
            return;
        }
        //charge attack specifics
        if (character.ChargeCheck(charge))
        {
            return;
        }

        if (isStatic)
        {
            return;
        }

        // Running animations...
        if (Input.GetKey(left) || Input.GetKey(right))
        {
            if (ignoreMovement)
            {
                return;
            }

            if (knocked)
            {
                return;
            }

            if (isGrounded)
            {
                animator.SetBool("IsRunning", true);
            }

            float moveDirection = Input.GetKey(left) ? -1f : 1f; // -1 for A, 1 for D

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
        if (Input.GetKeyDown(up) && isGrounded)
        {
            character.Jump();
        }

        // Heavy Punching
        if (Input.GetKeyDown(heavyAttack))
        {
            character.HeavyAttack();
        }
        //Blocking
        if (Input.GetKeyDown(block) && !casting)
        {
            character.Block();
        }
        else if (Input.GetKeyUp(block))
        {
            character.Unblock();
        }

        //ChargeAttack
        if (Input.GetKeyDown(charge) && isGrounded)
        {
            character.ChargeAttack();

        }

        //Get gown from pad
        if (Input.GetKeyDown(down))
        {
            Collider2D[] colliders = GetComponents<Collider2D>();

            colliders[3].enabled = false;
        }

        //LightAttack
        if (Input.GetKeyDown(lightAttack))
        {
            moveSpeed = OGMoveSpeed;
            character.LightAttack();
        }

        //Spells
        if (Input.GetKeyDown(ability) && !onCooldown && canCast && !casting)
        {

            character.Spell();
        }

        // Animation control for jumping, falling, and landing
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VerticalSpeed", rb.velocity.y);

    }

    void OnTriggerEnter2D(Collider2D other)
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

            if (grounds == 0)
            {
                isGrounded = false;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(explosionPoint.position, attackRange*4);
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        //Gizmos.DrawWireSphere(bellStunPoint.position, attackRange / 3);

    }

    public void StopPunching()
    {
        animator.SetBool("isHeavypunching", false);
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

        enemy.stayStatic();

        audiomngr.StopMusic();
        audiomngr.PlaySFX(audiomngr.death, audiomngr.deathVolume);
        if (enemy.currHealth == maxHealth)
        {
            winner.text = "FLAWLESS\n" + P2Name + " prevails!";
        }
        else if (enemy.currHealth <= 0)
        {
            winner.text = "Tie?\nDEATH PREVAILS...";
        }
        else
        {
            winner.text = P2Name + " prevails!";
        }

        winner.gameObject.SetActive(true);

        // Enable play again and main menu buttons
        playAgainButton.SetActive(true);
        mainMenuButton.SetActive(true);
    }

    void PermaDeath()
    {
        animator.SetBool("permanentDeath", true);
        this.enabled = false;
    }

    private void Awake()
    {
        audiomngr = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

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


    public void Knockback(float force, float time, bool axis)
    {
        if (knockable)
        {
            knockbackXaxis = axis;
            audiomngr.PlaySFX(audiomngr.knockback, audiomngr.deathVolume);
            bool enemyOnRight = enemy.transform.position.x > this.transform.position.x;
            KBForce = force;
            KBCounter = time;
            knockfromright = enemyOnRight;
            animator.SetBool("knocked", true);
        }
    }

    private IEnumerator ResetKnockedAfterDelay(float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Reset the knocked variable
        knocked = false;
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

    public bool IsEnemyClose()
    {
        return Vector3.Distance(this.transform.position, enemy.transform.position) <= 3f;
    }

    public void blockBreaker()
    {
        isBlocking = false;
    }

    public void Knockable(bool update)
    {
        knockable = update;
    }

    public void UsingAbility(float cd)
    {
        casting = true;
        animator.SetBool("Casting", true);
        EnemyAbilityBlock();
        knockable = false;
        animator.SetBool("isUsingAbility", true);
        cdbarimage.sprite = activeSprite;
        UpdateCooldownSlider(cd);
    }

    public Collider2D[] GetColliders()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        return colliders;
    }
    public Character GetCharacter()
    {
        if (character != null)
        {
            return character;
        }
        else
        {
            return null;
        }
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
    public void PlayerBlock(bool blck)
    {
        isBlocking=blck;
    }

    public void Casting(bool castin)
    {
        casting=castin;
    }

    public void BreakCharge()
    {
        character.StopCHarge();
    }

    public IEnumerator InterruptMovement(float time)
    {
        rb.velocity = Vector2.zero; // Stop the enemy's movement
        ignoreMovement = true;

        yield return new WaitForSeconds(time);

        ignoreMovement = false;
    }

    public IEnumerator Stun(float time)
    {
        stun.gameObject.SetActive(true);
        rb.velocity = Vector2.zero; // Stop the enemy's movement
        stunned = true;

        yield return new WaitForSeconds(time);

        stunned = false;
        stun.gameObject.SetActive(false);
    }

    public void PickRandomCharacter(SpriteRenderer spriteRenderer)
    {
        // Array of possible character type names
        string[] characterTypeNames = {
            "Steelager", "Vander", "Rager",
            "Skipler", "Fin", "Lazy Bigus",
            "Lithra", "Chiback"
        };

        // Randomly select an index
        int randomIndex = UnityEngine.Random.Range(0, characterTypeNames.Length);

        P1Name.text=characterTypeNames[randomIndex];
    }

    public void TakeDamage(int dmg,bool blockable){
        character.TakeDamage(dmg,blockable);
    }

    public void DisableBlock(bool whileKnocked)
    {
        character.blockDisabled=true;
        blockDisabled.gameObject.SetActive(true);
    }

    public void EnableBlock()
    {
        character.blockDisabled = false;
        blockDisabled.gameObject.SetActive(false);
    }

    public void BeamHit()
    {
        bigus.BeamHitEnemy();
        print("@@@1");
    }
}
