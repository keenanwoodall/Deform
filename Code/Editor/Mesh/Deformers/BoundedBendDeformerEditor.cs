using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Beans.Unity.Editor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (BoundedBendDeformer)), CanEditMultipleObjects]
	public class BoundedBendDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Angle = new GUIContent (text: "Angle", tooltip: "How many degrees the mesh should be bent by the time it reaches the top bounds.");
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent Mode = new GUIContent (text: "Mode", tooltip: "Unlimited: Entire mesh is bent.\nLimited: Mesh is only bent between bounds.");
			public static readonly GUIContent Bounds = new GUIContent (text: "Bounds", tooltip: "Any vertices outside this will be fully unbent.");
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private class Properties
		{
			public SerializedProperty Angle;
			public SerializedProperty Factor;
			public SerializedProperty Mode;
			public SerializedProperty Bounds;
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
			{
				Angle	= obj.FindProperty ("angle");
				Factor	= obj.FindProperty ("factor");
				Mode	= obj.FindProperty ("mode");
				Bounds	= obj.FindProperty ("bounds");
				Axis	= obj.FindProperty ("axis");
			}
		}

		private Properties properties;

		private ArcHandle angleHandle = new ArcHandle ();
		//private readonly VerticalBoundsHandle boundsHandle = new VerticalBoundsHandle ();
		private BoxBoundsHandle boxHandle = new BoxBoundsHandle();

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
				EditorGUILayout.PropertyField(properties.Bounds, Content.Bounds);
			}

			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			serializedObject.ApplyModifiedProperties ();

			EditorGUILayoutx.WIPAlert();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		public override void OnSceneGUI ()
		{
			base.OnSceneGUI ();

			var bend = target as BoundedBendDeformer;

			DrawAngleHandle (bend);

			boxHandle.handleColor = DeformEditorSettings.SolidHandleColor;
			boxHandle.wireframeColor = DeformEditorSettings.LightHandleColor;
			boxHandle.center = bend.Bounds.center;
			boxHandle.size = bend.Bounds.size;

			using (new Handles.DrawingScope(Matrix4x4.TRS(bend.Axis.position, bend.Axis.rotation, bend.Axis.lossyScale)))
			{
				using (var check = new EditorGUI.ChangeCheckScope())
				{
					boxHandle.DrawHandle();
					if (check.changed)
					{
						Undo.RecordObject(bend, "Changed Bounds");
						bend.Bounds = new Bounds(boxHandle.center, boxHandle.size);
						SceneView.RepaintAll();
					}
				}
			}

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawAngleHandle (BoundedBendDeformer bend)
        {

			var handleRotation = bend.Axis.rotation * Quaternion.Euler (-90, 0f, 0f);
			// There's some weird issue where if you pass the normal lossyScale, the handle's scale on the y axis is changed when the transform's z axis is changed.
			// My simple solution is to swap the y and z.
			var handleScale = new Vector3
			(
				x: bend.Axis.lossyScale.x,
				y: bend.Axis.lossyScale.z,
				z: bend.Axis.lossyScale.y
			);

			var matrix = Matrix4x4.TRS (bend.Axis.position + bend.Axis.up * bend.Bounds.min.y * bend.Axis.lossyScale.y, handleRotation, handleScale);

			var radiusDistanceOffset = HandleUtility.GetHandleSize (bend.Axis.position + bend.Axis.up * bend.Bounds.max.y) * DeformEditorSettings.ScreenspaceSliderHandleCapSize * 2f;

			angleHandle.angle = bend.Angle;
			angleHandle.radius = (bend.Bounds.max.y - bend.Bounds.min.y) + radiusDistanceOffset;
			angleHandle.fillColor = Color.clear;

			using (new Handles.DrawingScope (DeformEditorSettings.SolidHandleColor, matrix))
			{
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