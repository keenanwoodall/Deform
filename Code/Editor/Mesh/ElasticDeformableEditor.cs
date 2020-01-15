using UnityEngine;
using UnityEditor;
using Deform;
using Beans.Unity.Editor;

namespace DeformEditor
{
	[CustomEditor (typeof (ElasticDeformable)), CanEditMultipleObjects]
	public class ElasticDeformableEditor : DeformableEditor
	{
		private static class Content
		{
			public static readonly GUIContent ElasticStrength = new GUIContent (text: "Elastic Strength");
			public static readonly GUIContent ElasticDampening = new GUIContent (text: "Elastic Dampening");
		}

		private class Properties
		{
			public SerializedProperty ElasticStrength;
			public SerializedProperty ElasticDampening;

			public Properties (SerializedObject obj)
			{
				ElasticStrength		= obj.FindProperty ("elasticStrength");
				ElasticDampening	= obj.FindProperty ("elasticDampening");
			}
		}

		private Properties properties;

		protected override void OnEnable ()
		{
			base.OnEnable();
			properties = new Properties (serializedObject);
		}

		protected override void DrawMainSettings()
		{
			base.DrawMainSettings();
			EditorGUILayout.PropertyField(properties.ElasticStrength, Content.ElasticStrength);
			EditorGUILayout.PropertyField(properties.ElasticDampening, Content.ElasticDampening);
		}

		protected override void DrawHelpBoxes()
		{
			base.DrawHelpBoxes();
			EditorGUILayoutx.WIPAlert();
		}
	}
}
