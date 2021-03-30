// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

namespace GSFC.ARVR.XRUI.WorldSpaceMenu
{
    public class WorldSpaceMenuManager : MonoBehaviour
    {
        public void DimMenu()
        {
            gameObject.SetActive(false);
            foreach (InputHand hand in MRET.Infrastructure.Framework.MRET.InputRig.hands)
            {
                ControllerMenu.ControllerMenu men = hand.GetComponentInChildren<ControllerMenu.ControllerMenu>();
                if (men)
                {
                    men.UnDimMenu();
                }
            }
        }

        public void UnDimMenu()
        {
            gameObject.SetActive(true);
        }
    }
}