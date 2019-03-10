using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishyGameController : MonoBehaviour {
	public Vector3 goalspawnLocationCenter;
	public Vector3 size;
	public GameObject goal;
	public AudioSource intro;
    public Transform sceneAnchor;
  public float spawnTimerDelay = 10;
	private float spawnTimer;

	// Use this for initialization
	void Start () {
		intro.Play();
    spawnTimer = 0;
	}

	// Update is called once per frame
	void Update () {
    spawnTimer += Time.deltaTime;
		if (spawnTimer >= spawnTimerDelay)
		{
				//pick random food
				SpawnGoal();
				spawnTimer = 0;
		}
	}

	void SpawnGoal() {
		Vector3 pos = goalspawnLocationCenter + new Vector3(Random.Range(-size.x / 2, size.x / 2), Random.Range(-size.y / 2, size.y / 2),  Random.Range(-size.z / 2, size.z / 2));
		GameObject spawnedGoal = Instantiate(goal, pos, Quaternion.identity, sceneAnchor);
        spawnedGoal.transform.SetParent(null);

    }

	void OnDrawGizmosSelected(){
		Gizmos.color = new Color(1,0,0,0.5f);
		Gizmos.DrawCube(goalspawnLocationCenter, size);
	}
}
