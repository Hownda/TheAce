using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private void DetectServe()
    {
        // Right Mouse Button
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (Game.instance.thrownUp.Value)
            {
                return;
            }
            else
            {
                Game.instance.ThrowUp();
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
                Quaternion cameraRotation = GetComponentInChildren<Camera>().transform.localRotation;
                Game.instance.Serve();

                // Makes first collider hit valid
                Game.instance.SetBallServedServerRpc(true);
            }
        }
    }
}
