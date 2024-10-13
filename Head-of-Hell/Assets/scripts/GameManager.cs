using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
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
    static int roundCounter;
    public CharacterManager p1Manager, p2Manager;
    public GameObject playAgainButton;
    public GameObject mainMenuButton;
    public AudioManager audioManager;

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

            roundNumber=loadedRuleset.rounds;
        }
        else
        {
            Debug.LogWarning("No ruleset found in PlayerPrefs.");
        }

    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            roundCounter++;
        }
    }

    public void RoundEnd(int playerNum,string winnerName)
    {
        winner.gameObject.SetActive(true);
        DisableGamePlay();

        winner.text = winnerName + " prevails!";

        StartCoroutine(WaitAndCallShortWins(playerNum, winnerName));
    }

    public void RoundEndTie()
    {
        winner.gameObject.SetActive(true);
        DisableGamePlay();
        winner.text = "Tie?\nDEATH PREVAILS...";
    }

    public void RoundEndFlawless(int playerNum, string winnerName)
    {
        winner.gameObject.SetActive(true);
        DisableGamePlay();
        winner.text = "FLAWLESS\n" + winnerName + " prevails!";

        StartCoroutine(WaitAndCallShortWins(playerNum, winnerName));
    }

    public void ShortWins(int playerNum,string winnerName)
    {
        if (playerNum == 1)
        {
            player2Wins++;
        }
        else
        {
            player1Wins++;
        }

        if(player1Wins > roundNumber/2 || player2Wins > roundNumber / 2)
        {
            finalWinner.text = "Victory belongs to " + winnerName + "!\n Chan Chan smiles...";
            winner.gameObject.SetActive(false);
            finalWinner.gameObject.SetActive(true);
            roundCounter = 0;
            player1Wins = 0;
            player2Wins = 0;

            audioManager.PlaySFX(audioManager.dramaticDrums, audioManager.normalVol);

            playAgainButton.SetActive(true);
            mainMenuButton.SetActive(true);
        }
        else
        {
            roundCounter++;
            print("op "+roundCounter);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        
    }

    private IEnumerator WaitAndCallShortWins(int playerNum, string winnerName)
    {
        // Wait for 3 seconds
        yield return new WaitForSeconds(3f);

        // Call the ShortWins method after the delay
        ShortWins(playerNum, winnerName);
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


    // Update is called once per frame
    void Update()
    {
        
    }
}
