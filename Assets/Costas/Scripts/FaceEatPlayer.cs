using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceEatPlayer : MonoBehaviour {
    public AudioSource oof;
    
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "food")
        {
            Destroy(collision.gameObject);
            oof.Play();
        }
    }
}
