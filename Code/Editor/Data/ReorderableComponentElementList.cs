using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace DeformEditor
{
	/// <summary>
	/// Draws a reorderabe list of IComponentElements.
	/// </summary>
	/// <typeparam name="T">The type of component the element holds.</typeparam>
	public class ReorderableComponentElementList<T> where T : Component
	{
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
				EditorGUI.PropertyField (rect, list.serializedProperty.GetArrayElementAtIndex (index));
			};
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
	}
}