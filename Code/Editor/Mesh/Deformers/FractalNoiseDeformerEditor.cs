using UnityEngine;
using UnityEditor;
using Beans.Unity.Editor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (FractalNoiseDeformer))]
	public class FractalNoiseDeformerEditor : NoiseDeformerEditor
	{
		private class Content
		{
			public static readonly GUIContent Octaves = new GUIContent (text: "Octaves", tooltip: "The number of times noise is layered.");
			public static readonly GUIContent Persistance = new GUIContent (text: "Persistance", tooltip: "The multiplier that determines how much the magnitude changes each octave.");
			public static readonly GUIContent Lacunarity = new GUIContent (text: "Lacunarity", tooltip: "The multiplier that determines how much the frequency changes each octave.");
		}

		private class Properties
		{
			public SerializedProperty Octaves;
			public SerializedProperty Persistance;
			public SerializedProperty Lacunarity;

			public Properties (SerializedObject obj)
			{
				Octaves				= obj.FindProperty ("octaves");
				Persistance			= obj.FindProperty ("persistance");
				Lacunarity			= obj.FindProperty ("lacunarity");
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

			EditorGUILayout.Space ();

			serializedObject.UpdateIfRequiredOrScript ();

			EditorGUILayoutx.MinField (properties.Octaves, 1, Content.Octaves);
			EditorGUILayout.PropertyField (properties.Persistance, Content.Persistance);
			EditorGUILayout.PropertyField (properties.Lacunarity, Content.Lacunarity);

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}