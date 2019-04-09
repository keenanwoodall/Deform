using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (StarDeformer)), CanEditMultipleObjects]
	public class StarDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Frequency = new GUIContent (text: "Frequency", tooltip: "Number of crests and troughs per unit.");
			public static readonly GUIContent Magnitude = new GUIContent (text: "Magnitude", tooltip: "The strength of the wave.");
			public static readonly GUIContent Offset = new GUIContent (text: "Offset", tooltip: "The phase shift of the wave.");
			public static readonly GUIContent Speed = new GUIContent (text: "Speed", tooltip: "How much the phase shift changes per second.");
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private class Properties
		{
			public SerializedProperty Frequency;
			public SerializedProperty Magnitude;
			public SerializedProperty Offset;
			public SerializedProperty Speed;
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
			{
				Frequency	= obj.FindProperty ("frequency");
				Magnitude	= obj.FindProperty ("magnitude");
				Offset		= obj.FindProperty ("offset");
				Speed		= obj.FindProperty ("speed");
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

			EditorGUILayout.PropertyField (properties.Frequency, Content.Frequency);
			EditorGUILayout.PropertyField (properties.Magnitude, Content.Magnitude);
			EditorGUILayout.PropertyField (properties.Offset, Content.Offset);
			EditorGUILayout.PropertyField (properties.Speed, Content.Speed);
			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}