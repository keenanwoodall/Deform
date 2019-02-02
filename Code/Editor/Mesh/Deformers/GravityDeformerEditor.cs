using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (GravityDeformer)), CanEditMultipleObjects]
	public class GravityDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				Factor,
				Falloff, 
				Center;

			public void Update ()
			{
				Factor = DeformEditorGUIUtility.DefaultContent.Factor;
				Falloff = new GUIContent
				(
					text: "Falloff",
					tooltip: "The sharpness of the effect's transition."
				);
				Center = new GUIContent
				(
					text: "Center",
					tooltip: DeformEditorGUIUtility.Strings.AxisTooltip
				);
			}
		}

		private class Properties
		{
			public SerializedProperty
				Factor,
				Falloff, 
				Center;

			public void Update (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
				Falloff = obj.FindProperty ("falloff");
				Center	= obj.FindProperty ("center");
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

			EditorGUILayout.PropertyField (properties.Factor, content.Factor);
			DeformEditorGUILayout.MinField (properties.Falloff, 0f, content.Falloff);
			EditorGUILayout.PropertyField (properties.Center, content.Center);

			serializedObject.ApplyModifiedProperties ();
			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}