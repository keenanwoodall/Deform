using System.Linq;
using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (AutoGroupDeformer)), CanEditMultipleObjects]
	public class AutoGroupDeformerEditor : DeformerEditor
	{

		private static class Content
		{
			public static readonly string MixedValue = "-";
			public static readonly GUIContent RefreshGroup = new GUIContent
			(
				text: "Refresh Group",
				tooltip: "Should child deformers be found every time this deformer updates?\n-\nFor improved performance, set this to false when you know child deformers haven't been added or removed."
			);
		}

		private class Properties
		{
			public SerializedProperty RefreshGroup;

			public Properties (SerializedObject obj)
			{
				RefreshGroup = obj.FindProperty ("refreshGroup");
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

			EditorGUILayout.PropertyField (properties.RefreshGroup, Content.RefreshGroup);

			serializedObject.ApplyModifiedProperties ();

			var targetsHaveDifferentGroupSize = targets.Any (t => ((AutoGroupDeformer)t).GetGroupSize () != ((AutoGroupDeformer)target).GetGroupSize ());

			var firstGroup = (AutoGroupDeformer)target;
			EditorGUILayout.LabelField ($"Deformer Count: {(targetsHaveDifferentGroupSize ? Content.MixedValue : firstGroup.GetGroupSize ().ToString ())}", EditorStyles.miniLabel);
			if (!targetsHaveDifferentGroupSize && firstGroup.GetGroupSize () == 0)
				EditorGUILayout.HelpBox ("Add deformers to child game objects for this deformer to have an effect.", MessageType.Info);

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}