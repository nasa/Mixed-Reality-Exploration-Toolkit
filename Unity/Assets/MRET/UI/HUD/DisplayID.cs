// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.HUD;

namespace GOV.NASA.GSFC.XR.MRET.UI.HUD
{
    public class DisplayID : MonoBehaviour
    {
        public int DisplayIndex;

        public void OpenDisplay()
        {
            Text textBox = gameObject.GetComponentInChildren<Text>();
            Int32 index = Int32.Parse(textBox.text);

            GameObject Manager = GameObject.Find("HudManager");
            List<GameObject> Display = Manager.GetComponent<HudManager>().worldDisplays;
            Display[index - 1].SetActive(true);
        }
    }
}