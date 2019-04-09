using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (DeformableManager)), CanEditMultipleObjects]
	public class DeformableManagerEditor : Editor
	{
		private static class Content
		{
			public static readonly GUIContent Update = new GUIContent (text: "Update", tooltip: "Should the manager update?");
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

		private void OnEnable ()
		{
			properties = new Properties (serializedObject);
		}

		public override void OnInspectorGUI ()
		{
			serializedObject.UpdateIfRequiredOrScript ();

			EditorGUILayout.PropertyField (properties.Update, Content.Update);

			serializedObject.ApplyModifiedProperties ();
		}
	}
}