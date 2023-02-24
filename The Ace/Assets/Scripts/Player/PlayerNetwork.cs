using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    private Vector3[] spawnLocations = new[] { new Vector3(0, 0, 0), new Vector3(-10, 0, 5) };

    private PlayerDictionary dictionary;
    void Start()
    {
        transform.position = spawnLocations[OwnerClientId];
        if (IsServer)
        {
            dictionary = GameObject.FindGameObjectWithTag("GameManager").GetComponent<PlayerDictionary>();
            if (dictionary == null)
            {
                Debug.Log("No dictionary found!");
            }
            else
            {
                dictionary.NewPlayerToDictServerRpc();
                Debug.Log("Adding Player to " + dictionary);
            }
        }
    }
}
