using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (GroupDeformer)), CanEditMultipleObjects]
	public class GroupDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent ClearDeformers = new GUIContent 
			(
				text: "Clear",
				tooltip: "Removes all elements."
			);
			public GUIContent CleanDeformers = new GUIContent
			(
				text: "Clean",
				tooltip: "Removes all empty elements."
			);
		}

		private class Properties
		{
			public SerializedProperty DeformerElements;

			public void Update (SerializedObject obj)
			{
				DeformerElements = obj.FindProperty ("deformerElements");
			}
		}

		private Content content = new Content ();
		private Properties properties = new Properties ();

		private DeformerListEditor deformerList;

		private void OnEnable ()
		{
			properties.Update (serializedObject);

			deformerList = new DeformerListEditor (serializedObject, properties.DeformerElements);
		}

		private void OnDisable() => deformerList.Dispose();

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.Update ();
			deformerList.DoLayoutList ();
			serializedObject.ApplyModifiedProperties ();

			EditorGUILayout.Space ();

			var newDeformers = DeformEditorGUILayout.DragAndDropComponentArea<Deformer> ();
			if (newDeformers != null && newDeformers.Count > 0)
			{
				Undo.RecordObjects (targets, "Added Deformers");
				foreach (var t in targets)
				{
					var elements = ((GroupDeformer)t).DeformerElements;
					foreach (var newDeformer in newDeformers)
						elements.Add (new DeformerElement (newDeformer));
				}
			}

			EditorGUILayout.Space ();

			using (new EditorGUILayout.HorizontalScope ())
			{
				if (GUILayout.Button (content.ClearDeformers, EditorStyles.miniButtonRight))
				{
					Undo.RecordObjects (targets, "Cleared Deformers");
					foreach (var t in targets)
						((GroupDeformer)t).DeformerElements.Clear ();
				}

				if (GUILayout.Button (content.CleanDeformers, EditorStyles.miniButtonRight))
				{
					Undo.RecordObjects (targets, "Cleaned Deformers");
					foreach (var t in targets)
						((GroupDeformer)t).DeformerElements.RemoveAll (d => d.Deformer == null);
				}
			}


			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}