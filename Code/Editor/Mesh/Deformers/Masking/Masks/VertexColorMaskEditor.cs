using UnityEngine;
using UnityEditor;
using Beans.Unity.Editor;
using Deform.Masking;

namespace DeformEditor.Masking
{
	[CustomEditor (typeof (VertexColorMask)), CanEditMultipleObjects]
	public class VertexColorMaskEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent Falloff = DeformEditorGUIUtility.DefaultContent.Falloff;
			public static readonly GUIContent Invert = new GUIContent (text: "Invert");
			public static readonly GUIContent Channel = new GUIContent (text: "Channel");
		}

		private class Properties
		{
			public SerializedProperty Factor;
			public SerializedProperty Falloff;
			public SerializedProperty Invert;
			public SerializedProperty Channel;

			public Properties (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
				Falloff = obj.FindProperty ("falloff");
				Invert	= obj.FindProperty ("invert");
				Channel = obj.FindProperty ("channel");
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

			EditorGUILayout.Slider (properties.Factor, 0f, 1f, Content.Factor);
			EditorGUILayoutx.MinField (properties.Falloff, 0f, Content.Falloff);
			EditorGUILayout.PropertyField (properties.Invert, Content.Invert);
			EditorGUILayout.PropertyField (properties.Channel, Content.Channel);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}