using System;
using UnityEngine;
using UnityEngine.Events;
 
public class Proximity : MonoBehaviour {
 
    public GameObject player;  
    public bool PlayedAudioOnEnter = false;
    private Vector3 currentPos;
    private Vector3 playerDistance;
    public AudioSource audioPlayer;

    void Start () {
        currentPos = transform.position;
    }
     
    void Update () {
        currentPos = transform.position;
        Vector3 v = currentPos;
        playerDistance = player.transform.position - v;
        float magnitude = playerDistance.magnitude;
        Debug.Log(magnitude);
        if(magnitude <= 10f){
            if(!PlayedAudioOnEnter){
                audioPlayer.Play();
                PlayedAudioOnEnter = true;
            }
        }
        if(magnitude > 10f){
            PlayedAudioOnEnter = false;
        }
    }
}