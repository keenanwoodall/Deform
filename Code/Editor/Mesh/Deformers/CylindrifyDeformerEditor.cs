using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (CylindrifyDeformer)), CanEditMultipleObjects]
	public class CylindrifyDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				Factor, 
				Radius, 
				Axis;

			public void Update ()
			{
				Factor = DeformEditorGUIUtility.DefaultContent.Factor;
				Radius = new GUIContent
				(
					text: "Radius",
					tooltip: "The cylinder radius."
				);
				Axis = DeformEditorGUIUtility.DefaultContent.Axis;
			}
		}

		private class Properties
		{
			public SerializedProperty 
				Factor, 
				Radius, 
				Axis;

			public void Update (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
				Radius	= obj.FindProperty ("radius");
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
			EditorGUILayout.PropertyField (properties.Radius, content.Radius);
			EditorGUILayout.PropertyField (properties.Axis, content.Axis);

			serializedObject.ApplyModifiedProperties ();
			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void OnSceneGUI ()
		{
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