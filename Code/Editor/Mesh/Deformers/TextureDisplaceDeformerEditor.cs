using System.Linq;
using UnityEngine;
using UnityEditor;
using Beans.Unity.Editor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (TextureDisplaceDeformer)), CanEditMultipleObjects]
	public class TextureDisplaceDeformerEditor : DeformerEditor
	{
		private class Content
		{
			public static readonly string NotReadableWarning = "Texture is not marked as readable.";

			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent Repeat = new GUIContent (text: "Repeat");
			public static readonly GUIContent Space = new GUIContent (text: "Space");
			public static readonly GUIContent Channel = new GUIContent (text: "Channel");
			public static readonly GUIContent Offset = new GUIContent (text: "Offset");
			public static readonly GUIContent Tiling = new GUIContent (text: "Tiling");
			public static readonly GUIContent Texture = new GUIContent (text: "Texture");
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private class Properties
		{
			public SerializedProperty Factor;
			public SerializedProperty Repeat;
			public SerializedProperty Space;
			public SerializedProperty Channel;
			public SerializedProperty Offset;
			public SerializedProperty Tiling;
			public SerializedProperty Texture;
			public SerializedProperty Axis;

			public Properties (SerializedObject obj)
			{
				Factor	= obj.FindProperty ("factor");
				Repeat	= obj.FindProperty ("repeat");
				Space	= obj.FindProperty ("space");
				Channel = obj.FindProperty ("channel");
				Offset	= obj.FindProperty ("offset");
				Tiling	= obj.FindProperty ("tiling");
				Texture = obj.FindProperty ("texture");
				Axis	= obj.FindProperty ("axis");
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

			EditorGUILayout.PropertyField (properties.Factor, Content.Factor);
			EditorGUILayout.PropertyField (properties.Repeat, Content.Repeat);
			EditorGUILayout.PropertyField (properties.Space, Content.Space);
			EditorGUILayout.PropertyField (properties.Channel, Content.Channel);
			EditorGUILayout.PropertyField (properties.Offset, Content.Offset);
			EditorGUILayout.PropertyField (properties.Tiling, Content.Tiling);

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				EditorGUILayout.PropertyField (properties.Texture, Content.Texture);
				if (check.changed)
				{
					// need to apply properties early if the texture was changed so that the texture's value change occurs before Initialize()
					serializedObject.ApplyModifiedProperties ();
					foreach (var t in targets)
						((TextureDisplaceDeformer)t).Initialize ();
				}
			}

			EditorGUILayout.PropertyField (properties.Axis, Content.Axis);

			serializedObject.ApplyModifiedProperties ();

			if (targets.Where (t => ((TextureDisplaceDeformer)t).Texture != null).Any (t => !((TextureDisplaceDeformer)t).Texture.isReadable))
				EditorGUILayout.HelpBox (Content.NotReadableWarning, MessageType.Error, true);

			EditorGUILayoutx.WIPAlert ();

			EditorApplication.QueuePlayerLoopUpdate ();
		}
	}
}