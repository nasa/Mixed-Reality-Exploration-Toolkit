// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects
{
    /// <remarks>
    /// History:
    /// 15 September 2022: Created
    /// 22 Sept 2022: Refactored during integration into 22.1 (J. Hosler)
    /// </remarks>
	///
	/// <summary>
	/// ExplodePanelController
	///
	/// Controls the interaction of the UI components within the part explode panel
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public class ExplodePanelController : MRETBehaviour
	{
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(ExplodePanelController);

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

        /// <summary>
        /// The object of interest to explode.
        /// </summary>
        private IPhysicalSceneObject selectedObject;

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
        /// Initializes the panel with the supplied scene object.
        /// </summary>
        /// <param name="selected">The <code>ISceneObject</code> to work with for exploding.</param>
        public void Initialize(IPhysicalSceneObject selected)
        {
            if (selected == null)
            {
                LogWarning("Selected object not set", nameof(Initialize));
                return;
            }

            // Assign the physical scene obect reference because that is what we will use for exploding
            selectedObject = selected;

            // Initialize the selected object explode state
            Log("Initializing explode settings for part: " + selected.name, nameof(Initialize));

            // Set the part name
            SetSelectedObjectName(selected.name);

            // Initialize the explode components to their initial state. Do this before updating the explode state!
            ResetExplodeComponents();

            // Update the explode state
            UpdateExplodeState();

            _initialized = true;
        }

        /// <summary>
        /// Updates the selected part explode state
        /// </summary>
        protected void UpdateExplodeState()
        {
            // Update the selected object explode state
            if (selectedObject != null)
            {
                // Determine if random vectors are being used
                if (randomExplodeVectorsToggle)
                {
                    if (randomExplodeVectorsToggle.isOn)
                    {
                        selectedObject.ExplodeMode = Preferences.ExplodeMode.Random;
                    }
                    else
                    {
                        selectedObject.ExplodeMode = Preferences.ExplodeMode.Relative;
                    }
                }

                // Determine if local or global explode scale is being used
                if (localExplodeEnabledToggle)
                {
                    if (localExplodeEnabledToggle.isOn)
                    {
                        selectedObject.ExplodeScale = localExplodeScaleSlider.value;
                    }
                    else
                    {
                        selectedObject.ExplodeScale = MRET.ConfigurationManager.preferences.GlobalExplodeScaleValue;
                    }
                }

                // Log the value
                Log("Setting explode scale to '" + selectedObject.ExplodeScale +
                    "' for object: " + selectedObject.name, nameof(Initialize));
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
            if (selectedObject != null)
            {
                selectedObject.ExplodeScale = value;
                Log("Setting local explode scale value to '" + value + "' for part: " + selectedObject.name, nameof(SetExplodeScale));
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
                if (selectedObject != null)
                {
                    _ignoreEvents = true;

                    randomExplodeVectorsToggle.isOn = (selectedObject.ExplodeMode == Preferences.ExplodeMode.Random);

                    // TODO: Does this trigger the event?
                    localExplodeScaleSlider.value = selectedObject.ExplodeScale;

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
