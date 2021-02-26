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
            if (DeformEditorSettings.BuildStrippingMode == BuildStrippingMode.StripDeformInScenes)
            {
                // Destroy all the Deformers and Deformables in the scene (this will include derived types)
                Deformer[] deformers = Resources.FindObjectsOfTypeAll<Deformer>();
                foreach (Deformer deformer in deformers)
                {
                    var go = deformer.gameObject;
                    // Is it an ordinary scene object?
                    if (!EditorUtility.IsPersistent(go.transform.root.gameObject) && !(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave))
                    {
                        DestroyComponentAndClean(deformer);
                    }
                }
                Deformable[] deformables = Resources.FindObjectsOfTypeAll<Deformable>();
                foreach (Deformable deformable in deformables)
                {
                    deformable.assignOriginalMeshOnDisable = false;
                    
                    var go = deformable.gameObject;
                    // Is it an ordinary scene object?
                    if (!EditorUtility.IsPersistent(go.transform.root.gameObject) && !(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave))
                    {
                        DestroyComponentAndClean(deformable);
                    }
                }
            }
        }

        /// <summary>
        /// Destroys a component, then if the game object it's on has no components or children that's also destroyed
        /// </summary>
        private static void DestroyComponentAndClean(Component targetObject)
        {
            GameObject gameObject = targetObject.gameObject;
            Object.DestroyImmediate(targetObject);
            
            // Strip gameObjects if empty
            if (gameObject.transform.childCount == 0 && gameObject.GetComponents<Component>().Length == 1) // Transform counts as a component, so an empty game object has 1 component
            {
                // Game object with no components and no children, can be stripped
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}