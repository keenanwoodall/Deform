using UnityEngine;
using UnityEditor;
using Deform.Masking;

namespace DeformEditor.Masking
{
	[CustomEditor (typeof (SphereMask)), CanEditMultipleObjects]
	public class SphereMaskEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				Factor, 
				InnerRadius,
				OuterRadius, 
				Invert, 
				Axis;

			public void Update ()
			{
				Factor = DeformEditorGUIUtility.DefaultContent.Factor;
				InnerRadius = new GUIContent
				(
					text: "Inner Radius"
				);
				OuterRadius = new GUIContent
				(
					text: "Outer Radius"
				);
				Invert = new GUIContent
				(
					text: "Invert"
				);
				Axis = DeformEditorGUIUtility.DefaultContent.Axis;
			}
		}

		private class Properties
		{
			public SerializedProperty
				Factor, 
				InnerRadius, 
				OuterRadius, 
				Invert, 
				Axis;

			public void Update (SerializedObject obj)
			{
				Factor		= obj.FindProperty ("factor");
				InnerRadius = obj.FindProperty ("innerRadius");
				OuterRadius = obj.FindProperty ("outerRadius");
				Invert		= obj.FindProperty ("invert");
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

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.UpdateIfRequiredOrScript ();
			EditorGUILayout.Slider (properties.Factor, 0f, 1f, content.Factor);
			DeformEditorGUILayout.MaxField (properties.InnerRadius, properties.OuterRadius.floatValue, content.InnerRadius);
			DeformEditorGUILayout.MinField (properties.OuterRadius, properties.InnerRadius.floatValue, content.OuterRadius);
			EditorGUILayout.PropertyField (properties.Invert, content.Invert);
			EditorGUILayout.PropertyField (properties.Axis, content.Axis);
			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void OnSceneGUI ()
		{
			var sphereMask = target as SphereMask;

			DrawOuterRadiusHandle (sphereMask);
			DrawInnerRadiusHandle (sphereMask);

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawInnerRadiusHandle (SphereMask sphereMask)
		{
			using (new Handles.DrawingScope (Matrix4x4.TRS (sphereMask.Axis.position, sphereMask.Axis.rotation, sphereMask.Axis.localScale)))
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
			using (new Handles.DrawingScope (Matrix4x4.TRS (sphereMask.Axis.position, sphereMask.Axis.rotation, sphereMask.Axis.localScale)))
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