using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (NoiseDeformer)), CanEditMultipleObjects]
	public class NoiseDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				Mode, 
				Magnitude,
				MagnitudeScalar, 
				MagnitudeVector, 
				Frequency,
				FrequencyScalar,
				FrequencyVector, 
				Offset, OffsetVector,
				OffsetSpeedScalar,
				OffsetSpeedVector,
				Axis;

			public void Update ()
			{
				Mode = new GUIContent
				(
					text: "Mode",
					tooltip: "Derivative: Vertices move along vectors created by the difference between different noise samples.\nNormal: Vertices move along their normals.\nSpherical: Vertices get pushed away from the axis center.\nColor: Vertices get use the vertex color as a vector and move along it."
				);
				Magnitude = new GUIContent
				(
					text: "Magnitude"
				);
				MagnitudeScalar = new GUIContent
				(
					text: "Scale",
					tooltip: "Overall strength of the noise."
				);
				MagnitudeVector = new GUIContent
				(
					text: "Vector",
					tooltip: "Per axis strength of the noise."
				);
				Frequency = new GUIContent
				(
					text: "Frequency"
				);
				FrequencyScalar = new GUIContent
				(
					text: "Scale",
					tooltip: "Overall frequency of the noise."
				);
				FrequencyVector = new GUIContent
				(
					text: "Vector",
					tooltip: "Per axis frequency of the noise."
				);
				Offset = new GUIContent
				(
					text: "Offset"
				);
				OffsetVector = new GUIContent
				(
					text: "Offset",
					tooltip: "Per axis noise offset."
				);
				OffsetSpeedScalar = new GUIContent
				(
					text: "Speed",
					tooltip: "Total change of the offset per second."
				);
				OffsetSpeedVector = new GUIContent
				(
					text: "Velocity",
					tooltip: "Per axis change of the offset per second."
				);
				Axis = DeformEditorGUIUtility.DefaultContent.Axis;
			}
		}

		private class Properties
		{
			public SerializedProperty 
				Mode, 
				MagnitudeScalar,
				MagnitudeVector, 
				FrequencyScalar,
				FrequencyVector,
				OffsetVector, 
				OffsetSpeedScalar, 
				OffsetSpeedVector, 
				Axis;

			public void Update (SerializedObject obj)
			{
				Mode				= obj.FindProperty ("mode");
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
			EditorGUILayout.PropertyField (properties.Mode, content.Mode);

			EditorGUILayout.LabelField (content.Magnitude);

			using (new EditorGUI.IndentLevelScope ())
			{
				EditorGUILayout.PropertyField (properties.MagnitudeScalar, content.MagnitudeScalar);

				using (new EditorGUI.DisabledScope (!properties.Mode.hasMultipleDifferentValues && properties.Mode.enumValueIndex != 0))
					EditorGUILayout.PropertyField (properties.MagnitudeVector, content.MagnitudeVector);
			}

			EditorGUILayout.LabelField (content.Frequency);

			using (new EditorGUI.IndentLevelScope ())
			{
				EditorGUILayout.PropertyField (properties.FrequencyScalar, content.FrequencyScalar);
				EditorGUILayout.PropertyField (properties.FrequencyVector, content.FrequencyVector);
			}

			EditorGUILayout.LabelField (content.Offset);

			using (new EditorGUI.IndentLevelScope ())
			{
				EditorGUIUtility.wideMode = true;
				EditorGUILayout.PropertyField (properties.OffsetVector, content.OffsetVector);
				EditorGUIUtility.wideMode = false;

				EditorGUILayout.PropertyField (properties.OffsetSpeedScalar, content.OffsetSpeedScalar);

				EditorGUIUtility.wideMode = true;
				EditorGUILayout.PropertyField (properties.OffsetSpeedVector, content.OffsetSpeedVector);
				EditorGUIUtility.wideMode = false;
			}

			EditorGUILayout.PropertyField (properties.Axis, content.Axis);
			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}