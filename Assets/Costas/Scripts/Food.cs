using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour {
    public GameObject player;
    public GameObject face;
    public float speed;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float step = Time.deltaTime * speed;
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, step);
	}
}
