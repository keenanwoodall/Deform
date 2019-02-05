using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathCreationEditor
{
    //[CreateAssetMenu()]
    public class GlobalDisplaySettings : ScriptableObject
    {

        public enum HandleType { Sphere, Circle, Square };

        [Header("Appearance")]
        public float anchorSize = 10;
        public float controlSize = 7f;
        
        [Tooltip("If true, control points will be hidden when the control point mode is set to automatic. Otherwise they will inactive, but still visible.")]
        public bool hideAutoControls = true;
        public HandleType anchorShape;
        public HandleType controlShape;


        [Header("Anchor Colours")]
        public Color anchor = new Color(0.95f, 0.25f, 0.25f, 0.85f);
        public Color anchorHighlighted = new Color(1, 0.57f, 0.4f);
        public Color anchorSelected = Color.white;

        [Header("Control Colours")]
        public Color control = new Color(0.35f, 0.6f, 1, 0.85f);
        public Color controlHighlighted = new Color(0.8f, 0.67f, 0.97f);
        public Color controlSelected = Color.white;
        public Color handleDisabled = new Color(1, 1, 1, 0.2f);
        public Color controlLine = new Color(0, 0, 0, 0.35f);

        [Header("Bezier Path Colours")]
        public Color bezierPath = Color.green;
        public Color highlightedPath = new Color(1, 0.6f, 0);
        public Color bounds = new Color(1, 1, 1, .4f);
        public Color segmentBounds = new Color(1, 1, 1, .4f);

        [Header("Vertex Path Colours")]
        public Color vertexPath = Color.white;
        public Color vertex = Color.black;

        [Header("Normals")]
        public Color normals = Color.yellow;
        [Range(0,1)]
        public float normalsLength = .1f;
    }
}
