using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;
using ExitGames.Client.Photon;

public class StartButtonScript : MonoBehaviour
{
    public TMP_Text setting; // Assign in Inspector

    void Update()
    {
        // ENTER key starts depending on mode
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (PhotonNetwork.InRoom)
                StartOnlineMatch();
            else
                StartOfflineMatch();
        }
    }

    // UI button click
    public void OnStartButtonClicked()
    {
        if (PhotonNetwork.InRoom)
            StartOnlineMatch();
        else
            StartOfflineMatch();
    }

    // -------------------------------- ONLINE MODE ---------------------------------
    public void StartOnlineMatch()
    {
        // Check if WE picked a character
        if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("SelectedCharacter"))
        {
            Debug.Log("⚠ You must pick a character first!");
            return;
        }

        // Check if opponent exists & picked
        if (PhotonNetwork.PlayerList.Length < 2)
        {
            Debug.Log("⚠ Waiting for another player to join the room...");
            return;
        }

        if (!PhotonNetwork.PlayerList[1].CustomProperties.ContainsKey("SelectedCharacter"))
        {
            Debug.Log("⚠ Waiting for the other player to pick a character...");
            return;
        }

        Debug.Log("🎮 Both players selected characters. Loading game...");

        // Photon loads level across all players
        PhotonNetwork.LoadLevel("GamePlayScene");
    }

    // ------------------------------ OFFLINE MODE ---------------------------------
    private void StartOfflineMatch()
    {
        ApplyDefaultRulesetIfNeeded();
        SceneManager.LoadScene("GamePlayScene");
    }

    // ------------------------------ RULESET LOGIC --------------------------------
    private void ApplyDefaultRulesetIfNeeded()
    {
        if (setting.text != "Default") return;

        CustomRuleset ruleset = new CustomRuleset();
        ruleset.health = 100;
        ruleset.powerupsEnabled = true;
        ruleset.rounds = 1;
        ruleset.playerSpeed = 4;
        ruleset.quickDisabled = false;
        ruleset.heavyDisabled = false;
        ruleset.blockDisabled = false;
        ruleset.chargeDisabled = false;
        ruleset.specialDisabled = false;
        ruleset.hideHealth = false;
        ruleset.devTools = false;
        ruleset.portals = 0;
        ruleset.chanChan = true;

        string json = JsonUtility.ToJson(ruleset);
        PlayerPrefs.SetString("SelectedRuleset", json);
        PlayerPrefs.Save();
    }
}
