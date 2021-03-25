// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.Utilities.Transforms;

namespace GSFC.ARVR.Utilities.Collider
{
    /**
     * Abstract container class containing collider functions.
     * 
     * @author Jeffrey Hosler
     */
    public abstract class ColliderUtil
    {
        /**
         * Common function used to create a box collider on a game object
         * 
         * NOTE: This should be done before any transformations are done to the game object
         * or the box collider result will be wrong!
         * 
         * @param gameObject The <code>GameObject</code> to assign the box collider
         * 
         * @return The created <code>BoxCollider</code> assigned to the game object
         */
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

        /**
          * Common function used to create the non-convex mesh colliders on the meshes that make up
          * the supplied game object
          * 
          * NOTE: This should be done before any transformations are done to the game object
          * or the box collider result will be wrong!
          * 
          * @param gameObject The <code>GameObject</code> to assign the non-convex mesh collider
          * @param boxesPerEdge The number of boxes per edge on each mesh
          */
        public static void CreateNonConvexMeshColliders(GameObject gameObject, int boxesPerEdge = 20)
        {
            // Create the box collider
            // Generate the NonConvexMeshColliders for each mesh making up the object
            foreach (MeshFilter mesh in gameObject.GetComponentsInChildren<MeshFilter>())
            {
                NonConvexMeshCollider ncmc = mesh.gameObject.AddComponent<NonConvexMeshCollider>();
                ncmc.boxesPerEdge = boxesPerEdge;
                ncmc.Calculate();
            }
        }

    }

}
