using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (CylindrifyDeformer)), CanEditMultipleObjects]
	public class CylindrifyDeformerEditor : DeformerEditor
	{
		private class Content
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

			var direction = cylindrify.Axis.up;

			var worldPosition = cylindrify.Axis.position + direction * cylindrify.Factor * cylindrify.Radius;

			DeformHandles.Line (worldPosition, cylindrify.Axis.position, DeformHandles.LineMode.Light);
			DeformHandles.Line (worldPosition, cylindrify.Axis.position + direction * cylindrify.Radius, DeformHandles.LineMode.LightDotted);

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newWorldPosition = DeformHandles.MiniSlider (worldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (cylindrify, "Changed Radius");
					var newFactor = DeformHandlesUtility.DistanceAlongAxis (cylindrify.Axis, cylindrify.Axis.position, newWorldPosition, Axis.Y) / cylindrify.Radius;
					cylindrify.Factor = newFactor;
				}
			}
		}

		private void DrawRadiusHandle (CylindrifyDeformer cylindrify)
		{
			var direction = cylindrify.Axis.up;

			var worldPosition = cylindrify.Axis.position + direction * cylindrify.Radius;

			var size = HandleUtility.GetHandleSize (worldPosition) * DeformEditorSettings.ScreenspaceSliderHandleCapSize;

			DeformHandles.Circle (cylindrify.Axis.position, cylindrify.Axis.forward, cylindrify.Axis.up, cylindrify.Radius);

			DeformHandles.Line (worldPosition + cylindrify.Axis.forward * size, worldPosition + cylindrify.Axis.forward * size * 5f, DeformHandles.LineMode.Light);
			DeformHandles.Line (worldPosition - cylindrify.Axis.forward * size, worldPosition - cylindrify.Axis.forward * size * 5f, DeformHandles.LineMode.Light);

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newWorldPosition = DeformHandles.Slider (worldPosition, direction);
				if (check.changed)
				{
					var newRadius = DeformHandlesUtility.DistanceAlongAxis (cylindrify.Axis, cylindrify.Axis.position, newWorldPosition, Axis.Y);
					Undo.RecordObject (cylindrify, "Changed Radius");
					cylindrify.Radius = newRadius;
				}
			}
		}
	}
}