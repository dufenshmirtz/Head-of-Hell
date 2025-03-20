using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private static bool hasLoadedIntro = false;

    void Awake()
    {
        // Ensure this object persists across scene loads
        DontDestroyOnLoad(gameObject);

        // If the intro has not been shown yet, load it
        if (!hasLoadedIntro)
        {
            hasLoadedIntro = true;
            SceneManager.LoadScene("Scenes/Intro");
        }
    }
}
