using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (UVOffsetDeformer)), CanEditMultipleObjects]
	public class UVOffsetDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Offset = new GUIContent (text: "Offset");
		}

		private class Properties
		{
			public SerializedProperty Offset;

			public Properties (SerializedObject obj)
			{
				Offset = obj.FindProperty ("offset");
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

			EditorGUILayout.PropertyField (properties.Offset, Content.Offset);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}