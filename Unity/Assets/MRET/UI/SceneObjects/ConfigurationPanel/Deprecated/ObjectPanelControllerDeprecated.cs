// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Locomotion;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;
using GOV.NASA.GSFC.XR.MRET.Tools.UndoRedo;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects
{
    [System.Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.ObjectPanelController) + " class")]
    public class ObjectPanelControllerDeprecated : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(ObjectPanelControllerDeprecated);
            }
        }

        public GameObject selectedObject;
        public Toggle physics, visible, locked; //public Toggle physics, gravity, visible, locked;
        public Slider opacity;
        public Text titleText;
        public Toggle gravity;

        /// <summary>
        /// The explode toggle.
        /// </summary>
        [Tooltip("The toggle for the local/global explode scale.")]
        public Toggle explodeEnabledToggle;

        private bool enabling = false;
        private bool initialized = false;

        /// <summary>
        /// The object of interest to explode.
        /// </summary>
        private InteractablePartDeprecated selectedPart;

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)
                    ? IntegrityState.Failure      // Fail is base class fails or anything is null
                    : IntegrityState.Success);    // Otherwise, our integrity is valid
        }

        public void Initialize()
        {
            if (selectedObject == null)
            {
                Debug.LogWarning("[ObjectPanelController] Selected object not set.");
                return;
            }

            // Initialize physics toggle.
            physics.isOn = !selectedObject.GetComponent<Rigidbody>().isKinematic;

            // Initialize gravity toggle.
            gravity.isOn = selectedObject.GetComponent<Rigidbody>().useGravity;

            // Initialize visibility toggle.
            visible.isOn = selectedObject.GetComponentInChildren<MeshRenderer>().enabled;

            //Initialize locked toggle
            locked.isOn = InteractableSceneObjectDeprecated.GetSceneObjectForGameObject(selectedObject).locked;

            // Obtain the interactable part reference because that is what we need for
            // the explode capability
            selectedPart = selectedObject.GetComponent<InteractablePartDeprecated>();

            // Initialize the selected part explode state
            if (selectedPart)
            {
                bool explodeEnabled = false;

                // Initialize whether we are exploding
                if (explodeEnabledToggle)
                {
                    explodeEnabled = explodeEnabledToggle.isOn = selectedPart.explodeActive;
                }

                // Update the selected part explode state
                UpdateExplodeState(explodeEnabled);
            }

            initialized = true;
        }

        public void OnEnable()
        {
            if (initialized == false)
            {
                return;
            }

            if (selectedObject == null)
            {
                Debug.LogWarning("[ObjectPanelController] No selected object.");
                return;
            }

            enabling = true;

            // Initialize physics toggle.
            physics.isOn = !selectedObject.GetComponent<Rigidbody>().isKinematic;

            // Initialize gravity toggle.
            gravity.isOn = selectedObject.GetComponent<Rigidbody>().useGravity;

            // Initialize visibility toggle.
            visible.isOn = selectedObject.GetComponentInChildren<MeshRenderer>().enabled;

            //Initialize locked toggle
            locked.isOn = InteractableSceneObjectDeprecated.GetSceneObjectForGameObject(selectedObject).locked;

            // Initialize whether we are exploding
            if (selectedPart && explodeEnabledToggle)
            {
                explodeEnabledToggle.isOn = selectedPart.explodeActive;
            }

            enabling = false;
        }

        public void SetTitle(string titleToSet)
        {
            if (titleText != null)
            {
                // Limit title to 15 characters.
                titleText.text = titleToSet.Substring(0, System.Math.Min(titleToSet.Length, 15));
                if (titleToSet.Length > 15)
                {
                    titleText.text = titleText.text + "...";
                }
            }
        }

        public void ToggleGravity()
        {
            if (!enabling)
            {
                bool state = gravity.isOn;
                if (selectedObject != null)
                {
                    Rigidbody rBody = selectedObject.GetComponent<Rigidbody>();

                    // Only turn on gravity if physics is on.
                    if ((!physics.isOn) || (!state))
                    {
                        rBody.useGravity = false;
                        gravity.isOn = false;
                    }
                    else if (physics.isOn)
                    {
                        rBody.useGravity = true;
                    }

                    InteractablePartDeprecated iPart = selectedObject.GetComponent<InteractablePartDeprecated>();
                    UndoManagerDeprecated.instance.AddAction(ProjectActionDeprecated.UpdateObjectSettingsAction(iPart.name,
                        new InteractablePartDeprecated.InteractablePartSettings(iPart.grabbable,
                        !rBody.isKinematic, rBody.useGravity), iPart.guid.ToString()),
                        ProjectActionDeprecated.UpdateObjectSettingsAction(iPart.name,
                        new InteractablePartDeprecated.InteractablePartSettings(iPart.grabbable,
                        !rBody.isKinematic, !rBody.useGravity), iPart.guid.ToString()));
                }
            }
        }

        public void TogglePhysics()
        {
            if (!enabling)
            {
                bool state = physics.isOn;
                if (selectedObject != null)
                {
                    // Turn off gravity if physics is off.
                    if (!state)
                    {
                        if (gravity.isOn)
                        {
                            ToggleGravity();
                        }
                    }

                    Rigidbody rBody = selectedObject.GetComponent<Rigidbody>();
                    rBody.isKinematic = !state;

                    InteractablePartDeprecated iPart = selectedObject.GetComponent<InteractablePartDeprecated>();
                    UndoManagerDeprecated.instance.AddAction(ProjectActionDeprecated.UpdateObjectSettingsAction(iPart.name,
                        new InteractablePartDeprecated.InteractablePartSettings(iPart.grabbable,
                        !rBody.isKinematic, rBody.useGravity), iPart.guid.ToString()),
                        ProjectActionDeprecated.UpdateObjectSettingsAction(iPart.name,
                        new InteractablePartDeprecated.InteractablePartSettings(iPart.grabbable,
                        rBody.isKinematic, rBody.useGravity), iPart.guid.ToString()));
                }
            }
        }

        /// <summary>
        /// Updates the selected part explode state
        /// </summary>
        /// <param name="explodeEnabled">Indicated if the exploding is enabled</param>
        protected void UpdateExplodeState(bool explodeEnabled)
        {
            // Update the selected part explode state
            if (selectedPart)
            {
                if (explodeEnabled)
                {
                    Log("Starting explode for part: " + selectedPart.name, nameof(UpdateExplodeState));
                    selectedPart.StartExplode();
                    MRET.DataManager.SaveValue(LocomotionManager.explodeObjectKey, selectedPart);
                    MRET.LocomotionManager.EnableExploding();
                }
                else
                {
                    Log("Unexploding part: " + selectedPart.name, nameof(UpdateExplodeState));
                    MRET.LocomotionManager.DisableExploding();
                    MRET.DataManager.SaveValue(LocomotionManager.explodeObjectKey, null);
                    selectedPart.StopExplode();
                }
            }
        }

        /// <summary>
        /// Toggles the exploding
        /// </summary>
        public void ToggleExplode()
        {
            if (!enabling)
            {
                bool state = explodeEnabledToggle.isOn;

                // Update the selected part explode state
                UpdateExplodeState(state);
            }
        }

        public void ToggleVisibility()
        {
            if (!enabling)
            {
                bool state = visible.isOn;
                if (selectedObject != null)
                {
                    foreach (MeshRenderer rend in selectedObject.GetComponentsInChildren<MeshRenderer>())
                    {
                        rend.enabled = state;
                    }
                }
            }
        }

        public void ToggleLock()
        {
            if (!enabling)
            {
                bool state = locked.isOn;
                InteractableSceneObjectDeprecated selectedSceneObject = InteractableSceneObjectDeprecated.GetSceneObjectForGameObject(selectedObject);
                if (selectedSceneObject != null)
                {
                    selectedSceneObject.locked = state;
                }
            }
        }

        public void ChangeOpacity()
        {
            Renderer[] rends = selectedObject.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < rends.Length; i++)
            {
                Color colorToChange = rends[i].material.color;
                colorToChange.a = opacity.value;
                rends[i].material.color = colorToChange;
            }
        }

        public void physicsToggle(bool state)
        {
            physics.isOn = state;
        }

        public void gravityToggle(bool state)
        {
            gravity.isOn = state;
        }
    }
}