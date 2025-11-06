using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class CustomSettingsNavigation : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Button[] slotButtons;
    public Button[] editButtons;
    public Button backButton;

    private int currentIndex = 0;
    private bool onEditSide = false;

    void Start()
    {
        // Force EventSystem to reset its last selection
        EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(SelectFirstSlot());
    }

    IEnumerator SelectFirstSlot()
    {
        yield return null; // Wait 1 frame so Unity clears the old selection
        if (slotButtons.Length > 0)
            EventSystem.current.SetSelectedGameObject(slotButtons[0].gameObject);
    }

    void Update()
    {
        HandleNavigation();
    }

    void HandleNavigation()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            MoveVertical(1);
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            MoveVertical(-1);
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            MoveHorizontal(true);
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            MoveHorizontal(false);
        else if (Input.GetKeyDown(KeyCode.Return))
            ActivateCurrentButton();
    }

    void MoveVertical(int dir)
    {
        int max = slotButtons.Length;
        currentIndex = (currentIndex + dir + max) % max;
        UpdateSelection();
    }

    void MoveHorizontal(bool toRight)
    {
        onEditSide = toRight;
        UpdateSelection();
    }

    void UpdateSelection()
    {
        if (onEditSide)
            editButtons[currentIndex].Select();
        else
            slotButtons[currentIndex].Select();
    }

    void ActivateCurrentButton()
    {
        if (onEditSide)
            editButtons[currentIndex].onClick.Invoke();
        else
            slotButtons[currentIndex].onClick.Invoke();
    }
}
