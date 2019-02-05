using UnityEngine;
using UnityEditor;
using Deform.Masking;
using UnityEditor.IMGUI.Controls;

namespace DeformEditor.Masking
{
	[CustomEditor (typeof (BoxMask)), CanEditMultipleObjects]
	public class BoxMaskEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				Factor, 
				InnerBounds,
				OuterBounds, 
				Invert,
				Axis;

			public void Update ()
			{
				Factor = DeformEditorGUIUtility.DefaultContent.Axis;
				InnerBounds = new GUIContent
				(
					text: "Inner Bounds"
				);
				OuterBounds = new GUIContent
				(
					text: "Outer Bounds"
				);
				Invert = new GUIContent
				(
					text: "Invert"
				);
				Axis = DeformEditorGUIUtility.DefaultContent.Axis;
			}
		}

		private class Properties
		{
			public SerializedProperty 
				Factor, 
				InnerBounds,
				OuterBounds, 
				Invert, 
				Axis;

			public void Update (SerializedObject obj)
			{
				Factor		= obj.FindProperty ("factor");
				InnerBounds = obj.FindProperty ("innerBounds");
				OuterBounds = obj.FindProperty ("outerBounds");
				Invert		= obj.FindProperty ("invert");
				Axis		= obj.FindProperty ("axis");
			}
		}

		private Content content = new Content ();
		private Properties properties = new Properties ();

		private BoxBoundsHandle boxHandle = new BoxBoundsHandle ();

		private void OnEnable ()
		{
			content.Update ();
			properties.Update (serializedObject);
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.UpdateIfRequiredOrScript ();
			EditorGUILayout.Slider (properties.Factor, 0f, 1f, content.Factor);
			EditorGUILayout.PropertyField (properties.InnerBounds, content.InnerBounds);
			EditorGUILayout.PropertyField (properties.OuterBounds, content.OuterBounds);
			EditorGUILayout.PropertyField (properties.Invert, content.Invert);
			EditorGUILayout.PropertyField (properties.Axis, content.Axis);
			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void OnSceneGUI ()
		{
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

			using (new Handles.DrawingScope (Matrix4x4.TRS (boxMask.Axis.position, boxMask.Axis.rotation, boxMask.Axis.localScale)))
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

			using (new Handles.DrawingScope (Matrix4x4.TRS (boxMask.Axis.position, boxMask.Axis.rotation, boxMask.Axis.localScale)))
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