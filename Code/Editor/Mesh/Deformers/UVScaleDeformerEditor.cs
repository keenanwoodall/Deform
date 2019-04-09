using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (UVScaleDeformer)), CanEditMultipleObjects]
	public class UVScaleDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent Scale = new GUIContent (text: "Scale");
		}

		private class Properties
		{
			public SerializedProperty Factor;
			public SerializedProperty Scale;

			public Properties (SerializedObject obj)
			{
				Factor = obj.FindProperty ("factor");
				Scale = obj.FindProperty ("scale");
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
			EditorGUILayout.PropertyField (properties.Scale, Content.Scale);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}