using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (Deformer), false), CanEditMultipleObjects]
	public class DeformerEditor : Editor
	{
		private static class Content
		{
			public static readonly GUIContent Update = new GUIContent (text: "Update");
		}

		private class Properties
		{
			public SerializedProperty Update;

			public Properties (SerializedObject obj)
			{
				Update = obj.FindProperty ("update");
			}
		}

		private Properties properties;

		protected virtual void OnEnable ()
		{
			properties = new Properties (serializedObject);
		}

		public override void OnInspectorGUI ()
		{
			serializedObject.UpdateIfRequiredOrScript ();

			EditorGUILayout.PropertyField (properties.Update, Content.Update);

			serializedObject.ApplyModifiedProperties ();
			EditorApplication.QueuePlayerLoopUpdate ();
		}

		public virtual void OnSceneGUI () { }
	}
}