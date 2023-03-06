using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class PlayerInteraction : NetworkBehaviour
{
    public float receiveHeight;
    [SerializeField] private AudioClip clip;

    public Slider hitSlider;
    private float currentTime;
    private float hitCooldown = 0.5f;

    private void Update()
    {
        DetectServe();
        DetectReceive();

        // Reset ball (for debugging purposes)
        if (Input.GetKeyDown("r") && IsOwner)
        {            
            Game.instance.ResetBallServerRpc();
        }
        if (IsOwner) {
            currentTime += Time.deltaTime;
            currentTime = Mathf.Clamp(currentTime, 0.0f, hitCooldown);
            hitSlider.value = currentTime / hitCooldown;
        }
        else
        {
            hitSlider.gameObject.SetActive(false);
        }

        if (Game.instance.playersReady.Value && IsOwner)
        {
            hitSlider.gameObject.SetActive(true);
        }
    }
    private void DetectServe()
    {   if (Game.instance.ball == null)
        {
            return;
        }

        if (Vector3.Distance(transform.position, Game.instance.ball.transform.position) <= 2 && OwnerClientId == Game.instance.playerToServe.Value && IsOwner && Game.instance.playersReady.Value)
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
                    Game.instance.ThrowUp();
                    AudioManager.instance.PlaySound(clip);
                    ThrowUpServerRpc();
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
                    if (currentTime >= hitCooldown)
                    {
                        Quaternion cameraRotation = GetComponentInChildren<Camera>().transform.localRotation;
                        Game.instance.Serve(transform.rotation, cameraRotation);
                        AudioManager.instance.PlaySound(clip);
                        ServeServerRpc(transform.rotation, cameraRotation);
                        currentTime = 0;

                        Game.instance.SetBallServedServerRpc(true);
                        Game.instance.SetValidPointServerRpc(true);
                        Game.instance.SetLastTouchServerRpc(OwnerClientId);
                    }              
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
                if (IsOwner && Game.instance.ball.transform.position.y >= receiveHeight && currentTime >= hitCooldown)
                {
                    Quaternion cameraRotation = GetComponentInChildren<Camera>().transform.localRotation;
                    Game.instance.ReceiveHigh(transform.rotation, cameraRotation);
                    AudioManager.instance.PlaySound(clip);
                    ReceiveHighServerRpc(transform.rotation, cameraRotation);
                    currentTime = 0;
                }
                Game.instance.SetLastTouchServerRpc(OwnerClientId);
            }
            // Detect right click
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (IsOwner && GetComponent<PlayerMovement>().isGrounded && Game.instance.ball.transform.position.y <= receiveHeight && currentTime >= hitCooldown)
                {
                    Quaternion cameraRotation = GetComponentInChildren<Camera>().transform.localRotation;
                    Game.instance.ReceiveLow(transform.rotation, cameraRotation);
                    AudioManager.instance.PlaySound(clip);
                    ReceiveLowServerRpc(transform.rotation, cameraRotation);
                    currentTime = 0;
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
