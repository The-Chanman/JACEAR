using Bose.Wearable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Bose.Wearable
{

    /// <summary>
    /// Automatically rotates a GameObject to match the orientation of the Prototype Glasses.
    /// Provides both "absolute" and "relative" rotation modes.
    /// The <see cref="RotationSensor"/> must be started for the component to have any effect.
    /// </summary>
    public class FreezeDance : MonoBehaviour
    {

        public float min = 9.4f;
        public float max = 10.2f;
        public float curAcc;

        public AudioClip right;
        public AudioClip clipFreeze;
        public AudioClip intro;
        public AudioClip done;
        public AudioClip spin;
        public AudioClip boo;
        public AudioClip slideRight;
        public AudioClip slideLeft;


        private AudioSource audioRight;
        private AudioSource audioFreeze;
        private AudioSource audioIntro;
        private AudioSource audioDone;
        private AudioSource audioFeedback;
        private AudioSource audioInstructions;


        public AudioClip[] danceHarderAudioClips;
        public AudioClip[] freezeAudioClips;


        private AudioSource songPlayer;

        public int finalPoints = 200;

        private float timeleft = 50;
        private float changeVolTime;
        private float updateMoveCheck = 3f;

        private float xRot;
        private float yRot;
        private float zRot;



        //360 spin
        private Vector3 lastFwd;
        public float curXangle = 0;
        private float changeSpinTime;
        public bool inASpin;

        public bool inATask;
        int task = 0; //TODO make this nicer to select.




        public enum RotationReference
        {
            /// <summary>
            /// In absolute mode, pointing the Prototype Glasses north will orient the object's forward vector in
            /// the +Z direction.
            /// </summary>
            Absolute,

            /// <summary>
            /// In relative mode, the object is rotated with regards to a fixed reference orientation. Pointing in the
            /// direction of the reference orientation will orient the object's forward vector in the +Z direction.
            /// </summary>
            Relative
        }

        /// <summary>
        /// The reference mode to use when rotating the object. See <see cref="RotationReference"/> for descriptions
        /// of each rotation mode.
        /// </summary>
        public RotationReference ReferenceMode
        {
            get { return _mode; }
            private set { _mode = value; }
        }

        /// <summary>
        /// Get or set the reference rotation. Returns <code>Quaternion.identity</code> if in "absolute" mode,
        /// and the previously-set reference if in "relative" mode. Setting a reference rotation will automatically
        /// switch to "relative" mode.
        /// </summary>
        public Quaternion ReferenceRotation
        {
            get
            {
                if (_mode == RotationReference.Absolute)
                {
                    return Quaternion.identity;
                }
                else
                {
                    return Quaternion.Inverse(_inverseReference);
                }
            }
        }

        private WearableControl _wearableControl;
        private RotationReference _mode;
        private Quaternion _inverseReference;

        // Use this for initialization
        void Start()
        {
            lastFwd = transform.forward;
            inASpin = false;
            inATask = false;

        }

        private void OnEnable()
        {
            StartRotationSensor();
        }

        private void OnDeviceConnected(Device device)
        {
            StartRotationSensor();
        }

        private void StartRotationSensor()
        {
            if (_wearableControl.ConnectedDevice == null)
            {
                return;
            }

            _wearableControl.RotationSensor.Start();
            _wearableControl.AccelerometerSensor.Start();

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
            // Begin in absolute mode and cache the wearable controller.
            _wearableControl = WearableControl.Instance;
            _wearableControl.DeviceConnected += OnDeviceConnected;

            _mode = RotationReference.Absolute;
            _inverseReference = Quaternion.identity;

            // add the necessary AudioSources:
            audioRight = AddAudio(right, false, true, 1);
            audioFreeze = AddAudio(freezeAudioClips[0], false, true, 1);
            audioIntro = AddAudio(intro, false, true, 1);
            audioFeedback = AddAudio(danceHarderAudioClips[0], false, true, 1);
            audioDone = AddAudio(done, false, true, 1);
            audioInstructions = AddAudio(spin, false, true, 1);


            audioIntro.Play();

            songPlayer = GetComponent<AudioSource>();
            songPlayer.Play();
            songPlayer.volume = 1;
            audioFreeze.volume = 1;
            RandomTime();
            RandomSpinTime();
            finalPoints = 300;
        }

        // Update is called once per frame
        void Update()
        {
            

            if (_wearableControl.ConnectedDevice == null)
            {
                return;
            }

            // Get a frame of sensor data. Since no integration is being performed, we can safely ignore all
            // intermediate frames and just grab the most recent.
            SensorFrame frame = _wearableControl.LastSensorFrame;

            curAcc = frame.acceleration.value.magnitude;

            if (!songPlayer.isPlaying || finalPoints <= 0)
            {
                if (finalPoints < 0)
                {
                    finalPoints = 0;
                }
                Globals.score += finalPoints;
                songPlayer.volume = 0;
                audioDone.Play();
                return;
            }
            timeleft -= Time.deltaTime;
            if (timeleft < changeVolTime)
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
                        if (!audioFreeze.isPlaying)
                        {
                            audioFreeze.clip = freezeAudioClips[Random.Range(0, freezeAudioClips.Length)];
                            audioFreeze.Play(); //play buzz if they move
                            finalPoints -= 15;
                        }
                    }

                    if (timeleft < changeVolTime - 5)
                    {
                        songPlayer.volume = 1;
                        updateMoveCheck = Mathf.FloorToInt(Time.time) + 0.8f; //give react time to dance
                        RandomTime();
                        //if person successfully didn't move, play point audioRight.Play();
                        //else play buzz audioFreeze.Play();

                    }
                }
            }
            else //check that person is actually moving
            {
                //TODO make this nicer
                // TODO make sure event isn't near the freeze time
                if(timeleft < changeSpinTime) //do a spin 
                {
                    
                    if(task == 0 || task == 1) //slide to right or left
                    {
                        if (!inATask)
                        {
                            audioInstructions.Play();
                            inATask = true;
                            return;
                        }
                        if ((task == 0 && frame.acceleration.value.x > 3.0f) 
                            || (task == 1 && frame.acceleration.value.x < -3.0f))//tothe right or left
                        {
                            //you did it!
                            audioRight.Play();
                            finalPoints += 40;
                            RandomSpinTime();
                        }

                        if (timeleft < changeSpinTime - 5) //5 seconds to do a task
                        {
                            audioFeedback.clip = boo;
                            audioFeedback.Play();
                            RandomSpinTime();
                        }
                    }
                    else // spin
                    {
                        if (!inASpin)
                        {
                            curXangle = 0;
                            audioInstructions.clip = spin;
                            audioInstructions.Play();
                            lastFwd = transform.forward;
                            inASpin = true;
                            return;
                        }

                        DoASpin(); //calc angles


                        if (Mathf.Abs(curXangle) >= 120) //congrats you did a spin
                        {
                            //you did it!
                            audioRight.Play();
                            finalPoints += 40;
                            RandomSpinTime();
                        }

                        if (timeleft < changeSpinTime - 5) //5 seconds to do a spin
                        {
                            audioFeedback.clip = boo;
                            audioFeedback.Play();
                            RandomSpinTime();
                        }
                    }


                    

                }
                else if (Time.time >= updateMoveCheck)
                {
                    inASpin = false;
                    inATask = false;
                    updateMoveCheck = Mathf.FloorToInt(Time.time) + 3f;
                    //allow them a sec to react
                    if (frame.acceleration.value.magnitude > min && frame.acceleration.value.magnitude < max) 
                    {
                        if (!audioFeedback.isPlaying)
                        {
                            audioFeedback.clip = danceHarderAudioClips[Random.Range(0, danceHarderAudioClips.Length)];
                            audioFeedback.Play();
                            finalPoints -= 15;
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
            if ((Mathf.Abs(gameObject.transform.rotation.x - xRot) > 0.03) ||
                (Mathf.Abs(gameObject.transform.rotation.y - yRot) > 0.03) ||
                (Mathf.Abs(gameObject.transform.rotation.z - zRot) > 0.03))
            {
                return true;
            }
            return false;

        }

        //TODO
        private void DoASpin()
        {
            Vector3 curFwd = transform.forward;
            float ang = Vector3.Angle(curFwd, lastFwd);
            if (ang > 0.01)
            { 
                if (Vector3.Cross(curFwd, lastFwd).x < 0) ang = -ang;
                curXangle += ang; // accumulate in curAngleX
                lastFwd = curFwd; // and update lastFwd
            }
        }


        private void RandomTime()
        {
            float change = Random.Range(5, 20);
            changeVolTime = timeleft - change;
        }

        private void RandomSpinTime()
        {
            inATask = false;
            float change = Random.Range(10, 40);
            task = Random.Range(0, 3);
            if(task == 0)
            {
                audioInstructions.clip = slideLeft;
            } else if(task == 1)
            {
                audioInstructions.clip = slideRight;
            }
            
            changeSpinTime = timeleft - change;
        }
    }
}