using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (BendDeformer)), CanEditMultipleObjects]
	public class BendDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				Angle, 
				Factor, 
				Mode, 
				Top, 
				Bottom, 
				Axis;

			public void Update ()
			{
				Angle = new GUIContent
				(
					text: "Angle",
					tooltip: "How many degrees each vertice should bend based on distance from the axis."
				);
				Factor = DeformEditorGUIUtility.DefaultContent.Factor;
				Mode = new GUIContent
				(
					text: "Mode",
					tooltip: "Unlimited: Entire mesh is bent.\nLimited: Mesh is only bent between bounds."
				);
				Top = new GUIContent
				(
					text: "Top",
					tooltip: "Any vertices above this point will be unbent."
				);
				Bottom = new GUIContent
				(
					text: "Bottom",
					tooltip: "Any vertices below this point will be unbent."
				);
				Axis = DeformEditorGUIUtility.DefaultContent.Axis;
			}
		}

		private class Properties
		{
			public SerializedProperty 
				Angle, 
				Factor, 
				Mode, 
				Top, 
				Bottom, 
				Axis;

			public void Update (SerializedObject obj)
			{
				Angle	= obj.FindProperty ("angle");
				Factor	= obj.FindProperty ("factor");
				Mode	= obj.FindProperty ("mode");
				Top		= obj.FindProperty ("top");
				Bottom	= obj.FindProperty ("bottom");
				Axis	= obj.FindProperty ("axis");
			}
		}

		private Content content = new Content ();
		private Properties properties = new Properties ();

		private ArcHandle angleHandle = new ArcHandle ();

		private void OnEnable ()
		{
			content.Update ();
			properties.Update (serializedObject);
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			EditorGUILayout.PropertyField (properties.Angle, content.Angle);
			EditorGUILayout.PropertyField (properties.Factor, content.Factor);
			EditorGUILayout.PropertyField (properties.Mode, content.Mode);

			using (new EditorGUI.DisabledGroupScope (!properties.Mode.hasMultipleDifferentValues && properties.Mode.enumValueIndex == 0))
			{
				using (new EditorGUI.IndentLevelScope ())
				{
					DeformEditorGUILayout.MinField (properties.Top, properties.Bottom.floatValue, content.Top);
					DeformEditorGUILayout.MaxField (properties.Bottom, properties.Top.floatValue, content.Bottom);
				}
			}

			EditorGUILayout.PropertyField (properties.Axis, content.Axis);

			serializedObject.ApplyModifiedProperties ();
			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void OnSceneGUI ()
		{
			if (target == null)
				return;
			var bend = target as BendDeformer;

			if (bend.Mode == BoundsMode.Limited)
				DrawBoundsHandles (bend);
			else
				DrawAxisGuide (bend);
			DrawAngleHandle (bend);

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawAxisGuide (BendDeformer bend)
		{
			var direction = bend.Axis.up;
			var topWorldPosition = bend.Axis.position + bend.Axis.up * HandleUtility.GetHandleSize (bend.Axis.position) * 2f;
			var bottomWorldPosition = bend.Axis.position + -bend.Axis.up * HandleUtility.GetHandleSize (bend.Axis.position) * 2f;

			DeformHandles.Line (bottomWorldPosition, topWorldPosition, DeformHandles.LineMode.LightDotted);
		}

		private void DrawBoundsHandles (BendDeformer bend)
		{
			var direction = bend.Axis.up;
			var topWorldPosition = bend.Axis.position + bend.Axis.up * bend.Top;
			var bottomWorldPosition = bend.Axis.position + bend.Axis.up * bend.Bottom;

			DeformHandles.Line (bottomWorldPosition, topWorldPosition, DeformHandles.LineMode.LightDotted);

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newTopWorldPosition = DeformHandles.Slider (topWorldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (bend, "Changed Top");
					var newTop = DeformHandlesUtility.DistanceAlongAxis (bend.Axis, bend.Axis.position, newTopWorldPosition, Axis.Y);
					bend.Top = newTop;
				}
			}

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newBottomWorldPosition = DeformHandles.Slider (bottomWorldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (bend, "Changed Bottom");
					var newBottom = DeformHandlesUtility.DistanceAlongAxis (bend.Axis, bend.Axis.position, newBottomWorldPosition, Axis.Y);
					bend.Bottom = newBottom;
				}
			}
		}

		private void DrawAngleHandle (BendDeformer bend)
        {
			angleHandle.angle = bend.Angle;
			angleHandle.radius = 1f;
			angleHandle.fillColor = Color.clear;

			var direction = -bend.Axis.right;
			var normal = -bend.Axis.forward;
			var matrix = Matrix4x4.TRS (bend.Axis.position, Quaternion.LookRotation (direction, normal), Vector3.one);

			using (new Handles.DrawingScope (Handles.matrix))
			{
            	Handles.color = DeformEditorSettings.SolidHandleColor;
				Handles.matrix = matrix;

				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					angleHandle.DrawHandle ();
					if (check.changed)
					{
						Undo.RecordObject (bend, "Changed Angle");
						bend.Angle = angleHandle.angle;
					}
				}
			}
        }
	}
}