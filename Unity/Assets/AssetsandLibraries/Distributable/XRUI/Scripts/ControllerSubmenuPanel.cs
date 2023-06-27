// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.XRUI.ToolTip;

namespace GOV.NASA.GSFC.XR.XRUI.ControllerMenu
{
    /// <remarks>
    /// History:
    /// 17 February 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// ControllerSubmenuPanel
	///
	/// This is attached to any controller submenu panel.
	///
    /// Author: Dylan Z. Baker (MRETMan)
	/// </summary>
	/// 
	public class ControllerSubmenuPanel : MonoBehaviour
	{
        /// <summary>
        /// The close button.
        /// </summary>
        public Button closeButton;

        public void DimMenu()
        {
            ControllerMenu menu = GetComponentInParent<ControllerMenu>();
            if (menu != null)
            {
                menu.DimMenu();
            }

            ControllerMenuPanel panel = GetComponentInParent<ControllerMenuPanel>();
            if (panel != null)
            {
                foreach (MenuToolTipManager ttm in GetComponentsInChildren<MenuToolTipManager>())
                {
                    ttm.controllerMenuPanel = panel;
                }
            }
        }
	}
}