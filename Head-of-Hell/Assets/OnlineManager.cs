using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class OnlineManager : MonoBehaviourPunCallbacks
{
    public static OnlineManager instance;
    public bool ConnectAndCreateRoom = false;
    public string DesiredRoomName = "";
    public bool ConnectAndJoinRoom = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Connect()
    {
        if (PhotonNetwork.IsConnected)
            return;

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");

        if (ConnectAndCreateRoom)
        {
            ConnectAndCreateRoom = false;
            Debug.Log("Creating room: " + DesiredRoomName);
            CreateRoom(DesiredRoomName);
        }
        else if (ConnectAndJoinRoom)
        {
            ConnectAndJoinRoom = false;
            Debug.Log("Joining room: " + DesiredRoomName);
            JoinRoom(DesiredRoomName);
        }
    }


    public void CreateRoom(string roomName)
    {
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);
        TryStartCharacterSelect();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player joined: " + newPlayer.NickName);
        TryStartCharacterSelect();
    }

    private void TryStartCharacterSelect()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            Debug.Log("Both players connected → Switching to CharacterChoiceMenu UI");

            UIController.instance.OpenCharacterSelect();
        }
    }

}
