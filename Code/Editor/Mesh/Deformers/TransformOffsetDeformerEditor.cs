using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (TransformOffsetDeformer)), CanEditMultipleObjects]
	public class TransformOffsetDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Target = new GUIContent (text: "Target", tooltip: "The target transform's position, rotation and scale will be added to the deformable's mesh.");
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