using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (WaveDeformer)), CanEditMultipleObjects]
	public class WaveDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				WaveLength, 
				Steepness, 
				Speed,
				Offset,
				Axis;

			public void Update ()
			{
				WaveLength = new GUIContent 
				(
					text: "Wave Length",
					tooltip: "The period and magnitude of the wave."
				);
				Steepness = new GUIContent 
				(
					text: "Steepness",
					tooltip: "The sharpness and height of the wave peaks."
				);
				Speed = new GUIContent 
				(
					text: "Speed",
					tooltip: "The amount of change in the phase offset per second."
				);
				Offset = new GUIContent 
				(
					text: "Offset", tooltip: "The wave's phase offset."
				);
				Axis = DeformEditorGUIUtility.DefaultContent.Axis;
			}
		}

		private class Properties
		{
			public SerializedProperty 
				WaveLength,
				Steepness, 
				Speed, 
				Offset,
				Axis;

			public void Update (SerializedObject obj)
			{
				WaveLength	= obj.FindProperty ("waveLength");
				Steepness	= obj.FindProperty ("steepness");
				Speed		= obj.FindProperty ("speed");
				Offset		= obj.FindProperty ("offset");
				Axis		= obj.FindProperty ("axis");
			}
		}

		private Content content = new Content ();
		private Properties properties = new Properties ();

		private void OnEnable ()
		{
			content.Update ();
			properties.Update (serializedObject);
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			DeformEditorGUILayout.MinField (properties.WaveLength, 0f, content.WaveLength);
			EditorGUILayout.Slider (properties.Steepness, 0f, 1f, content.Steepness);
			EditorGUILayout.PropertyField (properties.Speed, content.Speed);
			EditorGUILayout.PropertyField (properties.Offset, content.Offset);
			EditorGUILayout.PropertyField (properties.Axis, content.Axis);

			DeformEditorGUILayout.WIPAlert ();

			serializedObject.ApplyModifiedProperties ();
			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}