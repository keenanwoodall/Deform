using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
    [CustomEditor (typeof (SquashAndStretchDeformer)), CanEditMultipleObjects]
    public class SquashAndStretchDeformerEditor : Editor
    {
		private class Content
		{
			public GUIContent 
				Factor, 
				Curvature,
				Top, 
				Bottom,
				Axis;

			public void Update ()
			{
				Factor = DeformEditorGUIUtility.DefaultContent.Factor;
				Curvature = new GUIContent
				(
					text: "Curvature",
					tooltip: "How much the mesh is bulged when squashed and squeezed when stretched."
				);
				Top = DeformEditorGUIUtility.DefaultContent.Top;
				Bottom = DeformEditorGUIUtility.DefaultContent.Bottom;
				Axis = DeformEditorGUIUtility.DefaultContent.Axis;
			}
		}

		private class Properties
		{
			public SerializedProperty 
				Factor,
				Curvature,
				Top, 
				Bottom,
				Axis;

			public void Update (SerializedObject obj)
			{
				Factor		= obj.FindProperty ("factor");
				Curvature	= obj.FindProperty ("curvature");
				Top			= obj.FindProperty ("top");
				Bottom		= obj.FindProperty ("bottom");
				Axis		= obj.FindProperty ("axis");
			}
		}

		private Content content = new Content ();
		private Properties properties = new Properties ();

		private void OnEnable ()
		{
			content.Update ();
			properties.Update (serializedObject);
		}

		public override void OnInspectorGUI()
        {
            base.OnInspectorGUI ();

			EditorGUILayout.PropertyField (properties.Factor, content.Factor);
			EditorGUILayout.PropertyField (properties.Curvature, content.Curvature);

			using (new EditorGUI.IndentLevelScope ())
			{
				DeformEditorGUILayout.MinField (properties.Top, properties.Bottom.floatValue, content.Top);
				DeformEditorGUILayout.MaxField (properties.Bottom, properties.Top.floatValue, content.Bottom);
			}

			EditorGUILayout.PropertyField (properties.Axis, content.Axis);

			serializedObject.ApplyModifiedProperties ();
			EditorApplication.QueuePlayerLoopUpdate ();
        }

        private void OnSceneGUI ()
        {
            if (target == null)
                return;

            var stretch = target as SquashAndStretchDeformer;

            DrawBoundsHandles (stretch);

            EditorApplication.QueuePlayerLoopUpdate ();
        }

        private void DrawBoundsHandles (SquashAndStretchDeformer stretch)
        {
			var direction = stretch.Axis.forward;
			var topWorldPosition = stretch.Axis.position + direction * stretch.Top;
			var botWorldPosition = stretch.Axis.position + direction * stretch.Bottom;

			DeformHandles.Line (topWorldPosition, botWorldPosition, DeformHandles.LineMode.LightDotted);

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newTopWorld = DeformHandles.Slider (topWorldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (stretch, "Changed Top");
					var newTop = DeformHandlesUtility.DistanceAlongAxis (stretch.Axis, stretch.Axis.position, newTopWorld, Axis.Z);
					stretch.Top = newTop;
				}
			}

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newBotWorld = DeformHandles.Slider (botWorldPosition, -direction);
				var newBot = DeformHandlesUtility.DistanceAlongAxis (stretch.Axis, stretch.Axis.position, newBotWorld, Axis.Z);
				if (check.changed)
				{
					Undo.RecordObject (stretch, "Changed Bottom");
					stretch.Bottom = newBot;
				}
			}
        }
    }
}