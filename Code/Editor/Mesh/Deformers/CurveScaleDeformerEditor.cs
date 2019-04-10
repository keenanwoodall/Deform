using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (CurveScaleDeformer)), CanEditMultipleObjects]
	public class CurveScaleDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent Bias = new GUIContent (text: "Bias");
			public static readonly GUIContent Offset = new GUIContent (text: "Offset");
			public static readonly GUIContent Curve = new GUIContent (text: "Curve");
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private class Properties
		{
			public SerializedProperty Factor;
			public SerializedProperty Bias;
			public SerializedProperty Offset;
			public SerializedProperty Curve;
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
				Bias	= obj.FindProperty ("bias");
				Offset	= obj.FindProperty ("offset");
				Curve	= obj.FindProperty ("curve");
				Axis	= obj.FindProperty ("axis");
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

			EditorGUILayout.PropertyField (properties.Factor, Content.Factor);
			EditorGUILayout.PropertyField (properties.Bias, Content.Bias);
			EditorGUILayout.PropertyField (properties.Offset, Content.Offset);
			EditorGUILayout.PropertyField (properties.Curve, Content.Curve);
			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		public override void OnSceneGUI ()
		{
			base.OnSceneGUI ();

			var curveScale = target as CurveScaleDeformer;

			var handleScale = new Vector3 (1f, 1f, curveScale.Axis.lossyScale.z);
			DeformHandles.Curve (curveScale.Curve, curveScale.Axis.position, curveScale.Axis.rotation, handleScale, curveScale.Factor * 0.5f, curveScale.Offset, curveScale.Bias * 0.5f);
		}
	}
}