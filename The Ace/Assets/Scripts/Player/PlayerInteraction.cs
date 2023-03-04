using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerInteraction : NetworkBehaviour
{
    public float receiveHeight;
    [SerializeField] private AudioClip clip;
    private float hitCooldown = 0.2f;
    private float timeStamp;
    private void Update()
    {
        DetectServe();
        DetectReceive();

        // Reset ball (for debugging purposes)
        if (Input.GetKeyDown("r") && IsOwner)
        {            
            Game.instance.ResetBallServerRpc();
        }
    }
    private void DetectServe()
    {   if (Game.instance.ball == null)
        {
            return;
        }

        if (Vector3.Distance(transform.position, Game.instance.ball.transform.position) <= 2 && OwnerClientId == Game.instance.playerToServe.Value)
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
                        AudioManager.instance.PlaySound(clip);
                        ThrowUpServerRpc();                        
                    }
                    Game.instance.SetThrownUpServerRpc(true);
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
                    if (IsOwner && timeStamp <= Time.time)
                    {
                        Quaternion cameraRotation = GetComponentInChildren<Camera>().transform.localRotation;
                        Game.instance.Serve(transform.rotation, cameraRotation);
                        AudioManager.instance.PlaySound(clip);
                        ServeServerRpc(transform.rotation, cameraRotation);
                        timeStamp = Time.time + hitCooldown;
                    }

                    // Makes first collider hit valid
                    Game.instance.SetBallServedServerRpc(true);
                    Game.instance.SetValidPointServerRpc(true);
                    Game.instance.SetLastTouchServerRpc(OwnerClientId);
                }
            }
        }
    }

    private void DetectReceive()
    {
        if (Game.instance.ball == null)
        {
            return;
        }

        if (Vector3.Distance(transform.position, Game.instance.ball.transform.position) <= 2 && Game.instance.ballServed.Value == true && GetComponent<PlayerNetwork>().teamIndex.Value == Game.instance.canReceive.Value )
        {
            // Detect left click
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (IsOwner && Game.instance.ball.transform.position.y >= receiveHeight && timeStamp <= Time.time)
                {
                    Quaternion cameraRotation = GetComponentInChildren<Camera>().transform.localRotation;
                    Game.instance.ReceiveHigh(transform.rotation, cameraRotation);
                    AudioManager.instance.PlaySound(clip);
                    ReceiveHighServerRpc(transform.rotation, cameraRotation);
                    timeStamp = Time.time + hitCooldown;
                }
                Game.instance.SetLastTouchServerRpc(OwnerClientId);
            }
            // Detect right click
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (IsOwner && GetComponent<PlayerMovement>().isGrounded && Game.instance.ball.transform.position.y <= receiveHeight && timeStamp <= Time.time)
                {
                    Quaternion cameraRotation = GetComponentInChildren<Camera>().transform.localRotation;
                    Game.instance.ReceiveLow(transform.rotation, cameraRotation);
                    AudioManager.instance.PlaySound(clip);
                    ReceiveLowServerRpc(transform.rotation, cameraRotation);
                    timeStamp = Time.time + hitCooldown;
                }
                Game.instance.SetLastTouchServerRpc(OwnerClientId);
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

    [ServerRpc] private void ReceiveHighServerRpc(Quaternion playerRotation, Quaternion cameraRotation)
    {
        ReceiveHighClientRpc(playerRotation, cameraRotation);
    }

    [ClientRpc] private void ReceiveHighClientRpc(Quaternion playerRotation, Quaternion cameraRotation)
    {
        if (!IsOwner)
        {
            Game.instance.ReceiveHigh(playerRotation, cameraRotation);
        }
    }

    [ServerRpc] private void ReceiveLowServerRpc(Quaternion playerRotation, Quaternion cameraRotation)
    {
        ReceiveLowClientRpc(playerRotation, cameraRotation);
    }

    [ClientRpc] private void ReceiveLowClientRpc(Quaternion playerRotation, Quaternion cameraRotation)
    {
        if (!IsOwner)
        {
            Game.instance.ReceiveLow(playerRotation, cameraRotation);
        }
    }
}
