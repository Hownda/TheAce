using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerInteraction : NetworkBehaviour
{
    private void Update()
    {
        DetectServe();
        if (Input.GetKeyDown("r") && IsOwner)
        {
            Game.instance.ResetBallServerRpc();
        }
    }
    private void DetectServe()
    {
        // Right Mouse Button
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (Game.instance.thrownUp.Value == true)
            {
                return;
            }
            else
            {
                if (IsOwner)
                {
                    Game.instance.ThrowUp();
                    ThrowUpServerRpc();
                }
                Game.instance.SetThrownUpServerRpc(true);
                //GameManager.instance.UpdateCanReceiveServerRpc((int)PhotonNetwork.LocalPlayer.CustomProperties["team"]);
            }
        }

        // Left Mouse Button
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (!Game.instance.thrownUp.Value == true)
            {
                return;
            }
            else
            {
                if (IsOwner)
                {
                    Quaternion cameraRotation = GetComponentInChildren<Camera>().transform.localRotation;
                    Game.instance.Serve(transform.rotation, cameraRotation);
                }

                // Makes first collider hit valid
                Game.instance.SetBallServedServerRpc(true);
            }
        }
    }

    
    [ServerRpc] public void ThrowUpServerRpc()
    {
        ThrowUpClientRpc();
    }

    
    [ClientRpc] private void ThrowUpClientRpc()
    {
        if (!IsOwner)
        {
            Game.instance.ThrowUp();
        }
    }

    [ServerRpc] public void ServeServerRpc(Quaternion playerRotation, Quaternion cameraRotation)
    {
        ServeClientRpc(playerRotation, cameraRotation);
    }

    [ClientRpc] private void ServeClientRpc(Quaternion playerRotation, Quaternion cameraRotation)
    {
        if (!IsOwner)
        {
            Game.instance.Serve(playerRotation, cameraRotation);
        }
    }
}
