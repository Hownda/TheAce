using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Photon.Pun;
using UnityEngine.UI;

public class Game : NetworkBehaviour
{
    public GameObject ballPrefab;
    public static Game instance;
    public GameObject loadingScreen;

    [HideInInspector] public GameObject ball;

    [Header("Conditions")]
    public NetworkVariable<bool> thrownUp = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> ballServed = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> validPoint = new NetworkVariable<bool>(true);
    public NetworkVariable<int> teamToServe = new NetworkVariable<int>(0);
    public NetworkVariable<ulong> playerToServe = new NetworkVariable<ulong>();
    public NetworkVariable<int> canReceive = new NetworkVariable<int>(1);

    public NetworkVariable<int> scoreTeam1 = new NetworkVariable<int>(0);
    public NetworkVariable<int> scoreTeam2 = new NetworkVariable<int>(0);

    public NetworkVariable<ulong> team1LastPlayerToServe = new NetworkVariable<ulong>(4);
    public NetworkVariable<ulong> team2LastPlayerToServe = new NetworkVariable<ulong>(4);
    private NetworkVariable<int> lastTeamToServe = new NetworkVariable<int>(0);
    public NetworkVariable<ulong> playerLastTouch = new NetworkVariable<ulong>();
    public NetworkVariable<ulong> playerPreviousTouch = new NetworkVariable<ulong>(4);
    public NetworkVariable<int> teamLastTouch = new NetworkVariable<int>();

    public NetworkVariable<int> team1Touches = new NetworkVariable<int>(0);
    public NetworkVariable<int> team2Touches = new NetworkVariable<int>(0);

    public NetworkVariable<bool> playersReady = new NetworkVariable<bool>(false);

    private List<ulong> team1 = new List<ulong>();
    private List<ulong> team2 = new List<ulong>();

    public Vector3[] spawnLocations;
    public Vector3[] spawnRotations;

    [Header("Volleyball Spawn Locations")]
    public Vector3 team2ServeLocation = new Vector3(-3.8f, 1.1f, 8.7f);
    public Vector3 team1ServeLocation = new Vector3(3.8f, 1.1f, -8.7f);

    private int throwUpForce = 350;
    private float serveVerticalForce = 8;
    private float serveHorizontalForce = 7;
    private float receiveForce = 1;
    private float receiveForceUp = 8.5f;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        SwitchReceive();
        //CountTouches();
    }

    private void SwitchReceive()
    {
        if (ball == null)
        {
            return;
        }
        if (ball.transform.position.y >= 0.84 && ball.transform.position.z > 0 && teamLastTouch.Value == 0 && ballServed.Value == true)
        {
            canReceive.Value = 1;
            team1Touches.Value = 0;
        }
        else if (ball.transform.position.y >= 0.84 && ball.transform.position.z < 0 && teamLastTouch.Value == 1 && ballServed.Value == true)
        {
            canReceive.Value = 0;
            team2Touches.Value = 0;
        }
    }

    /*private void CountTouches()
    {
        if (team1.Count == 2)
        {
            if (team1Touches.Value > 3)
            {
                ScoreServerRpc(1);
            }
            if (team1Touches.Value == 2 && playerLastTouch.Value == playerPreviousTouch.Value)
            {
                ScoreServerRpc(1);
            }
        }
        if (team1.Count < 2)
        {
            if (team1Touches.Value > 2)
            {
                ScoreServerRpc(1);
            }
        }
        if (team2.Count == 2)
        {
            if (team2Touches.Value > 3)
            {
                ScoreServerRpc(0);
            }
            if (team2Touches.Value == 2 && playerLastTouch.Value == playerPreviousTouch.Value)
            {
                ScoreServerRpc(0);
            }
        }
        if (team2.Count < 2)
        {
            if (team2Touches.Value > 2)
            {
                ScoreServerRpc(0);
            }
        }
    }*/

    [ServerRpc] public void StartGameServerRpc()
    {
        StartGameClientRpc();
    }

    [ClientRpc] private void StartGameClientRpc()
    {
        ResetBallClientRpc();
        StartCoroutine(SyncPlayerTeams());
        
    }

    private IEnumerator SyncPlayerTeams()
    {
        yield return new WaitForSeconds(1);
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<PlayerNetwork>().teamIndex.Value == 0)
            {
                if (!team1.Contains(player.GetComponent<NetworkObject>().OwnerClientId))
                {
                    team1.Add(player.GetComponent<NetworkObject>().OwnerClientId);
                }
            }
            else
            {
                if (!team2.Contains(player.GetComponent<NetworkObject>().OwnerClientId))
                team2.Add(player.GetComponent<NetworkObject>().OwnerClientId);
            }
        }
        UpdatePlayerToServeServerRpc();
        Debug.Log("Updating Serve");
    }

    private void SpawnBall()
    {
        Vector3 spawnPoint;
        if (teamToServe.Value == 0)
        {
            spawnPoint = team1ServeLocation;
            SetCanReceiveServerRpc(1);
        }
        else
        {
            spawnPoint = team2ServeLocation;
            SetCanReceiveServerRpc(0);
        }
        ball = Instantiate(ballPrefab, spawnPoint, Quaternion.Euler(Vector3.zero));
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

    [ServerRpc] public void ScoreServerRpc(int teamIndex)
    {
        if (validPoint.Value == false)
        {
            return;
        }
        else
        {
            validPoint.Value = false;

            if (teamIndex == 0)
            {
                teamToServe.Value = 0;
                Debug.Log("Team 1 scored");
                scoreTeam1.Value += 1;
            }
            else
            {
                teamToServe.Value = 1;
                Debug.Log("Team 2 scored");
                scoreTeam2.Value += 1;
            }
            StartCoroutine(ballResetDelay());
            team1Touches.Value = 0;
            team2Touches.Value = 0;
        }       
    }

    private IEnumerator ballResetDelay()
    {
        UpdatePlayerToServeServerRpc();
        yield return new WaitForSeconds(1);
        ResetBallServerRpc();
        
    }

    [ServerRpc(RequireOwnership = false)] public void SetThrownUpServerRpc(bool thrownUpValue)
    {
        thrownUp.Value = thrownUpValue;
    }

    [ServerRpc(RequireOwnership = false)] public void SetBallServedServerRpc(bool ballServedValue)
    {
        ballServed.Value = ballServedValue;
    }

    [ServerRpc(RequireOwnership = false)] public void SetValidPointServerRpc(bool validPointValue)
    {
        validPoint.Value = validPointValue;
    }

    [ServerRpc(RequireOwnership = false)] public void SetLastTouchServerRpc(ulong clientId)
    {
        playerPreviousTouch.Value = playerLastTouch.Value;
        playerLastTouch.Value = clientId;
        if (team1.Contains(clientId))
        {
            teamLastTouch.Value = 0;
            team1Touches.Value += 1;
        }
        else if (team2.Contains(clientId))
        {
            teamLastTouch.Value = 1;
            team2Touches.Value += 1;
        }
    }   
    
    [ServerRpc(RequireOwnership = false)] private void SetCanReceiveServerRpc(int canReceiveValue)
    {
        canReceive.Value = canReceiveValue;
    }

    [ServerRpc(RequireOwnership = false)] private void SetPlayersReadyServerRpc(bool playersReadyValue)
    {
        playersReady.Value = playersReadyValue;
    }

    [ServerRpc(RequireOwnership = false)] public void SetLastPlayerToServeServerRpc(ulong clientId)
    {
        if (team1.Contains(clientId))
        {
            team1LastPlayerToServe.Value = clientId;
        }
        if (team2.Contains(clientId))
        {
            team2LastPlayerToServe.Value = clientId;
        }
    }

    public void ThrowUp()
    {
        ball.GetComponent<Rigidbody>().isKinematic = false;
        ball.GetComponent<Rigidbody>().useGravity = true;
        ball.GetComponent<Rigidbody>().AddForce(new Vector3(0, throwUpForce, 0));
    }

    public void Serve(Vector3 playerShootDirection, Vector3 cameraDirection)
    {
        Vector3 addedForce = playerShootDirection * serveHorizontalForce + serveVerticalForce * cameraDirection;

        ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        ball.GetComponent<Rigidbody>().AddForce(addedForce, ForceMode.Impulse);
    }

    public void ReceiveHigh(Vector3 playerShootDirection, Vector3 cameraDirection)
    {
        Serve(playerShootDirection, cameraDirection);     
    }

    public void ReceiveLow(Vector3 playerShootDirection, Vector3 cameraDirection)
    {
        Vector3 forceToAdd = playerShootDirection * receiveForce + receiveForceUp * Vector3.up;

        ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        ball.GetComponent<Rigidbody>().AddForce(forceToAdd, ForceMode.Impulse);
    }

    #region
    [ServerRpc]
    public void UpdatePlayerToServeServerRpc()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            if (teamToServe.Value == 0)
            {
                playerToServe.Value = team1[0];
            }
            else
            {
                playerToServe.Value = team2[0];
            }
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount >= 3)
        {
            if (teamToServe.Value == 0)
            {
                if (team1.Count < 2)
                {
                    playerToServe.Value = team1[0];
                }

                if (team1.Count == 2 && lastTeamToServe.Value == 1)
                {
                    for (int i = 0; i < team1.Count; i++)
                    {
                        if (team1LastPlayerToServe.Value == team1[i])
                        {
                            if (i == 1)
                            {
                                playerToServe.Value = team1[0];
                            }
                            if (i == 0)
                            {
                                playerToServe.Value = team1[1];
                            }
                        }
                        if (team1LastPlayerToServe.Value == 4)
                        {
                            playerToServe.Value = team1[0];
                        }
                    }
                }
                if (team1.Count == 2 && lastTeamToServe.Value == 0 && team1LastPlayerToServe.Value != 4)
                {
                    playerToServe.Value = team1LastPlayerToServe.Value;
                }
                if (team1.Count == 2 && lastTeamToServe.Value == 0 && team1LastPlayerToServe.Value == 4)
                {
                    playerToServe.Value = team1[0];
                }
            }

            else if (teamToServe.Value == 1)
            {
                if (team2.Count < 2)
                {
                    playerToServe.Value = team2[0];
                }

                else if (team2.Count == 2 && lastTeamToServe.Value == 0)
                {
                    for (int i = 0; i < team2.Count; i++)
                    {
                        if (team2LastPlayerToServe.Value == team2[i])
                        {
                            if (i == 1)
                            {
                                playerToServe.Value = team2[0];
                            }
                            else if (i == 0)
                            {
                                playerToServe.Value = team2[1];
                            }
                        }

                        if (team2LastPlayerToServe.Value == 4)
                        {
                            playerToServe.Value = team2[0];
                        }
                    }
                }
                else if (team2.Count == 2 && lastTeamToServe.Value == 1)
                {
                    playerToServe.Value = team2LastPlayerToServe.Value;
                }
            }
        }
        lastTeamToServe.Value = teamToServe.Value;
        StartCoroutine(prepareServeDelay());
    }

    private IEnumerator prepareServeDelay()
    {
        yield return new WaitForSeconds(1);
        prepareServeServerRpc();
    }

    [ServerRpc]
    public void prepareServeServerRpc()
    {
        prepareServeClientRpc();
    }

    [ClientRpc] private void prepareServeClientRpc()
    {
        if (teamToServe.Value == 0)
        {
            for (int i = 0; i < team1.Count; i++)
            {
                if (team1[i] == playerToServe.Value)
                {

                    GameObject player = PlayerDictionary.instance.playerDictionary[team1[i]];
                    Vector3 moveVector = spawnLocations[0] - player.transform.position;
                    player.GetComponent<CharacterController>().detectCollisions = false;
                    player.GetComponent<CharacterController>().Move(moveVector);

                    if (player.transform.position != spawnLocations[0])
                    {
                        moveVector = spawnLocations[0] - player.transform.position;
                        player.GetComponent<CharacterController>().Move(moveVector);

                    }

                    player.GetComponent<CharacterController>().detectCollisions = true;
                    player.transform.rotation = Quaternion.Euler(spawnRotations[0]);
                }
                else
                {
                    GameObject player = PlayerDictionary.instance.playerDictionary[team1[i]];
                    Vector3 moveVector = spawnLocations[1] - player.transform.position;
                    player.GetComponent<CharacterController>().detectCollisions = false;
                    player.GetComponent<CharacterController>().Move(moveVector);

                    if (player.transform.position != spawnLocations[1])
                    {
                        moveVector = spawnLocations[1] - player.transform.position;
                        player.GetComponent<CharacterController>().Move(moveVector);

                    }

                    player.GetComponent<CharacterController>().detectCollisions = true;
                    player.transform.rotation = Quaternion.Euler(spawnRotations[1]);
                }
            }
            for (int i = 0; i < team2.Count; i++)
            {
                if (team2[i] == team2LastPlayerToServe.Value && team2LastPlayerToServe.Value != 4)
                {
                    GameObject player = PlayerDictionary.instance.playerDictionary[team2[i]];
                    Vector3 moveVector = spawnLocations[2] - player.transform.position;
                    player.GetComponent<CharacterController>().detectCollisions = false;
                    player.GetComponent<CharacterController>().Move(moveVector);

                    if (player.transform.position != spawnLocations[2])
                    {
                        moveVector = spawnLocations[2] - player.transform.position;
                        player.GetComponent<CharacterController>().Move(moveVector);

                    }

                    player.GetComponent<CharacterController>().detectCollisions = true;
                    player.transform.rotation = Quaternion.Euler(spawnRotations[2]);
                }
                if (team2LastPlayerToServe.Value == 4)
                {
                    GameObject player = PlayerDictionary.instance.playerDictionary[team2[i]];
                    Vector3 moveVector = spawnLocations[i + 2] - player.transform.position;
                    player.GetComponent<CharacterController>().detectCollisions = false;
                    player.GetComponent<CharacterController>().Move(moveVector);

                    if (player.transform.position != spawnLocations[i + 2])
                    {
                        moveVector = spawnLocations[i + 2] - player.transform.position;
                        player.GetComponent<CharacterController>().Move(moveVector);

                    }

                    player.GetComponent<CharacterController>().detectCollisions = true;
                    player.transform.rotation = Quaternion.Euler(spawnRotations[i + 2]);
                }
                if (team2[i] != team2LastPlayerToServe.Value && team2LastPlayerToServe.Value != 4)
                {
                    GameObject player = PlayerDictionary.instance.playerDictionary[team2[i]];
                    Vector3 moveVector = spawnLocations[3] - player.transform.position;
                    player.GetComponent<CharacterController>().detectCollisions = false;
                    player.GetComponent<CharacterController>().Move(moveVector);

                    if (player.transform.position != spawnLocations[3])
                    {
                        moveVector = spawnLocations[3] - player.transform.position;
                        player.GetComponent<CharacterController>().Move(moveVector);

                    }

                    player.GetComponent<CharacterController>().detectCollisions = true;
                    player.transform.rotation = Quaternion.Euler(spawnRotations[3]);
                }
            }
        }

        else if (teamToServe.Value == 1)
        {
            for (int i = 0; i < team2.Count; i++)
            {
                if (team2[i] == playerToServe.Value)
                {

                    GameObject player = PlayerDictionary.instance.playerDictionary[team2[i]];
                    Vector3 moveVector = spawnLocations[2] - player.transform.position;
                    player.GetComponent<CharacterController>().detectCollisions = false;
                    player.GetComponent<CharacterController>().Move(moveVector);

                    if (player.transform.position != spawnLocations[2])
                    {
                        moveVector = spawnLocations[2] - player.transform.position;
                        player.GetComponent<CharacterController>().Move(moveVector);

                    }

                    player.GetComponent<CharacterController>().detectCollisions = true;
                    player.transform.rotation = Quaternion.Euler(spawnRotations[2]);
                }
                else
                {
                    GameObject player = PlayerDictionary.instance.playerDictionary[team2[i]];
                    Vector3 moveVector = spawnLocations[3] - player.transform.position;
                    player.GetComponent<CharacterController>().detectCollisions = false;
                    player.GetComponent<CharacterController>().Move(moveVector);

                    if (player.transform.position != spawnLocations[3])
                    {
                        moveVector = spawnLocations[3] - player.transform.position;
                        player.GetComponent<CharacterController>().Move(moveVector);

                    }

                    player.GetComponent<CharacterController>().detectCollisions = true;
                    player.transform.rotation = Quaternion.Euler(spawnRotations[3]);
                }
            }
            for (int i = 0; i < team1.Count; i++)
            {
                if (team1[i] == team1LastPlayerToServe.Value && team1LastPlayerToServe.Value != 4)
                {
                    GameObject player = PlayerDictionary.instance.playerDictionary[team1[i]];
                    Vector3 moveVector = spawnLocations[0] - player.transform.position;
                    player.GetComponent<CharacterController>().detectCollisions = false;
                    player.GetComponent<CharacterController>().Move(moveVector);

                    if (player.transform.position != spawnLocations[0])
                    {
                        moveVector = spawnLocations[0] - player.transform.position;
                        player.GetComponent<CharacterController>().Move(moveVector);

                    }

                    player.GetComponent<CharacterController>().detectCollisions = true;
                    player.transform.rotation = Quaternion.Euler(spawnRotations[0]);
                }
                if (team1LastPlayerToServe.Value == 4)
                {
                    GameObject player = PlayerDictionary.instance.playerDictionary[team1[i]];
                    Vector3 moveVector = spawnLocations[i] - player.transform.position;
                    player.GetComponent<CharacterController>().detectCollisions = false;
                    player.GetComponent<CharacterController>().Move(moveVector);

                    if (player.transform.position != spawnLocations[i])
                    {
                        moveVector = spawnLocations[i] - player.transform.position;
                        player.GetComponent<CharacterController>().Move(moveVector);

                    }

                    player.GetComponent<CharacterController>().detectCollisions = true;
                    player.transform.rotation = Quaternion.Euler(spawnRotations[i]);
                }
                if (team1[i] != team1LastPlayerToServe.Value && team1LastPlayerToServe.Value != 4)
                {
                    GameObject player = PlayerDictionary.instance.playerDictionary[team1[i]];
                    Vector3 moveVector = spawnLocations[1] - player.transform.position;
                    player.GetComponent<CharacterController>().detectCollisions = false;
                    player.GetComponent<CharacterController>().Move(moveVector);

                    if (player.transform.position != spawnLocations[1])
                    {
                        moveVector = spawnLocations[1] - player.transform.position;
                        player.GetComponent<CharacterController>().Move(moveVector);

                    }

                    player.GetComponent<CharacterController>().detectCollisions = true;
                    player.transform.rotation = Quaternion.Euler(spawnRotations[1]);
                }
            }
        }
        if (loadingScreen.activeInHierarchy)
        {
            StartCoroutine(DisableLoadingScreen());
        }
    }

    private IEnumerator DisableLoadingScreen()
    {
        yield return new WaitForSeconds(2);
        loadingScreen.SetActive(false);
        SetPlayersReadyServerRpc(true);

    }
    #endregion
}


