﻿using Bose.Wearable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeDance : MonoBehaviour {

    

    public AudioClip right;
    public AudioClip wrong;
    public AudioClip intro;
    public AudioClip dance;
    public AudioClip done;


    private AudioSource audioRight;
    private AudioSource audioWrong;
    private AudioSource audioIntro;
    private AudioSource audioDance;
    private AudioSource audioDone;


    private AudioSource songPlayer;

    public int finalPoints = 200;

    private float timeleft = 50;
    private float changeVolTime;
    private float updateMoveCheck = 1f;

    private float xRot;
    private float yRot;
    private float zRot;


    // Use this for initialization
    void Start () {
        audioIntro.Play();
        songPlayer = GetComponent<AudioSource>();
        songPlayer.volume = 1;
        audioWrong.volume = 0.2f;
        RandomTime();
        
    }

    public AudioSource AddAudio(AudioClip clip, bool loop, bool playAwake, float vol)
    {
        AudioSource newAudio = gameObject.AddComponent<AudioSource>();
        newAudio.clip = clip;
        newAudio.loop = loop;
        newAudio.playOnAwake = playAwake;
        newAudio.volume = vol;
        return newAudio;
    }

    public void Awake()
    {
        // add the necessary AudioSources:
        audioRight = AddAudio(right, false, true, 1);
        audioWrong = AddAudio(wrong, false, true, 1);
        audioIntro = AddAudio(intro, false, true, 1);
        audioDance = AddAudio(dance, false, true, 1);
        audioDone = AddAudio(done, false, true, 1);
    }

    // Update is called once per frame
    void Update () {
        if(!songPlayer.isPlaying || finalPoints <= 0)
        {
            if(finalPoints < 0)
            {
                finalPoints = 0;
            }
            Globals.score += finalPoints;
            audioDone.Play();
        }
        timeleft -= Time.deltaTime;
        if(timeleft < changeVolTime)
        {
            songPlayer.volume = 0;

            if (timeleft > changeVolTime - .8) //allow a sec for person to react
            {
                FreezePosition();
            }
            else
            {
                //check that person isn't moving
                if (CheckMove() == true)
                {
                    FreezePosition(); //reset position to give them another chance
                    audioWrong.Play(); //play buzz if they move
                    finalPoints-= 15;
                }
            
                if (timeleft < changeVolTime - 5)
                {
                    songPlayer.volume = 1;
                    RandomTime();
                    //if person successfully didn't move, play point audioRight.Play();
                    //else play buzz audioWrong.Play();

                }
            }
        }
        else //check that person is actually moving
        {
            if (Time.time >= updateMoveCheck)
            {
                updateMoveCheck = Mathf.FloorToInt(Time.time) + 1f;
                if (!CheckMove())
                {
                    if (!audioDance.isPlaying)
                    {
                        //audioDance.Play();
                        finalPoints-= 15;
                    }
                    
                }
            }
            else
            {
                FreezePosition();
            }
        }
    }

    private void FreezePosition()
    {
        xRot = gameObject.transform.rotation.x;
        yRot = gameObject.transform.rotation.y;
        zRot = gameObject.transform.rotation.z;
    }

    private bool CheckMove()
    {
        if( (Mathf.Abs(gameObject.transform.rotation.x - xRot) > 0.03) ||
            (Mathf.Abs(gameObject.transform.rotation.y - yRot) > 0.03) ||
            (Mathf.Abs(gameObject.transform.rotation.z - zRot) > 0.03))
        {
            return true;
        }
        return false;

    }
        

    private void RandomTime()
    {
        float change = Random.Range(5,20);
        changeVolTime = timeleft - change;
    }
}
