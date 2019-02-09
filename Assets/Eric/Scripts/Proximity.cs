using System;
using UnityEngine;
using UnityEngine.Events;
 
public class Proximity : MonoBehaviour {
 
    public GameObject player;  
    public AudioSource audioPlayer;
    public float threshold = 10f;
    public bool PlayedAudioOnEnter = false;
    private Vector3 currentPos;
    private Vector3 playerDistance;
   

    void Start () {
        currentPos = transform.position;
    }
     
    void Update () {
        currentPos = transform.position;
        Vector3 v = currentPos;
        playerDistance = player.transform.position - v;
        float magnitude = playerDistance.magnitude;
        Debug.Log(magnitude);
        if(magnitude <= threshold){
            if(!PlayedAudioOnEnter){
                audioPlayer.Play();
                PlayedAudioOnEnter = true;
            }
        }
        if(magnitude > threshold){
            PlayedAudioOnEnter = false;
        }
    }
}