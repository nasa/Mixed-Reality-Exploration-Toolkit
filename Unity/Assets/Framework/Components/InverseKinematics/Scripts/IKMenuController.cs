// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;

namespace GSFC.ARVR.MRET.Components.IK
{
    public class IKMenuController : MonoBehaviour
    {
        public Dropdown ikModeDropdown;

        private bool initializingDropdown = true;

        void Start()
        {
            if (Infrastructure.Framework.MRET.IKManager.currentMode == IKManager.IKMode.Basic)
            {
                initializingDropdown = true;
                ikModeDropdown.value = 1;
            }
            else if (Infrastructure.Framework.MRET.IKManager.currentMode == IKManager.IKMode.Matlab)
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
                    Infrastructure.Framework.MRET.IKManager.SetIKMode(IKManager.IKMode.None);
                    Infrastructure.Framework.MRET.ControlMode.DisableAllControlTypes();
                    break;

                // Low-Quality.
                case 1:
                    Infrastructure.Framework.MRET.IKManager.SetIKMode(IKManager.IKMode.Basic);
                    Infrastructure.Framework.MRET.ControlMode.EnterInverseKinematicsMode();
                    break;

                // Matlab.
                case 2:
                    Infrastructure.Framework.MRET.IKManager.SetIKMode(IKManager.IKMode.Matlab);
                    Infrastructure.Framework.MRET.ControlMode.EnterInverseKinematicsMode();
                    break;

                // Unknown.
                default:
                    Infrastructure.Framework.MRET.IKManager.SetIKMode(IKManager.IKMode.None);
                    Infrastructure.Framework.MRET.ControlMode.DisableAllControlTypes();
                    break;
            }
        }
    }
}