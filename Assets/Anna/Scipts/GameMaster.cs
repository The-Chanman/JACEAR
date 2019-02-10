using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {

    public AudioClip start;
    public AudioClip zero;
    public AudioClip end;
    public AudioClip you_got;

    private AudioSource audioStart;
    private AudioSource audioZero;
    private AudioSource audioEnd;
    private AudioSource audioYou_got;


    private float timeleft = 10;
    public int score = 0;

	// Use this for initialization
	void Start () {
        //playIntro
        audioStart.Play();

	}
	
	// Update is called once per frame
	void Update () {
        timeleft -= Time.deltaTime;
        //Application.LoadLevel(Random.Range(0, Application.levelCount));
        if (timeleft < 0)
        {
            EndOfGame();
        }
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
    audioStart = AddAudio(start, false, true, 1);
    audioZero = AddAudio(zero, false, true, 1);
    audioEnd = AddAudio(end, false, true, 1);
    audioYou_got = AddAudio(you_got, false, true, 1);
}

void EndOfGame()
    {
        audioEnd.Play();
        audioZero.Play();
        audioYou_got.Play();
        //your score is

    }
}
