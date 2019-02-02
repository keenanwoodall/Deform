using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (PathDeformer)), CanEditMultipleObjects]
	public class PathDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				Scale, 
				Offset, 
				Twist, 
				Speed, 
				Path, 
				Axis;

			public void Update ()
			{
				Scale = new GUIContent
				(
					text: "Scale"
				);
				Offset = new GUIContent
				(
					text: "Offset"
				);
				Twist = new GUIContent
				(
					text: "Twist"
				);
				Speed = new GUIContent
				(
					text: "Speed"
				);
				Path = new GUIContent
				(
					text: "Path"
				);
				Axis = DeformEditorGUIUtility.DefaultContent.Axis;
			}
		}

		private class Properties
		{
			public SerializedProperty 
				Scale, 
				Offset, 
				Twist, 
				Speed, 
				Path, 
				Axis;

			public void Update (SerializedObject obj)
			{
				Scale	= obj.FindProperty ("scale");
				Offset	= obj.FindProperty ("offset");
				Twist	= obj.FindProperty ("twist");
				Speed	= obj.FindProperty ("speed");
				Path	= obj.FindProperty ("path");
				Axis	= obj.FindProperty ("axis");
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

			DeformEditorGUILayout.MinField (properties.Scale, 0f, content.Scale);
			EditorGUILayout.PropertyField (properties.Offset, content.Offset);
			EditorGUILayout.PropertyField (properties.Twist, content.Twist);
			EditorGUILayout.PropertyField (properties.Speed, content.Speed);
			EditorGUILayout.PropertyField (properties.Path, content.Path);
			EditorGUILayout.PropertyField (properties.Axis, content.Axis);

			serializedObject.ApplyModifiedProperties ();
			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}