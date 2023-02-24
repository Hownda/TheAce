using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworkSelector : MonoBehaviour
{
    public Button hostButton;
    public Button serverButton;
    public Button clientButton;

    public void OnClickHost()
    {
        NetworkManager.Singleton.StartHost();
        DisableButtons();
    }

    public void OnClickServer()
    {
        NetworkManager.Singleton.StartServer();
        DisableButtons();
    }
    public void OnClickClient()
    {
        NetworkManager.Singleton.StartClient();
        DisableButtons();
    }

    private void DisableButtons()
    {
        hostButton.gameObject.SetActive(false);
        serverButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
    }
}
