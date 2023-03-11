using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class PlayerInteraction : NetworkBehaviour
{
    public float receiveHeight;
    [SerializeField] private AudioClip clip;
    private Animator animator;

    public Slider hitSlider;
    private float currentTime;
    private float hitCooldown = 0.5f;


    private void Start()
    {
        animator = GetComponent<Animator>();
    }
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

        if (Vector3.Distance(transform.position, Game.instance.ball.transform.position) <= 2.3f && OwnerClientId == Game.instance.playerToServe.Value && IsOwner && Game.instance.playersReady.Value)
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
                        Vector3 cameraDirection = GetComponentInChildren<Camera>().transform.forward;
                        Game.instance.Serve(transform.forward, cameraDirection);
                        AudioManager.instance.PlaySound(clip);
                        ServeServerRpc(transform.forward, cameraDirection);
                        currentTime = 0;

                        Game.instance.SetBallServedServerRpc(true);
                        Game.instance.SetValidPointServerRpc(true);
                        Game.instance.SetLastTouchServerRpc(OwnerClientId);
                        Game.instance.SetLastPlayerToServeServerRpc(OwnerClientId);
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

        if (Vector3.Distance(transform.position, Game.instance.ball.transform.position) <= 2.3f && Game.instance.ballServed.Value == true && GetComponent<PlayerNetwork>().teamIndex.Value == Game.instance.canReceive.Value )
        {      
            // Detect left click
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (IsOwner && Game.instance.ball.transform.position.y >= receiveHeight && currentTime >= hitCooldown)
                {
                    Vector3 cameraDirection = GetComponentInChildren<Camera>().transform.forward;
                    Game.instance.ReceiveHigh(transform.forward, cameraDirection);
                    AudioManager.instance.PlaySound(clip);
                    ReceiveHighServerRpc(transform.forward, cameraDirection);
                    currentTime = 0;

                    animator.SetTrigger("Receive");
                }
                Game.instance.SetLastTouchServerRpc(OwnerClientId);
            }
            // Detect right click
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (IsOwner && GetComponent<PlayerMovement>().isGrounded && Game.instance.ball.transform.position.y <= receiveHeight && currentTime >= hitCooldown)
                {
                    Vector3 cameraDirection = GetComponentInChildren<Camera>().transform.forward;
                    Game.instance.ReceiveLow(transform.forward, cameraDirection);
                    AudioManager.instance.PlaySound(clip);
                    ReceiveLowServerRpc(transform.forward, cameraDirection);
                    currentTime = 0;

                    animator.SetTrigger("Receive");
                    AnimateReceiveServerRpc();
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

    [ServerRpc] public void ServeServerRpc(Vector3 playerShootDirection, Vector3 cameraDirection)
    {
        ServeClientRpc(playerShootDirection, cameraDirection);
    }

    [ClientRpc] private void ServeClientRpc(Vector3 playerShootDirection, Vector3 cameraDirection)
    {
        if (!IsOwner)
        {
            Game.instance.Serve(playerShootDirection, cameraDirection);
        }
    }

    [ServerRpc] private void ReceiveHighServerRpc(Vector3 playerShootDirection, Vector3 cameraDirection)
    {
        ReceiveHighClientRpc(playerShootDirection, cameraDirection);
    }

    [ClientRpc] private void ReceiveHighClientRpc(Vector3 playerShootDirection, Vector3 cameraDirection)
    {
        if (!IsOwner)
        {
            Game.instance.ReceiveHigh(playerShootDirection, cameraDirection);
        }
    }

    [ServerRpc] private void ReceiveLowServerRpc(Vector3 playerShootDirection, Vector3 cameraDirection)
    {
        ReceiveLowClientRpc(playerShootDirection, cameraDirection);
    }

    [ClientRpc] private void ReceiveLowClientRpc(Vector3 playerShootDirection, Vector3 cameraDirection)
    {
        if (!IsOwner)
        {
            Game.instance.ReceiveLow(playerShootDirection, cameraDirection);
        }
    }

    [ServerRpc] private void AnimateReceiveServerRpc()
    {
        AnimateReceiveClientRpc();
    }

    [ClientRpc] private void AnimateReceiveClientRpc()
    {
        if (!IsOwner)
        {
            animator.SetTrigger("Receive");
        }
    }
}
