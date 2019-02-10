using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Bose.Wearable;
public class LevelManager : MonoBehaviour {

    public float timeTillNextLevel = 100000.0f;

    private int curScene = 0;
    public int currentIndex;
    public WearableConnectUIPanel w;

	// Use this for initialization
	void Start () {
        w = FindObjectOfType<WearableConnectUIPanel>();

        w.DeviceConnectSuccess += OnConnect;
	}
    private void OnDestroy()
    {
        w.DeviceConnectSuccess -= OnConnect;
    }

    public void OnConnect()
    {
        LoadNextScene();
    }

    // Update is called once per frame
    void Update () {
        if (timeTillNextLevel > 0)
        {
            timeTillNextLevel -= Time.deltaTime;
            if (timeTillNextLevel < 0)
            {
                LoadNextScene();
            }
        }
    }
    
    public void LoadNextScene()
    {

        currentIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentIndex + 1, LoadSceneMode.Additive);

    }

    public void LoadPreviousScene()
    {
        currentIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentIndex - 1);
    }

    public void UnloadScene(int scene)
    {

        SceneManager.UnloadScene(scene);

        
    }
}
