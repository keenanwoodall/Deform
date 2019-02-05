using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace DeformEditor
{
	public static class DeformEditorGUILayout
	{
		/// <summary>
		/// Draws a property field for a float and prevents it from going below min.
		/// </summary>
		public static void MinField(SerializedProperty value, float min)
		{
			EditorGUILayout.PropertyField(value);
			value.floatValue = Mathf.Max(value.floatValue, min);
		}

		public static void MinField(SerializedProperty value, float min, GUIContent content)
		{
			EditorGUILayout.PropertyField(value, content);
			value.floatValue = Mathf.Max(value.floatValue, min);
		}

		/// <summary>
		/// Draws a property field for a float and prevents it from going above max.
		/// </summary>
		public static void MaxField(SerializedProperty value, float max)
		{
			EditorGUILayout.PropertyField(value);
			value.floatValue = Mathf.Min(value.floatValue, max);
		}

		public static void MaxField(SerializedProperty value, float max, GUIContent content)
		{
			EditorGUILayout.PropertyField(value, content);
			value.floatValue = Mathf.Min(value.floatValue, max);
		}

		/// <summary>
		/// Draws a property field for an int and prevents it from going below min.
		/// </summary>
		public static void MinField(SerializedProperty value, int min)
		{
			EditorGUILayout.PropertyField(value);
			value.intValue = Mathf.Max(value.intValue, min);
		}

		public static void MinField(SerializedProperty value, int min, GUIContent content)
		{
			EditorGUILayout.PropertyField(value, content);
			value.intValue = Mathf.Max(value.intValue, min);
		}

		/// <summary>
		/// Draws a property field for an int and prevents it from going above max.
		/// </summary>
		public static void MaxField(SerializedProperty value, int max)
		{
			EditorGUILayout.PropertyField(value);
			value.intValue = Mathf.Min(value.intValue, max);
		}

		public static void MaxField(SerializedProperty value, int max, GUIContent content)
		{
			EditorGUILayout.PropertyField(value, content);
			value.intValue = Mathf.Min(value.intValue, max);
		}

		/// <summary>
		/// Draws a warning HelpBox that says "WIP."
		/// </summary>
		public static void WIPAlert()
		{
			EditorGUILayout.HelpBox("WIP", MessageType.Warning, true);
		}

		/// <summary>
		/// Draws a drag n drop area and returns a list of any added components of type T.
		/// </summary>
		public static List<T> DragAndDropComponentArea<T>() where T : Component
		{
			var e = Event.current;

			var dropRect = GUILayoutUtility.GetRect(0f, 25f, GUILayout.ExpandWidth(true));
			GUI.Box(dropRect, string.Empty);
			GUI.Label(dropRect, $"Drag {typeof(T).Name}s Here", EditorStyles.centeredGreyMiniLabel);

			switch (e.type)
			{
				case EventType.DragUpdated:
				case EventType.DragPerform:
					if (!dropRect.Contains(e.mousePosition))
						return null;

					var components = new List<T>();
					foreach (var o in DragAndDrop.objectReferences)
					{
						if (o is T)
							components.Add((T) o);
						else if (o is GameObject)
							components.AddRange(((GameObject) o).GetComponents<T>());
					}

					if (components.Count > 0)
						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					else
						DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;

					if (e.type == EventType.DragPerform && components.Count > 0)
					{
						DragAndDrop.AcceptDrag();
						return components;
					}

					break;
			}

			return null;
		}
		
		
		public static bool DrawHeader(GUIContent label)
		{
			Rect r = GUILayoutUtility.GetRect(1, 17);
			return DrawHeader(r, label);
		}

		private static bool DrawHeader(Rect contentRect, GUIContent label)
		{
			Rect labelRect = contentRect;
			labelRect.xMin += 16f;
			labelRect.xMax -= 20f;
			Rect toggleRect = contentRect;
			toggleRect.xMin = EditorGUI.indentLevel* 15;
			toggleRect.y += 2f;
			toggleRect.width = 13f;
			toggleRect.height = 13f;
			contentRect.xMin = 0.0f;
			
			EditorGUI.DrawRect(contentRect, !EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 0.2f) : new Color(0.1f, 0.1f, 0.1f, 0.2f));
			EditorGUI.LabelField(labelRect, label, EditorStyles.boldLabel);
			labelRect.xMin = 0;
			Event current = Event.current;
			if (current.type == EventType.MouseDown)
			{
				if (labelRect.Contains(current.mousePosition))
				{
					if (current.button == 0)
					{
						current.Use();
						return true;
					}
				}
			}
			return false;
		}

		public static bool DrawHeaderWithFoldout(GUIContent label, bool expanded)
		{
			bool ret = DrawHeader(label);
			return Foldout(GUILayoutUtility.GetLastRect(), expanded) || ret;
		}
		
		private static bool Foldout(Rect r, bool expanded)
		{
			switch (Event.current.type) {
				case EventType.DragUpdated:
					if (!expanded) {
						if (r.Contains(Event.current.mousePosition)) {
							if (Event.current.delta.sqrMagnitude < 10)
							{
								Event.current.Use();
								return true;
							}
						}
					}
					break;
				case EventType.Repaint:
					//Only draw the Foldout - don't use it as a button or get focus
					r.x += 3;
					r.x += EditorGUI.indentLevel * 15;
					r.y += 1.5f;
					bool enabledT = GUI.enabled;
					GUI.enabled = false;
					EditorStyles.foldout.Draw(r, GUIContent.none, -1, expanded);
					GUI.enabled = enabledT;
					break;
			}
			return false;
		}
		
		public static void DrawSplitter()
		{
			Rect rect = GUILayoutUtility.GetRect(1f, 1f);
			rect.xMin = 0.0f;
			if (Event.current.type != EventType.Repaint)
				return;
			Color c = EditorGUIUtility.isProSkin ? new Color(0.12f, 0.12f, 0.12f) : new Color(0.6f, 0.6f, 0.6f);
			c.a = GUI.color.a;
			EditorGUI.DrawRect(rect, c);
		}
	}
}