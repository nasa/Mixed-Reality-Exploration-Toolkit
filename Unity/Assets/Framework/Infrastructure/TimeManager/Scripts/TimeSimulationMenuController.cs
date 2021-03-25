// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.UI;
using GSFC.ARVR.MRET.Telemetry.UI;
using GSFC.ARVR.MRET.Common.Schemas.TimeSimulationTypes;
using GSFC.ARVR.MRET.Infrastructure.Framework;

namespace GSFC.ARVR.MRET.Time.Simulation
{
    /**
     * Time simulation menu controller
     * 
     * Handles all the user interaction with the controls on the associated time simulation panel. The
     * values/settings of the individual controls are largely controlled by telemetry from the
     * DataManager using Telemetry UI scripts attached to the individual controls.<br>
     * 
     * @author Jeffrey Hosler
     */
    public class TimeSimulationMenuController : MonoBehaviour
    {
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
        private TimeSimulation timeSimulation;
        private TelemetryUIDateTimeSlider startTimeDateTimeSlider;

        /**
         * Awake is called once during initialization
         */
        private void Awake()
        {
            // Create our placeholder TimeSimulation object
            timeSimulation = gameObject.AddComponent<TimeSimulation>();
        }

        /**
         * Start is called before the first frame update
         */
        void Start()
        {
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

        /**
         * Updates the internal time simulation with the TimeManager settings
         * 
         * @see TimeSimulation
         * @see TimeManager
         */
        protected virtual void UpdateTimeSimulation()
        {
            // Assign the current TimeManager settings to our internal TimeSimulation
            timeSimulation.startTime = Infrastructure.Framework.MRET.TimeManager.startTime;
            timeSimulation.updateRate = Infrastructure.Framework.MRET.TimeManager.updateRate;
            timeSimulation.paused = Infrastructure.Framework.MRET.TimeManager.pause;
        }

        /**
         * Configures the TimeManager with the supplied time simulation settings
         * 
         * @param timeSimulation The <code>TimeSimulation</code> to use to configure the TimeManager
         * 
         * @see TimeSimulation
         * @see TimeManager
         */
        protected virtual void ConfigureTimeManager(TimeSimulation timeSimulation, bool reset)
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                Infrastructure.Framework.MRET.TimeManager.startTime = timeSimulation.startTime;
                Infrastructure.Framework.MRET.TimeManager.updateRate = timeSimulation.updateRate;
                Infrastructure.Framework.MRET.TimeManager.pause = timeSimulation.paused;
                if (reset) Infrastructure.Framework.MRET.TimeManager.ResetTime();
            }
        }

        /**
         * Loads the supplied time simulation
         * 
         * @param timeSimulationType the <code>TimeSimulationType</code> representing the
         *      serialized time simulation
         */
        public void LoadTimeSimulation(TimeSimulationType timeSimulationType)
        {
            // Deserialize the serialized type
            timeSimulation.Deserialize(timeSimulationType);

            // Configure the TimeManager with the settings
            ConfigureTimeManager(timeSimulation, true);
        }

        /**
         * Gets the supplied time simulation
         * 
         * @return The <code>TimeSimulationType</code> representing the serialized time
         *      simulation
         */
        public TimeSimulationType GetTimeSimulation()
        {
            // Update the time simulation
            UpdateTimeSimulation();

            // Serialize the time simulation
            return timeSimulation.Serialize();
        }

        /**
         * Update rate input field validation delegate. Makes sure the input converts to a
         * float value.
         * 
         * @param input The value of the input field before the addition
         * @param charIndex The index of the new character
         * @param addedChar The character to be added
         * 
         * @return the added character or null terminator if not allowed
         */
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

        /**
         * Event handler for a update rate input field change
         */
        public void UpdateRateInputFieldChanged()
        {
            if (updateRateInputField && Infrastructure.Framework.MRET.TimeManager)
            {
                if (float.TryParse(updateRateInputField.text, out float updateRate))
                {
                    // Update the time manager
                    Infrastructure.Framework.MRET.TimeManager.updateRate = updateRate;
                }
                else
                {
                    // TODO: Change color?
                }
            }
        }

        /**
         * Event handler for a update rate slider begin drag event
         */
        public void UpdateRateSliderBeginDrag()
        {
            updateRateSliderIsBeingDragged = true;
        }

        /**
         * Event handler for a update rate slider end drag event
         */
        public void UpdateRateSliderEndDrag()
        {
            updateRateSliderIsBeingDragged = false;
        }

        /**
         * Event handler for a update rate slider value changed event
         */
        public void UpdateRateSliderValueChanged()
        {
            if (updateRateSlider && Infrastructure.Framework.MRET.TimeManager)
            {
                if (updateRateSliderIsBeingDragged)
                {
                    // Update the TimeManager
                    Infrastructure.Framework.MRET.TimeManager.updateRate = updateRateSlider.value;
                }
            }
        }

        /**
         * Event handler for a start time input field change
         */
        public void StartTimeInputFieldChanged()
        {
            if (startTimeInputField && Infrastructure.Framework.MRET.TimeManager)
            {
                if (DateTime.TryParse(startTimeInputField.text, out DateTime startTime))
                {
                    // Update the TimeManager
                    Infrastructure.Framework.MRET.TimeManager.startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
                }
                else
                {
                    // TODO: Change color?
                }
            }
        }

        /**
         * Event handler for a start time slider begin drag event
         */
        public void StartTimeSliderBeginDrag()
        {
            startTimeSliderIsBeingDragged = true;
        }

        /**
         * Event handler for a start time slider end drag event
         */
        public void StartTimeSliderEndDrag()
        {
            startTimeSliderIsBeingDragged = false;
        }

        /**
         * Event handler for a start time slider value changed event
         */
        public void StartTimeSliderValueChanged()
        {
            if (startTimeSlider && Infrastructure.Framework.MRET.TimeManager)
            {
                if (startTimeSliderIsBeingDragged)
                {
                    // Update the TimeManager
                    Infrastructure.Framework.MRET.TimeManager.startTime = startTimeDateTimeSlider.GetDateTime(startTimeSlider.value);
                }
            }
        }

        /**
         * Event handler for the play/pause toggle value changed event
         */
        public void PlayPauseToggleValueChanged()
        {
            if (playPauseToggle && Infrastructure.Framework.MRET.TimeManager)
            {
                // Update the paused state of the TimeManager
                Infrastructure.Framework.MRET.TimeManager.pause = !playPauseToggle.isOn;
            }
        }

        /**
         * Event handler for the reset button clicked event
         */
        public void ResetButtonClicked()
        {
            if (resetButton && Infrastructure.Framework.MRET.TimeManager)
            {
                // Reset the TimeManager
                Infrastructure.Framework.MRET.TimeManager.ResetTime();
            }
        }

        // Start time "quick-click" buttons for modifying the start time
        #region STARTTIME_PRESETS

        public void StartTimePlusClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = Infrastructure.Framework.MRET.TimeManager.startTime;

                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.startTime = startTime.AddDays(1d);
            }
        }

        public void StartTimeYearClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = Infrastructure.Framework.MRET.TimeManager.startTime;

                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.startTime = startTime.AddYears(1);
            }
        }

        public void StartTimeYearReverseClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = Infrastructure.Framework.MRET.TimeManager.startTime;

                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.startTime = startTime.AddYears(-1);
            }
        }

        public void StartTimeMonthClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = Infrastructure.Framework.MRET.TimeManager.startTime;

                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.startTime = startTime.AddMonths(1);
            }
        }

        public void StartTimeMonthReverseClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = Infrastructure.Framework.MRET.TimeManager.startTime;

                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.startTime = startTime.AddMonths(-1);
            }
        }

        public void StartTimeDayClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = Infrastructure.Framework.MRET.TimeManager.startTime;

                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.startTime = startTime.AddDays(1d);
            }
        }

        public void StartTimeDayReverseClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = Infrastructure.Framework.MRET.TimeManager.startTime;

                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.startTime = startTime.AddDays(-1d);
            }
        }

        public void StartTimeMinClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = Infrastructure.Framework.MRET.TimeManager.startTime;

                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.startTime = startTime.AddMinutes(1d);
            }
        }

        public void StartTimeMinReverseClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = Infrastructure.Framework.MRET.TimeManager.startTime;

                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.startTime = startTime.AddMinutes(-1d);
            }
        }

        public void StartTimeHourClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = Infrastructure.Framework.MRET.TimeManager.startTime;

                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.startTime = startTime.AddHours(1d);
            }
        }

        public void StartTimeHourReverseClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = Infrastructure.Framework.MRET.TimeManager.startTime;

                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.startTime = startTime.AddHours(-1d);
            }
        }

        public void StartTimeSecClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = Infrastructure.Framework.MRET.TimeManager.startTime;

                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.startTime = startTime.AddSeconds(1d);
            }
        }

        public void StartTimeSecReverseClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = Infrastructure.Framework.MRET.TimeManager.startTime;

                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.startTime = startTime.AddSeconds(-1d);
            }
        }

        public void StartTimeZeroClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.startTime = DateTime.UtcNow;
            }
        }

        public void StartTimeMinusClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Add time to the existing start time
                DateTime startTime = Infrastructure.Framework.MRET.TimeManager.startTime;

                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.startTime = startTime.AddDays(-1d);
            }
        }

        #endregion //STARTTIME_PRESETS

        // Update rate "quick-click" buttons for modifying the start time
        #region UPDATERATE_PRESETS

        public void UpdateRatePlusClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.updateRate++;
            }
        }

        public void UpdateRateDayClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.updateRate += 86400f;
            }
        }

        public void UpdateRateDayReverseClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.updateRate -= 86400f;
            }
        }

        public void UpdateRateMinClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.updateRate += 60f;
            }
        }

        public void UpdateRateMinReverseClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.updateRate -= 60f;
            }
        }

        public void UpdateRateHourClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.updateRate += 3600f;
            }
        }

        public void UpdateRateHourReverseClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.updateRate -= 3600f;
            }
        }

        public void UpdateRateSecClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.updateRate += 1f;
            }
        }

        public void UpdateRateSecReverseClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.updateRate -= 1f;
            }
        }

        public void UpdateRateZeroClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.updateRate = 0f;
            }
        }

        public void UpdateRateMinusClicked()
        {
            if (Infrastructure.Framework.MRET.TimeManager)
            {
                // Update the TimeManager
                Infrastructure.Framework.MRET.TimeManager.updateRate--;
            }
        }

        #endregion //UPDATERATE_PRESETS

    }
}