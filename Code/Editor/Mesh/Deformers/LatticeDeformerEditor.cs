using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Beans.Unity.Editor;
using Deform;

namespace DeformEditor
{
    [CustomEditor(typeof(LatticeDeformer)), CanEditMultipleObjects]
    public class LatticeDeformerEditor : DeformerEditor
    {
        private static class Content
        {
            public static readonly GUIContent Corners = new GUIContent(text: "Corners", tooltip: "The lattice control points");
        }

        private class Properties
        {
            public SerializedProperty Corners;

            public Properties(SerializedObject obj)
            {
                Corners = obj.FindProperty("corners");
            }
        }

        private Properties properties;

        protected override void OnEnable()
        {
            base.OnEnable();

            properties = new Properties(serializedObject);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(properties.Corners, Content.Corners, true);

            serializedObject.ApplyModifiedProperties();

            EditorApplication.QueuePlayerLoopUpdate();
        }

        public override void OnSceneGUI()
        {
            base.OnSceneGUI();

            var lattice = target as LatticeDeformer;

            Handles.matrix = lattice.transform.localToWorldMatrix;
            var corners = lattice.Corners;
            DeformHandles.Line(corners[0], corners[1], DeformHandles.LineMode.SolidDotted);
            DeformHandles.Line(corners[1], corners[2], DeformHandles.LineMode.SolidDotted);
            DeformHandles.Line(corners[2], corners[3], DeformHandles.LineMode.SolidDotted);
            DeformHandles.Line(corners[3], corners[0], DeformHandles.LineMode.SolidDotted);

            DeformHandles.Line(corners[4], corners[5], DeformHandles.LineMode.SolidDotted);
            DeformHandles.Line(corners[5], corners[6], DeformHandles.LineMode.SolidDotted);
            DeformHandles.Line(corners[6], corners[7], DeformHandles.LineMode.SolidDotted);
            DeformHandles.Line(corners[7], corners[4], DeformHandles.LineMode.SolidDotted);

            DeformHandles.Line(corners[0], corners[4], DeformHandles.LineMode.SolidDotted);
            DeformHandles.Line(corners[1], corners[5], DeformHandles.LineMode.SolidDotted);
            DeformHandles.Line(corners[2], corners[6], DeformHandles.LineMode.SolidDotted);
            DeformHandles.Line(corners[3], corners[7], DeformHandles.LineMode.SolidDotted);

            EditorApplication.QueuePlayerLoopUpdate();
        }
    }
}