using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (MeltDeformer)), CanEditMultipleObjects]
	public class MeltDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				Factor, 
				Falloff, 
				Radius, 
				UseNormals, 
				ClampAtBottom, 
				Top, 
				Bottom, 
				Noise, 
				Vertical, 
				VerticalFrequency, 
				VerticalMagnitude, 
				Radial, 
				RadialFrequency, 
				RadialMagnitude, 
				Axis;

			public void Update ()
			{
				Factor = DeformEditorGUIUtility.DefaultContent.Factor;
				Falloff = DeformEditorGUIUtility.DefaultContent.Falloff;
				Radius = new GUIContent
				(
					text: "Radius",
					tooltip: "How far the vertices spread as they approach the bottom."
				);
				UseNormals = new GUIContent
				(
					text: "Use Normals",
					tooltip: "When true the vertices will spread along the normals' xy plane (relative to the melt axis) which can look better, but can result in a split mesh if you have unsmoothed vertices."
				);
				ClampAtBottom = new GUIContent
				(
					text: "Clamp At Bottom",
					tooltip: "When true, vertices won't be allowed below the bottom limit."
				);
				Top = DeformEditorGUIUtility.DefaultContent.Top;
				Bottom = new GUIContent
				(
					text: "Bottom",
					tooltip: "Any vertices below this will have the full effect."
				);
				Noise = new GUIContent
				(
					text: "Noise"
				);
				Vertical = new GUIContent
				(
					text: "Vertical"
				);
				VerticalFrequency = new GUIContent
				(
					text: "Frequency",
					tooltip: "The frequency of the vertical noise. Lower values result in a smoother mesh."
				);
				VerticalMagnitude = new GUIContent
				(
					text: "Magnitude",
					tooltip: "The strength of the vertical noise."
				);
				Radial = new GUIContent
				(
					text: "Radial"
				);
				RadialFrequency = new GUIContent
				(
					text: "Frequency",
					tooltip: "The frequency of the radial noise. Lower values result in a smoother mesh."
				);
				RadialMagnitude = new GUIContent
				(
					text: "Magnitude",
					tooltip: "The strength of the radial noise."
				);
				Axis = DeformEditorGUIUtility.DefaultContent.Axis;
			}
		}

		private class Properties
		{
			public SerializedProperty 
				Factor, 
				Falloff, 
				Radius, 
				UseNormals,
				ClampAtBottom,
				Top, 
				Bottom, 
				VerticalFrequency,
				VerticalMagnitude, 
				RadialFrequency,
				RadialMagnitude, 
				Axis;

			public void Update (SerializedObject obj)
			{
				Factor				= obj.FindProperty ("factor");
				Falloff				= obj.FindProperty ("falloff");
				Radius				= obj.FindProperty ("radius");
				UseNormals			= obj.FindProperty ("useNormals");
				ClampAtBottom		= obj.FindProperty ("clampAtBottom");
				Top					= obj.FindProperty ("top");
				Bottom				= obj.FindProperty ("bottom");
				VerticalFrequency	= obj.FindProperty ("verticalFrequency");
				VerticalMagnitude	= obj.FindProperty ("verticalMagnitude");
				RadialFrequency		= obj.FindProperty ("radialFrequency");
				RadialMagnitude		= obj.FindProperty ("radialMagnitude");
				Axis				= obj.FindProperty ("axis");
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
			DeformEditorGUILayout.MinField (properties.Factor, 0f, content.Factor);
			DeformEditorGUILayout.MinField (properties.Falloff, 0f);
			EditorGUILayout.PropertyField (properties.Radius, content.Radius);
			EditorGUILayout.PropertyField (properties.UseNormals, content.UseNormals);
			EditorGUILayout.PropertyField (properties.ClampAtBottom, content.ClampAtBottom);
			DeformEditorGUILayout.MinField (properties.Top, properties.Bottom.floatValue, content.Top);
			DeformEditorGUILayout.MaxField (properties.Bottom, properties.Top.floatValue, content.Bottom);

			EditorGUILayout.LabelField (content.Noise);
			using (new EditorGUI.IndentLevelScope ())
			{
				EditorGUILayout.LabelField (content.Vertical);

				using (new EditorGUI.IndentLevelScope ())
				{
					EditorGUILayout.PropertyField (properties.VerticalFrequency, content.VerticalFrequency);
					EditorGUILayout.PropertyField (properties.VerticalMagnitude, content.VerticalMagnitude);
				}

				EditorGUILayout.LabelField (content.Radial);

				using (new EditorGUI.IndentLevelScope ())
				{
					EditorGUILayout.PropertyField (properties.RadialFrequency, content.RadialFrequency);
					EditorGUILayout.PropertyField (properties.RadialMagnitude, content.RadialMagnitude);
				}
			}

			EditorGUILayout.PropertyField (properties.Axis, content.Axis);
			serializedObject.ApplyModifiedProperties ();

			DeformEditorGUILayout.WIPAlert ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void OnSceneGUI ()
		{
			if (target == null)
				return;

			var melt = target as MeltDeformer;

			DrawRadiusHandle (melt);
			DrawBoundsHandles (melt);

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawRadiusHandle (MeltDeformer melt)
		{
			var bottomWorldPosition = melt.Axis.position + melt.Axis.forward * melt.Bottom;

			var scaledRadius = (melt.Radius + 1f) * 0.5f;

			DeformHandles.Circle (bottomWorldPosition, melt.Axis.forward, melt.Axis.right, scaledRadius);

			var direction = melt.Axis.up;
			var radiusWorldPosition = melt.Axis.position + direction * scaledRadius;

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newRadiusWorldPosition = DeformHandles.Slider (radiusWorldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (melt, "Changed Radius");
					var newRadius = DeformHandlesUtility.DistanceAlongAxis (melt.Axis, melt.Axis.position, newRadiusWorldPosition, Axis.Y);
					melt.Radius = (newRadius * 2f) - 1f;
				}
			}
		}

		private void DrawBoundsHandles (MeltDeformer melt)
		{
			var direction = melt.Axis.forward;

			var topWorldPosition = melt.Axis.position + direction * melt.Top;
			var bottomWorldPosition = melt.Axis.position + direction * melt.Bottom;

			DeformHandles.Line (bottomWorldPosition, topWorldPosition, DeformHandles.LineMode.LightDotted);

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newBottomWorldPosition = DeformHandles.Slider (bottomWorldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (melt, "Changed Bottom");
					var newBottom = DeformHandlesUtility.DistanceAlongAxis (melt.Axis, melt.Axis.position, newBottomWorldPosition, Axis.Z);
					melt.Bottom = newBottom;
				}
			}

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newTopWorldPosition = DeformHandles.Slider (topWorldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (melt, "Changed Top");
					var newTop = DeformHandlesUtility.DistanceAlongAxis (melt.Axis, melt.Axis.position, newTopWorldPosition, Axis.Z);
					melt.Top = newTop;
				}
			}
		}
	}
}