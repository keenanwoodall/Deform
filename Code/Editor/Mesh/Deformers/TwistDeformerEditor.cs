using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Beans.Unity.Editor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (TwistDeformer)), CanEditMultipleObjects]
	public class TwistDeformerEditor : DeformerEditor
	{
		private const float ANGLE_HANDLE_RADIUS = 1.25f;

		private static class Content
		{
			public static readonly GUIContent StartAngle = new GUIContent (text: "Start Angle", tooltip: "When mode is limited this is how many degrees the vertices are twisted at the bottom bounds. When unlimited the vertices are twisted based on the different between the start and end angle.");
			public static readonly GUIContent EndAngle = new GUIContent (text: "End Angle", tooltip: "When mode is limited this is how many degrees the vertices are twisted at the top bounds. When unlimited the vertices are twisted based on the different between the start and end angle.");
			public static readonly GUIContent Offset = new GUIContent (text: "Offset", tooltip: "The base angle offset applied to the twist.");
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent Mode = new GUIContent (text: "Mode", tooltip: "Unlimited: Entire mesh is twisted based on the difference between the start and end angle.\nLimited: Vertices are only twisted within the bounds.");
			public static readonly GUIContent Smooth = DeformEditorGUIUtility.DefaultContent.Smooth;
			public static readonly GUIContent Top = new GUIContent (text: "Top", tooltip: "Vertices above this will be untwisted when the mode is limited.");
			public static readonly GUIContent Bottom = new GUIContent (text: "Bottom", tooltip: "Vertices below this will be untwisted when the mode is limited.");
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private class Properties
		{
			public SerializedProperty StartAngle;
			public SerializedProperty EndAngle;
			public SerializedProperty Offset;
			public SerializedProperty Factor;
			public SerializedProperty Mode;
			public SerializedProperty Smooth;
			public SerializedProperty Top;
			public SerializedProperty Bottom;
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
			{
				StartAngle	= obj.FindProperty ("startAngle");
				EndAngle	= obj.FindProperty ("endAngle");
				Offset		= obj.FindProperty ("offset");
				Factor		= obj.FindProperty ("factor");
				Mode		= obj.FindProperty ("mode");
				Smooth		= obj.FindProperty ("smooth");
				Top			= obj.FindProperty ("top");
				Bottom		= obj.FindProperty ("bottom");
				Axis		= obj.FindProperty ("axis");
			}
		}

		private Properties properties;

		private readonly ArcHandle startAngleHandle = new ArcHandle ();
		private readonly ArcHandle endAngleHandle = new ArcHandle ();
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

			EditorGUILayout.PropertyField (properties.StartAngle, Content.StartAngle);
			EditorGUILayout.PropertyField (properties.EndAngle, Content.EndAngle);
			EditorGUILayout.PropertyField (properties.Offset, Content.Offset);
			EditorGUILayout.PropertyField (properties.Factor, Content.Factor);
			EditorGUILayout.PropertyField (properties.Mode, Content.Mode);

			using (new EditorGUI.IndentLevelScope ())
			{
				EditorGUILayoutx.MinField (properties.Top, properties.Bottom.floatValue, Content.Top);
				EditorGUILayoutx.MaxField (properties.Bottom, properties.Top.floatValue, Content.Bottom);

				using (new EditorGUI.DisabledScope ((BoundsMode)properties.Mode.enumValueIndex == BoundsMode.Unlimited && !properties.Mode.hasMultipleDifferentValues))
					EditorGUILayout.PropertyField (properties.Smooth, Content.Smooth);
			}

			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);
			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		public override void OnSceneGUI ()
		{
			base.OnSceneGUI ();

			var twist = target as TwistDeformer;

			boundsHandle.HandleColor = DeformEditorSettings.SolidHandleColor;
			boundsHandle.ScreenspaceHandleSize = DeformEditorSettings.ScreenspaceSliderHandleCapSize;
			if (boundsHandle.DrawHandle (twist.Top, twist.Bottom, twist.Axis, Vector3.forward))
			{
				Undo.RecordObject (twist, "Changed Bounds");
				twist.Top = boundsHandle.Top;
				twist.Bottom = boundsHandle.Bottom;
			}

			DrawAngleHandles (twist);

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawAngleHandles (TwistDeformer twist)
		{
			startAngleHandle.angle = twist.StartAngle;
			startAngleHandle.radius = HandleUtility.GetHandleSize (twist.transform.position) * DeformEditorSettings.ScreenspaceAngleHandleSize;
			startAngleHandle.fillColor = Color.clear;
			endAngleHandle.angle = twist.EndAngle;
			endAngleHandle.radius = HandleUtility.GetHandleSize (twist.transform.position) * DeformEditorSettings.ScreenspaceAngleHandleSize;
			endAngleHandle.fillColor = Color.clear;

			var normal = -twist.Axis.forward;
			var direction = twist.Axis.right;
			var bottomMatrix = Matrix4x4.TRS (twist.transform.position + (twist.Axis.forward * twist.Bottom * twist.Axis.lossyScale.z), Quaternion.LookRotation (direction, normal), Vector3.one);
			var topMatrix = Matrix4x4.TRS (twist.transform.position + (twist.Axis.forward * twist.Top * twist.Axis.lossyScale.z), Quaternion.LookRotation (direction, normal), Vector3.one);

			using (new Handles.DrawingScope (Handles.matrix))
			{
				Handles.color = DeformEditorSettings.SolidHandleColor;

				Handles.matrix = bottomMatrix;
				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					startAngleHandle.DrawHandle ();
					if (check.changed)
					{
						Undo.RecordObject (twist, "Changed Start Angle");
						twist.StartAngle = startAngleHandle.angle;
					}
				}

				Handles.matrix = topMatrix;
				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					endAngleHandle.DrawHandle ();
					if (check.changed)
					{
						Undo.RecordObject (twist, "Changed End Angle");
						twist.EndAngle = endAngleHandle.angle;
					}
				}
			}
		}
	}
}