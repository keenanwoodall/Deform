using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (RepeaterDeformer))]
	public class RepeaterDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent Iterations = new GUIContent ("Iterations", "The number of times the deformer is run. Be careful not to make it too high.");
			public GUIContent Deformer = new GUIContent ("Deformer", "The deformer to be processed");
		}

		private class Properties
		{
			public SerializedProperty 
				Iterations,
				Deformer;

			public void Update (SerializedObject obj)
			{
				Iterations	= obj.FindProperty ("iterations");
				Deformer	= obj.FindProperty ("deformer");
			}
		}

		private Content content = new Content ();
		private Properties properties = new Properties ();

		private void OnEnable ()
		{
			properties.Update (serializedObject);
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			DeformEditorGUILayout.MinField (properties.Iterations, 0, content.Iterations);
			EditorGUILayout.PropertyField (properties.Deformer, content.Deformer);

			serializedObject.ApplyModifiedProperties ();
			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}