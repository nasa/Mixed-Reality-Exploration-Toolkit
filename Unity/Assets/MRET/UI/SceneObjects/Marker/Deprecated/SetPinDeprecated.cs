// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_1;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Pin;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Pin
{
    /// <remarks>
    /// History:
    /// 3 October 2022: Created
    /// </remarks>
    ///
    /// <summary>
    /// SetPinDeprecated
    ///
    /// Instantiates Pin to Pin Marker when grip is pressed
    /// <Works with cref="PinMarkerDeprecated"/>
    ///
    /// Author: Sean Letavish
    /// </summary>
    /// 
    public class SetPinDeprecated : MRETUpdateBehaviour
    {
        public override string ClassName =>  nameof(SetPinDeprecated);

        public enum ActivationButton
        {
            TriggerPress,
            GripPress,
        }

        GameObject HoloMarker;
        private static bool leftControllerPressed;
        private static bool rightControllerPressed;
        public static Transform pinMarkerTransform;

        [Tooltip("Button that will activate the scaling of an object.")]
        public ActivationButton activationButton = ActivationButton.GripPress;

        public void HandleControllerPressed(InputHand hand)
        {
            if (hand.handedness == InputHand.Handedness.right)
            {
                rightControllerPressed = true;
                foreach (PinMarkerDeprecated pm in hand.GetComponentsInChildren<PinMarkerDeprecated>())
                {
                    pinMarkerTransform = pm.transform;
                }

                if (PinPanelControllerDeprecated.setPinEnabled)
                {
                    ProjectManager.PinManagerDeprecated.InstantiatePin((PartType)PinFileBrowserHelperDeprecated.selectedPinFile, null);
                }

            }

            if (hand.handedness == InputHand.Handedness.left)
            {
                leftControllerPressed = true;
                foreach (PinMarkerDeprecated pm in hand.GetComponentsInChildren<PinMarkerDeprecated>())
                {
                    pinMarkerTransform = pm.transform;
                }

                if (PinPanelControllerDeprecated.setPinEnabled)
                {
                    ProjectManager.PinManagerDeprecated.InstantiatePin((PartType)PinFileBrowserHelperDeprecated.selectedPinFile, null);
                }

            }
        }
    }
}