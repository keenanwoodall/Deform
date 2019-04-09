using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Deform;
using Beans.Unity.Editor;

namespace DeformEditor
{
	[CustomEditor (typeof (CubifyDeformer)), CanEditMultipleObjects]
	public class CubifyDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent Width = new GUIContent (text: "Width", tooltip: "The width of the cube.");
			public static readonly GUIContent Height = new GUIContent (text: "Height", tooltip: "The height of the cube.");
			public static readonly GUIContent Length = new GUIContent (text: "Length", tooltip: "The length of the cube.");
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private class Properties
		{
			public SerializedProperty Factor;
			public SerializedProperty Width;
			public SerializedProperty Height;
			public SerializedProperty Length;
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
				Width	= obj.FindProperty ("width");
				Height	= obj.FindProperty ("height");
				Length	= obj.FindProperty ("length");
				Axis	= obj.FindProperty ("axis");
			}
		}

		private Properties properties;

		private BoxBoundsHandle boundsHandle = new BoxBoundsHandle ();

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
			EditorGUILayout.PropertyField (properties.Width, Content.Width);
			EditorGUILayout.PropertyField (properties.Height, Content.Height);
			EditorGUILayout.PropertyField (properties.Length, Content.Length);
			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			serializedObject.ApplyModifiedProperties ();

			EditorGUILayoutx.WIPAlert ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		public override void OnSceneGUI ()
		{
			base.OnSceneGUI ();

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

			using (new Handles.DrawingScope (Matrix4x4.TRS (cubify.Axis.position, cubify.Axis.rotation, cubify.Axis.lossyScale)))
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