using System.Collections;
using UnityEngine;
using TMPro;  // Import TextMeshPro namespace

public class CountdownManager : MonoBehaviour
{
    public TMP_Text countdownText; // Reference to the TextMeshPro UI element
    public int countdownTime = 3;  // Time to count down from
    public CharacterManager p1Manager;
    public CharacterManager p2Manager;
    public GameManager gameManager;
    public AudioManager audioManager;

    void Start()
    {
        string json = PlayerPrefs.GetString("SelectedRuleset", null);

        if (!string.IsNullOrEmpty(json))
        {
            // Convert the JSON string back to a CustomRuleset object
            CustomRuleset loadedRuleset = JsonUtility.FromJson<CustomRuleset>(json);

            if (!loadedRuleset.devTools)
            {
                // Start the countdown as soon as the scene loads,and if its not disabled by dev tools
                StartCoroutine(StartCountdown());
            }
            else
            {
                audioManager.PlayMusic();
            }
        }         
    }

    // Coroutine to handle the countdown
    IEnumerator StartCountdown()
    {

        DisableGamePlay();

        audioManager.PlaySFX(audioManager.trailerSound, audioManager.doubleVol);
        countdownText.text = "Round " + gameManager.GetRoundCounter().ToString();

        // Wait for a brief moment to let the player see the round number
        yield return new WaitForSeconds(3); // Adjust this delay as needed

        audioManager.PlaySFX(audioManager.trailerSound, audioManager.doubleVol);

        // After countdown, show "Fight!"
        countdownText.text = "Serve Chan Chan!";

        // Wait for a brief moment before clearing the text
        yield return new WaitForSeconds(2);

        audioManager.PlayMusic();

        // Clear the countdown text after "Fight!" is displayed
        countdownText.text = "";

        EnableGamePlay();
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
}
