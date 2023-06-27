// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.UI.HUD
{
    public class HudMenuManager : MonoBehaviour
    {
        public void DimMenu()
        {
            HudMenu hudMenu = MRET.HudManager.GetComponentInChildren<HudMenu>(true);
            if (hudMenu != null)
            {
                hudMenu.gameObject.SetActive(true);
                gameObject.SetActive(false);
            }
        }

        public void UnDimMenu()
        {
            gameObject.SetActive(true);
        }
    }
}