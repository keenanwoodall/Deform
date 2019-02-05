using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (UVOffsetDeformer)), CanEditMultipleObjects]
	public class UVOffsetDeformerEditor : Editor
	{
		private static class Content
		{
			public static GUIContent Offset = new GUIContent
			(
				text: "Offset"
			);
		}

		private class Properties
		{
			public SerializedProperty Offset;

			public void Update (SerializedObject obj)
			{
				Offset = obj.FindProperty ("offset");
			}
		}

		private Properties properties = new Properties ();

		private void OnEnable ()
		{
			properties.Update (serializedObject);
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.UpdateIfRequiredOrScript ();
			EditorGUILayout.PropertyField (properties.Offset, Content.Offset);
			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}