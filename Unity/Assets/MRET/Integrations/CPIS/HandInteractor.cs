// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.CPIS
{
    public class HandInteractor : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(HandInteractor);

        public InputHand hand;

        public IInteractable[] touchedObjects => currentTouching.ToArray();

        private List<IInteractable> currentTouching = new List<IInteractable>();

        private IInteractable currentInteractable;

        private bool grabbing = false;

        private bool pauseRequested = false;

        #region MRETBehaviour
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)

                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }
        #endregion MRETBehaviour

        public bool IsGrabbing()
        {
            return grabbing;
        }

        public void Grab()
        {
            if (grabbing)
            {
                Ungrab();
            }

            if (!CanPerformInteraction())
            {
                return;
            }

            // Obtain a hard reference to the interactable so that it doesn't get garbage collected,
            // which surfaced running in Hololens simulation. Unity bug suspected given that an interface
            // reference should work, but still passed the if check and went null in BeginGrab. Did not
            // see this issue outside of the Hololens simulation.
            Object hardReference = currentInteractable as Object;
            if ((hardReference != null) && currentInteractable.Grabbable)
            {
                // Pause locomotion because user is performing a grab action
                MRET.LocomotionManager.PauseRequest();
                pauseRequested = true;

                // Begin the grab
                currentInteractable.BeginGrab(hand);
                grabbing = currentInteractable.IsGrabbing;
                Log("Grabbing: " + currentInteractable.id);
            }
        }

        public void Ungrab()
        {
            if (!grabbing)
            {
                return;
            }

            // Obtain a hard reference to the interactable so that it doesn't get garbage collected,
            // which surfaced running in Hololens simulation. Unity bug suspected given that an interface
            // reference should work, but still passed the if check and went null in EndGrab. Did not
            // see this issue outside of the Hololens simulation.
            Object hardReference = currentInteractable as Object;
            if ((hardReference != null) && currentInteractable.IsGrabbing)
//                && currentInteractable.transform.IsChildOf(transform))
            {
                currentInteractable.EndGrab(hand);
                Log("Ungrabbing: " + currentInteractable.id);
            }

            // Unpause locomotion if we requested a pause
            if (pauseRequested)
            {
                MRET.LocomotionManager.PauseRelease();
            }
            grabbing = false;
        }

        public void Place()
        {
            foreach (IInteractable interactable in GetComponentsInChildren<IInteractable>())
            {
                interactable.EndPlacing();
            }
        }

        public void Use(InputHand inputHand)
        {
            if (!CanPerformInteraction())
            {
                return;
            }

            if ((currentInteractable != null) && currentInteractable.Usable)
            {
                currentInteractable.Use(inputHand);
            }
            else
            {
                foreach (IInteractable usedInteractable in MRET.UuidRegistry.Interactables)
                {
                    usedInteractable.Unuse(inputHand);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (grabbing)
            {
                return;
            }

            if (other == null)
            {
                return;
            }

            // Get the interactable from the game object
            IInteractable touchingInteractable = other.gameObject.GetComponent<IInteractable>();
            if ((touchingInteractable == null) && (other.attachedRigidbody != null))
            {
                // Try the attached rigid body
                touchingInteractable = other.attachedRigidbody.GetComponent<IInteractable>();
            }

            if (touchingInteractable != null)
            {
                // Register the interactable
                currentInteractable = touchingInteractable;
                if (!currentTouching.Contains(touchingInteractable))
                {
                    currentTouching.Add(touchingInteractable);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (grabbing)
            {
                return;
            }

            if (other == null)
            {
                return;
            }

            if (other.attachedRigidbody == null)
            {
                return;
            }

            IInteractable touchingInteractable = other.attachedRigidbody.GetComponent<IInteractable>();
            if ((touchingInteractable != null) && (touchingInteractable == currentInteractable))
            {
                currentInteractable = null;
                if (currentTouching.Contains(touchingInteractable))
                {
                    currentTouching.Remove(touchingInteractable);
                }
            }
        }

        private bool CanPerformInteraction()
        {
            return true; //!((bool) MRET.DataManager.FindPoint(LineDrawingManager.ISDRAWINGFLAGKEY));
        }
    }
}