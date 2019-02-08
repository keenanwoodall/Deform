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
			public const int Padding = 5;

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
			var indentedRect = EditorGUI.IndentedRect (rect);
			using (new EditorGUI.PropertyScope (rect, label, property))
			{
				var activeProperty = property.FindPropertyRelative ("active");
				var componentProperty = property.FindPropertyRelative ("component");

				var toggleRect = new Rect (indentedRect);
				toggleRect.xMax = toggleRect.xMin + EditorGUIUtility.singleLineHeight;

				using (new EditorGUI.DisabledScope (componentProperty.objectReferenceValue == null))
				{
					var activeContent = activeProperty.boolValue ? Content.ToggleOn : Content.ToggleOff;
					activeProperty.boolValue = GUI.Toggle (toggleRect, activeProperty.boolValue, activeContent, Content.Toggle);
				}

				var componentRect = new Rect (rect);
				// While the toggle rect needs to be indented manually, the object field rect is indented within the supplied rect.
				// For the left of the object field rect to actually be flush to the right side of the toggle rect, we need to find the size of the indent.
				var indentDelta = indentedRect.xMin - rect.xMin;
				componentRect.xMin = toggleRect.xMax + Content.Padding - indentDelta;

				EditorGUI.ObjectField (componentRect, componentProperty, GUIContent.none);
			}
		}
	}
}