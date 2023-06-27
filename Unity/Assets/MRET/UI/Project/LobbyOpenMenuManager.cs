// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.XRUI.WorldSpaceMenu;

namespace GOV.NASA.GSFC.XR.MRET.UI.Project
{
    public class LobbyOpenMenuManager : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(LobbyOpenMenuManager);
            }
        }

        public void LoadLobby()
        {
            if (MRET.ModeNavigator)
            {
                MRET.ModeNavigator.LoadLobby();
            }

            WorldSpaceMenuManager worldSpaceMenuManager = GetComponentInParent<WorldSpaceMenuManager>();
            if (worldSpaceMenuManager)
            {
                worldSpaceMenuManager.DimMenu();
            }
        }
    }
}