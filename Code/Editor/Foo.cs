using UnityEditor;
using UnityEngine;

namespace DeformEditor
{
    public class Foo
    {
        public static void DrawMarquee(Rect marqueeRect)
        {
            Handles.BeginGUI();

            Rect centerMarquee = new Rect(marqueeRect.position, marqueeRect.size * Vector2.one);
            var mainColor = new Color32(148, 184, 237, (byte) (0.33f * 255));
            var borderColor = new Color(1, 1, 1, 0.67f);

            GUI.DrawTexture(centerMarquee, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 1, mainColor, Vector4.zero, Vector4.zero);
            GUI.DrawTexture(marqueeRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 1, borderColor, Vector4.one, Vector4.zero);
            Handles.EndGUI();
        }

        public static bool SelectAllPressed => Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "SelectAll";

        public static int DisableObjectSelection()
        {
            var defaultControl = GUIUtility.GetControlID(FocusType.Passive);
            if (Event.current.type == EventType.Layout)
                HandleUtility.AddDefaultControl(defaultControl);
            return defaultControl;
        }

        public static Vector3 WorldToGUIPoint(Camera camera, Vector3 point)
        {
            Vector3 screenPoint = camera.WorldToScreenPoint(point);
                            
            // Flip the y position so it matches the rect coordinate space
            screenPoint.y = camera.pixelHeight - screenPoint.y;
            // Convert from pixels to points (e.g. on retina screens)
            screenPoint /= EditorGUIUtility.pixelsPerPoint;
            
            return screenPoint;
        }

        public static bool MouseActionAllowed()
        {
            if (Event.current.alt) return false;
            
            return true;
        }
    }
}