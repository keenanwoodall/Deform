using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (InflateDeformer)), CanEditMultipleObjects]
	public class InflateDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				Factor, 
				UseUpdatedNormals;

			public void Update ()
			{
				Factor = DeformEditorGUIUtility.DefaultContent.Factor;
				UseUpdatedNormals = new GUIContent
				(
					text: "Use Updated Normals",
					tooltip: "When true, the normals will be recalculated before the vertices are inflated. This is an expensive operation and will result in a split where adjacent triangles don't share vertices."
				);
			}
		}

		private class Properties
		{
			public SerializedProperty 
				Factor, 
				UseUpdatedNormals;

			public void Update (SerializedObject obj)
			{
				Factor				= obj.FindProperty ("factor");
				UseUpdatedNormals	= obj.FindProperty ("useUpdatedNormals");
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
			EditorGUILayout.PropertyField (properties.Factor, content.Factor);
			EditorGUILayout.PropertyField (properties.UseUpdatedNormals, content.UseUpdatedNormals);
			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}