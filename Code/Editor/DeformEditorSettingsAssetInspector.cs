using System.Collections.Generic;
using System.Reflection;
using Beans.Unity.Editor;
using Deform;
using UnityEditor;

namespace DeformEditor
{
    [CustomEditor(typeof(DeformEditorSettingsAsset))]
    public class DeformEditorSettingsAssetInspector : Editor
    {
        List<string> collapsedSections = new List<string>();
        
        public override void OnInspectorGUI()
        {
            var targetType = typeof(DeformEditorSettingsAsset);
            serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty iterator = serializedObject.GetIterator();

            bool currentSectionExpanded = true;
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                using (new EditorGUI.DisabledScope(iterator.propertyPath == "m_Script"))
                {
                    var property = iterator;
                    FieldInfo fieldInfo = targetType.GetField(property.propertyPath, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                    if (fieldInfo != null)
                    {
                        var customAttributes = fieldInfo.GetCustomAttributes();

                        foreach (var customAttribute in customAttributes)
                        {
                            if (customAttribute is CollapsibleSection collapsibleSection)
                            {
                                bool collapsed = collapsedSections.Contains(collapsibleSection.Title);
                                currentSectionExpanded = !collapsed;
                                EditorGUI.BeginChangeCheck();
                                bool newExpand = EditorGUILayoutx.FoldoutHeader(collapsibleSection.Title, !collapsed);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    if (newExpand)
                                    {
                                        collapsedSections.Remove(collapsibleSection.Title);
                                    }
                                    else
                                    {
                                        collapsedSections.Add(collapsibleSection.Title);
                                    }
                                }
                            }
                        }
                    }

                    if (currentSectionExpanded)
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}