// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GSFC.ARVR.MRET.Common.Schemas;
using UMP;

public class HudMenu : MonoBehaviour {

    /// <summary>
    /// Script that handles the mini HUD. Saves to the HudManager whenever navigated away from the EditSubMenu. 
    /// Loads from the HudManager whenever navigated to the EditSubMenu. Contains methods to add new displays and push them to the HUD. 
    /// </summary>

    [Tooltip("menu panel that holds mini HUD edit menu, use the edit sub menu panel of the HUDMenu")]
    public GameObject editSubMenu;
    [Tooltip("GameObject that is the parent to the mini HUD elements, located above canvases")]
    public GameObject miniHUD;
    [Tooltip("List of HUD displays")]
    public List<GameObject> localDisplayList = new List<GameObject>();
    [Tooltip("List for selecting groups of displays, not yet implemented")]
    public List<GameObject> selectedDisplays = new List<GameObject>();
    [Tooltip("Button to toggle hud on and off")]
    public Toggle hudToggle;
    [Tooltip("Adjust the scale of the HUD")]
    public Vector3 scale = new Vector3(0.86f, 0.86f, 0.86f);

    private HudManager hudManager;
    private bool updated = false;

    // Use this for initialization
    public void Start () {
        scale = new Vector3(0.86f, 0.86f, 0.86f);
        GameObject hudManagerObject = GameObject.Find("HudManager");
        hudManager = hudManagerObject.GetComponent<HudManager>();
	}

    // Gets the HUD from the hudManager
    public void GetHud()
    {
        ClearList();
        foreach (DisplayType display in hudManager.hudDisplays)
        {
            GetDisplay();
        }
    }

    // Saves the HUD to the hudManager
    public void SaveHud()
    {
        hudManager.hudDisplays.Clear();

        foreach (GameObject display in localDisplayList)
        {
            SaveDisplay(localDisplayList.IndexOf(display));
        }
    }
    
    // Clears the list of local displays
    public void ClearList()
    {
        foreach (GameObject display in localDisplayList)
        {
            Destroy(display);
        }
        localDisplayList.Clear();
    }


    public void OnDestroy()
    {
        //SaveHud();
        foreach (WorldSpaceMenuLoader menuLoader in transform.GetComponentsInChildren<WorldSpaceMenuLoader>(true))
        {
            menuLoader.DestroyMenu();
        }
    }

     // Called a bunch of times by GetHUD to get each display from the hudManager list
     public void GetDisplay()
     {
        // This function is essentially deserializing DisplayType
        localDisplayList.Add(Instantiate(hudManager.displayPrefab, miniHUD.transform)); // has index displayNum
        int displayNum = localDisplayList.Count - 1;
        DisplayType serializedDisplay = hudManager.hudDisplays[displayNum];
        GameObject display = localDisplayList[displayNum];

        float x = serializedDisplay.Transform.Position.X;
        float y = serializedDisplay.Transform.Position.Y;
        float z = serializedDisplay.Transform.Position.Z;
        display.transform.localPosition = new Vector3(x,y,z);

        x = serializedDisplay.Transform.Scale.X;
        y = serializedDisplay.Transform.Scale.Y;
        z = serializedDisplay.Transform.Scale.Z;
        display.transform.localScale = new Vector3(x, y, z);

        x = serializedDisplay.Transform.Rotation.X;
        y = serializedDisplay.Transform.Rotation.Y;
        z = serializedDisplay.Transform.Rotation.Z;
        float w = serializedDisplay.Transform.Rotation.W;
        display.transform.localRotation = new Quaternion(x, y, z, w);

        PanelSwitcher feedSwitch = display.GetComponent<PanelSwitcher>();
        DisplayController dispControl = display.GetComponent<DisplayController>();

        dispControl.type = DisplayController.Type.miniDisplay;
        dispControl.SetType();
        dispControl.displayNumber = displayNum;

        if (serializedDisplay.FeedSource != null)
        {
            feedSwitch.feedSource = new FeedSource(); // not needed?
            feedSwitch.feedSource.title = serializedDisplay.FeedSource.Title;
            //feedSwitch.feedSource.Deserialize(serializedDisplay.FeedSource);
            foreach (FeedSource feed in hudManager.availableSources)
            {
                if (feed.title.Equals(serializedDisplay.FeedSource.Title))
                {
                    feedSwitch.Initialized = true;
                    feedSwitch.feedSource = feed;
                    feedSwitch.SetPanelSource(feed);
                    break; 
                }
            }
        }
    }

    // Called a bunch of times by SaveHud to save each display to the hudManager list
    void SaveDisplay(int displayNum)
    {
        // This function is essentially serializing DisplayType

        DisplayType serializedDisplay = new DisplayType();
        GameObject display = localDisplayList[displayNum];

        serializedDisplay.Transform = new UnityTransformType();


        serializedDisplay.Transform.Position = new Vector3Type
        {
            X = display.transform.localPosition.x,
            Y = display.transform.localPosition.y,
            Z = display.transform.localPosition.z
        };

        serializedDisplay.Transform.Rotation = new QuaternionType
        {
            W = display.transform.localRotation.w,
            X = display.transform.localRotation.x,
            Y = display.transform.localRotation.y,
            Z = display.transform.localRotation.z
        };

        serializedDisplay.Transform.Scale = new Vector3Type
        {
            X = display.transform.localScale.x,
            Y = display.transform.localScale.y,
            Z = display.transform.localScale.z
        };

        PanelSwitcher feedSwitch = display.GetComponent<PanelSwitcher>();
        DisplayController dispControl = display.GetComponent<DisplayController>();

        serializedDisplay.Type = DisplayTypeType.Mini;
        serializedDisplay.DisplayNumber = displayNum;

        if(feedSwitch.feedSource != null)
        {
            serializedDisplay.FeedSource = new VideoSourceType();
            serializedDisplay.FeedSource.Title = feedSwitch.feedSource.title;
#if !HOLOLENS_BUILD
            serializedDisplay.FeedSource.Time = display.GetComponent<UniversalMediaPlayer>().Time;
            feedSwitch.feedSource.time = display.GetComponent<UniversalMediaPlayer>().Time;
#endif
            //serializedDisplay.FeedSource = feedSwitch.feedSource.Serialize();
        }
        hudManager.hudDisplays.Add(serializedDisplay);
    }


    public void HideMenu()
    {
        gameObject.SetActive(false);
    }

    // Pushes the HUD from the mini HUD to the HUD
    public void UpdateHUD() 
    {
        //Destroy(hudManager.HUD);
        hudManager.HUD = Instantiate(miniHUD, hudManager.frameHUD.transform);
        hudManager.HUD.transform.localPosition = Vector3.zero;
        hudManager.HUD.transform.localRotation = Quaternion.identity;
        hudManager.HUD.transform.localScale = scale;
        Destroy(hudManager.HUD.GetComponent<Image>());

        DisplayController[] hudList = hudManager.HUD.GetComponentsInChildren<DisplayController>();

        foreach (DisplayController display in hudList)
        {
            display.type = DisplayController.Type.HUDDisplay;
            display.SetType(); // SetType only affects interaction and settings menus, not whats being displayed
            display.gameObject.GetComponent<PanelSwitcher>().Initialized = true;
            display.gameObject.GetComponent<PanelSwitcher>().SetPanelSource(display.gameObject.GetComponent<PanelSwitcher>().feedSource);
        }

        updated = true;

    }

    // Add a display to the mini HUD
    public void Add()
    {
        localDisplayList.Add(Instantiate(hudManager.displayPrefab, miniHUD.transform));
        int displayNum = localDisplayList.Count - 1;
        localDisplayList[displayNum].transform.localRotation = Quaternion.identity;
        localDisplayList[displayNum].transform.localPosition = new Vector3(.05f * displayNum, 0, 0);
        
        localDisplayList[displayNum].transform.localScale = new Vector3(0.1f, 0.1f, 1);

        DisplayController Display = localDisplayList[displayNum].GetComponent<DisplayController>();
        Display.displayNumber = displayNum;
        Display.type = DisplayController.Type.miniDisplay;
        Display.SetType();

    }

    public void Delete()
    {
        foreach (var deleteDisplay in selectedDisplays)
        {
            hudManager.hudDisplays.RemoveAt(selectedDisplays.IndexOf(deleteDisplay));
        }
    }

    public void ToggleHud()
    {
        SwitchHUD(hudToggle.isOn);
    }

    public void SwitchHUD(bool on)
    {
        hudToggle.isOn = on;
        if (on)
        {
            UpdateHUD();
        }
        else
        {
            Destroy(hudManager.HUD);
        }
    }

    public void ThresholdSlide(Slider slider)
    {
        hudManager.frameHUD.GetComponentInParent<FrameHUD>().threshold = slider.value;
    }

    public void SpeedSlide(Slider slider)
    {
        hudManager.frameHUD.GetComponentInParent<FrameHUD>().speed = slider.value;
    }

    public void Select()
    {


    }

    // Feature to allow the user to undo/redo changes made to the HUD.
    public void Undo()
    {

    }

    public void Redo()
    {

    }






}
