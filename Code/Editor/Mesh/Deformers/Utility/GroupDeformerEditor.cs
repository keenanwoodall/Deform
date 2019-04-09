using UnityEngine;
using UnityEditor;
using Beans.Unity.Editor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (GroupDeformer)), CanEditMultipleObjects]
	public class GroupDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent ClearDeformers = new GUIContent 
			(
				text: "Clear",
				tooltip: "Removes all elements."
			);
			public static readonly GUIContent CleanDeformers = new GUIContent
			(
				text: "Clean",
				tooltip: "Removes all empty elements."
			);
		}

		private class Properties
		{
			public SerializedProperty DeformerElements;

			public Properties (SerializedObject obj)
			{
				DeformerElements = obj.FindProperty ("deformerElements");
			}
		}

		private Properties properties;

		private ReorderableComponentElementList<Deformer> deformerList;

		protected override void OnEnable ()
		{
			base.OnEnable ();
			properties = new Properties (serializedObject);
			deformerList = new ReorderableComponentElementList<Deformer> (serializedObject, properties.DeformerElements);
		}

		private void OnDisable ()
		{
			deformerList.Dispose ();
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.UpdateIfRequiredOrScript ();

			deformerList.DoLayoutList ();

			serializedObject.ApplyModifiedProperties ();

			EditorGUILayout.Space ();

			var newDeformers = EditorGUILayoutx.DragAndDropArea<Deformer> ();
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
				if (GUILayout.Button (Content.ClearDeformers, EditorStyles.miniButtonLeft))
				{
					Undo.RecordObjects (targets, "Cleared Deformers");
					foreach (var t in targets)
						((GroupDeformer)t).DeformerElements.Clear ();
				}

				if (GUILayout.Button (Content.CleanDeformers, EditorStyles.miniButtonRight))
				{
					Undo.RecordObjects (targets, "Cleaned Deformers");
					foreach (var t in targets)
						((GroupDeformer)t).DeformerElements.RemoveAll (d => d.Component == null);
				}
			}

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}