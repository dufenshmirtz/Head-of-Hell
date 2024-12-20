using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControlCustomizer : MonoBehaviour
{
    [Header("Player 1 Controls")]
    public Button P1_UpButton;
    public Button P1_DownButton;
    public Button P1_LeftButton;
    public Button P1_RightButton;
    public Button P1_QuickAttackButton;
    public Button P1_HeavyAttackButton;
    public Button P1_BlockButton;
    public Button P1_SpecialAbilityButton;
    public Button P1_ChargedAttackButton;

    [Header("Player 2 Controls")]
    public Button P2_UpButton;
    public Button P2_DownButton;
    public Button P2_LeftButton;
    public Button P2_RightButton;
    public Button P2_QuickAttackButton;
    public Button P2_HeavyAttackButton;
    public Button P2_BlockButton;
    public Button P2_SpecialAbilityButton;
    public Button P2_ChargedAttackButton;

    private Button currentButton; // Button being assigned
    private string currentAction; // Action being customized
    private int currentPlayer; // 1 for P1, 2 for P2

    private void Start()
    {
        // Set up button listeners for Player 1
        P1_UpButton.onClick.AddListener(() => BeginKeyBinding(1, "up", P1_UpButton));
        P1_DownButton.onClick.AddListener(() => BeginKeyBinding(1, "down", P1_DownButton));
        P1_LeftButton.onClick.AddListener(() => BeginKeyBinding(1, "left", P1_LeftButton));
        P1_RightButton.onClick.AddListener(() => BeginKeyBinding(1, "right", P1_RightButton));
        P1_QuickAttackButton.onClick.AddListener(() => BeginKeyBinding(1, "quickAttack", P1_QuickAttackButton));
        P1_HeavyAttackButton.onClick.AddListener(() => BeginKeyBinding(1, "heavyAttack", P1_HeavyAttackButton));
        P1_BlockButton.onClick.AddListener(() => BeginKeyBinding(1, "block", P1_BlockButton));
        P1_SpecialAbilityButton.onClick.AddListener(() => BeginKeyBinding(1, "specialAbility", P1_SpecialAbilityButton));
        P1_ChargedAttackButton.onClick.AddListener(() => BeginKeyBinding(1, "chargedAttack", P1_ChargedAttackButton));

        // Set up button listeners for Player 2
        P2_UpButton.onClick.AddListener(() => BeginKeyBinding(2, "up", P2_UpButton));
        P2_DownButton.onClick.AddListener(() => BeginKeyBinding(2, "down", P2_DownButton));
        P2_LeftButton.onClick.AddListener(() => BeginKeyBinding(2, "left", P2_LeftButton));
        P2_RightButton.onClick.AddListener(() => BeginKeyBinding(2, "right", P2_RightButton));
        P2_QuickAttackButton.onClick.AddListener(() => BeginKeyBinding(2, "quickAttack", P2_QuickAttackButton));
        P2_HeavyAttackButton.onClick.AddListener(() => BeginKeyBinding(2, "heavyAttack", P2_HeavyAttackButton));
        P2_BlockButton.onClick.AddListener(() => BeginKeyBinding(2, "block", P2_BlockButton));
        P2_SpecialAbilityButton.onClick.AddListener(() => BeginKeyBinding(2, "specialAbility", P2_SpecialAbilityButton));
        P2_ChargedAttackButton.onClick.AddListener(() => BeginKeyBinding(2, "chargedAttack", P2_ChargedAttackButton));
    }

    private void BeginKeyBinding(int player, string action, Button button)
    {
        currentPlayer = player;
        currentAction = action;
        currentButton = button;

        Debug.Log($"Waiting for key input for {action} (Player {player}).");
    }

    private void Update()
    {
        if (currentButton != null && Input.anyKeyDown)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    AssignKeyBinding(currentPlayer, currentAction, key);
                    UpdateButtonVisual(currentButton, key);
                    currentButton = null;
                    break;
                }
            }
        }
    }

    private void AssignKeyBinding(int player, string action, KeyCode newKey)
    {
        string keyName = $"P{player}_{action}";
        PlayerPrefs.SetString(keyName, newKey.ToString());
        Debug.Log($"Assigned {newKey} to {action} for Player {player}.");
    }

    private void UpdateButtonVisual(Button button, KeyCode key)
    {
        // Update the button's text or image to show the new key
        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = key.ToString();
        }

        
    }
}
