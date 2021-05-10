// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.Interactable
{
    public class HandInteractor : MonoBehaviour
    {
        public InputHand hand;

        private Interactable currentInteractable;

        private bool grabbing = false;

        public void Grab()
        {
            Ungrab();

            if (!CanPerformInteraction())
            {
                return;
            }

            if (currentInteractable != null)
            {
                MRET.LocomotionManager.PauseRequest();
                currentInteractable.BeginGrab(hand);
                grabbing = true;
            }
        }

        public void Ungrab()
        {
            if (currentInteractable != null
                && currentInteractable.transform.IsChildOf(transform))
            {
                currentInteractable.EndGrab(hand);
                MRET.LocomotionManager.PauseRelease();
            }

            grabbing = false;
        }

        public void Place()
        {
            foreach (Interactable interactable in GetComponentsInChildren<Interactable>())
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

            if (other.attachedRigidbody == null)
            {
                return;
            }

            Interactable touchingInteractable =
                other.attachedRigidbody.GetComponent<Interactable>();
            if (touchingInteractable != null)
            {
                currentInteractable = touchingInteractable;
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

            Interactable touchingInteractable =
                other.attachedRigidbody.GetComponent<Interactable>();
            if (touchingInteractable == currentInteractable)
            {
                currentInteractable = null;
            }
        }

        private bool CanPerformInteraction()
        {
            return !((bool)MRET.DataManager.FindPoint(DrawLineManager.ISDRAWINGFLAGKEY));
        }
    }
}