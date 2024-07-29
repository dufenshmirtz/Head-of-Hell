using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterChoiceMenu : MonoBehaviour
{
    public Button[] characterButtons;
    
    private int currentPlayer = 1;

    Button p1b;

    public TextMeshProUGUI p1characterNameText;

    public TextMeshProUGUI p2characterNameText;

    public Button startButton;

    bool picked = false;

    public MainMenuMusic audiomanager;

    public CharacterChoiceScript cscript;


    void Start()
    {
        picked = false;
        foreach (Button button in characterButtons)
        {
            button.onClick.AddListener(() => OnCharacterButtonClicked(button));
        }
    }

    public void OnCharacterButtonClicked(Button button)
    {
        Debug.Log("-1-");

        if (picked)
        {
            return;
        }
        // Get the character index or identifier from the button
        int characterIndex = System.Array.IndexOf(characterButtons, button);

        

        // Assign the character to the current player
        if (currentPlayer == 1)
        {
            Debug.Log("Player 1 chose character " + characterIndex);

            
            Transform childP1 = button.transform.Find("P1");
            childP1.gameObject.SetActive(true);
            currentPlayer = 2;
            p1b= button;
            p1characterNameText.text = button.name;

            CharacterSound(button.name);

            PlayerPrefs.SetString("Player1Choice",button.name);

        }
        else
        {
            Debug.Log("Player 2 chose character " + characterIndex);
            // TODO: Assign character to Player 2
            // TODO: Update UI to indicate Player 2's choice
            Transform childP1 = button.transform.Find("P2");
            childP1.gameObject.SetActive(true);
            button.Select();
            p2characterNameText.text = button.name;

            PlayerPrefs.SetString("Player2Choice", button.name);

            CharacterSound(button.name);

            
            foreach (Button butt in characterButtons)
            {
                

                if (butt!=p1b && butt != button)
                {
                    
                    butt.interactable = false;
                }
            }

            picked=true;

            cscript.BothPicked(true);

            startButton.gameObject.SetActive(true);
        }

        

        
    }

    public void ResetCharacterSelection()
    {
        Debug.Log("--00--00--");
        // Reactivate all character buttons
        foreach (Button button in characterButtons)
        {
            button.interactable = true;
            // Deactivate any child objects indicating player choices
            Transform childP1 = button.transform.Find("P1");
            if (childP1 != null)
            {
                childP1.gameObject.SetActive(false);
            }
            Transform childP2 = button.transform.Find("P2");
            if (childP2 != null)
            {
                childP2.gameObject.SetActive(false);
            }
        }
        // Reset current player to 1
        currentPlayer = 1;

        picked=false;

        cscript.BothPicked(false);

        p1characterNameText.text = "";
        p2characterNameText.text = "";

        startButton.gameObject.SetActive(false);
    }

    public void HoveringIn(string name)
    {
        if (picked){
            return;
        }

        if (currentPlayer == 1)
        {
            p1characterNameText.text = name;
        }
        else
        {
            p2characterNameText.text = name;
        }
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
    }
    public void HoveringOut()
    {

        if (picked)
        {
            return;
        }

        if (currentPlayer == 1)
        {
            p1characterNameText.text = "";
        }
        else
        {
            p2characterNameText.text = "";
        }
    }

    public void ManualCharacterButtonClick(Button button)
    {
        Debug.Log("-0-");
        OnCharacterButtonClicked(button);
    }

}
