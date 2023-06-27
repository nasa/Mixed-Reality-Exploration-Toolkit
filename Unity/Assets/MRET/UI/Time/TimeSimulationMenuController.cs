// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Time;
using GOV.NASA.GSFC.XR.MRET.UI.Data.Telemetry;

namespace GOV.NASA.GSFC.XR.MRET.UI.Time
{
    /// <remarks>
    /// History:
    /// 13 December 2021: Created
    /// </remarks>
    /// <summary>
    /// Time simulation menu controller
    ///
    /// Handles all the user interaction with the controls on the associated time simulation panel. The
    /// values/settings of the individual controls are largely controlled by telemetry from the
    /// DataManager using Telemetry UI scripts attached to the individual controls.<br>
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    public class TimeSimulationMenuController : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(TimeSimulationMenuController);

        // Start time slider
        public InputField startTimeInputField;
        public Slider startTimeSlider;
        private bool startTimeSliderIsBeingDragged = false;

        // Update rate components
        public InputField updateRateInputField;
        public Slider updateRateSlider;
        private bool updateRateSliderIsBeingDragged = false;

        // Play/Pause components
        public Toggle playPauseToggle;

        // Reset components
        public Button resetButton;

        // Internal TimeSimulation object to hold the user's simulation configuration
        private ITimeSimulation timeSimulation;
        private TelemetryUIDateTimeSlider startTimeDateTimeSlider;

        /// <seealso cref="MRETBehaviour.MRETAwake"/>
        protected override void MRETAwake()
        {
            base.MRETAwake();

            // Create our placeholder TimeSimulation object
            timeSimulation = new TimeSimulation();
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Initializer the validation functions
            if (updateRateInputField)
            {
                updateRateInputField.onValidateInput += delegate (string input, int charIndex, char addedChar)
                {
                    return UpdateRateInputValidation(input, charIndex, addedChar);
                };
            }

            // Get the start time range for slider value normalization
            if (startTimeSlider)
            {
                // Try to get the TelemetryUIDateTimeSlider reference
                startTimeDateTimeSlider = startTimeSlider.GetComponent<TelemetryUIDateTimeSlider>();
            }
        }

        /// <summary>
        /// Updates the internal time simulation with the TimeManager settings
        /// </summary>
        /// <seealso cref="TimeSimulation"/>
        /// <seealso cref="TimeManager"/>
        protected virtual void UpdateTimeSimulation()
        {
            // Assign the current TimeManager settings to our internal TimeSimulation
            timeSimulation.StartTime = MRET.TimeManager.startTime;
            timeSimulation.EndTime = MRET.TimeManager.endTime;
            timeSimulation.UpdateRate = MRET.TimeManager.timeUpdateRate;
            timeSimulation.Paused = MRET.TimeManager.pause;
        }

        /// <summary>
        /// Loads the supplied time simulation
        /// </summary>
        /// <param name="timeSimulationType">The <code>TimeSimulationType</code> representing
        ///     the serialized time simulation</param>
        public void LoadTimeSimulation(TimeSimulationType timeSimulationType)
        {
            // Deserialize the serialized type
            timeSimulation.Deserialize(timeSimulationType);

            // Configure the TimeManager with the settings
            timeSimulation.ConfigureTimeManager(true);
        }

        /// <summary>
        /// Gets the supplied time simulation
        /// </summary>
        /// <returns>The <code>TimeSimulationType</code> representing the serialized time
        ///      simulation</returns>
        public TimeSimulationType GetTimeSimulation()
        {
            // Update the time simulation
            UpdateTimeSimulation();

            // Serialize the time simulation
            TimeSimulationType serializedTimeSimulation = new TimeSimulationType();
            timeSimulation.Serialize(serializedTimeSimulation);
            return serializedTimeSimulation;
        }

        /// <summary>
        /// Update rate input field validation delegate. Makes sure the input converts to a
        /// float value.
        /// </summary>
        /// <param name="input">The value of the input field before the addition</param>
        /// <param name="charIndex">The index of the new character</param>
        /// <param name="addedChar">The character to be added</param>
        /// <returns>the added character or null terminator if not allowed</returns>
        private char UpdateRateInputValidation(string input, int charIndex, char addedChar)
        {
            char result = '\0';

            // This is what the user is trying to enter
            string testString = input + addedChar;

            // Try to parse the string into a float, but allow the start of a negative number or decimal place
            if ((testString == "-") || (testString == ".") || float.TryParse(testString, out float number))
            {
                // It checked out, so allow the character
                result = addedChar;
            }

            return result;
        }

        /// <summary>
        /// Event handler for a update rate input field change
        /// </summary>
        public void UpdateRateInputFieldChanged()
        {
            if (updateRateInputField && MRET.TimeManager)
            {
                if (float.TryParse(updateRateInputField.text, out float updateRate))
                {
                    // Update the time manager
                    MRET.TimeManager.timeUpdateRate = updateRate;
                }
                else
                {
                    // TODO: Change color?
                }
            }
        }

        /// <summary>
        /// Event handler for a update rate slider begin drag event
        /// </summary>
        public void UpdateRateSliderBeginDrag()
        {
            updateRateSliderIsBeingDragged = true;
        }

        /// <summary>
        /// Event handler for a update rate slider end drag event
        /// </summary>
        public void UpdateRateSliderEndDrag()
        {
            updateRateSliderIsBeingDragged = false;
        }

        /// <summary>
        /// Event handler for a update rate slider value changed event
        /// </summary>
        public void UpdateRateSliderValueChanged()
        {
            if (updateRateSlider && MRET.TimeManager)
            {
                if (updateRateSliderIsBeingDragged)
                {
                    // Update the TimeManager
                    MRET.TimeManager.timeUpdateRate = updateRateSlider.value;
                }
            }
        }

        /// <summary>
        /// Event handler for a start time input field change
        /// </summary>
        public void StartTimeInputFieldChanged()
        {
            if (startTimeInputField && MRET.TimeManager)
            {
                if (DateTime.TryParse(startTimeInputField.text, out DateTime startTime))
                {
                    // Update the TimeManager
                    MRET.TimeManager.startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
                }
                else
                {
                    // TODO: Change color?
                }
            }
        }

        /// <summary>
        /// Event handler for a start time slider begin drag event
        /// </summary>
        public void StartTimeSliderBeginDrag()
        {
            startTimeSliderIsBeingDragged = true;
        }

        /// <summary>
        /// Event handler for a start time slider end drag event
        /// </summary>
        public void StartTimeSliderEndDrag()
        {
            startTimeSliderIsBeingDragged = false;
        }

        /// <summary>
        /// Event handler for a start time slider value changed event
        /// </summary>
        public void StartTimeSliderValueChanged()
        {
            if (startTimeSlider && MRET.TimeManager)
            {
                if (startTimeSliderIsBeingDragged)
                {
                    // Update the TimeManager
                    MRET.TimeManager.startTime = startTimeDateTimeSlider.GetDateTime(startTimeSlider.value);
                }
            }
        }

        /// <summary>
        /// Event handler for the play/pause toggle value changed event
        /// </summary>
        public void PlayPauseToggleValueChanged()
        {
            if (playPauseToggle && MRET.TimeManager)
            {
                // Update the paused state of the TimeManager
                MRET.TimeManager.pause = !playPauseToggle.isOn;
            }
        }

        /// <summary>
        /// Event handler for the reset button clicked event
        /// </summary>
        public void ResetButtonClicked()
        {
            if (resetButton && MRET.TimeManager)
            {
                // Reset the TimeManager
                MRET.TimeManager.ResetTime();
            }
        }

        // Start time "quick-click" buttons for modifying the start time
        #region STARTTIME_PRESETS

        public void StartTimePlusClicked()
        {
            if (MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = MRET.TimeManager.startTime;

                // Update the TimeManager
                MRET.TimeManager.startTime = startTime.AddDays(1d);
            }
        }

        public void StartTimeYearClicked()
        {
            if (MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = MRET.TimeManager.startTime;

                // Update the TimeManager
                MRET.TimeManager.startTime = startTime.AddYears(1);
            }
        }

        public void StartTimeYearReverseClicked()
        {
            if (MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = MRET.TimeManager.startTime;

                // Update the TimeManager
                MRET.TimeManager.startTime = startTime.AddYears(-1);
            }
        }

        public void StartTimeMonthClicked()
        {
            if (MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = MRET.TimeManager.startTime;

                // Update the TimeManager
                MRET.TimeManager.startTime = startTime.AddMonths(1);
            }
        }

        public void StartTimeMonthReverseClicked()
        {
            if (MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = MRET.TimeManager.startTime;

                // Update the TimeManager
                MRET.TimeManager.startTime = startTime.AddMonths(-1);
            }
        }

        public void StartTimeDayClicked()
        {
            if (MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = MRET.TimeManager.startTime;

                // Update the TimeManager
                MRET.TimeManager.startTime = startTime.AddDays(1d);
            }
        }

        public void StartTimeDayReverseClicked()
        {
            if (MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = MRET.TimeManager.startTime;

                // Update the TimeManager
                MRET.TimeManager.startTime = startTime.AddDays(-1d);
            }
        }

        public void StartTimeMinClicked()
        {
            if (MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = MRET.TimeManager.startTime;

                // Update the TimeManager
                MRET.TimeManager.startTime = startTime.AddMinutes(1d);
            }
        }

        public void StartTimeMinReverseClicked()
        {
            if (MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = MRET.TimeManager.startTime;

                // Update the TimeManager
                MRET.TimeManager.startTime = startTime.AddMinutes(-1d);
            }
        }

        public void StartTimeHourClicked()
        {
            if (MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = MRET.TimeManager.startTime;

                // Update the TimeManager
                MRET.TimeManager.startTime = startTime.AddHours(1d);
            }
        }

        public void StartTimeHourReverseClicked()
        {
            if (MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = MRET.TimeManager.startTime;

                // Update the TimeManager
                MRET.TimeManager.startTime = startTime.AddHours(-1d);
            }
        }

        public void StartTimeSecClicked()
        {
            if (MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = MRET.TimeManager.startTime;

                // Update the TimeManager
                MRET.TimeManager.startTime = startTime.AddSeconds(1d);
            }
        }

        public void StartTimeSecReverseClicked()
        {
            if (MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = MRET.TimeManager.startTime;

                // Update the TimeManager
                MRET.TimeManager.startTime = startTime.AddSeconds(-1d);
            }
        }

        public void StartTimeZeroClicked()
        {
            if (MRET.TimeManager)
            {
                // Update the TimeManager
                MRET.TimeManager.startTime = DateTime.UtcNow;
            }
        }

        public void StartTimeMinusClicked()
        {
            if (MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = MRET.TimeManager.startTime;

                // Update the TimeManager
                MRET.TimeManager.startTime = startTime.AddDays(-1d);
            }
        }

        #endregion //STARTTIME_PRESETS

        // Update rate "quick-click" buttons for modifying the start time
        #region UPDATERATE_PRESETS

        public void UpdateRatePlusClicked()
        {
            if (MRET.TimeManager)
            {
                // Update the TimeManager
                MRET.TimeManager.timeUpdateRate++;
            }
        }

        public void UpdateRateDayClicked()
        {
            if (MRET.TimeManager)
            {
                // Update the TimeManager
                MRET.TimeManager.timeUpdateRate += 86400f;
            }
        }

        public void UpdateRateDayReverseClicked()
        {
            if (MRET.TimeManager)
            {
                // Update the TimeManager
                MRET.TimeManager.timeUpdateRate -= 86400f;
            }
        }

        public void UpdateRateMinClicked()
        {
            if (MRET.TimeManager)
            {
                // Update the TimeManager
                MRET.TimeManager.timeUpdateRate += 60f;
            }
        }

        public void UpdateRateMinReverseClicked()
        {
            if (MRET.TimeManager)
            {
                // Update the TimeManager
                MRET.TimeManager.timeUpdateRate -= 60f;
            }
        }

        public void UpdateRateHourClicked()
        {
            if (MRET.TimeManager)
            {
                // Update the TimeManager
                MRET.TimeManager.timeUpdateRate += 3600f;
            }
        }

        public void UpdateRateHourReverseClicked()
        {
            if (MRET.TimeManager)
            {
                // Update the TimeManager
                MRET.TimeManager.timeUpdateRate -= 3600f;
            }
        }

        public void UpdateRateSecClicked()
        {
            if (MRET.TimeManager)
            {
                // Update the TimeManager
                MRET.TimeManager.timeUpdateRate += 1f;
            }
        }

        public void UpdateRateSecReverseClicked()
        {
            if (MRET.TimeManager)
            {
                // Update the TimeManager
                MRET.TimeManager.timeUpdateRate -= 1f;
            }
        }

        public void UpdateRateZeroClicked()
        {
            if (MRET.TimeManager)
            {
                // Update the TimeManager
                MRET.TimeManager.timeUpdateRate = 0f;
            }
        }

        public void UpdateRateMinusClicked()
        {
            if (MRET.TimeManager)
            {
                // Update the TimeManager
                MRET.TimeManager.timeUpdateRate--;
            }
        }

        #endregion //UPDATERATE_PRESETS

    }
}