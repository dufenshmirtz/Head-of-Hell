using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterChoiceHandler : MonoBehaviour
{
    public string characterName;

    // Player colors
    public Color SteelagerColor;
    public Color FinColor;
    public Color RagerColor;
    public Color SkiplerColor;
    public Color VanderColor;
    public Color LazyBigusColor;
    public Color LithraColor;
    public Color ChibackColor;

    // Animator Controllers
    public RuntimeAnimatorController SteelagerAnimatorController;
    public RuntimeAnimatorController FinAnimatorController;
    public RuntimeAnimatorController RagerAnimatorController;
    public RuntimeAnimatorController SkiplerAnimatorController;
    public RuntimeAnimatorController VanderAnimatorController;
    public RuntimeAnimatorController LazyBigusAnimatorController;
    public RuntimeAnimatorController LithraAnimatorController;
    public RuntimeAnimatorController ChibackAnimatorController;

    Character character;
    Skipler skipler;
    Rager rager;
    Fin fin;
    Steelager steelager;
    LazyBigus bigus;
    Vander vander;
    Lithra lithra;
    Chiback chiback;
    Animator animator;

    public CharacterChoiceHandler enemyHandler;

    public int playerNum;

    void Awake()
    {
        // Fetch player character choices
        if (playerNum == 1)
        {
            characterName = PlayerPrefs.GetString("Player1Choice");
        }
        else
        {
            characterName = PlayerPrefs.GetString("Player2Choice");
        }

        // Handle "Random" selections
        if (characterName == "Random")
        {
            characterName = PickRandomCharacter();
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        // Assign character, color, and animator controller based on selection
        switch (characterName)
        {
            case "Steelager":
                steelager = this.gameObject.AddComponent<Steelager>();
                character = steelager;
                spriteRenderer.color = SteelagerColor;
                animator = GetComponent<Animator>();
                animator.runtimeAnimatorController = SteelagerAnimatorController;
                break;
            case "Vander":
                vander = this.gameObject.AddComponent<Vander>();
                character = vander;
                spriteRenderer.color = VanderColor;
                animator = GetComponent<Animator>();
                animator.runtimeAnimatorController = VanderAnimatorController;
                break;
            case "Rager":
                rager = this.gameObject.AddComponent<Rager>();
                character = rager;
                spriteRenderer.color = RagerColor;
                animator = GetComponent<Animator>();
                animator.runtimeAnimatorController = RagerAnimatorController;
                break;
            case "Skipler":
                skipler = this.gameObject.AddComponent<Skipler>();
                character = skipler;
                spriteRenderer.color = SkiplerColor;
                animator = GetComponent<Animator>();
                animator.runtimeAnimatorController = SkiplerAnimatorController;
                break;
            case "Fin":
                fin = this.gameObject.AddComponent<Fin>();
                character = fin;
                spriteRenderer.color = FinColor;
                animator = GetComponent<Animator>();
                animator.runtimeAnimatorController = FinAnimatorController;
                break;
            case "Lazy Bigus":
                bigus = this.gameObject.AddComponent<LazyBigus>();
                character = bigus;
                spriteRenderer.color = LazyBigusColor;
                animator = GetComponent<Animator>();
                animator.runtimeAnimatorController = LazyBigusAnimatorController;
                break;
            case "Lithra":
                lithra = this.gameObject.AddComponent<Lithra>();
                character = lithra;
                animator = GetComponent<Animator>();
                animator.runtimeAnimatorController = LithraAnimatorController;
                break;
            case "Chiback":
                chiback = this.gameObject.AddComponent<Chiback>();
                character = chiback;
                spriteRenderer.color = ChibackColor;
                animator = GetComponent<Animator>();
                animator.runtimeAnimatorController = ChibackAnimatorController;
                break;
        }
    }

    void Start()
    {

    }

    void Update()
    {

    }

    public string PickRandomCharacter()
    {
        // Array of possible character type names
        string[] characterTypeNames = {
            "Steelager", "Vander", "Rager",
            "Skipler", "Fin", "Lazy Bigus",
            "Lithra", "Chiback"
        };

        // Randomly select an index
        int randomIndex = UnityEngine.Random.Range(0, characterTypeNames.Length);

        return characterTypeNames[randomIndex];
    }

    public Character CharacterChoice(int playerNum)
    {
        if (playerNum == 1)
        {
            return character;
        }
        if (playerNum == 2)
        {
            return enemyHandler.CharacterChoice(1);
        }
        print("Randomizer Error");
        return null;
    }

    public string GetCharacterName(int playerNum)
    {
        if (playerNum == 1)
        {
            return characterName;
        }
        if (playerNum == 2)
        {
            return enemyHandler.GetCharacterName(1);
        }
        print("Randomizer Error");
        return null;
    }

    public Animator GetAnimator()
    {
        return animator;
    }
}
