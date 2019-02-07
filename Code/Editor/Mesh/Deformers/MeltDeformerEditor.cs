using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (MeltDeformer)), CanEditMultipleObjects]
	public class MeltDeformerEditor : DeformerEditor
	{
		private class Content
		{
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent Falloff = DeformEditorGUIUtility.DefaultContent.Falloff;
			public static readonly GUIContent Radius = new GUIContent (text: "Radius", tooltip: "How far the vertices spread as they approach the bottom.");
			public static readonly GUIContent UseNormals = new GUIContent (text: "Use Normals", tooltip: "When true the vertices will spread along the normals' xy plane (relative to the melt axis) which can look better, but can result in a split mesh if you have unsmoothed vertices.");
			public static readonly GUIContent ClampAtBottom = new GUIContent (text: "Clamp At Bottom", tooltip: "When true, vertices won't be allowed below the bottom limit.");
			public static readonly GUIContent Top = DeformEditorGUIUtility.DefaultContent.Top;
			public static readonly GUIContent Bottom = new GUIContent (text: "Bottom", tooltip: "Any vertices below this will have the full effect.");
			public static readonly GUIContent Noise = new GUIContent (text: "Noise");
			public static readonly GUIContent Vertical = new GUIContent (text: "Vertical");
			public static readonly GUIContent VerticalFrequency = new GUIContent (text: "Frequency", tooltip: "The frequency of the vertical noise. Lower values result in a smoother mesh.");
			public static readonly GUIContent VerticalMagnitude = new GUIContent (text: "Magnitude", tooltip: "The strength of the vertical noise.");
			public static readonly GUIContent Radial = new GUIContent (text: "Radial");
			public static readonly GUIContent RadialFrequency = new GUIContent (text: "Frequency", tooltip: "The frequency of the radial noise. Lower values result in a smoother mesh.");
			public static readonly GUIContent RadialMagnitude = new GUIContent (text: "Magnitude", tooltip: "The strength of the radial noise.");
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private class Properties
		{
			public SerializedProperty Factor;
			public SerializedProperty Falloff;
			public SerializedProperty Radius;
			public SerializedProperty UseNormals;
			public SerializedProperty ClampAtBottom;
			public SerializedProperty Top;
			public SerializedProperty Bottom;
			public SerializedProperty VerticalFrequency;
			public SerializedProperty VerticalMagnitude;
			public SerializedProperty RadialFrequency;
			public SerializedProperty RadialMagnitude; 
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
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

			DeformEditorGUILayout.MinField (properties.Factor, 0f, Content.Factor);
			DeformEditorGUILayout.MinField (properties.Falloff, 0f);
			EditorGUILayout.PropertyField (properties.Radius, Content.Radius);
			EditorGUILayout.PropertyField (properties.UseNormals, Content.UseNormals);
			EditorGUILayout.PropertyField (properties.ClampAtBottom, Content.ClampAtBottom);
			DeformEditorGUILayout.MinField (properties.Top, properties.Bottom.floatValue, Content.Top);
			DeformEditorGUILayout.MaxField (properties.Bottom, properties.Top.floatValue, Content.Bottom);

			EditorGUILayout.LabelField (Content.Noise);
			using (new EditorGUI.IndentLevelScope ())
			{
				EditorGUILayout.LabelField (Content.Vertical);

				using (new EditorGUI.IndentLevelScope ())
				{
					EditorGUILayout.PropertyField (properties.VerticalFrequency, Content.VerticalFrequency);
					EditorGUILayout.PropertyField (properties.VerticalMagnitude, Content.VerticalMagnitude);
				}

				EditorGUILayout.LabelField (Content.Radial);

				using (new EditorGUI.IndentLevelScope ())
				{
					EditorGUILayout.PropertyField (properties.RadialFrequency, Content.RadialFrequency);
					EditorGUILayout.PropertyField (properties.RadialMagnitude, Content.RadialMagnitude);
				}
			}

			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			serializedObject.ApplyModifiedProperties ();

			DeformEditorGUILayout.WIPAlert ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		public override void OnSceneGUI ()
		{
			base.OnSceneGUI ();

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