using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Beans.Unity.Editor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (TwirlDeformer)), CanEditMultipleObjects]
	public class TwirlDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Angle = new GUIContent (text: "Angle", tooltip: "How many degrees each vertice will rotate around the axis based on distance between the inner and outer bounds.");
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent Mode = new GUIContent (text: "Mode", tooltip: "Unlimited: The entire mesh will twirl.\nLimited: Only vertices between the bounds will twirl.");
			public static readonly GUIContent Smooth = DeformEditorGUIUtility.DefaultContent.Smooth;
			public static readonly GUIContent Inner = new GUIContent (text: "Inner", tooltip: "Vertices closer to the axis than the inner radius won't be twirled when the mode is limited.");
			public static readonly GUIContent Outer = new GUIContent (text: "Outer", tooltip: "Vertices further from the axis than the outer radius won't be twirled when the mode is limited.");
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private class Properties
		{
			public SerializedProperty Angle;
			public SerializedProperty Factor;
			public SerializedProperty Mode;
			public SerializedProperty Smooth;
			public SerializedProperty Inner;
			public SerializedProperty Outer; 
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
			{
				Angle	= obj.FindProperty ("angle");
				Factor	= obj.FindProperty ("factor");
				Mode	= obj.FindProperty ("mode");
				Smooth	= obj.FindProperty ("smooth");
				Inner	= obj.FindProperty ("inner");
				Outer	= obj.FindProperty ("outer");
				Axis	= obj.FindProperty ("axis");
			}
		}

		private Properties properties;

		private readonly ArcHandle angleHandle = new ArcHandle ();
		private readonly VerticalBoundsHandle boundsHandle = new VerticalBoundsHandle ();

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

			EditorGUILayout.PropertyField (properties.Angle, Content.Angle);
			EditorGUILayout.PropertyField (properties.Factor, Content.Factor);
			EditorGUILayout.PropertyField (properties.Mode, Content.Mode);

			using (new EditorGUI.IndentLevelScope ())
			{
				using (new EditorGUI.DisabledScope ((BoundsMode)properties.Mode.enumValueIndex == BoundsMode.Unlimited && !properties.Mode.hasMultipleDifferentValues))
				{
					EditorGUILayoutx.MaxField (properties.Inner, properties.Outer.floatValue, Content.Inner);
					EditorGUILayoutx.MinField (properties.Outer, properties.Inner.floatValue, Content.Outer);

					EditorGUILayout.PropertyField (properties.Smooth, Content.Smooth);
				}
			}

			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		public override void OnSceneGUI ()
		{
			base.OnSceneGUI ();

			var twirl = target as TwirlDeformer;

			DrawAngleHandle (twirl);

			if (twirl.Mode == BoundsMode.Limited)
			{
				boundsHandle.HandleColor = DeformEditorSettings.SolidHandleColor;
				boundsHandle.ScreenspaceHandleSize = DeformEditorSettings.ScreenspaceSliderHandleCapSize;
				if (boundsHandle.DrawHandle (twirl.Outer, twirl.Inner, twirl.Axis, Vector3.right))
				{
					Undo.RecordObject (twirl, "Changed Bounds");
					twirl.Outer = boundsHandle.Top;
					twirl.Inner = boundsHandle.Bottom;
				}
			}

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		void DrawAngleHandle (TwirlDeformer twirl)
		{
			var radiusDistanceOffset = HandleUtility.GetHandleSize (twirl.Axis.position + twirl.Axis.right * twirl.Outer) * DeformEditorSettings.ScreenspaceSliderHandleCapSize * 2f;
			angleHandle.angle = twirl.Angle;
			angleHandle.radius = twirl.Outer + radiusDistanceOffset;
			angleHandle.fillColor = Color.clear;
			angleHandle.angleHandleColor = DeformEditorSettings.SolidHandleColor;
			angleHandle.radiusHandleColor = Color.clear;

			var matrix = Matrix4x4.TRS (twirl.transform.position, twirl.transform.rotation, twirl.transform.lossyScale);

			using (new Handles.DrawingScope (matrix))
			{
				DeformHandles.Circle (Vector3.zero, Vector3.forward, Vector3.right, angleHandle.radius);

				Handles.color = DeformEditorSettings.SolidHandleColor;

				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					using (new Handles.DrawingScope (matrix * Matrix4x4.Rotate (Quaternion.LookRotation (Vector3.right, Vector3.forward))))
					{
						angleHandle.DrawHandle ();
						if (check.changed)
						{
							Undo.RecordObject (twirl, "Changed Angle");
							twirl.Angle = angleHandle.angle;
						}
					}
				}
			}
		}
	}
}