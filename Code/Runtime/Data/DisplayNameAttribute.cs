using UnityEngine;

namespace Deform
{
    /// <summary>
    /// Add this to a field to override its display name in the inspector, also optionally takes a tooltip which can be
    /// used instead of [Tooltip]
    /// </summary>
    public class DisplayNameAttribute : PropertyAttribute
    {
        public readonly GUIContent GUIContent;

        public DisplayNameAttribute(string displayName, string tooltip = "")
        {
            GUIContent = new GUIContent(displayName, tooltip);
        }
    }
}