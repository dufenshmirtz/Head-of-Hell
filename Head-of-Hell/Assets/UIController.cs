using UnityEngine;

public class UIController : MonoBehaviour
{
    public GameObject onlineMenu;
    public GameObject characterChoiceMenu;

    public static UIController instance;

    private void Awake()
    {
        instance = this;
    }

    public void OpenCharacterSelect()
    {
        Debug.Log("UIController → Opening Character Select UI");

        onlineMenu.SetActive(false);
        characterChoiceMenu.SetActive(true);
    }
}
