using UnityEngine;
using UnityEditor;
using Deform.Masking;

namespace DeformEditor.Masking
{
	[CustomEditor (typeof (VerticalGradientMask)), CanEditMultipleObjects]
	public class VerticalGradientMaskEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				Factor, 
				Falloff, 
				Invert, 
				Axis;

			public void Update ()
			{
				Factor = DeformEditorGUIUtility.DefaultContent.Factor;
				Falloff = DeformEditorGUIUtility.DefaultContent.Falloff;
				Invert = new GUIContent
				(
					text: "Invert"
				);
				Axis = DeformEditorGUIUtility.DefaultContent.Axis;
			}
		}

		private class Properties
		{
			public SerializedProperty
				Factor,
				Falloff, 
				Invert, 
				Axis;

			public void Update (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
				Falloff = obj.FindProperty ("falloff");
				Invert	= obj.FindProperty ("invert");
				Axis	= obj.FindProperty ("axis");
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
			EditorGUILayout.Slider (properties.Factor, 0f, 1f, content.Factor);
			DeformEditorGUILayout.MinField (properties.Falloff, 0f, content.Falloff);
			EditorGUILayout.PropertyField (properties.Invert, content.Invert);
			EditorGUILayout.PropertyField (properties.Axis, content.Axis);
			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}