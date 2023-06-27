// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.XRUI.Widget;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Feed;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Feed
{
    // FIXME: Not implemented so likely not used.

    public class VideoSourceMenuManager : MonoBehaviour
    {
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
}