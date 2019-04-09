using UnityEngine;
using UnityEditor;
using Deform.Masking;
using UnityEditor.IMGUI.Controls;

namespace DeformEditor.Masking
{
	[CustomEditor (typeof (BoxMask)), CanEditMultipleObjects]
	public class BoxMaskEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent InnerBounds = new GUIContent (text: "Inner Bounds");
			public static readonly GUIContent OuterBounds = new GUIContent (text: "Outer Bounds");
			public static readonly GUIContent Invert = new GUIContent (text: "Invert");
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private class Properties
		{
			public SerializedProperty Factor;
			public SerializedProperty InnerBounds;
			public SerializedProperty OuterBounds;
			public SerializedProperty Invert;
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
			{
				Factor		= obj.FindProperty ("factor");
				InnerBounds = obj.FindProperty ("innerBounds");
				OuterBounds = obj.FindProperty ("outerBounds");
				Invert		= obj.FindProperty ("invert");
				Axis		= obj.FindProperty ("axis");
			}
		}

		private Properties properties;

		private BoxBoundsHandle boxHandle = new BoxBoundsHandle ();

		protected override void OnEnable ()
		{
			base.OnEnable ();
			properties = new Properties (serializedObject);
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.UpdateIfRequiredOrScript ();

			EditorGUILayout.Slider (properties.Factor, 0f, 1f, Content.Factor);
			EditorGUILayout.PropertyField (properties.InnerBounds, Content.InnerBounds);
			EditorGUILayout.PropertyField (properties.OuterBounds, Content.OuterBounds);
			EditorGUILayout.PropertyField (properties.Invert, Content.Invert);
			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		public override void OnSceneGUI ()
		{
			base.OnSceneGUI ();

			var boxMask = target as BoxMask;

			DrawInnerBoundsHandle (boxMask);
			DrawOuterBoundsHandle (boxMask);

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawInnerBoundsHandle (BoxMask boxMask)
		{
			boxHandle.handleColor = DeformEditorSettings.SolidHandleColor;
			boxHandle.wireframeColor = DeformEditorSettings.LightHandleColor;
			boxHandle.center = boxMask.InnerBounds.center;
			boxHandle.size = boxMask.InnerBounds.size;

			using (new Handles.DrawingScope (Matrix4x4.TRS (boxMask.Axis.position, boxMask.Axis.rotation, boxMask.Axis.lossyScale)))
			{
				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					boxHandle.DrawHandle ();
					if (check.changed)
					{
						Undo.RecordObject (boxMask, "Changed Size");
						boxMask.InnerBounds = new Bounds (boxHandle.center, boxHandle.size);
						boxMask.OuterBounds.Encapsulate (boxMask.InnerBounds);
					}
				}
			}
		}
		private void DrawOuterBoundsHandle (BoxMask boxMask)
		{
			boxHandle.handleColor = DeformEditorSettings.SolidHandleColor;
			boxHandle.wireframeColor = DeformEditorSettings.LightHandleColor;
			boxHandle.center = boxMask.OuterBounds.center;
			boxHandle.size = boxMask.OuterBounds.size;

			using (new Handles.DrawingScope (Matrix4x4.TRS (boxMask.Axis.position, boxMask.Axis.rotation, boxMask.Axis.lossyScale)))
			{
				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					boxHandle.DrawHandle ();
					if (check.changed)
					{
						Undo.RecordObject (boxMask, "Changed Size");
						boxMask.OuterBounds = new Bounds (boxHandle.center, boxHandle.size);
					}
				}
			}
		}
	}
}