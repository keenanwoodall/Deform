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
		}

		private class Styles
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
				"Developed by Keenan Woodall.",
				Styles.CreditsText
			);

			EditorGUILayout.Space ();

			EditorGUILayout.LabelField
			(
				"Thanks to Thomas Ingram for helping with development.",
				Styles.CreditsText
			);

			EditorGUILayout.Space ();

			EditorGUILayout.LabelField
			(
				"Thanks to David Carney and William Besnard for testing and providing feedback.",
				Styles.CreditsText
			);
		}
	}
}