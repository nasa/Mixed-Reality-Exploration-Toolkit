using UnityEngine;

namespace GSFC.ARVR.Utilities.Transforms
{
    /**
     * Abstract container class containing transform functions.
     * 
     * @author Jeffrey Hosler
     */
    public abstract class TransformUtil
    {
        /**
         * Common function used to calculate the true object bounds in world space
         * 
         * @param gameObject The <code>GameObject</code> for the calculation
         * 
         * @return The <code>Bounds</code> of the supplied game object in world space
         */
         public static Bounds GetBounds(GameObject gameObject)
         {
            // Calculate the bounds including all of the children with renderers
            Bounds bounds = new Bounds(gameObject.transform.position, Vector3.zero);
            foreach (UnityEngine.Renderer renderer in gameObject.GetComponentsInChildren<UnityEngine.Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
         }
    }

}
