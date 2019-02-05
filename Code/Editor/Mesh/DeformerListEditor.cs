using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Deform;
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

		private readonly ReorderableList list;
		private Styles styles = new Styles ();
		private Content content = new Content ();
		//The selected Deformer's Editor variables
		private Deformer selectedDeformer;
		private GUIContent selectedEditorLabel;
		private Editor selectedEditor;
		[SerializeField]
		private bool selectedEditorExpanded = true;
		private MethodInfo selectedEditorOnSceneGUI;

		public DeformerListEditor (SerializedObject serializedObject, SerializedProperty elements)
		{
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui += SceneGUI;
#else
			SceneView.onSceneGUIDelegate += SceneGUI;
#endif

			list = new ReorderableList (serializedObject, elements);
			list.elementHeight = EditorGUIUtility.singleLineHeight;

			list.drawHeaderCallback += (r) => GUI.Label (r, new GUIContent (LIST_TITLE));
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
				//On select, create the Editor for the selected Deformer so it can be displayed below the list.
				if (l.index >= 0)
				{
					var elementProperty = l.serializedProperty.GetArrayElementAtIndex (l.index);
					var deformerProperty = elementProperty.FindPropertyRelative (DEFORMER_PROP);

					if (deformerProperty.objectReferenceValue != null)
					{
						selectedDeformer = (Deformer)deformerProperty.objectReferenceValue;
						//Create the editor
						if (selectedEditor != null)
							Object.DestroyImmediate (selectedEditor, true);
						selectedEditor = Editor.CreateEditor (deformerProperty.objectReferenceValue);

						//Get the OnSceneGUI method so it can be called from this editor
						selectedEditorOnSceneGUI = selectedEditor.GetType ().GetMethod ("OnSceneGUI", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

						//Create a label with icon for the foldout
						Type t = deformerProperty.objectReferenceValue.GetType ();
						selectedEditorLabel = EditorGUIUtility.ObjectContent (deformerProperty.objectReferenceValue, t);
						selectedEditorLabel.text = ObjectNames.NicifyVariableName (t.Name);
						return;
					}
				}
				if (selectedEditor != null)
					Object.DestroyImmediate (selectedEditor, true);
			};
		}

		private void SceneGUI (SceneView sceneView)
		{
			if (selectedDeformer == null)
				DisposeSelectedEditor ();

			//Display the selected Editor's OnSceneGUI content if expanded
			if (selectedEditorExpanded)
			{
				selectedEditorOnSceneGUI?.Invoke (selectedEditor, null);
				if (selectedDeformer != null)
					DeformHandles.TransformToolHandle (selectedDeformer.transform, 0.5f);
			}
		}

		public void Dispose ()
		{
			DisposeSelectedEditor ();

			//Remove scene view delegates
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= SceneGUI;
#else
			SceneView.onSceneGUIDelegate -= SceneGUI;
#endif
		}

		/// <summary>
		/// Cleanup the instantiated Editor ScriptableObject
		/// and reset of any other related content
		/// </summary>
		void DisposeSelectedEditor ()
		{
			if (selectedEditor != null)
				Object.DestroyImmediate (selectedEditor, true);
			selectedEditorOnSceneGUI = null;
		}

		public void DoLayoutList ()
		{

			try
			{
				list.DoLayoutList ();
			}
			catch (InvalidOperationException)
			{
				var so = list.serializedProperty.serializedObject;
				so.SetIsDifferentCacheDirty ();
				so.Update ();
			}


			if (selectedEditor != null)
			{
				if (list.index < 0)
				{
					//Cleanup the Editor if it has become deselected via a means that does not fire the selected callback
					//This could be when scripts recompile or an undo is made
					DisposeSelectedEditor ();
					return;
				}
				//Draw the foldout and InspectorGUI for the selected Editor.
				DeformEditorGUILayout.DrawSplitter ();
				if (DeformEditorGUILayout.DrawHeaderWithFoldout (selectedEditorLabel, selectedEditorExpanded))
					selectedEditorExpanded = !selectedEditorExpanded;
				if (selectedEditorExpanded)
					selectedEditor.OnInspectorGUI ();
				DeformEditorGUILayout.DrawSplitter ();
			}
		}
	}
}