// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.Base
{
    /// <remarks>
    /// History:
    /// 27 October 2020: Created
    /// </remarks>
    /// <summary>
    /// SDK wrapper for the head.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class InputHeadSDK : MonoBehaviour
    {
        /// <summary>
        /// Reference to the input head class.
        /// </summary>
        [Tooltip("Reference to the input head class.")]
        public InputHead inputHead;

        /// <summary>
        /// The head target. This should not be the camera Transform itself, but a child
        /// <code>GameObject</code> parented by the head so that the 'target' can be
        /// repositioned and oriented within the head to align the transform to match the
        /// location of the camera/eyes.
        /// </summary>
        [Tooltip("The head target. This should not be the camera Transform itself, but a child GameObject parented by the head so that the 'target' can be repositioned and oriented within the head to align the transform to match the location of the camera/eyes.")]
        public Transform target;

        /// <summary>
        /// The head collider.
        /// </summary>
        [Tooltip("The head collider.")]
        public new Collider collider;

        private void Start()
        {
            if (inputHead == null)
            {
                inputHead = GetComponent<InputHead>();
            }

            if (collider == null)
            {
                collider = GetComponentInChildren<Collider>();
            }

            if (target == null)
            {
                Debug.LogWarning("'target' not not set InputHeadSDK.");
            }
        }

        /// <summary>
        /// Initialize method for SDK input head wrapper.
        /// </summary>
        public virtual void Initialize()
        {
            //Debug.LogWarning("Initialize() not implemented for InputHeadSDK.");
        }

        /// <summary>
        /// Moves the head target to the supplied local position and local rotation
        /// </summary>
        /// <param name="localPosition">A <code>Vector3</code> representing the new local position of the target</param>
        /// <param name="localRotation">A <code>Quaternion</code> representing the new local rotation of the target</param>
        public virtual void MoveTarget(Vector3 localPosition, Quaternion localRotation)
        {
            // Adjust the target
            if (target != null)
            {
                target.localPosition = localPosition;
                target.localRotation = localRotation;

                // Invert the target x/y position adjustment to compensate for the move
                // so that the collider remains centered on the head. z remains the same
                // because it is forward to compensate for the eye position.
                if (collider is CapsuleCollider)
                {
                    CapsuleCollider capsuleCollider = (collider as CapsuleCollider);
                    capsuleCollider.center = new Vector3(
                        capsuleCollider.center.x,
                        -(localPosition.y),
                        capsuleCollider.center.z);
                }
                else if (collider is BoxCollider)
                {
                    BoxCollider boxCollider = (collider as BoxCollider);
                    boxCollider.center = new Vector3(
                        boxCollider.center.x,
                        -(localPosition.y),
                        boxCollider.center.z);
                }
                else if (collider is SphereCollider)
                {
                    SphereCollider sphereCollider = (collider as SphereCollider);
                    sphereCollider.center = new Vector3(
                        sphereCollider.center.x,
                        -(localPosition.y),
                        sphereCollider.center.z);
                }
                else if (collider is CharacterController)
                {
                    CharacterController characterCollider = (collider as CharacterController);
                    characterCollider.center = new Vector3(
                        characterCollider.center.x,
                        -(localPosition.y),
                        characterCollider.center.z);
                }
            }
        }
    }
}