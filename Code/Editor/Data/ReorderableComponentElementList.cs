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
		private Editor selectedComponentInspectorEditor;
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
					SceneView.onSceneGUIDelegate -= SceneGUI;
					SceneView.onSceneGUIDelegate += SceneGUI;

					selectedComponentInspectorEditor.OnInspectorGUI ();
				}
				else
					UnityEngine.Object.DestroyImmediate (selectedComponentInspectorEditor, true);
			};
		}

		private void SceneGUI (SceneView sceneView)
		{
			if (selectedComponentInspectorEditor == null)
				return;
			selectedComponentInspectorEditor.GetType ().GetMethod ("OnSceneGUI", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke (selectedComponentInspectorEditor, null);
			selectedComponentInspectorEditor.Repaint ();
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
			SceneView.onSceneGUIDelegate -= SceneGUI;
			UnityEngine.Object.DestroyImmediate (selectedComponentInspectorEditor, true);
			selectedComponentInspectorEditor = null;
		}
	}
}