using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bose.Wearable
{
	/// <summary>
	/// Provides a minimal data provider that allows connection to a virtual device, and logs messages when provider
	/// methods are called. Never generates data frames.
	/// </summary>
	[Serializable]
	public sealed class WearableDebugProvider : WearableProviderBase
	{
		private enum RotationType
		{
			Euler,
			AxisAngle
		}
		
		public string Name
		{
			get { return _name; }
			set {_name = value; }
		}

		public int RSSI
		{
			get { return _rssi; }
			set { _rssi = value; }
		}

		public string UID
		{
			get { return _uid; }
			set { _uid = value; }
		}

		public ProductId ProductId
		{
			get { return _productId; }
			set { _productId = value; }
		}

		public byte VariantId
		{
			get { return _variantId; }
			set { _variantId = value; }
		}

		public bool Verbose
		{
			get { return _verbose; }
			set { _verbose = value; }
		}

		#region Provider Unique

		public void SimulateDisconnect()
		{
			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderSimulateDisconnect);
			}

			DisconnectFromDevice();
		}

		#endregion

		#region WearableProvider Implementation

		internal override void SearchForDevices(Action<Device[]> onDevicesUpdated)
		{
			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderSearchingForDevices);
			}

			UpdateVirtualDeviceInfo();

			if (onDevicesUpdated != null)
			{
				onDevicesUpdated.Invoke(new []{_virtualDevice});
			}
		}

		internal override void StopSearchingForDevices()
		{
			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderStoppedSearching);
			}
		}

		internal override void ConnectToDevice(Device device, Action onSuccess, Action onFailure)
		{
			DisconnectFromDevice();

			UpdateVirtualDeviceInfo();

			if (device != _virtualDevice)
			{
				Debug.LogWarning(WearableConstants.DebugProviderInvalidConnectionWarning);
				return;
			}

			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderConnectingToDevice);
			}

			OnDeviceConnecting(_virtualDevice);

			_virtualDevice.isConnected = true;
			_connectedDevice = _virtualDevice;

			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderConnectedToDevice);
			}

			if (onSuccess != null)
			{
				onSuccess.Invoke();
			}

			OnDeviceConnected(_virtualDevice);
		}

		internal override void DisconnectFromDevice()
		{
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

			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderDisconnectedToDevice);
			}

			OnDeviceDisconnected(_connectedDevice.Value);

			_virtualDevice.isConnected = false;
			_connectedDevice = null;
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

			if (_verbose)
			{
				Debug.LogFormat(
					WearableConstants.DebugProviderSetUpdateInterval, 
					Enum.GetName(typeof(SensorUpdateInterval), updateInterval));
			}
			
			_sensorUpdateInterval = updateInterval;
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

			if (_verbose)
			{
				Debug.LogFormat(WearableConstants.DebugProviderStartSensor, Enum.GetName(typeof(SensorId), sensorId));
			}

			_sensorStatus[sensorId] = true;
			_nextSensorUpdateTime = Time.unscaledTime;
		}

		internal override void StopSensor(SensorId sensorId)
		{
			if (!_sensorStatus[sensorId])
			{
				return;
			}

			if (_verbose)
			{
				Debug.LogFormat(WearableConstants.DebugProviderStopSensor, Enum.GetName(typeof(SensorId), sensorId));
			}

			_sensorStatus[sensorId] = false;
		}

		internal override bool GetSensorActive(SensorId sensorId)
		{
			return (_connectedDevice != null) && _sensorStatus[sensorId];
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

			if (_verbose)
			{
				Debug.LogFormat(WearableConstants.DebugProviderEnableGesture, Enum.GetName(typeof(GestureId), gestureId));
			}

			_gestureStatus[gestureId] = true;
		}

		internal override void DisableGesture(GestureId gestureId)
		{
			if (!_gestureStatus[gestureId])
			{
				return;
			}

			if (_verbose)
			{
				Debug.LogFormat(WearableConstants.DebugProviderDisableGesture, Enum.GetName(typeof(GestureId), gestureId));
			}

			_gestureStatus[gestureId] = false;
		}

		internal override bool GetGestureEnabled(GestureId gestureId)
		{
			return (_connectedDevice != null) && _gestureStatus[gestureId];
		}

		internal override void OnInitializeProvider()
		{
			base.OnInitializeProvider();

			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderInit);
			}
		}

		internal override void OnDestroyProvider()
		{
			base.OnDestroyProvider();

			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderDestroy);
			}
		}

		internal override void OnEnableProvider()
		{
			base.OnEnableProvider();

			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderEnable);
			}
		}

		internal override void OnDisableProvider()
		{
			base.OnDisableProvider();

			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderDisable);
			}
		}

		internal override void OnUpdate()
		{
			UpdateVirtualDeviceInfo();

			if (!_enabled)
			{
				return;
			}
			
			// Clear the current frames; _lastSensorFrame will retain its previous value.
			if (_currentSensorFrames.Count > 0)
			{
				_currentSensorFrames.Clear();
			}

			while (Time.unscaledTime >= _nextSensorUpdateTime)
			{
				// If it's time to emit frames, do so until we have caught up.
				float deltaTime = WearableTools.SensorUpdateIntervalToSeconds(_sensorUpdateInterval);
				_nextSensorUpdateTime += deltaTime;

				// Update the timestamp and delta-time
				_lastSensorFrame.deltaTime = deltaTime;
				_lastSensorFrame.timestamp = _nextSensorUpdateTime;
				
				// Calculate rotation, which is used by all sensors.
				if (_simulateMovement)
				{
					if (_rotationType == RotationType.Euler)
					{
						_rotation = Quaternion.Euler(_eulerSpinRate * _lastSensorFrame.timestamp);
					}
					else if (_rotationType == RotationType.AxisAngle)
					{
						_rotation = Quaternion.AngleAxis(
							_axisAngleSpinRate.w * _lastSensorFrame.timestamp, 
							new Vector3(_axisAngleSpinRate.x, _axisAngleSpinRate.y, _axisAngleSpinRate.z).normalized);
					}
				}
				else
				{
					_rotation = Quaternion.identity;
				}
					
				
				// Update all active sensors
				if (_sensorStatus[SensorId.Accelerometer])
				{
					UpdateAccelerometerData();
				}

				if (_sensorStatus[SensorId.Gyroscope])
				{
					UpdateGyroscopeData();
				}

				if (_sensorStatus[SensorId.Rotation] || _sensorStatus[SensorId.GameRotation])
				{
					UpdateRotationSensorData();
				}

				UpdateGestureData();

				// Emit the frame
				_currentSensorFrames.Add(_lastSensorFrame);
				OnSensorsUpdated(_lastSensorFrame);
			}
		}

		#endregion

		#region Private

		[SerializeField]
		private string _name;

		[SerializeField]
		private int _rssi;

		[SerializeField]
		private ProductId _productId;

		[SerializeField]
		private byte _variantId;

		[SerializeField]
		private string _uid;

		[SerializeField]
		private bool _verbose;

		[SerializeField]
		private bool _simulateMovement;

		[SerializeField]
		private Vector3 _eulerSpinRate;

		[SerializeField]
		private Vector4 _axisAngleSpinRate;

		[SerializeField]
		private RotationType _rotationType;

		private Quaternion _rotation;

		private readonly Dictionary<SensorId, bool> _sensorStatus;
		private SensorUpdateInterval _sensorUpdateInterval;
		private float _nextSensorUpdateTime;
		
		private readonly Dictionary<GestureId, bool> _gestureStatus;

		private Device _virtualDevice;

		
		internal WearableDebugProvider()
		{
			_virtualDevice = new Device
			{
				name = _name,
				rssi = _rssi,
				uid = _uid,
				productId = _productId,
				variantId = _variantId
			};

			_name = WearableConstants.DebugProviderDefaultDeviceName;
			_rssi = WearableConstants.DebugProviderDefaultRSSI;
			_uid = WearableConstants.DebugProviderDefaultUID;
			_productId = WearableConstants.DebugProviderDefaultProductId;
			_variantId = WearableConstants.DebugProviderDefaultVariantId;

			_verbose = true;
			
			_eulerSpinRate = Vector3.zero;
			_axisAngleSpinRate = Vector3.up;

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
			
			_nextSensorUpdateTime = 0.0f;
			_rotation = Quaternion.identity;
		}

		private void UpdateVirtualDeviceInfo()
		{
			_virtualDevice.name = _name;
			_virtualDevice.rssi = _rssi;
			_virtualDevice.uid = _uid;
			_virtualDevice.productId = _productId;
			_virtualDevice.variantId = _variantId;
		}

		/// <summary>
		/// Simulate some acceleration data. 
		/// </summary>
		private void UpdateAccelerometerData()
		{
			Quaternion invRot = new Quaternion(-_rotation.x, -_rotation.y, -_rotation.z, _rotation.w);
			_lastSensorFrame.acceleration.value = invRot * new Vector3(0.0f, 9.80665f, 0.0f);
			_lastSensorFrame.acceleration.accuracy = SensorAccuracy.High;
		}

		/// <summary>
		/// Simulate some gyro data.
		/// </summary>
		private void UpdateGyroscopeData()
		{
			
			Quaternion invRot = new Quaternion(-_rotation.x, -_rotation.y, -_rotation.z, _rotation.w);
			_lastSensorFrame.angularVelocity.value = invRot * (_eulerSpinRate * Mathf.Deg2Rad);
			_lastSensorFrame.angularVelocity.accuracy = SensorAccuracy.High;
		}

		/// <summary>
		/// Simulate some rotation data.
		/// </summary>
		private void UpdateRotationSensorData()
		{
			// This is already calculated for us since the other sensors need it too.
			_lastSensorFrame.rotation.value = _rotation;
			_lastSensorFrame.rotation.measurementUncertainty = 0.0f;
			_lastSensorFrame.gameRotation = _lastSensorFrame.rotation;
		}

		/// <summary>
		/// Simulate some gesture data.
		/// </summary>
		private void UpdateGestureData()
		{
			// NOTE: Gestures are not currently implemented within the WearableDebugProvider.
			_lastSensorFrame.gestureId = GestureId.None;
		}
		
		#endregion
	}
}
