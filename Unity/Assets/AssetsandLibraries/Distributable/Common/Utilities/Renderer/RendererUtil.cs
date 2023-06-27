// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.Utilities.Renderer
{
    /// <remarks>
    /// History:
    /// 22 August 2020: Created
    /// </remarks>
	///
	/// <summary>
	/// RendererUtil
	///
	/// Abstract container class containing renderer functions.
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public abstract class RendererUtil
    {
        /// <summary>
        /// Common function used to calculate the true object bounds in world space based upon renderers
        /// </summary>
        /// <param name="gameObject">The <code>GameObject</code> for the calculation</param>
        /// <returns>The <code>Bounds</code> of the supplied game object in world space</returns>
        public static Bounds GetBounds(GameObject gameObject)
        {
            // Calculate the bounds including all of the children with renderers
            Bounds bounds = new Bounds(gameObject.transform.position, Vector3.zero);

            // Encapculate the renderers
            foreach (UnityEngine.Renderer renderer in gameObject.GetComponentsInChildren<UnityEngine.Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }

        /// <summary>
        /// Common function used to show/hide a game object
        /// </summary>
        /// <param name="gameObject">The <code>GameObject</code> to show/hide</param>
        /// <param name="enabled">The flag indicating whether to show/hide</param>
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

        /// <summary>
        /// Common function used to apply a material to a game object (including all of the children)
        /// </summary>
        /// <param name="gameObject">The <code>GameObject</code> to assign the material</param>
        /// <param name="material">The <code>Material</code> to apply</param>
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

        /// <summary>
        /// Applies an color to a game object (including all of the children)
        /// </summary>
        /// <param name="gameObject">The <code>GameObject</code> to assign the material</param>
        /// <param name="color">The <code>Color32</code> to apply</param>
        public static void ApplyColor(GameObject gameObject, Color32 color)
        {
            foreach (UnityEngine.Renderer rend in gameObject.GetComponentsInChildren<UnityEngine.Renderer>())
            {
                if (rend.material != null)
                {
                    rend.material.color = color;
                }
            }
        }
        /// <summary>
        /// Applies an opacity to a game object (including all of the children)
        /// </summary>
        /// <param name="gameObject">The <code>GameObject</code> to assign the material</param>
        /// <param name="opacity">The opacity value to apply. 0 is transparent, 255 is opaque.</param>
        public static void ApplyOpacity(GameObject gameObject, byte opacity)
        {
            foreach (UnityEngine.Renderer rend in gameObject.GetComponentsInChildren<UnityEngine.Renderer>())
            {
                if (rend.material != null)
                {
                    if (rend.material.HasProperty("_Color"))
                    {
                        Color32 colorToChange = rend.material.color;
                        colorToChange.a = opacity;
                        rend.material.color = colorToChange;
                    }
                }
            }
        }

    }
}
