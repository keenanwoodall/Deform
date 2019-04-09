using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (ScaleDeformer))]
	public class ScaleDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private class Properties
		{
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
			{
				Axis = obj.FindProperty ("axis");
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

			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}