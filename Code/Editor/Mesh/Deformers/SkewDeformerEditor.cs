using UnityEngine;
using UnityEditor;
using Beans.Unity.Editor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (SkewDeformer)), CanEditMultipleObjects]
	public class SkewDeformerEditor : DeformerEditor
	{
		private static class Content
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
		private VerticalBoundsHandle boundsHandle = new VerticalBoundsHandle ();

		protected override void OnEnable ()
		{
			base.OnEnable ();

			properties = new Properties (serializedObject);

			boundsHandle.HandleCapFunction = DeformHandles.HandleCapFunction;
			boundsHandle.DrawGuidelineCallback = (a, b) => DeformHandles.Line (a, b, DeformHandles.LineMode.LightDotted);
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

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		public override void OnSceneGUI ()
		{
			base.OnSceneGUI ();

			var skew = target as SkewDeformer;

			if (skew.Mode == BoundsMode.Limited)
			{
				boundsHandle.HandleColor = DeformEditorSettings.SolidHandleColor;
				boundsHandle.ScreenspaceHandleSize = DeformEditorSettings.ScreenspaceSliderHandleCapSize;
				if (boundsHandle.DrawHandle (skew.Top, skew.Bottom, skew.Axis, Vector3.up))
				{
					Undo.RecordObject (skew, "Changed Bounds");
					skew.Top = boundsHandle.Top;
					skew.Bottom = boundsHandle.Bottom;
				}
			}

			DrawFactorHandle (skew);

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawFactorHandle (SkewDeformer skew)
		{
			var center = Vector3.up * ((skew.Top + skew.Bottom) * 0.5f);
			var handlePosition = center + Vector3.forward * skew.Factor;

			using (new Handles.DrawingScope (Matrix4x4.TRS (skew.Axis.position, skew.Axis.rotation, skew.Axis.lossyScale)))
			{
				DeformHandles.Line (center, handlePosition, DeformHandles.LineMode.LightDotted);

				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var newWorldPosition = DeformHandles.Slider (handlePosition, Vector3.forward);
					if (check.changed)
					{
						Undo.RecordObject (skew, "Changed Factor");
						skew.Factor = newWorldPosition.z;
					}
				}
			}
		}
	}
}