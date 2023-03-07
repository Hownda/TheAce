using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Main Menu")]
    public GameObject settingsPanel;
    public GameObject mainPanel;

    [Header("Main Scene")]
    public GameObject pausePanel;

    [Header("Error")]
    public GameObject errorPanel;
    public Text errorNum, errorDetails;
    public Button errorButton;
    private int activeError;

    private void Awake()
    {
        GetData();
    }

    // Main Menu 
    public void OnClickPlay()
    {
        SceneManager.LoadScene("EnterUsername");
    }

    public void OnClickTraining()
    {
        return;
    }

    public void OnClickSettings()
    {
        settingsPanel.SetActive(true);
        mainPanel.SetActive(false);
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }


    // Main Scene | Pause Menu
    public void PauseMenu(bool toggle)
    {
        pausePanel.SetActive(toggle);
        Cursor.visible = toggle;
    }

    void GetData()
    {
        // TODO Data request
    }

    void OnError(int code)
    {
        activeError = code;

        switch (code)
        {
            case 2005:
                errorNum.text = "Error <color=red>" + code.ToString() + "</color>";
                errorDetails.text = "Version " + Application.version.ToString() + " is out of date. " +
                                    "Please update the game.";
                errorButton.GetComponentInChildren<Text>().text = "Launcher";
                break;
        }
    }

    public void ErrorButton()
    {
        switch (activeError)
        {
            case 2005:
                // TODO Open Launcher
                break;
        }
    }

}