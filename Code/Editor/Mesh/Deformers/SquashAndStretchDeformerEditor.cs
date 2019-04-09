using UnityEngine;
using UnityEditor;
using Beans.Unity.Editor;
using Deform;

namespace DeformEditor
{
    [CustomEditor (typeof (SquashAndStretchDeformer)), CanEditMultipleObjects]
    public class SquashAndStretchDeformerEditor : DeformerEditor
    {
		private static class Content
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
		private VerticalBoundsHandle boundsHandle = new VerticalBoundsHandle ();

		protected override void OnEnable ()
		{
			base.OnEnable ();

			properties = new Properties (serializedObject);

			boundsHandle.HandleCapFunction = DeformHandles.HandleCapFunction;
			boundsHandle.DrawGuidelineCallback = (a, b) => DeformHandles.Line (a, b, DeformHandles.LineMode.LightDotted);
		}

		public override void OnInspectorGUI()
        {
            base.OnInspectorGUI ();

			serializedObject.UpdateIfRequiredOrScript ();
			EditorGUILayout.PropertyField (properties.Factor, Content.Factor);
			EditorGUILayout.PropertyField (properties.Curvature, Content.Curvature);

			EditorGUILayoutx.MinField (properties.Top, properties.Bottom.floatValue, Content.Top);
			EditorGUILayoutx.MaxField (properties.Bottom, properties.Top.floatValue, Content.Bottom);

			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);
			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
        }

        public override void OnSceneGUI ()
        {
			base.OnSceneGUI ();

            var stretch = target as SquashAndStretchDeformer;

			var topMultiplier = 0f;
			var bottomMultiplier = 0f;
			if (stretch.Factor >= 0)
			{
				topMultiplier = 1f + stretch.Factor;
				bottomMultiplier = 1f + stretch.Factor;
			}
			else
			{
				topMultiplier = -1f / (stretch.Factor - 1f);
				bottomMultiplier = -1f / (stretch.Factor - 1f);
			}

			boundsHandle.HandleColor = DeformEditorSettings.SolidHandleColor;
			boundsHandle.ScreenspaceHandleSize = DeformEditorSettings.ScreenspaceSliderHandleCapSize;
			if (boundsHandle.DrawHandle (stretch.Top * topMultiplier, stretch.Bottom * bottomMultiplier, stretch.Axis, Vector3.forward))
			{
				Undo.RecordObject (stretch, "Changed Bounds");
				stretch.Top = boundsHandle.Top / topMultiplier;
				stretch.Bottom = boundsHandle.Bottom / bottomMultiplier;
			}

			EditorApplication.QueuePlayerLoopUpdate ();
        }
    }
}