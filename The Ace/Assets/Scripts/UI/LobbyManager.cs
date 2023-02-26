using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public static LobbyManager instance;

    public InputField roomNameField;
    public InputField ipInput;
    public InputField portInput;

    public GameObject lobbyPanel;
    public GameObject roomPanel;
    public GameObject ipPanel;
    public Text roomName;

    public RoomItem roomItemPrefab;
    List<RoomItem> roomItemsList = new List<RoomItem>();
    public Transform contentObject;

    public float timeBetweenUpdates = 1.5f;
    float nextUpdateTime;

    public List<PlayerItem> playerItemsList = new List<PlayerItem>();
    private List<PlayerItem> unassignedList = new List<PlayerItem>();
    private List<PlayerItem> team1List = new List<PlayerItem>();
    private List<PlayerItem> team2List = new List<PlayerItem>();

    public PlayerItem playerItemPrefab;
    public Transform unassignedItemParent;
    public Transform[] parents;

    [Header("Room Panel UI")]
    public GameObject playButton;
    public GameObject errorContainer;
    public Text errorMessage;

    [Header("IP Menu UI")]
    public GameObject ipErrorContainer;
    public Text ipErrorMessage;
    private string ipAddress;
    private string portAddress;

    private void Start()
    {
        instance = this;
        PhotonNetwork.JoinLobby();
    }

    public void OnClickCreate()
    {
        if (roomNameField.text.Length >= 1)
        {
            ipPanel.SetActive(true);
        }
    }

    public void OnClickConfirmCreate()
    {
        if (ipInput.text.Length < 8)
        {
            ipErrorContainer.SetActive(true);
            ipErrorMessage.text = "This IP is invalid!";
        }
        else
        {
            ipAddress = ipInput.text;
            portAddress = portInput.text;
            ipErrorContainer.SetActive(false);
            ipErrorMessage.text = "";
            RoomOptions roomOptions =
            new RoomOptions()
            {
                MaxPlayers = 4,
                BroadcastPropsChangeToAll = true
            };

            Hashtable roomCustomProps = new Hashtable();

            roomCustomProps.Add("ip", ipAddress);
            roomCustomProps.Add("port", portAddress);

            roomOptions.CustomRoomProperties = roomCustomProps;

            PhotonNetwork.CreateRoom(roomNameField.text, roomOptions);
            ipPanel.SetActive(false);
        }
    }

    public void OnClickLocal()
    {
        ipAddress = "127.0.0.1";
        portAddress = "7777";
        RoomOptions roomOptions =
        new RoomOptions()
        {
            MaxPlayers = 4,
            BroadcastPropsChangeToAll = true
        };

        Hashtable roomCustomProps = new Hashtable();

        roomCustomProps.Add("ip", ipAddress);
        roomCustomProps.Add("port", portAddress);

        roomOptions.CustomRoomProperties = roomCustomProps;

        PhotonNetwork.CreateRoom(roomNameField.text, roomOptions);
        ipPanel.SetActive(false);
    }

    public void OnClickLan()
    {
        ipAddress = "192.168.1.1";
        portAddress = "7777";
        RoomOptions roomOptions =
        new RoomOptions()
        {
            MaxPlayers = 4,
            BroadcastPropsChangeToAll = true
        };

        Hashtable roomCustomProps = new Hashtable();

        roomCustomProps.Add("ip", ipAddress);
        roomCustomProps.Add("port", portAddress);

        roomOptions.CustomRoomProperties = roomCustomProps;

        PhotonNetwork.CreateRoom(roomNameField.text, roomOptions);
        ipPanel.SetActive(false);
    }

    public override void OnJoinedRoom()
    {
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);
        roomName.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name;
        UpdatePlayerList();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (Time.time >= nextUpdateTime)
        {
            UpdateRoomList(roomList);
            nextUpdateTime = Time.time + timeBetweenUpdates;
        }
    }

    void UpdateRoomList(List<RoomInfo> list)
    {
        foreach (RoomItem item in roomItemsList)
        {
            Destroy(item.gameObject);
        }
        roomItemsList.Clear();

        foreach (RoomInfo room in list)
        {
            RoomItem newRoom = Instantiate(roomItemPrefab, contentObject);
            newRoom.SetRoomName(room.Name);
            roomItemsList.Add(newRoom);
        }
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void OnClickLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        roomPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public void UpdatePlayerList()
    {
        foreach (PlayerItem item in playerItemsList)
        {
            Destroy(item.gameObject);
        }
        playerItemsList.Clear();

        foreach (PlayerItem item in unassignedList)
        {
            Destroy(item);
        }
        unassignedList.Clear();

        foreach (PlayerItem item in team1List)
        {
            Destroy(item);
        }
        team1List.Clear();

        foreach (PlayerItem item in team2List)
        {
            Destroy(item);
        }
        team2List.Clear();

        if (PhotonNetwork.CurrentRoom == null)
        {
            return;
        }

        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            PlayerItem newPlayerItem = Instantiate(playerItemPrefab, unassignedItemParent);
            newPlayerItem.SetPlayerInfo(player.Value);
            newPlayerItem.transform.SetParent(parents[newPlayerItem.GetComponent<PlayerItem>().currentParentIndex]);

            if (newPlayerItem.GetComponent<PlayerItem>().currentParentIndex == 1)
            {
                team1List.Add(newPlayerItem);
            }
            if (newPlayerItem.GetComponent<PlayerItem>().currentParentIndex == 2)
            {
                team2List.Add(newPlayerItem);
            }
            if (newPlayerItem.GetComponent<PlayerItem>().currentParentIndex == 0)
            {
                unassignedList.Add(newPlayerItem);
            }

            playerItemsList.Add(newPlayerItem);
        }
    }

    public void OnTeamValueChanged()
    {
        UpdatePlayerList();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }


    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2 && team1List.Count > 0 && team2List.Count > 0 && unassignedList.Count == 0)
        {
            playButton.SetActive(true);
            errorContainer.SetActive(false);
            errorMessage.text = "";
        }

        else if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 3 && team1List.Count > 0 && team2List.Count > 0 && unassignedList.Count == 0)
        {
            playButton.SetActive(true);
            errorContainer.SetActive(false);
            errorMessage.text = "";
        }

        else if (PhotonNetwork.IsMasterClient && unassignedList.Count > 0)
        {
            errorContainer.SetActive(true);
            errorMessage.text = "There are unassigned players!";
            playButton.SetActive(false);
        }

        else if (PhotonNetwork.IsMasterClient && unassignedList.Count == 0 && team1List.Count == 0 || PhotonNetwork.IsMasterClient && team2List.Count == 0 && unassignedList.Count == 0)
        {
            errorContainer.SetActive(true);
            errorMessage.text = "One team is empty!";
            playButton.SetActive(false);
        }

        else if (PhotonNetwork.IsMasterClient && unassignedList.Count == 0 && PhotonNetwork.CurrentRoom.PlayerCount == 4 && team1List.Count == 3 || PhotonNetwork.IsMasterClient && unassignedList.Count == 0 && PhotonNetwork.CurrentRoom.PlayerCount == 4 && team2List.Count == 3)
        {
            errorContainer.SetActive(true);
            errorMessage.text = "These teams are unfair!";
            playButton.SetActive(false);
        }

        else if (PhotonNetwork.IsMasterClient && unassignedList.Count == 0 && PhotonNetwork.CurrentRoom.PlayerCount == 4 && team1List.Count == 2 || PhotonNetwork.IsMasterClient && unassignedList.Count == 0 && PhotonNetwork.CurrentRoom.PlayerCount == 4 && team2List.Count == 2)
        {
            errorContainer.SetActive(false);
            errorMessage.text = "";
            playButton.SetActive(true);
        }

        else if (!PhotonNetwork.IsMasterClient)
        {
            errorContainer.SetActive(false);
            errorMessage.text = "";
            playButton.SetActive(false);
        }

        else
        {
            playButton.SetActive(false);
        }
    }

    public void OnClickStartGame()
    {
        PhotonNetwork.LoadLevel("MainScene");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }


}

