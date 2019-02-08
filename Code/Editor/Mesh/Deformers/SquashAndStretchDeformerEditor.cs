using UnityEngine;
using UnityEditor;
using Beans.Unity.Editor;
using Deform;

namespace DeformEditor
{
    [CustomEditor (typeof (SquashAndStretchDeformer)), CanEditMultipleObjects]
    public class SquashAndStretchDeformerEditor : DeformerEditor
    {
		private class Content
		{
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent Curvature = new GUIContent (text: "Curvature", tooltip: "How much the mesh is bulged when squashed and squeezed when stretched.");
			public static readonly GUIContent Top = DeformEditorGUIUtility.DefaultContent.Top;
			public static readonly GUIContent Bottom = DeformEditorGUIUtility.DefaultContent.Bottom;
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private class Properties
		{
			public SerializedProperty Factor;
			public SerializedProperty Curvature;
			public SerializedProperty Top;
			public SerializedProperty Bottom;
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
			{
				Factor		= obj.FindProperty ("factor");
				Curvature	= obj.FindProperty ("curvature");
				Top			= obj.FindProperty ("top");
				Bottom		= obj.FindProperty ("bottom");
				Axis		= obj.FindProperty ("axis");
			}
		}

		private Properties properties;

		protected override void OnEnable ()
		{
			base.OnEnable ();
			properties = new Properties (serializedObject);
		}

		public override void OnInspectorGUI()
        {
            base.OnInspectorGUI ();

			serializedObject.UpdateIfRequiredOrScript ();
			EditorGUILayout.PropertyField (properties.Factor, Content.Factor);
			EditorGUILayout.PropertyField (properties.Curvature, Content.Curvature);

			using (new EditorGUI.IndentLevelScope ())
			{
				EditorGUILayoutx.MinField (properties.Top, properties.Bottom.floatValue, Content.Top);
				EditorGUILayoutx.MaxField (properties.Bottom, properties.Top.floatValue, Content.Bottom);
			}

			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);
			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
        }

        public override void OnSceneGUI ()
        {
			base.OnSceneGUI ();

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