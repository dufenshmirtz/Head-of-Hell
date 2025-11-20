using UnityEngine;
using TMPro;

public class OnlineMenuUI : MonoBehaviour
{
    public GameObject onlineMenu;
    public TMP_InputField roomNameInput;

    public void OpenMenu()
    {
        onlineMenu.SetActive(true);
    }

    public void CloseMenu()
    {
        onlineMenu.SetActive(false);
    }

    public void OnCreateRoomPressed()
    {
        if (string.IsNullOrEmpty(roomNameInput.text))
            return;

        OnlineManager.instance.DesiredRoomName = roomNameInput.text;
        OnlineManager.instance.ConnectAndCreateRoom = true;
        OnlineManager.instance.Connect();

    }

    public void OnJoinRoomPressed()
    {
        if (string.IsNullOrEmpty(roomNameInput.text))
            return;

        OnlineManager.instance.Connect();
        OnlineManager.instance.JoinRoom(roomNameInput.text);
    }
}
