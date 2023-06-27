// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.Time;

namespace GOV.NASA.GSFC.XR.MRET.UI.Time
{
    public class TimeIndicatorManager : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETUpdateBehaviour.ClassName"/>
        public override string ClassName => nameof(TimeIndicatorManager);

        [Tooltip("The format of the displayed date/time.")]
        public string timeFormat = "s";

        [Tooltip("The Text object used to display the date/time.")]
        public Text timeText;

        private TimeManager timeManager;
        private DataManager dataManager;

        /// <seealso cref="MRETUpdateBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Assign the managers
            dataManager = MRET.DataManager;
            timeManager = MRET.TimeManager;
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETStart"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            if ((dataManager != null) && (timeManager != null) && timeManager.enabled)
            {
                // Get the project time
                var projectTimeObj = dataManager.FindPoint(TimeManager.TIME_KEY_NOW);

                // Extract the time
                if (projectTimeObj is DateTime projectTime)
                {
                    // Format the time
                    string timeStr = projectTime.ToString(timeFormat);

                    // Display the time
                    if (timeText != null)
                    {
                        timeText.text = timeStr;
                    }
                }
            }
        }

        public void ResetTime()
        {
            if (timeManager != null)
            {
                // Mark for reset
                timeManager.ResetTime();
            }
        }
    }
}