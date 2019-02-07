using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (SkewDeformer)), CanEditMultipleObjects]
	public class SkewDeformerEditor : DeformerEditor
	{
		private class Content
		{
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent Mode = new GUIContent (text: "Mode", tooltip: "Unlimited: The entire mesh is skewed.\nLimited: Only vertices between the bounds are skewed.");
			public static readonly GUIContent Top = DeformEditorGUIUtility.DefaultContent.Top;
			public static readonly GUIContent Bottom = DeformEditorGUIUtility.DefaultContent.Bottom;
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private class Properties
		{
			public SerializedProperty Factor;
			public SerializedProperty Mode;
			public SerializedProperty Top;
			public SerializedProperty Bottom;
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
				Mode	= obj.FindProperty ("mode");
				Top		= obj.FindProperty ("top");
				Bottom	= obj.FindProperty ("bottom");
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
			EditorGUILayout.PropertyField (properties.Mode, Content.Mode);

			using (new EditorGUI.DisabledScope (properties.Mode.enumValueIndex == 0 && !properties.Mode.hasMultipleDifferentValues))
			{
				using (new EditorGUI.IndentLevelScope ())
				{
					DeformEditorGUILayout.MinField (properties.Top, properties.Bottom.floatValue, Content.Top);
					DeformEditorGUILayout.MaxField (properties.Bottom, properties.Top.floatValue, Content.Bottom);
				}
			}

			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);
			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		public override void OnSceneGUI ()
		{
			base.OnSceneGUI ();

			var skew = target as SkewDeformer;

			if (skew.Mode == BoundsMode.Limited)
				DrawBoundsHandles (skew);
			else
				DrawAxisGuide (skew);
			DrawFactorHandle (skew);

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawAxisGuide (SkewDeformer skew)
		{
			var direction = skew.Axis.up;

			var top = skew.Axis.position + direction * HandleUtility.GetHandleSize (skew.Axis.position) * 2f;
			var bottom = skew.Axis.position - direction * HandleUtility.GetHandleSize (skew.Axis.position) * 2f;

			Handles.color = DeformEditorSettings.LightHandleColor;
			Handles.DrawLine (top, bottom);
		}

		private void DrawBoundsHandles (SkewDeformer skew)
		{
			var direction = skew.Axis.up;

			var topHandleWorldPosition = skew.Axis.position + direction * skew.Top;
			var bottomHandleWorldPosition = skew.Axis.position + direction * skew.Bottom;

			DeformHandles.Line (topHandleWorldPosition, bottomHandleWorldPosition, DeformHandles.LineMode.Light);

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newTopWorldPosition = DeformHandles.Slider (topHandleWorldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (skew, "Changed Top");
					var newTop = DeformHandlesUtility.DistanceAlongAxis (skew.Axis, skew.Axis.position, newTopWorldPosition, Axis.Y);
					skew.Top = newTop;
				}
			}

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newBottomWorldPosition = DeformHandles.Slider (bottomHandleWorldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (skew, "Changed Bottom");
					var newBottom = DeformHandlesUtility.DistanceAlongAxis (skew.Axis, skew.Axis.position, newBottomWorldPosition, Axis.Y);
					skew.Bottom = newBottom;
				}
			}
		}

		private void DrawFactorHandle (SkewDeformer skew)
		{
			var direction = skew.Axis.forward;
			var handleWorldPosition = skew.Axis.position + direction * skew.Factor;

			DeformHandles.Line (skew.Axis.position, handleWorldPosition, DeformHandles.LineMode.LightDotted);

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newWorldPosition = DeformHandles.Slider (handleWorldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (skew, "Changed Factor");
					var newFactor = DeformHandlesUtility.DistanceAlongAxis (skew.Axis, skew.Axis.position, newWorldPosition, Axis.Z);
					skew.Factor = newFactor;
				}
			}
		}
	}
}