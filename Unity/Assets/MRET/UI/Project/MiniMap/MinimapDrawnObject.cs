// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.UI.MiniMap
{
    public class MinimapDrawnObject : MonoBehaviour
    {
        public Sprite minimapSprite;

        private void OnEnable()
        {
            MinimapController.RegisterMapObject(this);
        }

        private void OnDisable()
        {
            MinimapController.DeRegisterMapObject(this);
        }
    }
}