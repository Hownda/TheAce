using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Photon.Pun;

public class PlayerNetwork : NetworkBehaviour
{
    private Vector3[] spawnLocations = new[] { new Vector3(0, 0, 0), new Vector3(-10, 0, 5) };
    public NetworkVariable<int> teamIndex = new(writePerm: NetworkVariableWritePermission.Owner);

    private PlayerDictionary dictionary;
    void Start()
    {
        transform.position = spawnLocations[OwnerClientId];
        dictionary = GameObject.FindGameObjectWithTag("GameManager").GetComponent<PlayerDictionary>();
        if (IsServer)
        {
            if (dictionary == null)
            {
                Debug.Log("No dictionary found!");
            }
            else
            {
                dictionary.NewPlayerToDictServerRpc();
            }
        }
        if (IsOwner)
        {
            teamIndex.Value = (int)PhotonNetwork.LocalPlayer.CustomProperties["team"];
        }
    }

        
}
