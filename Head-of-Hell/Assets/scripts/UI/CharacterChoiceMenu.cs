using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterChoiceMenu : MonoBehaviour
{
    public Button[] characterButtons;

    private int currentPlayer = 1;
    private int requiredPlayers = 2;
    private int picksMade = 0;

    public TextMeshProUGUI p1characterNameText;
    public TextMeshProUGUI p2characterNameText;
    public TextMeshProUGUI p3characterNameText;

    public GameObject p3SelectionRoot;

    public Button startButton, randomButton;

    bool picked = false;

    public MainMenuMusic audiomanager;

    public CharacterChoiceScript cscript;

    void Start()
    {
        picked = false;
        RefreshModeUI();

        foreach (Button button in characterButtons)
        {
            button.onClick.AddListener(() => OnCharacterButtonClicked(button));
        }
    }

    void OnEnable()
    {
        RefreshModeUI();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            DeselectCharacter();
        }
    }

    public void OnCharacterButtonClicked(Button button)
    {
        if (picked)
        {
            return;
        }

        AssignPick(currentPlayer, button);
        picksMade++;

        if (picksMade >= requiredPlayers)
        {
            picked = true;
            cscript.BothPicked(true);

            if (startButton != null)
                startButton.gameObject.SetActive(true);

            if (requiredPlayers == 2)
            {
                foreach (Button butt in characterButtons)
                {
                    if (!IsChosenButton(butt))
                        butt.interactable = false;
                }
            }

            return;
        }

        currentPlayer = picksMade + 1;
    }

    void DeselectCharacter()
    {
        if (picksMade <= 0)
            return;

        if (picked)
        {
            picked = false;
            cscript.BothPicked(false);

            if (startButton != null)
                startButton.gameObject.SetActive(false);
        }

        ClearPick(picksMade);

        foreach (Button button in characterButtons)
            button.interactable = true;

        picksMade--;
        currentPlayer = picksMade + 1;
    }

    public void ResetCharacterSelection()
    {
        RefreshModeUI();

        foreach (Button button in characterButtons)
        {
            button.interactable = true;
            ToggleChild(button, "P1", false);
            ToggleChild(button, "P2", false);
            ToggleChild(button, "P3", false);
        }

        currentPlayer = 1;
        picksMade = 0;
        picked = false;

        cscript.BothPicked(false);
        cscript.ClearPlayer1Picked();
        cscript.ClearPlayer2Picked();
        cscript.ClearPlayer3Picked();

        if (p1characterNameText != null) p1characterNameText.text = "";
        if (p2characterNameText != null) p2characterNameText.text = "";
        if (p3characterNameText != null) p3characterNameText.text = "";

        PlayerPrefs.DeleteKey("Player1Choice");
        PlayerPrefs.DeleteKey("Player2Choice");
        PlayerPrefs.DeleteKey("Player3Choice");

        if (startButton != null)
            startButton.gameObject.SetActive(false);
    }

    public void HoveringIn(string name)
    {
        if (picked)
        {
            return;
        }

        SetChoiceLabel(currentPlayer, name);
    }

    void CharacterSound(string name)
    {
        if (name == "Lazy Bigus")
        {
            Debug.Log("rizzz");
            audiomanager.PlaySFX(audiomanager.sleep, 1f);
        }
        if (name == "Steelager")
        {
            audiomanager.PlaySFX(audiomanager.roar, 1f);
        }
        if (name == "Skipler")
        {
            audiomanager.PlaySFX(audiomanager.dash, 1f);
        }
        if (name == "Vander")
        {
            audiomanager.PlaySFX(audiomanager.stab, 1f);
        }
        if (name == "Fin")
        {
            audiomanager.PlaySFX(audiomanager.counter, 1f);
        }
        if (name == "Rager")
        {
            audiomanager.PlaySFX(audiomanager.grab, 1f);
        }
        if (name == "Random")
        {
            audiomanager.PlaySFX(audiomanager.random, 1.5f);
        }
        if (name == "Lithra")
        {
            audiomanager.PlaySFX(audiomanager.bell, 1.5f);
        }
        if (name == "Chiback")
        {
            audiomanager.PlaySFX(audiomanager.fire, 1f);
        }
    }

    public void HoveringOut()
    {
        if (picked)
        {
            return;
        }

        SetChoiceLabel(currentPlayer, "");
    }

    public void ManualCharacterButtonClick(Button button)
    {
        Debug.Log("-0-");
        OnCharacterButtonClicked(button);
    }

    private Button GetRandomCharacterButton()
    {
        List<Button> availableButtons = new List<Button>(characterButtons);
        availableButtons.RemoveAll(button => button.name == "Random");

        int randomIndex = Random.Range(0, availableButtons.Count);
        return availableButtons[randomIndex];
    }

    private void RefreshModeUI()
    {
        requiredPlayers = GameModeSelectionState.PlayerCount;

        if (p3SelectionRoot != null)
            p3SelectionRoot.SetActive(requiredPlayers == 3);

        if (requiredPlayers != 3)
        {
            if (p3characterNameText != null)
                p3characterNameText.text = "";

            PlayerPrefs.DeleteKey("Player3Choice");
        }
    }

    private void AssignPick(int playerNumber, Button button)
    {
        SetChoiceLabel(playerNumber, button.name);
        CharacterSound(button.name);
        PlayerPrefs.SetString($"Player{playerNumber}Choice", button.name);

        switch (playerNumber)
        {
            case 1:
                ToggleChild(button, "P1", true);
                cscript.SetPlayer1Picked(button);
                break;
            case 2:
                ToggleChild(button, "P2", true);
                cscript.SetPlayer2Picked(button);
                break;
            case 3:
                ToggleChild(button, "P3", true);
                cscript.SetPlayer3Picked(button);
                // TODO(GamePlayScene): read Player3Choice and spawn a P3 CharacterManager in 1v1v1.
                break;
        }
    }

    private void ClearPick(int playerNumber)
    {
        foreach (Button button in characterButtons)
        {
            ToggleChild(button, $"P{playerNumber}", false);
        }

        switch (playerNumber)
        {
            case 1:
                cscript.ClearPlayer1Picked();
                break;
            case 2:
                cscript.ClearPlayer2Picked();
                break;
            case 3:
                cscript.ClearPlayer3Picked();
                break;
        }

        SetChoiceLabel(playerNumber, "");
        PlayerPrefs.DeleteKey($"Player{playerNumber}Choice");
    }

    private void SetChoiceLabel(int playerNumber, string characterName)
    {
        switch (playerNumber)
        {
            case 1:
                if (p1characterNameText != null) p1characterNameText.text = characterName;
                break;
            case 2:
                if (p2characterNameText != null) p2characterNameText.text = characterName;
                break;
            case 3:
                if (p3characterNameText != null) p3characterNameText.text = characterName;
                break;
        }
    }

    private bool IsChosenButton(Button button)
    {
        Transform p1 = button.transform.Find("P1");
        Transform p2 = button.transform.Find("P2");
        Transform p3 = button.transform.Find("P3");

        return (p1 != null && p1.gameObject.activeSelf)
            || (p2 != null && p2.gameObject.activeSelf)
            || (p3 != null && p3.gameObject.activeSelf);
    }

    private void ToggleChild(Button button, string childName, bool active)
    {
        Transform child = button.transform.Find(childName);
        if (child != null)
            child.gameObject.SetActive(active);
    }
}
