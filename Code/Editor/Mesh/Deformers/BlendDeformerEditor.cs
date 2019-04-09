using UnityEngine;
using UnityEditor;
using Deform;
using Beans.Unity.Editor;

namespace DeformEditor
{
	[CustomEditor (typeof (BlendDeformer)), CanEditMultipleObjects]
	public class BlendDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent Cache = new GUIContent (text: "Cache", tooltip: "The vertex cache to blend towards. It must have the same vertex count as the deformable.");
		}

		private class Properties
		{
			public SerializedProperty Factor;
			public SerializedProperty Cache;

			public Properties (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
				Cache	= obj.FindProperty ("cache");
			}
		}

		Properties properties;

		protected override void OnEnable ()
		{
			base.OnEnable ();
			properties = new Properties (serializedObject);
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.UpdateIfRequiredOrScript ();

			UnityEditor.EditorGUILayout.Slider (properties.Factor, 0f, 1f, Content.Factor);

			using (var check = new UnityEditor.EditorGUI.ChangeCheckScope ())
			{
				UnityEditor.EditorGUILayout.ObjectField (properties.Cache, Content.Cache);
				if (check.changed)
				{
					// need to apply properties early if the cache was changed so that cache's value change occurs before Initialize()
					serializedObject.ApplyModifiedProperties ();
					foreach (var t in targets)
						((BlendDeformer)t).Initialize ();
				}
			}

			serializedObject.ApplyModifiedProperties ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}