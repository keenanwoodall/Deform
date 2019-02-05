using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Object = UnityEngine.Object;

namespace DeformEditor
{
	public class DeformerListEditor : IDisposable
	{
		private const int PADDING = 5;

		private readonly string LIST_TITLE = "Deformers";
		private readonly string ACTIVE_PROP = "Active";
		private readonly string DEFORMER_PROP = "Deformer";

		private class Styles
		{
			public GUIStyle Toggle;

			public Styles ()
			{
				Toggle = new GUIStyle ();
				Toggle.contentOffset = new Vector2 (2, 3);
			}
		}

		private class Content
		{
			public readonly Texture2D ToggleOnTexture = EditorGUIUtility.FindTexture ("animationvisibilitytoggleon");
			public readonly Texture2D ToggleOffTexture = EditorGUIUtility.FindTexture ("animationvisibilitytoggleoff");

			public GUIContent ToggleOn;
			public GUIContent ToggleOff;

			public Content ()
			{
				ToggleOn = new GUIContent (string.Empty, ToggleOnTexture);
				ToggleOff = new GUIContent (string.Empty, ToggleOffTexture);
			}
		}

		private Styles styles = new Styles ();
		private Content content = new Content ();

		private ReorderableList list;

		public DeformerListEditor (SerializedObject serializedObject, SerializedProperty elements)
		{
			#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui += SceneGUI;
			#else
			SceneView.onSceneGUIDelegate += SceneGUI;
			#endif
			
			list = new ReorderableList (serializedObject, elements);

			list.elementHeight = EditorGUIUtility.singleLineHeight;

			list.drawHeaderCallback  += (r) => GUI.Label (r, new GUIContent (LIST_TITLE));
			list.drawElementCallback += (Rect rect, int index, bool isActive, bool isFocused) =>
			{
				if (styles == null)
					styles = new Styles ();
				if (content == null)
					content = new Content ();

				var elementProperty = list.serializedProperty.GetArrayElementAtIndex (index);
				var activeProperty = elementProperty.FindPropertyRelative (ACTIVE_PROP);
				var deformerProperty = elementProperty.FindPropertyRelative (DEFORMER_PROP);

				if (deformerProperty.objectReferenceValue != null)
				{
					var activeRect = new Rect (rect);
					var activeContent = activeProperty.boolValue ? content.ToggleOn : content.ToggleOff;

					activeRect.xMax = activeRect.xMin + EditorGUIUtility.singleLineHeight;
					activeProperty.boolValue = GUI.Toggle (activeRect, activeProperty.boolValue, activeContent, styles.Toggle);
				}

				var objectRect = new Rect (rect);
				objectRect.xMin += EditorGUIUtility.singleLineHeight + PADDING;
				EditorGUI.ObjectField (objectRect, deformerProperty, GUIContent.none);
			};
			list.onSelectCallback += l =>
			{
				if (l.index >= 0)
				{
					var elementProperty = l.serializedProperty.GetArrayElementAtIndex(l.index);
					var deformerProperty = elementProperty.FindPropertyRelative(DEFORMER_PROP);

					if (deformerProperty.objectReferenceValue != null)
					{
						editor = Editor.CreateEditor(deformerProperty.objectReferenceValue);
						
						Type t = deformerProperty.objectReferenceValue.GetType();
						onSceneGUIMethod = editor.GetType().GetMethod("OnSceneGUI", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
						editorLabel = EditorGUIUtility.ObjectContent(deformerProperty.objectReferenceValue, t);
						editorLabel.text = ObjectNames.NicifyVariableName(t.Name);
						return;
					}
				}
				if(editor != null)
					Object.DestroyImmediate(editor, true);
			};
		}

		private GUIContent editorLabel;
		private Editor editor;
		private bool expandedEditor = true;
		private MethodInfo onSceneGUIMethod;

		private void SceneGUI(SceneView sceneView)
		{
			if(expandedEditor)
				onSceneGUIMethod?.Invoke(editor, null);
		}

		public void Dispose()
		{
			if(editor != null)
				Object.DestroyImmediate(editor, true);
			#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= SceneGUI;
			#else
			SceneView.onSceneGUIDelegate -= SceneGUI;
			#endif
		}

		public void DoLayoutList ()
		{
			try
			{
				list.DoLayoutList ();

				if (editor != null)
				{
					DeformEditorGUILayout.DrawSplitter();
					if (DeformEditorGUILayout.DrawHeaderWithFoldout(editorLabel, expandedEditor))
						expandedEditor = !expandedEditor;
					if(expandedEditor)
						editor.OnInspectorGUI();
					DeformEditorGUILayout.DrawSplitter();
				}
			}
			catch (InvalidOperationException)
			{
				var so = list.serializedProperty.serializedObject;
				so.SetIsDifferentCacheDirty ();
				so.Update ();
			}
		}
	}
}