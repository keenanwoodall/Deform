using UnityEngine;
using UnityEditor;
using Beans.Unity.Editor;

namespace DeformEditor
{
	public class CreditsWindow : EditorWindow
	{
		private class Content
		{
			public static readonly GUIContent Title = new GUIContent (text: "Credits");
			public static readonly GUIContent Twitter = new GUIContent (text: "Twitter");
			public static readonly GUIContent GitHub = new GUIContent (text: "GitHub");
			public static readonly GUIContent Website = new GUIContent (text: "Website");
		}

		private class Styles
		{
			public static readonly GUIStyle Title;
			public static readonly GUIStyle CreditsText;
			public static readonly GUIStyle Link;

			static Styles ()
			{
				Title = new GUIStyle (EditorStyles.largeLabel);
				Title.alignment = TextAnchor.MiddleCenter;
				Title.fontStyle = FontStyle.Bold;

				CreditsText = new GUIStyle (EditorStyles.label);
				CreditsText.wordWrap = true;

				Link = new GUIStyle (EditorStyles.label);
				Link.normal.textColor = new Color (0.2f, 0.2f, 1f);
				Link.hover.textColor = new Color (0.5f, 0.5f, 1f);
			}
		}

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

			EditorGUILayout.LabelField
			(
				"Thanks to Thomas Ingram for helping with development.",
				Styles.CreditsText
			);

			EditorGUILayout.Space ();

			EditorGUILayout.LabelField
			(
				"Thanks to Alexander Ameye, David Carney and William Besnard for testing and providing feedback.",
				Styles.CreditsText
			);

			EditorGUILayout.Space ();

			EditorGUILayout.LabelField
			(
				"Developed by Keenan Woodall.",
				Styles.CreditsText
			);

			EditorGUILayout.Space ();

			var twitterRect = GUILayoutUtility.GetRect (Content.Twitter, Styles.Link, GUILayout.ExpandWidth (false));
			if (GUI.Button (twitterRect, Content.Twitter, Styles.Link))
				Application.OpenURL ("https://twitter.com/keenanwoodall");

			var githubRect = GUILayoutUtility.GetRect (Content.GitHub, Styles.Link, GUILayout.ExpandWidth (false));
			if (GUI.Button (githubRect, Content.GitHub, Styles.Link))
				Application.OpenURL ("https://github.com/keenanwoodall");

			var websiteRect = GUILayoutUtility.GetRect (Content.Website, Styles.Link, GUILayout.ExpandWidth (false));
			if (GUI.Button (websiteRect, Content.Website, Styles.Link))
				Application.OpenURL ("http://keenanwoodall.com");
		}
	}
}