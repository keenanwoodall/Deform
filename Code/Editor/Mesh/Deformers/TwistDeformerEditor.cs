using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (TwistDeformer)), CanEditMultipleObjects]
	public class TwistDeformerEditor : Editor
	{
		private const float ANGLE_HANDLE_RADIUS = 1.25f;

		private class Content
		{
			public GUIContent 
				StartAngle, 
				EndAngle, 
				Offset,
				Factor, 
				Mode, 
				Smooth,
				Top,
				Bottom,
				Axis;

			public void Update ()
			{
				StartAngle = new GUIContent
				(
					text: "Start Angle",
					tooltip: "When mode is limited this is how many degrees the vertices are twisted at the bottom bounds. When unlimited the vertices are twisted based on the different between the start and end angle."
				);
				EndAngle = new GUIContent
				(
					text: "End Angle",
					tooltip: "When mode is limited this is how many degrees the vertices are twisted at the top bounds. When unlimited the vertices are twisted based on the different between the start and end angle."
				);
				Offset = new GUIContent
				(
					text: "Offset",
					tooltip: "The base angle offset applied to the twist."
				);
				Factor = DeformEditorGUIUtility.DefaultContent.Factor;
				Mode = new GUIContent
				(
					text: "Mode",
					tooltip: "Unlimited: Entire mesh is twisted based on the difference between the start and end angle.\nLimited: Vertices are only twisted within the bounds."
				);
				Smooth = DeformEditorGUIUtility.DefaultContent.Smooth;
				Top = new GUIContent
				(
					text: "Top",
					tooltip: "Vertices above this will be untwisted when the mode is limited."
				);
				Bottom = new GUIContent
				(
					text: "Bottom", tooltip: "Vertices below this will be untwisted when the mode is limited."
				);
				Axis = DeformEditorGUIUtility.DefaultContent.Axis;
			}
		}

		private class Properties
		{
			public SerializedProperty 
				StartAngle, 
				EndAngle,
				Offset, 
				Factor, 
				Mode,
				Smooth,
				Top,
				Bottom,
				Axis;

			public void Update (SerializedObject obj)
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

		private Content content = new Content ();
		private Properties properties = new Properties ();

		private ArcHandle startAngleHandle = new ArcHandle ();
		private ArcHandle endAngleHandle = new ArcHandle ();

		private void OnEnable ()
		{
			content.Update ();
			properties.Update (serializedObject);
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			EditorGUILayout.PropertyField (properties.StartAngle, content.StartAngle);
			EditorGUILayout.PropertyField (properties.EndAngle, content.EndAngle);
			EditorGUILayout.PropertyField (properties.Offset, content.Offset);
			EditorGUILayout.PropertyField (properties.Factor, content.Factor);
			EditorGUILayout.PropertyField (properties.Mode, content.Mode);

			using (new EditorGUI.IndentLevelScope ())
			{
				DeformEditorGUILayout.MinField (properties.Top, properties.Bottom.floatValue, content.Top);
				DeformEditorGUILayout.MaxField (properties.Bottom, properties.Top.floatValue, content.Bottom);

				using (new EditorGUI.DisabledScope (properties.Mode.enumValueIndex == 0 && !properties.Mode.hasChildren))
					EditorGUILayout.PropertyField (properties.Smooth, content.Smooth);
			}

			EditorGUILayout.PropertyField (properties.Axis, content.Axis);

			serializedObject.ApplyModifiedProperties ();
			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void OnSceneGUI ()
		{
			if (target == null)
				return;

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