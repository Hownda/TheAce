using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Photon.Pun;
using Unity.Netcode.Transports.UTP;

public class PreNetwork : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
            (string)PhotonNetwork.CurrentRoom.CustomProperties["ip"],
            (ushort)System.Convert.ToInt32((string)PhotonNetwork.CurrentRoom.CustomProperties["port"]),
            "0.0.0.0"
        );
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                OnHostClick();
            }
            else
            {
                StartCoroutine(OnClientClick());
            }
        }
    }

    IEnumerator OnClientClick()
    {
        yield return new WaitForSeconds(0.05f);
        NetworkManager.Singleton.StartClient();

    }

    private void OnHostClick()
    {
        NetworkManager.Singleton.StartHost();
    }

}