using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Bose.Wearable;

public class LevelManager : MonoBehaviour {

    public float timeTillNextLevel = 60.0f;
    private float curTime;
    public int startingLevel = 1;
    public int currentIndex;
    private int sceneToUnLoad;
    public WearableConnectUIPanel w;
    public bool gameStart = false;
    public GameObject newGameButton;
    public GameObject restartButton;

    // Use this for initialization

    void Start() {
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
        Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ We are going to load this scene: " + startingLevel);
        LoadNextScene(startingLevel);
        Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ New current index is: " + currentIndex);
        curTime = timeTillNextLevel;
    }
    // Update is called once per frame
    /*   void Update() {
           if (curTime > 0)
           {
               curTime -= Time.deltaTime;
               if (curTime < 0)
               {
                   int curLevel = currentIndex + 1;
                   if (!(curLevel >= SceneManager.sceneCountInBuildSettings))
                   {
                       LoadNextScene(curLevel);
                       UnloadScene(currentIndex);
                       curTime = timeTillNextLevel;
                       currentIndex++;
                   }
                   else
                   {
                       currentIndex = startingLevel;
                       LoadNextScene(curLevel);
                       UnloadScene(currentIndex);
                       curTime = timeTillNextLevel;
                       currentIndex++;
                   }
                   Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@" + curLevel);
               }
           }
       } */

    void Update()
    {
        if (gameStart)
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
                    currentIndex = startingLevel;
                    //Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ We are going to load this scene WE INSIDE THE IF: " + currentIndex);
                }
                //Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ We are going to unload this scene: " + sceneToUnLoad);
                UnloadScene(sceneToUnLoad);
                //Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ We are going to load this scene: " + currentIndex);
                LoadNextScene(currentIndex);

                curTime = timeTillNextLevel;

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

        newGameButton.SetActive(false);
        
        restartButton.SetActive(true);

    }

    public void restartGame()
    {
        //Resets Score
        Globals.score = 0;
        newGameButton.SetActive(true);
        restartButton.SetActive(false);

    }

    public void skipGame()
    {
        curTime = 0;
    }

}
