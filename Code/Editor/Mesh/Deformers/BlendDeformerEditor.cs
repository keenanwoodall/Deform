using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (BlendDeformer)), CanEditMultipleObjects]
	public class BlendDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				Factor, 
				Cache;

			public void Update ()
			{
				Factor = DeformEditorGUIUtility.DefaultContent.Factor;
				Cache = new GUIContent
				(
					text: "Cache",
					tooltip: "The vertex cache to blend towards. It must have the same vertex count as the deformable."
				);
			}
		}

		private class Properties
		{
			public SerializedProperty 
				Factor, 
				Cache;

			public void Update (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
				Cache	= obj.FindProperty ("cache");
			}
		}

		Content content = new Content ();
		Properties properties = new Properties ();

		private void OnEnable ()
		{
			content.Update ();
			properties.Update (serializedObject);
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			EditorGUILayout.Slider (properties.Factor, 0f, 1f, content.Factor);

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				EditorGUILayout.ObjectField (properties.Cache, content.Cache);
				if (check.changed)
				{
					// need to apply properties early if the cache was changed so that cache's value change occurs before Initialize()
					serializedObject.ApplyModifiedProperties ();
					foreach (var t in targets)
						((BlendDeformer)t).Initialize ();
				}
			}

			serializedObject.ApplyModifiedProperties ();
		}
	}
}