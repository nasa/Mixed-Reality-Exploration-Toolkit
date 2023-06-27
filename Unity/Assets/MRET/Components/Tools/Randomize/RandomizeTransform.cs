// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Tools.Randomize
{
    /**
     * Abstract class to randomize an object transform based upon the assigned property
     * settings. This class will automatically randomize on the current transform based upon
     * the property values at <code>Start</code>.
     * 
     * If this instance is disabled, the randomization will not occur until either the
     * instance is enabled, or a direct call to <code>PerformRandomization</code>.<br/>
     * 
     * The random seed for this instance may be specified to ensure deterministic results.
     * If the random seed is not specified, the current random seed is used.<br/>
     * 
     * @see Randomize
     * 
     * @author Jeffrey Hosler
     */
    public abstract class RandomizeTransform : Randomize
    {
        public new static readonly string NAME = nameof(RandomizeTransform);

        public const bool DEFAULT_RANDOM_X = true;
        public const bool DEFAULT_RANDOM_Y = true;
        public const bool DEFAULT_RANDOM_Z = true;

        [Tooltip("Flag to indicate whether the X axis is randomized")]
        public bool randomizeX = DEFAULT_RANDOM_X;
        [Tooltip("Flag to indicate whether the Y axis is randomized")]
        public bool randomizeY = DEFAULT_RANDOM_Y;
        [Tooltip("Flag to indicate whether the Z axis is randomized")]
        public bool randomizeZ = DEFAULT_RANDOM_Z;

        /**
         * Configures this RandomizeTransform instance from the supplied configuration settings.
         * 
         * @param config A <code>RandomizeConfig</code> containing the configuration settings
         * 
         * @see RandomizeConfig
         */
        public override void Configure(RandomizeConfig config)
        {
            // Take the inherited behavior
            base.Configure(config);

            // Process the transform specific configuration
            if (config is RandomizeTransformConfig)
            {
                RandomizeTransformConfig transformConfig = config as RandomizeTransformConfig;

                randomizeX = transformConfig.randomizeX;
                randomizeY = transformConfig.randomizeY;
                randomizeZ = transformConfig.randomizeZ;
            }
            else
            {
                Debug.LogWarning("[" + NAME + "]: Unexpected configuration type: " + nameof(RandomizeTransformConfig));
            }
        }

    }

    /**
     * Helper class to contain configuration settings for a RandomizeTransform class. Useful
     * for maintaining a set of configuration values for future instantiation of a RandomizeTransform
     * class, i.e. deserialization with instantiation at a later time.
     * 
     * This class is maintained here to make maintenance on RandomizeTransform easier should changes to
     * the properties need to occur.
     */
    [System.Serializable]
    public abstract class RandomizeTransformConfig : RandomizeConfig
    {
        [Tooltip("Flag to indicate if the X axis should be randomized")]
        public bool randomizeX = RandomizeTransform.DEFAULT_RANDOM_X;
        [Tooltip("Flag to indicate if the Y axis should be randomized")]
        public bool randomizeY = RandomizeTransform.DEFAULT_RANDOM_Y;
        [Tooltip("Flag to indicate if the Z axis should be randomized")]
        public bool randomizeZ = RandomizeTransform.DEFAULT_RANDOM_Z;
    }

}