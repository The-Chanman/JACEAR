using System.Collections.Generic;
using UnityEngine;

namespace Bose.Wearable.Examples
{
	/// <summary>
	/// Main logic for the Basic Demo. Configures the <see cref="RotationMatcher"/> based on UI input, and
	/// fades out the reference glasses based on closeness.
	/// Handles starting and stopping sensors on connect/reconnect/disconnect.
	/// </summary>
	public class BasicDemoController : MonoBehaviour
	{
		private WearableControl _wearableControl;
		private RotationMatcher _matcher;

		private void Awake()
		{
			_matcher = GetComponent<RotationMatcher>();

			// Grab an instance of the WearableControl singleton. This is the primary access point to the wearable SDK.
			_wearableControl = WearableControl.Instance;

			// If a device is connected, immediately start the rotation sensor
			// This ensures that we will receive data from the sensor during play.
			StartSensors();
		}

		private void OnDestroy()
		{
			// Ensure that the controller is no longer subscribing to connections after it is destroyed.
			if (WearableControl.Instance != null)
			{
				WearableControl.Instance.DeviceConnected -= OnDeviceConnected;
			}
		}

		private void OnEnable()
		{
			// Subscribe to DeviceConnected to handle reconnects that happen during play.
			_wearableControl.DeviceConnected += OnDeviceConnected;
		}

		private void OnDisable()
		{
			_wearableControl.DeviceConnected -= OnDeviceConnected;
		}

		private void OnDeviceConnected(Device obj)
		{
			// If a device is reconnected during play, ensures the rotation sensor is still running.
			StartSensors();
		}

		/// <summary>
		/// Sets rotation to relative mode using the current orientation.
		/// </summary>
		public void SetRelativeReference()
		{
			_matcher.SetRelativeReference(_wearableControl.LastSensorFrame.rotation);
		}

		/// <summary>
		/// Sets rotation to absolute mode.
		/// </summary>
		public void SetAbsoluteReference()
		{
			_matcher.SetAbsoluteReference();
		}

		/// <summary>
		/// Configures the update interval and sets all needed sensors
		/// </summary>
		private void StartSensors()
		{
			if (_wearableControl.ConnectedDevice != null)
			{
				_wearableControl.SetSensorUpdateInterval(SensorUpdateInterval.FortyMs);
				_wearableControl.RotationSensor.Start();
			}
		}
	}
}
