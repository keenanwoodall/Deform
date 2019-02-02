using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (SpherifyDeformer)), CanEditMultipleObjects]
	public class SpherifyDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				Factor, 
				Radius, 
				Smooth, 
				Axis;

			public void Update ()
			{
				Factor = DeformEditorGUIUtility.DefaultContent.Factor;
				Radius = new GUIContent
				(
					text: "Radius",
					tooltip: "The radius of the sphere that the points are pushed towards."
				);
				Smooth = new GUIContent
				(
					text: "Smooth",
					tooltip: "Should the interpolation towards the sphere be smoothed."
				);
				Axis = DeformEditorGUIUtility.DefaultContent.Axis;
			}
		}

		private class Properties
		{
			public SerializedProperty 
				Factor, 
				Radius, 
				Smooth, 
				Axis;

			public void Update (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
				Radius	= obj.FindProperty ("radius");
				Smooth	= obj.FindProperty ("smooth");
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

			EditorGUILayout.Slider (properties.Factor, 0f, 1f, content.Factor);
			EditorGUILayout.PropertyField (properties.Radius, content.Radius);
			EditorGUILayout.PropertyField (properties.Smooth, content.Smooth);
			EditorGUILayout.PropertyField (properties.Axis, content.Axis);

			serializedObject.ApplyModifiedProperties ();
			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void OnSceneGUI ()
		{
			if (target == null)
				return;

			var spherify = target as SpherifyDeformer;

			DrawFactorHandle (spherify);
			DrawRadiusHandle (spherify);

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawRadiusHandle (SpherifyDeformer spherify)
		{
			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newRadius = DeformHandles.Radius (spherify.Axis.rotation, spherify.Axis.position, spherify.Radius);
				if (check.changed)
				{
					Undo.RecordObject (spherify, "Changed Radius");
					spherify.Radius = newRadius;
				}
			}
		}

		private void DrawFactorHandle (SpherifyDeformer spherify)
		{
			if (spherify.Radius == 0f)
				return;

			var direction = spherify.Axis.forward;
			var worldPosition = spherify.Axis.position + direction * (spherify.Factor * spherify.Radius);

			DeformHandles.Line (spherify.Axis.position, worldPosition, DeformHandles.LineMode.Light);
			DeformHandles.Line (worldPosition, spherify.Axis.position + direction * spherify.Radius, DeformHandles.LineMode.LightDotted);

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newWorldPosition = DeformHandles.Slider (worldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (spherify, "Changed Factor");
					var newFactor = DeformHandlesUtility.DistanceAlongAxis (spherify.Axis, spherify.Axis.position, newWorldPosition, Axis.Z) / spherify.Radius;
					spherify.Factor = newFactor;
				}
			}
		}
	}
}