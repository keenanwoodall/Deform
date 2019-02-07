using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (PathDeformer)), CanEditMultipleObjects]
	public class PathDeformerEditor : DeformerEditor
	{
		private class Content
		{
			public static readonly GUIContent Scale = new GUIContent (text: "Scale");
			public static readonly GUIContent Offset = new GUIContent (text: "Offset");
			public static readonly GUIContent Twist = new GUIContent (text: "Twist");
			public static readonly GUIContent Speed = new GUIContent (text: "Speed");
			public static readonly GUIContent Path = new GUIContent (text: "Path");
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
			public static readonly GUIContent CreatePath = new GUIContent (text: "Create Path");
		}

		private class Properties
		{
			public SerializedProperty Scale;
			public SerializedProperty Offset;
			public SerializedProperty Twist;
			public SerializedProperty Speed;
			public SerializedProperty Path;
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
			{
				Scale	= obj.FindProperty ("scale");
				Offset	= obj.FindProperty ("offset");
				Twist	= obj.FindProperty ("twist");
				Speed	= obj.FindProperty ("speed");
				Path	= obj.FindProperty ("path");
				Axis	= obj.FindProperty ("axis");
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

			EditorGUILayout.PropertyField (properties.Scale, Content.Scale);
			EditorGUILayout.PropertyField (properties.Offset, Content.Offset);
			EditorGUILayout.PropertyField (properties.Twist, Content.Twist);
			EditorGUILayout.PropertyField (properties.Speed, Content.Speed);
			EditorGUILayout.PropertyField (properties.Path, Content.Path);
			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			if (properties.Path.objectReferenceValue == null && !properties.Path.hasMultipleDifferentValues)
			{
				if (GUILayout.Button (Content.CreatePath))
				{
					var pathDeformer = (PathDeformer)target;
					if (targets.Length == 1)
						properties.Path.objectReferenceValue = Undo.AddComponent<PathCreation.PathCreator> (pathDeformer.gameObject);
					else
					{
						var newPathObject = new GameObject ("Path Creator");
						Undo.RegisterCreatedObjectUndo (newPathObject, "Created Path Creator");
						var newPathCreator = newPathObject.AddComponent<PathCreation.PathCreator> ();
						properties.Path.objectReferenceValue = newPathCreator;
					}
				}
			}

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}