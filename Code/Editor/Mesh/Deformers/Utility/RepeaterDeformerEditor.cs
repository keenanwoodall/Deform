using System.Reflection;
using UnityEngine;
using UnityEditor;
using Beans.Unity.Editor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (RepeaterDeformer)), CanEditMultipleObjects]
	public class RepeaterDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Iterations = new GUIContent ("Iterations", "The number of times the deformer is run. Be careful not to make it too high.");
			public static readonly GUIContent Deformer = new GUIContent ("Deformer", "The deformer to be processed");
		}

		private class Properties
		{
			public SerializedProperty Iterations;
			public SerializedProperty DeformerElement;

			public void Update (SerializedObject obj)
			{
				Iterations	= obj.FindProperty ("iterations");
				DeformerElement	= obj.FindProperty ("deformerElement");
			}
		}

		private Editor deformerEditor;
		private Properties properties = new Properties ();

		protected override void OnEnable ()
		{
			base.OnEnable ();
			properties.Update (serializedObject);
		}

		private void OnDisable ()
		{
			Dispose ();
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.UpdateIfRequiredOrScript ();

			EditorGUILayoutx.MinField (properties.Iterations, 0, Content.Iterations);
			EditorGUILayout.PropertyField (properties.DeformerElement, Content.Deformer);

			serializedObject.ApplyModifiedProperties ();

			var deformerProperty = properties.DeformerElement.FindPropertyRelative ("component");
			var deformer = deformerProperty.objectReferenceValue;


			if (!properties.DeformerElement.hasMultipleDifferentValues && deformer != null)
			{
				CreateCachedEditor (deformer, null, ref deformerEditor);

				SceneView.onSceneGUIDelegate -= SceneGUI;
				SceneView.onSceneGUIDelegate += SceneGUI;

				using (new EditorGUILayout.VerticalScope (DeformEditorResources.GetStyle ("Box")))
					deformerEditor.OnInspectorGUI ();
			}

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void SceneGUI (SceneView sceneView)
		{
			deformerEditor?.GetType ().GetMethod ("OnSceneGUI", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke (deformerEditor, null);
			deformerEditor.Repaint ();
		}

		public void Dispose ()
		{
			SceneView.onSceneGUIDelegate -= SceneGUI;
			Object.DestroyImmediate (deformerEditor, true);
			deformerEditor = null;
		}
	}
}