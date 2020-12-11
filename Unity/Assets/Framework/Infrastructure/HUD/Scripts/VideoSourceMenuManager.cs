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
