// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Tools.Randomize
{
    /**
     * Randomizes an object rotation based upon the assigned property settings. This
     * class will automatically randomize the rotation on the current transform based upon
     * the property values at <code>Start</code>.
     * 
     * If this instance is disabled, the randomization will not occur until either the
     * instance is enabled, or a direct call to <code>PerformRandomization</code>.<br/>
     * 
     * The random seed for this instance may be specified to ensure deterministic results.
     * If the random seed is not specified, the current random seed is used.<br/>
     * 
     * @see RandomizeTransform
     * 
     * @author Weston Bell-Geddes (Initial concept)
     * @author Jeffrey Hosler (Reimplemented for better serialization and packaged into Randomize framework)
     */
    public class RandomizeRotation : RandomizeTransform
    {
        public new static readonly string NAME = nameof(RandomizeRotation);

        public const float DEFAULT_ROTATION_MIN = 0.0f;
        public const float DEFAULT_ROTATION_MAX = 360.0f;

        [Tooltip("Minimum rotatation angle in degrees (Inclusive)")]
        public Vector3 rotationMin = new Vector3(
            DEFAULT_ROTATION_MIN,
            DEFAULT_ROTATION_MIN,
            DEFAULT_ROTATION_MIN);
        [Tooltip("Maximum rotatation angle in degrees (Inclusive)")]
        public Vector3 rotationMax = new Vector3(
            DEFAULT_ROTATION_MAX,
            DEFAULT_ROTATION_MAX,
            DEFAULT_ROTATION_MAX);

        /**
         * Overridden to perform the rotation randomization
         */
        protected override void PerformObjectRandomization()
        {
            // Randomize the axes independently if enabled, preserving existing values if not
            float x = randomizeX ? Random.Range(rotationMin.x, rotationMax.x) : transform.localRotation.eulerAngles.x;
            float y = randomizeY ? Random.Range(rotationMin.y, rotationMax.y) : transform.localRotation.eulerAngles.y;
            float z = randomizeZ ? Random.Range(rotationMin.z, rotationMax.z) : transform.localRotation.eulerAngles.z;

            // Apply a random rotation to the attached transform
            transform.rotation = Quaternion.Euler(x, y, z);
        }

        /**
         * Configures this RandomizeRotation instance from the supplied configuration settings.
         * 
         * @param config A <code>RandomizeConfig</code> containing the configuration settings
         * 
         * @see RandomizeConfig
         */
        public override void Configure(RandomizeConfig config)
        {
            // Take the inherited behavior
            base.Configure(config);

            // Process the rotation specific configuration
            if (config is RandomizeRotationConfig)
            {
                RandomizeRotationConfig rotationConfig = config as RandomizeRotationConfig;

                rotationMin = new Vector3(
                    rotationConfig.rotationMin.x,
                    rotationConfig.rotationMin.y,
                    rotationConfig.rotationMin.z);
                rotationMax = new Vector3(
                    rotationConfig.rotationMax.x,
                    rotationConfig.rotationMax.y,
                    rotationConfig.rotationMax.z);
            }
            else
            {
                Debug.LogWarning("[" + NAME + "]: Unexpected configuration type: " + nameof(RandomizeTransformConfig));
            }
        }

    }

    /**
     * Helper class to contain configuration settings for a RandomizeRotation class. Useful
     * for maintaining a set of configuration values for future instantiation of a RandomizeRotation
     * class, i.e. deserialization with instantiation at a later time.
     * 
     * This class is maintained here to make maintenance on RandomizeRotation easier should changes to
     * the properties need to occur.
     */
    [System.Serializable]
    public class RandomizeRotationConfig : RandomizeTransformConfig
    {
        [Tooltip("Minimum rotation to apply to the generated object")]
        public Vector3 rotationMin = new Vector3(
            RandomizeRotation.DEFAULT_ROTATION_MIN,
            RandomizeRotation.DEFAULT_ROTATION_MIN,
            RandomizeRotation.DEFAULT_ROTATION_MIN);
        [Tooltip("maximum rotation to apply to the generated object")]
        public Vector3 rotationMax = new Vector3(
            RandomizeRotation.DEFAULT_ROTATION_MAX,
            RandomizeRotation.DEFAULT_ROTATION_MAX,
            RandomizeRotation.DEFAULT_ROTATION_MAX);
    }

}