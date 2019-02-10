using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceEatFace : MonoBehaviour {
    public AudioSource nomnom;
    public Transform player;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = player.position;
        transform.rotation = player.rotation;
	}

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "food")
        {
            Destroy(collision.gameObject);
            nomnom.Play();
            Globals.score++;
        }
    }
}
