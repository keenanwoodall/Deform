using UnityEngine;
using UnityEditor;
using Beans.Unity.Editor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (TaperDeformer)), CanEditMultipleObjects]
	public class TaperDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Top = new GUIContent (text: "Top", tooltip: "Vertices above this will be fully scaled by the top factor.");
			public static readonly GUIContent Bottom = new GUIContent (text: "Bottom", tooltip: "Vertices below this will be fully scaled by the bottom factor.");
			public static readonly GUIContent Curvature = new GUIContent (text: "Curvature", tooltip: "The bulge strength. Positive values make it bulge in, negative makes it bulge out.");
			public static readonly GUIContent Smooth = DeformEditorGUIUtility.DefaultContent.Smooth;
			public static readonly GUIContent TopFactor = new GUIContent (text: "Top Factor", tooltip: "The scale of the mesh at the top bounds.");
			public static readonly GUIContent BottomFactor = new GUIContent (text: "Bottom Factor", tooltip: "The scale of the mesh at the bottom bounds.");
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private struct Properties
		{
			public SerializedProperty Top;
			public SerializedProperty Bottom;
			public SerializedProperty Curvature;
			public SerializedProperty Smooth;
			public SerializedProperty TopFactor;
			public SerializedProperty BottomFactor;
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
			{
				Top				= obj.FindProperty ("top");
				Bottom			= obj.FindProperty ("bottom");
				Curvature		= obj.FindProperty ("curvature");
				Smooth			= obj.FindProperty ("smooth");
				TopFactor		= obj.FindProperty ("topFactor");
				BottomFactor	= obj.FindProperty ("bottomFactor");
				Axis			= obj.FindProperty ("axis");
			}
		}

		private Properties properties;
		private readonly VerticalBoundsHandle boundsHandle = new VerticalBoundsHandle ();

		protected override void OnEnable ()
		{
			base.OnEnable ();

			properties = new Properties (serializedObject);

			boundsHandle.HandleCapFunction = DeformHandles.HandleCapFunction;
			boundsHandle.DrawGuidelineCallback = (a, b) => DeformHandles.Line (a, b, DeformHandles.LineMode.LightDotted);
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.UpdateIfRequiredOrScript ();

			EditorGUILayoutx.MinField (properties.Top, properties.Bottom.floatValue, Content.Top);
			EditorGUILayoutx.MaxField (properties.Bottom, properties.Top.floatValue, Content.Bottom);
			EditorGUILayout.PropertyField (properties.Curvature, Content.Curvature);
			EditorGUILayout.PropertyField (properties.TopFactor, Content.TopFactor);
			EditorGUILayout.PropertyField (properties.BottomFactor, Content.BottomFactor);
			EditorGUILayout.PropertyField (properties.Smooth, Content.Smooth);
			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		public override void OnSceneGUI ()
		{
			base.OnSceneGUI ();

			var taper = target as TaperDeformer;

			boundsHandle.HandleColor = DeformEditorSettings.SolidHandleColor;
			boundsHandle.ScreenspaceHandleSize = DeformEditorSettings.ScreenspaceSliderHandleCapSize;
			if (boundsHandle.DrawHandle (taper.Top, taper.Bottom, taper.Axis, Vector3.forward))
			{
				Undo.RecordObject (taper, "Changed Bounds");
				taper.Top = boundsHandle.Top;
				taper.Bottom = boundsHandle.Bottom;
			}

			DrawTopFactorHandles (taper);
			DrawBottomFactorHandles (taper);

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawTopFactorHandles (TaperDeformer taper)
		{
			var topPosition = Vector3.forward * taper.Top;

			var topFactorXPosition = topPosition + Vector3.right * taper.TopFactor.x * 0.5f;
			var topFactorYPosition = topPosition + Vector3.up * taper.TopFactor.y * 0.5f;

			using (new Handles.DrawingScope (Matrix4x4.TRS (taper.Axis.position, taper.Axis.rotation, taper.Axis.lossyScale)))
			{
				DeformHandles.Line (topPosition, topFactorXPosition, DeformHandles.LineMode.LightDotted);
				DeformHandles.Line (topPosition, topFactorYPosition, DeformHandles.LineMode.LightDotted);

				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var newTopFactorXWorldPosition = DeformHandles.Slider (topFactorXPosition, Vector3.right);
					if (check.changed)
					{
						Undo.RecordObject (taper, "Changed Top Factor");
						taper.TopFactor = new Vector2 (newTopFactorXWorldPosition.x * 2f, taper.TopFactor.y);
					}
				}

				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var newTopFactorYWorldPosition = DeformHandles.Slider (topFactorYPosition, Vector3.up);
					if (check.changed)
					{
						Undo.RecordObject (taper, "Changed Top Factor");
						taper.TopFactor = new Vector2 (taper.TopFactor.x, newTopFactorYWorldPosition.y * 2f);
					}
				}
			}
		}

		private void DrawBottomFactorHandles (TaperDeformer taper)
		{
			var bottomPosition = Vector3.forward * taper.Bottom;

			var bottomFactorXPosition = bottomPosition + Vector3.right * taper.BottomFactor.x * 0.5f;
			var bottomFactorYPosition = bottomPosition + Vector3.up * taper.BottomFactor.y * 0.5f;

			using (new Handles.DrawingScope (Matrix4x4.TRS (taper.Axis.position, taper.Axis.rotation, taper.Axis.lossyScale)))
			{
				DeformHandles.Line (bottomPosition, bottomFactorXPosition, DeformHandles.LineMode.LightDotted);
				DeformHandles.Line (bottomPosition, bottomFactorYPosition, DeformHandles.LineMode.LightDotted);

				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var newBottomFactorXWorldPosition = DeformHandles.Slider (bottomFactorXPosition, Vector3.right);
					if (check.changed)
					{
						Undo.RecordObject (taper, "Changed Bottom Factor");
						taper.BottomFactor = new Vector2 (newBottomFactorXWorldPosition.x * 2f, taper.BottomFactor.y);
					}
				}

				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var newBottomFactorYWorldPosition = DeformHandles.Slider (bottomFactorYPosition, Vector3.up);
					if (check.changed)
					{
						Undo.RecordObject (taper, "Changed Bottom Factor");
						taper.BottomFactor = new Vector2 (taper.BottomFactor.x, newBottomFactorYWorldPosition.y * 2f);
					}
				}
			}
		}
	}
}