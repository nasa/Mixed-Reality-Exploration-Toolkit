// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;

public class DropdownHelper : MonoBehaviour
{
    public Dropdown dropdown;

    public void OnEnable()
    {
        dropdown.Hide();
        dropdown.interactable = true;
    }
}