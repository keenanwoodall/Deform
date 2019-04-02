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

			public static readonly GUIStyle Toggle = DeformEditorResources.GetStyle ("Visibility Toggle");
		}

		public override void OnGUI (Rect rect, SerializedProperty property, GUIContent label)
		{
			using (new EditorGUI.PropertyScope (rect, label, property))
			{
				var activeProperty = property.FindPropertyRelative ("active");
				var componentProperty = property.FindPropertyRelative ("component");

				var toggleRect = new Rect (rect);
				toggleRect.xMax = toggleRect.xMin + EditorGUIUtility.singleLineHeight;

				using (new EditorGUI.DisabledScope (componentProperty.objectReferenceValue == null))
					activeProperty.boolValue = GUI.Toggle (toggleRect, activeProperty.boolValue, GUIContent.none, Content.Toggle);

				var componentRect = new Rect (rect);
				// While the toggle rect needs to be indented manually, the object field rect is indented within the supplied rect.
				// For the left of the object field rect to actually be flush to the right side of the toggle rect, we need to find the size of the indent.
				var indentDelta = EditorGUI.IndentedRect (rect).xMin - rect.xMin;
				componentRect.xMin = toggleRect.xMax + Content.Padding - indentDelta;

				EditorGUI.ObjectField (componentRect, componentProperty, GUIContent.none);
			}
		}
	}
}