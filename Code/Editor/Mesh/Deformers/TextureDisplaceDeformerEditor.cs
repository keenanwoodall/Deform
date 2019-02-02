using System.Linq;
using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (TextureDisplaceDeformer)), CanEditMultipleObjects]
	public class TextureDisplaceDeformerEditor : Editor
	{
		private class Content
		{
			public GUIContent 
				Factor, 
				Repeat,
				Mode,
				Offset,
				Tiling, 
				Texture, 
				Axis;

			public void Update ()
			{
				Factor = DeformEditorGUIUtility.DefaultContent.Factor;
				Repeat = new GUIContent
				(
					text: "Repeat"
				);
				Mode = new GUIContent
				(
					text: "Mode"
				);
				Offset = new GUIContent
				(
					text: "Offset"
				);
				Tiling = new GUIContent
				(
					text: "Tiling"
				);
				Texture = new GUIContent
				(
					text: "Texture"
				);
				Axis = DeformEditorGUIUtility.DefaultContent.Axis;
			}
		}

		private class Properties
		{
			public SerializedProperty 
				Factor, 
				Repeat, 
				Mode,
				Offset, 
				Tiling,
				Texture,
				Axis;

			public void Update (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
				Repeat	= obj.FindProperty ("repeat");
				Mode	= obj.FindProperty ("mode");
				Offset	= obj.FindProperty ("offset");
				Tiling	= obj.FindProperty ("tiling");
				Texture = obj.FindProperty ("texture");
				Axis	= obj.FindProperty ("axis");
			}
		}

		private Content content = new Content ();
		private Properties properties = new Properties ();

		private readonly string NotReadableWarning = "Texture is not marked as readable.";

		private void OnEnable ()
		{
			content.Update ();
			properties.Update (serializedObject);
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			EditorGUILayout.PropertyField (properties.Factor, content.Factor);
			EditorGUILayout.PropertyField (properties.Repeat, content.Repeat);
			EditorGUILayout.PropertyField (properties.Mode, content.Mode);
			EditorGUILayout.PropertyField (properties.Offset, content.Offset);
			EditorGUILayout.PropertyField (properties.Tiling, content.Tiling);

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				EditorGUILayout.PropertyField (properties.Texture, content.Texture);
				if (check.changed)
				{
					// need to apply properties early if the texture  was changed so that texture's value change occurs before Initialize()
					serializedObject.ApplyModifiedProperties ();
					foreach (var t in targets)
						((TextureDisplaceDeformer)t).Initialize ();
				}
			}

			EditorGUILayout.PropertyField (properties.Axis, content.Axis);

			if (targets.Where (t => ((TextureDisplaceDeformer)t).Texture != null).Any (t => !((TextureDisplaceDeformer)t).Texture.isReadable))
				EditorGUILayout.HelpBox (NotReadableWarning, MessageType.Error, true);

			DeformEditorGUILayout.WIPAlert ();

			serializedObject.ApplyModifiedProperties ();
			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}