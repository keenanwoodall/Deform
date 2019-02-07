using System.Collections.Generic;
using PathCreation;
using PathCreation.Utility;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace PathCreationEditor {
    /// Editor class for the creation of Bezier and Vertex paths

    [CustomEditor (typeof (PathCreator))]
    public class PathEditor : Editor {

        #region Fields

        // Interaction:
        const float segmentSelectDistanceThreshold = 10f;
        const float screenPolylineMaxAngleError = .3f;
        const float screenPolylineMinVertexDst = .01f;
        bool shareTransformsWithPath = false; // Should changes to pathcreator's transform affect the path (and vice versa)

        // Help messages:
        const string helpInfo = "Shift-click to add or insert new points. Control-click to delete points. For more detailed infomation, please refer to the documentation.";
        static readonly string[] spaceNames = { "3D (xyz)", "2D (xy)", "Top-down (xz)" };
        static readonly string[] tabNames = { "Bézier Path", "Vertex Path" };
        const string constantSizeTooltip = "If true, anchor and control points will keep a constant size when zooming in the editor.";

        // Display
        const int inspectorSectionSpacing = 10;
        const float constantHandleScale = .01f;
        const float normalsSpacing = .1f;
        GUIStyle boldFoldoutStyle;

        // References:
        PathCreator creator;
        Editor globalDisplaySettingsEditor;
        ScreenSpacePolyLine screenSpaceLine;
        ScreenSpacePolyLine.MouseInfo pathMouseInfo;
        GlobalDisplaySettings globalDisplaySettings;
        PathHandle.HandleColours splineAnchorColours;
        PathHandle.HandleColours splineControlColours;
        Dictionary<GlobalDisplaySettings.HandleType, Handles.CapFunction> capFunctions;
        ArcHandle anchorAngleHandle = new ArcHandle ();
        VertexPath normalsVertexPath;

        // State variables:
        int selectedSegmentIndex;
        int draggingHandleIndex;
        int mouseOverHandleIndex;
        int handleIndexToDisplayAsTransform;

        bool shiftLastFrame;
        bool hasUpdatedScreenSpaceLine;
        bool hasUpdatedNormalsVertexPath;
        bool editingNormalsOld;

        Vector3 positionOld;
        Quaternion rotationOld;
        Vector3 scaleOld;
        Quaternion currentHandleRot = Quaternion.identity;
        Color handlesStartCol;

        // Constants
        const int bezierPathTab = 0;
        const int vertexPathTab = 1;

        #endregion

        #region Inspectors

        public override void OnInspectorGUI () {
            // Initialize GUI styles
            if (boldFoldoutStyle == null) {
                boldFoldoutStyle = new GUIStyle (EditorStyles.foldout);
                boldFoldoutStyle.fontStyle = FontStyle.Bold;
            }

            Undo.RecordObject (creator, "Path settings changed");

            // Draw Bezier and Vertex tabs
            int tabIndex = GUILayout.Toolbar (data.tabIndex, tabNames);
            if (tabIndex != data.tabIndex) {
                data.tabIndex = tabIndex;
                TabChanged ();
            }

            // Draw inspector for active tab
            switch (data.tabIndex) {
                case bezierPathTab:
                    DrawBezierPathInspector ();
                    break;
                case vertexPathTab:
                    DrawVertexPathInspector ();
                    break;
            }

            // Notify of undo/redo that might modify the path
            if (Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed") {
                data.PathModifiedByUndo ();
            }

            // Update visibility of default transform tool
            UpdateToolVisibility ();
        }

        void DrawBezierPathInspector () {
            using (var check = new EditorGUI.ChangeCheckScope ()) {
                // Path options:
                data.showPathOptions = EditorGUILayout.Foldout (data.showPathOptions, new GUIContent ("Bézier Path Options"), true, boldFoldoutStyle);
                if (data.showPathOptions) {
                    bezierPath.Space = (PathSpace) EditorGUILayout.Popup ("Space", (int) bezierPath.Space, spaceNames);
                    bezierPath.ControlPointMode = (BezierPath.ControlMode) EditorGUILayout.EnumPopup (new GUIContent ("Control Mode"), bezierPath.ControlPointMode);
                    if (bezierPath.ControlPointMode == BezierPath.ControlMode.Automatic) {
                        bezierPath.AutoControlLength = EditorGUILayout.Slider (new GUIContent ("Control Spacing"), bezierPath.AutoControlLength, 0, 1);
                    }

                    bezierPath.IsClosed = EditorGUILayout.Toggle ("Closed Path", bezierPath.IsClosed);
                    data.pathTransformationEnabled = EditorGUILayout.Toggle (new GUIContent ("Enable Transforms"), data.pathTransformationEnabled);

                    // If a point has been selected
                    if (handleIndexToDisplayAsTransform != -1) {
                        EditorGUILayout.LabelField ("Selected Point:");

                        using (new EditorGUI.IndentLevelScope ()) {
                            var currentPosition = creator.bezierPath[handleIndexToDisplayAsTransform];
                            var newPosition = EditorGUILayout.Vector3Field ("Position", currentPosition);
                            if (newPosition != currentPosition) {
                                Undo.RecordObject (creator, "Move point");
                                creator.bezierPath.MovePoint (handleIndexToDisplayAsTransform, newPosition);
                            }
                            // Don't draw the angle field if we aren't selecting an anchor point/not in 3d space
                            if (handleIndexToDisplayAsTransform % 3 == 0 && creator.bezierPath.Space == PathSpace.xyz) {
                                var anchorIndex = handleIndexToDisplayAsTransform / 3;
                                var currentAngle = creator.bezierPath.GetAnchorNormalAngle (anchorIndex);
                                var newAngle = EditorGUILayout.FloatField ("Angle", currentAngle);
                                if (newAngle != currentAngle) {
                                    Undo.RecordObject (creator, "Set Angle");
                                    creator.bezierPath.SetAnchorNormalAngle (anchorIndex, newAngle);
                                }
                            }
                        }
                    }

                    if (GUILayout.Button ("Reset Path")) {
                        Undo.RecordObject (creator, "Reset Path");
                        bool in2DEditorMode = EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode2D;
                        data.ResetBezierPath (creator.transform.position, in2DEditorMode);
                        EditorApplication.QueuePlayerLoopUpdate ();
                    }

                    GUILayout.Space (inspectorSectionSpacing);
                }

                data.showNormals = EditorGUILayout.Foldout (data.showNormals, new GUIContent ("Normals Options"), true, boldFoldoutStyle);
                if (data.showNormals) {
                    bezierPath.FlipNormals = EditorGUILayout.Toggle (new GUIContent ("Flip Normals"), bezierPath.FlipNormals);
                    if (bezierPath.Space == PathSpace.xyz) {
                        bezierPath.GlobalNormalsAngle = EditorGUILayout.Slider (new GUIContent ("Global Angle"), bezierPath.GlobalNormalsAngle, 0, 360);

                        if (GUILayout.Button ("Reset Normals")) {
                            Undo.RecordObject (creator, "Reset Normals");
                            bezierPath.FlipNormals = false;
                            bezierPath.ResetNormalAngles ();
                        }
                    }
                    GUILayout.Space (inspectorSectionSpacing);
                }

                // Editor display options
                data.showDisplayOptions = EditorGUILayout.Foldout (data.showDisplayOptions, new GUIContent ("Display Options"), true, boldFoldoutStyle);
                if (data.showDisplayOptions) {
                    data.showPathBounds = GUILayout.Toggle (data.showPathBounds, new GUIContent ("Show Path Bounds"));
                    data.showPerSegmentBounds = GUILayout.Toggle (data.showPerSegmentBounds, new GUIContent ("Show Segment Bounds"));
                    data.displayAnchorPoints = GUILayout.Toggle (data.displayAnchorPoints, new GUIContent ("Show Anchor Points"));
                    if (!(bezierPath.ControlPointMode == BezierPath.ControlMode.Automatic && globalDisplaySettings.hideAutoControls)) {
                        data.displayControlPoints = GUILayout.Toggle (data.displayControlPoints, new GUIContent ("Show Control Points"));
                    }
                    data.keepConstantHandleSize = GUILayout.Toggle (data.keepConstantHandleSize, new GUIContent ("Constant Point Size", constantSizeTooltip));
                    data.bezierHandleScale = Mathf.Max (0, EditorGUILayout.FloatField (new GUIContent ("Handle Scale"), data.bezierHandleScale));
                    DrawGlobalDisplaySettingsInspector ();
                }

                if (check.changed) {
                    SceneView.RepaintAll ();
                    EditorApplication.QueuePlayerLoopUpdate ();
                }
            }
        }

        void DrawVertexPathInspector () {
            data.showVertexPathOptions = EditorGUILayout.Foldout (data.showVertexPathOptions, new GUIContent ("Vertex Path Options"), true, boldFoldoutStyle);
            if (data.showVertexPathOptions) {
                using (var check = new EditorGUI.ChangeCheckScope ()) {
                    data.vertexPathMaxAngleError = EditorGUILayout.Slider (new GUIContent ("Max Angle Error"), data.vertexPathMaxAngleError, 0, 45);
                    data.vertexPathMinVertexSpacing = EditorGUILayout.Slider (new GUIContent ("Min Vertex Dst"), data.vertexPathMinVertexSpacing, 0, 1);

                    GUILayout.Space (inspectorSectionSpacing);
                    if (check.changed) {
                        data.VertexPathSettingsChanged ();
                        SceneView.RepaintAll ();
                        EditorApplication.QueuePlayerLoopUpdate ();
                    }
                }
            }

            data.showVertexPathDisplayOptions = EditorGUILayout.Foldout (data.showVertexPathDisplayOptions, new GUIContent ("Display Options"), true, boldFoldoutStyle);
            if (data.showVertexPathDisplayOptions) {
                using (var check = new EditorGUI.ChangeCheckScope ()) {
                    data.vertexHandleSize = EditorGUILayout.Slider (new GUIContent ("Vertex Scale"), data.vertexHandleSize, 0, 1);
                    data.showNormalsInVertexMode = GUILayout.Toggle (data.showNormalsInVertexMode, new GUIContent ("Show Normals"));

                    if (check.changed) {
                        SceneView.RepaintAll ();
                        EditorApplication.QueuePlayerLoopUpdate ();
                    }
                }
                DrawGlobalDisplaySettingsInspector ();
            }
        }

        void DrawGlobalDisplaySettingsInspector () {
            using (var check = new EditorGUI.ChangeCheckScope ()) {
                data.globalDisplaySettingsFoldout = EditorGUILayout.InspectorTitlebar (data.globalDisplaySettingsFoldout, globalDisplaySettings);
                if (data.globalDisplaySettingsFoldout) {
                    CreateCachedEditor (globalDisplaySettings, null, ref globalDisplaySettingsEditor);
                    globalDisplaySettingsEditor.OnInspectorGUI ();
                }
                if (check.changed) {
                    UpdateGlobalDisplaySettings ();
                    SceneView.RepaintAll ();
                }
            }
        }

        #endregion

        #region Scene GUI

        void OnSceneGUI () {
            using (var check = new EditorGUI.ChangeCheckScope ()) {
                handlesStartCol = Handles.color;
                switch (data.tabIndex) {
                    case bezierPathTab:
                        ProcessBezierPathInput (Event.current);
                        DrawBezierPathSceneEditor ();
                        break;
                    case vertexPathTab:
                        DrawVertexPathSceneEditor ();
                        break;
                }

                // Don't allow clicking over empty space to deselect the object
                if (Event.current.type == EventType.Layout) {
                    HandleUtility.AddDefaultControl (0);
                }

                if (check.changed) {
                    EditorApplication.QueuePlayerLoopUpdate ();
                }
            }
        }

        void DrawVertexPathSceneEditor () {

            Color bezierCol = globalDisplaySettings.bezierPath;
            bezierCol.a *= .5f;

            for (int i = 0; i < bezierPath.NumSegments; i++) {
                Vector3[] points = bezierPath.GetPointsInSegment (i);
                Handles.DrawBezier (points[0], points[3], points[1], points[2], bezierCol, null, 2);
            }

            Handles.color = globalDisplaySettings.vertexPath;
            for (int i = 0; i < creator.path.NumVertices; i++) {
                int nextIndex = (i + 1) % creator.path.NumVertices;
                if (nextIndex != 0 || bezierPath.IsClosed) {
                    Handles.DrawLine (creator.path.vertices[i], creator.path.vertices[nextIndex]);
                }
            }

            if (data.showNormalsInVertexMode) {
                Handles.color = globalDisplaySettings.normals;
                for (int i = 0; i < creator.path.NumVertices; i++) {
                    Handles.DrawLine (creator.path.vertices[i], creator.path.vertices[i] + creator.path.normals[i] * globalDisplaySettings.normalsLength);
                }
            }

            Handles.color = globalDisplaySettings.vertex;
            for (int i = 0; i < creator.path.NumVertices; i++) {
                Handles.SphereHandleCap (0, creator.path.vertices[i], Quaternion.identity, data.vertexHandleSize * .1f, EventType.Repaint);
            }
        }

        void ProcessBezierPathInput (Event e) {

            // Update path pivot point on mouse up
            if (e.type == EventType.MouseUp) {
                currentHandleRot = Quaternion.identity;
                bezierPath.Pivot = bezierPath.PathBounds.center;
            }

            // Find which handle mouse is over. Start by looking at previous handle index first, as most likely to still be closest to mouse
            int previousMouseOverHandleIndex = (mouseOverHandleIndex == -1) ? 0 : mouseOverHandleIndex;
            mouseOverHandleIndex = -1;
            for (int i = 0; i < bezierPath.NumPoints; i += 3) {
                int handleIndex = (previousMouseOverHandleIndex + i) % bezierPath.NumPoints;
                float handleRadius = GetHandleDiameter (globalDisplaySettings.anchorSize * data.bezierHandleScale, bezierPath[handleIndex]) / 2f;
                float dst = HandleUtility.DistanceToCircle (bezierPath[handleIndex], handleRadius);
                if (dst == 0) {
                    mouseOverHandleIndex = handleIndex;
                    break;
                }
            }

            // Shift-left click (when mouse not over a handle) to split or add segment
            if (mouseOverHandleIndex == -1) {
                if (e.type == EventType.MouseDown && e.button == 0 && e.shift) {
                    UpdatePathMouseInfo ();
                    // Insert point along selected segment
                    if (selectedSegmentIndex != -1 && selectedSegmentIndex < bezierPath.NumSegments) {
                        Vector3 newPathPoint = pathMouseInfo.closestWorldPointToMouse;
                        Undo.RecordObject (creator, "Split segment");
                        bezierPath.SplitSegment (newPathPoint, selectedSegmentIndex, pathMouseInfo.timeOnBezierSegment);
                    }
                    // If path is not a closed loop, add new point on to the end of the path
                    else if (!bezierPath.IsClosed) {
                        // insert new point at same dst from scene camera as the point that comes before it (for a 3d path)
                        float dstCamToEndpoint = (Camera.current.transform.position - bezierPath[bezierPath.NumPoints - 1]).magnitude;
                        Vector3 newPathPoint = MouseUtility.GetMouseWorldPosition (bezierPath.Space, dstCamToEndpoint);

                        Undo.RecordObject (creator, "Add segment");
                        if (e.control || e.command) {
                            bezierPath.AddSegmentToStart (newPathPoint);
                        } else {
                            bezierPath.AddSegmentToEnd (newPathPoint);
                        }

                    }

                }
            }

            // Control click or backspace/delete to remove point
            if (e.keyCode == KeyCode.Backspace || e.keyCode == KeyCode.Delete || ((e.control || e.command) && e.type == EventType.MouseDown && e.button == 0)) {
                if (mouseOverHandleIndex != -1) {
                    Undo.RecordObject (creator, "Delete segment");
                    bezierPath.DeleteSegment (mouseOverHandleIndex);
                    if (mouseOverHandleIndex == handleIndexToDisplayAsTransform) {
                        handleIndexToDisplayAsTransform = -1;
                    }
                    mouseOverHandleIndex = -1;
                    Repaint ();
                }
            }

            // Holding shift and moving mouse (but mouse not over a handle/dragging a handle)
            if (draggingHandleIndex == -1 && mouseOverHandleIndex == -1) {
                bool shiftDown = e.shift && !shiftLastFrame;
                if (shiftDown || ((e.type == EventType.MouseMove || e.type == EventType.MouseDrag) && e.shift)) {

                    UpdatePathMouseInfo ();

                    if (pathMouseInfo.mouseDstToLine < segmentSelectDistanceThreshold) {
                        if (pathMouseInfo.closestSegmentIndex != selectedSegmentIndex) {
                            selectedSegmentIndex = pathMouseInfo.closestSegmentIndex;
                            HandleUtility.Repaint ();
                        }
                    } else {
                        selectedSegmentIndex = -1;
                        HandleUtility.Repaint ();
                    }

                }
            }

            if (shareTransformsWithPath) {
                // Move bezier path if creator's transform position has changed
                if (creator.transform.position != positionOld) {
                    bezierPath.Position += (creator.transform.position - positionOld);
                    positionOld = creator.transform.position;
                }
                // Rotate bezier path if creator's transform rotation has changed
                if (creator.transform.rotation != rotationOld) {
                    bezierPath.Rotation = creator.transform.rotation;
                    creator.transform.rotation = bezierPath.Rotation; // set to constrained value
                    rotationOld = creator.transform.rotation;
                }
                // Scale bezier path if creator's transform scale has changed
                if (creator.transform.localScale != scaleOld) {
                    bezierPath.Scale = creator.transform.localScale;
                    creator.transform.localScale = bezierPath.Scale; // set to constrained value
                    scaleOld = creator.transform.localScale;
                }
            }

            shiftLastFrame = e.shift;

        }

        void DrawBezierPathSceneEditor () {
            bool displayControlPoints = data.displayControlPoints && (bezierPath.ControlPointMode != BezierPath.ControlMode.Automatic || !globalDisplaySettings.hideAutoControls);
            Bounds bounds = bezierPath.PathBounds;

            // Draw normals
            if (data.showNormals) {
                if (!hasUpdatedNormalsVertexPath) {
                    normalsVertexPath = new VertexPath (bezierPath, normalsSpacing);
                    hasUpdatedNormalsVertexPath = true;
                }

                if (editingNormalsOld != data.showNormals) {
                    editingNormalsOld = data.showNormals;
                    Repaint ();
                }

                Handles.color = globalDisplaySettings.normals;
                for (int i = 0; i < normalsVertexPath.NumVertices; i++) {
                    Handles.DrawLine (normalsVertexPath.vertices[i], normalsVertexPath.vertices[i] + normalsVertexPath.normals[i] * globalDisplaySettings.normalsLength);
                }

            }

            for (int i = 0; i < bezierPath.NumSegments; i++) {
                Vector3[] points = bezierPath.GetPointsInSegment (i);

                if (data.showPerSegmentBounds) {
                    Bounds segmentBounds = CubicBezierUtility.CalculateBounds (points);
                    Handles.color = globalDisplaySettings.segmentBounds;
                    Handles.DrawWireCube (segmentBounds.center, segmentBounds.size);
                }

                // Draw lines between control points
                if (displayControlPoints) {
                    Handles.color = (bezierPath.ControlPointMode == BezierPath.ControlMode.Automatic) ? globalDisplaySettings.handleDisabled : globalDisplaySettings.controlLine;
                    Handles.DrawLine (points[1], points[0]);
                    Handles.DrawLine (points[2], points[3]);
                }

                // Draw path
                bool highlightSegment = (i == selectedSegmentIndex && Event.current.shift && draggingHandleIndex == -1 && mouseOverHandleIndex == -1);
                Color segmentCol = (highlightSegment) ? globalDisplaySettings.highlightedPath : globalDisplaySettings.bezierPath;
                Handles.DrawBezier (points[0], points[3], points[1], points[2], segmentCol, null, 2);
            }

            // Draw rotate/scale/move tool
            if (data.pathTransformationEnabled && !Event.current.alt && !Event.current.shift) {
                if (Tools.current == Tool.Rotate) {
                    Undo.RecordObject (creator, "Rotate Path");
                    Quaternion newHandleRot = Handles.DoRotationHandle (currentHandleRot, bezierPath.Pivot);
                    Quaternion deltaRot = newHandleRot * Quaternion.Inverse (currentHandleRot);
                    currentHandleRot = newHandleRot;

                    Quaternion newRot = deltaRot * bezierPath.Rotation;
                    bezierPath.Rotation = newRot;
                    if (shareTransformsWithPath) {
                        creator.transform.rotation = newRot;
                        rotationOld = newRot;
                    }
                } else if (Tools.current == Tool.Scale) {
                    Undo.RecordObject (creator, "Scale Path");
                    bezierPath.Scale = Handles.DoScaleHandle (bezierPath.Scale, bezierPath.Pivot, Quaternion.identity, HandleUtility.GetHandleSize (bezierPath.Pivot));
                    if (shareTransformsWithPath) {
                        creator.transform.localScale = bezierPath.Scale;
                        scaleOld = bezierPath.Scale;
                    }
                } else {
                    Undo.RecordObject (creator, "Move Path");

                    bezierPath.Pivot = bounds.center;
                    Vector3 newCentre = Handles.DoPositionHandle (bezierPath.Pivot, Quaternion.identity);
                    Vector3 deltaCentre = newCentre - bezierPath.Pivot;
                    bezierPath.Position += deltaCentre;
                    if (shareTransformsWithPath) {
                        creator.transform.position = bezierPath.Position;
                        positionOld = bezierPath.Position;
                    }
                }

            }

            if (data.showPathBounds) {
                Handles.color = globalDisplaySettings.bounds;
                Handles.DrawWireCube (bounds.center, bounds.size);
            }

            if (data.displayAnchorPoints) {
                for (int i = 0; i < bezierPath.NumPoints; i += 3) {
                    DrawHandle (i);
                }
            }
            if (displayControlPoints) {
                for (int i = 1; i < bezierPath.NumPoints - 1; i += 3) {
                    DrawHandle (i);
                    DrawHandle (i + 1);
                }
            }
        }

        void DrawHandle (int i) {
            Vector3 handlePosition = bezierPath[i];

            float anchorHandleSize = GetHandleDiameter (globalDisplaySettings.anchorSize * data.bezierHandleScale, bezierPath[i]);
            float controlHandleSize = GetHandleDiameter (globalDisplaySettings.controlSize * data.bezierHandleScale, bezierPath[i]);

            bool isAnchorPoint = i % 3 == 0;
            bool isInteractive = isAnchorPoint || bezierPath.ControlPointMode != BezierPath.ControlMode.Automatic;
            float handleSize = (isAnchorPoint) ? anchorHandleSize : controlHandleSize;
            bool doTransformHandle = i == handleIndexToDisplayAsTransform;

            PathHandle.HandleColours handleColours = (isAnchorPoint) ? splineAnchorColours : splineControlColours;
            if (i == handleIndexToDisplayAsTransform) {
                handleColours.defaultColour = (isAnchorPoint) ? globalDisplaySettings.anchorSelected : globalDisplaySettings.controlSelected;
            }
            var cap = capFunctions[(isAnchorPoint) ? globalDisplaySettings.anchorShape : globalDisplaySettings.controlShape];
            PathHandle.HandleInputType handleInputType;
            handlePosition = PathHandle.DrawHandle (handlePosition, bezierPath.Space, isInteractive, handleSize, cap, handleColours, out handleInputType, i);

            if (doTransformHandle) {
                // Show normals rotate tool 
                if (data.showNormals && Tools.current == Tool.Rotate && isAnchorPoint && bezierPath.Space == PathSpace.xyz) {
                    Handles.color = handlesStartCol;

                    int attachedControlIndex = (i == bezierPath.NumPoints - 1) ? i - 1 : i + 1;
                    Vector3 dir = (bezierPath[attachedControlIndex] - handlePosition).normalized;
                    float handleRotOffset = (360 + bezierPath.GlobalNormalsAngle) % 360;
                    anchorAngleHandle.radius = handleSize * 3;
                    anchorAngleHandle.angle = handleRotOffset + bezierPath.GetAnchorNormalAngle (i / 3);
                    Vector3 handleDirection = Vector3.Cross (dir, Vector3.up);
                    Matrix4x4 handleMatrix = Matrix4x4.TRS (
                        handlePosition,
                        Quaternion.LookRotation (handleDirection, dir),
                        Vector3.one
                    );

                    using (new Handles.DrawingScope (handleMatrix)) {
                        // draw the handle
                        EditorGUI.BeginChangeCheck ();
                        anchorAngleHandle.DrawHandle ();
                        if (EditorGUI.EndChangeCheck ()) {
                            Undo.RecordObject (creator, "Set angle");
                            bezierPath.SetAnchorNormalAngle (i / 3, anchorAngleHandle.angle - handleRotOffset);
                        }
                    }

                } else {
                    handlePosition = Handles.DoPositionHandle (handlePosition, Quaternion.identity);
                }

            }

            switch (handleInputType) {
                case PathHandle.HandleInputType.LMBDrag:
                    draggingHandleIndex = i;
                    handleIndexToDisplayAsTransform = -1;
                    Repaint ();
                    break;
                case PathHandle.HandleInputType.LMBRelease:
                    draggingHandleIndex = -1;
                    handleIndexToDisplayAsTransform = -1;
                    Repaint ();
                    break;
                case PathHandle.HandleInputType.LMBClick:
                    if (Event.current.shift) {
                        handleIndexToDisplayAsTransform = -1; // disable move tool if new point added
                    } else {
                        if (handleIndexToDisplayAsTransform == i) {
                            handleIndexToDisplayAsTransform = -1; // disable move tool if clicking on point under move tool
                        } else {
                            handleIndexToDisplayAsTransform = i;
                        }
                    }
                    Repaint ();
                    break;
                case PathHandle.HandleInputType.LMBPress:
                    if (handleIndexToDisplayAsTransform != i) {
                        handleIndexToDisplayAsTransform = -1;
                        Repaint ();
                    }
                    break;
            }

            if (bezierPath[i] != handlePosition) {
                Undo.RecordObject (creator, "Move point");
                bezierPath.MovePoint (i, handlePosition);

                // If the use is holding alt, try and mirror the control point.
                if (Event.current.modifiers == EventModifiers.Alt) {
                    // 0 = Anchor, 1 = Left Control, 2 = Right Control
                    var pointType = i % 3;

                    // If we are selecting a control point
                    if (pointType != 0) {
                        // If we are selecting the left control point
                        if (pointType == 2) {
                            // If the path doesn't loop and the user is selecting the last control point there isn't a control point to mirror
                            if (i < bezierPath.NumPoints - 2 && !bezierPath.IsClosed)
                                return;
                            // Get the index of this control's anchor.
                            var anchorIndex = (i + 1) % bezierPath.NumPoints;
                            var anchorPoint = bezierPath[anchorIndex];
                            // Get the index of the anchors other control.
                            // We don't have to loop this index b/c if it's the last control, it's anchors index will be 1.
                            var otherControlPointIndex = anchorIndex + 1;
                            // Move the other control point to the opposite of the selected control point's position relative to it's anchor.
                            bezierPath.MovePoint (otherControlPointIndex, anchorPoint - (handlePosition - anchorPoint));
                        }
                        // If we are selecting the right control point
                        else if (pointType == 1) {
                            // If the path doesn't loop and the user is selecting the first control point there isn't a control point to mirror.
                            if (i > 1 && !bezierPath.IsClosed)
                                return;
                            // Get the index of this control's anchor.
                            var anchorIndex = i - 1;
                            var anchorPoint = bezierPath[anchorIndex];
                            // Get the index of the anchors other control.
                            var otherControlPointIndex = anchorIndex - 1;
                            // Make sure to loop this index back around if it is < 1.
                            if (otherControlPointIndex < 0)
                                otherControlPointIndex = bezierPath.NumPoints - Mathf.Abs (otherControlPointIndex);
                            // Move the other control point to the opposite of the selected control point's position relative to it's anchor.
                            bezierPath.MovePoint (otherControlPointIndex, anchorPoint - (handlePosition - anchorPoint));
                        }
                    }
                }
            }

        }

        #endregion

        #region Internal methods

        void OnDisable () {
            Tools.hidden = false;
        }

        void OnEnable () {
            creator = (PathCreator) target;
            bool in2DEditorMode = EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode2D;
            creator.InitializeEditorData (in2DEditorMode);
            positionOld = creator.transform.position;
            rotationOld = creator.transform.rotation;
            scaleOld = creator.transform.localScale;

            data.bezierCreated -= ResetState;
            data.bezierCreated += ResetState;
            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.undoRedoPerformed += OnUndoRedo;

            LoadDisplaySettings ();
            UpdateGlobalDisplaySettings ();
            UpdateToolVisibility ();
            ResetState ();
        }

        void OnUndoRedo () {
            hasUpdatedScreenSpaceLine = false;
            hasUpdatedNormalsVertexPath = false;
            selectedSegmentIndex = -1;

            Repaint ();
        }

        void TabChanged () {
            SceneView.RepaintAll ();
            RepaintUnfocusedSceneViews ();
        }

        void LoadDisplaySettings () {
            globalDisplaySettings = GlobalDisplaySettings.Load ();

            capFunctions = new Dictionary<GlobalDisplaySettings.HandleType, Handles.CapFunction> ();
            capFunctions.Add (GlobalDisplaySettings.HandleType.Circle, Handles.CylinderHandleCap);
            capFunctions.Add (GlobalDisplaySettings.HandleType.Sphere, Handles.SphereHandleCap);
            capFunctions.Add (GlobalDisplaySettings.HandleType.Square, Handles.CubeHandleCap);
        }

        void UpdateGlobalDisplaySettings () {
            var gds = globalDisplaySettings;
            splineAnchorColours = new PathHandle.HandleColours (gds.anchor, gds.anchorHighlighted, gds.anchorSelected, gds.handleDisabled);
            splineControlColours = new PathHandle.HandleColours (gds.control, gds.controlHighlighted, gds.controlSelected, gds.handleDisabled);

            anchorAngleHandle.fillColor = new Color (1, 1, 1, .05f);
            anchorAngleHandle.wireframeColor = Color.grey;
            anchorAngleHandle.radiusHandleColor = Color.clear;
            anchorAngleHandle.angleHandleColor = Color.white;
        }

        void ResetState () {
            selectedSegmentIndex = -1;
            draggingHandleIndex = -1;
            mouseOverHandleIndex = -1;
            handleIndexToDisplayAsTransform = -1;
            hasUpdatedScreenSpaceLine = false;
            hasUpdatedNormalsVertexPath = false;
            bezierPath.Pivot = bezierPath.PathBounds.center;

            bezierPath.OnModified -= OnPathModifed;
            bezierPath.OnModified += OnPathModifed;

            SceneView.RepaintAll ();
            EditorApplication.QueuePlayerLoopUpdate ();
        }

        void OnPathModifed () {
            hasUpdatedScreenSpaceLine = false;
            hasUpdatedNormalsVertexPath = false;

            RepaintUnfocusedSceneViews ();
        }

        void RepaintUnfocusedSceneViews () {
            // If multiple scene views are open, repaint those which do not have focus.
            if (SceneView.sceneViews.Count > 1) {
                foreach (SceneView sv in SceneView.sceneViews) {
                    if (EditorWindow.focusedWindow != (EditorWindow) sv) {
                        sv.Repaint ();
                    }
                }
            }
        }

        void UpdatePathMouseInfo () {
            if (!hasUpdatedScreenSpaceLine) {
                screenSpaceLine = new ScreenSpacePolyLine (bezierPath, screenPolylineMaxAngleError, screenPolylineMinVertexDst);
                hasUpdatedScreenSpaceLine = true;
            }
            pathMouseInfo = screenSpaceLine.CalculateMouseInfo ();
        }

        float GetHandleDiameter (float diameter, Vector3 handlePosition) {
            float scaledDiameter = diameter * constantHandleScale;
            if (data.keepConstantHandleSize) {
                scaledDiameter *= HandleUtility.GetHandleSize (handlePosition) * 2.5f;
            }
            return scaledDiameter;
        }

        BezierPath bezierPath {
            get {
                return data.bezierPath;
            }
        }

        PathCreatorData data {
            get {
                return creator.EditorData;
            }
        }

        bool editingNormals {
            get {
                return Tools.current == Tool.Rotate && handleIndexToDisplayAsTransform % 3 == 0 && bezierPath.Space == PathSpace.xyz;
            }
        }

        void UpdateToolVisibility () {
            // Hide/unhide tools depending on if inspector is folded
            bool hideTools = UnityEditorInternal.InternalEditorUtility.GetIsInspectorExpanded (creator);
            if (Tools.hidden != hideTools) {
                Tools.hidden = hideTools;
            }
        }

        #endregion

    }

}