// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using TMPro;

namespace GOV.NASA.GSFC.XR.MRET.UI.Tools.Widgets
{
    public class DropdownTMPHelper : MonoBehaviour
    {
        public TMP_Dropdown dropdown;

        private void Start()
        {
            if (dropdown == null)
            {
                dropdown = GetComponent<TMP_Dropdown>();
            }
        }

        private void OnEnable()
        {
            if (dropdown != null)
            {
                dropdown.Hide();
                dropdown.interactable = true;
            }
        }
    }
}