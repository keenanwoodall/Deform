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
		}

		private class Properties
		{
			public SerializedProperty Target;

			public Properties (SerializedObject obj)
			{
				Target = obj.FindProperty ("target");
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

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}