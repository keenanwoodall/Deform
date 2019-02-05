using System.Linq;
using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (AutoGroupDeformer)), CanEditMultipleObjects]
	public class AutoGroupDeformerEditor : Editor
	{
		private readonly string MIXED_VALUE = "-";

		private class Content
		{
			public GUIContent RefreshGroup = new GUIContent
			(
				"Refresh Group",
				"Should child deformers be found every time this deformer updates?\n-\nFor improved performance, set this to false when you know child deformers haven't been added or removed."
			);
		}

		private class Properties
		{
			public SerializedProperty RefreshGroup;

			public void Update (SerializedObject obj)
			{
				RefreshGroup = obj.FindProperty ("refreshGroup");
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

			serializedObject.UpdateIfRequiredOrScript ();
			EditorGUILayout.PropertyField (properties.RefreshGroup, content.RefreshGroup);
			serializedObject.ApplyModifiedProperties ();

			var targetsHaveDifferentGroupSize = targets.Any (t => ((AutoGroupDeformer)t).GetGroupSize () != ((AutoGroupDeformer)target).GetGroupSize ());

			var firstGroup = (AutoGroupDeformer)target;
			EditorGUILayout.LabelField ($"Deformer Count: {(targetsHaveDifferentGroupSize ? MIXED_VALUE : firstGroup.GetGroupSize ().ToString ())}", EditorStyles.miniLabel);
			if (!targetsHaveDifferentGroupSize && firstGroup.GetGroupSize () == 0)
				EditorGUILayout.HelpBox ("Add deformers to child game objects for this deformer to have an effect.", MessageType.Info);

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}