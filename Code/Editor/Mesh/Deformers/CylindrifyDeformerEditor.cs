using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (CylindrifyDeformer)), CanEditMultipleObjects]
	public class CylindrifyDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent Radius = new GUIContent (text: "Radius", tooltip: "The cylinder radius.");
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private class Properties
		{
			public SerializedProperty Factor;
			public SerializedProperty Radius; 
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
				Radius	= obj.FindProperty ("radius");
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
			EditorGUILayout.PropertyField (properties.Radius, Content.Radius);
			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		public override void OnSceneGUI ()
		{
			base.OnSceneGUI ();

			if (target == null)
				return;

			var cylindrify = target as CylindrifyDeformer;

			DrawRadiusHandle (cylindrify);
			DrawFactorHandle (cylindrify);

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawFactorHandle (CylindrifyDeformer cylindrify)
		{
			if (cylindrify.Radius == 0f)
				return;

			var position = Vector3.up * cylindrify.Factor * cylindrify.Radius;

			var axis = cylindrify.Axis;
			using (new Handles.DrawingScope (Matrix4x4.TRS (axis.position, axis.rotation, axis.lossyScale)))
			{
				DeformHandles.Line (position, Vector3.zero, DeformHandles.LineMode.Light);
				DeformHandles.Line (position, Vector3.up * cylindrify.Radius, DeformHandles.LineMode.LightDotted);

				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var newWorldPosition = DeformHandles.Slider (position, Vector3.up);
					if (check.changed)
					{
						Undo.RecordObject (cylindrify, "Changed Radius");
						cylindrify.Factor = newWorldPosition.y;
					}
				}
			}
		}

		private void DrawRadiusHandle (CylindrifyDeformer cylindrify)
		{
			var position = Vector3.up * cylindrify.Radius;

			var axis = cylindrify.Axis;
			using (new Handles.DrawingScope (Matrix4x4.TRS (axis.position, axis.rotation, axis.lossyScale)))
			{
				var size = HandleUtility.GetHandleSize (position) * DeformEditorSettings.ScreenspaceSliderHandleCapSize;

				DeformHandles.Circle (Vector3.zero, Vector3.forward, Vector3.up, cylindrify.Radius);

				DeformHandles.Line (position + Vector3.forward * size, position + Vector3.forward * size * 5f, DeformHandles.LineMode.Light);
				DeformHandles.Line (position - Vector3.forward * size, position - Vector3.forward * size * 5f, DeformHandles.LineMode.Light);

				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var newWorldPosition = DeformHandles.Slider (position, Vector3.up);
					if (check.changed)
					{
						Undo.RecordObject (cylindrify, "Changed Radius");
						cylindrify.Radius = cylindrify.Axis.position.y;
					}
				}
			}
		}
	}
}