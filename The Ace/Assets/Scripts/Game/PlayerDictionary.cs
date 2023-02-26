using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Photon.Pun;

public class PlayerDictionary : NetworkBehaviour
{
    public Dictionary<ulong, GameObject> playerDictionary = new Dictionary<ulong, GameObject>();
    public int dictionaryCount = 0;
    public int playersInRoom;

    public static PlayerDictionary instance;

    private void Start()
    {
        instance = this;
        playersInRoom = PhotonNetwork.CurrentRoom.PlayerCount;
    }

    [ServerRpc]
    public void NewPlayerToDictServerRpc()
    {
        NewPlayerToDictClientRpc();
    }

    [ClientRpc]
    private void NewPlayerToDictClientRpc()
    {
        playerDictionary.Clear();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            playerDictionary.Add(player.GetComponent<NetworkObject>().OwnerClientId, player);
        }
        if (playerDictionary.Count == playersInRoom && IsServer)
        {
            GetComponent<Game>().StartGameServerRpc();
        }
    }

    [ServerRpc]
    public void RemovePlayerFromDictServerRpc(ulong clientId)
    {
        playerDictionary.Remove(clientId);
    }
}
