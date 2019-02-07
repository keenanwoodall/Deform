using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (BulgeDeformer)), CanEditMultipleObjects]
	public class BulgeDeformerEditor : DeformerEditor
	{
		private class Content
		{
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent Top = DeformEditorGUIUtility.DefaultContent.Top;
			public static readonly GUIContent Bottom = DeformEditorGUIUtility.DefaultContent.Bottom;
			public static readonly GUIContent Smooth = DeformEditorGUIUtility.DefaultContent.Smooth;
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private class Properties
		{
			public SerializedProperty Factor;
			public SerializedProperty Top;
			public SerializedProperty Bottom;
			public SerializedProperty Smooth;
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
				Top		= obj.FindProperty ("top");
				Bottom	= obj.FindProperty ("bottom");
				Smooth	= obj.FindProperty ("smooth");
				Axis	= obj.FindProperty ("axis");
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

			EditorGUILayout.PropertyField (properties.Factor, Content.Factor);
			DeformEditorGUILayout.MinField (properties.Top, properties.Bottom.floatValue, Content.Top);
			DeformEditorGUILayout.MaxField (properties.Bottom, properties.Top.floatValue, Content.Bottom);
			EditorGUILayout.PropertyField (properties.Smooth, Content.Smooth);
			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		public override void OnSceneGUI ()
		{
			base.OnSceneGUI ();

			if (target == null)
				return;

			var bulge = target as BulgeDeformer;

			DrawFactorHandle (bulge);
			DrawBoundsHandles (bulge);

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawFactorHandle (BulgeDeformer bulge)
		{
			var direction = bulge.Axis.up;

			var worldPosition = bulge.Axis.position + direction * ((bulge.Factor + 1f) * 0.5f);

			DeformHandles.Line (bulge.Axis.position, worldPosition, DeformHandles.LineMode.LightDotted);

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newWorldPosition = DeformHandles.Slider (worldPosition, direction);
				if (check.changed)
				{
					var newFactor = DeformHandlesUtility.DistanceAlongAxis (bulge.Axis, bulge.Axis.position, newWorldPosition, Axis.Y) * 2f - 1f;
					Undo.RecordObject (bulge, "Changed Factor");
					bulge.Factor = newFactor;
				}
			}
		}

		private void DrawBoundsHandles (BulgeDeformer bulge)
		{
			var direction = bulge.Axis.forward;

			var topWorldPosition = bulge.Axis.position + direction * bulge.Top;
			var botWorldPosition = bulge.Axis.position + direction * bulge.Bottom;

			DeformHandles.Line (topWorldPosition, botWorldPosition, DeformHandles.LineMode.LightDotted);

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newTopWorld = DeformHandles.Slider (topWorldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (bulge, "Changed Top");
					var newTop = DeformHandlesUtility.DistanceAlongAxis (bulge.Axis, bulge.Axis.position, newTopWorld, Axis.Z);
					bulge.Top = newTop;
				}
			}

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newBotWorld = DeformHandles.Slider (botWorldPosition, -direction);
				if (check.changed)
				{
					Undo.RecordObject (bulge, "Changed Bottom");
					var newBot = DeformHandlesUtility.DistanceAlongAxis (bulge.Axis, bulge.Axis.position, newBotWorld, Axis.Z);
					bulge.Bottom = newBot;
				}
			}
		}
	}
}