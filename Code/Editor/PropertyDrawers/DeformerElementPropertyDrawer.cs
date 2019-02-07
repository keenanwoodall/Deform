using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomPropertyDrawer (typeof (DeformerElement))]
	public class DeformerElementPropertyDrawer : PropertyDrawer
	{
		private static class Content
		{
			public const int MARGIN = 5;

			public static readonly Texture2D ToggleOnTexture = EditorGUIUtility.FindTexture ("animationvisibilitytoggleon");
			public static readonly Texture2D ToggleOffTexture = EditorGUIUtility.FindTexture ("animationvisibilitytoggleoff");

			public static readonly GUIContent ToggleOn = new GUIContent (string.Empty, ToggleOnTexture);
			public static readonly GUIContent ToggleOff = new GUIContent (string.Empty, ToggleOffTexture);

			public static readonly GUIStyle Toggle = new GUIStyle ();

			static Content ()
			{
				Toggle.contentOffset = new Vector2 (2, 3);
			}
		}

		public override void OnGUI (Rect rect, SerializedProperty property, GUIContent label)
		{
			using (new EditorGUI.PropertyScope (rect, label, property))
			{
				var activeProperty = property.FindPropertyRelative ("active");
				var componentProperty = property.FindPropertyRelative ("component");

				using (new EditorGUI.DisabledScope (componentProperty.objectReferenceValue == null))
				{
					var activeRect = new Rect (rect);
					var activeContent = activeProperty.boolValue ? Content.ToggleOn : Content.ToggleOff;

					activeRect.xMax = activeRect.xMin + EditorGUIUtility.singleLineHeight;
					activeProperty.boolValue = GUI.Toggle (activeRect, activeProperty.boolValue, activeContent, Content.Toggle);
				}

				var objectRect = new Rect (rect);
				objectRect.xMin += EditorGUIUtility.singleLineHeight + Content.MARGIN;
				EditorGUI.ObjectField (objectRect, componentProperty, GUIContent.none);
			}
		}
	}
}