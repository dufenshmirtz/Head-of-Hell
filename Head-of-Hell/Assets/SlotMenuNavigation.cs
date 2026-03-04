using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotMenuNavigation : MonoBehaviour
{
    public Button[] buttons;
    int index = 0;

    void Start()
    {
        SelectButton();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            index++;
            if (index >= buttons.Length) index = 0;
            SelectButton();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            index--;
            if (index < 0) index = buttons.Length - 1;
            SelectButton();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            buttons[index].onClick.Invoke();
        }
    }

    void SelectButton()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            Transform border = buttons[i].transform.Find("Border");
            if (border != null)
                border.gameObject.SetActive(false);
        }

        Transform selectedBorder = buttons[index].transform.Find("Border");
        if (selectedBorder != null)
            selectedBorder.gameObject.SetActive(true);

        EventSystem.current.SetSelectedGameObject(buttons[index].gameObject);
        buttons[index].Select();
    }
}