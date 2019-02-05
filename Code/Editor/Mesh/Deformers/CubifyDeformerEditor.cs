using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (CubifyDeformer)), CanEditMultipleObjects]
	public class CubifyDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				Factor, 
				Width, 
				Height, 
				Length, 
				Axis;

			public void Update ()
			{
				Factor = DeformEditorGUIUtility.DefaultContent.Factor;
				Width = new GUIContent
				(
					text: "Width",
					tooltip: "The width of the cube."
				);
				Height = new GUIContent
				(
					text: "Height",
					tooltip: "The height of the cube."
				);
				Length = new GUIContent
				(
					text: "Length",
					tooltip: "The length of the cube."
				);
				Axis = DeformEditorGUIUtility.DefaultContent.Axis;
			}
		}

		private class Properties
		{
			public SerializedProperty 
				Factor, 
				Width, 
				Height, 
				Length, 
				Axis;

			public void Update (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
				Width	= obj.FindProperty ("width");
				Height	= obj.FindProperty ("height");
				Length	= obj.FindProperty ("length");
				Axis	= obj.FindProperty ("axis");
			}
		}

		private Content content = new Content ();
		private Properties properties = new Properties ();

		private BoxBoundsHandle boundsHandle = new BoxBoundsHandle ();

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
			EditorGUILayout.PropertyField (properties.Width, content.Width);
			EditorGUILayout.PropertyField (properties.Height, content.Height);
			EditorGUILayout.PropertyField (properties.Length, content.Length);
			EditorGUILayout.PropertyField (properties.Axis, content.Axis);
			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void OnSceneGUI ()
		{
			if (target == null)
				return;

			var cubify = target as CubifyDeformer;

			DrawBoundsHandle (cubify);

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawBoundsHandle (CubifyDeformer cubify)
		{
			boundsHandle.handleColor = DeformEditorSettings.SolidHandleColor;
			boundsHandle.wireframeColor = DeformEditorSettings.LightHandleColor;
			boundsHandle.center = Vector3.zero;
			boundsHandle.size = new Vector3 (cubify.Width, cubify.Height, cubify.Length);

			using (new Handles.DrawingScope (Matrix4x4.TRS (cubify.Axis.position, cubify.Axis.rotation, cubify.Axis.localScale)))
			{
				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					boundsHandle.DrawHandle ();
					if (check.changed)
					{
						Undo.RecordObject (cubify, "Changed Size");
						cubify.Width = boundsHandle.size.x;
						cubify.Height = boundsHandle.size.y;
						cubify.Length = boundsHandle.size.z;
					}
				}
			}
		}
	}
}