// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.HUD;

namespace GOV.NASA.GSFC.XR.MRET.UI.HUD
{
    public class EditSubMenu : MonoBehaviour
    {

        public HudMenu hudMenu;

        public void OnEnable()
        {
            hudMenu.miniHUD.SetActive(true);
            hudMenu.GetHud();

        }


        public void OnDisable()
        {
            //hudMenu.SaveHud();
            hudMenu.miniHUD.SetActive(false);
        }

    }
}