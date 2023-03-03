using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class Billboard : NetworkBehaviour
{
    Camera cam;
    void Update()
    {
        if (cam == null)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                if (player.GetComponent<NetworkObject>().OwnerClientId != OwnerClientId) 
                {
                    cam = player.GetComponentInChildren<Camera>();
                }
            }
                
        }
        if (cam == null)
        {
            return;
        }
        transform.LookAt(cam.transform);
    }
}
