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
				Axis,
				CreatePath;

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
				CreatePath = new GUIContent
				(
					text: "Create Path"
				);
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
			Undo.undoRedoPerformed += MarkTargetPathsDirty;

			content.Update ();
			properties.Update (serializedObject);
		}

		private void OnDisable ()
		{
			Undo.undoRedoPerformed -= MarkTargetPathsDirty;
		}

		private void MarkTargetPathsDirty ()
		{
			foreach (var t in targets)
			{
				var pathDeformer = (PathDeformer)t;
				pathDeformer.SetPathDataDirty ();
			}
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.UpdateIfRequiredOrScript ();
			DeformEditorGUILayout.MinField (properties.Scale, 0f, content.Scale);
			EditorGUILayout.PropertyField (properties.Offset, content.Offset);
			EditorGUILayout.PropertyField (properties.Twist, content.Twist);
			EditorGUILayout.PropertyField (properties.Speed, content.Speed);
			EditorGUILayout.PropertyField (properties.Path, content.Path);
			EditorGUILayout.PropertyField (properties.Axis, content.Axis);

			if (properties.Path.objectReferenceValue == null && !properties.Path.hasMultipleDifferentValues)
			{
				if (GUILayout.Button (content.CreatePath))
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