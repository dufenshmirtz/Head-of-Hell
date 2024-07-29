using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenu : MonoBehaviour
{
    public void MenuScreen()
    {
        // Load Menu scene
        SceneManager.LoadScene(0);
    }
}
