using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace DeformEditor
{
	/// <summary>
	/// Draws a reorderabe list of IComponentElements.
	/// </summary>
	/// <typeparam name="T">The type of component the element holds.</typeparam>
	public class ReorderableComponentElementList<T> : IDisposable where T : Component
	{
		private delegate void SceneGUICallback ();

		private Editor selectedComponentInspectorEditor;
		private SceneGUICallback selectedComponentSceneGUI;
		private readonly ReorderableList list;

		/// <summary>
		/// Make sure your implementation of IComponentElement has a PropertyDrawer.
		/// </summary>
		public ReorderableComponentElementList (SerializedObject serializedObject, SerializedProperty elements)
		{
			list = new ReorderableList (serializedObject, elements);
			list.elementHeight = EditorGUIUtility.singleLineHeight;

			list.drawHeaderCallback += (r) => GUI.Label (r, new GUIContent ($"{typeof (T).Name}s"));
			list.drawElementCallback += (Rect rect, int index, bool isActive, bool isFocused) =>
			{
				var elementProperty = list.serializedProperty.GetArrayElementAtIndex (index);
				EditorGUI.PropertyField (rect, elementProperty);

				// get the current element's component property
				var componentProperty = elementProperty.FindPropertyRelative ("component");
				// and the property's object reference
				var component = componentProperty.objectReferenceValue;
				// if the current element is selected
				if (!componentProperty.hasMultipleDifferentValues && index == list.index && component != null)
				{
					// create it's editor and draw it
					Editor.CreateCachedEditor (component, null, ref selectedComponentInspectorEditor);

					selectedComponentInspectorEditor.OnInspectorGUI ();

					selectedComponentSceneGUI = ((DeformerEditor)selectedComponentInspectorEditor).OnSceneGUI;
					RemoveSceneGUIListener (SceneGUI);
					AddSceneGUIListener (SceneGUI);
				}
				else
					UnityEngine.Object.DestroyImmediate (selectedComponentInspectorEditor, true);
			};
		}

		private void SceneGUI (SceneView sceneView)
		{
			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				selectedComponentSceneGUI ();
				if (check.changed)
					selectedComponentInspectorEditor.Repaint ();
			}
		}

		private void AddSceneGUIListener (UnityEditor.SceneView.OnSceneFunc callback)
		{
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui += callback;
#else
			SceneView.onSceneGUIDelegate += callback;
#endif
		}
		private void RemoveSceneGUIListener (UnityEditor.SceneView.OnSceneFunc callback)
		{
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= callback;
#else
			SceneView.onSceneGUIDelegate -= callback;
#endif
		}

		public void DoLayoutList ()
		{
			try
			{
				list.DoLayoutList ();
			}
			// If an error is thrown, the serialized object was modified but not marked as dirty so we need to force it to sync back up with the targets.
			catch (InvalidOperationException)
			{
				var so = list.serializedProperty.serializedObject;
				so.SetIsDifferentCacheDirty ();
				so.Update ();
			}
		}

		public void Dispose ()
		{
			RemoveSceneGUIListener (SceneGUI);
			UnityEngine.Object.DestroyImmediate (selectedComponentInspectorEditor, true);
			selectedComponentInspectorEditor = null;
		}
	}
}