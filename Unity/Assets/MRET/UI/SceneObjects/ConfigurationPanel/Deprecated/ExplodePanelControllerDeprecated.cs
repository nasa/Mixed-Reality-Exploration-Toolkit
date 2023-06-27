// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Part
{
    /// <remarks>
    /// History:
    /// 15 September 2022: Created
    /// 22 Sept 2022: Refactored during integration into 22.1 (J. Hosler)
    /// </remarks>
    ///
    /// <summary>
    /// ExplodePanelControllerDeprecated
    ///
    /// Controls the interaction of the UI components within the part explode panel
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    [System.Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.ExplodePanelController) + " class")]
    public class ExplodePanelControllerDeprecated : MRETBehaviour
	{
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName
		{
			get
			{
				return nameof(ExplodePanelControllerDeprecated);
			}
		}

        /// <summary>
        /// The object of interest to explode.
        /// </summary>
        private InteractablePartDeprecated selectedPart;

        /// <summary>
        /// The text component to contain the selected object name for the panel.
        /// </summary>
        public Text selectedObjectNameText;

        /// <summary>
        /// The text component to display the current explode value.
        /// </summary>
        [Tooltip("The text component to display the current explode scale.")]
        public Text explodeScaleText;

        /// <summary>
        /// The slider to change the local explode scale.
        /// </summary>
        [Tooltip("The slider for the local explode scale.")]
        public Slider localExplodeScaleSlider;

        /// <summary>
        /// The local vs global explode scale toggle.
        /// </summary>
        [Tooltip("The toggle for the local/global explode scale.")]
        public Toggle localExplodeEnabledToggle;

        /// <summary>
        /// The random explode vectors toggle.
        /// </summary>
        [Tooltip("The toggle for random explode vectors.")]
        public Toggle randomExplodeVectorsToggle;

        private bool _ignoreEvents = false;
        private bool _initialized = false;

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) ||
                (selectedObjectNameText == null) ||
                (explodeScaleText == null) ||
                (localExplodeScaleSlider == null) ||
                (localExplodeEnabledToggle == null)  ||
                (randomExplodeVectorsToggle == null) // TODO: || (MyRequiredRef == null)
                    ? IntegrityState.Failure         // Fail is base class fails or anything is null
                    : IntegrityState.Success);       // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            // Initialize the scale components to their initial state
            ResetExplodeComponents();

            // Setup the local explode scale slider
            if (localExplodeScaleSlider)
            {
                // Listen for slider changes
                localExplodeScaleSlider.onValueChanged.AddListener((v) =>
                {
                    SetExplodeScale(v);
                });
            }
            else
            {
                LogError("Explode slider reference is not set.", nameof(MRETStart));
            }
        }

        /// <summary>
        /// Initializes the panel.
        /// </summary>
        /// <param name="selected">The object to work with for deletion.</param>
        public void Initialize(GameObject selected)
        {
            if (selected == null)
            {
                LogWarning("Selected object not set", nameof(Initialize));
                return;
            }

            // Obtain the interactable part reference because that is what we need for
            // the explode capability
            selectedPart = selected.GetComponent<InteractablePartDeprecated>();

            // Initialize the selected part explode state
            if (selectedPart != null)
            {
                Log("Initializing explode settings for part: " + selectedPart.name, nameof(Initialize));

                // Set the part name
                SetSelectedObjectName(selectedPart.transform.name);

                // Initialize the explode components to their initial state. Do this before updating the explode state!
                ResetExplodeComponents();

                // Update the explode state
                UpdateExplodeState();
            }

            _initialized = true;
        }

        /// <summary>
        /// Updates the selected part explode state
        /// </summary>
        protected void UpdateExplodeState()
        {
            // Update the selected part explode state
            if (selectedPart)
            {
                // Determine if random vectors are being used
                if (randomExplodeVectorsToggle)
                {
                    if (randomExplodeVectorsToggle.isOn)
                    {
                        selectedPart.explodeMode = Preferences.ExplodeMode.Random;
                    }
                    else
                    {
                        selectedPart.explodeMode = Preferences.ExplodeMode.Relative;
                    }
                }

                // Determine if local or global explode scale is being used
                if (localExplodeEnabledToggle)
                {
                    if (localExplodeEnabledToggle.isOn)
                    {
                        selectedPart.explodeScaleValue = localExplodeScaleSlider.value;
                    }
                    else
                    {
                        selectedPart.explodeScaleValue = MRET.ConfigurationManager.preferences.GlobalExplodeScaleValue;
                    }
                }

                // Log the value
                Log("Setting explode scale value to '" + selectedPart.explodeScaleValue +
                    "' for part: " + selectedPart.name, nameof(Initialize));
            }
        }

        /// <summary>
        /// Sets the selected object name text
        /// </summary>
        /// <param name="selectedObjectName">The selected object name to display in the Text component</param>
        protected void SetSelectedObjectName(string selectedObjectName)
        {
            if (selectedObjectNameText != null)
            {
                selectedObjectNameText.text = selectedObjectName;
            }
        }

        /// <summary>
        /// Sets the explode value
        /// </summary>
        /// <param name="value"></param>
        public void SetExplodeScale(float value)
        {
            if (_ignoreEvents) return;

            // Set the selected part scale
            if (selectedPart != null)
            {
                selectedPart.explodeScaleValue = value;
                Log("Setting local explode scale value to '" + value + "' for part: " + selectedPart.name, nameof(SetExplodeScale));
            }

            // Reset the slider values to their new state
            ResetExplodeComponents();
        }

        /// <summary>
        /// Resets the explode value components to match the preferences
        /// </summary>
        protected virtual void ResetExplodeComponents()
        {
            // Update the slider values to match the preferences
            if (localExplodeScaleSlider)
            {
                // Update the slider range based upon the global settings
                localExplodeScaleSlider.minValue = MRET.ConfigurationManager.preferences.GlobalExplodeMin;
                localExplodeScaleSlider.maxValue = MRET.ConfigurationManager.preferences.GlobalExplodeMax;

                // Look at the selected part to see certain settings
                if (selectedPart)
                {
                    _ignoreEvents = true;

                    randomExplodeVectorsToggle.isOn = (selectedPart.explodeMode == Preferences.ExplodeMode.Random);

                    // TODO: Does this trigger the event?
                    localExplodeScaleSlider.value = selectedPart.explodeScaleValue;

                    _ignoreEvents = false;
                }

                // Toggle the local explode scale slider
                if (localExplodeEnabledToggle)
                {
                    localExplodeScaleSlider.gameObject.SetActive(localExplodeEnabledToggle.isOn);
                }

                // Update the text component to match the explode scale
                if (explodeScaleText)
                {
                    explodeScaleText.text = localExplodeScaleSlider.value.ToString();
                }
            }
        }

        /// <summary>
        /// Toggles the local/global explode scale
        /// </summary>
        public void ToggleUsingLocalExplodeScale()
        {
            if (_ignoreEvents) return;

            // Update the explode scale
            UpdateExplodeState();

            // Update the scale components
            ResetExplodeComponents();
        }

        /// <summary>
        /// Toggles the random explode vectors
        /// </summary>
        public void ToggleUsingRandomExplodeVectors()
        {
            if (_ignoreEvents) return;

            // Update the explode scale
            UpdateExplodeState();

            // Update the scale components
            ResetExplodeComponents();
        }
    }
}
