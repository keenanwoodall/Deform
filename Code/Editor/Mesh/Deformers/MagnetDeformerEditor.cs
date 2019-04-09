using UnityEngine;
using UnityEditor;
using Beans.Unity.Editor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (MagnetDeformer)), CanEditMultipleObjects]
	public class MagnetDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent Falloff = new GUIContent (text: "Falloff", tooltip: "The sharpness of the effect's transition.");
			public static readonly GUIContent Center = new GUIContent (text: "Center", tooltip:DeformEditorGUIUtility.Strings.AxisTooltip);
		}

		private class Properties
		{
			public SerializedProperty Factor;
			public SerializedProperty Falloff;
			public SerializedProperty Center;

			public Properties (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
				Falloff = obj.FindProperty ("falloff");
				Center	= obj.FindProperty ("center");
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

			EditorGUILayout.PropertyField (properties.Factor, Content.Factor);
			EditorGUILayoutx.MinField (properties.Falloff, 0f, Content.Falloff);
			EditorGUILayout.PropertyField (properties.Center, Content.Center);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}