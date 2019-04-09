using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (InflateDeformer)), CanEditMultipleObjects]
	public class InflateDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent UseUpdatedNormals = new GUIContent (text: "Use Updated Normals", tooltip: "When true, the normals will be recalculated before the vertices are inflated. This is an expensive operation and will result in a split where adjacent triangles don't share vertices.");
		}

		private class Properties
		{
			public SerializedProperty Factor;
			public SerializedProperty UseUpdatedNormals;

			public Properties (SerializedObject obj)
			{
				Factor				= obj.FindProperty ("factor");
				UseUpdatedNormals	= obj.FindProperty ("useUpdatedNormals");
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

			EditorGUILayout.PropertyField (properties.Factor, Content.Factor);
			EditorGUILayout.PropertyField (properties.UseUpdatedNormals, Content.UseUpdatedNormals);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}