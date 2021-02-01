using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Deform;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace DeformEditor
{
    [CustomEditor(typeof(LatticeDeformer)), CanEditMultipleObjects]
    public class LatticeDeformerEditor : DeformerEditor
    {
        private Vector3Int newResolution;

        HashSet<int> selectedIndices = new HashSet<int>();

        private static class Content
        {
            public static readonly GUIContent Target = new GUIContent(text: "Target", tooltip: DeformEditorGUIUtility.Strings.AxisTooltip);
            public static readonly GUIContent Corners = new GUIContent(text: "Corners", tooltip: "The lattice control points");
            public static readonly GUIContent Resolution = new GUIContent(text: "Resolution", tooltip: "Per axis control point counts, the higher the resolution the more splits");
        }

        private class Properties
        {
            public SerializedProperty Target;
            public SerializedProperty Corners;
            public SerializedProperty Resolution;

            public Properties(SerializedObject obj)
            {
                Target = obj.FindProperty("target");
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

            EditorGUILayout.PropertyField(properties.Target, Content.Target);

            EditorGUILayout.PropertyField(properties.Corners, Content.Corners, true);

            newResolution = EditorGUILayout.Vector3IntField(Content.Resolution, newResolution);
            // Make sure we have at least two control points per axis
            newResolution = Vector3Int.Max(newResolution, new Vector3Int(2, 2, 2));
            // Don't let the lattice resolution get ridiculously high
            newResolution = Vector3Int.Min(newResolution, new Vector3Int(32, 32, 32));

            if (GUILayout.Button("Update Lattice"))
            {
                Undo.RecordObject(target, "Update Lattice");
                ((LatticeDeformer) target).GenerateControlPoints(newResolution, true);
                selectedIndices.Clear();
            }

            if (GUILayout.Button("Reset Lattice Points"))
            {
                Undo.RecordObject(target, "Reset Lattice Points");
                ((LatticeDeformer) target).GenerateControlPoints(newResolution);
                selectedIndices.Clear();
            }

            serializedObject.ApplyModifiedProperties();

            EditorApplication.QueuePlayerLoopUpdate();
        }

        public override void OnSceneGUI()
        {
            base.OnSceneGUI();

            var lattice = target as LatticeDeformer;
            var corners = lattice.Corners;

            using (new Handles.DrawingScope(lattice.transform.localToWorldMatrix))
            {
                var cachedZTest = Handles.zTest;

                // Change the depth testing to only show handles in front of solid objects (i.e. typical depth testing) 
                Handles.zTest = CompareFunction.LessEqual;

                // Draw the lattice
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
                        }
                    }
                }

                // Restore the original z test value now we're done with our drawing
                Handles.zTest = cachedZTest;

                for (int z = 0; z < resolution.z; z++)
                {
                    for (int y = 0; y < resolution.y; y++)
                    {
                        for (int x = 0; x < resolution.x; x++)
                        {
                            var controlPointHandleID = GUIUtility.GetControlID("LatticeDeformerControlPoint".GetHashCode(), FocusType.Passive);
                            var activeColor = DeformEditorSettings.SolidHandleColor;
                            var cornerIndex = lattice.GetIndex(x, y, z);

                            if (GUIUtility.hotControl == controlPointHandleID || selectedIndices.Contains(cornerIndex))
                            {
                                activeColor = Handles.selectedColor;
                            }
                            else if (HandleUtility.nearestControl == controlPointHandleID)
                            {
                                activeColor = Handles.preselectionColor;
                            }

                            Event e = Event.current;
                            if (e.type == EventType.MouseDown && HandleUtility.nearestControl == controlPointHandleID && e.button == 0)
                            {
                                GUIUtility.hotControl = controlPointHandleID;
                                GUIUtility.keyboardControl = controlPointHandleID;
                                e.Use();

                                if ((e.modifiers & EventModifiers.Control) != 0)
                                {
                                    selectedIndices.Remove(cornerIndex);
                                }
                                else
                                {
                                    if ((e.modifiers & EventModifiers.Shift) == 0)
                                    {
                                        selectedIndices.Clear();
                                    }

                                    selectedIndices.Add(cornerIndex);
                                }
                            }

                            using (new Handles.DrawingScope(activeColor))
                            {
                                var position = corners[cornerIndex];
                                var size = HandleUtility.GetHandleSize(position) * DeformEditorSettings.ScreenspaceLatticeHandleCapSize;

                                Handles.DotHandleCap(
                                    controlPointHandleID,
                                    position,
                                    Quaternion.identity,
                                    size,
                                    Event.current.type);
                            }
                        }
                    }
                }
            }

            if (selectedIndices.Count != 0)
            {
                // Make sure when we start selecting control points we don't have a transform tool active as it'll be confusing having multiple ones
                Tools.current = Tool.None;

                // Get the average position
                var position = float3.zero;

                if (Tools.pivotMode == PivotMode.Center)
                {
                    foreach (var index in selectedIndices)
                    {
                        position += corners[index];
                    }

                    position /= selectedIndices.Count;
                }
                else
                {
                    position = corners[selectedIndices.First()];
                }

                position = lattice.Target.TransformPoint(position);

                EditorGUI.BeginChangeCheck();
                var rotation = lattice.Target.rotation;
                if (Tools.pivotRotation == PivotRotation.Global)
                {
                    rotation = Quaternion.identity;
                }

                float3 newPosition = Handles.PositionHandle(position, rotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Update Lattice");

                    var delta = newPosition - position;
                    delta = lattice.Target.InverseTransformVector(delta);
                    foreach (var selectedIndex in selectedIndices)
                    {
                        corners[selectedIndex] += delta;
                    }
                }
            }

            EditorApplication.QueuePlayerLoopUpdate();
        }
    }
}