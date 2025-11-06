using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CustomSettingsNavigation : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Button[] editButtons;  // Your 5 "Edit" buttons
    public Button backButton;     // Your Back button

    private int currentIndex = 0;

    void Start()
    {
        // Select the first Edit button by default
        if (editButtons.Length > 0)
            EventSystem.current.SetSelectedGameObject(editButtons[0].gameObject);
    }

    void Update()
    {
        HandleNavigation();
    }

    void HandleNavigation()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            MoveSelection(1);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            MoveSelection(-1);
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ActivateCurrentButton();
        }
    }

    void MoveSelection(int direction)
    {
        // 5 edit buttons + 1 back button → total options
        int totalOptions = editButtons.Length + 1;

        currentIndex = (currentIndex + direction + totalOptions) % totalOptions;

        if (currentIndex < editButtons.Length)
            editButtons[currentIndex].Select();
        else
            backButton.Select();
    }

    void ActivateCurrentButton()
    {
        if (currentIndex < editButtons.Length)
            editButtons[currentIndex].onClick.Invoke();
        else
            backButton.onClick.Invoke();
    }
}
