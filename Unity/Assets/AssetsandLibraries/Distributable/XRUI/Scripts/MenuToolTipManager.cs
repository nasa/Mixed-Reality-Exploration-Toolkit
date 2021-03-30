// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GSFC.ARVR.XRUI.ControllerMenu;

public class MenuToolTipManager : MonoBehaviour
{
    public string TooltipText;

    public Text InfoTextBox;

    public ControllerMenuPanel controllerMenuPanel;

    void Start()
    {
        if (InfoTextBox == null)
        {
            controllerMenuPanel = GetComponentInParent<ControllerMenuPanel>();
            if (controllerMenuPanel)
            {
                InfoTextBox = controllerMenuPanel.tooltipText;
            }
        }
       
    }

    public void StartedHitting()
    {
        if (controllerMenuPanel)
        {
            if (TooltipText != "" && InfoTextBox)
            {
                InfoTextBox.text = TooltipText;
            }
        }
    }

    public void StoppedHitting()
    {
        if (controllerMenuPanel)
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