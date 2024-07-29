using System.Collections;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
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
    protected Rigidbody2D rb;
    
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public float ogRange = 0.5f;
    public LayerMask enemyLayer;
    public bool isBlocking = false;
    public helthbarscript healthbar;
    public int heavyDMG = 21;
    public int lightDMG = 5;
    AudioManager audiomngr;
    public TextMeshProUGUI P1Name, winner;
    private string P2Name;
    public GameObject playAgainButton;
    public GameObject mainMenuButton;


    //Steelager ability
    bool FullGerosActive = false;
    float fullGerosspeedSaver;

    
    //handling variables
    int grounds = 0;
    private bool isGrounded;
    private bool isCrouching;
    int isonpad = 0;

    //cd
    protected bool onCooldown = false;
    public Slider cooldownSlider;
    protected float cdTimer = 0f;

    //bar images
    public Image cdbarimage;
    public Sprite activeSprite, ogSprite;

    //dash
    protected float dashingPower = 40f;
    protected float dashingTime = 0.1f;
    protected bool dashin = false;
    public PlayerScript enemy;
    protected bool dashhit = false;

    //counterShiet
    bool counterOn = false;
    bool countered = false;
    bool counterDone = false;
    bool ignoreCOff = false;

    

    //player colors
    public Color SteelagerColor;
    public Color FinColor;
    public Color RagerColor;
    public Color SkiplerColor;
    public Color VanderColor;
    public Color LazyBigusColor;

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

    //cooldowns
    public float SteelagerCd=30f;
    public float FinCd = 15f;
    public float RagerCd = 20f;
    public float SkiplerCd = 8f;
    public float LazyBigusCd = 30f;
    public float VanderCd = 20f;

    //abilty specifics
    public int FinDmg=30;
    public int RagerDmg1 = 2, RagerDmg2=7, RagerDmg3=10; //dmg1*4 , overall 25
    public float LazyBigusHeal=30f;
    float LazyBigusHealTime;
    public int VanderDmg =10,VanderHeal=5;
    public int SkiplerDmg = 10;

    //additional
    bool isStatic = false;
    bool casting = false;
    bool canCast = true;
    bool knocked = false;
    bool canRotate = true;

    //knockback

    public float KBForce;
    public float KBCounter;
    public float KBTotalTime;
    public bool knockfromright;
    bool knockbackXaxis;
    bool knockable = true;

    //charge attack
    bool charged = false;
    bool charging = false;
    private Coroutine chargeCoroutine;
    float chargeTime = 0.5f;
    int chargeDmg = 34;

    //bigus
    bool bigusActive = false;

    //shootin
    public GameObject bulletPrefab; // The bullet prefab
    public Transform firePoint; // The point from where the bullet will be instantiated
    public float bulletSpeed = 35f; // Speed of the bullet
    bool isShootin = false;
    public KeyCode shoot;

    //bombin
    public GameObject bombPrefab; // The bullet prefab
    public Transform bombPoint; // The point from where the bullet will be instantiated
    public Transform bombsParent;
    bool bombCharging = false;

    //katana
    int katanaDmg=3;
    bool katanaready = true;

    //sword-Dash
    public float swordDashPower=10f;
    public float swordDashTime = 0.14f;
    public bool swordDashReady = true;
    public bool swordDashin = false;
    public int swordDashDmg = 5;

    //roll
    float rollPower = 8f;
    float rollTime = 0.39f;
    bool rollReady = true;
    bool rollin = false;

    Character character;


    void Start()
    {
        //basic variables assignment
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currHealth = maxHealth;
        healthbar.SetMaxHealth(maxHealth);

        if (player == 1)  //player name assignment
        {
            P1Name.text = PlayerPrefs.GetString("Player1Choice");
            P2Name = PlayerPrefs.GetString("Player2Choice");

             
        }
        else
        {
            P1Name.text = PlayerPrefs.GetString("Player2Choice");
            P2Name = PlayerPrefs.GetString("Player1Choice");

        }
        Debug.Log(winner.text +" " + player);

        winner.gameObject.SetActive(false);
        
        playAgainButton.SetActive(false);
        mainMenuButton.SetActive(false);

        OGMoveSpeed = moveSpeed;
        heavySpeed = moveSpeed / 2;

        cooldownSlider.maxValue = 1f;

        //character choice
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (P1Name.text == "Steelager")
        {
            character = new Steelager();
            spriteRenderer.color = SteelagerColor;
        }
        if (P1Name.text == "Vander")
        {
            character = new Vander();
            spriteRenderer.color = VanderColor;
        }
        if (P1Name.text == "Rager")
        {
            character = new Rager();
            spriteRenderer.color = RagerColor;
        }
        if (P1Name.text == "Skipler")
        {
            character = new Skipler();
            spriteRenderer.color = SkiplerColor;
        }
        if (P1Name.text == "Fin")
        {
            character = new Fin();
            spriteRenderer.color = FinColor;
        }
        if (P1Name.text == "Lazy Bigus")
        {
            character = new LazyBigus();
            spriteRenderer.color = LazyBigusColor;
        }

        character.InitializeCharacter(this, audiomngr);
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

        

        if (bigusActive)
        {
            return;
        }

        if (!canRotate)
        {
            return;
        }
        //charge attack specifics
        if (charging)
        {
            stayStatic();
            if (charged)
            {
                
                if (Input.GetKeyUp(charge))
                {
                    stayDynamic();
                    animator.SetTrigger("ChargedHit");
                    charged = false;
                    animator.SetBool("Casting", true);
               
                }
                return;
            }
            if (Input.GetKeyUp(charge))
            {
                stayDynamic();
                animator.SetBool("Charging", false);
                charging = false;
                knockable = true;
            }

            
                
            return;
        }

        if (isStatic)
        {
            return;
        }



        // Running animations...
        if (Input.GetKey(left) || Input.GetKey(right))
        {
            if ((dashin))
            {
                return;
            }

            if (knocked)
            {
                return;
            }

            if (swordDashin)
            {
                return;
            }

            if (rollin)
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
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            animator.SetTrigger("Jump");
            audiomngr.PlaySFX(audiomngr.jump, audiomngr.jumpVolume);
        }

        // Heavy Punching
        if (Input.GetKeyDown(heavyAttack))
        {
            character.HeavyAttack();
        }
        //Blocking
        if (Input.GetKeyDown(block))
        {
            
            animator.SetTrigger("critsi");
            animator.SetBool("Crouch", true);
            isBlocking = true;

            ResetQuickPunch();
        }
        else if (Input.GetKeyUp(block))
        {
            animator.SetBool("cWalk", false);
            animator.SetBool("Crouch", false);
            isBlocking = false;
        }

        //ChargeAttack
        if (Input.GetKeyDown(charge) && isGrounded)
        {
            if(P1Name.text == "Skipler"){
                knockable = false;
                charging = true;
                animator.SetBool("Charging", true);
                animator.SetTrigger("SkiplerCharge");
                StartCharge();
            }else if (P1Name.text == "Vander")
            {
                knockable = false;
                charging = true;
                animator.SetBool("Charging", true);
                animator.SetTrigger("VanderCharge");
                StartCharge();
            }
            else if (P1Name.text == "Steelager")
            {
                knockable = false;
                charging = true;
                animator.SetBool("Charging", true);
                animator.SetTrigger("TNTCharge");
                StartCharge();
            }
            else if (P1Name.text == "Lazy Bigus")
            {
                knockable = false;
                charging = true;
                animator.SetBool("Charging", true);
                animator.SetTrigger("GunCharge");
                StartCharge();
            }
            else if (P1Name.text == "Rager")
            {
                knockable = false;
                charging = true;
                animator.SetBool("Charging", true);
                animator.SetTrigger("RagerCharge");
                StartCharge();
            }
            else if (P1Name.text == "Fin")
            {
                knockable = false;
                charging = true;
                animator.SetBool("Charging", true);
                animator.SetTrigger("FinCharge");
                StartCharge();
            }
            else
            {
                knockable = false;
                charging = true;
                animator.SetBool("Charging", true);
                animator.SetTrigger("StartCharge");
                StartCharge();
            }
            
        }

        //downkey
        if (Input.GetKeyDown(down))
        {
            Collider2D[] colliders = GetComponents<Collider2D>();

            colliders[3].enabled = false;
        }

        //Special attack
        if (Input.GetKeyDown(lightAttack))
        {

            moveSpeed = OGMoveSpeed;

            //Shoot
            if (P1Name.text == "Lazy Bigus")
            {
                if (!isShootin)
                {
                    animator.SetTrigger("Shoot");
                    StartCoroutine(ResetShooting());
                }
            }

            //Bomb
            if (P1Name.text == "Steelager")
            {
                if (!bombCharging)
                {
                    ThrowBomb();
                }
            }

            //QuickPunch
            if (P1Name.text == "Rager")
            {
                animator.SetTrigger("Punch2");            
             
            }

            //Katana
            if (P1Name.text == "Vander")
            {
                if (katanaready)
                {
                    animator.SetTrigger("katana");
                    katanaready = false;
                    StartCoroutine(ResetKatana());
                }
                
            }

            //SwordDash
            if (P1Name.text == "Skipler")
            {
                if (swordDashReady)
                {
                    StartCoroutine(SwordDash());
                }
                
            }

            //Dodge-Roll
            if (P1Name.text == "Fin")
            {
                if (rollReady)
                {
                    StartCoroutine(Roll());
                }

            }
        }

        //Spells

        if (Input.GetKeyDown(ability) && !onCooldown && canCast && !casting)
        {
            EnemyAbilityBlock();
            casting = true;
            animator.SetBool("Casting", true); 

            //Koubi Nikis
            if (P1Name.text == "Lazy Bigus")
            {
                bigusActive= true;
                cdbarimage.sprite = activeSprite;

                LazyBigusHealTime = LazyBigusHeal / 5f;
                //Activate abilty function

                audiomngr.PauseMusic();
                audiomngr.PlaySFX(audiomngr.lullaby, audiomngr.lullaVol);

                StartCoroutine(LazyBigusStateCoroutine(LazyBigusHealTime));


                UpdateCooldownSlider(60);
            }

            //Full Geros
            if (P1Name.text == "Steelager")
            {
                knockable = false;
                FullGerosActive = true;
                moveSpeed += 2f;
                fullGerosspeedSaver = OGMoveSpeed;
                OGMoveSpeed = moveSpeed;


                animator.SetTrigger("FullGeros");

                cdbarimage.sprite = activeSprite;

                
                audiomngr.PlaySFX(audiomngr.growl, audiomngr.heavyAttackVolume);
                

                Invulnerable();

                StartCoroutine(FullGerosDeactivateAfterDelay(10));

                UpdateCooldownSlider(60);
            }
            //Dufen-Dash
            if (P1Name.text == "Skipler")
            {
                knockable = false;
                animator.SetBool("isUsingAbility", true);
                cdbarimage.sprite = activeSprite;
                StartCoroutine(DashAnimation());
                UpdateCooldownSlider(2); // Start cooldown for T key
            }

            //counter
            if (P1Name.text == "Fin")
            {
                knockable = false;
                cdbarimage.sprite = activeSprite;
                audiomngr.PlaySFX(audiomngr.counterScream, audiomngr.counterVol);
                animator.SetTrigger("counter");
                counterOn = true;


            }

            //Combo
            if (P1Name.text == "Rager")
            {
                knockable = false;
                animator.SetBool("isUsingAbility", true);
                animator.SetTrigger("comboInit");

                ResetQuickPunch();

                UpdateCooldownSlider(30);
            }

            //Lifesteal Stab
            if (P1Name.text == "Vander")
            {
                knockable = false;
                animator.SetBool("isUsingAbility", true);
                cdbarimage.sprite = activeSprite;
                attackRange += 0.5f;
                animator.SetTrigger("Stab");
                UpdateCooldownSlider(25);
            }
        }



        if (countered)
        {
            Debug.Log("mouni");
            audiomngr.PlaySFX(audiomngr.counterSucces, audiomngr.doubleVol);
            enemy.stayStatic();
            stayStatic();
            animator.SetTrigger("counterHit");
            countered = false;
            UpdateCooldownSlider(2);
        }

        // Animation control for jumping, falling, and landing
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VerticalSpeed", rb.velocity.y);
    }

    public virtual void QuickAttack()
    {

    }

    public virtual void HeavyAttack()
    {
        if (P1Name.text == "Skipler")
        {
            animator.SetTrigger("skiplaHeavy");
            audiomngr.PlaySFX(audiomngr.skiplaHeavyCharge, audiomngr.heavySwooshVolume);

        }
        else if (P1Name.text == "Steelager")
        {
            animator.SetTrigger("BombPunch");
            audiomngr.PlaySFX(audiomngr.heavyswoosh, audiomngr.heavySwooshVolume);
        }
        else if (P1Name.text == "Vander")
        {
            animator.SetTrigger("VanderHeavy");
            audiomngr.PlaySFX(audiomngr.heavyswoosh, audiomngr.heavySwooshVolume);
        }
        else if (P1Name.text == "Lazy Bigus")
        {
            animator.SetTrigger("BigusHeavy");
            audiomngr.PlaySFX(audiomngr.heavyswoosh, audiomngr.heavySwooshVolume);
        }
        else if (P1Name.text == "Rager")
        {
            animator.SetTrigger("RagerHeavy");
            audiomngr.PlaySFX(audiomngr.heavyswoosh, audiomngr.heavySwooshVolume);
        }
        else
        {
            animator.SetTrigger("Punch");
            audiomngr.PlaySFX(audiomngr.heavyswoosh, audiomngr.heavySwooshVolume);
        }


        ResetQuickPunch();
    }

    public virtual void SpecialAttack()
    {

    }

    public virtual void ChargeAttack()
    {

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (dashin && other.CompareTag("Player") && !dashhit)  //--here
        {
            DealDashDmg();
            dashhit = true;
            
        }

        

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
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    public void StopPunching()
    {
        animator.SetBool("isHeavypunching",false);
    }

    public void DealDashDmg()
    {

        enemy.StopPunching();
        enemy.StopCHarge();
        enemy.TakeDamage(SkiplerDmg);
        audiomngr.PlaySFX(audiomngr.dashHit, audiomngr.heavyAttackVolume);


    }

    public void DealCounterDmg()
    {
        enemy.StopPunching();
        enemy.StopCHarge();
        audiomngr.PlaySFX(audiomngr.counterClong, audiomngr.doubleVol);
        enemy.TakeDamage(FinDmg);

        stayDynamic();
        enemy.stayDynamic();

        enemy.Knockback(10f,.3f, false);

        //end the counter
        CounterSuccessOff();
        

    }

    public void DealDmg()
    {
        character.DealDmg();
    }

    public void DealLightDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {

            //hitEnemy.GetComponent<PlayerScript>().TakeDamage(lightDMG); //--here
            enemy.TakeDamage(lightDMG);
            audiomngr.PlaySFX(audiomngr.lightattack, audiomngr.lightAttackVolume);
        }
        else
        {
            audiomngr.PlaySFX(audiomngr.swoosh, audiomngr.swooshVolume);
        }

        animator.SetBool("QuickPunch", false);

    }

    public void DealComboDmg()
    {

        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {
            enemy.StopPunching();
            enemy.StopCHarge();
            cdbarimage.sprite = activeSprite;
            //dmg and sound
            hitEnemy.GetComponent<PlayerScript>().TakeDamage(0); //--here
            audiomngr.PlaySFX(audiomngr.lightattack, audiomngr.lightAttackVolume);

            //playerState
            stayStatic();
            canRotate = false;
            //enemystate
            enemy.stayStatic();
            enemy.blockBreaker();
            enemy.AbilityDisabled();
            enemy.Grabbed();

            animator.SetBool("ComboReady", true);
        }
        else
        {
            audiomngr.PlaySFX(audiomngr.swoosh, audiomngr.swooshVolume);
            enemy.AbilityEnabled();

            animator.SetBool("isUsingAbility", false);
            ResetQuickPunch();
            //cd
            cdTimer = RagerCd;
            onCooldown = true;
            StartCoroutine(AbilityCooldown(RagerCd));

        }

    }

    public void DealChargeDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {
            enemy.StopPunching();
            enemy.StopCHarge();
            //hitEnemy.GetComponent<PlayerScript>().TakeDamage(lightDMG); //--here
            enemy.TakeDamage(chargeDmg);
            enemy.Knockback(13f,0.4f,false);
            audiomngr.PlaySFX(audiomngr.smash, audiomngr.doubleVol);
        }
        else
        {
            audiomngr.PlaySFX(audiomngr.swoosh, audiomngr.swooshVolume);
        }
        knockable = true;
        charging = false;
        animator.SetBool("Casting", false);
        animator.SetBool("Charging", false);
        stayDynamic();
    }

    public void Startcombo()
    {
        animator.SetTrigger("Combo");
    }
    public void firstHit()
    {
        Debug.Log(RagerDmg1);
        enemy.GetComponent<PlayerScript>().TakeDamage(RagerDmg1); //--here
        audiomngr.PlaySFX(audiomngr.lightattack, audiomngr.lightAttackVolume);
    }

    public void secondHit()
    {
        enemy.GetComponent<PlayerScript>().TakeDamage(RagerDmg2); //--here
        audiomngr.PlaySFX(audiomngr.heavyattack, audiomngr.lightAttackVolume);
    }

    public void thirdHit()
    {
        enemy.GetComponent<PlayerScript>().TakeDamage(RagerDmg3); //--here
        audiomngr.PlaySFX(audiomngr.klong, audiomngr.doubleVol);

        //player state
        stayDynamic();
        canRotate = true;


        //enemy state
        enemy.stayDynamic();
        enemy.AbilityEnabled();
        enemy.moveSpeed = OGMoveSpeed;
        enemy.Knockback(8f, .25f, false);

        //cd
        ResetQuickPunch();
        animator.SetBool("ComboReady", false);
        cdTimer = RagerCd;
        onCooldown = true;
        StartCoroutine(AbilityCooldown(RagerCd));

    }

    public void Grabbed()
    {
        audiomngr.PlaySFX(audiomngr.grab, audiomngr.heavyAttackVolume);
        animator.SetTrigger("grabbed");
    }

    public void DealStabDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {
            enemy.StopPunching();
            enemy.StopCHarge();
            //hitEnemy.GetComponent<PlayerScript>().TakeDamage(lightDMG); //--here
            enemy.TakeDamage(VanderDmg);
            currHealth += VanderHeal;
            if(currHealth>maxHealth)
            {
                currHealth = maxHealth;
            }
            healthbar.SetHealth(currHealth);
            audiomngr.PlaySFX(audiomngr.stabHit, audiomngr.doubleVol);
        }
        else
        {
            audiomngr.PlaySFX(audiomngr.stab, audiomngr.swooshVolume);
        }

        attackRange = ogRange;

        enemy.AbilityEnabled();
        //cd
        cdTimer = VanderCd;
        onCooldown = true;
        StartCoroutine(AbilityCooldown(VanderCd));

    }

    public void DealKatanaDmg1()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {

            //hitEnemy.GetComponent<PlayerScript>().TakeDamage(lightDMG); //--here
            enemy.TakeDamage(katanaDmg);
            audiomngr.PlaySFX(audiomngr.katanaHit, audiomngr.lightAttackVolume);
        }
        else
        {
            audiomngr.PlaySFX(audiomngr.katanaSwoosh, audiomngr.swooshVolume);
        }

    }

    public void DealKatanaDmg2()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {

            //hitEnemy.GetComponent<PlayerScript>().TakeDamage(lightDMG); //--here
            enemy.TakeDamage(katanaDmg);
            currHealth += katanaDmg;
            if (currHealth > maxHealth)
            {
                currHealth = maxHealth;
            }
            healthbar.SetHealth(currHealth);
            enemy.Knockback(10f, .15f, true);
            audiomngr.PlaySFX(audiomngr.katanaHit2, 1.5f);
        }
        else
        {
            audiomngr.PlaySFX(audiomngr.katanaSwoosh, audiomngr.swooshVolume);
        }

    }

    public void DealSwordDashDmg()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {
            enemy.TakeDamage(swordDashDmg);
            enemy.Knockback(10f, .15f, true);
            audiomngr.PlaySFX(audiomngr.sworDashHit, 0.8f);
        }
        else
        {
            audiomngr.PlaySFX(audiomngr.sworDashMiss, audiomngr.swooshVolume);
        }
    }

    public void stayStatic()
    {
        isStatic= true;
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

    public void TakeDamage(int dmg)
    {
        if (FullGerosActive)
        {
            return;
        }

        if (rollin)
        {
            return;
        }

        if (dashin)
        {
            return;
        }

        if (counterOn)
        {
            if (!counterDone) {
                countered = true;
            }
            counterDone = true;
            return;
        }

        ResetQuickPunch();

       if(dmg==chargeDmg)
        {
            Debug.Log("asstoass");
            StopCHarge();
        }

        if (isBlocking)
        {
            if (dmg == heavyDMG) //if its heavy attack take half the damage
            {
                currHealth -= 5;
                
                healthbar.SetHealth(currHealth);
            }
            //if its light attack take no dmg

            if(dmg== chargeDmg)
            {
                currHealth -= dmg;

                healthbar.SetHealth(currHealth);
            }

        }
        else
        {
            currHealth -= dmg;
            Debug.Log(currHealth + " helth");

            animator.SetTrigger("tookDmg");

            

            healthbar.SetHealth(currHealth);
        }


        if (currHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {

        Debug.Log(player);
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
        else if(enemy.currHealth<=0)
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

    public void HeavyPunchStart()
    {
        if(!FullGerosActive)
        {
            moveSpeed = heavySpeed;
            StartCoroutine(WaitAndSetSpeed());
        } 
        animator.SetBool("isHeavypunching", true);
        
    }

    public void HeavyPunchEnd()
    {
        moveSpeed = OGMoveSpeed;
        animator.SetBool("isHeavypunching", false);
    }


    private void Awake()
    {
        audiomngr = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    private IEnumerator WaitAndSetSpeed()
    {
        
        yield return new WaitForSeconds(0.49f);  // Waits for 0.49 seconds
        moveSpeed = OGMoveSpeed;
        
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
            audiomngr.PlaySFX(audiomngr.charged, 0.7f);
        }

    }

    IEnumerator LazyBigusStateCoroutine(float duration)
    {
        // Disable colliders and set the Rigidbody to Static
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

        // Set the player as dead
        animator.SetBool("isDead", true);


        // Loop for the specified duration
        //float elapsedTime = 0f;
        for (int i = 0; i < duration; i++)
        {

            // Regenerate health
            currHealth += 5;
            healthbar.SetHealth(currHealth);


            yield return new WaitForSeconds(1f);
        }

        if(currHealth>maxHealth)
        {
            currHealth = maxHealth;
        }

        animator.SetBool("isDead", false);

        animator.SetBool("permanentDeath", false);

        this.enabled = true;

        audiomngr.StartMusic();

        animator.SetTrigger("Angel");

        // Enable colliders and restore the Rigidbody
        //Rejuvenation();

    }

    void Invulnerable()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();

        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }

        isStatic = true;
    }

    private void Rejuvenation()
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
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        isStatic = false;

        if (P1Name.text == "Steelager")  //--here
        {
            animator.SetTrigger("animationOver");
        }

        if (P1Name.text == "Lazy Bigus")
        {
            moveSpeed = OGMoveSpeed;
            // Start the cooldown timer
            onCooldown = true;
            cdTimer = LazyBigusCd;
            EnemyAbilityEnable();
            StartCoroutine(AbilityCooldown(LazyBigusCd));
            bigusActive = false;
        }
    }

    IEnumerator FullGerosDeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        FullGerosActive = false;
        OGMoveSpeed = fullGerosspeedSaver;
        moveSpeed = OGMoveSpeed;
        EnemyAbilityEnable();

        // Start the cooldown timer
        cdTimer = SteelagerCd;
        onCooldown = true;


        StartCoroutine(AbilityCooldown(SteelagerCd));
    }
    public IEnumerator AbilityCooldown(float duration)
    {
            knockable = true;
            cdbarimage.sprite = ogSprite;  //change the bar appearance to normal
            animator.SetBool("isUsingAbility", false);
            animator.SetBool("Casting", false);
            while (cdTimer > 0)
            {
                yield return new WaitForSeconds(1f);
                cdTimer -= 1f;
                UpdateCooldownSlider(duration); // Update the cooldown slider every second
            }
        
            // Reset the cooldown flag
            casting = false;
            onCooldown = false;
            cdTimer = 0f;
    }

    void UpdateCooldownSlider(float duration)
    {
        float progress = Mathf.Clamp01(1f - cdTimer / duration);
        cooldownSlider.value = progress;
    }

    IEnumerator DashAnimation()
    {
        dashin = true;
        // Store the original gravity scale
        float ogGravityScale = rb.gravityScale;

        // Disable gravity while dashing
        rb.gravityScale = 0f;

        // Store the current velocity
        Vector2 currentVelocity = rb.velocity;

        // Determine the dash direction based on the input
        float moveDirection = Input.GetKey(left) ? -1f : 1f; ;

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
        colliders[3].enabled = true;
        colliders[4].enabled = true;


        audiomngr.PlaySFX(audiomngr.dash, 1);
        // Calculate the dash velocity
        Vector2 dashVelocity = new Vector2(moveDirection * dashingPower, currentVelocity.y);

        // Apply the dash velocity
        rb.velocity = dashVelocity;

        // Trigger the dash animation
        animator.SetTrigger("Dash");



        // Wait for the dash duration
        yield return new WaitForSeconds(dashingTime);

        // Reset the velocity after the dash
        rb.velocity = currentVelocity;

        // Reset the gravity scale
        rb.gravityScale = ogGravityScale;

        dashin = false;


        foreach (Collider2D collider in colliders)
        {
            if (collider != colliders[3])
            {
                collider.enabled = true;
            }
        }
        colliders[3].enabled = false;
        colliders[4].enabled = false;

        dashhit = false;

        // Start the cooldown timer
        cdTimer = SkiplerCd;
        onCooldown = true;

        EnemyAbilityEnable();

        StartCoroutine(AbilityCooldown(SkiplerCd));

        

    }

    public void Knockback(float force,float time,bool axis)
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
            Debug.Log(enemyOnRight + " knockback!");
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

    public void CounterOff()
    {

        
        if (!ignoreCOff)
        {
            
            // Start the cooldown timer
            cdTimer = FinCd-5;
            onCooldown = true;
            EnemyAbilityEnable();
            StartCoroutine(AbilityCooldown(FinCd-5));
        }
        else
        {
            ignoreCOff = false;
        }
        counterDone = false;
        counterOn = false;
        cdbarimage.sprite = ogSprite;

        

    }

    public void CounterSuccessOff()
    {
        
        

        ignoreCOff = true;

        counterDone = false;
        counterOn = false;
        cdbarimage.sprite = ogSprite;

        cdTimer = FinCd;
        onCooldown = true;
        EnemyAbilityEnable();
        StartCoroutine(AbilityCooldown(FinCd));

        

    }

    public void StopCHarge()
    {
        Debug.Log(player + " jojo");
        stayDynamic();
        animator.SetBool("Charging", false);
        charging = false;
        knockable = true;
        charged = false;
        animator.SetBool("Casting", false);
        animator.ResetTrigger("ChargedHit");
    }

    public bool IsEnemyClose()
    {
        return Vector3.Distance(this.transform.position, enemy.transform.position) <= 3f;
    }

    public void blockBreaker()
    {
        isBlocking = false;
    }

    public void BigusKnock()
    {
        if (IsEnemyClose())
        {
            enemy.StopCHarge();
            enemy.Knockback(10f, .3f, false);
        }
    }

    void Shoot()
    {
        audiomngr.PlaySFX(audiomngr.shoot, audiomngr.normalVol);
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(transform.localScale.x * bulletSpeed, 0); // Shoots in the direction the character is facing

        Destroy(bullet, 2f);
    }

    public void firstShootFrame()
    {
        isShootin = true;
    }

    IEnumerator ResetShooting()
    {
        
        yield return new WaitForSeconds(2f);
        audiomngr.PlaySFX(audiomngr.reload, audiomngr.normalVol);
        isShootin = false;
    }

    void ThrowBomb()
    {
        bombCharging = true;
        audiomngr.PlaySFX(audiomngr.fuse, audiomngr.normalVol);
        GameObject bomb = Instantiate(bombPrefab, bombPoint.position, firePoint.rotation);
        Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();
        bomb.transform.SetParent(bombsParent);
        StartCoroutine(ResetBomb());

    }

    public void Boom()
    {
        audiomngr.PlaySFX(audiomngr.explosion, audiomngr.lessVol);
    }

    IEnumerator ResetBomb()
    {

        yield return new WaitForSeconds(2f);
        audiomngr.PlaySFX(audiomngr.lighter, audiomngr.normalVol);
        bombCharging = false;
    }

    IEnumerator ResetKatana()
    {

        yield return new WaitForSeconds(2f);
        audiomngr.PlaySFX(audiomngr.katanaSeath, audiomngr.doubleVol);
        katanaready = true;
    }

    IEnumerator SwordDash()
    {
        swordDashin = true;
        
        // Store the original gravity scale
        float ogGravityScale = rb.gravityScale;

        // Disable gravity while dashing
        rb.gravityScale = 0f;

        // Store the current velocity
        Vector2 currentVelocity = rb.velocity;

        // Determine the dash direction based on the input
        float moveDirection = Input.GetKey(left) ? -1f : 1f; ;


        audiomngr.PlaySFX(audiomngr.sworDashin, 1);
        // Calculate the dash velocity
        Vector2 sworddashVelocity = new Vector2(moveDirection * swordDashPower, currentVelocity.y);

        // Apply the dash velocity
        rb.velocity = sworddashVelocity;

        // Trigger the dash animation
        animator.SetTrigger("swordDash");



        // Wait for the dash duration
        yield return new WaitForSeconds(swordDashTime);

        // Reset the velocity after the dash
        rb.velocity = currentVelocity;

        // Reset the gravity scale
        rb.gravityScale = ogGravityScale;

        swordDashReady = false;

        swordDashin = false;

        StartCoroutine(ResetSwordDash());

        
    }

    IEnumerator ResetSwordDash()
    {

        yield return new WaitForSeconds(3f);
        audiomngr.PlaySFX(audiomngr.sworDashTada, audiomngr.lessVol);
        swordDashReady = true;
    }

    IEnumerator Roll()
    {
        rollin = true;
        knockable = false;

        // Store the original gravity scale
        float ogGravityScale = rb.gravityScale;

        // Disable gravity while dashing
        rb.gravityScale = 0f;

        // Store the current velocity
        Vector2 currentVelocity = rb.velocity;

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
        colliders[3].enabled = true;
        colliders[4].enabled = true;

        // Determine the dash direction based on the input
        float moveDirection = Input.GetKey(left) ? -1f : 1f; ;


        audiomngr.PlaySFX(audiomngr.roll, 1);
        // Calculate the dash velocity
        Vector2 rollVelocity = new Vector2(moveDirection * rollPower, currentVelocity.y);

        // Apply the dash velocity
        rb.velocity = rollVelocity;

        // Trigger the dash animation
        animator.SetTrigger("roll");



        // Wait for the dash duration
        yield return new WaitForSeconds(rollTime);

        // Reset the velocity after the dash
        rb.velocity = currentVelocity;

        foreach (Collider2D collider in colliders)
        {
            if (collider != colliders[3])
            {
                collider.enabled = true;
            }
        }
        colliders[3].enabled = false;
        colliders[4].enabled = false;

        

        // Reset the gravity scale
        rb.gravityScale = ogGravityScale;

        rollReady = false;

        rollin = false;

        knockable = true;

        StartCoroutine(ResetRoll());


    }

    IEnumerator ResetRoll()
    {

        yield return new WaitForSeconds(2f);
        audiomngr.PlaySFX(audiomngr.rollReady, audiomngr.lessVol);
        rollReady = true;
    }

    public void QuickPunchStart()
    {
        animator.SetBool("QuickPunch", true);

    }

    public void ResetQuickPunch()
    {
        animator.SetBool("QuickPunch", false);
    }

    public void FuseSound()
    {
        audiomngr.PlaySFX(audiomngr.fuse, 1f);
    }
}
