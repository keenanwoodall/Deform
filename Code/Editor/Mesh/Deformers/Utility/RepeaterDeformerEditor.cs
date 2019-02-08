using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (RepeaterDeformer)), CanEditMultipleObjects]
	public class RepeaterDeformerEditor : DeformerEditor
	{
		private class Content
		{
			public static readonly GUIContent Iterations = new GUIContent ("Iterations", "The number of times the deformer is run. Be careful not to make it too high.");
			public static readonly GUIContent Deformer = new GUIContent ("Deformer", "The deformer to be processed");
		}

		private class Properties
		{
			public SerializedProperty Iterations;
			public SerializedProperty Deformer;

			public void Update (SerializedObject obj)
			{
				Iterations	= obj.FindProperty ("iterations");
				Deformer	= obj.FindProperty ("deformerElement");
			}
		}

		private Content content = new Content ();
		private Properties properties = new Properties ();

		protected override void OnEnable ()
		{
			base.OnEnable ();
			properties.Update (serializedObject);
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.UpdateIfRequiredOrScript ();

			DeformEditorGUILayout.MinField (properties.Iterations, 0, Content.Iterations);
			EditorGUILayout.PropertyField (properties.Deformer, Content.Deformer);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}