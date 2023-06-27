// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;

namespace GOV.NASA.GSFC.XR.XRUI.WorldSpaceMenu
{
    public class WorldSpaceMenuManager : MonoBehaviour
    {
        [Tooltip("Indicates whether the controller menu should be hidden when this controller is enabled.")]
        public bool HideControllerMenuOnEnable = false;

        private void OnEnable()
        {
            if (HideControllerMenuOnEnable)
            {
                DimControllerMenu();
            }
        }

        private void DimControllerMenu()
        {
            foreach (InputHand hand in MRET.MRET.InputRig.hands)
            {
                ControllerMenu.ControllerMenu men = hand.GetComponentInChildren<ControllerMenu.ControllerMenu>();
                if (men && men.IsShown())
                {
                    men.DimMenu();
                }
            }
        }

        private void UnDimControllerMenu()
        {
            foreach (InputHand hand in MRET.MRET.InputRig.hands)
            {
                ControllerMenu.ControllerMenu men = hand.GetComponentInChildren<ControllerMenu.ControllerMenu>();
                if (men)
                {
                    men.UnDimMenu();
                }
            }
        }

        public void DimMenu()
        {
            gameObject.SetActive(false);
            UnDimControllerMenu();
        }

        public void UnDimMenu()
        {
            gameObject.SetActive(true);
        }
    }
}