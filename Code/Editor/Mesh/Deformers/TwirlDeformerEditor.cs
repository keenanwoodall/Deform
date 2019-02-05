using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (TwirlDeformer)), CanEditMultipleObjects]
	public class TwirlDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				Angle, 
				Factor, 
				Mode, 
				Smooth,
				Inner,
				Outer, 
				Axis;

			public void Update ()
			{
				Angle = new GUIContent
				(
					text: "Angle",
					tooltip: "How many degrees each vertice will rotate around the axis based on distance between the inner and outer bounds."
				);
				Factor = DeformEditorGUIUtility.DefaultContent.Factor;
				Mode = new GUIContent
				(
					text: "Mode",
					tooltip: "Unlimited: The entire mesh will twirl.\nLimited: Only vertices between the bounds will twirl."
				);
				Smooth = DeformEditorGUIUtility.DefaultContent.Smooth;
				Inner = new GUIContent
				(
					text: "Inner",
					tooltip: "Vertices closer to the axis than the inner radius won't be twirled when the mode is limited."
				);
				Outer = new GUIContent
				(
					text: "Outer", tooltip: "Vertices further from the axis than the outer radius won't be twirled when the mode is limited."
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
				Smooth,
				Inner, 
				Outer, 
				Axis;

			public void Update (SerializedObject obj)
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

			serializedObject.UpdateIfRequiredOrScript ();
			EditorGUILayout.PropertyField (properties.Angle, content.Angle);
			EditorGUILayout.PropertyField (properties.Factor, content.Factor);
			EditorGUILayout.PropertyField (properties.Mode, content.Mode);

			using (new EditorGUI.IndentLevelScope ())
			{
				using (new EditorGUI.DisabledScope (properties.Mode.enumValueIndex == 0 && !properties.Mode.hasMultipleDifferentValues))
				{
					DeformEditorGUILayout.MaxField (properties.Inner, properties.Outer.floatValue, content.Inner);
					DeformEditorGUILayout.MinField (properties.Outer, properties.Inner.floatValue, content.Outer);

					EditorGUILayout.PropertyField (properties.Smooth, content.Smooth);
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

			var twirl = target as TwirlDeformer;
			
			if (twirl.Mode == BoundsMode.Limited)
				DrawBoundsHandles (twirl);
			DrawAngleHandle (twirl);

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawBoundsHandles (TwirlDeformer twirl)
		{
			using (new Handles.DrawingScope (Handles.matrix))
			{
				var direction = twirl.Axis.right;
				var innerWorldPosition = twirl.transform.position + direction * twirl.Inner;
				var outerWorldPosition = twirl.transform.position + direction * twirl.Outer;

				DeformHandles.Line (innerWorldPosition, outerWorldPosition, DeformHandles.LineMode.LightDotted);

				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var newInnerWorld = DeformHandles.Slider (innerWorldPosition, direction);
					if (check.changed)
					{
						Undo.RecordObject (twirl, "Changed Inner");
						var newInner = DeformHandlesUtility.DistanceAlongAxis (twirl.Axis, twirl.Axis.position, newInnerWorld, Axis.X);
						twirl.Inner = newInner;
					}
				}

				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var newOuterWorld = DeformHandles.Slider (outerWorldPosition, -direction);
					if (check.changed)
					{
						Undo.RecordObject (twirl, "Changed Outer");
						var newOuter = DeformHandlesUtility.DistanceAlongAxis (twirl.Axis, twirl.Axis.position, newOuterWorld, Axis.X);
						twirl.Outer = newOuter;
					}
				}
			}
		}

		void DrawAngleHandle (TwirlDeformer twirl)
		{
			angleHandle.angle = twirl.Angle;
			angleHandle.radius = twirl.Outer;
			angleHandle.fillColor = Color.clear;
			angleHandle.angleHandleColor = DeformEditorSettings.SolidHandleColor;
			angleHandle.radiusHandleColor = Color.clear;

			var normal = twirl.Axis.forward;
			var direction = twirl.Axis.right;
			var matrix = Matrix4x4.TRS (twirl.transform.position, Quaternion.LookRotation (direction, normal), Vector3.one);

			using (new Handles.DrawingScope (matrix))
			{
				DeformHandles.Circle (Vector3.zero, Vector3.up, Vector3.back, angleHandle.radius);

				Handles.color = DeformEditorSettings.SolidHandleColor;

				using (var check = new EditorGUI.ChangeCheckScope ())
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