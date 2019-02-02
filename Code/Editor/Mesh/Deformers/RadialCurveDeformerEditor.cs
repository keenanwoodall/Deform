using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (RadialCurveDeformer)), CanEditMultipleObjects]
	public class RadialCurveDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				Factor, 
				Offset, 
				Falloff,
				Curve, 
				Axis;

			public void Update ()
			{
				Factor = DeformEditorGUIUtility.DefaultContent.Factor;
				Offset = new GUIContent
				(
					text: "Offset"
				);
				Falloff = DeformEditorGUIUtility.DefaultContent.Falloff;
				Curve = new GUIContent
				(
					text: "Curve"
				);
				Axis = DeformEditorGUIUtility.DefaultContent.Axis;
			}
		}

		private class Properties
		{
			public SerializedProperty 
				Factor,
				Offset, 
				Falloff,
				Curve,
				Axis;

			public void Update (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
				Offset	= obj.FindProperty ("offset");
				Falloff = obj.FindProperty ("falloff");
				Curve	= obj.FindProperty ("curve");
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

			EditorGUILayout.PropertyField (properties.Factor, content.Factor);
			EditorGUILayout.PropertyField (properties.Offset, content.Offset);
			DeformEditorGUILayout.MinField (properties.Falloff, 0f, content.Falloff);
			EditorGUILayout.PropertyField (properties.Curve, content.Curve);
			EditorGUILayout.PropertyField (properties.Axis, content.Axis);

			serializedObject.ApplyModifiedProperties ();
			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}