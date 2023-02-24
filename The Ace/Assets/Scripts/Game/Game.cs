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
    private int throwUpForce = 350;

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

    [ServerRpc] public void SetThrownUpServerRpc(bool thrownUpValue)
    {
        thrownUp.Value = thrownUpValue;
    }

    [ServerRpc] public void SetBallServedServerRpc(bool ballServedValue)
    {
        ballServed.Value = ballServedValue;
    }

    public void ThrowUp()
    {
        ball.GetComponent<Rigidbody>().isKinematic = false;
        ball.GetComponent<Rigidbody>().useGravity = true;
        ball.GetComponent<Rigidbody>().AddForce(new Vector3(0, throwUpForce, 0));
    }

    public void Serve()
    {

    }



}
