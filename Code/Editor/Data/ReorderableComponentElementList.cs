using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Beans.Unity.Editor;
using Object = UnityEngine.Object;

namespace DeformEditor
{
	/// <summary>
	/// Draws a reorderable list of IComponentElements.
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

			list.onAddCallback = (list) =>
			{
				var property = list.serializedProperty;

				property.arraySize++;

				// Even though in the DeformerElement class, active defaults to true, serialized bools default to false.
				var lastElement = property.GetArrayElementAtIndex (property.arraySize - 1);
				lastElement.FindPropertyRelative ("active").boolValue = true;
			};

			list.drawHeaderCallback = (r) => GUI.Label (r, new GUIContent ($"{typeof (T).Name}s"));
			list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
			{
				try
				{
					var elementProperty = list.serializedProperty.GetArrayElementAtIndex (index);

					EditorGUI.PropertyField (rect, elementProperty);

					// get the current element's component property
					var componentProperty = elementProperty.FindPropertyRelative ("component");

					if (componentProperty == null)
					{
						elementProperty.serializedObject.SetIsDifferentCacheDirty ();
						elementProperty.serializedObject.Update ();

						componentProperty = elementProperty.FindPropertyRelative ("component");
					}

					// and the property's object reference
					var component = (Component)componentProperty.objectReferenceValue;

					// if the current element is selected
					if (!componentProperty.hasMultipleDifferentValues && index == list.index && component != null)
					{
						// create it's editor and draw it
						Editor.CreateCachedEditor (component, null, ref selectedComponentInspectorEditor);
#if UNITY_2019_1_OR_NEWER
                        SceneView.duringSceneGui -= SceneGUI;
                        SceneView.duringSceneGui += SceneGUI;
#else
                        SceneView.onSceneGUIDelegate -= SceneGUI;
						SceneView.onSceneGUIDelegate += SceneGUI;
#endif

                        var foldoutName = $"{ObjectNames.NicifyVariableName (componentProperty.objectReferenceValue.GetType ().Name)} Properties";
						using (var foldout = new EditorGUILayoutx.FoldoutContainerScope (list.serializedProperty, foldoutName, DeformEditorResources.GetStyle ("Box"), EditorStyles.foldout))
						{
							if (foldout.isOpen)
							{
								selectedComponentInspectorEditor.OnInspectorGUI ();
							}
						}
					}
				}
				catch (NullReferenceException)
				{
					list.serializedProperty.serializedObject.SetIsDifferentCacheDirty ();
					list.serializedProperty.serializedObject.Update ();
				}
			};
		}

		private void SceneGUI (SceneView sceneView)
		{
			if (selectedComponentInspectorEditor == null || selectedComponentInspectorEditor.target == null)
				return;
			var method = selectedComponentInspectorEditor.GetType().GetMethod("OnSceneGUI", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (method == null)
				return;
			method.Invoke (selectedComponentInspectorEditor, null);
			selectedComponentInspectorEditor.Repaint ();
		}

		public void DoLayoutList ()
		{
			try
			{
				// list.DoLayoutList doesn't support indenting so list.DoList will be used with a manually indented rect.
				var rect = GUILayoutUtility.GetRect (1, list.GetHeight ());
				rect = EditorGUI.IndentedRect (rect);
				list.DoList (rect);
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
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= SceneGUI;
#else
            SceneView.onSceneGUIDelegate -= SceneGUI;
#endif
			Object.DestroyImmediate (selectedComponentInspectorEditor, true);
			selectedComponentInspectorEditor = null;
		}
	}
}