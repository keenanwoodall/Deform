using UnityEngine;
using UnityEditor;
using Deform;
using Beans.Unity.Editor;

namespace DeformEditor
{
	[CustomEditor (typeof (WaveDeformer)), CanEditMultipleObjects]
	public class WaveDeformerEditor : DeformerEditor
	{
		private static class Content
		{ 
			public static readonly GUIContent WaveLength = new GUIContent (text: "Wave Length", tooltip: "The period and magnitude of the wave.");
			public static readonly GUIContent Steepness = new GUIContent (text: "Steepness", tooltip: "The sharpness and height of the wave peaks.");
			public static readonly GUIContent Speed = new GUIContent (text: "Speed", tooltip: "The amount of change in the phase offset per second.");
			public static readonly GUIContent Offset = new GUIContent (text: "Offset", tooltip: "The wave's phase offset.");
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private class Properties
		{
			public SerializedProperty WaveLength;
			public SerializedProperty Steepness;
			public SerializedProperty Speed;
			public SerializedProperty Offset;
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
			{
				WaveLength	= obj.FindProperty ("waveLength");
				Steepness	= obj.FindProperty ("steepness");
				Speed		= obj.FindProperty ("speed");
				Offset		= obj.FindProperty ("offset");
				Axis		= obj.FindProperty ("axis");
			}
		}

		private Properties properties;

		protected override void OnEnable ()
		{
			base.OnEnable ();
			properties = new Properties (serializedObject);
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.UpdateIfRequiredOrScript ();

			EditorGUILayoutx.MinField (properties.WaveLength, 0f, Content.WaveLength);
			EditorGUILayout.Slider (properties.Steepness, 0f, 1f, Content.Steepness);
			EditorGUILayout.PropertyField (properties.Speed, Content.Speed);
			EditorGUILayout.PropertyField (properties.Offset, Content.Offset);
			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			serializedObject.ApplyModifiedProperties ();

			EditorGUILayoutx.WIPAlert ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}