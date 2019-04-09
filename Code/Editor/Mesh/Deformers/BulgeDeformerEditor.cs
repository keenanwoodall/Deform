using UnityEngine;
using UnityEditor;
using Beans.Unity.Editor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (BulgeDeformer)), CanEditMultipleObjects]
	public class BulgeDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Factor	= DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent Top		= DeformEditorGUIUtility.DefaultContent.Top;
			public static readonly GUIContent Bottom	= DeformEditorGUIUtility.DefaultContent.Bottom;
			public static readonly GUIContent Smooth	= DeformEditorGUIUtility.DefaultContent.Smooth;
			public static readonly GUIContent Axis		= DeformEditorGUIUtility.DefaultContent.Axis;
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
		private VerticalBoundsHandle boundsHandle = new VerticalBoundsHandle ();

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

			EditorGUILayout.PropertyField (properties.Factor, Content.Factor);
			EditorGUILayoutx.MinField (properties.Top, properties.Bottom.floatValue, Content.Top);
			EditorGUILayoutx.MaxField (properties.Bottom, properties.Top.floatValue, Content.Bottom);
			EditorGUILayout.PropertyField (properties.Smooth, Content.Smooth);
			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		public override void OnSceneGUI ()
		{
			base.OnSceneGUI ();

			var bulge = target as BulgeDeformer;

			DrawFactorHandle (bulge);

			boundsHandle.HandleColor = DeformEditorSettings.SolidHandleColor;
			boundsHandle.ScreenspaceHandleSize = DeformEditorSettings.ScreenspaceSliderHandleCapSize;
			if (boundsHandle.DrawHandle (bulge.Top, bulge.Bottom, bulge.Axis, Vector3.forward))
			{
				Undo.RecordObject (bulge, "Changed Bounds");
				bulge.Top = boundsHandle.Top;
				bulge.Bottom = boundsHandle.Bottom;
			}

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawFactorHandle (BulgeDeformer bulge)
		{
			var direction = bulge.Axis.up;

			var center = bulge.Axis.position + (bulge.Axis.forward * ((bulge.Top + bulge.Bottom) * 0.5f));
			var worldPosition = center + direction * ((bulge.Factor + 1f) * 0.5f);

			DeformHandles.Line (center, worldPosition, DeformHandles.LineMode.LightDotted);

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
	}
}