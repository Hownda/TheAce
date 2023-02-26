using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerItem : MonoBehaviourPunCallbacks
{
    Player player;
    public Text playerName;
    public static PlayerItem instance;
    public int currentParentIndex;
    private PhotonView PV;

    public string ipAddress;
    public string portAddress;

    public ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        PV = GetComponent<PhotonView>();
    }

    public void SetPlayerInfo(Player _player)
    {
        playerName.text = _player.NickName;
        player = _player;
        UpdatePlayerItem(player);
    }

    public void OnClickUnassigned()
    {
        playerProperties["team"] = 0;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }

    public void OnClickTeam1()
    {
        playerProperties["team"] = 1;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }

    public void OnClickTeam2()
    {
        playerProperties["team"] = 2;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (player == targetPlayer)
        {
            UpdatePlayerItem(targetPlayer);
            LobbyManager.instance.OnTeamValueChanged();
        }
    }

    void UpdatePlayerItem(Player player)
    {
        if (player.CustomProperties.ContainsKey("team"))
        {
            currentParentIndex = (int)player.CustomProperties["team"];
            playerProperties["team"] = (int)player.CustomProperties["team"];
        }
        else
        {
            playerProperties["team"] = 0;
        }

    }
}

