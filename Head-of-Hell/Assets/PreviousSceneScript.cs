using UnityEngine;
using System.Collections.Generic;

public class UIBackNavigator : MonoBehaviour
{
    public static UIBackNavigator Instance;

    private Stack<GameObject> uiHistory = new Stack<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional if you want persistence
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoBack();
        }
    }

    public void OpenUI(GameObject newUI)
    {
        if (uiHistory.Count > 0 && uiHistory.Peek() != null)
        {
            uiHistory.Peek().SetActive(false);
        }

        uiHistory.Push(newUI);
        newUI.SetActive(true);
    }

    public void GoBack()
    {
        if (uiHistory.Count > 1)
        {
            GameObject top = uiHistory.Pop();
            top.SetActive(false);

            GameObject previous = uiHistory.Peek();
            previous.SetActive(true);
        }
        else
        {
            Debug.Log("No previous UI to go back to.");
        }
    }

    public void ResetHistory()
    {
        uiHistory.Clear();
    }
}
