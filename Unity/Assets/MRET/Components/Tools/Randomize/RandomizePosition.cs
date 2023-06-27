// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.Utilities.Statistics;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Tools.Randomize
{
    /**
     * Randomizes an object position based upon the assigned property settings. This
     * class will automatically randomize the position on the current transform based upon
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
     * @author Jeffrey Hosler
     */
    public class RandomizePosition : RandomizeTransform
    {
        public new static readonly string NAME = nameof(RandomizePosition);

        public const RandomizeDistributionFunctionType DEFAULT_DISTRIBUTION_TYPE = RandomizeDistributionFunctionType.Linear;

        public const float DEFAULT_POSITION_MIN = 0.0f;
        public const float DEFAULT_POSITION_MAX = 100.0f;

        public const float DEFAULT_POSITION_MEAN = 0f;
        public const float DEFAULT_POSITION_STANDARD_DEVIATION = 1f;

        [Tooltip("Indicates the distribution function used to calculate the position")]
        public RandomizeDistributionFunctionType distributionType = DEFAULT_DISTRIBUTION_TYPE;

        [Tooltip("Minimum position values (Inclusive)")]
        public Vector3 positionMin = new Vector3(
            DEFAULT_POSITION_MIN,
            DEFAULT_POSITION_MIN,
            DEFAULT_POSITION_MIN);
        [Tooltip("Maximum position values (Inclusive)")]
        public Vector3 positionMax = new Vector3(
            DEFAULT_POSITION_MAX,
            DEFAULT_POSITION_MAX,
            DEFAULT_POSITION_MAX);

        [Tooltip("Normalized position distribution mean values. This controls the position of the normal distribution curve on the axes.")]
        public Vector3 positionMean = new Vector3(
            DEFAULT_POSITION_MEAN,
            DEFAULT_POSITION_MEAN,
            DEFAULT_POSITION_MEAN);
        [Tooltip("Normalized position distribution standard deviation values. This controls the width of the normal distribution curve on the axes. Smaller values are narrower curves.")]
        public Vector3 positionStandardDeviation = new Vector3(
            DEFAULT_POSITION_STANDARD_DEVIATION,
            DEFAULT_POSITION_STANDARD_DEVIATION,
            DEFAULT_POSITION_STANDARD_DEVIATION);

        /**
         * Overridden to perform the position randomization
         */
        protected override void PerformObjectRandomization()
        {
            float x, y, z;

            // Calculate the position
            switch (distributionType)
            {
                case RandomizeDistributionFunctionType.Normal:
                    // Perform a normailzed distribution using Box Muller
                    x = randomizeX ? StatisticsUtil.BoxMuller(positionMean.x, positionStandardDeviation.x) : transform.localPosition.x;
                    y = randomizeY ? StatisticsUtil.BoxMuller(positionMean.y, positionStandardDeviation.y) : transform.localPosition.y;
                    z = randomizeZ ? StatisticsUtil.BoxMuller(positionMean.z, positionStandardDeviation.z) : transform.localPosition.z;
                    break;

                case RandomizeDistributionFunctionType.Linear:
                default:
                    // Linear distribution
                    x = randomizeX ? Random.Range(positionMin.x, positionMax.x) : transform.localPosition.x;
                    y = randomizeY ? Random.Range(positionMin.y, positionMax.y) : transform.localPosition.y;
                    z = randomizeZ ? Random.Range(positionMin.z, positionMax.z) : transform.localPosition.z;
                    break;
            }

            // Apply a random position to the attached transform
            transform.localPosition = new Vector3(x, y, z);
        }

        /**
         * Configures this RandomizePosition instance from the supplied configuration settings.
         * 
         * @param config A <code>RandomizeConfig</code> containing the configuration settings
         * 
         * @see RandomizeConfig
         */
        public override void Configure(RandomizeConfig config)
        {
            // Take the inherited behavior
            base.Configure(config);

            // Process the position specific configuration
            if (config is RandomizePositionConfig)
            {
                RandomizePositionConfig positionConfig = config as RandomizePositionConfig;

                // Configure the setting based upon the distribution function
                distributionType = positionConfig.distributionType;
                switch (distributionType)
                {
                    case RandomizeDistributionFunctionType.Normal:
                        // Normal distribution
                        positionMean = new Vector3(
                            positionConfig.positionMean.x,
                            positionConfig.positionMean.y,
                            positionConfig.positionMean.z);
                        positionStandardDeviation = new Vector3(
                            positionConfig.positionStandardDeviation.x,
                            positionConfig.positionStandardDeviation.y,
                            positionConfig.positionStandardDeviation.z);
                        break;

                    case RandomizeDistributionFunctionType.Linear:
                    default:
                        // Linear distribution
                        positionMin = new Vector3(
                            positionConfig.positionMin.x,
                            positionConfig.positionMin.y,
                            positionConfig.positionMin.z);
                        positionMax = new Vector3(
                            positionConfig.positionMax.x,
                            positionConfig.positionMax.y,
                            positionConfig.positionMax.z);
                        break;
                }

            }
            else
            {
                Debug.LogWarning("[" + NAME + "]: Unexpected configuration type: " + nameof(RandomizeTransformConfig));
            }
        }

    }

    /**
    * Helper class to contain configuration settings for a RandomizePosition class. Useful
    * for maintaining a set of configuration values for future instantiation of a RandomizePosition
    * class, i.e. deserialization with instantiation at a later time.
    * 
    * This class is maintained here to make maintenance on RandomizePosition easier should changes to
    * the properties need to occur.
    */
    [System.Serializable]
    public class RandomizePositionConfig : RandomizeTransformConfig
    {
        [Tooltip("Indicates the distribution function used to position the generated object")]
        public RandomizeDistributionFunctionType distributionType = RandomizePosition.DEFAULT_DISTRIBUTION_TYPE;

        [Tooltip("Mean position used to place the generated object (the position of the normal distribution curve on the axis). Only used if distribution type is 'Normal'")]
        public Vector3 positionMean = new Vector3(
            RandomizePosition.DEFAULT_POSITION_MEAN,
            RandomizePosition.DEFAULT_POSITION_MEAN,
            RandomizePosition.DEFAULT_POSITION_MEAN);
        [Tooltip("Standard deviation used to place the generated object (the width of the normal distribution curve on the axis). Smaller values are narrower curves. Only used if distribution type is 'Normal'")]
        public Vector3 positionStandardDeviation = new Vector3(
            RandomizePosition.DEFAULT_POSITION_STANDARD_DEVIATION,
            RandomizePosition.DEFAULT_POSITION_STANDARD_DEVIATION,
            RandomizePosition.DEFAULT_POSITION_STANDARD_DEVIATION);

        [Tooltip("Minimum position from the transform of this object to place the generated object. Only used if distribution type is 'Linear'")]
        public Vector3 positionMin = new Vector3(
            RandomizePosition.DEFAULT_POSITION_MIN,
            RandomizePosition.DEFAULT_POSITION_MIN,
            RandomizePosition.DEFAULT_POSITION_MIN);
        [Tooltip("Maximum position from the transform of this object to place the generated object. Only used if distribution type is 'Linear'")]
        public Vector3 positionMax = new Vector3(
            RandomizePosition.DEFAULT_POSITION_MAX,
            RandomizePosition.DEFAULT_POSITION_MAX,
            RandomizePosition.DEFAULT_POSITION_MAX);
    }

}