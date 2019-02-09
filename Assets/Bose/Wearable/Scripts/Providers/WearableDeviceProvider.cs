using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Bose.Wearable
{
	[Serializable]
	public class WearableDeviceProvider : WearableProviderBase
	{
		#region Provider Unique
		/// <summary>
		/// Represents a session with an Wearable Device.
		/// </summary>
		private enum SessionStatus
		{
			Closed,
			Opening,
			Open
		}

		/// <summary>
		/// The RSSI threshold below which devices will be filtered out.
		/// </summary>
		public int RSSIFilterThreshold
		{
			get
			{
				return _RSSIFilterThreshold == 0
					? WearableConstants.DefaultRSSIThreshold
					: _RSSIFilterThreshold;
			}
		}

		/// <summary>
		/// Sets the Received Signal Strength Indication (RSSI) filter level; devices underneath the rssiThreshold filter
		/// threshold will not be made available to connect to. A valid value for <paramref name="rssiThreshold"/> is
		/// set between -70 and -30; anything outside of that range will be clamped to the nearest allowed value.
		/// </summary>
		/// <param name="rssiThreshold"></param>
		public void SetRssiFilter(int rssiThreshold)
		{
			_RSSIFilterThreshold = Mathf.Clamp(rssiThreshold, WearableConstants.MinimumRSSIValue, WearableConstants.MaximumRSSIValue);
		}

		/// <summary>
		/// Indicates whether the SDK has been initialized to simulate available and connected devices.
		/// </summary>
		public bool SimulateHardwareDevices
		{
			get { return _simulateHardwareDevices; }
		}

		[Tooltip(WearableConstants.SimulateHardwareDeviceTooltip)]
		[SerializeField]
		private bool _simulateHardwareDevices;

		#endregion

		#region Provider API

		internal override void SearchForDevices(Action<Device[]> onDevicesUpdated)
		{
			StopSearchingForDevices();

			if (onDevicesUpdated == null)
			{
				return;
			}

			#if UNITY_IOS && !UNITY_EDITOR
			WearableStartDeviceSearch(RSSIFilterThreshold);
			_deviceSearchCallback = onDevicesUpdated;
			_performDeviceSearch = true;
			_nextDeviceSearchTime = Time.unscaledTime + WearableConstants.DeviceSearchUpdateIntervalInSeconds;
			#else
			onDevicesUpdated(WearableConstants.EmptyDeviceList);
			#endif
		}

		internal override void StopSearchingForDevices()
		{
			if (_performDeviceSearch)
			{
				_performDeviceSearch = false;
				_deviceSearchCallback = null;
				_nextDeviceSearchTime = float.PositiveInfinity;

				#if UNITY_IOS && !UNITY_EDITOR
				WearableStopDeviceSearch();
				#endif
			}
		}

		internal override void ConnectToDevice(Device device, Action onSuccess, Action onFailure)
		{
			DisconnectFromDevice();

			_performDeviceConnection = true;
			_deviceConnectSuccessCallback = onSuccess;
			_deviceConnectFailureCallback = onFailure;
			_deviceToConnect = device;
			_nextDeviceConnectTime = Time.unscaledTime + WearableConstants.DeviceConnectUpdateIntervalInSeconds;

			#if UNITY_IOS && !UNITY_EDITOR
			WearableOpenSession(_deviceToConnect.uid);
			#endif

			OnDeviceConnecting(_deviceToConnect);
		}

		internal override void DisconnectFromDevice()
		{
			StopDeviceConnection();
			StopDeviceMonitor();

			for (var i = 0; i < WearableConstants.GestureIds.Length; i++)
			{
				if (WearableConstants.GestureIds[i] == GestureId.None)
				{
					continue;
				}

				_gestureStatus[WearableConstants.GestureIds[i]] = false;
			}

			if (_connectedDevice == null)
			{
				return;
			}

			OnDeviceDisconnected(_connectedDevice.Value);

			_connectedDevice = null;

			#if UNITY_IOS && !UNITY_EDITOR
			WearableCloseSession();
			#endif
		}

		internal override SensorUpdateInterval GetSensorUpdateInterval()
		{
			return _sensorUpdateInterval;
		}

		internal override void SetSensorUpdateInterval(SensorUpdateInterval updateInterval)
		{
			if (_connectedDevice == null)
			{
				Debug.LogWarning(WearableConstants.SetUpdateRateWithoutDeviceWarning);
				return;
			}

			_sensorUpdateInterval = updateInterval;

			// n.b. currently, the only way to set the global update interval is along with a call to EnableSensor.
			// Until a method is added for this, a suitable workaround is to call EnableSensor on a sensor that is
			// already enabled. If no sensors are enabled, the cached value will of _sensorUpdateInterval will be
			// used the next time a sensor is enabled.

			#if UNITY_IOS && !UNITY_EDITOR
			var enumerator = _sensorStatus.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<SensorId, bool> element = enumerator.Current;
					if (element.Value)
					{
						WearableEnableSensor((int) element.Key, (int) _sensorUpdateInterval);

						// Only one call is needed since the interval is global
						break;
					}
				}
			}
			finally
			{
				enumerator.Dispose();
			}
			#endif
		}

		internal override void StartSensor(SensorId sensorId)
		{
			if (_connectedDevice == null)
			{
				_sensorStatus[sensorId] = false;
				Debug.LogWarning(WearableConstants.StartSensorWithoutDeviceWarning);
				return;
			}

			if (_sensorStatus[sensorId])
			{
				return;
			}

			#if UNITY_IOS && !UNITY_EDITOR
			WearableEnableSensor((int)sensorId, (int)_sensorUpdateInterval);
			switch (sensorId)
			{
				case SensorId.Accelerometer:
					WearableListenForAccelerometerData(true);
					break;
				case SensorId.Gyroscope:
					WearableListenForGyroscopeData(true);
					break;
				case SensorId.Rotation:
					WearableListenForRotationData(true, _rotationMode);
					break;
				case SensorId.GameRotation:
					WearableListenForRotationData(true, _rotationMode);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			#endif

			_sensorStatus[sensorId] = true;
		}

		internal override void StopSensor(SensorId sensorId)
		{
			if (!_sensorStatus[sensorId])
			{
				return;
			}

			#if UNITY_IOS && !UNITY_EDITOR
			WearableDisableSensor((int)sensorId);
			switch (sensorId)
			{
				case SensorId.Accelerometer:
					WearableListenForAccelerometerData(false);
					break;
				case SensorId.Gyroscope:
					WearableListenForGyroscopeData(false);
					break;
				case SensorId.Rotation:
					WearableListenForRotationData(false, _rotationMode);
					break;
				case SensorId.GameRotation:
					WearableListenForRotationData(false, _rotationMode);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			#endif

			_sensorStatus[sensorId] = false;
		}

		internal override bool GetSensorActive(SensorId sensorId)
		{
			return _sensorStatus[sensorId];
		}

		internal override void EnableGesture(GestureId gestureId)
		{
			if (_connectedDevice == null)
			{
				_gestureStatus[gestureId] = false;
				Debug.LogWarning(WearableConstants.EnableGestureWithoutDeviceWarning);
				return;
			}

			if (_gestureStatus[gestureId])
			{
				return;
			}

			#if UNITY_IOS && !UNITY_EDITOR
			WearableEnableGesture((int)gestureId);
			WearableListenForGestureData(true);
			#elif UNITY_ANDROID && !UNITY_EDITOR
			// @TODO: The Android API appears to still be evolving here.  If they keep adding separate
			// calls for each gesture we should turn this into a switch statement.
			if (gestureId == GestureId.DoubleTap)
			{
				Plugin.EnableDoubleTap();
			}
			else
			{
				Debug.LogFormat(WearableConstants.EnableGestureNotSupportedWarning, Enum.GetName(typeof(GestureId), gestureId));
			}
			#endif

			_gestureStatus[gestureId] = true;
		}

		internal override void DisableGesture(GestureId gestureId)
		{
			if (!_gestureStatus[gestureId])
			{
				return;
			}

			_gestureStatus[gestureId] = false;

			#if UNITY_IOS && !UNITY_EDITOR
			WearableEnableGesture((int)gestureId);

			// Are any gestures still active?  If not, stop listening.
			bool anyActive = false;
			GestureId[] gestureValues = WearableConstants.GestureIds;
			for (int i = 0; i < gestureValues.Length; ++i)
			{
				if (gestureValues[i] != GestureId.None)
				{
					anyActive |= _gestureStatus[gestureValues[i]];
				}
			}
			if (anyActive == false)
			{
				WearableListenForGestureData(false);
			}
			#elif UNITY_ANDROID && !UNITY_EDITOR
			// @TODO: The Android API appears to still be evolving here.  If they keep adding separate
			// calls for each gesture we should turn this into a switch statement.
			if (gestureId == GestureId.DoubleTap)
			{
				Plugin.DisableDoubleTap();
			}
			else
			{
				Debug.LogFormat(WearableConstants.DisableGestureNotSupportedWarning, Enum.GetName(typeof(GestureId), gestureId));
			}
			#endif
		}

		internal override bool GetGestureEnabled(GestureId gestureId)
		{
			return (_connectedDevice != null) && _gestureStatus[gestureId];
		}

		internal override void OnInitializeProvider()
		{
			if (_initialized)
			{
				return;
			}

			#if UNITY_IOS && !UNITY_EDITOR
			WearableInitialize(_simulateHardwareDevices);
			#else
			Debug.LogError(WearableConstants.UnsupportedPlatformError);
			#endif

			base.OnInitializeProvider();

			StopDeviceConnection();
			StopDeviceMonitor();
			StopSearchingForDevices();
		}

		internal override void OnDestroyProvider()
		{
			base.OnDestroyProvider();

			DisconnectFromDevice();

			StopDeviceConnection();
			StopDeviceMonitor();
			StopSearchingForDevices();
		}

		/// <summary>
		/// When enabled, resume monitoring the device session if necessary.
		/// </summary>
		internal override void OnEnableProvider()
		{
			if (_enabled)
			{
				return;
			}

			base.OnEnableProvider();

			if (_connectedDevice != null)
			{
				StartDeviceMonitor();
			}
		}

		/// <summary>
		/// When disabled, stop actively searching for, connecting to, and monitoring devices.
		/// </summary>
		internal override void OnDisableProvider()
		{
			if (!_enabled)
			{
				return;
			}

			base.OnDisableProvider();

			StopSearchingForDevices();
			StopDeviceMonitor();
			StopDeviceConnection();
		}

		internal override void OnUpdate()
		{
			// Request the latest updates for this frame
			if (_connectedDevice != null)
			{
				GetLatestSensorUpdates();
			}

			// Check if it's time to query discovered devices
			if (_performDeviceSearch && Time.unscaledTime >= _nextDeviceSearchTime)
			{
				_nextDeviceSearchTime += WearableConstants.DeviceSearchUpdateIntervalInSeconds;
				Device[] devices = GetDiscoveredDevices();
				if (_deviceSearchCallback != null)
				{
					_deviceSearchCallback.Invoke(devices);
				}
			}

			// Check if it's time to query the connection routine
			if (_performDeviceConnection && Time.unscaledTime >= _nextDeviceConnectTime)
			{
				_nextDeviceConnectTime += WearableConstants.DeviceConnectUpdateIntervalInSeconds;
				PerformDeviceConnection();
			}

			// Check if it's time to query the device monitor
			if (_pollDeviceMonitor && Time.unscaledTime >= _nextDeviceMonitorTime)
			{
				// NB: The monitor uses the same time interval
				_nextDeviceMonitorTime += WearableConstants.DeviceConnectUpdateIntervalInSeconds;
				MonitorDeviceSession();
			}
		}

		#endregion

		#region Private

		// Sensor status
		private readonly Dictionary<SensorId, bool> _sensorStatus;
		private SensorUpdateInterval _sensorUpdateInterval;

		// Gestures
		private readonly Dictionary<GestureId, bool> _gestureStatus;

		// Device search
		private bool _performDeviceSearch;
		private Action<Device[]> _deviceSearchCallback;
		private float _nextDeviceSearchTime;

		// Device connection
		private bool _performDeviceConnection;
		private Device _deviceToConnect;
		#pragma warning disable 0414
		private Action _deviceConnectSuccessCallback;
		private Action _deviceConnectFailureCallback;
		#pragma warning restore 0414
		private float _nextDeviceConnectTime;

		// Device monitoring
		private bool _pollDeviceMonitor;
		private float _nextDeviceMonitorTime;

		private int _RSSIFilterThreshold;

		internal WearableDeviceProvider()
		{
			_rotationMode = RotationMode.SixDof;

			_sensorStatus = new Dictionary<SensorId, bool>();
			_sensorUpdateInterval = WearableConstants.DefaultUpdateInterval;

			_sensorStatus.Add(SensorId.Accelerometer, false);
			_sensorStatus.Add(SensorId.Gyroscope, false);
			_sensorStatus.Add(SensorId.Rotation, false);
			_sensorStatus.Add(SensorId.GameRotation, false);

			// All gestures start disabled.
			_gestureStatus = new Dictionary<GestureId, bool>();
			for (var i = 0; i < WearableConstants.GestureIds.Length; i++)
			{
				if (WearableConstants.GestureIds[i] == GestureId.None)
				{
					continue;
				}

				_gestureStatus.Add(WearableConstants.GestureIds[i], false);
			}
		}

		/// <summary>
		/// Used internally by WearableControl to get the latest buffer of SensorFrame updates from
		/// the Wearable Device; the newest frame in that batch is set as the CurrentSensorFrame.
		/// </summary>
		private void GetLatestSensorUpdates()
		{
			_currentSensorFrames.Clear();

			#if UNITY_IOS && !UNITY_EDITOR
			unsafe
			{
				BridgeSensorFrame* frames = null;
				int count = 0;
				WearableGetSensorFrames(&frames, &count);
				if (count > 0)
				{
					for (int i = 0; i < count; i++)
					{
						var frame = frames + i;
						_currentSensorFrames.Add(new SensorFrame {
							timestamp = WearableConstants.Sensor2UnityTime * frame->timestamp,
							deltaTime = WearableConstants.Sensor2UnityTime * frame->deltaTime,
							acceleration = frame->acceleration,
							angularVelocity = frame->angularVelocity,
							rotation = frame->rotation,
							gameRotation = frame->gameRotation,
							gestureId = frame->gestureId
						});
					}

					_lastSensorFrame = _currentSensorFrames[_currentSensorFrames.Count - 1];

					OnSensorsUpdated(_lastSensorFrame);
				}
			}
			#endif
		}


		/// <summary>
		/// Used internally to get the latest list of discovered devices from
		/// the native SDK.
		/// </summary>
		private Device[] GetDiscoveredDevices()
		{
			Device[] devices = WearableConstants.EmptyDeviceList;

			#if UNITY_IOS && !UNITY_EDITOR
			unsafe
			{
				BridgeDevice* nativeDevices = null;
				int count = 0;
				WearableGetDiscoveredDevices(&nativeDevices, &count);
				if (count > 0)
				{
					devices = new Device[count];
					for (int i = 0; i < count; i++)
					{
						devices[i] = (Device)Marshal.PtrToStructure(new IntPtr(nativeDevices + i), typeof(Device));
					}
				}
			}
			#endif

			return devices;
		}

		/// <summary>
		/// Attempts to create a session to a specified device and then checks for the session status perpetually until
		/// a SessionStatus of either Open or Closed is returned, equating to either successful or failed.
		/// </summary>
		private void PerformDeviceConnection()
		{
			#if UNITY_IOS && !UNITY_EDITOR
			string errorMessage = string.Empty;
			SessionStatus sessionStatus = (SessionStatus)WearableGetSessionStatus(ref errorMessage);
			switch (sessionStatus)
			{
				// Receiving a session status of Closed while attempting to open a session indicates an error occured.
				case SessionStatus.Closed:
					if (string.IsNullOrEmpty(errorMessage))
					{
						Debug.LogWarning(WearableConstants.DeviceConnectionFailed);
					}
					else
					{
						Debug.LogWarningFormat(WearableConstants.DeviceConnectionFailedWithMessage, errorMessage);
					}

					if (_deviceConnectFailureCallback != null)
					{
						_deviceConnectFailureCallback.Invoke();
					}

					StopDeviceConnection();

					break;

				case SessionStatus.Opening:
					// Device is still connecting, just wait
					break;

				case SessionStatus.Open:
					Debug.Log(WearableConstants.DeviceConnectionOpened);

					// ProductId and VariantId are only accessible after a connection has been opened. Update the values for the _connectDevice.
					_deviceToConnect.productId = (ProductId)WearableGetDeviceProductID();
					_deviceToConnect.variantId = (byte)WearableGetDeviceVariantID();

					// Make sure productId and variantId values are defined.
					if (!Enum.IsDefined(typeof(ProductId), _deviceToConnect.productId))
					{
						_deviceToConnect.productId = ProductId.Undefined;
					}

					_connectedDevice = _deviceToConnect;

					if (_deviceConnectSuccessCallback != null)
					{
						_deviceConnectSuccessCallback.Invoke();
					}

					OnDeviceConnected(_deviceToConnect);

					StartDeviceMonitor();

					StopDeviceConnection();

					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
			#endif
		}

		/// <summary>
		/// Enables the device monitor
		/// </summary>
		private void StartDeviceMonitor()
		{
			_pollDeviceMonitor = true;

			// NB The device monitor runs on the same time interval as the connection routine
			_nextDeviceMonitorTime = Time.unscaledTime + WearableConstants.DeviceConnectUpdateIntervalInSeconds;
		}

		/// <summary>
		/// Halts the device monitor
		/// </summary>
		private void StopDeviceMonitor()
		{
			_pollDeviceMonitor = false;
			_nextDeviceMonitorTime = float.PositiveInfinity;
		}

		/// <summary>
		/// Monitors the current device SessionStatus until a non-Open session status is returned. Once this has occured,
		/// the device has become disconnected and should render all state as such.
		/// </summary>
		private void MonitorDeviceSession()
		{
		#if UNITY_IOS && !UNITY_EDITOR
			string errorMessage = string.Empty;
			SessionStatus sessionStatus = (SessionStatus)WearableGetSessionStatus(ref errorMessage);
			if (sessionStatus != SessionStatus.Open)
			{
				if (string.IsNullOrEmpty(errorMessage))
				{
					Debug.Log(WearableConstants.DeviceConnectionMonitorWarning);
				}
				else
				{
					Debug.LogFormat(WearableConstants.DeviceConnectionMonitorWarningWithMessage, errorMessage);
				}

				if (_connectedDevice != null)
				{
					OnDeviceDisconnected(_connectedDevice.Value);
				}

				_sensorStatus[SensorId.Accelerometer] = false;
				_sensorStatus[SensorId.Gyroscope] = false;
				_sensorStatus[SensorId.Rotation] = false;
				_sensorStatus[SensorId.GameRotation] = false;

				StopDeviceMonitor();

				_connectedDevice = null;
			}
		#endif
		}

		/// <summary>
		/// Halts the device connection routine
		/// </summary>
		private void StopDeviceConnection()
		{
			_performDeviceConnection = false;
			_deviceConnectFailureCallback = null;
			_deviceConnectSuccessCallback = null;
			_nextDeviceConnectTime = float.PositiveInfinity;
		}

		#endregion

		#region Internal iOS Native Methods
		#if UNITY_IOS && !UNITY_EDITOR
		/// <summary>
		/// This struct matches the plugin bridge definition and is only used as a temporary convert from the native
		/// code struct to the public struct.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		private unsafe struct BridgeDevice
		{
			public char* uid;
			public char* name;
			public bool isConnected;
			public int rssi;
			public int productId;
			public int variantId;
		}

		/// <summary>
		/// This struct matches the plugin bridge definition and is only used as a temporary convert from the native
		/// code struct to the public struct.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		private struct BridgeSensorFrame
		{
			public int timestamp;
			public int deltaTime;
			public SensorVector3 acceleration;
			public SensorVector3 angularVelocity;
			public SensorQuaternion rotation;
			public SensorQuaternion gameRotation;
			public GestureId gestureId;
		}

		/// <summary>
		/// Initializes the Wearable SDK and Plugin Bridge; where <paramref name="simulateDevices"/> is true, only simulated
		/// devices will be able to connect and get data from. If false, only real Wearable devices can be used.
		/// </summary>
		[DllImport("__Internal")]
		private static extern void WearableInitialize(bool simulateDevices);

		/// <summary>
		/// Starts the search for available Wearable devices in range whose RSSI is greater than <paramref name="rssiThreshold"/>
		/// </summary>
		/// <param name="rssiThreshold"></param>
		[DllImport("__Internal")]
		private static extern void WearableStartDeviceSearch(int rssiThreshold);

		/// <summary>
		/// Returns all available Wearable devices.
		/// </summary>
		/// <param name="devices"></param>
		/// <param name="count"></param>
		[DllImport("__Internal")]
		private static extern unsafe void WearableGetDiscoveredDevices(BridgeDevice** devices, int *count);

		/// <summary>
		/// Stops searching for available Wearable devices in range.
		/// </summary>
		[DllImport("__Internal")]
		private static extern void WearableStopDeviceSearch();

		/// <summary>
		/// Attempts to open a session with a specific Wearable Device by way of <paramref name="deviceUid"/>.
		/// </summary>
		/// <param name="deviceUid"></param>
		[DllImport("__Internal")]
		private static extern void WearableOpenSession(string deviceUid);

		/// <summary>
		/// Assesses the SessionStatus of the currently opened session for a specific Wearable device. If there has
		/// been an error, <paramref name="errorMsg"/> will be populated with the text contents of the error message.
		/// </summary>
		/// <param name="errorMsg"></param>
		/// <returns></returns>
		[DllImport("__Internal")]
		private static extern int WearableGetSessionStatus(ref string errorMsg);

		[DllImport("__Internal")]
		private static extern void WearableCloseSession();

		/// <summary>
		/// Returns all unread BridgeSensorFrames from the Wearable Device.
		/// </summary>
		/// <returns></returns>
		[DllImport("__Internal")]
		private static extern unsafe void WearableGetSensorFrames(BridgeSensorFrame** sensorFrames, int* count);

		/// <summary>
		/// Starts listening for accelerometer data. When enabled accelerometer data will be written to the
		/// SensorFrame. When disabled accelerometer data will stop being written to the SensorFrame. You can
		/// stop listening for accelerometer data and still keep the accelerometer sensor active.
		/// </summary>
		/// <param name="isEnabled"></param>
		/// <returns></returns>
		[DllImport("__Internal")]
		private static extern void WearableListenForAccelerometerData(bool isEnabled);

		/// <summary>
		/// Starts listening for rotation data. When enabled rotation data will be written to the
		/// SensorFrame. When disabled rotation data will stop being written to the SensorFrame. You can
		/// stop listening for rotation data and still keep the rotation sensor active. Pass the desired RotationMode
		/// to determine if GameRotation or Rotation will be used.
		/// </summary>
		/// <param name="isEnabled"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		[DllImport("__Internal")]
		private static extern void WearableListenForRotationData(bool isEnabled, RotationMode mode);

		/// <summary>
		/// Starts listening for gyroscope data. When enabled gyroscope data will be written to the
		/// SensorFrame. When disabled gyroscope data will stop being written to the SensorFrame. You can
		/// stop listening for gyroscope data and still keep the gyroscope sensor active.
		/// </summary>
		/// <param name="isEnabled"></param>
		[DllImport("__Internal")]
		private static extern void WearableListenForGyroscopeData(bool isEnabled);

		/// <summary>
		/// Enable a specific sensor by passing the desired <paramref name="sensorId"/> which is the int value
		/// of the SensorId enum. Pass the desired <paramref name="intervalId"/> which is the enum value of SensorUpdateInterval.
		/// All sensors by default will have the same update interval. If you enable a new sensor with a different update interval than
		/// the current configuration, all old sensors will be set to have the new update interval.
		/// </summary>
		/// <param name="sensorId"></param>
		/// <param name="intervalId"></param>
		/// <returns></returns>
		[DllImport("__Internal")]
		private static extern void WearableEnableSensor(int sensorId, int intervalId);

		/// <summary>
		/// Disable a specific sensor by passing the desired <paramref name="sensorId"/> which is the int value
		/// of the SensorId enum. Any listeners on the sensor that you disable will stop receiving callbacks once disabled.
		/// </summary>
		/// <param name="sensorId"></param>
		[DllImport("__Internal")]
		private static extern void WearableDisableSensor(int sensorId);

		/// <summary>
		/// Enable a specific gesture by passing the desired <paramref name="gestureId"/> which is the int value
		/// of the GestureId enum. You won't begin receiving data until you call WearableListenForGestureData(int gestureId)
		/// after gesture activation.
		/// </summary>
		/// <param name="gestureId"></param>
		[DllImport("__Internal")]
		private static extern void WearableEnableGesture(int gestureId);

		/// <summary>
		/// Disable a specific gesture by passing the desired <paramref name="gestureId"/> which is the int value
		/// of the GestureId enum.
		/// </summary>
		/// <param name="gestureId"></param>
		[DllImport("__Internal")]
		private static extern void WearableDisableGesture(int gestureId);

		/// <summary>
		/// Start listening for gesture data callbacks. If enabled, gestures will be written to the SensorFrame when detected. Only gestures
		/// that have been activated by WearableEnableGesture(int gestureId) will be listened for. When disabled no gesture data will be
		/// written for any GestureId.
		/// </summary>
		/// <param name="enabled"></param>
		[DllImport("__Internal")]
		private static extern void WearableListenForGestureData(bool enabled);

		/// <summary>
		/// Returns the ProductId of a device. This will default to 0 if there is not an open session yet. The ProductId of a device is only available once a session has been opened.
		/// </summary>
		[DllImport("__Internal")]
		private static extern int WearableGetDeviceProductID();

		/// <summary>
		/// Returns the VariantId of a device. This will default to 0 if there is not an open session yet. The VariantId of a device is only available once a session has been opened.
		/// </summary>
		[DllImport("__Internal")]
		private static extern int WearableGetDeviceVariantID();

		#endif
		#endregion
	}
}
