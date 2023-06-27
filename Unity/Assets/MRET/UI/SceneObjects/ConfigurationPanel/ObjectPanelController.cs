// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.Utilities.Math;
using GOV.NASA.GSFC.XR.MRET.Locomotion;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects
{
    public class ObjectPanelController : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(ObjectPanelController);

        public Toggle physics;
        public Toggle gravity;
        public Toggle visible;
        public Toggle locked;

        [Tooltip("The toggle for the local/global explode scale.")]
        public Toggle explodeEnabledToggle;

        public Slider opacity;
        public Text titleText;

        public ISceneObject SelectedObject { get; private set; }

        private bool enabling = false;
        private bool initialized = false;

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

        public void Initialize(ISceneObject selectedObject, string panelTitle = null)
        {
            if (selectedObject == null)
            {
                LogWarning("Selected object not set.", nameof(Initialize));
                return;
            }

            SelectedObject = selectedObject;

            // Initialize physics toggle.
            physics.isOn = (SelectedObject is IPhysicalSceneObject) ?
                (SelectedObject as IPhysicalSceneObject).EnableCollisions :
                false;

            // Initialize gravity toggle.
            gravity.isOn = (SelectedObject is IPhysicalSceneObject) ?
                (SelectedObject as IPhysicalSceneObject).EnableGravity :
                false;

            // Initialize visibility toggle.
            visible.isOn = (SelectedObject is IPhysicalSceneObject) ?
                (SelectedObject as IPhysicalSceneObject).Visible :
                false;

            // Initialize locked toggle
            locked.isOn = (SelectedObject is IInteractable) ?
                !(SelectedObject as IInteractable).Usable :
                true;

            // Initialize explode toggle.
            explodeEnabledToggle.isOn = (SelectedObject is IPhysicalSceneObject) ?
                (SelectedObject as IPhysicalSceneObject).Exploding :
                false;

            // Initialize the selected part explode state
            if (SelectedObject is IPhysicalSceneObject)
            {
                // Update the selected part explode state
                UpdateExplodeState(explodeEnabledToggle.isOn);
            }

            // Set the panel title
            SetTitle(panelTitle ?? SelectedObject.name);

            initialized = true;
        }

        public void OnEnable()
        {
            if (initialized == false)
            {
                return;
            }

            if (SelectedObject == null)
            {
                LogWarning("No selected object.", nameof(OnEnable));
                return;
            }

            enabling = true;

            // Initialize physics toggle.
            physics.isOn = (SelectedObject is IPhysicalSceneObject) ?
                (SelectedObject as IPhysicalSceneObject).EnableCollisions :
                false;

            // Initialize gravity toggle.
            gravity.isOn = (SelectedObject is IPhysicalSceneObject) ?
                (SelectedObject as IPhysicalSceneObject).EnableGravity :
                false;

            // Initialize visibility toggle.
            visible.isOn = (SelectedObject is IPhysicalSceneObject) ?
                (SelectedObject as IPhysicalSceneObject).Visible :
                false;

            // Initialize locked toggle
            locked.isOn = (SelectedObject is IInteractable) ?
                !(SelectedObject as IInteractable).Usable :
                true;

            // Initialize explode toggle.
            explodeEnabledToggle.isOn = (SelectedObject is IPhysicalSceneObject) ?
                (SelectedObject as IPhysicalSceneObject).Exploding :
                false;

            // Initialize the selected part explode state
            if (SelectedObject is IPhysicalSceneObject)
            {
                // Update the selected part explode state
                UpdateExplodeState(explodeEnabledToggle.isOn);
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
                if (SelectedObject is IPhysicalSceneObject)
                {
                    IPhysicalSceneObject physicalSceneObject = (SelectedObject as IPhysicalSceneObject);

                    // Only turn on gravity if physics is on.
                    if ((!physics.isOn) || (!state))
                    {
                        physicalSceneObject.EnableGravity = false;
                        gravity.isOn = false;
                    }
                    else if (physics.isOn)
                    {
                        physicalSceneObject.EnableGravity = true;
                    }

                    ProjectManager.UndoManager.AddAction(
                        new UpdatePhysicsAction(physicalSceneObject,
                            physicalSceneObject.EnableCollisions, physicalSceneObject.EnableGravity, physicalSceneObject.Mass),
                        new UpdatePhysicsAction(physicalSceneObject,
                            physicalSceneObject.EnableCollisions, !physicalSceneObject.EnableGravity, physicalSceneObject.Mass));
                }
            }
        }

        public void TogglePhysics()
        {
            if (!enabling)
            {
                bool state = physics.isOn;
                if (SelectedObject is IPhysicalSceneObject)
                {
                    IPhysicalSceneObject physicalSceneObject = (SelectedObject as IPhysicalSceneObject);

                    // Turn off gravity if physics is off.
                    if (!state)
                    {
                        if (gravity.isOn)
                        {
                            ToggleGravity();
                        }
                    }

                    physicalSceneObject.EnableCollisions = state;

                    ProjectManager.UndoManager.AddAction(
                        new UpdatePhysicsAction(physicalSceneObject,
                            physicalSceneObject.EnableCollisions, physicalSceneObject.EnableGravity, physicalSceneObject.Mass),
                        new UpdatePhysicsAction(physicalSceneObject,
                            !physicalSceneObject.EnableCollisions, physicalSceneObject.EnableGravity, physicalSceneObject.Mass));
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
            if (SelectedObject is IPhysicalSceneObject)
            {
                IPhysicalSceneObject physicalSceneObject = (SelectedObject as IPhysicalSceneObject);

                if (explodeEnabled)
                {
                    Log("Starting explode for part: " + physicalSceneObject.name, nameof(UpdateExplodeState));
                    physicalSceneObject.StartExplode();
                    MRET.DataManager.SaveValue(LocomotionManager.explodeObjectKey, physicalSceneObject);
                    MRET.LocomotionManager.EnableExploding();
                }
                else
                {
                    Log("Unexploding part: " + physicalSceneObject.name, nameof(UpdateExplodeState));
                    MRET.LocomotionManager.DisableExploding();
                    MRET.DataManager.SaveValue(LocomotionManager.explodeObjectKey, null);
                    physicalSceneObject.StopExplode();
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
                if (SelectedObject is IPhysicalSceneObject)
                {
                    IPhysicalSceneObject physicalSceneObject = (SelectedObject as IPhysicalSceneObject);

                    physicalSceneObject.Visible = state;
                }
            }
        }

        public void ToggleLock()
        {
            if (!enabling)
            {
                bool state = locked.isOn;
                if (SelectedObject is IInteractable)
                {
                    IInteractable interactableSceneObject = (SelectedObject as IInteractable);

                    interactableSceneObject.Usable = !state;
                }
            }
        }

        public void ChangeOpacity()
        {
            if (SelectedObject is IPhysicalSceneObject)
            {
                IPhysicalSceneObject physicalSceneObject = (SelectedObject as IPhysicalSceneObject);

                byte normalizedOpacity = (byte) MathUtil.Normalize(opacity.value, opacity.minValue, opacity.maxValue, 0, 255);
                physicalSceneObject.Opacity = normalizedOpacity;
            }
        }

        public void PhysicsToggle(bool state)
        {
            physics.isOn = state;
        }

        public void GravityToggle(bool state)
        {
            gravity.isOn = state;
        }
    }
}