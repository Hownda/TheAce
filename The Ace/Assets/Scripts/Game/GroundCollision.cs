using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ball"))
        {
            if (col.transform.position.z > 0)
                Game.instance.ScoreServerRpc(0);
            else
                Game.instance.ScoreServerRpc(1);
        }
    }
}
