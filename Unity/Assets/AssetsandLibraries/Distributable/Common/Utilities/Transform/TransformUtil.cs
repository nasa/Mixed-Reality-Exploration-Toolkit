// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.Utilities.Transforms
{
    /**
     * Abstract container class containing transform functions.
     * 
     * @author Jeffrey Hosler
     */
    public abstract class TransformUtil
    {
        /// <summary>
        /// Common function used to calculate the true object bounds in world space
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

            // Encapculate the recttransforms
            foreach (RectTransform rectTransform in gameObject.GetComponentsInChildren<RectTransform>())
            {
                // We can obtain the corners in world space
                Vector3[] corners = new Vector3[4];
                rectTransform.GetWorldCorners(corners);

                // Encalculate each corner
                foreach (Vector3 corner in corners)
                {
                    bounds.Encapsulate(corner);
                }
            }

            return bounds;
        }

        /// <summary>
        /// Converts the supplied size in Unity units to a scale, factoring in
        /// the encapsulated renderer bounds of all the supplied game object children.
        /// </summary>
        /// <param name="gameObject">The <code>GameObject</code> to use for the calculation</param>
        /// <param name="size">The desired <code>Vector3</code> size in Unity units</param>
        /// <returns>The <code>Vector3</code> representing the supplied size, adjusted
        ///     for current scale of the supplied game object.</returns>
        public static Vector3 GetSizeAsScale(GameObject gameObject, Vector3 size)
        {
            Vector3 result = Vector3.zero;

            // Reset object rotation (for accurate bounds).
            Quaternion originalRotation = gameObject.transform.rotation;
            gameObject.transform.rotation = Quaternion.identity;

            // Obtain the bounds by encapsulating all the children
            Bounds bounds = GetBounds(gameObject);

            // We need the current scale and dimensions for our calculation
            Vector3 worldScale = gameObject.transform.lossyScale;
            Vector3 localScale = gameObject.transform.localScale;
            float scale = worldScale.x / localScale.x;
            Vector3 boundsSize = bounds.size;

            // Scale for accurate sizing
            result.x = (boundsSize.x > 0) ? ((size.x * localScale.x) / boundsSize.x) : result.x;
            result.y = (boundsSize.y > 0) ? ((size.y * localScale.y) / boundsSize.y) : result.y;
            result.z = (boundsSize.z > 0) ? ((size.z * localScale.z) / boundsSize.z) : result.z;

            // Restore the rotation
            gameObject.transform.rotation = originalRotation;

            return result;
        }

        /// <summary>
        /// Converts the supplied size in Unity units to a scale, factoring in
        /// the shared mesh of game object.
        /// </summary>
        /// <param name="gameObject">The <code>GameObject</code> to use for the calculation</param>
        /// <param name="size">The desired <code>Vector3</code> size in Unity units</param>
        /// <returns>The <code>Vector3</code> representing the supplied size adjusted
        ///     for current scale of the supplied game object, or <code>Vector3.zero</code>
        ///     if no shared mesh.</returns>
        public static Vector3 GetSizeAsScaleFromSharedMesh(GameObject gameObject, Vector3 size)
        {
            Vector3 result = Vector3.zero;

            // Obtain the shared mesh
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter && meshFilter.sharedMesh)
            {
                // Obtain the bounds of the shared mesh
                Bounds bounds = meshFilter.sharedMesh.bounds;

                // We need the current scale and dimensions for our calculation
                Vector3 worldScale = gameObject.transform.lossyScale;
                Vector3 localScale = gameObject.transform.localScale;
                float scale = worldScale.x / localScale.x;
                Vector3 boundsSize = bounds.size;

                // Scale for accurate sizing
                result.x = (boundsSize.x > 0) ? ((size.x * scale) / boundsSize.x) : result.x;
                result.y = (boundsSize.y > 0) ? ((size.y * scale) / boundsSize.y) : result.y;
                result.z = (boundsSize.z > 0) ? ((size.z * scale) / boundsSize.z) : result.z;
            }

            return result;
        }

        /// <summary>
        /// Returns the size of the supplied game object in Unity units, factoring in
        /// the encapsulated renderer bounds of all the supplied game object children.
        /// </summary>
        /// <param name="gameObject">The <code>GameObject</code> to use for the calculation</param>
        /// <returns>The <code>Vector3</code> representing the size in Unity units.</returns>
        public static Vector3 GetSize(GameObject gameObject)
        {
            Vector3 result = Vector3.zero;

            // Reset object rotation (for accurate bounds).
            Quaternion originalRotation = gameObject.transform.rotation;
            gameObject.transform.rotation = Quaternion.identity;

            // Obtain the bounds by encapsulating all the children
            Bounds bounds = GetBounds(gameObject);

            // We need the current scale and dimensions for our calculation
            Vector3 worldScale = gameObject.transform.lossyScale;
            Vector3 localScale = gameObject.transform.localScale;
            float scale = worldScale.x / localScale.x;
            Vector3 boundsSize = bounds.size;

            // Scale for accurate sizing
            result.x = boundsSize.x / scale;
            result.y = boundsSize.y / scale;
            result.z = boundsSize.z / scale;

            // Restore the rotation
            gameObject.transform.rotation = originalRotation;

            return result;
        }
    }

}
