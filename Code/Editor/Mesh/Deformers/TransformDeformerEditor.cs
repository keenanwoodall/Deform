using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (TransformDeformer)), CanEditMultipleObjects]
	public class TransformDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent Target;

			public void Update ()
			{
				Target = new GUIContent
				(
					text: "Target",
					tooltip: "The target transform will basically replace the mesh transform. This is handy for changing the pivot of the mesh."
				);
			}
		}

		private class Properties
		{
			public SerializedProperty Target;

			public void Update (SerializedObject obj)
			{
				Target = obj.FindProperty ("target");
			}
		}

		private Content content = new Content ();
		private Properties properties = new Properties ();

		private void OnEnable ()
		{
			content.Update ();
			properties.Update (serializedObject);
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.UpdateIfRequiredOrScript ();
			EditorGUILayout.PropertyField (properties.Target, content.Target);
			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}