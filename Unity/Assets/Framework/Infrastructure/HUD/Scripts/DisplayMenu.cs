// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GSFC.ARVR.MRET.Infrastructure.Framework;

public class DisplayMenu : MonoBehaviour {

    public ScrollListManager objectScrollList;
    public GameObject menuObject;
    public GameObject newDisplayButton;
    public GameObject contentHolder;
    public GameObject displayPrefab;


    public Transform trackHeadset;

    public List<Button> displayButtonList = new List<Button>();
    private int DisplayCount = -1;


    private int currentSelection = -1;


    public float InitialPosY = 192;
    public float DistanceY = 140;

    public float InitialPosX = -120;
    public float DistanceX = 120;

    private HudManager hudManager;

    // Use this for initialization

    void Start () {

        objectScrollList.SetTitle("Create a Display");

        GameObject hudManagerObject = GameObject.Find("HudManager");
        hudManager = hudManagerObject.GetComponent<HudManager>();
        trackHeadset = MRET.InputRig.head.transform;


        foreach (GameObject display in hudManager.worldDisplays)
        {
            DisplayCount++;
            objectScrollList.AddScrollListItem((DisplayCount + 1).ToString());
            Button NewButton = objectScrollList.GetScrollListButton(DisplayCount);
            displayButtonList.Add(NewButton);

        }

        OrganizeMenu();

        // Set position
    }


    public void NewDisplay()
    {
        // Create new UI element
        DisplayCount++;
        objectScrollList.AddScrollListItem((DisplayCount + 1).ToString());

        Button NewButton = objectScrollList.GetScrollListButton(DisplayCount);
        displayButtonList.Add(NewButton);

        //create new display object in scene
        GameObject NewDisplay = Instantiate(displayPrefab);

        NewDisplay.transform.SetParent(trackHeadset);
        NewDisplay.transform.localRotation = Quaternion.identity;
        NewDisplay.transform.localPosition = Vector3.zero;
        NewDisplay.transform.Translate(Vector3.forward);
        
        NewDisplay.transform.SetParent(null);

        hudManager.worldDisplays.Add(NewDisplay);
        Text textObject = hudManager.worldDisplays[DisplayCount].GetComponentInChildren<Text>();
        textObject.text = (DisplayCount + 1).ToString();

        OrganizeMenu();
    }

    public void SelectObject()
    {
        if (currentSelection != -1)
        {

        }
    }

    private void OrganizeMenu()
    {
        float posX = InitialPosX;
        float posY = InitialPosY;

        float AddPosX = InitialPosX;
        float AddPosY = InitialPosY - DistanceY;

        RectTransform DisplayTransform;
        RectTransform AddDisplayTransform = newDisplayButton.GetComponent<RectTransform>();

        for (int i = 0; i < displayButtonList.Count; i++)
        {
            displayButtonList[i].transform.SetParent(contentHolder.transform);
            displayButtonList[i].transform.localScale = Vector3.one;
            displayButtonList[i].transform.localRotation = Quaternion.identity;

            if (i % 3 == 0) // sort to first column
            {
                posX = InitialPosX;
                posY -= DistanceY;

                AddPosX += DistanceX;
            }

            else if (i % 3 == 1) // sort to second column
            {
                posX += DistanceX;

                AddPosX += DistanceX;
            }

            else if (i % 3 == 2) // sort to third column
            {
                posX += DistanceX;

                AddPosX = InitialPosX;
                AddPosY -= DistanceY;
            }

            DisplayTransform = displayButtonList[i].GetComponent<RectTransform>();
            DisplayTransform.anchoredPosition = new Vector2(posX, posY);

            AddDisplayTransform.anchoredPosition = new Vector2(AddPosX, AddPosY);
        }
    }

    private void SetActiveSelection(int listID)
    {
        currentSelection = listID;
        objectScrollList.HighlightItem(listID);
    }

    private bool menuDimmed = true;

    public void ToggleMenu()
    {
        if (menuDimmed)
        {
            UnDimMenu();
        }
        else
        {
            DimMenu();
        }
    }

    public void DimMenu()
    {
        menuObject.SetActive(false);
        menuDimmed = true;
    }

    public void UnDimMenu()
    {
        menuObject.SetActive(true);
        menuDimmed = false;
    }

    public bool IsDimmed()
    {
        return menuDimmed;
    }

    public void DeleteDisplay(int DisplayIndex)
    {
        displayButtonList.RemoveAt(DisplayIndex);
        hudManager.worldDisplays.RemoveAt(DisplayIndex);
        // Needs to be called here to delete both button and display
    }




}
