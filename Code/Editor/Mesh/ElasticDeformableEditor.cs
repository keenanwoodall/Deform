using UnityEngine;
using UnityEditor;
using Deform;
using Beans.Unity.Editor;

namespace DeformEditor
{
	[CustomEditor (typeof (ElasticDeformable)), CanEditMultipleObjects]
	public class ElasticDeformableEditor : DeformableEditor
	{
		private class Properties
		{
			public SerializedProperty MaskedDampingRatio;
			public SerializedProperty MaskedAngularFrequency;
			public SerializedProperty DampingRatio;
			public SerializedProperty AngularFrequency;
			public SerializedProperty Mask;

			public Properties (SerializedObject obj)
			{
				MaskedDampingRatio		= obj.FindProperty ("maskedDampingRatio");
				MaskedAngularFrequency	= obj.FindProperty ("maskedAngularFrequency");
				DampingRatio			= obj.FindProperty ("dampingRatio");
				AngularFrequency		= obj.FindProperty ("angularFrequency");
				Mask					= obj.FindProperty ("mask");
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
			EditorGUILayout.PropertyField(properties.DampingRatio);
			EditorGUILayout.PropertyField(properties.AngularFrequency);
			EditorGUILayout.PropertyField(properties.Mask);
			if (properties.Mask.enumValueIndex != 0)
			{
				EditorGUILayout.PropertyField(properties.MaskedDampingRatio);
				EditorGUILayout.PropertyField(properties.MaskedAngularFrequency);
			}
		}

		protected override void DrawHelpBoxes()
		{
			base.DrawHelpBoxes();
			EditorGUILayoutx.WIPAlert();
		}
	}
}
