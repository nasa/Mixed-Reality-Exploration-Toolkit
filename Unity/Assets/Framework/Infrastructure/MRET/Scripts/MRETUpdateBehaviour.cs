// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;

namespace GSFC.ARVR.MRET
{
    public enum UpdateFrequency
    {
        HzCustom = 0,
        Hz1 = 1,
        Hz2 = 2,
        Hz3 = 3,
        Hz4 = 4,
        Hz5 = 5,
        Hz10 = 10,
        Hz20 = 20,
        Hz30 = 30,
        Hz60 = 60,
        Hz120 = 120,
        HzMaximum = int.MaxValue
    };

    /// <remarks>
    /// History:
    /// 16 February 2021: Created. Migrated the update functionality from MRETBehaviour to this
    ///     subclass to allow for MRET classes that do not need periodic updating to extend the
    ///     MRETBehaviour base class. Classes that require periodic updating would extend this
    ///     class.
    /// </remarks>
    ///
    /// <summary>
    /// MRETUpdateBehaviour
    /// 
    /// The base class for all MRET infrastructure/components that require updating. This class
    /// extends the MRETBehaviour class to introduce performance managed update behavior.<br>
    ///
    /// Author: Jeffrey C. Hosler
    /// </summary>
    /// 
	public abstract class MRETUpdateBehaviour : MRETBehaviour
	{
        public const UpdateFrequency DEFAULT_UPDATERATE = UpdateFrequency.Hz1;

        [Tooltip("Specifies the desired update rate in Hertz.")]
        public UpdateFrequency updateRate = DEFAULT_UPDATERATE;

        [Tooltip("Specifies a custom update rate in Hertz. Only used when the update rate is set to custom.")]
        public float customRate = 0;

        private float activeHertz = -1f;

        /// <value>Used for tracking when to perform updates</value>
        private DateTime currentTime;
        private DateTime lastUpdate;

        /// <value>Update period in Milliseconds</value>
        protected float updatePeriodMs
        {
            get
            {
                return GetUpdatePeriodMs();
            }
        }

        /// <summary>
        /// Calculates the update period in milliseconds based upon the public property settings. This value
        /// is calculated on demand to allow for the update rate to be adjusted during execution if desired.<br>
        /// </summary>
        /// 
        /// <returns>Update period in Milliseconds based upon the public property values. ((1 / updateRate) * 1000)</returns>
        private float GetUpdatePeriodMs()
        {
            // Obtain the update rate in Hertz
            float hertz = (float)updateRate;
            if (updateRate == UpdateFrequency.HzCustom)
            {
                hertz = customRate;
            }

            // Validate the update rate
            if (hertz <= 0)
            {
                Debug.LogWarning("[" + ClassName + "; " + name + "] Invalid update rate specified. Defaulting to " +
                    (int)DEFAULT_UPDATERATE + "Hz.");
                hertz = (float)DEFAULT_UPDATERATE;
            }

            // Check to see if the hertz is different than what we were using before and save it
            if (hertz.CompareTo(activeHertz) != 0)
            {
                // Log the frequency being used
                string hzStr = (updateRate == UpdateFrequency.HzMaximum) ? "maximum" : hertz.ToString();
                Debug.Log("[" + ClassName + "; " + name + "] running at " + hzStr + " Hz");
                activeHertz = hertz;
            }

            // Calculate the period in milliseconds
            return ((1f / hertz) * 1000f);
        }

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            return base.IntegrityCheck();
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            // Initialize the times we will use to control the udpate rate
            currentTime = DateTime.UtcNow;
            lastUpdate = default(DateTime);

#if MRET_DEBUG
            Debug.Log("[" + ClassName + "->" + nameof(MRETStart) + "; " + name + "] executing...");
#endif
        }

        /// <summary>
        /// This method is available for subclasses to perform periodic updates. This method is triggered
        /// by the Update method, throttled by the update rate property.
        /// </summary>
        /// 
        /// <see cref="updateRate"/>
        /// 
        protected virtual void MRETUpdate()
        {
#if MRET_DEBUG
            Debug.Log("[" + ClassName + "->" + nameof(MRETUpdate) + "; " + name + "] executing...");
#endif
        }

        /// <summary>
        /// This method is used by Unity and called once per frame. For MRET, this method controls the update
        /// rate at which the MRETUpdate method is called using the updateRate property settings. As a result,
        /// this method is thereby defined so that subclasses cannot override the behavior. Subclasses should
        /// override the MRETUpdate method to perform periodic updates.
        /// </summary>
        /// 
        /// <see cref="updateRate"/>
        /// <see cref="MRETUpdate"/>
        /// 
        protected void Update()
        {
            // Update the current time
            currentTime = DateTime.UtcNow;

            // How much time has lapsed since the last update
            TimeSpan span = currentTime - lastUpdate;

            // Determine if we need to perform an update
            if (span.TotalMilliseconds >= updatePeriodMs)
            {
                // Perform the update
                MRETUpdate();

                // Record the update
                lastUpdate = currentTime;
            }
        }

    }
}
