// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.XRUI.WorldSpaceMenu;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;
using GSFC.ARVR.XRUI.ControllerMenu;
using GSFC.ARVR.MRET.Infrastructure.Framework;

public class LobbyOpenMenuManager : MonoBehaviour
{
    private void Start()
    {
        foreach (InputHand hand in MRET.InputRig.hands)
        {
            ControllerMenu menu = hand.GetComponentInChildren<ControllerMenu>();
            if (menu != null)
            {
                if (menu.menuObject.activeSelf)
                {
                    menu.DimMenu();
                }
            }    
        }
    }

    public void LoadLobby()
    {
        ModeNavigator modeNavigator = FindObjectOfType<ModeNavigator>();
        if (modeNavigator)
        {
            modeNavigator.LoadLobby();
            
        }

        WorldSpaceMenuManager worldSpaceMenuManager = GetComponentInParent<WorldSpaceMenuManager>();
        if (worldSpaceMenuManager)
        {
            worldSpaceMenuManager.DimMenu();
        }
    }
}