using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    private int currentScoreTeam1 = 0;
    private int currentScoreTeam2 = 0;
    [Header("ScoreUI")]
    public Text scoreTeam1;
    public Text scoreTeam2;

    #region SINGLETON
    //Create a reference to this specific script

    public static ScoreUI instance;

    private void Awake()
    {
        instance = this;
    }

    #endregion

    void Update()
    {
        SetScore();
    }

    public void SetScore()
    {
        currentScoreTeam1 = Game.instance.scoreTeam1.Value;
        currentScoreTeam2 = Game.instance.scoreTeam2.Value;
        scoreTeam1.text = currentScoreTeam1.ToString();
        scoreTeam2.text = currentScoreTeam2.ToString();
    }
}
