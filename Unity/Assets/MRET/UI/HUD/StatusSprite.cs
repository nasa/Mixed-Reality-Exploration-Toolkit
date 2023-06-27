// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GOV.NASA.GSFC.XR.MRET.UI.HUD
{
    public class StatusSprite : MonoBehaviour
    {
        public Image sprite;

        public Color standby;
        public Color alert;

        public bool isAlert = false;
        private bool isChanged;
        //public even

        // Use this for initialization
        void Start()
        {
            StandbyMode();
            isChanged = isAlert;
        }

        void Update()
        {
            if (isChanged != isAlert)
            {
                if (isAlert)
                {
                    AlertMode();
                }
                else if (!isAlert)
                {
                    StandbyMode();
                }

                isChanged = isAlert;
            }

        }

        public void AlertMode()
        {
            sprite.color = alert;
        }

        public void StandbyMode()
        {
            sprite.color = standby;
        }

    }
}