using Deform;
using UnityEditor;
using UnityEngine;

namespace DeformEditor
{
    [CustomPropertyDrawer(typeof(DisplayNameAttribute))]
    public class DisplayNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, ((DisplayNameAttribute) attribute).GUIContent);
        }
    }
}