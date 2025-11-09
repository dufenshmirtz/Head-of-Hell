using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayAgainButton : MonoBehaviour
{
    public GameManager manager;
    public void RestartGame()
    {
        manager.CheckForRandomCharacters();
        //training
        if (manager.trainingMode)
        {
            manager.SoftResetRound();
            return;
        }
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
