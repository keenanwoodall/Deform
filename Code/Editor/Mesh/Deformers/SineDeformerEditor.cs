using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (SineDeformer)), CanEditMultipleObjects]
	public class SineDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				Frequency, 
				Magnitude, 
				Falloff, 
				Offset, 
				Speed, 
				Axis;

			public void Update ()
			{
				Frequency = new GUIContent
				(
					text: "Frequency",
					tooltip: "Number of crests and troughs per unit."
				);
				Magnitude = new GUIContent
				(
					text: "Magnitude",
					tooltip: "The strength of the wave."
				);
				Falloff = new GUIContent
				(
					text: "Falloff",
					tooltip: "How quickly the magnitude decreases over distance along the axis."
				);
				Offset = new GUIContent
				(
					text: "Offset",
					tooltip: "The phase shift of the wave."
				);
				Speed = new GUIContent
				(
					text: "Speed",
					tooltip: "How much the phase shift changes per second."
				);
				Axis = DeformEditorGUIUtility.DefaultContent.Axis;
			}
		}

		private class Properties
		{
			public SerializedProperty 
				Frequency,
				Magnitude,
				Falloff, 
				Offset, 
				Speed, 
				Axis;

			public void Update (SerializedObject obj)
			{
				Frequency	= obj.FindProperty ("frequency");
				Magnitude	= obj.FindProperty ("magnitude");
				Falloff		= obj.FindProperty ("falloff");
				Offset		= obj.FindProperty ("offset");
				Speed		= obj.FindProperty ("speed");
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

			var firstSine = target as SineDeformer;

			EditorGUILayout.PropertyField (properties.Frequency, content.Frequency);
			EditorGUILayout.PropertyField (properties.Magnitude, content.Magnitude);
			DeformEditorGUILayout.MinField (properties.Falloff, 0f, content.Falloff);
			EditorGUILayout.PropertyField (properties.Offset, content.Offset);
			EditorGUILayout.PropertyField (properties.Speed, content.Speed);
			EditorGUILayout.PropertyField (properties.Axis, content.Axis);

			serializedObject.ApplyModifiedProperties ();
			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void OnSceneGUI ()
		{
			if (target == null)
				return;

			var sine = target as SineDeformer;

			DrawFrequencyHandle (sine);
			DrawMagnitudeHandle (sine);
			DrawGuides (sine);
			DrawCurve (sine);

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void DrawFrequencyHandle (SineDeformer sine)
		{
			var direction = sine.Axis.forward;
			var frequencyHandleWorldPosition = sine.Axis.position + direction * (2f / sine.Frequency);

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newFrequencyWorldPosition = DeformHandles.Slider (frequencyHandleWorldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (sine, "Changed Frequency");
					var newFrequency = 2f / DeformHandlesUtility.DistanceAlongAxis (sine.Axis, sine.Axis.position, newFrequencyWorldPosition, Axis.Z);
					sine.Frequency = newFrequency;
				}
			}
		}

		private void DrawMagnitudeHandle (SineDeformer sine)
		{
			var direction = sine.Axis.up;
			var magnitudeHandleWorldPosition = sine.Axis.position + direction * sine.Magnitude;

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newMagnitudeWorldPosition = DeformHandles.Slider (magnitudeHandleWorldPosition, direction);
				if (check.changed)
				{
					Undo.RecordObject (sine, "Changed Magnitude");
					var newMagnitude = DeformHandlesUtility.DistanceAlongAxis (sine.Axis, sine.Axis.position, newMagnitudeWorldPosition, Axis.Y);
					sine.Magnitude = newMagnitude;
				}
			}
		}

		private void DrawGuides (SineDeformer sine)
		{
			var direction = sine.Axis.forward;
			var distance = (sine.Frequency != 0f) ? 2f / sine.Frequency : 100f;

			var baseA = sine.Axis.position - direction * distance;
			var baseB = sine.Axis.position + direction * distance;

			DeformHandles.Line (baseA, baseB, DeformHandles.LineMode.LightDotted);
			DeformHandles.Line (baseA + sine.Axis.up * sine.Magnitude, baseB + sine.Axis.up * sine.Magnitude, DeformHandles.LineMode.LightDotted);
			DeformHandles.Line (baseA - sine.Axis.up * sine.Magnitude, baseB - sine.Axis.up * sine.Magnitude, DeformHandles.LineMode.LightDotted);
		}

		private void DrawCurve (SineDeformer sine)
		{
			var forward = sine.Axis.forward;
			var a = sine.Axis.position - (forward * (2f / sine.Frequency));
			var b = sine.Axis.position + (forward * (2f / sine.Frequency));

			var pointSet = false;
			var lastPointOnCurve = Vector3.zero;

			for (int i = 0; i <= DeformHandles.DEF_CURVE_SEGMENTS; i++)
			{
				var pointOnLine = sine.Axis.worldToLocalMatrix.MultiplyPoint3x4 (Vector3.Lerp (a, b, i / (float)DeformHandles.DEF_CURVE_SEGMENTS));
				var newPointOnCurve = pointOnLine + Vector3.up * Mathf.Sin ((pointOnLine.z * sine.Frequency + sine.GetTotalOffset ()) * Mathf.PI * 2f);
				newPointOnCurve.y *= sine.Magnitude;
				newPointOnCurve.y *= Mathf.Exp (-sine.Falloff * Mathf.Abs (newPointOnCurve.z));
				newPointOnCurve = sine.Axis.localToWorldMatrix.MultiplyPoint3x4 (newPointOnCurve);
				if (pointSet)
					DeformHandles.Line (lastPointOnCurve, newPointOnCurve, DeformHandles.LineMode.Solid);
				lastPointOnCurve = newPointOnCurve;
				pointSet = true;
			}
		}
	}
}