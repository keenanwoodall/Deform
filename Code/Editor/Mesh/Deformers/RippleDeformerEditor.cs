using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (RippleDeformer)), CanEditMultipleObjects]
	public class RippleDeformerEditor : DeformerEditor
	{
		private class Content
		{
			public static readonly GUIContent Frequency = new GUIContent (text: "Frequency", tooltip: "Higher values mean more ripples.");
			public static readonly GUIContent Magnitude = new GUIContent (text: "Magnitude", tooltip: "The strength of the ripples,");
			public static readonly GUIContent Mode = new GUIContent (text: "Mode", tooltip: "Unlimited: Entire mesh is rippled.\nLimited: Mesh only ripples between bounds.");
			public static readonly GUIContent Falloff = new GUIContent (text: "Falloff", tooltip: "When at 0, vertices outside the bounds will match the height of the bounds edge.\nWhen at 1, vertices outside the bounds will be unchanged.");
			public static readonly GUIContent InnerRadius = new GUIContent (text: "Inner Radius", tooltip: "Vertices within this radius don't ripple.");
			public static readonly GUIContent OuterRadius = new GUIContent (text: "Outer Radius", tooltip: "Vertices outside this radius don't ripple.");
			public static readonly GUIContent Speed = new GUIContent (text: "Speed", tooltip: "How fast the offset changes.");
			public static readonly GUIContent Offset = new GUIContent (text: "Offset", tooltip: "Offset of the ripple curve.");
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private class Properties
		{
			public SerializedProperty Frequency;
			public SerializedProperty Magnitude;
			public SerializedProperty Mode;
			public SerializedProperty Falloff;
			public SerializedProperty InnerRadius;
			public SerializedProperty OuterRadius;
			public SerializedProperty Speed;
			public SerializedProperty Offset;
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
			{
				Frequency	= obj.FindProperty ("frequency");
				Magnitude	= obj.FindProperty ("magnitude");
				Mode		= obj.FindProperty ("mode");
				Falloff		= obj.FindProperty ("falloff");
				InnerRadius = obj.FindProperty ("innerRadius");
				OuterRadius = obj.FindProperty ("outerRadius");
				Speed		= obj.FindProperty ("speed");
				Offset		= obj.FindProperty ("offset");
				Axis		= obj.FindProperty ("axis");
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

			EditorGUILayout.PropertyField (properties.Frequency, Content.Frequency);
			EditorGUILayout.PropertyField (properties.Magnitude, Content.Magnitude);
			EditorGUILayout.PropertyField (properties.Mode, Content.Mode);

			using (new EditorGUI.DisabledScope (properties.Mode.enumValueIndex == 0 && !properties.Mode.hasMultipleDifferentValues))
			{
				using (new EditorGUI.IndentLevelScope ())
				{
					EditorGUILayout.Slider (properties.Falloff, 0f, 1f, Content.Falloff);
					DeformEditorGUILayout.MaxField (properties.InnerRadius, properties.OuterRadius.floatValue, Content.InnerRadius);
					DeformEditorGUILayout.MinField (properties.OuterRadius, properties.InnerRadius.floatValue, Content.OuterRadius);
				}
			}

			EditorGUILayout.PropertyField (properties.Speed, Content.Speed);
			EditorGUILayout.PropertyField (properties.Offset, Content.Offset);
			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		public override void OnSceneGUI ()
		{
			base.OnSceneGUI ();

			var ripple = target as RippleDeformer;

			DrawMagnitudeHandle (ripple);
			if (ripple.Mode == BoundsMode.Limited)
				DrawBoundsHandles (ripple);

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawMagnitudeHandle (RippleDeformer ripple)
		{
			var direction = ripple.Axis.forward;
			var worldPosition = ripple.Axis.position + direction * ripple.Magnitude;

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newWorldPosition = DeformHandles.Slider (worldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (ripple, "Changed Magnitude");
					var newMagnitude = DeformHandlesUtility.DistanceAlongAxis (ripple.Axis, ripple.Axis.position, newWorldPosition, Axis.Z);
					ripple.Magnitude = newMagnitude;
				}
				var offset = newWorldPosition - ripple.Axis.position;
				DeformHandles.Line (ripple.Axis.position - offset, newWorldPosition, DeformHandles.LineMode.LightDotted);
			}

		}

		private void DrawBoundsHandles (RippleDeformer ripple)
		{
			var direction = ripple.Axis.up;

			var innerWorldPosition = ripple.Axis.position + direction * ripple.InnerRadius;
			var outerWorldPosition = ripple.Axis.position + direction * ripple.OuterRadius;

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newInnerWorldPosition = DeformHandles.Slider (innerWorldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (ripple, "Changed Inner Radius");
					var newInnerRadius = DeformHandlesUtility.DistanceAlongAxis (ripple.Axis, ripple.Axis.position, newInnerWorldPosition, Axis.Y);
					ripple.InnerRadius = newInnerRadius;
				}
			}

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newOuterWorldPosition = DeformHandles.Slider (outerWorldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (ripple, "Changed Outer Radius");
					var newOuterRadius = DeformHandlesUtility.DistanceAlongAxis (ripple.Axis, ripple.Axis.position, newOuterWorldPosition, Axis.Y);
					ripple.OuterRadius = newOuterRadius;
				}
			}

			DeformHandles.Circle (ripple.Axis.position, ripple.Axis.forward, ripple.Axis.right, ripple.InnerRadius);
			DeformHandles.Circle (ripple.Axis.position, ripple.Axis.forward, ripple.Axis.right, ripple.OuterRadius);
		}
	}
}