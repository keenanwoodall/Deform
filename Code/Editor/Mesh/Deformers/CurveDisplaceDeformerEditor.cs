using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (CurveDisplaceDeformer)), CanEditMultipleObjects]
	public class CurveDisplaceDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent Offset = new GUIContent (text: "Offset");
			public static readonly GUIContent Curve = new GUIContent (text: "Curve");
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private class Properties
		{
			public SerializedProperty Factor;
			public SerializedProperty Offset;
			public SerializedProperty Curve;
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
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
			EditorGUILayout.PropertyField (properties.Offset, Content.Offset);
			EditorGUILayout.PropertyField (properties.Curve, Content.Curve);
			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		public override void OnSceneGUI ()
		{
			base.OnSceneGUI ();

			var curveDisplace = target as CurveDisplaceDeformer;

			if (curveDisplace.Curve == null || curveDisplace.Curve.length < 1)
				return;
			DeformHandles.Curve (curveDisplace.Curve, curveDisplace.Axis, curveDisplace.Factor, curveDisplace.Offset, 0f);
		}
	}
}