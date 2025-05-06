using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    static int player1Wins = 0;
    static int player2Wins = 0;
    public GameObject[] stages;
    string stageName;
    public TextMeshProUGUI winner;
    public TextMeshProUGUI finalWinner;
    string p1, p2;
    static int roundNumber;
    static int roundCounter = 1;
    public CharacterManager p1Manager, p2Manager;
    public GameObject playAgainButton;
    public GameObject mainMenuButton;
    public AudioManager audioManager;
    public GameObject p1R1, p1R2, p1R3;
    public GameObject p2R1, p2R2, p2R3;
    static int p1Rounds = 0, p2Rounds = 0;
    bool tie = false;
    static string c1Name,c2Name;
    static bool p1Random = false;
    static bool p2Random = false;
    bool gameEnd = false;
    static int portalNumber;
    public GameObject[] portalPairs;

    // Start is called before the first frame update
    void Start()
    {
        stageName = PlayerPrefs.GetString("SelectedStage");
        if (stageName == "Stage 1")
        {
            stages[0].SetActive(true);
        }
        else if (stageName == "Stage 2")
        {
            stages[1].SetActive(true);
        }
        else if (stageName == "Stage 3")
        {
            stages[2].SetActive(true);
        }

        string json = PlayerPrefs.GetString("SelectedRuleset", null);

        if (!string.IsNullOrEmpty(json))
        {
            // Convert the JSON string back to a CustomRuleset object
            CustomRuleset loadedRuleset = JsonUtility.FromJson<CustomRuleset>(json);

            roundNumber = loadedRuleset.rounds;
            portalNumber = loadedRuleset.portals;
        }
        else
        {
            Debug.LogWarning("No ruleset found in PlayerPrefs.");
        }

        switch (portalNumber)
        {
            case 0: 
                break;
            case 1:
                portalPairs[0].SetActive (true);
                break;
            case 2:
                portalPairs[0].SetActive(true);
                portalPairs[1].SetActive (true);
                break;
            case 3:
                portalPairs[2].SetActive(true);
                break;
            case 4:
                portalPairs[2].SetActive(true);
                portalPairs[3].SetActive(true);
                break;
        }

        ActivateIndicators();
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void RoundEnd(int playerNum, string winnerName)
    {
        winner.gameObject.SetActive(true);
        DisableGamePlay();
        winner.text = winnerName + " prevails!";

        ShortWins(playerNum, winnerName);

        StartCoroutine(WaitAndCheck(playerNum, winnerName));
    }

    public void RoundEndTie(int playerNum)
    {

        winner.gameObject.SetActive(true);
        DisableGamePlay();
        winner.text = "Tie?\nDEATH PREVAILS...";
        tie = true;
        if (playerNum == 1)
        {
            player1Wins--;
        }
        else
        {
            player2Wins--;
        }

        ActivateIndicators();
        CheckForRandomCharacters();
        StartCoroutine(WaitAndrestart());

    }

    public void RoundEndFlawless(int playerNum, string winnerName)
    {
        winner.gameObject.SetActive(true);
        DisableGamePlay();
        winner.text = "FLAWLESS\n" + winnerName + " prevails!";

        ShortWins(playerNum, winnerName);

        StartCoroutine(WaitAndCheck(playerNum, winnerName));
    }

    public void ShortWins(int playerNum, string winnerName)
    {
        if (playerNum == 1)
        {
            player2Wins++;
        }
        else
        {
            player1Wins++;
        }

        ActivateIndicators();

    }

    public void CheckForEnd(int playerNum, string winnerName)
    {
        if (tie)
        {
            tie = false;
            return;
        }

        if (player1Wins > roundNumber / 2 || player2Wins > roundNumber / 2)
        {
            finalWinner.text = "Victory belongs to " + winnerName + "!\n Chan Chan smiles...";
            winner.gameObject.SetActive(false);
            finalWinner.gameObject.SetActive(true);
            roundCounter = 1;
            player1Wins = 0;
            player2Wins = 0;

            audioManager.PlaySFX(audioManager.dramaticDrums, audioManager.doubleVol);

            gameEnd = true;

            playAgainButton.SetActive(true);
            mainMenuButton.SetActive(true);

            CheckForRandomCharacters();
        }
        else
        {
            roundCounter++;
            CheckForRandomCharacters();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

    }



    private IEnumerator WaitAndCheck(int playerNum, string winnerName)
    {
        // Wait for 3 seconds
        yield return new WaitForSeconds(3f);


        // Call the ShortWins method after the delay
        CheckForEnd(playerNum, winnerName);
    }

    private IEnumerator WaitAndrestart()
    {
        // Wait for 3 seconds
        yield return new WaitForSeconds(3f);

        tie = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }

    public int GetRoundCounter()
    {
        return roundCounter;
    }

    public void EnableGamePlay()
    {
        p1Manager.Resume();
        p2Manager.Resume();
    }

    public void DisableGamePlay()
    {
        p1Manager.Pause();
        p2Manager.Pause();
    }

    void ActivateIndicators()
    {
        if (player1Wins == 1)
        {
            p1R1.SetActive(true);
        }
        if (player2Wins == 1)
        {
            p2R1.SetActive(true);
        }
        if (player1Wins == 2)
        {
            p1R1.SetActive(true);
            p1R2.SetActive(true);
        }
        if (player2Wins == 2)
        {
            p2R1.SetActive(true);
            p2R2.SetActive(true);
        }
        if (player1Wins == 3)
        {
            p1R1.SetActive(true);
            p1R2.SetActive(true);
            p1R3.SetActive(true);
        }
        if (player2Wins == 3)
        {
            p2R1.SetActive(true);
            p2R2.SetActive(true);
            p2R3.SetActive(true);
        }
    }

    public void CheckForRandomCharacters()
    {
        print(roundCounter+" rc");
        if(PlayerPrefs.GetString("Player1Choice")=="Random"  && roundCounter>1)
        {
            c1Name = p1Manager.GetCharacterName(1);
            PlayerPrefs.SetString("Player1Choice",c1Name);
            p1Random = true;
        }

        if (PlayerPrefs.GetString("Player2Choice")=="Random" && roundCounter>1)
        {
            c2Name = p2Manager.GetCharacterName(1);
            PlayerPrefs.SetString("Player2Choice", c2Name);
            p2Random = true;
        }

        if(p1Random && gameEnd)
        {
            PlayerPrefs.SetString("Player1Choice", "Random");
        }

        if (p2Random && gameEnd)
        {
            PlayerPrefs.SetString("Player2Choice", "Random");
        }
    }


    // Update is called once per frame
    void Update()
    {

    }

    public void ResetStatics()
    {
        roundCounter = 1;
        player1Wins = 0;
        player2Wins = 0;
    }
}
