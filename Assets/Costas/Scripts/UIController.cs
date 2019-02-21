using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class UIController : MonoBehaviour {

    public GameObject score;
    public GameObject highScore;
    TextMeshProUGUI scoreText;
    TextMeshProUGUI highScoreText;

    // Use this for initialization
    void Start () {

        scoreText = score.GetComponent<TextMeshProUGUI>();
        highScoreText = highScore.GetComponent<TextMeshProUGUI>();



    }
	
	// Update is called once per frame
	void Update () {
        scoreText.text = "Score: " + Globals.score.ToString();
        highScoreText.text = "HighScore: " + Globals.highscore.ToString();
    }
}
