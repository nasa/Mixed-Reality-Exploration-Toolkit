// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.Integrations.XRUI;

namespace GOV.NASA.GSFC.XR.MRET.UI.Tools.Ruler
{
    public class RulerMenuController : MenuController
    {
        public static readonly string siRulerKey = "MRET.INTERNAL.TOOLS.RULER.SI";
        public static readonly string imperialUnitsKey = "MRET.INTERNAL.TOOLS.RULER.WRONG";

        public GameObject siRuler, imperialRuler;
        public RulerMenuController otherRulers;
        public bool mostRecent = false;

        public override void Initialize()
        {
            mostRecent = true;
            if (otherRulers != null)
            {
                otherRulers.mostRecent = false;
            }
            ExitMode();
        }

        public void DisableAllRulers()
        {
            if (siRuler.activeSelf || imperialRuler.activeSelf)
            {
                siRuler.SetActive(false);
                imperialRuler.SetActive(false);
                MRET.ControlMode.DisableAllControlTypes();
            }

            // Save to DataManager.
            MRET.DataManager.SaveValue(new DataManager.DataValue(siRulerKey, false));
            MRET.DataManager.SaveValue(new DataManager.DataValue(imperialUnitsKey, false));
        }

        // Exit ruler without setting the global control mode.
        public void ExitMode()
        {
            siRuler.SetActive(false);
            imperialRuler.SetActive(false);

            // Save to DataManager.
            MRET.DataManager.SaveValue(new DataManager.DataValue(siRulerKey, false));
            MRET.DataManager.SaveValue(new DataManager.DataValue(imperialUnitsKey, false));
        }

        public void EnableSIRuler()
        {
            if (otherRulers != null)
            {
                otherRulers.ExitMode();
            }
            siRuler.SetActive(true);
            imperialRuler.SetActive(false);
            MRET.ControlMode.EnterRulerMode();

            // Save to DataManager.
            MRET.DataManager.SaveValue(new DataManager.DataValue(siRulerKey, true));
            MRET.DataManager.SaveValue(new DataManager.DataValue(imperialUnitsKey, false));
        }

        public void EnableImperialRuler()
        {
            if (otherRulers != null)
            {
                otherRulers.ExitMode();
            }
            siRuler.SetActive(false);
            imperialRuler.SetActive(true);
            MRET.ControlMode.EnterRulerMode();

            // Save to DataManager.
            MRET.DataManager.SaveValue(new DataManager.DataValue(siRulerKey, false));
            MRET.DataManager.SaveValue(new DataManager.DataValue(imperialUnitsKey, true));
        }
    }
}