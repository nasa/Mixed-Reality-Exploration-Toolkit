// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.Integrations.XRUI;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Feed
{
    public class FeedSourceMenuController : MenuController
    {
        public static readonly string leftScreen0Key = "MRET.INTERNAL.TOOLS.FEEDPANEL.LEFT.0";
        public static readonly string leftScreen1Key = "MRET.INTERNAL.TOOLS.FEEDPANEL.LEFT.1";
        public static readonly string rightScreen0Key = "MRET.INTERNAL.TOOLS.FEEDPANEL.RIGHT.0";
        public static readonly string rightScreen1Key = "MRET.INTERNAL.TOOLS.FEEDPANEL.RIGHT.1";

        public GameObject leftScreen, rightScreen;
        public InputHand hand;

        public override void Initialize()
        {
            MRET.DataManager.SaveValue(leftScreen0Key, false);
            MRET.DataManager.SaveValue(leftScreen1Key, false);
            MRET.DataManager.SaveValue(rightScreen0Key, false);
            MRET.DataManager.SaveValue(rightScreen1Key, false);
        }

        public void SwitchLeftScreen(bool on)
        {
            if (on)
            {
                leftScreen.SetActive(true);
            }
            else
            {
                leftScreen.SetActive(false);
            }

            // Save to DataManager.
            if (hand.handedness == InputHand.Handedness.left || hand.handedness == InputHand.Handedness.neutral)
            {
                MRET.DataManager.SaveValue(new DataManager.DataValue(leftScreen0Key, on));
            }
            else if (hand.handedness == InputHand.Handedness.right)
            {
                MRET.DataManager.SaveValue(new DataManager.DataValue(leftScreen1Key, on));
            }
        }

        public void SwitchRightScreen(bool on)
        {
            if (on)
            {
                rightScreen.SetActive(true);
            }
            else
            {
                rightScreen.SetActive(false);
            }

            // Save to DataManager.
            if (hand.handedness == InputHand.Handedness.left || hand.handedness == InputHand.Handedness.neutral)
            {
                MRET.DataManager.SaveValue(new DataManager.DataValue(rightScreen0Key, on));
            }
            else if (hand.handedness == InputHand.Handedness.right)
            {
                MRET.DataManager.SaveValue(new DataManager.DataValue(rightScreen1Key, on));
            }
        }
    }
}