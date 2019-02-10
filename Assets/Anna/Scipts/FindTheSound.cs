using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindTheSound : MonoBehaviour {

    public AudioClip[] randomAudioClips;

	// Use this for initialization
	void Start () {
        
        //randomAudioClips = Resources.LoadAll<AudioClip>("Assets/Anna/Sounds/LocateSounds");
        print(randomAudioClips.Length);

        float radius = 5f;
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI * 2f / 8;
            Vector3 newPos = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            GameObject go = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), newPos, Quaternion.identity);
            AudioSource audioS = go.AddComponent<AudioSource>();
            audioS.clip = randomAudioClips[i];
            audioS.Play();
            audioS.loop = true;
            audioS.spatialBlend = 1;
            audioS.spatialize = true;
            go.AddComponent<ONSPAudioSource>();
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
