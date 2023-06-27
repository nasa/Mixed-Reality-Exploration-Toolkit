// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Tools.Randomize
{
    /**
     * Abstract class to randomize some content of object based upon the assigned property
     * settings. This class will automatically randomize on the current object based upon
     * the property values at <code>Start</code>.
     * 
     * If this instance is disabled, the randomization will not occur until either the
     * instance is enabled, or a direct call to <code>PerformRandomization</code>.<br/>
     * 
     * The random seed for this instance may be specified to ensure deterministic results.
     * If the random seed is not specified, the current random seed is used.<br/>
     * 
     * @author Jeffrey Hosler
     */
    public abstract class Randomize : MonoBehaviour
    {
        public static readonly string NAME = nameof(Randomize);

        public const int DEFAULT_RANDOM_SEED = default;

        [Tooltip("Random number seed. If not explicitly set, will default to the current random seed.")]
        public int randomSeed = DEFAULT_RANDOM_SEED;

        protected virtual void Start()
        {
            // Perform the content randomization of the attached object
            PerformRandomization();
        }

        /**
         * Called to perform the custom randomization of the attached object. This method
         * must be implemented by subclasses.
         */
        protected abstract void PerformObjectRandomization();

        /**
         * Performs the randomization, factoring in the random seed settings if specified
         * 
         * @see #randomSeed
         */
        public void PerformRandomization()
        {
            // Retain the existing random state in case we have to restore it later
            Random.State oldState = Random.state;

            // If the random seed was specifically set, use it
            if (randomSeed != DEFAULT_RANDOM_SEED)
            {
                // Make sure our seed is accurate
                Random.InitState(randomSeed);
            }

            // Perform the object randomization
            PerformObjectRandomization();

            // Restore the old state so we don't mess up logic elsewhere in the application
            if (randomSeed != DEFAULT_RANDOM_SEED)
            {
                Random.state = oldState;
            }
        }

        /**
         * Configures this Randomize instance from the supplied configuration settings.
         * 
         * @param config A <code>RandomizeConfig</code> containing the configuration settings
         * 
         * @see RandomizeConfig
         */
        public virtual void Configure(RandomizeConfig config)
        {
            // Initialize from the settings
            randomSeed = config.randomSeed;
        }

        /**
         * Applies a randomization to the supplied object
         * 
         * @param gameObject The <code>GameObject</code> to perform a randomization
         * @param config The <code>RandomizeConfig</code> containing the settings for the randomization
         * @param enable A boolean value indicating whether the randomization that is created should be
         *      enabled or disabled, controlling whether the randomization is performed as part of the
         *      MonoBehavior (true) or if the calling application wants the randomization performed now (false)
         * 
         * @see Randomize
         * @see RandomizeConfig
         */
        public static void ApplyRandomization<T>(GameObject generatedObject, RandomizeConfig config, bool enable = true) where T : Randomize
        {
            if ((generatedObject != null) && (config != null))
            {
                // Create the randomization component and assign the parent
                Randomize randomization = generatedObject.AddComponent<T>();
                randomization.transform.parent = generatedObject.transform;

                // Let the randomization component configure itself from the supplied config settings
                randomization.Configure(config);

                // Enable the component
                randomization.enabled = enable;

                // Perform the randomization if disabled
                if (!randomization.enabled) randomization.PerformRandomization();
            }
        }

    }

    /**
     * Abstract helper class to contain configuration settings for a Randomize class. Useful
     * for maintaining a set of configuration values for future instantiation of a Randomize
     * class, i.e. deserialization with instantiation at a later time.
     * 
     * This class is maintained here to make maintenance on Randomize easier should changes to
     * the properties need to occur.
     */
    [System.Serializable]
    public abstract class RandomizeConfig
    {
        [Tooltip("The random seed to use for the randomization. If not set, the current random seed is used.")]
        public int randomSeed = Randomize.DEFAULT_RANDOM_SEED;
    }

}