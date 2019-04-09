using UnityEngine;
using UnityEditor;
using Beans.Unity.Editor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (RadialSkewDeformer)), CanEditMultipleObjects]
	public class RadialSkewDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent Mode = new GUIContent (text: "Mode", tooltip: "Unlimited: Entire mesh is skewed.\nLimited: Mesh is only skewed between bounds.");
			public static readonly GUIContent Top = new GUIContent (text: "Top", tooltip: "Any vertices above this will be unskewed.");
			public static readonly GUIContent Bottom = new GUIContent (text: "Bottom", tooltip: "Any vertices below this will be unskewed.");
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		public class Properties
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

			using (new EditorGUI.DisabledScope ((BoundsMode)properties.Mode.enumValueIndex == BoundsMode.Unlimited && !properties.Mode.hasMultipleDifferentValues))
			{
				using (new EditorGUI.IndentLevelScope ())
				{
					EditorGUILayoutx.MinField (properties.Top, properties.Bottom.floatValue, Content.Top);
					EditorGUILayoutx.MaxField (properties.Bottom, properties.Top.floatValue, Content.Bottom);
				}
			}


			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			serializedObject.ApplyModifiedProperties ();

			EditorGUILayoutx.WIPAlert ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		public override void OnSceneGUI ()
		{
			base.OnSceneGUI ();

			var skew = target as RadialSkewDeformer;

			if (skew.Mode == BoundsMode.Limited)
				DrawBoundsHandles (skew);
			else
				DrawAxisGuide (skew);
			DrawFactorHandle (skew);

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawAxisGuide (RadialSkewDeformer skew)
		{
			var direction = skew.Axis.up;

			var top = skew.Axis.position + direction * HandleUtility.GetHandleSize (skew.Axis.position) * 2f;
			var bottom = skew.Axis.position - direction * HandleUtility.GetHandleSize (skew.Axis.position) * 2f;

			DeformHandles.Line (top, bottom, DeformHandles.LineMode.LightDotted);
		}

		private void DrawBoundsHandles (RadialSkewDeformer skew)
		{
			var direction = skew.Axis.up;

			var topHandleWorldPosition = skew.Axis.position + direction * skew.Top;
			var bottomHandleWorldPosition = skew.Axis.position + direction * skew.Bottom;

			DeformHandles.Line (topHandleWorldPosition, bottomHandleWorldPosition, DeformHandles.LineMode.LightDotted);

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

		private void DrawFactorHandle (RadialSkewDeformer skew)
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