using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (TaperDeformer)), CanEditMultipleObjects]
	public class TaperDeformerEditor : DeformerEditor
	{
		private class Content
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

		protected override void OnEnable ()
		{
			base.OnEnable ();
			properties = new Properties (serializedObject);
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.UpdateIfRequiredOrScript ();

			DeformEditorGUILayout.MinField (properties.Top, properties.Bottom.floatValue, Content.Top);
			DeformEditorGUILayout.MaxField (properties.Bottom, properties.Top.floatValue, Content.Bottom);
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

			DrawBoundsHandles (taper);
			DrawTopFactorHandles (taper);
			DrawBottomFactorHandles (taper);

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawBoundsHandles (TaperDeformer taper)
		{
			var direction = taper.Axis.forward;
			var topWorldPosition = taper.Axis.position + direction * taper.Top;
			var bottomWorldPosition = taper.Axis.position + direction * taper.Bottom;

			DeformHandles.Line (topWorldPosition, bottomWorldPosition, DeformHandles.LineMode.LightDotted);

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newTopWorldPosition = DeformHandles.Slider (topWorldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (taper, "Changed Top");
					var newTop = DeformHandlesUtility.DistanceAlongAxis (taper.Axis, taper.Axis.position, newTopWorldPosition, Axis.Z);
					taper.Top = newTop;
				}
			}

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newBottomWorldPosition = DeformHandles.Slider (bottomWorldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (taper, "Changed Bottom");
					var newBottom = DeformHandlesUtility.DistanceAlongAxis (taper.Axis, taper.Axis.position, newBottomWorldPosition, Axis.Z);
					taper.Bottom = newBottom;
				}
			}
		}

		private void DrawTopFactorHandles (TaperDeformer taper)
		{
			var directionX = taper.Axis.right;
			var directionY = taper.Axis.up;
			var directionZ = taper.Axis.forward;

			var topWorldPosition = taper.Axis.position + directionZ * taper.Top;

			var topFactorXWorldPosition = topWorldPosition + directionX * taper.TopFactor.x;
			var topFactorYWorldPosition = topWorldPosition + directionY * taper.TopFactor.y;

			DeformHandles.Line (topWorldPosition, topFactorXWorldPosition, DeformHandles.LineMode.LightDotted);
			DeformHandles.Line (topWorldPosition, topFactorYWorldPosition, DeformHandles.LineMode.LightDotted);

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newTopFactorXWorldPosition = DeformHandles.Slider (topFactorXWorldPosition, directionX);
				if (check.changed)
				{
					Undo.RecordObject (taper, "Changed Top Factor");
					var newTopFactorX = DeformHandlesUtility.DistanceAlongAxis (taper.Axis, taper.Axis.position, newTopFactorXWorldPosition, Axis.X);
					taper.TopFactor = new Vector2 (newTopFactorX, taper.TopFactor.y);
				}
			}

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newTopFactorYWorldPosition = DeformHandles.Slider (topFactorYWorldPosition, directionY);
				if (check.changed)
				{
					Undo.RecordObject (taper, "Changed Top Factor");
					var newTopFactorY = DeformHandlesUtility.DistanceAlongAxis (taper.Axis, taper.Axis.position, newTopFactorYWorldPosition, Axis.Y);
					taper.TopFactor = new Vector2 (taper.TopFactor.x, newTopFactorY);
				}
			}
		}

		private void DrawBottomFactorHandles (TaperDeformer taper)
		{
			var directionX = taper.Axis.right;
			var directionY = taper.Axis.up;
			var directionZ = taper.Axis.forward;

			var bottomWorldPosition = taper.Axis.position + directionZ * taper.Bottom;

			var bottomFactorXWorldPosition = bottomWorldPosition + directionX * taper.BottomFactor.x;
			var bottomFactorYWorldPosition = bottomWorldPosition + directionY * taper.BottomFactor.y;

			DeformHandles.Line (bottomWorldPosition, bottomFactorXWorldPosition, DeformHandles.LineMode.LightDotted);
			DeformHandles.Line (bottomWorldPosition, bottomFactorYWorldPosition, DeformHandles.LineMode.LightDotted);

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newBottomFactorXWorldPosition = DeformHandles.Slider (bottomFactorXWorldPosition, directionX);
				if (check.changed)
				{
					Undo.RecordObject (taper, "Changed Bottom Factor");
					var newBottomFactorX = DeformHandlesUtility.DistanceAlongAxis (taper.Axis, taper.Axis.position, newBottomFactorXWorldPosition, Axis.X);
					taper.BottomFactor = new Vector2 (newBottomFactorX, taper.BottomFactor.y);
				}
			}

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newBottomFactorYWorldPosition = DeformHandles.Slider (bottomFactorYWorldPosition, directionY);
				if (check.changed)
				{
					Undo.RecordObject (taper, "Changed Bottom Factor");
					var newBottomFactorY = DeformHandlesUtility.DistanceAlongAxis (taper.Axis, taper.Axis.position, newBottomFactorYWorldPosition, Axis.Y);
					taper.BottomFactor = new Vector2 (taper.BottomFactor.x, newBottomFactorY);
				}
			}
		}
	}
}