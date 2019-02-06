using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace DeformEditor
{
	public static class DeformEditorGUILayout
	{
		public static void Splitter ()
		{
			var rect = GUILayoutUtility.GetRect (1f, 1f);
			if (Event.current.type != EventType.Repaint)
				return;
			rect.xMin = 0f;
			rect.xMax = Screen.width;
			var color = DeformEditorGUIUtility.EditorLowlightColor;
			color.a = GUI.color.a;
			EditorGUI.DrawRect (rect, color);
		}

		public class FoldoutWideScope : System.IDisposable
		{
			private static GUIStyle DefaultLabelStyle;
			static FoldoutWideScope ()
			{
				DefaultLabelStyle = new GUIStyle (EditorStyles.label);
				DefaultLabelStyle.alignment = TextAnchor.MiddleLeft;
			}

			public bool isOpen;
			private readonly string text;

			public FoldoutWideScope (ref bool isOpen, string text) : this (ref isOpen, text, DefaultLabelStyle) { }
			public FoldoutWideScope (ref bool isOpen, string text, GUIStyle labelStyle)
			{
				this.isOpen = isOpen;
				this.text = text;

				Splitter ();

				var toggleRect = GUILayoutUtility.GetRect (1, EditorGUIUtility.singleLineHeight);
				toggleRect.xMin = 0;
				toggleRect.xMax = Screen.width;
				EditorGUI.DrawRect (toggleRect, DeformEditorGUIUtility.EditorHighlightColor);
				EditorGUI.indentLevel++;
				EditorGUI.LabelField (toggleRect, text, labelStyle);
				isOpen = EditorGUI.Foldout (toggleRect, isOpen, GUIContent.none, true);
				EditorGUI.indentLevel--;
			}

			public void Dispose ()
			{
				Splitter ();
			}
		}

		public class FoldoutContainerScope : System.IDisposable
		{
			public static GUIStyle DefaultContainerStyle;
			public static GUIStyle DefaultLabelStyle;

			static FoldoutContainerScope ()
			{
				DefaultContainerStyle = new GUIStyle (EditorStyles.helpBox);
				DefaultLabelStyle = new GUIStyle (EditorStyles.foldout);
				DefaultLabelStyle.font = EditorStyles.boldFont;
			}

			public bool isOpen;
			private readonly string text;

			public FoldoutContainerScope (ref bool isOpen, string text) : this (ref isOpen, text, DefaultContainerStyle, DefaultLabelStyle) { }
			public FoldoutContainerScope (ref bool isOpen, string text, GUIStyle containerStyle, GUIStyle labelStyle)
			{
				this.isOpen = isOpen;
				this.text = text;
				EditorGUI.indentLevel++;
				EditorGUILayout.BeginVertical (containerStyle);
				GUILayout.Space (3);
				isOpen = EditorGUI.Foldout (EditorGUILayout.GetControlRect (), isOpen, text, true, labelStyle);
			}

			public void Dispose ()
			{
				GUILayout.Space (3);
				EditorGUILayout.EndVertical ();
				EditorGUI.indentLevel--;
			}
		}

		/// <summary>
		/// Draws a property field for a float and prevents it from going below min.
		/// </summary>
		public static void MinField (SerializedProperty value, float min)
		{
			EditorGUILayout.PropertyField (value);
			value.floatValue = Mathf.Max (value.floatValue, min);
		}

		public static void MinField (SerializedProperty value, float min, GUIContent content)
		{
			EditorGUILayout.PropertyField (value, content);
			value.floatValue = Mathf.Max (value.floatValue, min);
		}

		/// <summary>
		/// Draws a property field for a float and prevents it from going above max.
		/// </summary>
		public static void MaxField (SerializedProperty value, float max)
		{
			EditorGUILayout.PropertyField (value);
			value.floatValue = Mathf.Min (value.floatValue, max);
		}

		public static void MaxField (SerializedProperty value, float max, GUIContent content)
		{
			EditorGUILayout.PropertyField (value, content);
			value.floatValue = Mathf.Min (value.floatValue, max);
		}

		/// <summary>
		/// Draws a property field for an int and prevents it from going below min.
		/// </summary>
		public static void MinField (SerializedProperty value, int min)
		{
			EditorGUILayout.PropertyField (value);
			value.intValue = Mathf.Max (value.intValue, min);
		}

		public static void MinField (SerializedProperty value, int min, GUIContent content)
		{
			EditorGUILayout.PropertyField (value, content);
			value.intValue = Mathf.Max (value.intValue, min);
		}

		/// <summary>
		/// Draws a property field for an int and prevents it from going above max.
		/// </summary>
		public static void MaxField (SerializedProperty value, int max)
		{
			EditorGUILayout.PropertyField (value);
			value.intValue = Mathf.Min (value.intValue, max);
		}

		public static void MaxField (SerializedProperty value, int max, GUIContent content)
		{
			EditorGUILayout.PropertyField (value, content);
			value.intValue = Mathf.Min (value.intValue, max);
		}

		/// <summary>
		/// Draws a warning HelpBox that says "WIP."
		/// </summary>
		public static void WIPAlert ()
		{
			EditorGUILayout.HelpBox ("WIP", MessageType.Warning, true);
		}

		/// <summary>
		/// Draws a drag n drop area and returns a list of any added components of type T.
		/// </summary>
		public static List<T> DragAndDropComponentArea<T> () where T : Component
		{
			var e = Event.current;

			var dropRect = GUILayoutUtility.GetRect (0f, 25f, GUILayout.ExpandWidth (true));
			GUI.Box (dropRect, string.Empty);
			GUI.Label (dropRect, $"Drag {typeof (T).Name}s Here", EditorStyles.centeredGreyMiniLabel);

			switch (e.type)
			{
				case EventType.DragUpdated:
				case EventType.DragPerform:
					if (!dropRect.Contains (e.mousePosition))
						return null;

					var components = new List<T> ();
					foreach (var o in DragAndDrop.objectReferences)
					{
						if (o is T)
							components.Add ((T)o);
						else if (o is GameObject)
							components.AddRange (((GameObject)o).GetComponents<T> ());
					}

					if (components.Count > 0)
						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					else
						DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;

					if (e.type == EventType.DragPerform && components.Count > 0)
					{
						DragAndDrop.AcceptDrag ();
						return components;
					}

					break;
			}

			return null;
		}
	}
}