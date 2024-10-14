using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenu : MonoBehaviour
{
    public GameManager gameManager;
    public void MenuScreen()
    {
        gameManager.ResetStatics();
        // Load Menu scene
        SceneManager.LoadScene(0);
    }
}
