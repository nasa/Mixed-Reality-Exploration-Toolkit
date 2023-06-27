// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Part
{
    /// <summary>
    /// FIXME: DEPRECATED. Use ObjectPanelController
    /// </summary>
    /// <see cref="ObjectPanelController"/>
    [System.Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.ObjectPanelController) + " class")]
    public class PartPanelController : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(PartPanelController);

        public GameObject selectedObject;
        public GameObject SelectedObject
        {
            get => selectedObject;
            set
            {
                Part = null;
                if (value != null)
                {
                    Part = selectedObject.GetComponent<InteractablePart>();
                    if (Part == null)
                    {
                        LogWarning("No interactable part component in selected object.", nameof(SelectedObject));
                    }
                }
            }
        }
        protected InteractablePart Part;

        public Toggle physics, explode, visible, locked; //public Toggle physics, gravity, explode, visible, locked;
        public Slider opacity;
        public Text titleText;
        public Toggle gravity;

        public void Initialize()
        {
        }

        #region MRETUpdateBehaviour
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) || // TODO: || (MyRequiredRef == null)
                (selectedObject == null)
                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            if (selectedObject == null)
            {
                LogWarning("No selected object.", nameof(MRETStart));
                return;
            }

            if (Part == null)
            {
                LogWarning("No interactable part component in object.", nameof(MRETStart));
                return;
            }

            // Initialize physics toggle.
            physics.isOn = !Part.EnableCollisions;

            // Initialize gravity toggle.
            gravity.isOn = Part.EnableGravity;

            // Initialize explode toggle.
            explode.isOn = Part.Exploding;

            // Initialize visibility toggle.
            visible.isOn = Part.Visible;

            //Initialize locked toggle
            locked.isOn = !Part.Usable;
        }
        #endregion MRETUpdateBehaviour

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
            if (Part == null)
            {
                LogWarning("Object selection did not contain a valid interactable part.", nameof(ToggleGravity));
                return;
            }

            // Initialize gravity toggle.
            gravity.isOn = Part.EnableGravity;

            // Turn off gravity if physics is off.
            if (!physics.isOn)
            {
                Part.EnableGravity = false;
                gravity.isOn = false;
            }
            else
            {
                // Toggle the gravity
                gravity.isOn = !gravity.isOn;

                // Apply the new gravity setting
                ProjectManager.UndoManager.AddAction(
                    new UpdatePhysicsAction(Part, Part.EnableCollisions, Part.EnableGravity, Part.Mass),
                    new UpdatePhysicsAction(Part, Part.EnableCollisions, !Part.EnableGravity, Part.Mass));
            }
        }

        public void TogglePhysics()
        {
            if (Part == null)
            {
                LogWarning("Object selection did not contain a valid interactable part.", nameof(TogglePhysics));
                return;
            }

            // Initialize physics toggle.
            physics.isOn = !Part.EnableCollisions;

            // Turn off gravity if physics is off.
            if (!physics.isOn && gravity.isOn)
            {
                ToggleGravity();
            }

            // Register the action
            ProjectManager.UndoManager.AddAction(
                new UpdatePhysicsAction(Part, Part.EnableCollisions, Part.EnableGravity, Part.Mass),
                new UpdatePhysicsAction(Part, !Part.EnableCollisions, Part.EnableGravity, Part.Mass));
        }

        public void ToggleExplosion()
        {
            bool state = explode.isOn;
            if (Part != null)
            {
                if (state)
                {
                    // Unexplode.
                    Part.Explode();
                }
                else
                {
                    // Explode.
                    Part.Unexplode();
                }
            }
        }

        public void ToggleVisibility()
        {
            visible.isOn = !visible.isOn;

            if (Part != null)
            {
                Part.Visible = visible.isOn;
            }
        }

        public void ToggleLock()
        {
            locked.isOn = !locked.isOn;

            if (Part != null)
            {
                Part.Usable = !locked.isOn;
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

        public void EnablePhysics(bool state)
        {
            physics.isOn = state;
            TogglePhysics();
        }

        public void EnableGravity(bool state)
        {
            gravity.isOn = state;
            ToggleGravity();
        }
    }
}