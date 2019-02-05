using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (StarDeformer)), CanEditMultipleObjects]
	public class StarDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				Frequency, 
				Magnitude, 
				Offset, 
				Speed, 
				Axis;

			public void Update ()
			{
				Frequency = new GUIContent
				(
					text: "Frequency",
					tooltip: "Number of crests and troughs per unit."
				);
				Magnitude = new GUIContent
				(
					text: "Magnitude",
					tooltip: "The strength of the wave."
				);
				Offset = new GUIContent
				(
					text: "Offset",
					tooltip: "The phase shift of the wave."
				);
				Speed = new GUIContent
				(
					text: "Speed",
					tooltip: "How much the phase shift changes per second."
				);
				Axis = DeformEditorGUIUtility.DefaultContent.Axis;
			}
		}

		private class Properties
		{
			public SerializedProperty 
				Frequency, 
				Magnitude, 
				Offset,
				Speed,
				Axis;

			public void Update (SerializedObject obj)
			{
				Frequency	= obj.FindProperty ("frequency");
				Magnitude	= obj.FindProperty ("magnitude");
				Offset		= obj.FindProperty ("offset");
				Speed		= obj.FindProperty ("speed");
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

			serializedObject.UpdateIfRequiredOrScript ();
			EditorGUILayout.PropertyField (properties.Frequency, content.Frequency);
			EditorGUILayout.PropertyField (properties.Magnitude, content.Magnitude);
			EditorGUILayout.PropertyField (properties.Offset, content.Offset);
			EditorGUILayout.PropertyField (properties.Speed, content.Speed);
			EditorGUILayout.PropertyField (properties.Axis, content.Axis);
			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}