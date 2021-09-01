// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using UMP;

public class PanelSwitcher : MonoBehaviour
{
    /// <summary>
    ///  Handles the feed being displayed 
    /// </summary>


    [Tooltip("GameObject that holds the canvases")]
    public GameObject canvasObject;
    [Tooltip("Image that holds the content that is being displayed")]
    public RawImage displayImage;
    public Texture defaultTexture;
    public Text label;
    public Text displayText;
    public Dropdown sourceDropdown;
    public Dropdown miniSourceDropdown;
#if !HOLOLENS_BUILD
    public ZenFulcrum.EmbeddedBrowser.Browser htmlBrowser;
#endif
    public DisplayController dispControl;
    public FeedSource feedSource;
    public bool Initialized = false;
    public int value;
    public RenderTexture renderTexture;

#if !HOLOLENS_BUILD
    private UniversalMediaPlayer UMP;
#endif
    public FeedSource[] availableSources = { null };
    private float actionTime = 0.0f;
    private float period = 0.25f;

    private string lastPathWritten = "";
    private int lastPathWrittenIndex = 0;
    public bool capturingLeft = false, capturingRight = false;
    public int val;
    // public GameObject cameraFlash;

    //public string pick;

    void Start()
    {
        // cameraFlash.SetActive(false);
#if !HOLOLENS_BUILD
        UMP = gameObject.GetComponent<UniversalMediaPlayer>();
#endif
        if (dispControl.type == DisplayController.Type.miniDisplay)
        {
            sourceDropdown = miniSourceDropdown;
        }

        if (!Initialized)
        {
            SetPanelSource(null);
        }
        SetSourceDropdownOptions();
        
    }

    void OnEnable()
    {
        SetSourceDropdownOptions();
    }

    void Update()
    {
        if (feedSource != null)
        {
            bool upToDate = feedSource.upToDate;
            if (!upToDate)
            {
                switch (feedSource.type)
                {
                    case FeedSource.Type.virtualFeed:

                        break;

                    case FeedSource.Type.externalFeed:
#if !HOLOLENS_BUILD
                        //this may slow things down A LOT
                        if(UMP.IsReady && !UMP.IsPlaying)
                        {
                            UMP.Play();
                        }
                        if (UMP.IsPlaying) // may be dangerous if multiple things access
                        {
                            feedSource.time = UMP.Time;
                        }
#endif

                        break;

                    case FeedSource.Type.dataFeed:

                        break;

                    case FeedSource.Type.htmlFeed:

#if !HOLOLENS_BUILD
                        if (htmlBrowser.Url != null)
                        {
                            if (feedSource.sourceLink != htmlBrowser.Url)
                            {
                                feedSource.sourceLink = htmlBrowser.Url;
                            }
                        }
#endif
                        //else
                        // {
                        //   htmlBrowser.Url = feedSource.sourceLink;
                        //   htmlBrowser.LoadURL(htmlBrowser.Url, true);
                        //}
                        break;

                    case FeedSource.Type.spriteFeed:

                        switch (feedSource.spriteType)
                        {
                            case FeedSource.SpriteType.toggle:
                                displayImage.color = feedSource.sprite.color;
                                upToDate = true;
                                break;

                            case FeedSource.SpriteType.number:
                                
                                if (Time.time > actionTime)
                                {
                                    displayText.text = feedSource.value.text;
                                    actionTime += period;
                                }
                                
                                upToDate = true;
                                break;

                        }
                        break;

                    default:

                        break;
                }
            }
        }
      
    }

    public void SelectPanelSource()
    {

        if (sourceDropdown.value == 0)
        {
            SetPanelSource(null); // end click
        }
        else
        {
            renderTexture = availableSources[sourceDropdown.value - 1]._renderTexture;
            Debug.Log("you pressed " + availableSources[sourceDropdown.value - 1].title);
            SetPanelSource(availableSources[sourceDropdown.value - 1]);

        }
    }

    public void SetPanelSource(FeedSource source)
    {
#if !HOLOLENS_BUILD
        UMP = gameObject.GetComponent<UniversalMediaPlayer>();
#endif
        feedSource = source;
        Initialized = true; 

        if (source == null)
        {
#if !HOLOLENS_BUILD
            htmlBrowser.gameObject.SetActive(false);
#endif
            canvasObject.SetActive(true);
            label.text = "Feed Disconnected";
            displayImage.texture = defaultTexture;
        }
        else
        {

            label.text = source.title;
#if !HOLOLENS_BUILD
            if (UMP.IsPlaying)
            {
                UMP.Stop();
            }
#endif

            switch (source.type)
            {
                case FeedSource.Type.virtualFeed:
                    if (dispControl.size) dispControl.size.interactable = true;
#if !HOLOLENS_BUILD
                    htmlBrowser.gameObject.SetActive(false);
#endif
                    canvasObject.SetActive(true);
                    displayImage.texture = source.renderTexture;
#if !HOLOLENS_BUILD
                    htmlBrowser.EnableInput = false;
#endif
#if !HOLOLENS_BUILD
                    UMP.Path = null;
                    UMP.AutoPlay = false;
#endif
                    break;

                case FeedSource.Type.externalFeed:
                    if (dispControl.size)  dispControl.size.interactable = true;
#if !HOLOLENS_BUILD
                    htmlBrowser.gameObject.SetActive(false);
#endif
                    canvasObject.SetActive(true);
#if !HOLOLENS_BUILD
                    htmlBrowser.EnableInput = false;
#endif
#if !HOLOLENS_BUILD
                    UMP.Path = source.sourceLink;
                    UMP.Prepare();     
                    UMP.OnPathPrepared(source.sourceLink, true);
                    UMP.Play();
                    UMP.Time = source.time;
#endif
                    break;

                case FeedSource.Type.dataFeed:
                    if (dispControl.size)  dispControl.size.interactable = true;
#if !HOLOLENS_BUILD
                    htmlBrowser.gameObject.SetActive(false);
#endif
                    canvasObject.SetActive(true);

#if !HOLOLENS_BUILD
                    htmlBrowser.EnableInput = false;
#endif
#if !HOLOLENS_BUILD
                    UMP.Path = null;
                    UMP.AutoPlay = false;
#endif

                    break;

                case FeedSource.Type.htmlFeed:
                    if (dispControl.size)  dispControl.size.interactable = false;
#if !HOLOLENS_BUILD
                    htmlBrowser.gameObject.SetActive(true);
#endif
                    canvasObject.SetActive(false);
#if !HOLOLENS_BUILD
                    htmlBrowser.Url = source.sourceLink; // for some reason taking a while to pass url, trying this
                    htmlBrowser.LoadURL(source.sourceLink, true);
                    htmlBrowser.EnableInput = true;
#endif
#if !HOLOLENS_BUILD
                    UMP.Path = null;
                    UMP.AutoPlay = false;
#endif
                    break;

                case FeedSource.Type.spriteFeed:
                    if (dispControl.size)  dispControl.size.interactable = true;
#if !HOLOLENS_BUILD
                    htmlBrowser.gameObject.SetActive(false);
#endif
                    canvasObject.SetActive(true);
#if !HOLOLENS_BUILD
                    htmlBrowser.EnableInput = false;
#endif
#if !HOLOLENS_BUILD
                    UMP.Path = null;
                    UMP.AutoPlay = false;
#endif
                    switch (source.spriteType)
                    {
                        case FeedSource.SpriteType.toggle:
                            displayImage.gameObject.SetActive(true);
                            displayText.gameObject.SetActive(false);
                            displayImage.texture = source.sprite.mainTexture;
                            displayImage.color = source.sprite.color;
                            break;
                        case FeedSource.SpriteType.number:
                            displayImage.gameObject.SetActive(false);
                            displayText.gameObject.SetActive(true);
                            displayText.text = feedSource.value.text;
                            break;
                    }
                    break;

                default:
                    if (dispControl.size)  dispControl.size.interactable = true;
#if !HOLOLENS_BUILD
                    htmlBrowser.gameObject.SetActive(false);
#endif
                    canvasObject.SetActive(true);
#if !HOLOLENS_BUILD
                    htmlBrowser.EnableInput = false;
#endif
                    Debug.LogWarning("[PanelSwitcher->SetPanelSource] Unable to decipher video source type.");

                    break;
            }
            if (dispControl.feedSettings) dispControl.feedSettings.SetActive(false);
            if (dispControl.miniFeedSettings) dispControl.miniFeedSettings.SetActive(false);
            if (dispControl.feedSettingsDimmed) dispControl.feedSettingsDimmed = true;
        }

        // Update options for next time.
        SetSourceDropdownOptions();
    }


    public void SetSourceDropdownOptions()
    {
        List<string> dropdownLabels = new List<string>();
        dropdownLabels.Add("None");

        FeedSource[] sources = GetVideoSources();
        foreach (FeedSource source in sources)
        {
            dropdownLabels.Add(source.title);
        }

        sourceDropdown.ClearOptions();
        sourceDropdown.AddOptions(dropdownLabels);
        availableSources = sources;
    }

    private FeedSource[] GetVideoSources()
    {
        return FindObjectsOfType<FeedSource>();
    }
    private int resizeScalex = 2;
    private int resizeScaley = 2;

    public void Screenshot()
    {
        Debug.Log("you pressed the button");

        System.DateTime now = System.DateTime.Now;
        string imgPath = Application.dataPath + "/Captures/";

        if (!System.IO.Directory.Exists(imgPath))
        {
            System.IO.Directory.CreateDirectory(imgPath);
        }
        Texture2D tex = new Texture2D(renderTexture.width * resizeScalex, renderTexture.height * resizeScaley, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture;

        for (int x = 0; x < renderTexture.width; x++)
        {
            for (int y = 0; y < renderTexture.height; y++)
            {
                for (int i = 0; i < resizeScalex; i++)
                {
                    for (int j = 0; j < resizeScaley; j++)
                    {
                        tex.ReadPixels(new Rect(x, y, 1, 1), x * resizeScalex + i, y * resizeScaley + j);
                    }
                }
            }
        }
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();

        string pathToWrite = imgPath + "image" + now.Year + now.DayOfYear + now.Hour + now.Minute + now.Second;
        if (pathToWrite == lastPathWritten)
        {
            pathToWrite = pathToWrite + "-" + lastPathWrittenIndex++;
        }
        else
        {
            lastPathWrittenIndex = 0;
            lastPathWritten = pathToWrite;
        }
        System.IO.File.WriteAllBytes(pathToWrite + ".png", bytes);
        //StartCoroutine(FlashCamera());

    }
    /* private void StartCoroutine(IEnumerable enumerable)
    {
        throw new NotImplementedException();
    }

    private IEnumerable FlashCamera()
    {
        cameraFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        cameraFlash.SetActive(false);
    }*/

}
