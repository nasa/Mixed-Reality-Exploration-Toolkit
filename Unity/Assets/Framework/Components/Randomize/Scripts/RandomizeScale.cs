using UnityEngine;

namespace GSFC.ARVR.MRET.Randomize
{
    /**
     * Randomizes an object scale based upon the assigned property settings. This
     * class will automatically randomize the scale on the current transform based upon
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
    public class RandomizeScale : RandomizeTransform
    {
        public new static readonly string NAME = nameof(RandomizeScale);

        public const bool DEFAULT_MAINTAIN_AXIAL_RATIO = true;

        public const float DEFAULT_SCALE_MIN = 0.1f;
        public const float DEFAULT_SCALE_MAX = 1.0f;

        [Tooltip("Flag to indicate whether the the axial ratio should be maintained when scale is randomized")]
        public bool maintainAxialRatio = DEFAULT_MAINTAIN_AXIAL_RATIO;

        [Tooltip("Minimum scale value (Inclusive). Used when maintaining axial ratio")]
        public float maintainAxialRatioScaleMin = DEFAULT_SCALE_MIN;
        [Tooltip("Maximum scale value (Inclusive). Used when maintaining axial ratio")]
        public float maintainAxialRatioScaleMax = DEFAULT_SCALE_MAX;

        [Tooltip("Minimum scale values (Inclusive)")]
        public Vector3 scaleMin = new Vector3(
             DEFAULT_SCALE_MIN,
             DEFAULT_SCALE_MIN,
             DEFAULT_SCALE_MIN);
        [Tooltip("Maximum scale values (Inclusive)")]
        public Vector3 scaleMax = new Vector3(
            DEFAULT_SCALE_MAX,
            DEFAULT_SCALE_MAX,
            DEFAULT_SCALE_MAX);

        /**
         * Overridden to perform the scale randomization
         */
        protected override void PerformObjectRandomization()
        {
            // Generate the random values
            float x, y, z;
            if (maintainAxialRatio)
            {
                // Assign all axes the same value
                x = y = z = Random.Range(maintainAxialRatioScaleMin, maintainAxialRatioScaleMax);
            }
            else
            {
                // Randomize the axes independently if enabled, preserving existing values if not
                x = randomizeX ? Random.Range(scaleMin.x, scaleMax.x) : transform.localScale.x;
                y = randomizeY ? Random.Range(scaleMin.y, scaleMax.y) : transform.localScale.y;
                z = randomizeZ ? Random.Range(scaleMin.z, scaleMax.z) : transform.localScale.z;
            }

            // Apply a random scale to the attached transform
            transform.localScale = new Vector3(x, y, z);
        }

        /**
         * Configures this RandomizeScale instance from the supplied configuration settings.
         * 
         * @param config A <code>RandomizeConfig</code> containing the configuration settings
         * 
         * @see RandomizeConfig
         */
        public override void Configure(RandomizeConfig config)
        {
            // Take the inherited behavior
            base.Configure(config);

            // Process the scale specific configuration
            if (config is RandomizeScaleConfig)
            {
                RandomizeScaleConfig scaleConfig = config as RandomizeScaleConfig;

                // Configure the setting based upon the maintain axial ratio settings
                maintainAxialRatio = scaleConfig.maintainAxialRatio;
                if (maintainAxialRatio)
                {
                    // Maintain axial ratio
                    maintainAxialRatioScaleMin = scaleConfig.maintainAxialRatioScaleMin;
                    maintainAxialRatioScaleMax = scaleConfig.maintainAxialRatioScaleMax;
                }
                else
                {
                    // Axes are independent
                    scaleMin = new Vector3(
                        scaleConfig.scaleMin.x,
                        scaleConfig.scaleMin.y,
                        scaleConfig.scaleMin.z);
                    scaleMax = new Vector3(
                        scaleConfig.scaleMax.x,
                        scaleConfig.scaleMax.y,
                        scaleConfig.scaleMax.z);
                }
            }
            else
            {
                Debug.LogWarning("[" + NAME + "]: Unexpected configuration type: " + nameof(RandomizeTransformConfig));
            }
        }

    }

    /**
    * Helper class to contain configuration settings for a RandomizeScale class. Useful
    * for maintaining a set of configuration values for future instantiation of a RandomizeScale
    * class, i.e. deserialization with instantiation at a later time.
    * 
    * This class is maintained here to make maintenance on RandomizeScale easier should changes to
    * the properties need to occur.
    */
    [System.Serializable]
    public class RandomizeScaleConfig : RandomizeTransformConfig
    {
        public bool maintainAxialRatio = RandomizeScale.DEFAULT_MAINTAIN_AXIAL_RATIO;

        [Tooltip("Minimum scale to apply to the generated object. Only used if maintain axial ratio is true.")]
        public float maintainAxialRatioScaleMin = RandomizeScale.DEFAULT_SCALE_MIN;
        [Tooltip("Maximum scale to apply to the generated object. Only used if maintain axial ratio is true.")]
        public float maintainAxialRatioScaleMax = RandomizeScale.DEFAULT_SCALE_MAX;

        [Tooltip("Minimum scale to apply to the generated object. Only used if maintain axial ratio is false.")]
        public Vector3 scaleMin = new Vector3(
            RandomizeScale.DEFAULT_SCALE_MIN,
            RandomizeScale.DEFAULT_SCALE_MIN,
            RandomizeScale.DEFAULT_SCALE_MIN);
        [Tooltip("Maximum scale to apply to the generated object. Only used if maintain axial ratio is false.")]
        public Vector3 scaleMax = new Vector3(
            RandomizeScale.DEFAULT_SCALE_MAX,
            RandomizeScale.DEFAULT_SCALE_MAX,
            RandomizeScale.DEFAULT_SCALE_MAX);
    }

}