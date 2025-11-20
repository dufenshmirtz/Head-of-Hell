using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;


public class CharacterChoiceMenu : MonoBehaviourPunCallbacks
{
    public Button[] characterButtons;

    private int currentPlayer = 1;

    Button p1b;

    public TextMeshProUGUI p1characterNameText;

    public TextMeshProUGUI p2characterNameText;

    public Button startButton, randomButton;

    bool picked = false;

    public MainMenuMusic audiomanager;

    public CharacterChoiceScript cscript;


    private void Awake()
    {
        if (PhotonNetwork.InRoom)
        {
            bool IAmPlayer1 = PhotonNetwork.LocalPlayer.ActorNumber == 1;

            // Player 1 UI Objects
            Transform player1Label = transform.Find("Player1");
            Transform p1Choice = transform.Find("p1Choice");

            // Player 2 UI Objects
            Transform player2Label = transform.Find("Player2");
            Transform p2Choice = transform.Find("p2Choice");

            if (IAmPlayer1)
            {
                // Hide Player 2 UI on Player 1's screen
                if (player2Label) player2Label.gameObject.SetActive(false);
                if (p2Choice) p2Choice.gameObject.SetActive(false);
            }
            else
            {
                // Hide Player 1 UI on Player 2's screen
                if (player1Label) player1Label.gameObject.SetActive(false);
                if (p1Choice) p1Choice.gameObject.SetActive(false);
            }
        }
    }

    void Start()
    {
        picked = false;
        foreach (Button button in characterButtons)
        {
            button.onClick.AddListener(() => OnCharacterButtonClicked(button));
        }
    }
    

    void Update()
    {
        // Detect right mouse button click to undo selection
        if (Input.GetMouseButtonDown(1)) // 1 = right mouse button
        {
            DeselectCharacter();
        }
    }

    public void OnCharacterButtonClicked(Button button)
    {

        if (PhotonNetwork.InRoom)
        {
            // If this player already picked, don't allow double picking
            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("SelectedCharacter"))
                return;

            // Save selection to Photon properties
            ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
            h["SelectedCharacter"] = button.name;
            PhotonNetwork.LocalPlayer.SetCustomProperties(h);

            // Sync highlight for BOTH clients
            photonView.RPC("RPC_ShowSelection", RpcTarget.All,
                PhotonNetwork.LocalPlayer.ActorNumber, button.name);

            // Play sound locally
            CharacterSound(button.name);

            return; // IMPORTANT so offline code doesn't run
        }


        
        



        // Assign the character to the current player
        if (currentPlayer == 1)
        {
            Transform childP1 = button.transform.Find("P1");
            if (childP1 != null) childP1.gameObject.SetActive(true);
            currentPlayer = 2;
            p1b = button;

            p1characterNameText.text = button.name;

            CharacterSound(button.name);

            PlayerPrefs.SetString("Player1Choice", button.name);
        }
        else
        {
            Transform childP2 = button.transform.Find("P2");
            if (childP2 != null) childP2.gameObject.SetActive(true);
            button.Select();
            p2characterNameText.text = button.name;

            PlayerPrefs.SetString("Player2Choice", button.name);

            CharacterSound(button.name);

            foreach (Button butt in characterButtons)
            {
                if (butt != p1b && butt != button)
                {
                    butt.interactable = false;
                }
            }

            picked = true;

            cscript.BothPicked(true);

            startButton.gameObject.SetActive(true);
        }
    }
    [PunRPC]
    void RPC_ShowSelection(int actorNumber, string characterName)
    {
        // Update the text label
        if (actorNumber == 1 && p1characterNameText != null)
            p1characterNameText.text = characterName;

        if (actorNumber == 2 && p2characterNameText != null)
            p2characterNameText.text = characterName;

        // Highlight correct button
        foreach (Button btn in characterButtons)
        {
            if (btn.name == characterName)
            {
                Transform p1 = btn.transform.Find("P1");
                Transform p2 = btn.transform.Find("P2");

                if (actorNumber == 1 && p1 != null) p1.gameObject.SetActive(true);
                if (actorNumber == 2 && p2 != null) p2.gameObject.SetActive(true);
            }
        }
    }
    private void CheckIfBothPicked()
    {
        bool p1Picked = false;
        bool p2Picked = false;

        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (p.CustomProperties.ContainsKey("SelectedCharacter"))
            {
                if (p.ActorNumber == 1) p1Picked = true;
                if (p.ActorNumber == 2) p2Picked = true;
            }
        }

        if (p1Picked && p2Picked)
        {
            startButton.gameObject.SetActive(true);
            cscript.BothPicked(true);
            Debug.Log("Both players have picked");
        }
    }

    void DeselectCharacter()
    {
        if (picked) // Both players have picked
        {
            // Deselect Player 2 first
            foreach (Button button in characterButtons)
            {
                Transform childP2 = button.transform.Find("P2");
                if (childP2 != null) childP2.gameObject.SetActive(false);
                Transform border = button.transform.Find("Border");
                if (border != null) border.gameObject.SetActive(false);

                button.interactable = true;
            }
            p2characterNameText.text = "";
            PlayerPrefs.DeleteKey("Player2Choice");

            // Reset game state for Player 2
            picked = false;
            cscript.BothPicked(false);
            startButton.gameObject.SetActive(false);

            // Switch back to Player 2 to allow picking again
            currentPlayer = 2;
        }
        else if (currentPlayer == 2 && p1b != null) // Only Player 1 has picked
        {
            // Deselect Player 1
            Transform childP1 = p1b.transform.Find("P1");
            if (childP1 != null) childP1.gameObject.SetActive(false);
            p1characterNameText.text = "";
            PlayerPrefs.DeleteKey("Player1Choice");
            p1b.interactable = true;
            p1b = null;

            // Switch back to Player 1
            currentPlayer = 1;
        }
        else if (currentPlayer == 1) // Only Player 2 has picked
        {
            // Deselect Player 2
            foreach (Button button in characterButtons)
            {
                Transform childP2 = button.transform.Find("P2");
                if (childP2 != null) childP2.gameObject.SetActive(false);
                button.interactable = true;
            }
            p2characterNameText.text = "";
            PlayerPrefs.DeleteKey("Player2Choice");

            // Reset game state
            picked = false;
            cscript.BothPicked(false);
            startButton.gameObject.SetActive(false);
        }
    }
    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        CheckIfBothPicked();
    }

    public void ResetCharacterSelection()
    {
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

        picked = false;

        cscript.BothPicked(false);

        p1characterNameText.text = "";
        p2characterNameText.text = "";

        startButton.gameObject.SetActive(false);
    }

    public void HoveringIn(string name)
    {
        if (picked)
        {
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

    private Button GetRandomCharacterButton()
    {
        List<Button> availableButtons = new List<Button>(characterButtons);
        availableButtons.RemoveAll(button => button.name == "Random");

        int randomIndex = Random.Range(0, availableButtons.Count);
        return availableButtons[randomIndex];
    }


}