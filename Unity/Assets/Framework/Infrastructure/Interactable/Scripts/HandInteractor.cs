// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.Interactable
{
    public class HandInteractor : MonoBehaviour
    {
        public InputHand hand;

        public Interactable[] touchedObjects
        {
            get
            {
                return currentTouching.ToArray();
            }
        }

        private List<Interactable> currentTouching = new List<Interactable>();

        private Interactable currentInteractable;

        private bool grabbing = false;

        private bool stillTouching = false; // if hand is still touching the currentInteractable while grabbing

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

            MRET.LocomotionManager.PauseRelease();
            grabbing = false;
            stillTouching = false;
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
            else
            {
                foreach (Interactable usedInteractable in FindObjectsOfType<Interactable>())
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
                if(other != null && other.attachedRigidbody != null && other.attachedRigidbody.GetComponent<Interactable>() == currentInteractable)
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

            Interactable touchingInteractable =
                other.attachedRigidbody.GetComponent<Interactable>();
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
                if(other != null && other.attachedRigidbody != null && other.attachedRigidbody.GetComponent<Interactable>() == currentInteractable)
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

            Interactable touchingInteractable =
                other.attachedRigidbody.GetComponent<Interactable>();
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