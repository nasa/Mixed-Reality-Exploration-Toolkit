// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GSFC.ARVR.Utilities.Renderer
{
    /**
     * Abstract container class containing renderer functions.
     * 
     * @author Jeffrey Hosler
     */
    public abstract class RendererUtil
    {
        /**
         * Common function used to show/hide a game object
         * 
         * @param gameObject The <code>GameObject</code> to show/hide
         * @param enabled The flag indicating whether to show/hide
         */
        public static void Show(GameObject gameObject, bool enabled = true)
        {
            // Enable the renderers
            foreach (UnityEngine.Renderer renderer in gameObject.GetComponentsInChildren<UnityEngine.Renderer>())
            {
                if (renderer != null)
                {
                    // Show the object
                    renderer.enabled = enabled;
                }
            }
        }

        /**
         * Common function used to apply a material to a game object (including all of the children)
         * 
         * @param gameObject The <code>GameObject</code> to assign the material
         * @param enabled The flag indicating whether to show/hide
         */
        public static void ApplyMaterial(GameObject gameObject, Material material)
        {
            // Enable the renderers
            foreach (UnityEngine.Renderer renderer in gameObject.GetComponentsInChildren<UnityEngine.Renderer>())
            {
                if (renderer != null)
                {
                    // Show the object
                    renderer.material = material;
                }
            }
        }

    }

}
