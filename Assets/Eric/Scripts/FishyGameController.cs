using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishyGameController : MonoBehaviour {
	public Vector3 goalspawnLocationCenter;
	public Vector3 size;
	public GameObject goal;
	public AudioSource intro;
	public AudioSource success;


	// Use this for initialization
	void Start () {
		SpawnGoal();
	}

	// Update is called once per frame
	void Update () {

	}

	void SpawnGoal() {
		Vector3 pos = goalspawnLocationCenter + new Vector3(Random.Range(-size.x / 2, size.x / 2), Random.Range(-size.y / 2, size.y / 2),  Random.Range(-size.z / 2, size.z / 2));
		Instantiate(goal, pos, Quaternion.identity);
	}

	void OnDrawGizmosSelected(){
		Gizmos.color = new Color(1,0,0,0.5f);
		Gizmos.DrawCube(goalspawnLocationCenter, size);
	}
}
