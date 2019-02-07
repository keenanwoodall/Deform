using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (TwistDeformer)), CanEditMultipleObjects]
	public class TwistDeformerEditor : DeformerEditor
	{
		private const float ANGLE_HANDLE_RADIUS = 1.25f;

		private class Content
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

		private ArcHandle startAngleHandle = new ArcHandle ();
		private ArcHandle endAngleHandle = new ArcHandle ();

		protected override void OnEnable ()
		{
			base.OnEnable ();
			properties = new Properties (serializedObject);
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
				DeformEditorGUILayout.MinField (properties.Top, properties.Bottom.floatValue, Content.Top);
				DeformEditorGUILayout.MaxField (properties.Bottom, properties.Top.floatValue, Content.Bottom);

				using (new EditorGUI.DisabledScope (properties.Mode.enumValueIndex == 0 && !properties.Mode.hasChildren))
					EditorGUILayout.PropertyField (properties.Smooth, Content.Smooth);
			}

			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);
			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void OnSceneGUI ()
		{
			base.OnSceneGUI ();

			var twist = target as TwistDeformer;

			DrawBoundsHandles (twist);
			DrawAngleHandles (twist);

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawBoundsHandles (TwistDeformer twist)
		{
			using (new Handles.DrawingScope (Handles.matrix))
			{
				Handles.color = DeformEditorSettings.SolidHandleColor;

				var direction = twist.Axis.forward;
				var topWorldPosition = twist.transform.position + direction * twist.Top;
				var botWorldPosition = twist.transform.position + direction * twist.Bottom;

				DeformHandles.Line (topWorldPosition, botWorldPosition, DeformHandles.LineMode.LightDotted);

				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var newTopWorld = DeformHandles.Slider (topWorldPosition, direction);
					if (check.changed)
					{
						Undo.RecordObject (twist, "Changed Top");
						var newTop = DeformHandlesUtility.DistanceAlongAxis (twist.Axis, twist.Axis.position, newTopWorld, Axis.Z);
						twist.Top = newTop;
					}
				}

				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var newBotWorld = DeformHandles.Slider (botWorldPosition, -direction);
					if (check.changed)
					{
						Undo.RecordObject (twist, "Changed Bottom");
						var newBot = DeformHandlesUtility.DistanceAlongAxis (twist.Axis, twist.Axis.position, newBotWorld, Axis.Z);
						twist.Bottom = newBot;
					}
				}
			}
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
			var bottomMatrix = Matrix4x4.TRS (twist.transform.position + (twist.Axis.forward * twist.Bottom), Quaternion.LookRotation (direction, normal), Vector3.one);
			var topMatrix = Matrix4x4.TRS (twist.transform.position + (twist.Axis.forward * twist.Top), Quaternion.LookRotation (direction, normal), Vector3.one);

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