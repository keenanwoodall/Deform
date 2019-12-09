using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (TransformDeformer)), CanEditMultipleObjects]
	public class TransformDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Target = new GUIContent (text: "Target", tooltip: "The target transform will basically replace the mesh transform. This is handy for changing the pivot of the mesh.");
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
		}

		private class Properties
		{
			public SerializedProperty Target;
			public SerializedProperty Factor;

			public Properties (SerializedObject obj)
			{
				Target = obj.FindProperty ("target");
				Factor = obj.FindProperty("factor");
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

			EditorGUILayout.PropertyField (properties.Target, Content.Target);
			EditorGUILayout.PropertyField(properties.Factor, Content.Factor);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}