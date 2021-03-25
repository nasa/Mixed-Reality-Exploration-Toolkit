// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudCreateMenuManager : MonoBehaviour {

    public ScrollListManager templateListDisplay;
    private HudManager hudManager;


    private int currentSelection = -1;

    void Start ()
    {
        hudManager = FindObjectOfType<HudManager>();
        templateListDisplay.SetTitle("Heads-Up Displays");
        PopulateScrollList();
    }

    public void Create()
    {

    }

    public void PopulateScrollList()
    {

    }

    private void SetActiveSelection(int listID)
    {

    }

    private void OnEnable()
    {
        PopulateScrollList();
    }




















}
