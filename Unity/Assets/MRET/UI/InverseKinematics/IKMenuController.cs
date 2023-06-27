// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.IK;

namespace GOV.NASA.GSFC.XR.MRET.UI.IK
{
    public class IKMenuController : MonoBehaviour
    {
        public Dropdown ikModeDropdown;

        private bool initializingDropdown = true;

        void Start()
        {
            if (MRET.IKManager.currentMode == IKManager.IKMode.Basic)
            {
                initializingDropdown = true;
                ikModeDropdown.value = 1;
            }
            else if (MRET.IKManager.currentMode == IKManager.IKMode.Matlab)
            {
                initializingDropdown = true;
                ikModeDropdown.value = 2;
            }
            else
            {
                initializingDropdown = true;
                ikModeDropdown.value = 0;
            }

            // Handle IK Mode Updates.
            ikModeDropdown.onValueChanged.AddListener(delegate
            {
                if (!initializingDropdown)
                {
                    HandleIKModeChange();
                }
                initializingDropdown = false;
            });
        }

        public void HandleIKModeChange()
        {
            switch (ikModeDropdown.value)
            {
                // Off.
                case 0:
                    MRET.IKManager.SetIKMode(IKManager.IKMode.None);
                    MRET.ControlMode.DisableAllControlTypes();
                    break;

                // Low-Quality.
                case 1:
                    MRET.IKManager.SetIKMode(IKManager.IKMode.Basic);
                    MRET.ControlMode.EnterInverseKinematicsMode();
                    break;

                // Matlab.
                case 2:
                    MRET.IKManager.SetIKMode(IKManager.IKMode.Matlab);
                    MRET.ControlMode.EnterInverseKinematicsMode();
                    break;

                // Unknown.
                default:
                    MRET.IKManager.SetIKMode(IKManager.IKMode.None);
                    MRET.ControlMode.DisableAllControlTypes();
                    break;
            }
        }
    }
}