using UnityEngine;
using System.Collections.Generic;

namespace PathCreation
{
    public class PathCreator : MonoBehaviour
    {

        /// This class stores data for the path editor, and provides accessors to get the current vertex and bezier path.
        /// Attach to a GameObject to create a new path editor.

        public event System.Action pathUpdated;

        [SerializeField, HideInInspector]
        PathCreatorData editorData;
        [SerializeField, HideInInspector]
        bool initialized;

        // Vertex path created from the current bezier path
        public VertexPath path
        {
            get
            {
                if (!initialized)
                {
                    InitializeEditorData(false);
                }
                return editorData.vertexPath;
            }
        }

        // The bezier path created in the editor
        public BezierPath bezierPath
        {
            get
            {
                if (!initialized)
                {
                    InitializeEditorData(false);
                }
                return editorData.bezierPath;
            }
            set
            {
                if (!initialized)
                {
                    InitializeEditorData(false);
                }
                editorData.bezierPath = value;
            }
        }

        #region Internal methods

        /// Used by the path editor to initialise some data
        public void InitializeEditorData(bool in2DMode)
        {
            if (editorData == null)
            {
                editorData = new PathCreatorData();
            }
            editorData.bezierOrVertexPathModified -= OnPathUpdated;
            editorData.bezierOrVertexPathModified += OnPathUpdated;

            editorData.Initialize(transform.position, in2DMode);
            initialized = true;
        }

        public PathCreatorData EditorData
        {
            get
            {
                return editorData;
            }

        }

        void OnPathUpdated()
        {
            if (pathUpdated != null)
            {
                pathUpdated();
            }
        }

        #endregion
    }
}
