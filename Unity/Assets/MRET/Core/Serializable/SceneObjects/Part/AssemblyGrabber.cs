// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System.Collections.Generic;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Part
{
    public class AssemblyGrabber : PhysicalSceneObject<PhysicalSceneObjectType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(AssemblyGrabber);

        [Tooltip("Root of assembly.")]
        public GameObject assemblyRoot;

        public List<AssemblyGrabber> otherGrabbers = new List<AssemblyGrabber>();

        /// <seealso cref="PhysicalSceneObject{T}.GetConfigurationPanelInteractable"/>
        protected override IInteractable GetConfigurationPanelInteractable()
        {
            // We don't want the configuration panel associate with this grabber. We
            // want the enclosure for the assembly.
            return assemblyRoot.GetComponentInChildren<InteractableEnclosure>();
        }

        /// <seealso cref="PhysicalSceneObject{T}.AfterBeginGrab(InputHand)"/>
        protected override void AfterBeginGrab(InputHand hand)
        {
            base.AfterBeginGrab(hand);

            // Check to see if any other grabbers are grabbing.
            foreach (AssemblyGrabber otherGrabber in otherGrabbers)
            {
                if (otherGrabber.IsGrabbing)
                {
                    EndGrab(hand);
                    return;
                }
            }

            // Override the original parent to point to our assembly
            originalParent = assemblyRoot.transform.parent;

            // Set assembly to be child of the hand while grabbing
            assemblyRoot.transform.SetParent(hand.transform);
        }

        /// <seealso cref="PhysicalSceneObject{T}.AfterEndGrab(InputHand)"/>
        protected override void AfterEndGrab(InputHand hand)
        {
            base.AfterEndGrab(hand);

            // Set parent back to original
            if (originalParent)
            {
                assemblyRoot.transform.SetParent(originalParent);
            }
        }

    }
}