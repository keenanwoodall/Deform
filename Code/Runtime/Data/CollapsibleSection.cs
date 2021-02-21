using UnityEngine;

namespace Deform
{
    public class CollapsibleSection : PropertyAttribute
    {
        public readonly string Title;
        public CollapsibleSection(string title)
        {
            this.Title = title;
        }
    }
}