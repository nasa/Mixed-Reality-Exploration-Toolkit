// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Integrations.XRUI;

namespace GSFC.ARVR.MRET.Components.Ruler
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
                Infrastructure.Framework.MRET.ControlMode.DisableAllControlTypes();
            }

            // Save to DataManager.
            DataManager.instance.SaveValue(new DataManager.DataValue(siRulerKey, false));
            DataManager.instance.SaveValue(new DataManager.DataValue(imperialUnitsKey, false));
        }

        // Exit ruler without setting the global control mode.
        public void ExitMode()
        {
            siRuler.SetActive(false);
            imperialRuler.SetActive(false);

            // Save to DataManager.
            DataManager.instance.SaveValue(new DataManager.DataValue(siRulerKey, false));
            DataManager.instance.SaveValue(new DataManager.DataValue(imperialUnitsKey, false));
        }

        public void EnableSIRuler()
        {
            if (otherRulers != null)
            {
                otherRulers.ExitMode();
            }
            siRuler.SetActive(true);
            imperialRuler.SetActive(false);
            Infrastructure.Framework.MRET.ControlMode.EnterRulerMode();

            // Save to DataManager.
            DataManager.instance.SaveValue(new DataManager.DataValue(siRulerKey, true));
            DataManager.instance.SaveValue(new DataManager.DataValue(imperialUnitsKey, false));
        }

        public void EnableImperialRuler()
        {
            if (otherRulers != null)
            {
                otherRulers.ExitMode();
            }
            siRuler.SetActive(false);
            imperialRuler.SetActive(true);
            Infrastructure.Framework.MRET.ControlMode.EnterRulerMode();

            // Save to DataManager.
            DataManager.instance.SaveValue(new DataManager.DataValue(siRulerKey, false));
            DataManager.instance.SaveValue(new DataManager.DataValue(imperialUnitsKey, true));
        }
    }
}