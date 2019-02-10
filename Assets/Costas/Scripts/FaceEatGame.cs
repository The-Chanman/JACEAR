using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceEatGame : MonoBehaviour {

    public GameObject player;
    private Transform setup;
    private int score;
    private float spawnTimer;
    private int numObjects;
    public GameObject[] food;
    // Use this for initialization
	void Start () {
        spawnTimer = 0;
	}
	
	// Update is called once per frame
	void Update () {

        spawnTimer += Time.deltaTime;

		if (spawnTimer >= 5)
        {
            //pick random food
            int randoFood = Random.Range(0, food.Length);
            Instantiate(food[randoFood], Random.onUnitSphere * 10, Quaternion.identity);
            spawnTimer = 0;
        }
	}


}
