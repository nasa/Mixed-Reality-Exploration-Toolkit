// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET.Project
{
    /// <remarks>
    /// History:
    /// 13 Sep 2021: Created (Jeffrey Hosler)
    /// </remarks>
	///
	/// <summary>
	/// The MRET project environment.<br>
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public class Environment : XRObject<EnvironmentType>, IEnvironment<EnvironmentType>
	{
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(Environment);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private EnvironmentType serializedEnvironment;

        public ReflectionProbe[] ReflectionProbes => GetComponentsInChildren<ReflectionProbe>();

        #region MRETUpdateBehaviour
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
        #endregion MRETUpdateBehaviour

        #region Serializable
        /// <summary>
        /// Asynchronously Deserializes the supplied serialized asset and updates the supplied state
        /// with the resulting asset.
        /// </summary>
        /// <param name="serializedScene">The serialized <code>AssetType</code> scene</param>
        /// <param name="sceneDeserializationState">The <code>SerializationState</code> to populate with the state.</param>
        /// 
        /// <see cref="AssetType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected IEnumerator DeserializeScene(AssetType serializedScene, SerializationState sceneDeserializationState)
        {
            Action<bool> SceneLoadedAction = (bool sceneLoaded) =>
            {
                if (sceneLoaded)
                {
                    // Transfer all the game objects in the scene to our hierarchy.
                    Scene scene = SceneManager.GetSceneByName(serializedScene.AssetName);
                    if (scene != null)
                    {
                        GameObject[] sceneGameObjects = scene.GetRootGameObjects();
                        if (sceneGameObjects.Length > 0)
                        {
                            foreach (GameObject sceneObject in sceneGameObjects)
                            {
                                sceneObject.transform.SetParent(transform);
                            }
                        }

                        // Unload the scene now that we've transferred the game objects
                        SceneManager.UnloadSceneAsync(scene);
                    }

                    // Mark as complete
                    sceneDeserializationState.complete = true;
                }
                else
                {
                    // Record the error
                    sceneDeserializationState.Error("A problem was encountered deserializing the scene");
                }
            };

            // Load the model
            SchemaUtil.DeserializeScene(serializedScene, true, SceneLoadedAction);

            // Wait for the model deserialization process to finish
            while (!sceneDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            yield return null;
        }

        /// <summary>
        /// Asynchronously Deserializes the supplied serialized skybox and updates the supplied state
        /// with the deserialization state.
        /// </summary>
        /// <param name="serializedSkyBox">The serialized <code>SkyBoxType</code> skybox settings</param>
        /// <param name="skyboxDeserializationState">The <code>SerializationState</code> to populate with the state.</param>
        /// 
        /// <see cref="SkyBoxType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected IEnumerator DeserializeSkyBox(MaterialType serializedSkyBox, SerializationState skyboxDeserializationState)
        {
            Action<Material> SkyboxLoadedAction = (Material loadedMaterial) =>
            {
                // Make sure the load finished successfully
                if (loadedMaterial != null)
                {
                    RenderSettings.skybox = loadedMaterial;
                    DynamicGI.UpdateEnvironment();

                    // Mark as complete
                    skyboxDeserializationState.complete = true;
                }
                else
                {
                    skyboxDeserializationState.Error("The skybox material asset could not be loaded");
                }
            };

            // Deserialize the skybox
            SchemaUtil.DeserializeMaterial(serializedSkyBox, SkyboxLoadedAction);

            // Wait for the deserialization to complete
            while (!skyboxDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            yield return null;
        }

        /// <seealso cref="XRObject{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(EnvironmentType serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Process this object specific deserialization

            // Save the serialized reference
            serializedEnvironment = serialized;

            // Perform the asset deserialization
            if (serializedEnvironment.Asset != null)
            {
                AssetType serializedScene = serializedEnvironment.Asset;

                // Deserialize the scene
                SerializationState sceneDeserializationState = new SerializationState();
                StartCoroutine(DeserializeScene(serializedScene, sceneDeserializationState));

                // Wait for the coroutine to complete
                while (!sceneDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(sceneDeserializationState);

                // If the asset loading failed, exit with an error
                if (deserializationState.IsError) yield break;
            }

            // Deserialize the skybox
            if ((serializedEnvironment.SkyBox != null) && (serializedEnvironment.SkyBox.Items != null))
            {
                SerializationState skyboxDeserializationState = new SerializationState();
                StartCoroutine(DeserializeSkyBox(serializedEnvironment.SkyBox, skyboxDeserializationState));

                // Wait for the coroutine to complete
                while (!skyboxDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state, but continue on error
                if (skyboxDeserializationState.IsError)
                {
                    LogWarning(skyboxDeserializationState.ErrorMessage, nameof(Deserialize));
                }
            }

            // Deserialize gravity
            if (serializedEnvironment.Gravity != null)
            {
                float gravity = -Physics.gravity.y;
                SchemaUtil.DeserializeAcceleration(serializedEnvironment.Gravity, ref gravity);
                Physics.gravity = new Vector3(0f, -gravity, 0f);
            }

            // Deserialize the clipping planes
            if (serializedEnvironment.ClippingPlane != null)
            {
                // Set the user's clipping planes.
                Camera headsetCam = MRET.InputRig.activeCamera;
                if (headsetCam != null)
                {
                    headsetCam.nearClipPlane = serializedEnvironment.ClippingPlane.Near;
                    headsetCam.farClipPlane = serializedEnvironment.ClippingPlane.Far;
                }
            }

            // Deserialize the locomotion settings
            if (serializedEnvironment.LocomotionSettings != null)
            {
                LocomotionSettingsType serializedLocomotionSettings = serializedEnvironment.LocomotionSettings;

                // Constraints
                GravityConstraint gravityContraint = GravityConstraint.Allowed;
                float slow = float.NaN, normal = float.NaN, fast = float.NaN;

                // Armswing
                if (serializedLocomotionSettings.Armswing != null)
                {
                    // Deserialize the constraints
                    SchemaUtil.DeserializeLocomotionConstraints(serializedLocomotionSettings.Armswing,
                        ref gravityContraint, ref slow, ref normal, ref fast);

                    // Set the constraints
                    MRET.LocomotionManager.ArmswingGravityConstraint = gravityContraint;
                    if (!float.IsNaN(slow)) MRET.LocomotionManager.ArmswingSlowMotionConstraintMultiplier = slow;
                    if (!float.IsNaN(normal)) MRET.LocomotionManager.ArmswingNormalMotionConstraintMultiplier = normal;
                    if (!float.IsNaN(fast)) MRET.LocomotionManager.ArmswingFastMotionConstraintMultiplier = fast;
                }

                // Flying
                if (serializedLocomotionSettings.Fly != null)
                {
                    // Deserialize the constraints
                    SchemaUtil.DeserializeLocomotionConstraints(serializedLocomotionSettings.Fly,
                        ref gravityContraint, ref slow, ref normal, ref fast);

                    // Set the constraints
                    MRET.LocomotionManager.FlyingGravityConstraint = gravityContraint;
                    if (!float.IsNaN(slow)) MRET.LocomotionManager.FlyingSlowMotionConstraintMultiplier = slow;
                    if (!float.IsNaN(normal)) MRET.LocomotionManager.FlyingNormalMotionConstraintMultiplier = normal;
                    if (!float.IsNaN(fast)) MRET.LocomotionManager.FlyingFastMotionConstraintMultiplier = fast;
                }

                // Navigation
                if (serializedLocomotionSettings.Navigation != null)
                {
                    // Deserialize the constraints
                    SchemaUtil.DeserializeLocomotionConstraints(serializedLocomotionSettings.Navigation,
                        ref gravityContraint, ref slow, ref normal, ref fast);

                    // Set the constraints
                    MRET.LocomotionManager.NavigationGravityConstraint = gravityContraint;
                    if (!float.IsNaN(slow)) MRET.LocomotionManager.NavigationSlowMotionConstraintMultiplier = slow;
                    if (!float.IsNaN(normal)) MRET.LocomotionManager.NavigationNormalMotionConstraintMultiplier = normal;
                    if (!float.IsNaN(fast)) MRET.LocomotionManager.NavigationFastMotionConstraintMultiplier = fast;
                }

                // Teleport
                if (serializedLocomotionSettings.Teleport != null)
                {
                    if (serializedLocomotionSettings.Teleport.MaxDistanceSpecified)
                    {
                        MRET.LocomotionManager.TeleportMaxDistance = serializedLocomotionSettings.Teleport.MaxDistance;
                    }
                }
            }

            // Force a reload of the reflection probes
            ReloadReflectionProbes();

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <seealso cref="XRObject{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(EnvironmentType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Serialize the object specific configuration

            // Asset (optional)
            // Start with our internal serialized scene asset to serialize out the asset
            // using the original deserialized structure (if was provided during deserialization)
            AssetType serializedScene = null;
            if (serializedEnvironment != null)
            {
                // Use this scene asset structure
                serializedScene = serializedEnvironment.Asset;
            }

            // Serialize out the scene asset
            serialized.Asset = serializedScene;

            // Skybox (required)
            MaterialType serializedSkybox = null;
            if (serializedEnvironment != null)
            {
                // Use this skybox structure
                serializedSkybox = serializedEnvironment.SkyBox;
            }

            // Skybox is required so create a default if not available
            if (serializedSkybox == null)
            {
                serializedSkybox = new MaterialType();
            }

            // Serialize out the skybox
            serialized.SkyBox = serializedSkybox;

            // Gravity (optional)
            AccelerationType serializedGravity = null;
            if (serializedEnvironment != null)
            {
                serializedGravity = serializedEnvironment.Gravity;
                if (serializedGravity != null)
                {
                    float gravity = -Physics.gravity.y;
                    SchemaUtil.SerializeAcceleration(gravity, serializedGravity);
                }
            }
            serialized.Gravity = serializedGravity;

            // Serialize the clipping planes
            EnvironmentTypeClippingPlane serializedClippingPlane = null;
            if (serializedEnvironment.ClippingPlane != null)
            {
                serializedClippingPlane = serializedEnvironment.ClippingPlane;
                Camera headsetCam = MRET.InputRig.activeCamera;
                if (headsetCam != null)
                {
                    serializedClippingPlane.Near = headsetCam.nearClipPlane;
                    serializedClippingPlane.Far = headsetCam.farClipPlane;
                }
            }
            serialized.ClippingPlane = serializedClippingPlane;

            // Serialize the locomotion settings
            LocomotionSettingsType serializedLocomotionSettings = null;
            if (serializedEnvironment != null)
            {
                // Use this locomotion settings structure
                serializedLocomotionSettings = serializedEnvironment.LocomotionSettings;

                // Serialize the locomotion settings
                if (serializedLocomotionSettings != null)
                {
                    // Constraints
                    GravityConstraint gravityContraint = GravityConstraint.Allowed;
                    float slow = float.NaN, normal = float.NaN, fast = float.NaN;

                    // Armswing
                    LocomotionConstraintsType serializedArmswing = null;
                    if (serializedLocomotionSettings.Armswing != null)
                    {
                        serializedArmswing = serializedLocomotionSettings.Armswing;

                        // Get the constraints
                        gravityContraint = MRET.LocomotionManager.ArmswingGravityConstraint;
                        slow = MRET.LocomotionManager.ArmswingSlowMotionConstraintMultiplier;
                        normal = MRET.LocomotionManager.ArmswingNormalMotionConstraintMultiplier;
                        fast = MRET.LocomotionManager.ArmswingFastMotionConstraintMultiplier;

                        // Serialize the constraints
                        SchemaUtil.SerializeLocomotionConstraints(gravityContraint, slow, normal, fast, serializedArmswing);
                    }
                    serializedLocomotionSettings.Armswing = serializedArmswing;

                    // Flying
                    LocomotionConstraintsType serializedFlying = null;
                    if (serializedLocomotionSettings.Fly != null)
                    {
                        serializedFlying = serializedLocomotionSettings.Fly;

                        // Get the constraints
                        gravityContraint = MRET.LocomotionManager.FlyingGravityConstraint;
                        slow = MRET.LocomotionManager.FlyingSlowMotionConstraintMultiplier;
                        normal = MRET.LocomotionManager.FlyingNormalMotionConstraintMultiplier;
                        fast = MRET.LocomotionManager.FlyingFastMotionConstraintMultiplier;

                        // Serialize the constraints
                        SchemaUtil.SerializeLocomotionConstraints(gravityContraint, slow, normal, fast, serializedFlying);
                    }
                    serializedLocomotionSettings.Fly = serializedFlying;

                    // Navigation
                    LocomotionConstraintsType serializedNavigation = null;
                    if (serializedLocomotionSettings.Navigation != null)
                    {
                        serializedNavigation = serializedLocomotionSettings.Navigation;

                        // Get the constraints
                        gravityContraint = MRET.LocomotionManager.NavigationGravityConstraint;
                        slow = MRET.LocomotionManager.NavigationSlowMotionConstraintMultiplier;
                        normal = MRET.LocomotionManager.NavigationNormalMotionConstraintMultiplier;
                        fast = MRET.LocomotionManager.NavigationFastMotionConstraintMultiplier;

                        // Serialize the constraints
                        SchemaUtil.SerializeLocomotionConstraints(gravityContraint, slow, normal, fast, serializedNavigation);
                    }
                    serializedLocomotionSettings.Navigation = serializedNavigation;

                    // Teleport
                    LocomotionSettingsTypeTeleport serializedTeleport = null;
                    if (serializedLocomotionSettings.Teleport != null)
                    {
                        serializedTeleport = serializedLocomotionSettings.Teleport;
                        serializedTeleport.MaxDistanceSpecified = true;
                        serializedTeleport.MaxDistance = MRET.LocomotionManager.TeleportMaxDistance;
                    }
                    serializedLocomotionSettings.Teleport = serializedTeleport;
                }
            }
            serialized.LocomotionSettings = serializedLocomotionSettings;

            // Save the final serialized reference
            serializedEnvironment = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serializable

        /// <summary>
        /// Reloads the reflection probes
        /// </summary>
        protected virtual void ReloadReflectionProbes()
        {
            foreach (ReflectionProbe rProbe in ReflectionProbes)
            {
                // Reload all reflection probes.
                if (rProbe.enabled)
                {
                    rProbe.enabled = false;
                    rProbe.enabled = true;
                }

                // Force realtime reflection probes to reinitialize.
                if (rProbe.mode == UnityEngine.Rendering.ReflectionProbeMode.Realtime)
                {
                    rProbe.mode = UnityEngine.Rendering.ReflectionProbeMode.Baked;
                    rProbe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
                }
            }
        }

    }
}
