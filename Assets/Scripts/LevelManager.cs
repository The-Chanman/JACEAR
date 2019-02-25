using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Bose.Wearable;

public class LevelManager : MonoBehaviour {

    public float timeTillNextLevel = 60.0f;
    public float curTime;
    public int startingLevel = 1;
    public int currentIndex;
    private int sceneToUnLoad;
    public WearableConnectUIPanel w;
    public bool gameStart;
    public bool gameEnd;
    public GameObject newGameButton;
    public GameObject endButton;
    public GameObject Title;
    public GameObject GameOver;
    // Use this for initialization

    void Start() {
        gameStart = false;

        w = FindObjectOfType<WearableConnectUIPanel>();
        currentIndex = SceneManager.GetActiveScene().buildIndex;
        Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ This is the current index " + currentIndex);
        w.DeviceConnectSuccess += OnConnect;
    }
    private void OnDestroy()
    {
        w.DeviceConnectSuccess -= OnConnect;

    }

    public void OnConnect()
    {
        currentIndex = startingLevel;
        // Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ We are going to load this scene: " + startingLevel);
        //LoadNextScene(startingLevel);
        // Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ New current index is: " + currentIndex);
        //curTime = timeTillNextLevel;
    }
    // Update is called once per frame
    void Update()
    {
        if (gameStart && !gameEnd)
        {
            if (curTime > 0)
            {
                curTime -= Time.deltaTime;
            }
            else if (curTime < 0)
            {

                //Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ Time has reached less than 0");
                sceneToUnLoad = currentIndex;
                currentIndex++;
                if (currentIndex >= SceneManager.sceneCountInBuildSettings)
                {
                    //GAME Restarts FROM THE FIRST GAME
                    currentIndex = startingLevel;

                    //GAME ENDS AND TALLYS YOUR SCORE
                    gameEnd = true;
                    newGameButton.SetActive(true);
       		 		endButton.SetActive(false);
                    Title.SetActive(false);
                    GameOver.SetActive(true);


                    //Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ We are going to load this scene WE INSIDE THE IF: " + currentIndex);
                }
                //Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ We are going to unload this scene: " + sceneToUnLoad);
                UnloadScene(sceneToUnLoad);
                //Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ We are going to load this scene: " + currentIndex);
                if(!gameEnd){
					LoadNextScene(currentIndex);
	                curTime = timeTillNextLevel;
                }

            }
        }

    }

    public void LoadNextScene(int scene)
    {
        SceneManager.LoadScene(currentIndex, LoadSceneMode.Additive);
    }

    public void LoadPreviousScene()
    {
        currentIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentIndex);
    }

    public void UnloadScene(int scene)
    {
        SceneManager.UnloadScene(scene);
    }

    public void startGame()
    {

        gameStart = true;
		gameEnd = false;
        LoadNextScene(startingLevel);
        curTime = timeTillNextLevel;
        newGameButton.SetActive(false);
        endButton.SetActive(true);
        Title.SetActive(true);
        GameOver.SetActive(false);

    }

    public void endGame()
    {
        //Resets Score
        Globals.score = 0;
        sceneToUnLoad = currentIndex;
        UnloadScene(sceneToUnLoad);
        currentIndex = startingLevel;
        newGameButton.SetActive(true);
        endButton.SetActive(false);
        Title.SetActive(false);
        GameOver.SetActive(true);

    }

    public void skipGame()
    {
        curTime = -1f;
    }

}
