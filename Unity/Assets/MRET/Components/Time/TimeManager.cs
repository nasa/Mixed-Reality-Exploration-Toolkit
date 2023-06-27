// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.Utilities.Properties;
using GOV.NASA.GSFC.XR.MRET.Data;

namespace GOV.NASA.GSFC.XR.MRET.Time
{
    /// <summary>
    /// Time manager
    /// 
    /// Maintains the project time, allowing for simulated time. Time is written to the DataManager
    /// so that other scripts can query the time and other time settings.
    /// 
    /// The following keys are written to the DataManager:
    /// 
    ///  TIME_KEY_START:        The start time of the simulation in the local time
    ///  TIME_KEY_START_UTC:    The start time of the simulation in UTC
    ///  TIME_KEY_END:          The end time of the simulation in the local time
    ///  TIME_KEY_END_UTC:      The end time of the simulation in UTC
    ///  TIME_KEY_NOW:          The current simulated time in the local time
    ///  TIME_KEY_NOW_UTC:      The current simulated time in the UTC time
    ///  TIME_KEY_UPDATERATE:   The the number of simulated seconds that represent a realtime second.
    ///                         Negative values reverse time
    ///  TIME_KEY_PAUSED:       The paused state of this manager
    ///  TIME_KEY_RESETREQUEST: Available to scripts to request a reset of the time simulation. Setting
    ///                         this field will be detected by the TimeManager and perform a full reset
    ///                         of the time.
    ///                         
    /// @author Jeffrey Hosler
    /// </summary>
    public class TimeManager : MRETUpdateManager<TimeManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(TimeManager);

        // Prefix of all time keys
        public const string TIME_KEY_PREFIX = "GOV.NASA.GSFC.XR.MRET.TIME";

        // DataManager keys
        public const string TIME_KEY_START = TIME_KEY_PREFIX + ".START";
        public const string TIME_KEY_START_UTC = TIME_KEY_PREFIX + ".START.UTC";
        public const string TIME_KEY_END = TIME_KEY_PREFIX + ".END";
        public const string TIME_KEY_END_UTC = TIME_KEY_PREFIX + ".END.UTC";
        public const string TIME_KEY_NOW = TIME_KEY_PREFIX + ".NOW";
        public const string TIME_KEY_NOW_UTC = TIME_KEY_PREFIX + ".NOW.UTC";
        public const string TIME_KEY_UPDATERATE = TIME_KEY_PREFIX + ".UPDATERATE";
        public const string TIME_KEY_PAUSED = TIME_KEY_PREFIX + ".PAUSED";
        public const string TIME_KEY_RESETREQUEST = TIME_KEY_PREFIX + ".RESETREQUEST";

        // Used for tracking when to update the current time
        private DateTime lastUpdate = default(DateTime);

        [Tooltip("The DataManager to use for storing and retrieving the simulated date/time settings. If not supplied, one will be located at Start")]
        public DataManager dataManager;

        // Control properties
#if UNITY_EDITOR
        [Tooltip("The start date/time for the simulation. If not specified, the current system time will be used.")]
        public UDateTime startTime = default(DateTime);

        [Tooltip("The end date/time for the simulation. If not specified, the simulation will not stop.")]
        public UDateTime endTime = default(DateTime);
#else
        public DateTime startTime = default(DateTime);
        public DateTime endTime = default(DateTime);
#endif

        [Tooltip("Represents the number of simulated seconds that represent a realtime second. Negative values reverse time.")]
        public float timeUpdateRate = 1.0f;

        // Used to pause the simulation
        public bool pause = false;

        // Identifies the direction in time between the start and end time
        private enum DirectionType { forward, backward };
        private DirectionType direction = DirectionType.forward;

        private DateTime _currentTime;
        public DateTime currentTime
        {
            get => _currentTime;
        }

        /// <seealso cref="MRETManager{M}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();

            // Try to get a reference to the data manager if not explictly set
            if (dataManager == null)
            {
                dataManager = MRET.DataManager;
            }

            // Initialize the time
            InitializeTime();
        }

        /// <summary>
        /// Initializes the time to the start time specified by the public property
        /// <code>startTime</code>. The DataManager is updated with the initial state
        /// information for the time<br/>
        /// </summary>
        /// <seealso cref="startTime"/>
        private void InitializeTime()
        {
            // Check if the start time was initialized. If it's the default value, use current time
            DateTime _startTime = startTime; // <= Convert from UDateTime to DateTime for comparison
            if (DateTime.Compare(startTime, default(DateTime)) == 0)
            {
                startTime = DateTime.UtcNow;
            }

            // Make sure we are using publishing a DateTime object, so assign startTime to a DateTime variable
            DateTime initializedStartTime = startTime;
            DateTime initializedEndTime = endTime;

            // Determine which direction we are *supposed* to move in time. UpdateRate does not affect this setting.
            direction = DateTime.Compare(startTime, endTime) < 0 ? DirectionType.forward : DirectionType.backward;

            // Initialize the time
            _currentTime = lastUpdate = initializedStartTime;

            // Store the initial values in the data store
            if (dataManager != null)
            {
                // Store the start time
                dataManager.SaveValue(TIME_KEY_START, initializedStartTime.ToLocalTime());
                dataManager.SaveValue(TIME_KEY_START_UTC, initializedStartTime.ToUniversalTime());
                dataManager.SaveValue(TIME_KEY_END, initializedEndTime.ToLocalTime());
                dataManager.SaveValue(TIME_KEY_END_UTC, initializedEndTime.ToUniversalTime());

                // Store the current time and control settings
                dataManager.SaveValue(TIME_KEY_NOW, _currentTime.ToLocalTime());
                dataManager.SaveValue(TIME_KEY_NOW_UTC, _currentTime.ToUniversalTime());
                dataManager.SaveValue(TIME_KEY_UPDATERATE, timeUpdateRate);
                dataManager.SaveValue(TIME_KEY_PAUSED, pause);
            }
        }

        /// <summary>
        /// Performs a check to see if the time manager should be reset to the start time.<br/>
        /// This method checks for the request in the DataManager and if a request is detected
        /// the reset is performed and the request flag is reset.<br/>
        /// </summary>
        /// <seealso cref="startTime"/>
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

        /// <summary>
        /// Updates the current time at the specified update rate. The value is stored in
        /// the DataManager under the keys:
        /// 
        ///     <code>TIME_KEY_NOW</code> and
        ///     <code>TIME_KEY_NOW_UTC</code>
        ///     
        /// The frequency of this method call will impact the time calculations, so
        /// it is recommended to keep the update rate for this class at maximum.
        /// </summary>
        /// <seealso cref="currentTime"/>
        /// <seealso cref="MRETUpdateBehaviour.updateRate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            // Check for a reset request
            CheckForReset();

            // Store the control telemetry in the data manager
            if (dataManager != null)
            {
                // Make sure we are using publishing a DateTime object, so assign startTime to a DateTime variable
                DateTime initializedStartTime = startTime;

                // Store the start time
                dataManager.SaveValue(TIME_KEY_START, initializedStartTime.ToLocalTime());
                dataManager.SaveValue(TIME_KEY_START_UTC, initializedStartTime.ToUniversalTime());

                dataManager.SaveValue(TIME_KEY_PAUSED, pause);
                dataManager.SaveValue(TIME_KEY_UPDATERATE, timeUpdateRate);
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
                double simMs = span.Milliseconds * timeUpdateRate;

                // Advance/reverse time the simulated number of milliseconds
                _currentTime += TimeSpan.FromMilliseconds(simMs);

                // Check to see if an end time was specified
                DateTime _endTime = endTime; // <= Convert from UDateTime to DateTime for comparison
                if (DateTime.Compare(_endTime, default(DateTime)) != 0)
                {
                    // Pause if the current time has exceeded the end time in the
                    // correct direction
                    pause = (direction == DirectionType.forward)
                        ? (DateTime.Compare(_currentTime, _endTime) >= 0)
                        : (DateTime.Compare(_currentTime, _endTime) <= 0);
                    if (pause)
                    {
                        Log("Time paused because end time was reached.");
                    }
                }

                // Store the current time in the data manager
                if (dataManager != null)
                {
                    dataManager.SaveValue(TIME_KEY_NOW, _currentTime.ToLocalTime());
                    dataManager.SaveValue(TIME_KEY_NOW_UTC, _currentTime.ToUniversalTime());
                }

                // Record the update
                lastUpdate = now;
            }

            // Debug.Log("Time updated: " + currentTime);
        }

        /// <summary>
        /// Writes the reset request to the data manager so that the next time through the Update
        /// method, the reset will be detected, triggering a full reset of the time.
        /// </summary>
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