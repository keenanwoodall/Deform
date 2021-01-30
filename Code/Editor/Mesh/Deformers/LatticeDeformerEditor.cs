using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
    [CustomEditor(typeof(LatticeDeformer)), CanEditMultipleObjects]
    public class LatticeDeformerEditor : DeformerEditor
    {
        private Vector3Int newResolution;

        private static class Content
        {
            public static readonly GUIContent Corners = new GUIContent(text: "Corners", tooltip: "The lattice control points");
            public static readonly GUIContent Resolution = new GUIContent(text: "Resolution", tooltip: "Per axis control point counts, the higher the resolution the more splits");
        }

        private class Properties
        {
            public SerializedProperty Corners;
            public SerializedProperty Resolution;

            public Properties(SerializedObject obj)
            {
                Corners = obj.FindProperty("corners");
                Resolution = obj.FindProperty("resolution");
            }
        }

        private Properties properties;

        protected override void OnEnable()
        {
            base.OnEnable();

            properties = new Properties(serializedObject);
            newResolution = properties.Resolution.vector3IntValue;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(properties.Corners, Content.Corners, true);

            newResolution = EditorGUILayout.Vector3IntField(Content.Resolution, newResolution);
            // Make sure we have at least two control points per axis
            newResolution = Vector3Int.Max(newResolution, new Vector3Int(2, 2, 2));
            if (GUILayout.Button("Update"))
            {
                Undo.RecordObject(target, "Update Lattice");
                ((LatticeDeformer) target).GenerateCorners(newResolution);
            }

            serializedObject.ApplyModifiedProperties();

            EditorApplication.QueuePlayerLoopUpdate();
        }

        public override void OnSceneGUI()
        {
            base.OnSceneGUI();

            var lattice = target as LatticeDeformer;

            Handles.matrix = lattice.transform.localToWorldMatrix;
            var corners = lattice.Corners;

            var resolution = lattice.Resolution;
            for (int z = 0; z < resolution.z - 1; z++)
            {
                for (int y = 0; y < resolution.y - 1; y++)
                {
                    for (int x = 0; x < resolution.x - 1; x++)
                    {
                        int index000 = lattice.GetIndex(x, y, z);
                        int index100 = lattice.GetIndex(x + 1, y, z);
                        int index010 = lattice.GetIndex(x, y + 1, z);
                        int index110 = lattice.GetIndex(x + 1, y + 1, z);
                        int index001 = lattice.GetIndex(x, y, z + 1);
                        int index101 = lattice.GetIndex(x + 1, y, z + 1);
                        int index011 = lattice.GetIndex(x, y + 1, z + 1);
                        int index111 = lattice.GetIndex(x + 1, y + 1, z + 1);

                        var lineMode = DeformHandles.LineMode.Solid;
                        DeformHandles.Line(corners[index000], corners[index100], lineMode);
                        DeformHandles.Line(corners[index010], corners[index110], lineMode);
                        DeformHandles.Line(corners[index001], corners[index101], lineMode);
                        DeformHandles.Line(corners[index011], corners[index111], lineMode);

                        DeformHandles.Line(corners[index000], corners[index010], lineMode);
                        DeformHandles.Line(corners[index100], corners[index110], lineMode);
                        DeformHandles.Line(corners[index001], corners[index011], lineMode);
                        DeformHandles.Line(corners[index101], corners[index111], lineMode);

                        DeformHandles.Line(corners[index000], corners[index001], lineMode);
                        DeformHandles.Line(corners[index100], corners[index101], lineMode);
                        DeformHandles.Line(corners[index010], corners[index011], lineMode);
                        DeformHandles.Line(corners[index110], corners[index111], lineMode);


                        //Handles.SphereHandleCap(0, corners[index], Quaternion.identity, 0.1f, EventType.Repaint);
                    }
                }
            }

            EditorApplication.QueuePlayerLoopUpdate();
        }
    }
}