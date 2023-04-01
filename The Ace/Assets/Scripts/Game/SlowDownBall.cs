using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowDownBall : MonoBehaviour
{
    public float velocityModifier = 3.0f;

    // Detect if ball collided with the net
    private void OnCollisionEnter(Collision col)
    {
        col.collider.GetComponent<Rigidbody>().velocity = col.collider.GetComponent<Rigidbody>().velocity / velocityModifier;
    }
}
