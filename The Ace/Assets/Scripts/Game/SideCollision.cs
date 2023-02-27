using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideCollision : MonoBehaviour
{
    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Ball"))
        {
            // Team 1 side borders
            if (Game.instance.teamLastTouch.Value == 1)
            {
                Game.instance.ScoreServerRpc(0);
            }

            else
            {
                Game.instance.ScoreServerRpc(1);
            }
        }
    }
}

