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
			public static readonly GUIContent CullingMode = new GUIContent("Culling Mode", $"{nameof(ElasticDeformable)} is a continuos simulation and should never be culled.");
		}
		private class Properties
		{
			public SerializedProperty CullingMode;
			public SerializedProperty MaskedDampingRatio;
			public SerializedProperty MaskedAngularFrequency;
			public SerializedProperty DampingRatio;
			public SerializedProperty AngularFrequency;
			public SerializedProperty Gravity;
			public SerializedProperty Mask;

			public Properties (SerializedObject obj)
			{
				CullingMode				= obj.FindProperty ("cullingMode");
				MaskedDampingRatio		= obj.FindProperty ("maskedDampingRatio");
				MaskedAngularFrequency	= obj.FindProperty ("maskedAngularFrequency");
				DampingRatio			= obj.FindProperty ("dampingRatio");
				AngularFrequency		= obj.FindProperty ("angularFrequency");
				Gravity					= obj.FindProperty ("gravity");
				Mask					= obj.FindProperty ("mask");
			}
		}

		private Properties properties;

		protected override void OnEnable ()
		{
			base.OnEnable();
			properties = new Properties (serializedObject);

			overrideCullingModeGUI = () =>
			{
				using (new EditorGUI.DisabledScope(properties.CullingMode.enumValueIndex != 1))
					EditorGUILayout.PropertyField(properties.CullingMode, Content.CullingMode);
			};
		}

		public override void OnInspectorGUI()
		{
			if (properties.CullingMode.enumValueIndex == 1)
			{
				EditorGUILayout.HelpBox ($"Culling Mode should be set to {nameof(CullingMode.AlwaysUpdate)}. The elasticity is a continuous effect and pausing it when culled when may result in snapping when becoming visible again.", MessageType.Error, true);
			}
			base.OnInspectorGUI();
		}

		protected override void DrawMainSettings()
		{
			base.DrawMainSettings();
			EditorGUILayout.PropertyField(properties.Mask);
			EditorGUILayout.PropertyField(properties.DampingRatio);
			EditorGUILayout.PropertyField(properties.AngularFrequency);
			if (properties.Mask.enumValueIndex != 0)
			{
				EditorGUILayout.PropertyField(properties.MaskedDampingRatio);
				EditorGUILayout.PropertyField(properties.MaskedAngularFrequency);
			}

			EditorGUILayout.PropertyField(properties.Gravity);
		}

		protected override void DrawHelpBoxes()
		{
			base.DrawHelpBoxes();
			EditorGUILayoutx.WIPAlert();
		}
	}
}
