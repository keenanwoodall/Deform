using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (NoiseDeformer), true), CanEditMultipleObjects]
	public abstract class NoiseDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Mode = new GUIContent (text: "Mode", tooltip: "Derivative: Vertices move along vectors created by the difference between different noise samples.\nNormal: Vertices move along their normals.\nSpherical: Vertices get pushed away from the axis center.\nColor: Vertices get use the vertex color as a vector and move along it.");
			public static readonly GUIContent Magnitude = new GUIContent (text: "Magnitude");
			public static readonly GUIContent MagnitudeScalar = new GUIContent (text: "Scale", tooltip: "Overall strength of the noise.");
			public static readonly GUIContent MagnitudeVector = new GUIContent (text: "Vector", tooltip: "Per axis strength of the noise.");
			public static readonly GUIContent Frequency = new GUIContent (text: "Frequency");
			public static readonly GUIContent FrequencyScalar = new GUIContent (text: "Scale", tooltip: "Overall frequency of the noise.");
			public static readonly GUIContent FrequencyVector = new GUIContent (text: "Vector", tooltip: "Per axis frequency of the noise.");
			public static readonly GUIContent Offset = new GUIContent (text: "Offset");
			public static readonly GUIContent OffsetVector = new GUIContent (text: "Offset", tooltip: "Per axis noise offset.");
			public static readonly GUIContent OffsetSpeedScalar = new GUIContent (text: "Speed", tooltip: "Total change of the offset per second.");
			public static readonly GUIContent OffsetSpeedVector = new GUIContent (text: "Velocity", tooltip: "Per axis change of the offset per second.");
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private class Properties
		{
			public SerializedProperty Mode;
			public SerializedProperty MagnitudeScalar;
			public SerializedProperty MagnitudeVector;
			public SerializedProperty FrequencyScalar;
			public SerializedProperty FrequencyVector;
			public SerializedProperty OffsetVector;
			public SerializedProperty OffsetSpeedScalar;
			public SerializedProperty OffsetSpeedVector;
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
			{
				Mode = obj.FindProperty ("mode");
				MagnitudeScalar = obj.FindProperty ("magnitudeScalar");
				MagnitudeVector = obj.FindProperty ("magnitudeVector");
				FrequencyScalar = obj.FindProperty ("frequencyScalar");
				FrequencyVector = obj.FindProperty ("frequencyVector");
				OffsetVector = obj.FindProperty ("offsetVector");
				OffsetSpeedScalar = obj.FindProperty ("offsetSpeedScalar");
				OffsetSpeedVector = obj.FindProperty ("offsetSpeedVector");
				Axis = obj.FindProperty ("axis");
			}
		}

		protected delegate void DrawPropertyOverrideCallback (SerializedProperty property, GUIContent defaultContent);

		private Properties properties;

		protected DrawPropertyOverrideCallback drawNoiseModeOverride;
		protected DrawPropertyOverrideCallback drawMagnitudeScalarOverride;
		protected DrawPropertyOverrideCallback drawMagnitudeVectorOverride;
		protected DrawPropertyOverrideCallback drawFrequencyScalarOverride;
		protected DrawPropertyOverrideCallback drawFrequencyVectorOverride;
		protected DrawPropertyOverrideCallback drawOffsetVectorOverride;
		protected DrawPropertyOverrideCallback drawOffsetSpeedScalarOverride;
		protected DrawPropertyOverrideCallback drawOffsetSpeedVectorOverride;

		protected override void OnEnable ()
		{
			base.OnEnable ();

			properties = new Properties (serializedObject);
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.UpdateIfRequiredOrScript ();

			if (drawNoiseModeOverride != null)
				drawNoiseModeOverride (properties.Mode, Content.Mode);
			else
				EditorGUILayout.PropertyField (properties.Mode, Content.Mode);

			EditorGUILayout.LabelField (Content.Magnitude);

			using (new EditorGUI.IndentLevelScope ())
			{
				if (drawMagnitudeScalarOverride != null)
					drawMagnitudeScalarOverride (properties.MagnitudeScalar, Content.MagnitudeScalar);
				else
					EditorGUILayout.PropertyField (properties.MagnitudeScalar, Content.MagnitudeScalar);

				using (new EditorGUI.DisabledScope (!properties.Mode.hasMultipleDifferentValues && (NoiseMode)properties.Mode.enumValueIndex != NoiseMode._3D))
				{
					if (drawMagnitudeVectorOverride != null)
						drawMagnitudeVectorOverride (properties.MagnitudeVector, Content.MagnitudeVector);
					else
						EditorGUILayout.PropertyField (properties.MagnitudeVector, Content.MagnitudeVector);
				}
			}

			EditorGUILayout.LabelField (Content.Frequency);

			using (new EditorGUI.IndentLevelScope ())
			{
				if (drawFrequencyScalarOverride != null)
					drawFrequencyScalarOverride (properties.FrequencyScalar, Content.FrequencyScalar);
				else
					EditorGUILayout.PropertyField (properties.FrequencyScalar, Content.FrequencyScalar);

				if (drawFrequencyVectorOverride != null)
					drawFrequencyVectorOverride (properties.FrequencyVector, Content.FrequencyVector);
				else
					EditorGUILayout.PropertyField (properties.FrequencyVector, Content.FrequencyVector);
			}

			EditorGUILayout.LabelField (Content.Offset);

			using (new EditorGUI.IndentLevelScope ())
			{
				EditorGUIUtility.wideMode = true;
				if (drawOffsetVectorOverride != null)
					drawOffsetVectorOverride (properties.OffsetVector, Content.OffsetVector);
				else
					EditorGUILayout.PropertyField (properties.OffsetVector, Content.OffsetVector);
				EditorGUIUtility.wideMode = false;

				if (drawOffsetSpeedScalarOverride != null)
					drawOffsetSpeedScalarOverride (properties.OffsetSpeedScalar, Content.OffsetSpeedScalar);
				else
					EditorGUILayout.PropertyField (properties.OffsetSpeedScalar, Content.OffsetSpeedScalar);

				EditorGUIUtility.wideMode = true;
				if (drawOffsetSpeedVectorOverride != null)
					drawOffsetSpeedVectorOverride (properties.OffsetSpeedVector, Content.OffsetSpeedVector);
				else
					EditorGUILayout.PropertyField (properties.OffsetSpeedVector, Content.OffsetSpeedVector);
				EditorGUIUtility.wideMode = false;
			}

			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}