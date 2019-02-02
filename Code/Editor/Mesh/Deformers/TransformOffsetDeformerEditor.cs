using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (TransformOffsetDeformer)), CanEditMultipleObjects]
	public class TransformOffsetDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent Target;

			public void Update ()
			{
				Target = new GUIContent
				(
					text: "Target",
					tooltip: "The target transform's position, rotation and scale will be added to the deformable's mesh."
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

			EditorGUILayout.PropertyField (properties.Target, content.Target);

			serializedObject.ApplyModifiedProperties ();
			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}