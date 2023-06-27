﻿// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GOV.NASA.GSFC.XR.XRUI.ControllerMenu;

namespace GOV.NASA.GSFC.XR.MRET.UI.HUD
{
    // FIXME: Candidate for removal. XRUI has a tooltip manager that is very similar.
    public class ToolTipManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string TooltipText;

        public Text InfoTextBox;

        private ControllerMenuPanel controllerMenuPanel;

        void Start()
        {
            if (InfoTextBox == null)
            {
                controllerMenuPanel = GetComponentInParent<ControllerMenuPanel>();
                InfoTextBox = controllerMenuPanel.tooltipText;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (TooltipText != "" && InfoTextBox)
            {
                InfoTextBox.text = TooltipText;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipText != "")
            {
                if (InfoTextBox.text == TooltipText)
                {
                    InfoTextBox.text = "";
                }
            }
        }
    }
}