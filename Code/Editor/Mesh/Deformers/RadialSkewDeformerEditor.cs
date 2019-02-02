using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (RadialSkewDeformer)), CanEditMultipleObjects]
	public class RadialSkewDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent
				Factor, 
				Mode, 
				Top, 
				Bottom,
				Axis;

			public void Update ()
			{
				Factor = DeformEditorGUIUtility.DefaultContent.Factor;
				Mode = new GUIContent
				(
					text: "Mode",
					tooltip: "Unlimited: Entire mesh is skewed.\nLimited: Mesh is only skewed between bounds."
				);
				Top = new GUIContent
				(
					text: "Top",
					tooltip: "Any vertices above this will be unskewed."
				);
				Bottom = new GUIContent
				(
					text: "Bottom",
					tooltip: "Any vertices below this will be unskewed."
				);
				Axis = DeformEditorGUIUtility.DefaultContent.Axis;
			}
		}

		public class Properties
		{
			public SerializedProperty 
				Factor, 
				Mode, 
				Top, 
				Bottom, 
				Axis;

			public void Update (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
				Mode	= obj.FindProperty ("mode");
				Top		= obj.FindProperty ("top");
				Bottom	= obj.FindProperty ("bottom");
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

			EditorGUILayout.PropertyField (properties.Factor, content.Factor);
			EditorGUILayout.PropertyField (properties.Mode, content.Mode);

			using (new EditorGUI.DisabledScope (properties.Mode.enumValueIndex == 0 && !properties.Mode.hasMultipleDifferentValues))
			{
				using (new EditorGUI.IndentLevelScope ())
				{
					DeformEditorGUILayout.MinField (properties.Top, properties.Bottom.floatValue, content.Top);
					DeformEditorGUILayout.MaxField (properties.Bottom, properties.Top.floatValue, content.Bottom);
				}
			}


			EditorGUILayout.PropertyField (properties.Axis, content.Axis);

			DeformEditorGUILayout.WIPAlert ();

			serializedObject.ApplyModifiedProperties ();
			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void OnSceneGUI ()
		{
			if (target == null)
				return;

			var skew = target as RadialSkewDeformer;

			if (skew.Mode == BoundsMode.Limited)
				DrawBoundsHandles (skew);
			else
				DrawAxisGuide (skew);
			DrawFactorHandle (skew);

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawAxisGuide (RadialSkewDeformer skew)
		{
			var direction = skew.Axis.up;

			var top = skew.Axis.position + direction * HandleUtility.GetHandleSize (skew.Axis.position) * 2f;
			var bottom = skew.Axis.position - direction * HandleUtility.GetHandleSize (skew.Axis.position) * 2f;

			DeformHandles.Line (top, bottom, DeformHandles.LineMode.LightDotted);
		}

		private void DrawBoundsHandles (RadialSkewDeformer skew)
		{
			var direction = skew.Axis.up;

			var topHandleWorldPosition = skew.Axis.position + direction * skew.Top;
			var bottomHandleWorldPosition = skew.Axis.position + direction * skew.Bottom;

			DeformHandles.Line (topHandleWorldPosition, bottomHandleWorldPosition, DeformHandles.LineMode.LightDotted);

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newTopWorldPosition = DeformHandles.Slider (topHandleWorldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (skew, "Changed Top");
					var newTop = DeformHandlesUtility.DistanceAlongAxis (skew.Axis, skew.Axis.position, newTopWorldPosition, Axis.Y);
					skew.Top = newTop;
				}
			}

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newBottomWorldPosition = DeformHandles.Slider (bottomHandleWorldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (skew, "Changed Bottom");
					var newBottom = DeformHandlesUtility.DistanceAlongAxis (skew.Axis, skew.Axis.position, newBottomWorldPosition, Axis.Y);
					skew.Bottom = newBottom;
				}
			}
		}

		private void DrawFactorHandle (RadialSkewDeformer skew)
		{
			var direction = skew.Axis.forward;
			var handleWorldPosition = skew.Axis.position + direction * skew.Factor;

			DeformHandles.Line (skew.Axis.position, handleWorldPosition, DeformHandles.LineMode.LightDotted);

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newWorldPosition = DeformHandles.Slider (handleWorldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (skew, "Changed Factor");
					var newFactor = DeformHandlesUtility.DistanceAlongAxis (skew.Axis, skew.Axis.position, newWorldPosition, Axis.Z);
					skew.Factor = newFactor;
				}
			}
		}
	}
}