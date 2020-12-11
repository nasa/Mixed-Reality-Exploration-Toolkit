using System;
using UnityEngine;
using UnityEngine.UI;
using GSFC.ARVR.MRET.Telemetry.UI;
using GSFC.ARVR.MRET.Common.Schemas.TimeSimulationTypes;

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
        public static readonly string NAME = nameof(TimeSimulationMenuController);

        [Tooltip("The TimeManager being used to supply the simulated date/time. If not supplied, one will be located at Start")]
        public TimeManager timeManager;

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
            // Located the TimeManager if one wasn't assigned to the script in the editor
            if (timeManager == null)
            {
                timeManager = SessionManager.instance.timeManager;
            }

            // Set the transform
            if (!timeManager)
            {
                Debug.LogError("[" + NAME + "]: TimeManager reference unavailable.");
            }

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
            timeSimulation.startTime = timeManager.startTime;
            timeSimulation.updateRate = timeManager.updateRate;
            timeSimulation.paused = timeManager.pause;
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
            if (timeManager)
            {
                timeManager.startTime = timeSimulation.startTime;
                timeManager.updateRate = timeSimulation.updateRate;
                timeManager.pause = timeSimulation.paused;
                if (reset) timeManager.ResetTime();
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
            if (updateRateInputField && timeManager)
            {
                if (float.TryParse(updateRateInputField.text, out float updateRate))
                {
                    // Update the time manager
                    timeManager.updateRate = updateRate;
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
            if (updateRateSlider && timeManager)
            {
                if (updateRateSliderIsBeingDragged)
                {
                    // Update the TimeManager
                    timeManager.updateRate = updateRateSlider.value;
                }
            }
        }

        /**
         * Event handler for a start time input field change
         */
        public void StartTimeInputFieldChanged()
        {
            if (startTimeInputField && timeManager)
            {
                if (DateTime.TryParse(startTimeInputField.text, out DateTime startTime))
                {
                    // Update the TimeManager
                    timeManager.startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
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
            if (startTimeSlider && timeManager)
            {
                if (startTimeSliderIsBeingDragged)
                {
                    // Update the TimeManager
                    timeManager.startTime = startTimeDateTimeSlider.GetDateTime(startTimeSlider.value);
                }
            }
        }

        /**
         * Event handler for the play/pause toggle value changed event
         */
        public void PlayPauseToggleValueChanged()
        {
            if (playPauseToggle && timeManager)
            {
                // Update the paused state of the TimeManager
                timeManager.pause = !playPauseToggle.isOn;
            }
        }

        /**
         * Event handler for the reset button clicked event
         */
        public void ResetButtonClicked()
        {
            if (resetButton && timeManager)
            {
                // Reset the TimeManager
                timeManager.ResetTime();
            }
        }

        // Start time "quick-click" buttons for modifying the start time
        #region STARTTIME_PRESETS

        public void StartTimePlusClicked()
        {
            if (timeManager)
            {
                // Add time to the existing start time
                DateTime startTime = timeManager.startTime;

                // Update the TimeManager
                timeManager.startTime = startTime.AddDays(1d);
            }
        }

        public void StartTimeYearClicked()
        {
            if (timeManager)
            {
                // Add time to the existing start time
                DateTime startTime = timeManager.startTime;

                // Update the TimeManager
                timeManager.startTime = startTime.AddYears(1);
            }
        }

        public void StartTimeYearReverseClicked()
        {
            if (timeManager)
            {
                // Add time to the existing start time
                DateTime startTime = timeManager.startTime;

                // Update the TimeManager
                timeManager.startTime = startTime.AddYears(-1);
            }
        }

        public void StartTimeMonthClicked()
        {
            if (timeManager)
            {
                // Add time to the existing start time
                DateTime startTime = timeManager.startTime;

                // Update the TimeManager
                timeManager.startTime = startTime.AddMonths(1);
            }
        }

        public void StartTimeMonthReverseClicked()
        {
            if (timeManager)
            {
                // Add time to the existing start time
                DateTime startTime = timeManager.startTime;

                // Update the TimeManager
                timeManager.startTime = startTime.AddMonths(-1);
            }
        }

        public void StartTimeDayClicked()
        {
            if (timeManager)
            {
                // Add time to the existing start time
                DateTime startTime = timeManager.startTime;

                // Update the TimeManager
                timeManager.startTime = startTime.AddDays(1d);
            }
        }

        public void StartTimeDayReverseClicked()
        {
            if (timeManager)
            {
                // Add time to the existing start time
                DateTime startTime = timeManager.startTime;

                // Update the TimeManager
                timeManager.startTime = startTime.AddDays(-1d);
            }
        }

        public void StartTimeMinClicked()
        {
            if (timeManager)
            {
                // Add time to the existing start time
                DateTime startTime = timeManager.startTime;

                // Update the TimeManager
                timeManager.startTime = startTime.AddMinutes(1d);
            }
        }

        public void StartTimeMinReverseClicked()
        {
            if (timeManager)
            {
                // Add time to the existing start time
                DateTime startTime = timeManager.startTime;

                // Update the TimeManager
                timeManager.startTime = startTime.AddMinutes(-1d);
            }
        }

        public void StartTimeHourClicked()
        {
            if (timeManager)
            {
                // Add time to the existing start time
                DateTime startTime = timeManager.startTime;

                // Update the TimeManager
                timeManager.startTime = startTime.AddHours(1d);
            }
        }

        public void StartTimeHourReverseClicked()
        {
            if (timeManager)
            {
                // Add time to the existing start time
                DateTime startTime = timeManager.startTime;

                // Update the TimeManager
                timeManager.startTime = startTime.AddHours(-1d);
            }
        }

        public void StartTimeSecClicked()
        {
            if (timeManager)
            {
                // Add time to the existing start time
                DateTime startTime = timeManager.startTime;

                // Update the TimeManager
                timeManager.startTime = startTime.AddSeconds(1d);
            }
        }

        public void StartTimeSecReverseClicked()
        {
            if (timeManager)
            {
                // Add time to the existing start time
                DateTime startTime = timeManager.startTime;

                // Update the TimeManager
                timeManager.startTime = startTime.AddSeconds(-1d);
            }
        }

        public void StartTimeZeroClicked()
        {
            if (timeManager)
            {
                // Update the TimeManager
                timeManager.startTime = DateTime.UtcNow;
            }
        }

        public void StartTimeMinusClicked()
        {
            if (timeManager)
            {
                // Add time to the existing start time
                DateTime startTime = timeManager.startTime;

                // Update the TimeManager
                timeManager.startTime = startTime.AddDays(-1d);
            }
        }

        #endregion //STARTTIME_PRESETS

        // Update rate "quick-click" buttons for modifying the start time
        #region UPDATERATE_PRESETS

        public void UpdateRatePlusClicked()
        {
            if (timeManager)
            {
                // Update the TimeManager
                timeManager.updateRate++;
            }
        }

        public void UpdateRateDayClicked()
        {
            if (timeManager)
            {
                // Update the TimeManager
                timeManager.updateRate += 86400f;
            }
        }

        public void UpdateRateDayReverseClicked()
        {
            if (timeManager)
            {
                // Update the TimeManager
                timeManager.updateRate -= 86400f;
            }
        }

        public void UpdateRateMinClicked()
        {
            if (timeManager)
            {
                // Update the TimeManager
                timeManager.updateRate += 60f;
            }
        }

        public void UpdateRateMinReverseClicked()
        {
            if (timeManager)
            {
                // Update the TimeManager
                timeManager.updateRate -= 60f;
            }
        }

        public void UpdateRateHourClicked()
        {
            if (timeManager)
            {
                // Update the TimeManager
                timeManager.updateRate += 3600f;
            }
        }

        public void UpdateRateHourReverseClicked()
        {
            if (timeManager)
            {
                // Update the TimeManager
                timeManager.updateRate -= 3600f;
            }
        }

        public void UpdateRateSecClicked()
        {
            if (timeManager)
            {
                // Update the TimeManager
                timeManager.updateRate += 1f;
            }
        }

        public void UpdateRateSecReverseClicked()
        {
            if (timeManager)
            {
                // Update the TimeManager
                timeManager.updateRate -= 1f;
            }
        }

        public void UpdateRateZeroClicked()
        {
            if (timeManager)
            {
                // Update the TimeManager
                timeManager.updateRate = 0f;
            }
        }

        public void UpdateRateMinusClicked()
        {
            if (timeManager)
            {
                // Update the TimeManager
                timeManager.updateRate--;
            }
        }

        #endregion //UPDATERATE_PRESETS

    }
}