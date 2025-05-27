using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonNavigator : MonoBehaviour
{
    public Button[] buttons;                     
    public GameObject[] highlightBorders;        
    private int selectedIndex = 0;

    void Start()
    {
        UpdateSelection();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedIndex = (selectedIndex + 1) % buttons.Length;
            UpdateSelection();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedIndex = (selectedIndex - 1 + buttons.Length) % buttons.Length;
            UpdateSelection();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            PressSelectedButton();
        }
    }

    void UpdateSelection()
    {
        // Select the button in EventSystem
        EventSystem.current.SetSelectedGameObject(buttons[selectedIndex].gameObject);
        buttons[selectedIndex].Select();

        // Update borders visibility
        for (int i = 0; i < highlightBorders.Length; i++)
        {
            highlightBorders[i].SetActive(i == selectedIndex);
        }
    }

    void PressSelectedButton()
    {
        buttons[selectedIndex].onClick.Invoke();
    }
}
