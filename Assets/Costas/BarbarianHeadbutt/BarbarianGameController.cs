﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Bose.Wearable
{


    /// <summary>
    /// Automatically rotates a GameObject to match the orientation of the Prototype Glasses.
    /// Provides both "absolute" and "relative" rotation modes.
    /// The <see cref="RotationSensor"/> must be started for the component to have any effect.
    /// </summary>
    public class BarbarianGameController : MonoBehaviour
    {
        private bool actionTaken = false;
        private bool turnStart;
        private float turnTimer;
        private float turnSpeed = 4f;
        private bool hurt;
        private int direction;
        

        public AudioSource intro;
        public AudioSource left;
        public AudioSource right;
        public AudioSource jump;
        public AudioSource back;
        public AudioSource duck;
        public AudioSource headbutt;
        public AudioSource oof;
        public AudioSource grunt;
        public AudioSource delayedPoint;

        private AudioSource[] audioSources;
        public LevelManager levelManager;

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

        private void Awake()
        {
            // Begin in absolute mode and cache the wearable controller.
            _wearableControl = WearableControl.Instance;
            _wearableControl.DeviceConnected += OnDeviceConnected;

            hurt = false;

            _mode = RotationReference.Absolute;
            _inverseReference = Quaternion.identity;
            // Get audiosources
            audioSources = new AudioSource[] { left, right, jump, back, duck };
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

        private void Update()
        {


            if (_wearableControl.ConnectedDevice == null)
            {
                return;
            }

            // Get a frame of sensor data. Since no integration is being performed, we can safely ignore all
            // intermediate frames and just grab the most recent.
            SensorFrame frame = _wearableControl.LastSensorFrame;

            if (turnStart)
            {
                //Pick audio clip
                direction = Random.Range(0, audioSources.Length);
                //Play direction
                audioSources[direction].Play();
                //Start turn timer
                turnTimer = turnSpeed;
                if (hurt)
                {
                    oof.Play();
                    hurt = false;
                    if (Globals.score > 0)
                    {
                        Globals.score--;
                    }
                }
                actionTaken = false;
            }
            turnStart = false;


            if (turnTimer <= 0.0f)
            {
                //Start next turn
                turnStart = true;
            }

            if (turnTimer > 0.0f && !actionTaken)
            {
                if (frame.acceleration.value.x > 4.0f)
                {
                    //right
                    grunt.Play();
                    if (direction == 1)
                    {
                        delayedPoint.Play();
                        Globals.score++;
                    }
                    else
                    {
                        hurt = true;
                    }
                    actionTaken = true;
                }
                else if (frame.acceleration.value.x < -4.0f)
                {
                    //left
                    grunt.Play();
                    if (direction == 0)
                    {
                        delayedPoint.Play();
                        Globals.score++;
                    }
                    else
                    {
                        hurt = true;
                    }
                    actionTaken = true;
                }

                if (frame.acceleration.value.y > 13.0f)
                {
                    //down
                    grunt.Play();
                    if (direction == 4)
                    {
                        delayedPoint.Play();
                        Globals.score++;
                    }
                    else
                    {
                        hurt = true;
                    }
                    actionTaken = true;
                }
                else if (frame.acceleration.value.y < 5.0f)
                {
                    //up
                    grunt.Play();
                    if (direction == 2)
                    {
                        delayedPoint.Play();
                        Globals.score++;
                    }
                    else
                    {
                        hurt = true;
                    }
                    actionTaken = true;
                }
                if (frame.acceleration.value.z > 4.0f)
                {
                    //forward
                    headbutt.Play();
                    if (direction == 5)
                    {
                        delayedPoint.Play();
                        Globals.score++;
                    }
                    else
                    {
                        hurt = true;
                    }
                    actionTaken = true;
                }
                else if (frame.acceleration.value.z < -4.0f)
                {
                    //back
                    grunt.Play();
                    if (direction == 3)
                    {
                        delayedPoint.Play();
                        Globals.score++;
                    }
                    else
                    {
                        hurt = true;
                    }
                    actionTaken = true;
                }
            }

            float subTime = Time.deltaTime;

            turnTimer -= subTime;
            

        }

        /// <summary>
        /// Set rotation to always use the rotation from the latest <see cref="SensorFrame"/> when matching the
        /// rotation.
        /// </summary>
        public void SetAbsoluteReference()
        {
            ReferenceMode = RotationReference.Absolute;
        }

        /// <summary>
        /// Set the reference to the device's current orientation.
        /// </summary>
        public void SetRelativeReference()
        {
            ReferenceMode = RotationReference.Relative;

            if (_wearableControl.ConnectedDevice != null)
            {
                _inverseReference = Quaternion.Inverse(_wearableControl.LastSensorFrame.rotation);
            }
        }

        /// <summary>
        /// Set the <see cref="Quaternion"/> <paramref name="rotation"/> as a reference when matching the rotation.
        /// </summary>
        /// <param name="rotation"></param>
        public void SetRelativeReference(Quaternion rotation)
        {
            ReferenceMode = RotationReference.Relative;
            _inverseReference = Quaternion.Inverse(rotation);
        }
    }
}
