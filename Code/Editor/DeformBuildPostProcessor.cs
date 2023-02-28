using Deform;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace DeformEditor
{
    public enum BuildStrippingMode
    {
        StripDeformInScenes,
        NoStripping,
    };
    
    public class DeformBuildPostProcessor
    {
        [PostProcessScene]
        public static void OnPostprocessScene()
        {
            var deformables = Resources.FindObjectsOfTypeAll<Deformable>();
            foreach (var deformable in deformables)
            {
                if (deformable.StripMode == StripMode.DontStrip)
                    continue;
                
                deformable.assignOriginalMeshOnDisable = false;
                
                var go = deformable.gameObject;
                // Is it an ordinary scene object?
                if (!EditorUtility.IsPersistent(go.transform.root.gameObject) && !(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave))
                {
                    Object.DestroyImmediate(deformable);
                }
            }
        }
    }
}