// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GSFC.ARVR.Utilities.Properties;

namespace GSFC.ARVR.MRET.Time
{
    /**
     * Time manager
     * 
     * Maintains the project time, allowing for simulated time. Time is written to the DataManager
     * so that other scripts can query the time and other time settings.
     * 
     * The following keys are written to the DataManager:
     * 
     *  TIME_KEY_START:        The start time of the simulation in the local time
     *  TIME_KEY_START_UTC:    The start time of the simulation in UTC
     *  TIME_KEY_NOW:          The current simulated time in the local time
     *  TIME_KEY_NOW_UTC:      The current simulated time in the UTC time
     *  TIME_KEY_UPDATERATE:   The the number of simulated seconds that represent a realtime second.
     *                         Negative values reverse time
     *  TIME_KEY_PAUSED:       The paused state of this manager
     *  TIME_KEY_RESETREQUEST: Available to scripts to request a reset of the time simulation. Setting
     *                         this field will be detected by the TimeManager and perform a full reset
     *                         of the time.
     *                         
     * @author Jeffrey Hosler
     */
    public class TimeManager : MonoBehaviour
    {
        public static readonly string NAME = nameof(TimeManager);

        // Prefix of all time keys
        public const string TIME_KEY_PREFIX = "GSFC.ARVR.MRET.TIME";

        // DataManager keys
        public const string TIME_KEY_START = TIME_KEY_PREFIX + ".START";
        public const string TIME_KEY_START_UTC = TIME_KEY_PREFIX + ".START.UTC";
        public const string TIME_KEY_NOW = TIME_KEY_PREFIX + ".NOW";
        public const string TIME_KEY_NOW_UTC = TIME_KEY_PREFIX + ".NOW.UTC";
        public const string TIME_KEY_UPDATERATE = TIME_KEY_PREFIX + ".UPDATERATE";
        public const string TIME_KEY_PAUSED = TIME_KEY_PREFIX + ".PAUSED";
        public const string TIME_KEY_RESETREQUEST = TIME_KEY_PREFIX + ".RESETREQUEST";

        // Used for tracking when to update the current time
        private DateTime lastUpdate = default(DateTime);

        [Tooltip("The DataManager to use for storing and retrieving the simulated date/time settings. If not supplied, one will be located at Start")]
        public DataManager dataManager;

        // Performance Management
        private int updateCounter = 0;
        [Tooltip("Modulates the frequency of date/time updates published to the DataManager. The value represents a counter modulo to determine how many calls to Update will be skipped before publishing.")]
        public int updateRateModulo = 1;

        // Control properties
#if UNITY_EDITOR
        [Tooltip("The start date/time for the simulation. If not specified, the current system time will be used.")]
        public UDateTime startTime = default(DateTime);
#else
        public DateTime startTime = default(DateTime);
#endif

        [Tooltip("Represents the number of simulated seconds that represent a realtime second. Negative values reverse time.")]
        public float updateRate = 1.0f;

        // Used to pause the simulation
        public bool pause = false;

        private DateTime currentTime;
        public DateTime CurrentTime
        {
            get
            {
                return this.currentTime;
            }
        }

        /**
         * Called to perform initialization on the time Manager.
         */
        public void Initialize()
        {
            // Try to get a reference to the data manager if not explictly set
            if (dataManager == null)
            {
                dataManager = Infrastructure.Framework.MRET.DataManager;
            }

            // Initialize the time
            InitializeTime();
        }

        /**
         * Initializes the time to the start time specified by the public property
         * <code>startTime</code>. The DataManager is updated with the initial state
         * information for the time<br/>
         * 
         * @see startTime
         */
        private void InitializeTime()
        {
            // Check if the start time was initialized. If it's the default value, use current time
            if (startTime == default(DateTime))
            {
                startTime = DateTime.UtcNow;
            }

            // Make sure we are using publishing a DateTime object, so assign startTime to a DateTime variable
            DateTime initializedStartTime = startTime;

            // Initialize the time
            currentTime = lastUpdate = initializedStartTime;

            // Store the initial values in the data store
            if (dataManager != null)
            {
                // Store the start time
                dataManager.SaveValue(TIME_KEY_START, initializedStartTime.ToLocalTime());
                dataManager.SaveValue(TIME_KEY_START_UTC, initializedStartTime.ToUniversalTime());

                // Store the current time and control settings
                dataManager.SaveValue(TIME_KEY_NOW, currentTime.ToLocalTime());
                dataManager.SaveValue(TIME_KEY_NOW_UTC, currentTime.ToUniversalTime());
                dataManager.SaveValue(TIME_KEY_UPDATERATE, updateRate);
                dataManager.SaveValue(TIME_KEY_PAUSED, pause);
            }
        }

        /**
         * Performs a check to see if the time manager should be reset to the start time.<br/>
         * This method checks for the request in the DataManager and if a request is detected
         * the reset is performed and the request flag is reset.<br/>
         * 
         * @see startTime
         */
        private void CheckForReset()
        {
            // Store the initial values in the data store
            if (dataManager != null)
            {
                var resetRequest = dataManager.FindPoint(TIME_KEY_RESETREQUEST);
                if ((resetRequest is bool) && ((bool)resetRequest))
                {
                    // Initialize the time
                    InitializeTime();

                    // Reset the request
                    dataManager.SaveValue(TIME_KEY_RESETREQUEST, false);
                }
            }
        }

        /**
         * Updates the current time at the specified update rate. The value is stored in
         * the DataManager under the key <code>TIME_KEY_NOW</code> if a DataManager is
         * avaliable.
         */
        private void Update()
        {
            // Check for a reset request
            CheckForReset();

            // Performance management
            updateCounter++;
            if (updateCounter >= updateRateModulo)
            {
                // Reset the update counter
                updateCounter = 0;

                // Store the control telemetry in the data manager
                if (dataManager != null)
                {
                    // Make sure we are using publishing a DateTime object, so assign startTime to a DateTime variable
                    DateTime initializedStartTime = startTime;

                    // Store the start time
                    dataManager.SaveValue(TIME_KEY_START, initializedStartTime.ToLocalTime());
                    dataManager.SaveValue(TIME_KEY_START_UTC, initializedStartTime.ToUniversalTime());

                    dataManager.SaveValue(TIME_KEY_PAUSED, pause);
                    dataManager.SaveValue(TIME_KEY_UPDATERATE, updateRate);
                }

                // Check if the simulation is paused
                if (!pause)
                {
                    // Update the time
                    DateTime now = DateTime.Now;

                    // How much time has lapsed since the last update
                    TimeSpan span = now - lastUpdate;

                    // Multiply the real milliseconds by our update rate to get the simulated
                    // milliseconds that have elapsed
                    double simMs = span.Milliseconds * updateRate;

                    // Advance/reverse time the simulated number of milliseconds
                    currentTime += TimeSpan.FromMilliseconds(simMs);

                    // Store the current time in the data manager
                    if (dataManager != null)
                    {
                        dataManager.SaveValue(TIME_KEY_NOW, currentTime.ToLocalTime());
                        dataManager.SaveValue(TIME_KEY_NOW_UTC, currentTime.ToUniversalTime());
                    }

                    // Record the update
                    lastUpdate = now;
                }

                // Debug.Log("Time updated: " + currentTime);
            }
        }

        /**
         * Writes the reset request to the data manager so that the next time through the Update
         * method, the reset will be detected, triggering a full reset of the time.
         */
        public void ResetTime()
        {
            // Store the reset request into the data manager
            if (dataManager != null)
            {
                // Mark for reset
                dataManager.SaveValue(TIME_KEY_RESETREQUEST, true);
            }
        }

    }

}