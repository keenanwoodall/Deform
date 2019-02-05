using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (BulgeDeformer)), CanEditMultipleObjects]
	public class BulgeDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				Factor, 
				Top, 
				Bottom, 
				Smooth, 
				Axis;

			public void Update ()
			{
				Factor	= DeformEditorGUIUtility.DefaultContent.Factor;
				Top		= DeformEditorGUIUtility.DefaultContent.Top;
				Bottom	= DeformEditorGUIUtility.DefaultContent.Bottom;
				Smooth	= DeformEditorGUIUtility.DefaultContent.Smooth;
				Axis	= DeformEditorGUIUtility.DefaultContent.Axis;
			}
		}

		private class Properties
		{
			public SerializedProperty 
				Factor,
				Top, 
				Bottom, 
				Smooth,
				Axis;

			public void Update (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
				Top		= obj.FindProperty ("top");
				Bottom	= obj.FindProperty ("bottom");
				Smooth	= obj.FindProperty ("smooth");
				Axis	= obj.FindProperty ("axis");
			}
		}

		private Content content = new Content ();
		private Properties properties = new Properties ();

		private void OnEnable ()
		{
			content.Update ();
			properties.Update (serializedObject);
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.UpdateIfRequiredOrScript ();
			EditorGUILayout.PropertyField (properties.Factor, content.Factor);
			DeformEditorGUILayout.MinField (properties.Top, properties.Bottom.floatValue, content.Top);
			DeformEditorGUILayout.MaxField (properties.Bottom, properties.Top.floatValue, content.Bottom);
			EditorGUILayout.PropertyField (properties.Smooth, content.Smooth);
			EditorGUILayout.PropertyField (properties.Axis, content.Axis);
			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void OnSceneGUI ()
		{
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