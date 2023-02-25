using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Game : NetworkBehaviour
{
    public GameObject ballPrefab;
    public static Game instance;

    private GameObject ball;

    [Header("Conditions")]
    public NetworkVariable<bool> thrownUp = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> ballServed = new NetworkVariable<bool>(false);

    [Header("Volleyball Spawn Locations")]
    public Vector3 team1ServeLocation = new Vector3(-3.8f, 1.1f, 9);
    public Vector3 team2ServeLocation = new Vector3(3.8f, 1.1f, -9);

    [Header("Physics")]
    public int throwUpForce = 350;
    public float serveVerticalForce = 20;
    public float serveHorizontalForce = 13;

    private void Awake()
    {
        instance = this;
    }
    [ServerRpc] public void StartGameServerRpc()
    {
        StartGameClientRpc();
    }

    [ClientRpc] private void StartGameClientRpc()
    {
        SpawnBall();
    }

    private void SpawnBall()
    {
        ball = Instantiate(ballPrefab, team1ServeLocation, Quaternion.Euler(Vector3.zero));
        ball.GetComponent<Rigidbody>().isKinematic = true;
        ball.GetComponent<Rigidbody>().useGravity = false;
    }

    [ServerRpc(RequireOwnership = false)] public void ResetBallServerRpc()
    {
        ResetBallClientRpc();
        thrownUp.Value = false;
        ballServed.Value = false;
    }

    [ClientRpc] private void ResetBallClientRpc()
    {
        Destroy(ball);
        SpawnBall();
        
    }

    [ServerRpc(RequireOwnership = false)] public void SetThrownUpServerRpc(bool thrownUpValue)
    {
        thrownUp.Value = thrownUpValue;
    }

    [ServerRpc(RequireOwnership = false)] public void SetBallServedServerRpc(bool ballServedValue)
    {
        ballServed.Value = ballServedValue;
    }

    public void ThrowUp()
    {
        ball.GetComponent<Rigidbody>().isKinematic = false;
        ball.GetComponent<Rigidbody>().useGravity = true;
        ball.GetComponent<Rigidbody>().AddForce(new Vector3(0, throwUpForce, 0));
    }

    public void Serve(Quaternion playerRotation, Quaternion cameraRotation)
    {
        Vector3 playerRotationDirection = playerRotation * Vector3.forward;
        Vector3 cameraRotationDirection = cameraRotation * Vector3.up;
        Vector3 addedForce = playerRotationDirection * serveHorizontalForce + serveVerticalForce * cameraRotationDirection * -cameraRotation.x;

        ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        ball.GetComponent<Rigidbody>().AddForce(addedForce, ForceMode.Impulse);              
    }
}
