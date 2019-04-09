using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Beans.Unity.Editor
{
	public static class EditorGUILayoutx
	{
		private static class Styles
		{
			public static readonly Color SplitterDarkColor = new Color (0.12f, 0.12f, 0.12f, 1.333f);
			public static readonly Color SplitterLightColor = new Color (0.6f, 0.6f, 0.6f, 1.333f);

			public static readonly Color HeaderBackgroundDarkColor = new Color (0.1f, 0.1f, 0.1f, 0.2f);
			public static readonly Color HeaderBackgroundLightColor = new Color (1f, 1f, 1f, 0.2f);

			public static readonly Color LinkColor = new Color (0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);

			public static readonly GUIStyle Link;

			static Styles ()
			{
				Link = new GUIStyle (EditorStyles.label);
				Link.wordWrap = false;
				Link.normal.textColor = LinkColor;
				Link.stretchWidth = false;
			}
		}

		public static bool LinkLabel (GUIContent label, params GUILayoutOption[] options) => LinkLabel (label, Styles.Link, options);
		public static bool LinkLabel (GUIContent label, GUIStyle style, params GUILayoutOption[] options)
		{
			var rect = GUILayoutUtility.GetRect (label, style, options);

			Handles.BeginGUI ();
			Handles.color = style.normal.textColor;
			Handles.DrawLine (new Vector3 (rect.xMin, rect.yMax), new Vector3 (rect.xMax, rect.yMax));
			Handles.color = Color.white;
			Handles.EndGUI ();

			EditorGUIUtility.AddCursorRect (rect, MouseCursor.Link);

			return GUI.Button (rect, label, style);
		}

		public static void Splitter (bool wide = true)
		{
			var rect = GUILayoutUtility.GetRect (1f, 1f);

			if (wide)
			{
				rect.xMin = 0f;
				rect.width += 4f;
			}

			if (Event.current.type != EventType.Repaint)
				return;

			EditorGUI.DrawRect (rect, EditorGUIUtility.isProSkin ? Styles.SplitterLightColor : Styles.SplitterDarkColor);
		}

		public static bool FoldoutHeader (string title, bool foldout) => FoldoutHeader (title, foldout, EditorStyles.boldLabel);
		public static bool FoldoutHeader (string title, bool foldout, GUIStyle headerStyle)
		{
			var backgroundRect = GUILayoutUtility.GetRect (1, 17);

			var labelRect = backgroundRect;
			labelRect.xMin += 16f;
			labelRect.xMax -= 20f;

			var foldoutRect = backgroundRect;
			foldoutRect.y += 1f;
			foldoutRect.width = 13f;
			foldoutRect.height = 13f;

			// Background rect should be full-width
			backgroundRect.xMin = 0f;
			backgroundRect.width += 4f;

			// Background
			EditorGUI.DrawRect (backgroundRect, EditorGUIUtility.isProSkin ? Styles.HeaderBackgroundDarkColor : Styles.HeaderBackgroundLightColor);

			// Title
			EditorGUI.LabelField (labelRect, EditorGUIUtility.TrTextContent (title), headerStyle);

			// Foldout
			foldout = GUI.Toggle (foldoutRect, foldout, GUIContent.none, EditorStyles.foldout);

			var e = Event.current;
			if (e.type == EventType.MouseDown && backgroundRect.Contains (e.mousePosition) && e.button == 0)
			{
				foldout = !foldout;
				e.Use ();
			}

			return foldout;
		}

		public class FoldoutContainerScope : System.IDisposable
		{
			private static readonly GUIStyle DefaultContainerStyle;
			private static readonly GUIStyle DefaultLabelStyle;

			static FoldoutContainerScope ()
			{
				DefaultContainerStyle = GUI.skin.FindStyle ("Box");
				DefaultLabelStyle = new GUIStyle (EditorStyles.foldout);
			}

			public bool isOpen { get; private set; }
			private int lastIndentLevel = 0;

			public FoldoutContainerScope (ref bool isOpen, string text) : this (ref isOpen, text, DefaultContainerStyle, DefaultLabelStyle) { }
			public FoldoutContainerScope (ref bool isOpen, string text, GUIStyle containerStyle, GUIStyle labelStyle)
			{
				this.isOpen = isOpen;
				lastIndentLevel = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 1;
				EditorGUILayout.BeginVertical (containerStyle);
				GUILayout.Space (3);
				isOpen = EditorGUI.Foldout (EditorGUILayout.GetControlRect (), isOpen, text, true, labelStyle);
			}

			public FoldoutContainerScope (SerializedProperty isExpanded, string text) : this (isExpanded, text, DefaultContainerStyle, DefaultLabelStyle) { }
			public FoldoutContainerScope (SerializedProperty isExpanded, string text, GUIStyle containerStyle, GUIStyle labelStyle)
			{
				this.isOpen = isExpanded.isExpanded;
				lastIndentLevel = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 1;
				EditorGUILayout.BeginVertical (containerStyle);
				GUILayout.Space (3);
				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					isOpen = EditorGUI.Foldout (EditorGUILayout.GetControlRect (), isOpen, text, true, labelStyle);
					if (check.changed)
					{
						isExpanded.serializedObject.ApplyModifiedPropertiesWithoutUndo ();
						isExpanded.isExpanded = isOpen;
					}
				}
			}

			public void Dispose ()
			{
				GUILayout.Space (3);
				EditorGUILayout.EndVertical ();
				EditorGUI.indentLevel = lastIndentLevel;
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
		public static List<T> DragAndDropArea<T> (params GUILayoutOption[] options) where T : Object
		{
			var e = Event.current;

			var dropRect = GUILayoutUtility.GetRect (0f, 25f, options);
			dropRect = EditorGUI.IndentedRect (dropRect);

			GUI.Box (dropRect, string.Empty);
			GUI.Label (dropRect, $"+ Drag {typeof (T).Name}s Here", EditorStyles.centeredGreyMiniLabel);

			switch (e.type)
			{
				case EventType.DragUpdated:
				case EventType.DragPerform:
					if (!dropRect.Contains (e.mousePosition))
						return null;

					var dropped = new List<T> ();
					foreach (var obj in DragAndDrop.objectReferences)
					{
						if (obj is T instance)
							dropped.Add (instance);
						else if (typeof (T).IsSubclassOf (typeof (Component)) && obj is GameObject gameObject)
							dropped.AddRange (gameObject.GetComponents<T> ());
					}

					if (dropped.Count > 0)
						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					else
						DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;

					if (e.type == EventType.DragPerform && dropped.Count > 0)
					{
						DragAndDrop.AcceptDrag ();
						return dropped;
					}

					break;
			}

			return null;
		}
	}
}