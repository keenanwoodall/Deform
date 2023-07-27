using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Deform;
using Unity.Mathematics;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;

namespace DeformEditor
{
    [CustomEditor(typeof(LatticeDeformer))]
    public class LatticeDeformerEditor : DeformerEditor
    {
        private Vector3Int newResolution;

        private float3 handleScale = Vector3.one;
        private Tool activeTool = Tool.None;

        enum MouseDragState
        {
            NotActive,
            Eligible,
            InProgress
        }

        private MouseDragState mouseDragState = MouseDragState.NotActive;
        private Vector2 mouseDownPosition;
        private int previousSelectionCount = 0;

        // Positions of selected points before a rotate or scale begins
        private List<float3> selectedOriginalPositions = new List<float3>();
        
        // Positions and resolution before a resize
        private float3[] cachedResizePositions = new float3[0];
        private Vector3Int cachedResizeResolution;
        
        [SerializeField] private List<int> selectedIndices = new List<int>();

        private static class Content
        {
            public static readonly GUIContent Resolution = new GUIContent(text: "Resolution", tooltip: "Per axis control point counts, the higher the resolution the more splits");
            public static readonly GUIContent Mode = new GUIContent(text: "Mode", tooltip: "Mode by which vertices are positioned between control points");
            public static readonly GUIContent StopEditing = new GUIContent(text: "Stop Editing Control Points", tooltip: "Restore normal transform tools\n\nShortcut: Escape");
        }

        private class Properties
        {
            public SerializedProperty Resolution;
            public SerializedProperty Mode;

            public Properties(SerializedObject obj)
            {
                Resolution = obj.FindProperty("resolution");
                Mode = obj.FindProperty("mode");
            }
        }

        private Properties properties;

        protected override void OnEnable()
        {
            base.OnEnable();

            properties = new Properties(serializedObject);
            
            LatticeDeformer latticeDeformer = ((LatticeDeformer) target);
            newResolution = latticeDeformer.Resolution;
            CacheResizePositionsFromChange();
            
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        private void UndoRedoPerformed()
        {
            LatticeDeformer latticeDeformer = ((LatticeDeformer) target);
            newResolution = latticeDeformer.Resolution;
            CacheResizePositionsFromChange();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            LatticeDeformer latticeDeformer = ((LatticeDeformer) target);

            serializedObject.UpdateIfRequiredOrScript();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(properties.Mode);
            newResolution = EditorGUILayout.Vector3IntField(Content.Resolution, newResolution);
            // Make sure we have at least two control points per axis
            newResolution = Vector3Int.Max(newResolution, new Vector3Int(2, 2, 2));
            // Don't let the lattice resolution get ridiculously high
            newResolution = Vector3Int.Min(newResolution, new Vector3Int(32, 32, 32));

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Update Lattice");
                latticeDeformer.GenerateControlPoints(newResolution, cachedResizePositions, cachedResizeResolution);
                selectedIndices.Clear();
            }

            if (GUILayout.Button("Reset Lattice Points"))
            {
                Undo.RecordObject(target, "Reset Lattice Points");
                latticeDeformer.GenerateControlPoints(newResolution);
                selectedIndices.Clear();
                
                CacheResizePositionsFromChange();
            }

            if (latticeDeformer.CanAutoFitBounds)
            {
                if (GUILayout.Button("Auto-Fit Bounds"))
                {
                    Undo.RecordObject(latticeDeformer.transform, "Auto-Fit Bounds");
                    latticeDeformer.FitBoundsToParentDeformable();
                }
            }

            serializedObject.ApplyModifiedProperties();

            EditorApplication.QueuePlayerLoopUpdate();
        }

        public override void OnSceneGUI()
        {
            base.OnSceneGUI();

            LatticeDeformer lattice = target as LatticeDeformer;
            Transform transform = lattice.transform;
            float3[] controlPoints = lattice.ControlPoints;
            Event e = Event.current;

            using (new Handles.DrawingScope(transform.localToWorldMatrix))
            {
                var cachedZTest = Handles.zTest;

                // Change the depth testing to only show handles in front of solid objects (i.e. typical depth testing) 
                Handles.zTest = CompareFunction.LessEqual;
                DrawLattice(lattice, DeformHandles.LineMode.Solid);
                // Change the depth testing to only show handles *behind* solid objects 
                Handles.zTest = CompareFunction.Greater;
                DrawLattice(lattice, DeformHandles.LineMode.Light);

                // Restore the original z test value now we're done with our drawing
                Handles.zTest = cachedZTest;

                var resolution = lattice.Resolution;
                for (int z = 0; z < resolution.z; z++)
                {
                    for (int y = 0; y < resolution.y; y++)
                    {
                        for (int x = 0; x < resolution.x; x++)
                        {
                            var controlPointHandleID = GUIUtility.GetControlID("LatticeDeformerControlPoint".GetHashCode(), FocusType.Passive);
                            var activeColor = DeformEditorSettings.SolidHandleColor;
                            var controlPointIndex = lattice.GetIndex(x, y, z);

                            if (GUIUtility.hotControl == controlPointHandleID || selectedIndices.Contains(controlPointIndex))
                            {
                                activeColor = Handles.selectedColor;
                            }
                            else if (HandleUtility.nearestControl == controlPointHandleID)
                            {
                                activeColor = Handles.preselectionColor;
                            }

                            if (e.type == EventType.MouseDown && HandleUtility.nearestControl == controlPointHandleID && e.button == 0 && MouseActionAllowed)
                            {
                                BeginSelectionChangeRegion();
                                GUIUtility.hotControl = controlPointHandleID;
                                GUIUtility.keyboardControl = controlPointHandleID;
                                e.Use();

                                bool modifierKeyPressed = e.control || e.shift || e.command;

                                if (modifierKeyPressed && selectedIndices.Contains(controlPointIndex))
                                {
                                    // Pressed a modifier key so toggle the selection
                                    selectedIndices.Remove(controlPointIndex);
                                }
                                else
                                {
                                    if (!modifierKeyPressed)
                                    {
                                        selectedIndices.Clear();
                                    }

                                    if (!selectedIndices.Contains(controlPointIndex))
                                    {
                                        selectedIndices.Add(controlPointIndex);
                                    }
                                }

                                EndSelectionChangeRegion();
                            }

                            if (Tools.current != Tool.None && selectedIndices.Count != 0)
                            {
                                // If the user changes tool, change our internal mode to match but disable the corresponding Unity tool
                                // (e.g. they hit W key or press on the Rotate Tool button on the top left toolbar) 
                                activeTool = Tools.current;
                                Tools.current = Tool.None;
                            }

                            using (new Handles.DrawingScope(activeColor))
                            {
                                var position = controlPoints[controlPointIndex];
                                var size = HandleUtility.GetHandleSize(position) * DeformEditorSettings.ScreenspaceLatticeHandleCapSize;

                                Handles.DotHandleCap(
                                    controlPointHandleID,
                                    position,
                                    Quaternion.identity,
                                    size,
                                    e.type);
                            }
                        }
                    }
                }
            }

            var defaultControl = DeformUnityObjectSelection.DisableSceneViewObjectSelection();

            if (selectedIndices.Count != 0)
            {
                var currentPivotPosition = float3.zero;

                if (Tools.pivotMode == PivotMode.Center)
                {
                    // Get the average position
                    foreach (var index in selectedIndices)
                    {
                        currentPivotPosition += controlPoints[index];
                    }

                    currentPivotPosition /= selectedIndices.Count;
                }
                else
                {
                    // Match the scene view behaviour that Pivot mode uses the last selected object as pivot
                    currentPivotPosition = controlPoints[selectedIndices.Last()];
                }

                float3 handlePosition = transform.TransformPoint(currentPivotPosition);

                if (e.type == EventType.MouseDown)
                {
                    // Potentially started interacting with a handle so reset everything
                    handleScale = Vector3.one;
                    // Make sure we cache the positions just before the interaction changes them
                    CacheOriginalPositions();
                }

                var originalPivotPosition = float3.zero;

                if (Tools.pivotMode == PivotMode.Center)
                {
                    // Get the average position
                    foreach (var originalPosition in selectedOriginalPositions)
                    {
                        originalPivotPosition += originalPosition;
                    }

                    originalPivotPosition /= selectedIndices.Count;
                }
                else
                {
                    // Match the scene view behaviour that Pivot mode uses the last selected object as pivot
                    originalPivotPosition = selectedOriginalPositions.LastOrDefault();
                }

                var handleRotation = transform.rotation;
                if (Tools.pivotRotation == PivotRotation.Global)
                {
                    handleRotation = Quaternion.identity;
                }

                if (activeTool == Tool.Move)
                {
                    EditorGUI.BeginChangeCheck();
                    float3 newPosition = Handles.PositionHandle(handlePosition, handleRotation);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Update Lattice");

                        var delta = newPosition - handlePosition;
                        delta = transform.InverseTransformVector(delta);
                        foreach (var selectedIndex in selectedIndices)
                        {
                            controlPoints[selectedIndex] += delta;
                        }
                        
                        CacheResizePositionsFromChange();
                    }
                }
                else if (activeTool == Tool.Rotate)
                {
                    EditorGUI.BeginChangeCheck();
                    quaternion newRotation = Handles.RotationHandle(handleRotation, handlePosition);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Update Lattice");

                        for (var index = 0; index < selectedIndices.Count; index++)
                        {
                            if (Tools.pivotRotation == PivotRotation.Global)
                            {
                                controlPoints[selectedIndices[index]] = originalPivotPosition + (float3) transform.InverseTransformDirection(mul(newRotation, transform.TransformDirection(selectedOriginalPositions[index] - originalPivotPosition)));
                            }
                            else
                            {
                                controlPoints[selectedIndices[index]] = originalPivotPosition + mul(mul(inverse(handleRotation), newRotation), (selectedOriginalPositions[index] - originalPivotPosition));
                            }
                        }
                        
                        CacheResizePositionsFromChange();
                    }
                }
                else if (activeTool == Tool.Scale)
                {
                    var size = HandleUtility.GetHandleSize(handlePosition);
                    EditorGUI.BeginChangeCheck();
                    handleScale = Handles.ScaleHandle(handleScale, handlePosition, handleRotation, size);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Update Lattice");

                        for (var index = 0; index < selectedIndices.Count; index++)
                        {
                            if (Tools.pivotRotation == PivotRotation.Global)
                            {
                                controlPoints[selectedIndices[index]] = originalPivotPosition + (float3) transform.InverseTransformDirection(handleScale * transform.TransformDirection(selectedOriginalPositions[index] - originalPivotPosition));
                            }
                            else
                            {
                                controlPoints[selectedIndices[index]] = originalPivotPosition + handleScale * (selectedOriginalPositions[index] - originalPivotPosition);
                            }
                        }
                        
                        CacheResizePositionsFromChange();
                    }
                }

                Handles.BeginGUI();
                if (GUI.Button(new Rect((EditorGUIUtility.currentViewWidth - 200) / 2, SceneView.currentDrawingSceneView.position.height - 60, 200, 30), Content.StopEditing))
                {
                    DeselectAll();
                }

                Handles.EndGUI();
            }

            if (e.button == 0) // Left Mouse Button
            {
                if (e.type == EventType.MouseDown && HandleUtility.nearestControl == defaultControl && MouseActionAllowed)
                {
                    mouseDownPosition = e.mousePosition;
                    mouseDragState = MouseDragState.Eligible;
                }
                else if (e.type == EventType.MouseDrag && mouseDragState == MouseDragState.Eligible)
                {
                    mouseDragState = MouseDragState.InProgress;
                    SceneView.currentDrawingSceneView.Repaint();
                }
                else if (GUIUtility.hotControl == 0 &&
                         (e.type == EventType.MouseUp
                          || (mouseDragState == MouseDragState.InProgress && e.rawType == EventType.MouseUp))) // Have they released the mouse outside the scene view while doing marquee select?
                {
                    if (mouseDragState == MouseDragState.InProgress)
                    {
                        var mouseUpPosition = e.mousePosition;

                        Rect marqueeRect = Rect.MinMaxRect(Mathf.Min(mouseDownPosition.x, mouseUpPosition.x),
                            Mathf.Min(mouseDownPosition.y, mouseUpPosition.y),
                            Mathf.Max(mouseDownPosition.x, mouseUpPosition.x),
                            Mathf.Max(mouseDownPosition.y, mouseUpPosition.y));

                        BeginSelectionChangeRegion();

                        if (!e.shift && !e.control && !e.command)
                        {
                            selectedIndices.Clear();
                        }

                        for (var index = 0; index < controlPoints.Length; index++)
                        {
                            Camera camera = SceneView.currentDrawingSceneView.camera;
                            var screenPoint = DeformEditorGUIUtility.WorldToGUIPoint(camera, transform.TransformPoint(controlPoints[index]));

                            if (screenPoint.z < 0)
                            {
                                // Don't consider points that are behind the camera
                                continue;
                            }

                            if (marqueeRect.Contains(screenPoint))
                            {
                                if (e.control || e.command) // Remove selection
                                {
                                    selectedIndices.Remove(index);
                                }
                                else
                                {
                                    selectedIndices.Add(index);
                                }
                            }
                        }

                        EndSelectionChangeRegion();
                    }
                    else
                    {
                        if (selectedIndices.Count == 0) // This shouldn't be called if you have any points selected (we want to allow you to deselect the points)
                        {
                            DeformUnityObjectSelection.AttemptMouseUpObjectSelection();
                        }
                        else
                        {
                            DeselectAll();
                        }
                    }

                    mouseDragState = MouseDragState.NotActive;
                }
            }

            if (e.type == EventType.Repaint && mouseDragState == MouseDragState.InProgress)
            {
                var mouseUpPosition = e.mousePosition;

                Rect marqueeRect = Rect.MinMaxRect(Mathf.Min(mouseDownPosition.x, mouseUpPosition.x),
                    Mathf.Min(mouseDownPosition.y, mouseUpPosition.y),
                    Mathf.Max(mouseDownPosition.x, mouseUpPosition.x),
                    Mathf.Max(mouseDownPosition.y, mouseUpPosition.y));
                DeformUnityObjectSelection.DrawUnityStyleMarquee(marqueeRect);
                SceneView.RepaintAll();
            }

            // If the lattice is visible, override Unity's built-in Select All so that it selects all control points 
            if (DeformUnityObjectSelection.SelectAllPressed)
            {
                BeginSelectionChangeRegion();
                selectedIndices.Clear();
                var resolution = lattice.Resolution;
                for (int z = 0; z < resolution.z; z++)
                {
                    for (int y = 0; y < resolution.y; y++)
                    {
                        for (int x = 0; x < resolution.x; x++)
                        {
                            var controlPointIndex = lattice.GetIndex(x, y, z);
                            selectedIndices.Add(controlPointIndex);
                        }
                    }
                }

                EndSelectionChangeRegion();

                e.Use();
            }

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                DeselectAll();
            }

            EditorApplication.QueuePlayerLoopUpdate();
        }

        private void DeselectAll()
        {
            BeginSelectionChangeRegion();
            selectedIndices.Clear();
            EndSelectionChangeRegion();
        }

        private void BeginSelectionChangeRegion()
        {
            Undo.RecordObject(this, "Selection Change");
            previousSelectionCount = selectedIndices.Count;
        }

        private void EndSelectionChangeRegion()
        {
            if (selectedIndices.Count != previousSelectionCount)
            {
                if (selectedIndices.Count != 0 && previousSelectionCount == 0 && Tools.current == Tool.None) // Is this our first selection?
                {
                    // Make sure when we start selecting control points we actually have a useful tool equipped
                    activeTool = Tool.Move;
                }
                else if (selectedIndices.Count == 0 && previousSelectionCount != 0)
                {
                    // If we have deselected we should probably restore the active tool from before
                    Tools.current = activeTool;
                }
                
                // Selected positions have changed so make sure we're up to date
                CacheOriginalPositions();

                // Different UI elements may be visible depending on selection count, so redraw when it changes
                Repaint();
            }
        }

        private void CacheOriginalPositions()
        {
            // Cache the selected control point positions before the interaction, so that all handle
            // transformations are done using the original values rather than compounding error each frame
            var latticeDeformer = (target as LatticeDeformer);
            float3[] controlPoints = latticeDeformer.ControlPoints;
            selectedOriginalPositions.Clear();
            foreach (int selectedIndex in selectedIndices)
            {
                selectedOriginalPositions.Add(controlPoints[selectedIndex]);
            }
        }

        private void CacheResizePositionsFromChange()
        {
            var latticeDeformer = (target as LatticeDeformer);
            float3[] controlPoints = latticeDeformer.ControlPoints;
            cachedResizePositions = new float3[controlPoints.Length];
            controlPoints.CopyTo(cachedResizePositions, 0);

            cachedResizeResolution = latticeDeformer.Resolution;
        }

        private static bool MouseActionAllowed
        {
            get
            {
                if (Event.current.alt) return false;

                return true;
            }
        }

        private void DrawLattice(LatticeDeformer lattice, DeformHandles.LineMode lineMode)
        {
            var resolution = lattice.Resolution;
            var controlPoints = lattice.ControlPoints;
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

                        DeformHandles.Line(controlPoints[index000], controlPoints[index100], lineMode);
                        DeformHandles.Line(controlPoints[index010], controlPoints[index110], lineMode);
                        DeformHandles.Line(controlPoints[index001], controlPoints[index101], lineMode);
                        DeformHandles.Line(controlPoints[index011], controlPoints[index111], lineMode);

                        DeformHandles.Line(controlPoints[index000], controlPoints[index010], lineMode);
                        DeformHandles.Line(controlPoints[index100], controlPoints[index110], lineMode);
                        DeformHandles.Line(controlPoints[index001], controlPoints[index011], lineMode);
                        DeformHandles.Line(controlPoints[index101], controlPoints[index111], lineMode);

                        DeformHandles.Line(controlPoints[index000], controlPoints[index001], lineMode);
                        DeformHandles.Line(controlPoints[index100], controlPoints[index101], lineMode);
                        DeformHandles.Line(controlPoints[index010], controlPoints[index011], lineMode);
                        DeformHandles.Line(controlPoints[index110], controlPoints[index111], lineMode);
                    }
                }
            }
        }
    }
}