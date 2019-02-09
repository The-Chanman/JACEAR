using UnityEditor;
using UnityEngine;

namespace Bose.Wearable.Editor.Inspectors
{
	[CustomPropertyDrawer(typeof(WearableUSBProvider))]
	public class WearableUSBProviderDrawer : PropertyDrawer
	{
		private const string DescriptionBox =
			"A provider that lets the Unity editor attach to a device connected by USB.";
		private const string DebugLoggingField = "_debugLogging";

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUILayout.HelpBox(DescriptionBox, MessageType.None);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(property.FindPropertyRelative(DebugLoggingField), WearableConstants.EmptyLayoutOptions);
		}
	}
}
