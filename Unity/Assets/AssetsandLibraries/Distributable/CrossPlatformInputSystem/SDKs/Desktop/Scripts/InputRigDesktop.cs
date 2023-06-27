// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.Base;

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.Desktop
{
    /// <remarks>
    /// History:
    /// 27 October 2020: Created
    /// 17 March 2021: Removed the running/walking references to make use of the generalized
    ///     motion constraints in the input rig SDK. Removed the armswing implementations, and
    ///     updated to reflect use of the controller interface. (J. Hosler)
    /// 5 April 2021: Removed redundant character controller property (D. Baker)
    /// 17 November 2021: Added implementation for flying locomotion (DZB)
    /// </remarks>
    /// <summary>
    /// Desktop wrapper for the input rig.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class InputRigDesktop : InputRigSDK
    {
        [SerializeField]
        GameObject _cameraOffsetObject;

        /// <summary>
        /// The <see cref="GameObject"/> to move to desired height off the floor (defaults to this object if none provided).
        /// This is used to transform the XR device from camera space to rig space.
        /// </summary>
        public GameObject cameraFloorOffsetObject
        {
            get => _cameraOffsetObject;
            set
            {
                _cameraOffsetObject = value;
            }
        }

        public override InputHand placingHand
        {
            get
            {
                foreach (InputHand hand in inputRig.hands)
                {
                    InputHandDesktop ihd = hand.GetComponent<InputHandDesktop>();
                    if (ihd != null)
                    {
                        if (ihd.dynamic)
                        {
                            return hand;
                        }
                    }
                }
                Debug.LogError("[InputHandDesktop] Unable to find placing hand.");
                return null;
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Start()
        {
            // Set the correct player height
            UpdatePlayerHeight(PlayerHeight);

            // Create the event handler for adjusting the camera offset when player height is changed
            PlayerHeightChange += PlayerHeightChanged;
        }

        private void OnDestroy()
        {
            // Remove the player height changed event handler
            PlayerHeightChange -= PlayerHeightChanged;
        }

#region Player

        /// <summary>
        /// Updates the rig components to adjust for the player height
        /// </summary>
        /// <param name="playerHeight"></param>
        protected virtual void UpdatePlayerHeight(float playerHeight)
        {
            if (_cameraOffsetObject != null)
            {
                // Adjust the camera offset to the correct height, factoring in the top of the head relative to the camera offset.
                if ((inputRig.head != null) && (inputRig.head.Collider != null))
                {
                    float headTop = inputRig.head.Collider.bounds.max.y;
                    float headHalfHeight = float.NaN;
                    if (inputRig.head.Collider is CapsuleCollider)
                    {
                        CapsuleCollider capsuleCollider = (inputRig.head.Collider as CapsuleCollider);
                        headHalfHeight = capsuleCollider.transform.TransformVector(new Vector3(0,capsuleCollider.height / 2f,0)).y;
                    }
                    else if (inputRig.head.Collider is BoxCollider)
                    {
                        BoxCollider boxCollider = (inputRig.head.Collider as BoxCollider);
                        headHalfHeight = boxCollider.transform.TransformVector(boxCollider.size / 2f).y;
                    }
                    else if (inputRig.head.Collider is SphereCollider)
                    {
                        SphereCollider sphereCollider = (inputRig.head.Collider as SphereCollider);
                        headHalfHeight = sphereCollider.transform.TransformVector(new Vector3(0, sphereCollider.radius, 0)).y;
                    }
                    else if (inputRig.head.Collider is CharacterController)
                    {
                        CharacterController characterCollider = (inputRig.head.Collider as CharacterController);
                        headHalfHeight = characterCollider.transform.TransformVector(new Vector3(0, characterCollider.height / 2f, 0)).y;
                    }

                    // Make sure we have a valid head center
                    if (!float.IsNaN(headHalfHeight))
                    {
                        // Adjust the camera offset to reflect the new player height
                        Vector3 newHeight = new Vector3(0, playerHeight - headHalfHeight, 0);
                        Vector3 newLocalPosition = new Vector3(
                            _cameraOffsetObject.transform.localPosition.x,
                            _cameraOffsetObject.transform.InverseTransformVector(newHeight).y,
                            _cameraOffsetObject.transform.localPosition.z);
                        _cameraOffsetObject.transform.localPosition = newLocalPosition;
                    }
                }
            }
        }

        /// <summary>
        /// Event handler to respond to player height change events
        /// </summary>
        /// <param name="previousHeight"></param>
        /// <param name="newHeight"></param>
        private void PlayerHeightChanged(float previousHeight, float newHeight)
        {
            // Update the player height
            UpdatePlayerHeight(newHeight);
        }

#endregion Player

#region Physics

        /// <seealso cref="InputRigSDK.GravityEnabled"/>
        public override bool GravityEnabled
        {
            get
            {
                return physics.useGravity;
            }
        }

        /// <seealso cref="InputRigSDK.EnableGravity"/>
        public override void EnableGravity()
        {
            physics.useGravity = true;
        }

        /// <seealso cref="InputRigSDK.DisableGravity"/>
        public override void DisableGravity()
        {
            physics.useGravity = false;
        }

#endregion // Physics

#region Locomotion

#region Locomotion [Armswing]

        /// <seealso cref="InputRigSDK.ArmswingEnabled"/>
        public override bool ArmswingEnabled
        {
            get
            {
                // TODO: implement armswing?
                return false;
            }
        }

        /// <seealso cref="InputRigSDK.EnableArmswing"/>
        public override void EnableArmswing()
        {
            Debug.Log("[InputRigDesktop->EnableArmswing] Armswing not implemented for desktop.");
        }

        /// <seealso cref="InputRigSDK.DisableArmswing"/>
        public override void DisableArmswing()
        {
            Debug.Log("[InputRigDesktop->DisableArmswing] Armswing not implemented for desktop.");
        }

#endregion // Locomotion [Armswing]

#region Locomotion [Flying]

        /// <seealso cref="InputRigSDK.FlyingEnabled"/>
        public override bool FlyingEnabled
        {
            get
            {
                return _flyingController.GetControlActive();
            }
        }

        /// <seealso cref="InputRigSDK.EnableFlying"/>
        public override void EnableFlying()
        {
            _flyingController.SetControlActive(true);
        }

        /// <seealso cref="InputRigSDK.DisableFlying"/>
        public override void DisableFlying()
        {
            _flyingController.SetControlActive(false);
        }

#endregion // Locomotion [Flying]

#region Locomotion [Navigation]

        /// <seealso cref="InputRigSDK.NavigationEnabled"/>
        public override bool NavigationEnabled
        {
            get
            {
                return _navigationController.GetControlActive();
            }
        }

        /// <seealso cref="InputRigSDK.EnableNavigation"/>
        public override void EnableNavigation()
        {
            _navigationController.SetControlActive(true);
        }

        /// <seealso cref="InputRigSDK.DisableNavigation"/>
        public override void DisableNavigation()
        {
            _navigationController.SetControlActive(false);
        }

#endregion // Locomotion [Navigation]

#endregion // Locomotion
    }
}