// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;

namespace GOV.NASA.GSFC.XR.MRET.UI.Tools.Widgets
{
    public class DropdownHelper : MonoBehaviour
    {
        public Dropdown dropdown;

        private void Start()
        {
            if (dropdown == null)
            {
                dropdown = GetComponent<Dropdown>();
            }
        }

        public void OnEnable()
        {
            if (dropdown != null)
            {
                dropdown.Hide();
                dropdown.interactable = true;
            }
        }
    }
}