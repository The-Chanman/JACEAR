using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishyPlayer : MonoBehaviour {
  public AudioSource success;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}

	void OnCollisionEnter(Collision collision){
		if (collision.gameObject.tag == "FishyGoal")
		{
				Destroy(collision.gameObject);
				success.Play();
				Globals.score++;
		}
	}
}
