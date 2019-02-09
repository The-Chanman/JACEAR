using UnityEngine;
using System.Collections;
 
 public class Orbit : MonoBehaviour {
 
     public float xDelta = 1.5f;  // Amount to move left and right from the start point
     public float xSpeed = 2.0f;
     public float zDelta = 1.5f;  // Amount to move left and right from the start point
     public float zSpeed = 2.0f;  
     private Vector3 startPos;
 
     void Start () {
         startPos = transform.position;
     }
     
     void Update () {
         Vector3 v = startPos;
         v.x += xDelta * Mathf.Cos (Time.time * xSpeed);
         v.z += zDelta * -Mathf.Sin (Time.time * zSpeed);
         transform.position = v;
     }
 }