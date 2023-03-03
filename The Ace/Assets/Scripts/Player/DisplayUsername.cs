using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using Photon.Pun;
using Unity.Collections;

public class DisplayUsername : NetworkBehaviour
{

    public TMP_Text usernameText;
    private NetworkVariable<FixedString32Bytes> playerName = new(writePerm: NetworkVariableWritePermission.Owner);

    void Update()
    {
        if (IsOwner)
        {
            playerName.Value = PhotonNetwork.LocalPlayer.NickName;
            usernameText.text = playerName.Value.ToString();
        }
        else
        {
            usernameText.text = playerName.Value.ToString();
        }
    }
}
