using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ModeSelectScript : MonoBehaviour
{

    public Button[] buttons;  // Assign your buttons in the Inspector
    private int selectedIndex = 0;
    public MainMenuMusic sfx;
    bool notSelected;
    public GameObject onlineMenu;


    // Start is called before the first frame update
    void Start()
    {
        notSelected = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Navigate through the buttons with the arrow keys
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (notSelected)
            {
                AutoSelectButton();
                return;
            }
            Navigate(1);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (notSelected)
            {
                AutoSelectButton();
                return;
            }
            Navigate(-1);
        }

        // Press Enter to click the selected button
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ClickSelectedButton();
        }
    }

    void ClickSelectedButton()
    {
        // Click the currently selected button
        var pointer = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(buttons[selectedIndex].gameObject, pointer, ExecuteEvents.pointerClickHandler);
    }




    void Navigate(int direction)
    {
        sfx.ButtonSound();
        // Deselect the current button
        buttons[selectedIndex].OnDeselect(null);

        // Update the selected index
        selectedIndex += direction;
        if (selectedIndex >= buttons.Length) selectedIndex = 0;
        if (selectedIndex < 0) selectedIndex = buttons.Length - 1;

        // Select the new button
        EventSystem.current.SetSelectedGameObject(buttons[selectedIndex].gameObject);
        buttons[selectedIndex].Select();
    }

    void AutoSelectButton()
    {
        // Ensure the first button is selected by default
        if (buttons.Length > 0)
        {
            sfx.ButtonSound();
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
            buttons[0].Select();
            notSelected = false;
        }
    }
    public void OpenOnlineMenu()
    {
        onlineMenu.SetActive(true);
    }


}



