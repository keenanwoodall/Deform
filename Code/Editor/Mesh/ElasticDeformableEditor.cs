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
			public SerializedProperty DampingRatio;
			public SerializedProperty AngularFrequency;

			public Properties (SerializedObject obj)
			{
				DampingRatio		= obj.FindProperty ("dampingRatio");
				AngularFrequency	= obj.FindProperty ("angularFrequency");
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
		}

		protected override void DrawHelpBoxes()
		{
			base.DrawHelpBoxes();
			EditorGUILayoutx.WIPAlert();
		}
	}
}
