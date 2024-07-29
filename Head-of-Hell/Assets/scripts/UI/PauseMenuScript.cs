using UnityEngine;
using UnityEngine.UI;

public class PauseMenuScript : MonoBehaviour
{
    public GameObject menu;  // Assign your menu panel in the Inspector
    public PlayerScript player1;
    public PlayerScript player2;
    public AudioManager sfx;


    void Start()
    {
        // Ensure the menu is initially closed
        menu.SetActive(false);
        
    }

    void Update()
    {
        // Check if the ESC key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    // Method to toggle the menu
    public void ToggleMenu()
    {
        sfx.ButtonSound();
        menu.SetActive(!menu.activeSelf);
        if (!menu.activeSelf)
        {
            player1.stayDynamic();
            player2.stayDynamic();
        }
        else
        {
            player1.stayStatic();
            player2.stayStatic();
        }
    }

    // Method to be called on button click
    public void OnButtonClick()
    {
        ToggleMenu();
    }
}
