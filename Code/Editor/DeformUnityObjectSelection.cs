using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DeformEditor
{
    /// <summary>
    /// Provides helper methods for custom editor tools when dealing with Unity's built in object selection systems
    /// </summary>
    public static class DeformUnityObjectSelection
    {
        /// <summary>
        /// Mimics Unity's built in selection marquee
        /// </summary>
        public static void DrawUnityStyleMarquee(Rect marqueeRect)
        {
            Handles.BeginGUI();

            Rect innerMarquee = new Rect(marqueeRect.position, marqueeRect.size * Vector2.one);
            // These colours roughly match Unity's built in marquee
            var innerColor = new Color32(148, 184, 237, (byte) (0.33f * 255));
            var borderColor = new Color(1, 1, 1, 0.67f);

            GUI.DrawTexture(innerMarquee, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 1, innerColor, Vector4.zero, Vector4.zero);
            GUI.DrawTexture(marqueeRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 1, borderColor, Vector4.one, Vector4.zero);

            Handles.EndGUI();
        }

        /// <summary>
        /// Has the user activated the built in Select All command?
        /// </summary>
        public static bool SelectAllPressed => Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "SelectAll";

        /// <summary>
        /// Prevent Unity's scene view selection system responding to mouse inputs (e.g. clicks and marquee mouse drags)
        /// </summary>
        public static int DisableSceneViewObjectSelection()
        {
            var defaultControl = GUIUtility.GetControlID(FocusType.Passive);
            if (Event.current.type == EventType.Layout)
                HandleUtility.AddDefaultControl(defaultControl);
            return defaultControl;
        }

        /// <summary>
        /// Unity's built in mouse up selection relies on RectSelection thinking it is the correct control in the
        /// MouseDown, in some cases we may override that for our own purposes but later want to use the MouseUp
        /// selection after all
        /// </summary>
        public static void AttemptMouseUpObjectSelection()
        {
            if (Event.current.type != EventType.MouseUp)
            {
                Debug.LogError("MouseUp selection should only be attempted from the MouseUp event");
            }

            var rectSelection = typeof(SceneView).GetField("m_RectSelection", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(SceneView.currentDrawingSceneView);

            // Make sure we have the correct control "hot" that RectSelection is expecting
            int rectSelectionID = (int) typeof(Editor).Assembly.GetType("UnityEditor.RectSelection").GetField("s_RectSelectionID", BindingFlags.NonPublic | BindingFlags.Static).GetValue(rectSelection);

            GUIUtility.hotControl = rectSelectionID;

            typeof(Editor).Assembly.GetType("UnityEditor.RectSelection").GetMethod("OnGUI").Invoke(rectSelection, null);
        }
    }
}