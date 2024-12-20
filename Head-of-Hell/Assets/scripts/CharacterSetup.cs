using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSetup : MonoBehaviour
{

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

    public GameObject[] stages;

    public Transform attackPoint;

    public GameObject blockDisabledIndicator;
    public GameObject poison;
    public GameObject Stack1Poison;
    public GameObject Stack2Poison;
    public GameObject Stack3Poison;
    public GameObject stun;
    public GameObject shield;
    public GameManager gameManager;

    public Rigidbody2D rb;
    public Animator animator;

    public LayerMask enemyLayer;

    

    public Image cdbarimage;
    public Sprite activeSprite, ogSprite;
    public Slider cooldownSlider;
    public AudioManager audioManager;
    public helthbarscript healthbar;
    public TextMeshProUGUI P1Name, winner;
    public GameObject playAgainButton;
    public GameObject mainMenuButton;
    public GameObject quickAttackIndicator;


    public int playerNum;

    void Start()
    {
        LoadKeyBindings();
    }

    public void LoadKeyBindings()
    {
        // Player 1 Default Keys
        if (playerNum == 1)
        {
            up = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString($"P1_up", "W"));
            down = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString($"P1_down", "S"));
            left = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString($"P1_left", "A"));
            right = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString($"P1_right", "D"));
            lightAttack = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString($"P1_quickAttack", "U"));
            heavyAttack = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString($"P1_heavyAttack", "I"));
            block = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString($"P1_block", "O"));
            ability = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString($"P1_specialAbility", "P"));
            charge = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString($"P1_chargedAttack", "j"));
        }

        // Player 2 Default Keys
        else if (playerNum == 2)
        {
            up = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString($"P2_up", "UpArrow"));
            down = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString($"P2_down", "DownArrow"));
            left = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString($"P2_left", "LeftArrow"));
            right = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString($"P2_right", "RightArrow"));
            lightAttack = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString($"P2_quickAttack", "M"));
            heavyAttack = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString($"P2_heavyAttack", "N"));
            block = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString($"P2_block", "B"));
            ability = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString($"P2_specialAbility", "V"));
            charge = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString($"P2_chargedAttack", "Space"));
        }
    }

    public void SaveKeyBinding(string action, KeyCode newKey)
    {
        PlayerPrefs.SetString($"P{playerNum}_{action}", newKey.ToString());
        PlayerPrefs.Save();

        // Reload keybindings
        LoadKeyBindings();
    }

}
