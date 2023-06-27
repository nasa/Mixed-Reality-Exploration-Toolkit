// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

namespace GOV.NASA.GSFC.XR.MRET.AutoSave
{
    /// <remarks>
    /// History:
    /// 11 July 2022: Created
    /// </remarks>
    /// <summary>
    /// ExitHandler is a class that manages
    /// the shutdown process of MRET.
    /// Author: Jordan A. Ritchey
    /// </summary>
    public class ExitHandler : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(ExitHandler);

        /// <summary>
        /// Event called when "Exit" button is pressed in the ControllerMenu.
        /// </summary>
        public void ExitMRET()
        {
            // quit MRET
            MRET.Quit();
        }
    }
}