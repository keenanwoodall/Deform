using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (SpherifyDeformer)), CanEditMultipleObjects]
	public class SpherifyDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent Radius = new GUIContent (text: "Radius", tooltip: "The radius of the sphere that the points are pushed towards.");
			public static readonly GUIContent Mode = new GUIContent (text: "Mode", tooltip: "Unlimited: Vertices' from any distance will interpolate towards the nearest point on the sphere.\nLimited: Vertices will only interpolate towards the sphere's surface when within the sphere.");
			public static readonly GUIContent Smooth = new GUIContent (text: "Smooth", tooltip: "Should the interpolation towards the sphere be smoothed.");
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private class Properties
		{
			public SerializedProperty Factor;
			public SerializedProperty Radius;
			public SerializedProperty Mode;
			public SerializedProperty Smooth;
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
				Radius	= obj.FindProperty ("radius");
				Mode	= obj.FindProperty ("mode");
				Smooth	= obj.FindProperty ("smooth");
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

			EditorGUILayout.Slider (properties.Factor, 0f, 1f, Content.Factor);
			EditorGUILayout.PropertyField (properties.Radius, Content.Radius);
			EditorGUILayout.PropertyField (properties.Mode, Content.Mode);
			using (new EditorGUI.DisabledScope (properties.Smooth.hasMultipleDifferentValues || (BoundsMode)properties.Mode.enumValueIndex == BoundsMode.Unlimited))
				EditorGUILayout.PropertyField (properties.Smooth, Content.Smooth);
			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		public override void OnSceneGUI ()
		{
			base.OnSceneGUI ();

			var spherify = target as SpherifyDeformer;

			DrawRadiusHandle (spherify);
			DrawFactorHandle (spherify);

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawRadiusHandle (SpherifyDeformer spherify)
		{
			using (new Handles.DrawingScope (Matrix4x4.TRS (spherify.Axis.position, spherify.Axis.rotation, spherify.Axis.lossyScale)))
			{
				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var newRadius = DeformHandles.Radius (Quaternion.identity, Vector3.zero, spherify.Radius);
					if (check.changed)
					{
						Undo.RecordObject (spherify, "Changed Radius");
						spherify.Radius = newRadius;
					}
				}
			}
		}

		private void DrawFactorHandle (SpherifyDeformer spherify)
		{
			if (spherify.Radius == 0f)
				return;

			var direction = Vector3.forward;
			var position = direction * (spherify.Factor * spherify.Radius);

			using (new Handles.DrawingScope (Matrix4x4.TRS (spherify.Axis.position, spherify.Axis.rotation, spherify.Axis.lossyScale)))
			{
				DeformHandles.Line (Vector3.zero, position, DeformHandles.LineMode.Light);
				DeformHandles.Line (position, direction * spherify.Radius, DeformHandles.LineMode.LightDotted);

				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var newWorldPosition = DeformHandles.Slider (position, direction);
					if (check.changed)
					{
						Undo.RecordObject (spherify, "Changed Factor");
						var newFactor = newWorldPosition.z / spherify.Radius;
						spherify.Factor = newFactor;
					}
				}
			}
		}
	}
}