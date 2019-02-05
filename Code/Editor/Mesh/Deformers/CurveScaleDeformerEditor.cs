using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (CurveScaleDeformer)), CanEditMultipleObjects]
	public class CurveScaleDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				Factor, 
				Bias, 
				Offset, 
				Curve, 
				Axis;

			public void Update ()
			{
				Factor = DeformEditorGUIUtility.DefaultContent.Factor;
				Bias = new GUIContent
				(
					text: "Bias"
				);
				Offset = new GUIContent
				(
					text: "Offset"
				);
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
				Bias, 
				Offset, 
				Curve, 
				Axis;

			public void Update (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
				Bias	= obj.FindProperty ("bias");
				Offset	= obj.FindProperty ("offset");
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

			serializedObject.UpdateIfRequiredOrScript ();
			EditorGUILayout.PropertyField (properties.Factor, content.Factor);
			EditorGUILayout.PropertyField (properties.Bias, content.Bias);
			EditorGUILayout.PropertyField (properties.Offset, content.Offset);
			EditorGUILayout.PropertyField (properties.Curve, content.Curve);
			EditorGUILayout.PropertyField (properties.Axis, content.Axis);
			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void OnSceneGUI ()
		{
			var curveScale = target as CurveScaleDeformer;

			DeformHandles.Curve (curveScale.Curve, curveScale.Axis, curveScale.Factor * 0.5f, curveScale.Offset, curveScale.Bias * 0.5f);
		}
	}
}