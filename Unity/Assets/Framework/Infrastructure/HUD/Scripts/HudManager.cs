// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Serialization;
using GSFC.ARVR.MRET.Common.Schemas;

public class HudManager : MonoBehaviour
{
    /// <summary>
    /// Stores the data from the HUDMenu and world space display lists, also keeps track of feed sources available and provides the link between the HUDMenu and the HUD.  
    /// </summary>

    public static HudManager instance;

    [Tooltip("GameObject that holds the world space HUD")]
    public GameObject frameHUD;
    [Tooltip("Controllers to keep the menu updated on either controller")]
    public GameObject leftController, rightController;
    [Tooltip("Object that holds the HUD, used by HudMenu in the UpdateHud method")]
    public GameObject HUD;
    [Tooltip("Stores HUD displays")]
    public List<DisplayType> hudDisplays = new List<DisplayType>();
    [Tooltip("Stores world displays")]
    public List<GameObject> worldDisplays = new List<GameObject>();
    [Tooltip("Prefab for the display")]
    public GameObject displayPrefab;
    [Tooltip("Array of feedsources in the scene")]
    public FeedSource[] availableSources = { null };

    private HudMenu hudMenu;

    void Start ()
    {
        instance = this;
        availableSources = GetVideoSources();
    }

    private FeedSource[] GetVideoSources()
    {
        return FindObjectsOfType<FeedSource>();
    }

    public void LoadFromXML(string filePath)
    {
        XmlSerializer ser = new XmlSerializer(typeof(HUDType));
        XmlReader reader = XmlReader.Create(filePath);
        try
        {
            hudDisplays.Clear();
            HUDType hud = (HUDType)ser.Deserialize(reader);
            Load(hud);
            reader.Close();  
        }
        catch (Exception e)
        {
            Debug.Log("[HudManger->LoadFromXML] " + e.ToString());
            reader.Close();
        }

    }

    public void Load(HUDType hud)
    {
        hudDisplays.Clear();
        Deserialize(hud);
    }

    public void SaveToXML(string filePath)
    {
        XmlSerializer ser = new XmlSerializer(typeof(HUDType));
        XmlWriter writer = XmlWriter.Create(filePath);

        hudMenu = FindObjectOfType<HudMenu>();
        hudMenu.SaveHud();

        try
        {
            ser.Serialize(writer, Serialize());
            writer.Close();
        }
        catch(Exception e)
        {
            Debug.Log("[HudManger->SaveToXML]" + e.ToString());
            writer.Close();
        }
    }

    public HUDType Save()
    {
        HudMenu hudMenu = FindObjectOfType<HudMenu>();
        if (hudMenu == null)
        {
            Debug.LogError("[HUDManager] Unable to find HUD Menu.");
            return null;
        }

        hudMenu.SaveHud();
        return Serialize();
    }

    public HUDType Serialize()
    {
        HUDType serializedHud = new HUDType();
        serializedHud.Display = hudDisplays.ToArray();

        return serializedHud;
    }

    public void Deserialize(HUDType hud)
    {
        try
        {
            foreach(DisplayType display in hud.Display)
            {
                hudDisplays.Add(display);
            }

            hudMenu = FindObjectOfType<HudMenu>();
            hudMenu.GetHud();
        }
        catch(Exception e)
        {
            Debug.Log("[HudManger->Deserialize] " + e.ToString());
        }

    }
}