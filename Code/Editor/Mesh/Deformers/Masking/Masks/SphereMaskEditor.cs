using UnityEngine;
using UnityEditor;
using Beans.Unity.Editor;
using Deform.Masking;

namespace DeformEditor.Masking
{
	[CustomEditor (typeof (SphereMask)), CanEditMultipleObjects]
	public class SphereMaskEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent InnerRadius = new GUIContent (text: "Inner Radius");
			public static readonly GUIContent OuterRadius = new GUIContent (text: "Outer Radius");
			public static readonly GUIContent Invert = new GUIContent (text: "Invert");
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private class Properties
		{
			public SerializedProperty Factor;
			public SerializedProperty InnerRadius;
			public SerializedProperty OuterRadius;
			public SerializedProperty Invert;
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
			{
				Factor		= obj.FindProperty ("factor");
				InnerRadius = obj.FindProperty ("innerRadius");
				OuterRadius = obj.FindProperty ("outerRadius");
				Invert		= obj.FindProperty ("invert");
				Axis		= obj.FindProperty ("axis");
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

			EditorGUILayout.Slider (properties.Factor, 0f, 1f, Content.Factor);
			EditorGUILayoutx.MaxField (properties.InnerRadius, properties.OuterRadius.floatValue, Content.InnerRadius);
			EditorGUILayoutx.MinField (properties.OuterRadius, properties.InnerRadius.floatValue, Content.OuterRadius);
			EditorGUILayout.PropertyField (properties.Invert, Content.Invert);
			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		public override void OnSceneGUI ()
		{
			base.OnSceneGUI ();

			var sphereMask = target as SphereMask;

			DrawOuterRadiusHandle (sphereMask);
			DrawInnerRadiusHandle (sphereMask);

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawInnerRadiusHandle (SphereMask sphereMask)
		{
			using (new Handles.DrawingScope (Matrix4x4.TRS (sphereMask.Axis.position, sphereMask.Axis.rotation, sphereMask.Axis.lossyScale)))
			{
				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var newRadius = DeformHandles.Radius (Quaternion.identity, Vector3.zero, sphereMask.InnerRadius * 0.5f);
					if (check.changed)
					{
						Undo.RecordObject (sphereMask, "Changed Radius");
						sphereMask.InnerRadius = newRadius * 2f;
					}
				}
			}
		}

		private void DrawOuterRadiusHandle (SphereMask sphereMask)
		{
			using (new Handles.DrawingScope (Matrix4x4.TRS (sphereMask.Axis.position, sphereMask.Axis.rotation, sphereMask.Axis.lossyScale)))
			{
				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var newRadius = DeformHandles.Radius (Quaternion.identity, Vector3.zero, sphereMask.OuterRadius * 0.5f);
					if (check.changed)
					{
						Undo.RecordObject (sphereMask, "Changed Radius");
						sphereMask.OuterRadius = newRadius * 2f;
					}
				}
			}
		}
	}
}