// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.CPIS
{
    public class HandInteractorDeprecated : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(HandInteractorDeprecated);
            }
        }

        public InputHand hand;

        public SceneObjectDeprecated[] touchedObjects
        {
            get
            {
                return currentTouching.ToArray();
            }
        }

        private List<SceneObjectDeprecated> currentTouching = new List<SceneObjectDeprecated>();

        private SceneObjectDeprecated currentInteractable;

        private bool grabbing = false;

        private bool stillTouching = false; // if hand is still touching the currentInteractable while grabbing

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

        public void Grab()
        {
            Ungrab();

            if (!CanPerformInteraction())
            {
                return;
            }

            if (currentInteractable != null)
            {
                if (currentInteractable.grabbable)
                {
                    MRET.LocomotionManager.PauseRequest();
                    currentInteractable.BeginGrab(hand);
                    grabbing = true;
                    stillTouching = true;
                }
            }
        }

        public void Ungrab()
        {
            if (currentInteractable != null && currentInteractable.IsGrabbedBy(hand))
            {
                currentInteractable.EndGrab(hand);
                
                // fixed issue when grab ends in constrained motion mode
                if(!stillTouching)
                {
                    currentInteractable = null;
                }
            }

            // FIXME: PauseRelease should only be called if pause was requested.
            // It is supposed to work like reference counting, so this call could
            // undo something it wasn't meant to unpause, so the logic in Grab and
            // Ungrab should be revisited.
            MRET.LocomotionManager.PauseRelease();
            grabbing = false;
            stillTouching = false;
        }

        public void Place()
        {
            foreach (SceneObjectDeprecated interactable in GetComponentsInChildren<SceneObjectDeprecated>())
            {
                interactable.Place();
            }
        }

        public void Use(InputHand inputHand)
        {
            if (!CanPerformInteraction())
            {
                return;
            }

            if (currentInteractable != null)
            {
                currentInteractable.Use(inputHand);
            }
            else
            {
                foreach (SceneObjectDeprecated usedInteractable in FindObjectsOfType<SceneObjectDeprecated>())
                {
                    usedInteractable.Unuse(inputHand);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (grabbing)
            {
                // check if the hand has re-entered the object it is grabbing
                if(other != null && other.attachedRigidbody != null && other.attachedRigidbody.GetComponent<SceneObjectDeprecated>() == currentInteractable)
                {
                    stillTouching = true;
                }
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

            SceneObjectDeprecated touchingInteractable =
                other.attachedRigidbody.GetComponent<SceneObjectDeprecated>();
            if (touchingInteractable != null)
            {
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
                // check if the hand has exited the object it is grabbing
                if(other != null && other.attachedRigidbody != null && other.attachedRigidbody.GetComponent<SceneObjectDeprecated>() == currentInteractable)
                {
                    stillTouching = false;
                }
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

            SceneObjectDeprecated touchingInteractable =
                other.attachedRigidbody.GetComponent<SceneObjectDeprecated>();
            if (touchingInteractable == currentInteractable)
            {
                currentInteractable = null;
                if (currentTouching.Contains(touchingInteractable))
                {
                    currentTouching.Add(touchingInteractable);
                }
            }
        }

        private bool CanPerformInteraction()
        {
            return true; //!((bool) MRET.DataManager.FindPoint(DrawLineManager.ISDRAWINGFLAGKEY));
        }
    }
}