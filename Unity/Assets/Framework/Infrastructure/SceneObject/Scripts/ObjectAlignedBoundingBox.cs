// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.SceneObject
{
    /// <remarks>
    /// History:
    /// 9 August 2021: Created
    /// </remarks>
    /// <summary>
    /// Represents a bounding box that is aligned with an object's local axes (i.e. transform.right, transform.up, and transform.forward).
    /// This is used for computations with Motion Constraints in <c>SceneObject</c>.
    /// Author: Dev M. Chheda
    /// </summary>
    [Serializable] public class ObjectAlignedBoundingBox
    {   
        /// <summary>
        /// The transform that this bounding box is assigned to.
        /// </summary>
        public Transform transform;

        /// <summary>
        /// Offset from transform.position.
        /// </summary>
        public Vector3 centerOffset = new Vector3();

        /// <summary>
        /// The bound along the transform.right axis.
        /// </summary>
        public float rightBound = 0f;
        
        /// <summary>
        /// The bound along the transform.up axis.
        /// </summary>
        public float upBound = 0f;

        /// <summary>
        /// The bound along the transform.forward axis.
        /// </summary>
        public float forwardBound = 0f;

        /// <summary>
        /// Creates a new ObjectAlignedBoundingBox using the passed parameters.
        /// </summary>
        public ObjectAlignedBoundingBox(Transform transform, Vector3 centerOffset, float rightBound, float upBound, float forwardBound)
        {
            this.transform = transform;
            this.centerOffset = centerOffset;
            this.rightBound = rightBound;
            this.upBound = upBound;
            this.forwardBound = forwardBound;
        }

        /// <summary>
        /// Returns a new ObjectAlignedBoundingBox that encapuslates a given transform using axis-aligned bounding boxes.
        /// </summary>
        public static ObjectAlignedBoundingBox Encapsulate(Transform t)
        {
            // Align the object's rotation with world axes
            Quaternion originalRotation = t.rotation;
            t.rotation = Quaternion.identity;

            Collider[] allColliders = t.GetComponentsInChildren<Collider>();
            Bounds bounds = new Bounds(t.position, Vector3.zero);

            foreach (Collider collider in allColliders)
            {
                bounds.Encapsulate(collider.bounds);
            }
            
            // Rerotate object to original orientation
            t.rotation = originalRotation;

            return new ObjectAlignedBoundingBox(
                t,
                bounds.center - t.position, 
                bounds.extents.x, 
                bounds.extents.y, 
                bounds.extents.z
            );
        }

        /// <summary>
        /// Checks whether an ObjectAlignedBoundingBox is defined and nondegenerate (i.e. has positive bounds).
        /// </summary>
        public static bool IsValid(ObjectAlignedBoundingBox oabb) {
            if (oabb == null || oabb.transform == null) return false;
            try
            {
                return (oabb.rightBound > 0) && (oabb.upBound > 0) && (oabb.forwardBound > 0);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Scales a vector based on this bounding box and its assigned transform.
        /// A zero bounding vector corresponds to the center of the bounding box, and each of the components 
        /// of the bounding vector is scaled along the appropriate local axis of the transform.
        /// </summary>
        /// <param name="boundingVector">The bounding vector. If all components are between -1 and 1, the bounding vector is inside the bounding box.</param>
        public Vector3 ScaleBoundingVector(Vector3 boundingVector)
        {
            return ScaleBoundingVector(boundingVector.x, boundingVector.y, boundingVector.z);
        }

        /// <summary>
        /// Scales a vector based on this bounding box and its assigned transform.
        /// A zero bounding vector corresponds to the center of the bounding box, and each of the components 
        /// of the bounding vector is scaled along the appropriate local axis of the transform.
        /// </summary>
        /// <param name="x">The x-component of the bounding vector. Values between -1 and 1 are inside the bounding box.</param>
        /// <param name="y">The y-component of the bounding vector. Values between -1 and 1 are inside the bounding box.</param>
        /// <param name="z">The z-component of the bounding vector. Values between -1 and 1 are inside the bounding box.</param>
        public Vector3 ScaleBoundingVector(float x, float y, float z)
        {
            return transform.position
            + transform.right * (x * this.rightBound + this.centerOffset.x)
            + transform.up * (y * this.upBound + this.centerOffset.y)
            + transform.forward * (z * this.forwardBound + this.centerOffset.z); 
        }
    }
}
