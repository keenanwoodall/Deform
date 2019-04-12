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

		private readonly BoxBoundsHandle boxHandle = new BoxBoundsHandle ();

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
			var innerBounds = properties.InnerBounds.boundsValue;
			var outerBounds = properties.OuterBounds.boundsValue;

			innerBounds = EnforcePositiveExtents (innerBounds);

			innerBounds.min = new Vector3 (Mathf.Max (innerBounds.min.x, outerBounds.min.x), Mathf.Max (innerBounds.min.y, outerBounds.min.y), Mathf.Max (innerBounds.min.z, outerBounds.min.z));
			innerBounds.max = new Vector3 (Mathf.Min (innerBounds.max.x, outerBounds.max.x), Mathf.Min (innerBounds.max.y, outerBounds.max.y), Mathf.Min (innerBounds.max.z, outerBounds.max.z));

			EditorGUILayout.PropertyField (properties.OuterBounds, Content.OuterBounds);
			outerBounds = properties.OuterBounds.boundsValue;
			outerBounds.min = new Vector3 (Mathf.Min (innerBounds.min.x, outerBounds.min.x), Mathf.Min (innerBounds.min.y, outerBounds.min.y), Mathf.Min (innerBounds.min.z, outerBounds.min.z));
			outerBounds.max = new Vector3 (Mathf.Max (innerBounds.max.x, outerBounds.max.x), Mathf.Max (innerBounds.max.y, outerBounds.max.y), Mathf.Max (innerBounds.max.z, outerBounds.max.z));

			properties.InnerBounds.boundsValue = innerBounds;
			properties.OuterBounds.boundsValue = outerBounds;

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

			var innerBounds = boxMask.InnerBounds;
			var outerBounds = boxMask.OuterBounds;

			innerBounds = EnforcePositiveExtents (innerBounds);

			innerBounds.min = new Vector3 (Mathf.Max (innerBounds.min.x, outerBounds.min.x), Mathf.Max (innerBounds.min.y, outerBounds.min.y), Mathf.Max (innerBounds.min.z, outerBounds.min.z));
			innerBounds.max = new Vector3 (Mathf.Min (innerBounds.max.x, outerBounds.max.x), Mathf.Min (innerBounds.max.y, outerBounds.max.y), Mathf.Min (innerBounds.max.z, outerBounds.max.z));

			DrawOuterBoundsHandle (boxMask);

			outerBounds = boxMask.OuterBounds;

			outerBounds.min = new Vector3 (Mathf.Min (innerBounds.min.x, outerBounds.min.x), Mathf.Min (innerBounds.min.y, outerBounds.min.y), Mathf.Min (innerBounds.min.z, outerBounds.min.z));
			outerBounds.max = new Vector3 (Mathf.Max (innerBounds.max.x, outerBounds.max.x), Mathf.Max (innerBounds.max.y, outerBounds.max.y), Mathf.Max (innerBounds.max.z, outerBounds.max.z));

			boxMask.InnerBounds = innerBounds;
			boxMask.OuterBounds = outerBounds;

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

		private Bounds EnforcePositiveExtents (Bounds bounds)
		{
			var extents = bounds.extents;

			extents.x = Mathf.Max (0f, extents.x);
			extents.y = Mathf.Max (0f, extents.y);
			extents.z = Mathf.Max (0f, extents.z);

			bounds.extents = extents;

			return bounds;
		}
	}
}