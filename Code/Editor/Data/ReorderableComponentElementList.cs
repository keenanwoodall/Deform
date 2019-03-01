using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Object = UnityEngine.Object;
using Beans.Unity.Editor;

namespace DeformEditor
{
	/// <summary>
	/// Draws a reorderabe list of IComponentElements.
	/// </summary>
	/// <typeparam name="T">The type of component the element holds.</typeparam>
	public class ReorderableComponentElementList<T> : IDisposable where T : Component
	{
		private readonly ReorderableList list;
		private Editor selectedComponentInspectorEditor;

		/// <summary>
		/// Make sure your implementation of IComponentElement has a PropertyDrawer and 
		/// serialized fields for for the component reference and active bool called "component" and "active".
		/// </summary>
		public ReorderableComponentElementList (SerializedObject serializedObject, SerializedProperty elements)
		{
			list = new ReorderableList (serializedObject, elements);
			list.elementHeight = EditorGUIUtility.singleLineHeight;

			list.drawHeaderCallback = (r) => GUI.Label (r, new GUIContent ($"{typeof (T).Name}s"));
			list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
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
					SceneView.onSceneGUIDelegate -= SceneGUI;
					SceneView.onSceneGUIDelegate += SceneGUI;

					using (var foldout = new EditorGUILayoutx.FoldoutContainerScope (list.serializedProperty, $"{component.name} Properties"))
					{
						if (foldout.isOpen)
							selectedComponentInspectorEditor.OnInspectorGUI ();
					}
				}
			};
		}

		private void SceneGUI (SceneView sceneView)
		{
			selectedComponentInspectorEditor?.GetType ().GetMethod ("OnSceneGUI", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke (selectedComponentInspectorEditor, null);
			selectedComponentInspectorEditor.Repaint ();
		}

		public void DoLayoutList ()
		{
			try
			{
				list.DoLayoutList ();
			}
			// If an error is thrown, the serialized object was modified but not marked as dirty so we need to force it to sync back up with the targets.
			catch (Exception e)
			{
				if (e is InvalidOperationException || e is NullReferenceException)
				{
					var so = list.serializedProperty.serializedObject;
					so.SetIsDifferentCacheDirty ();
					so.Update ();
				}
			}
		}

		public void Dispose ()
		{
			SceneView.onSceneGUIDelegate -= SceneGUI;
			Object.DestroyImmediate (selectedComponentInspectorEditor, true);
			selectedComponentInspectorEditor = null;
		}
	}
}