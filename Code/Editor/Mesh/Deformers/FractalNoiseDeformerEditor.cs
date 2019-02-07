using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (FractalNoiseDeformer))]
	public class FractalNoiseDeformerEditor : DeformerEditor
	{
		private class Content
		{
			public static readonly GUIContent Mode = new GUIContent (text: "Mode", tooltip: "Derivative: Vertices move along vectors created by the difference between different noise samples.\nNormal: Vertices move along their normals.\nSpherical: Vertices get pushed away from the axis center.\nColor: Vertices get use the vertex color as a vector and move along it.");
			public static readonly GUIContent Octaves = new GUIContent (text: "Octaves", tooltip: "The number of times noise is layered.");
			public static readonly GUIContent Persistance = new GUIContent (text: "Persistance", tooltip: "The multiplier that determines how much the magnitude changes each octave.");
			public static readonly GUIContent Lacunarity = new GUIContent (text: "Lacunarity", tooltip: "The multiplier that determines how much the frequency changes each octave.");
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
			public SerializedProperty Octaves;
			public SerializedProperty Persistance;
			public SerializedProperty Lacunarity;
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
				Mode				= obj.FindProperty ("mode");
				Octaves				= obj.FindProperty ("octaves");
				Persistance			= obj.FindProperty ("persistance");
				Lacunarity			= obj.FindProperty ("lacunarity");
				MagnitudeScalar		= obj.FindProperty ("magnitudeScalar");
				MagnitudeVector		= obj.FindProperty ("magnitudeVector");
				FrequencyScalar		= obj.FindProperty ("frequencyScalar");
				FrequencyVector		= obj.FindProperty ("frequencyVector");
				OffsetVector		= obj.FindProperty ("offsetVector");
				OffsetSpeedScalar	= obj.FindProperty ("offsetSpeedScalar");
				OffsetSpeedVector	= obj.FindProperty ("offsetSpeedVector");
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

			EditorGUILayout.PropertyField (properties.Mode, Content.Mode);

			DeformEditorGUILayout.MinField (properties.Octaves, 1, Content.Octaves);
			EditorGUILayout.PropertyField (properties.Persistance, Content.Persistance);
			EditorGUILayout.PropertyField (properties.Lacunarity, Content.Lacunarity);

			EditorGUILayout.LabelField (Content.Magnitude);

			using (new EditorGUI.IndentLevelScope ())
			{
				EditorGUILayout.PropertyField (properties.MagnitudeScalar, Content.MagnitudeScalar);

				using (new EditorGUI.DisabledScope (!properties.Mode.hasMultipleDifferentValues && properties.Mode.enumValueIndex != 0))
					EditorGUILayout.PropertyField (properties.MagnitudeVector, Content.MagnitudeVector);
			}

			EditorGUILayout.LabelField (Content.Frequency);

			using (new EditorGUI.IndentLevelScope ())
			{
				EditorGUILayout.PropertyField (properties.FrequencyScalar, Content.FrequencyScalar);
				EditorGUILayout.PropertyField (properties.FrequencyVector, Content.FrequencyVector);
			}

			EditorGUILayout.LabelField (Content.Offset);

			using (new EditorGUI.IndentLevelScope ())
			{
				EditorGUIUtility.wideMode = true;
				EditorGUILayout.PropertyField (properties.OffsetVector, Content.OffsetVector);
				EditorGUIUtility.wideMode = false;

				EditorGUILayout.PropertyField (properties.OffsetSpeedScalar, Content.OffsetSpeedScalar);

				EditorGUIUtility.wideMode = true;
				EditorGUILayout.PropertyField (properties.OffsetSpeedVector, Content.OffsetSpeedVector);
				EditorGUIUtility.wideMode = false;
			}

			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}