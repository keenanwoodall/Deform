using UnityEngine;
using UnityEditor;
using Beans.Unity.Editor;

namespace DeformEditor
{
	public class CreditsWindow : EditorWindow
	{
		private static class Content
		{
			public static readonly GUIContent Title = new GUIContent (text: "Credits");
			public static readonly GUIContent Twitter = new GUIContent (text: "Twitter");
			public static readonly GUIContent GitHub = new GUIContent (text: "GitHub");
			public static readonly GUIContent Website = new GUIContent (text: "Website");
		}

		private static class Styles
		{
			public static readonly GUIStyle Title;
			public static readonly GUIStyle CreditsText;

			static Styles ()
			{
				Title = new GUIStyle (EditorStyles.largeLabel);
				Title.alignment = TextAnchor.MiddleCenter;
				Title.fontStyle = FontStyle.Bold;

				CreditsText = new GUIStyle (EditorStyles.label);
				CreditsText.wordWrap = true;
			}
		}

		private Vector2 scrollPosition = Vector2.zero;

		[MenuItem ("Window/Deform/Credits", priority = 10200)]
		[MenuItem ("Tools/Deform/Credits", priority = 10200)]
		public static void ShowWindow ()
		{
			GetWindow<CreditsWindow> ("Credits", true);
		}

		private void OnGUI ()
		{
			EditorGUILayout.LabelField (Content.Title, Styles.Title, GUILayout.ExpandWidth (true));

			EditorGUILayoutx.Splitter ();

			using (new EditorGUILayout.ScrollViewScope (scrollPosition))
			{
				EditorGUILayout.LabelField
				(
					"Developed by Keenan Woodall.",
					Styles.CreditsText
				);

				EditorGUILayout.Space ();

				if (EditorGUILayoutx.LinkLabel (Content.Twitter))
					Application.OpenURL ("https://twitter.com/keenanwoodall");

				if (EditorGUILayoutx.LinkLabel (Content.GitHub))
					Application.OpenURL ("https://github.com/keenanwoodall");

				if (EditorGUILayoutx.LinkLabel (Content.Website))
					Application.OpenURL ("http://keenanwoodall.com");

				EditorGUILayout.Space ();

				EditorGUILayout.LabelField
				(
					"Thanks to Thomas Ingram for helping with development.",
					Styles.CreditsText
				);

				EditorGUILayout.Space ();

				EditorGUILayout.LabelField
				(
					"Thanks to Alexander Ameye, William Besnard, Raphael Herdlicka and David Carney for testing and providing feedback.",
					Styles.CreditsText
				);
			}
		}
	}
}