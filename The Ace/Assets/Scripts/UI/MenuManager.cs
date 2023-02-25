using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Main Menu")]
    public GameObject settingsPanel;
    public GameObject mainPanel;

    [Header("Main Scene")]
    public GameObject pausePanel;

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

}