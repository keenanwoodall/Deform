using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (CurveDisplaceDeformer)), CanEditMultipleObjects]
	public class CurveDisplaceDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				Factor, 
				Offset, 
				Curve, 
				Axis;

			public void Update ()
			{
				Factor = DeformEditorGUIUtility.DefaultContent.Factor;
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
				Offset,
				Curve, 
				Axis;

			public void Update (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
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
			EditorGUILayout.PropertyField (properties.Offset, content.Offset);
			EditorGUILayout.PropertyField (properties.Curve, content.Curve);
			EditorGUILayout.PropertyField (properties.Axis, content.Axis);
			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void OnSceneGUI ()
		{
			var curveDisplace = target as CurveDisplaceDeformer;

			if (curveDisplace.Curve == null || curveDisplace.Curve.length < 1)
				return;
			DeformHandles.Curve (curveDisplace.Curve, curveDisplace.Axis, curveDisplace.Factor, curveDisplace.Offset, 0f);
		}
	}
}