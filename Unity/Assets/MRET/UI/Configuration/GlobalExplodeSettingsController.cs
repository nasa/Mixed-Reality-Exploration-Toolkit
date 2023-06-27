// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET.UI.Configuration
{
    /// <remarks>
    /// History:
    /// 13 September 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// GlobalExplodeSettingsController
	///
	/// Controls the interaction of the UI components within the global explode settings panel
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public class GlobalExplodeSettingsController : MRETBehaviour
	{
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName
		{
			get
			{
				return nameof(GlobalExplodeSettingsController);
			}
		}

        /// <summary>
        /// The text component to display the explode max.
        /// </summary>
        [Tooltip("The text component to display the explode max.")]
        public Text globalExplodeMaxText;

        /// <summary>
        /// The text component to display the current explode value.
        /// </summary>
        [Tooltip("The text component to display the current explode value.")]
        public Text globalExplodeValueText;

        /// <summary>
        /// The input field for the global explode max.
        /// </summary>
        [Tooltip("The input field for the explode max.")]
        public InputField globalExplodeMaxInputField;

        /// <summary>
        /// The slider to change the global explode value.
        /// </summary>
        [Tooltip("The slider for the explode value.")]
        public Slider globalExplodeSlider;

        /// <summary>
        /// The local vs global explode scale toggle.
        /// </summary>
        [Tooltip("The toggle indicating if random vectors are used for explode.")]
        public Toggle randomVectorsToggle;
        
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
		{
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) ||
                (globalExplodeSlider == null) || // TODO: || (MyRequiredRef == null)
                (randomVectorsToggle == null)    // TODO: || (MyRequiredRef == null)
                    ? IntegrityState.Failure     // Fail is base class fails or anything is null
                    : IntegrityState.Success);   // Otherwise, our integrity is valid
		}

		/// <seealso cref="MRETBehaviour.MRETStart"/>
		protected override void MRETStart()
		{
			// Take the inherited behavior
			base.MRETStart();

            // Initialize the value components to their initial state
            ResetValueComponents();

            // Setup the value slider
            if (globalExplodeSlider)
            {
                // Listen for slider changes
                globalExplodeSlider.onValueChanged.AddListener((v) =>
                {
                    SetGlobalExplodeValue(v);
                });
            }
            else
            {
                LogError("Global explode slider reference is not set.", nameof(MRETStart));
            }

            // Initialize the max text components
            ResetMaxComponents();

            // Setup the global max value input field
            if (globalExplodeMaxInputField)
            {
                // Slider listener for value changes
                globalExplodeMaxInputField.onValueChanged.AddListener((v) =>
                {
                    SetGlobalExplodeMax(v);
                });
            }
            else
            {
                LogError("Global explode max reference is not set.", nameof(MRETStart));
            }
        }

        /// <summary>
        /// Sets the global explode value
        /// </summary>
        /// <param name="value"></param>
        public void SetGlobalExplodeValue(float value)
        {
            MRET.ConfigurationManager.preferences.GlobalExplodeScaleValue = value;

            // Reset the slider values to their new state
            ResetValueComponents();
        }

        /// <summary>
        /// Sets the global explode max
        /// </summary>
        /// <param name="strValue"></param>
        public void SetGlobalExplodeMax(string strValue)
        {
            int value;
            if (int.TryParse(strValue, out value))
            {
                // Update the max
                MRET.ConfigurationManager.preferences.GlobalExplodeMax = value;

                // Reset the max components
                ResetMaxComponents();

                // Reset the value components
                ResetValueComponents();
            }
            else
            {
                LogError("Failed to convert text input to a valid integer.", nameof(SetGlobalExplodeMax));
            }
        }

        /// <summary>
        /// Resets the global explode value components to match the preferences
        /// </summary>
        protected virtual void ResetValueComponents()
        {
            // Update the slider values to match the preferences
            if (globalExplodeSlider)
            {
                // Update the slider range and value
                globalExplodeSlider.minValue = MRET.ConfigurationManager.preferences.GlobalExplodeMin;
                globalExplodeSlider.maxValue = MRET.ConfigurationManager.preferences.GlobalExplodeMax;
                globalExplodeSlider.value = MRET.ConfigurationManager.preferences.GlobalExplodeScaleValue;

                // Update the text component to match the global explode value
                if (globalExplodeValueText)
                {
                    globalExplodeValueText.text = globalExplodeSlider.value.ToString();
                }
            }
        }

        /// <summary>
        /// Resets the global explode max components to match the preferences
        /// </summary>
        protected virtual void ResetMaxComponents()
        {
            // Pull the max value preference out as an integer
            string globalExplodeMaxStr = ((int)MRET.ConfigurationManager.preferences.GlobalExplodeMax).ToString();

            // Set the text component for the global explode max value
            if (globalExplodeMaxText)
            {
                globalExplodeMaxText.text = globalExplodeMaxStr;
            }

            // Set the input field for the global explode max value
            if (globalExplodeMaxInputField)
            {
                globalExplodeMaxInputField.text = globalExplodeMaxStr;
            }
        }

        /// <summary>
        /// Toggles the local/global explode scale
        /// </summary>
        public void ToggleRandomVectors()
        {
            if (randomVectorsToggle)
            {
                bool state = randomVectorsToggle.isOn;

                if (state)
                {
                    MRET.ConfigurationManager.preferences.globalExplodeMode = Preferences.ExplodeMode.Random;
                }
                else
                {
                    MRET.ConfigurationManager.preferences.globalExplodeMode = Preferences.ExplodeMode.Relative;
                }
            }
        }
    }
}
