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
		private class Content
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

		private ArcHandle angleHandle = new ArcHandle ();

		protected override void OnEnable ()
		{
			base.OnEnable ();
			properties = new Properties (serializedObject);
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
				using (new EditorGUI.DisabledScope (properties.Mode.enumValueIndex == 0 && !properties.Mode.hasMultipleDifferentValues))
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