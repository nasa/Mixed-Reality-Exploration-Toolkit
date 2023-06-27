// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.Utilities.Transforms;

namespace GOV.NASA.GSFC.XR.Utilities.Collider
{
    /**
     * Abstract container class containing collider functions.
     * 
     * @author Jeffrey Hosler
     */
    public abstract class ColliderUtil
    {
        /// <summary>
        /// Indicates the existence of a collider in the supplied game object. This is useful
        /// since alternative collider implementations may be used for non-convex solutions.
        /// </summary>
        /// <param name="gameObject">The <code>GameObject</code> to query</param>
        /// <returns>An boolean value indicating the existence of a collider in the supplied game object.</returns>
        public static bool HasCollider(GameObject gameObject)
        {
            return ((gameObject.GetComponent<UnityEngine.Collider>() != null)
#if MRET_EXTENSION_NONCONVEXMESHCOLLIDER
                || (gameObject.GetComponent<NonConvexMeshCollider>() != null) // Doesn't extend Collider
#endif
                );
        }

        /// <summary>
        /// Indicates the existence of a collider in any of the children (including the root) of the supplied
        /// game object. This is useful since alternative collider implementations may be used for non-convex
        /// solutions.
        /// </summary>
        /// <param name="gameObject">The <code>GameObject</code> to query</param>
        /// <returns>An boolean value indicating the existence of a collider in the children of the supplied
        ///     game object.</returns>
        public static bool HasColliderInChildren(GameObject gameObject)
        {
            return ((gameObject.GetComponentInChildren<UnityEngine.Collider>() != null)
#if MRET_EXTENSION_NONCONVEXMESHCOLLIDER
                || (gameObject.GetComponentInChildren<NonConvexMeshCollider>() != null) // Doesn't extend Collider
#endif
                );
        }

        /// <summary>
        /// Common function used to calculate the true object bounds in world space based upon colliders
        /// </summary>
        /// <param name="gameObject">The <code>GameObject</code> for the calculation</param>
        /// <returns>The <code>Bounds</code> of the supplied game object collider in world space</returns>
        public static Bounds GetBounds(GameObject gameObject)
        {
            // Calculate the bounds including all of the children with colliders
            Bounds bounds = new Bounds(gameObject.transform.position, Vector3.zero);

            // Encapculate the renderers
            foreach (UnityEngine.Collider collider in gameObject.GetComponentsInChildren<UnityEngine.Collider>())
            {
                bounds.Encapsulate(collider.bounds);
            }

            return bounds;
        }

        /// <summary>
        /// Common function used to enable/disable the colliders on a game object
        /// </summary>
        /// <param name="gameObject">The <code>GameObject</code> containing the colliders to enable/disable</param>
        /// <param name="enabled">The flag indicating whether to enable/disable</param>
        public static void Enable(GameObject gameObject, bool enabled = true)
        {
            // Enable the colliders
            foreach (UnityEngine.Collider collider in gameObject.GetComponentsInChildren<UnityEngine.Collider>())
            {
                collider.enabled = enabled;
            }
        }

        /// <summary>
        /// Common function used to create a box collider on a game object
        /// 
        /// NOTE: This should be done before any transformations are done to the game object
        /// or the box collider result will be wrong!
        /// 
        /// </summary>
        /// <param name="gameObject">The <code>GameObject</code> to assign the box collider</param>
        /// <returns>The created <code>BoxCollider</code> assigned to the game object</returns>
        public static BoxCollider CreateBoxCollider(GameObject gameObject)
        {
            // Create the box collider
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();

            // Calculate the bounds including all of the children with renderers
            Bounds bounds = TransformUtil.GetBounds(gameObject);

            // Renderer bounds will be in world space, so we need to convert the information to local space
            collider.size = gameObject.transform.InverseTransformVector(bounds.size);
            collider.center = gameObject.transform.InverseTransformPoint(bounds.center);

            return collider;
        }

        /// <summary>
        /// Common function used to create the non-convex mesh colliders on the meshes that make up
        /// the supplied game object
        /// 
        /// NOTE: This should be done before any transformations are done to the game object
        /// or the box collider result will be wrong!
        /// 
        /// </summary>
        /// <param name="gameObject">The <code>GameObject</code> to assign the non-convex mesh collider</param>
        /// <param name="boxesPerEdge">The optional number of boxes per edge on each mesh. Default is 20.</param>
        public static void CreateNonConvexMeshColliders(GameObject gameObject, int boxesPerEdge = 20)
        {
#if MRET_EXTENSION_NONCONVEXMESHCOLLIDER
            // Create the box collider
            // Generate the NonConvexMeshColliders for each mesh making up the object
            foreach (MeshFilter mesh in gameObject.GetComponentsInChildren<MeshFilter>())
            {
                NonConvexMeshCollider ncmc = mesh.gameObject.AddComponent<NonConvexMeshCollider>();
                ncmc.boxesPerEdge = boxesPerEdge;
                ncmc.Calculate();
            }
#else
            Debug.LogWarning("NonConvexMeshCollider is not available. Defaulting to BoxCollider.");
            CreateBoxCollider(gameObject);
#endif
        }

    }

}
