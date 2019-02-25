using Bose.Wearable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bose.Wearable
{
  public class FishyPlayer : MonoBehaviour {
    public AudioSource success;
    public GameObject player;
    public bool swimSwing;
    public float movementSpeed = 15.0f;
    public float clockwise = 15.0f;
    public float counterClockwise = -15.0f;
    public AudioSource forwardSwingFeedback;
    public AudioSource backwardSwingFeedback;

    // Bose stuff that isn't explained at all very well
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
  	void Start () {

  	}

    // Bose awake stuff
    public void Awake()
    {
      // Begin in absolute mode and cache the wearable controller.
      _wearableControl = WearableControl.Instance;
      _wearableControl.DeviceConnected += OnDeviceConnected;

      _mode = RotationReference.Absolute;
      _inverseReference = Quaternion.identity;
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
  	// Update is called once per frame
  	void Update () {

      if (_wearableControl.ConnectedDevice == null)
      {
          return;
      }
      // Get a frame of sensor data. Since no integration is being performed, we can safely ignore all
      // intermediate frames and just grab the most recent.
      SensorFrame frame = _wearableControl.LastSensorFrame;
      if (frame.acceleration.value.z < -3.5f && swimSwing == false)
      {
          swimSwing = true;
          player.transform.position += player.transform.forward * Time.deltaTime * movementSpeed;
          forwardSwingFeedback.Play();
      }
      if (frame.acceleration.value.z > 3.5f && swimSwing == true)
      {
          swimSwing = false;
          player.transform.position += player.transform.forward * Time.deltaTime * movementSpeed;
          backwardSwingFeedback.Play();
      }

      if (_mode == RotationReference.Absolute)
      {
        // In absolute mode, match the rotation exactly.
        player.transform.rotation = frame.rotation;
      }
      else if (_mode == RotationReference.Relative)
      {
        // In relative mode, left-apply the inverse of the reference rotation to compute the relative change
        player.transform.rotation = _inverseReference * frame.rotation;
      }

      if(Input.GetKey(KeyCode.W)) {
           player.transform.position += player.transform.forward * Time.deltaTime * movementSpeed;
       }
       else if(Input.GetKey(KeyCode.S)) {
           player.transform.position -= player.transform.forward * Time.deltaTime * movementSpeed;
       }
       else if(Input.GetKey(KeyCode.A)) {
           player.transform.position -= player.transform.right * Time.deltaTime * movementSpeed;
       }
       else if(Input.GetKey(KeyCode.D)) {
           player.transform.position += player.transform.right * Time.deltaTime * movementSpeed;
       }

       if(Input.GetKey(KeyCode.E)) {
           transform.Rotate(0, Time.deltaTime * clockwise, 0);
       }
       else if(Input.GetKey(KeyCode.Q)) {
           transform.Rotate(0, Time.deltaTime * counterClockwise, 0);
       }
  	}

  	void OnCollisionEnter(Collision collision){
  		if (collision.gameObject.tag == "FishyGoal")
  		{
  				Destroy(collision.gameObject);
  				success.Play();
  				Globals.score++;
  		}
  	}
  }
}
