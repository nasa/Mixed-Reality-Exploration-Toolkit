// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using System.Collections.Generic;
#if MRET_EXTENSION_FINALIK
using RootMotion.FinalIK;
#endif
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;

namespace GOV.NASA.GSFC.XR.MRET.Avatar
{
    public enum GenderType
    {
        female,
        male,
        other
    }

    /// <remarks>
    /// History:
    /// 25 August 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// AvatarManager
	///
	/// Manages the avatar system including the set of avatars that can be selected by the user.
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public class AvatarManager : MRETManager<AvatarManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(AvatarManager);

        /** IK Targets **/
        [Tooltip("The input head that should tracked by the avatar's head")]
        public InputHead avatarHead;

        [Tooltip("The input hand that should tracked by the avatar's left hand")]
        public InputHand avatarLeftHand;

        [Tooltip("The input hand that should tracked by the avatar's right hand")]
        public InputHand avatarRightHand;

        [Tooltip("List of all the available avatar models")]
        public List<Avatar> avatars = new List<Avatar>();

        [Tooltip("Index of the active avatar")]
        public int activeAvatarIndex = -1;

        private GameObject avatarContainer;

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)

                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETSingleton{T}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();

            // Check assertions
            if ((avatarHead == null) || (avatarLeftHand == null) || (avatarRightHand == null))
            {
                LogError("The avatar tracking targets are not initialized properly.", nameof(Initialize));
                return;
            }

            // Create a container in the input rig for the avatars
            avatarContainer = new GameObject("Avatars");
            avatarContainer.transform.parent = MRET.InputRig.transform;
            avatarContainer.transform.localPosition = Vector3.zero;
            avatarContainer.transform.localRotation = Quaternion.identity;
            avatarContainer.transform.localScale = Vector3.one;

            // Initialize the avatars
            for (int i=0; i<avatars.Count; i++)
            {
                // Get the avatar prefab reference
                Avatar avatar = avatars[i];

                // Determine if this avatar is the user's avatar
                if ((avatar != null) && (avatar.name == MRET.ConfigurationManager.userPreferences.avatarId))
                {
                    activeAvatarIndex = i;
                    break;
                }
            }

            // Set the active avatar
            SetActiveAvatar(activeAvatarIndex);
        }

        /// <summary>
        /// Creates a runtime copy of the avatar model and places it in the avatar container
        /// </summary>
        /// <param name="avatar"></param>
        protected void InstantiateAvatar(Avatar avatar)
        {
            // Make sure there is a valid model for the avatar
            if (avatar.model && !avatar.IsInstantiated)
            {
                // Instantiate the model for our runtime copy
                avatar.model = Instantiate(avatar.model, avatarContainer.transform);
                avatar.IsInstantiated = true;

                // Make sure we reset the local transform to zero
                avatar.model.transform.localPosition = Vector3.zero;
                avatar.model.transform.localRotation = Quaternion.identity;

                // Dynamically hookup the IK target transforms
#if MRET_EXTENSION_FINALIK
                VRIK ik = avatar.model.gameObject.GetComponent<VRIK>();
                if (ik)
                {
                    // Assign the IK targets
                    ik.solver.spine.headTarget = avatarHead.Target;
                    ik.solver.leftArm.target = avatarLeftHand.Target;
                    ik.solver.rightArm.target = avatarRightHand.Target;
                }
#endif
            }
        }

        /// <summary>
        /// Enables the supplied avatar
        /// </summary>
        /// <param name="avatar">The internal <code>Avatar</code>code> to enable</code></param>
        protected void EnableAvatar(Avatar avatar)
        {
            // Ignore if the configuration manager does not allow for avatars
            if (!MRET.ConfigurationManager.config.Avatars)
            {
                return;
            }

            // Make sure there is a valid model for the avatar
            if ((avatar != null) && (avatar.model != null))
            {
                // Assign the avatar to the rig. NULL is ok.
                MRET.InputRig.avatar = avatar.model;

                // Enable the avatar
                avatar.model.SetActive(true);
            }
        }

        /// <summary>
        /// Disables the supplied avatar
        /// </summary>
        /// <param name="avatar">The internal <code>Avatar</code>code> to disable</code></param>
        protected void DisableAvatar(Avatar avatar)
        {
            // Make sure there is a valid model for the avatar
            if ((avatar != null) && (avatar.model != null))
            {
                // Assign the avatar to the rig. NULL is ok.
                MRET.InputRig.avatar = null;

                // Disable the avatar
                avatar.model.SetActive(false);
            }
        }

        /// <summary>
        /// Updates the target offsets with the settings for the supplied avatar
        /// </summary>
        /// <param name="avatar">The <code>Avatar</code> containing the offsets</param>
        protected void UpdateTargetOffsets(Avatar avatar)
        {
            // Make sure there is a valid model for the avatar
            if ((avatar != null) && (avatar.model != null))
            {
                // Head target
                if (avatarHead)
                {
                    avatarHead.MoveTarget(
                        avatar.ikTargetOffsets.head.position,
                        Quaternion.Euler(avatar.ikTargetOffsets.head.rotation));
                }

                // Left hand target
                if (avatarLeftHand)
                {
                    avatarLeftHand.MoveTarget(
                        avatar.ikTargetOffsets.leftHand.position,
                        Quaternion.Euler(avatar.ikTargetOffsets.leftHand.rotation));
                }

                // Right hand target
                if (avatarRightHand)
                {
                    avatarRightHand.MoveTarget(
                        avatar.ikTargetOffsets.rightHand.position,
                        Quaternion.Euler(avatar.ikTargetOffsets.rightHand.rotation));
                }
            }
        }

        /// <summary>
        /// Updates the user preference settings
        /// </summary>
        /// <param name="avatar">The <code>Avatar</code> to update</param>
        protected void UpdateUserPreferences(Avatar avatar)
        {
            // Make sure the avatar ID is assigned
            MRET.ConfigurationManager.userPreferences.avatarId = avatar.name;

            // Update the blend shapes from the user preferences
            UpdateBlendShapes(avatar, avatar.blendShapeIndices.size,
                MRET.ConfigurationManager.userPreferences.avatarSize);
            UpdateBlendShapes(avatar, avatar.blendShapeIndices.weight,
                MRET.ConfigurationManager.userPreferences.avatarWeight);
            UpdateBlendShapes(avatar, avatar.blendShapeIndices.height,
                MRET.ConfigurationManager.userPreferences.avatarHeight);
            UpdateBlendShapes(avatar, avatar.blendShapeIndices.legLength,
                MRET.ConfigurationManager.userPreferences.avatarLegLength);
            UpdateBlendShapes(avatar, avatar.blendShapeIndices.armLength,
                MRET.ConfigurationManager.userPreferences.avatarArmLength);
        }

        /// <summary>
        /// Updates all the renderer blend shapes for this avatar to the supplied value
        /// </summary>
        /// <param name="avatar">The <code>Avatar</code> to update</param>
        /// <param name="index">The index of all the blend shapes to update</param>
        /// <param name="value">The new weighted value of the blend shapes (0.0 - 1.0)</param>
        protected float UpdateBlendShapes(Avatar avatar, int index, float value)
        {
            float result = value;

            // Make sure we have valid input
            if ((avatar == null) || (avatar.model == null) || (index == BlendShapeIndices.UNDEFINED))
            {
                return value;
            }

            // Log a message
            Log("Updating blendshape index " + index + " for avatar: " + avatar.name, nameof(UpdateBlendShapes));

            // All SkinnedMeshRenderers under Character Meshes GameObject
            SkinnedMeshRenderer[] meshRenderers = avatar.model.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (SkinnedMeshRenderer meshRenderer in meshRenderers)
            {
                // Make sure we have a valid blend shape index
                if (meshRenderer.sharedMesh.blendShapeCount >= index)
                {
                    // Apply the value to the supplied index on this blend shape
                    meshRenderer.SetBlendShapeWeight(index, value);

                    // Read back the value for our return result
                    result = meshRenderer.GetBlendShapeWeight(index);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the active avatar container
        /// </summary>
        /// <returns>The <code>Avatar</code> container or null</returns>
        public Avatar GetActiveAvatar()
        {
            Avatar result = null;

            // Make sure the index is within valid range
            if ((activeAvatarIndex >= 0) && (activeAvatarIndex < avatars.Count))
            {
                // Obtain the avatar container
                result = avatars[activeAvatarIndex];

                // Make sure the avatar is instantiated
                InstantiateAvatar(result);
            }
            else
            {
                // Reset the active index in case we are in a bad internal state
                activeAvatarIndex = -1;
            }

            return result;
        }

        /// <summary>
        /// Returns the active avatar model
        /// </summary>
        /// <returns>The avatar <code>GameObject</code> or null</returns>
        public GameObject GetActiveAvatarModel()
        {
            GameObject result = null;

            // Obtain the internal avatar container
            Avatar avatar = GetActiveAvatar();

            // Make sure the internal avatar container is valid
            if (avatar != null)
            {
                // Obtain the model reference
                result = avatar.model;
            }

            return result;
        }

        /// <summary>
        /// Returns the active avatar thumbnail image
        /// </summary>
        /// <returns>The avatar thumbnail <code>Image</code> or null</returns>
        public Sprite GetActiveAvatarThumbnail()
        {
            Sprite result = null;

            // Obtain the internal active avatar container
            Avatar avatar = GetActiveAvatar();

            // Make sure the internal avatar container is valid
            if (avatar != null)
            {
                // Obtain the model reference
                result = avatar.thumbnail;
            }

            return result;
        }

        /// <summary>
        /// Returns the active avatar thumbnail image
        /// </summary>
        /// <returns>The avatar thumbnail <code>Image</code> or null</returns>
        public GenderType GetActiveAvatarGender()
        {
            GenderType result = Avatar.DEFAULT_GENDER;

            // Obtain the internal active avatar container
            Avatar avatar = GetActiveAvatar();

            // Make sure the internal avatar container is valid
            if (avatar != null)
            {
                // Obtain the model reference
                result = avatar.gender;
            }

            return result;
        }

        /// <summary>
        /// SelectCharacter
        /// 
        /// Disables all base meshes and enables specific model via index character. Uses ModelController methods
        /// Called when model button is pressed under Appearances UI in CharacterCustomizationMenu
        /// </summary>
        public void SetActiveAvatar(int index)
        {
            // Disable active avatar
            DisableAvatar(GetActiveAvatar());

            // Make sure the index is within valid range
            if ((index >= 0) && (index < avatars.Count))
            {
                // Set the active index
                activeAvatarIndex = index;

                // Obtain the avatar container reference so that we can configure it for MRET
                Avatar avatar = GetActiveAvatar();

                // Update the this avatar with the user preferences
                UpdateUserPreferences(avatar);

                // Update the target offsets for this avatar
                UpdateTargetOffsets(avatar);

                // Enable the avatar
                EnableAvatar(avatar);
            }
        }

        /// <summary>
        /// Sets the avatar siza to the supplied normalized value
        /// </summary>
        /// <param name="size">Normalized size in the range [0.0,1.0]</param>
        public void SetAvatarSize(float size)
        {
            // Obtain the internal active avatar container
            Avatar avatar = GetActiveAvatar();

            // Make sure the avatar container is valid
            if (avatar != null)
            {
                // This avatar may not have a blendshape for weight
                if (avatar.blendShapeIndices.weight != BlendShapeIndices.UNDEFINED)
                {
                    // Log a message
                    Log("Updating size for avatar: " + avatar.name, nameof(SetAvatarSize));

                    // Update the blend shapes
                    float _size = UpdateBlendShapes(avatar, avatar.blendShapeIndices.size, size);

                    // Store the size value into the user preferences
                    MRET.ConfigurationManager.userPreferences.avatarSize = _size;
                }
            }
        }

        /// <summary>
        /// Sets the avatar weight to the supplied normalized value
        /// </summary>
        /// <param name="weight">Normalized weight in the range [0.0,1.0]</param>
        public void SetAvatarWeight(float weight)
        {
            // Obtain the internal active avatar container
            Avatar avatar = GetActiveAvatar();

            // Make sure the avatar container is valid
            if (avatar != null)
            {
                // This avatar may not have a blendshape for weight
                if (avatar.blendShapeIndices.weight != BlendShapeIndices.UNDEFINED)
                {
                    // Log a message
                    Log("Updating weight for avatar: " + avatar.name, nameof(SetAvatarWeight));

                    // Update the blend shapes
                    float _weight = UpdateBlendShapes(avatar, avatar.blendShapeIndices.weight, weight);

                    // Store the weight value into the user preferences
                    MRET.ConfigurationManager.userPreferences.avatarWeight = _weight;
                }
            }
        }

        /// <summary>
        /// Sets the avatar height to the supplied normalized value
        /// </summary>
        /// <param name="height">Normalized height in the range [0.0,1.0]</param>
        public void SetAvatarHeight(float height)
        {
            // Obtain the internal active avatar container
            Avatar avatar = GetActiveAvatar();

            // Make sure the avatar container is valid
            if (avatar != null)
            {
                // This avatar may not have a blendshape for height
                if (avatar.blendShapeIndices.height != BlendShapeIndices.UNDEFINED)
                {
                    // Log a message
                    Log("Updating height for avatar: " + avatar.name, nameof(SetAvatarHeight));

                    // Update the blend shapes
                    float _height = UpdateBlendShapes(avatar, avatar.blendShapeIndices.height, height);

                    // Store the height value into the user preferences
                    MRET.ConfigurationManager.userPreferences.avatarHeight = _height;

                    // TODO: Test
                    float newPlayerHeight = Avatar.MAX_HEIGHT_M - ((Avatar.MAX_HEIGHT_M - Avatar.MIN_HEIGHT_M) * _height);
                    MRET.InputRig.PlayerHeight = newPlayerHeight;
                }
            }
        }

        /// <summary>
        /// Sets the avatar leg length to the supplied normalized value
        /// </summary>
        /// <param name="legLength">Normalized leg length in the range [0.0,1.0]</param>
        public void SetAvatarLegLength(float legLength)
        {
            // Obtain the internal active avatar container
            Avatar avatar = GetActiveAvatar();

            // Make sure the avatar container is valid
            if (avatar != null)
            {
                // This avatar may not have a blendshape for leg length
                if (avatar.blendShapeIndices.legLength != BlendShapeIndices.UNDEFINED)
                {
                    // Log a message
                    Log("Updating leg length for avatar: " + avatar.name, nameof(SetAvatarLegLength));

                    // Update the blend shapes
                    float _legLength = UpdateBlendShapes(avatar, avatar.blendShapeIndices.legLength, legLength);

                    // Store the leg length value into the user preferences
                    MRET.ConfigurationManager.userPreferences.avatarLegLength = _legLength;
                }
            }
        }

        /// <summary>
        /// Sets the arm length to the supplied normalized value
        /// </summary>
        /// <param name="armLength">Normalized arm length in the range [0.0,1.0]</param>
        public void SetAvatarArmLength(float armLength)
        {
            // Obtain the internal active avatar container
            Avatar avatar = GetActiveAvatar();

            // Make sure the avatar container is valid
            if (avatar != null)
            {
                // This avatar may not have a blendshape for arm length
                if (avatar.blendShapeIndices.armLength != BlendShapeIndices.UNDEFINED)
                {
                    // Log a message
                    Log("Updating arm length for avatar: " + avatar.name, nameof(SetAvatarArmLength));

                    // Update the blend shapes
                    float _armLength = UpdateBlendShapes(avatar, avatar.blendShapeIndices.armLength, armLength);

                    // Store the arm length value into the user preferences
                    MRET.ConfigurationManager.userPreferences.avatarArmLength = _armLength;
                }
            }
        }
    }

    /// <summary>
    /// Container for an avatar
    /// </summary>
    [System.Serializable]
    public class Avatar
    {
        public const float MAX_HEIGHT_M = 1.83f; // Darcy
        public const float MIN_HEIGHT_M = 1.53f; // Darcy
        public const GenderType DEFAULT_GENDER = GenderType.male;

        [Tooltip("The avatar name")]
        public string name = "";

        [Tooltip("The thumbnail image")]
        public Sprite thumbnail = null;

        [Tooltip("The avatar model")]
        public GameObject model = null;

        [Tooltip("The avatar gender")]
        public GenderType gender = DEFAULT_GENDER;

        [Tooltip("The local offsets for the IK targets")]
        public TargetOffsets ikTargetOffsets;

        [Tooltip("The blend shape indices for morphing")]
        public BlendShapeIndices blendShapeIndices;

        public bool IsInstantiated { get; set; } = false;
    }

    [System.Serializable]
    public class TargetOffsets
    {
        [Tooltip("The local offset for the head target")]
        public TargetOffset head;

        [Tooltip("The local offset for the left hand target")]
        public TargetOffset leftHand;

        [Tooltip("The local offset for the right hand target")]
        public TargetOffset rightHand;
    }

    [System.Serializable]
    public class TargetOffset
    {
        [Tooltip("The local position offset for the target")]
        public Vector3 position;

        [Tooltip("The local rotation offset for the target")]
        public Vector3 rotation;
    }

    [System.Serializable]
    public class BlendShapeIndices
    {
        public const int UNDEFINED = -1;

        [Tooltip("The blend shape index for the size")]
        public int size = UNDEFINED;

        [Tooltip("The blend shape index for weight")]
        public int weight = UNDEFINED;

        [Tooltip("The blend shape index for height")]
        public int height = UNDEFINED;

        [Tooltip("The blend shape index for leg length")]
        public int legLength = UNDEFINED;

        [Tooltip("The blend shape index for arm length")]
        public int armLength = UNDEFINED;
    }

}