// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoSourceMenuManager : MonoBehaviour {

    public ScrollListManager videoListDisplay;
    public FeedSource Source;
    private List<FeedSource> videoStreams;
    private int currentSelection = -1;

    void Start()
    {
        Source = FindObjectOfType<FeedSource>();

        videoListDisplay.SetTitle("Source");
        PopulateScrollList();
    }

    public void Open()
    {



    }

    private void PopulateScrollList()
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
