// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Serialization;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Feed;
using GOV.NASA.GSFC.XR.MRET.UI.HUD;

namespace GOV.NASA.GSFC.XR.MRET.HUD
{
    public class HudManager : MRETManager<HudManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(HudManager);

        /// <summary>
        /// Stores the data from the HUDMenu and world space display lists, also keeps track of feed sources available and provides the link between the HUDMenu and the HUD.  
        /// </summary>

        [Tooltip("GameObject that holds the world space HUD")]
        public GameObject frameHUD;
        [Tooltip("Controllers to keep the menu updated on either controller")]
        public GameObject leftController, rightController;
        [Tooltip("Object that holds the HUD, used by HudMenu in the UpdateHud method")]
        public GameObject HUD;
        [Tooltip("Stores HUD panels")]
        public List<PanelType> hudPanels = new List<PanelType>();
        [Tooltip("Stores world displays")]
        public List<GameObject> worldDisplays = new List<GameObject>();
        [Tooltip("Prefab for the display")]
        public GameObject displayPrefab;
        [Tooltip("Array of feedsources in the scene")]
        public FeedSource[] availableSources = { null };

        private HudMenu hudMenu;

        [HideInInspector]
        public GameObject miniHUD;

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            availableSources = GetVideoSources();
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            base.MRETOnDestroy();

            foreach (GameObject worldDisplay in worldDisplays)
            {
                Destroy(worldDisplay);
            }
        }

        private FeedSource[] GetVideoSources()
        {
            return MRET.UuidRegistry.RegisteredTypes<FeedSource>();
        }

        public void LoadFromXML(string filePath)
        {
            XmlSerializer ser = new XmlSerializer(typeof(HUDType));
            XmlReader reader = XmlReader.Create(filePath);
            try
            {
                hudPanels.Clear();
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
            hudPanels.Clear();
            Deserialize(hud);

            availableSources = GetComponentsInChildren<FeedSource>();
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
            catch (Exception e)
            {
                Debug.Log("[HudManger->SaveToXML]" + e.ToString());
                writer.Close();
            }
        }

        public HUDType Save()
        {
            HudMenu hudMenu = GetComponentInChildren<HudMenu>(true);


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
            serializedHud.Panel = hudPanels.ToArray();

            return serializedHud;
        }

        public void Deserialize(HUDType hud)
        {
            try
            {
                foreach (PanelType display in hud.Panel)
                {
                    hudPanels.Add(display);
                }

                hudMenu = FindObjectOfType<HudMenu>(true);
                hudMenu.GetHud();
            }
            catch (Exception e)
            {
                Debug.Log("[HudManger->Deserialize] " + e.ToString());
            }

        }

        public FeedSource FindFeed(string feedId)
        {
            FeedSource result = null;

            foreach (FeedSource feed in availableSources)
            {
                if (feed.description.Equals(feedId))
                {
                    result = feed;
                    break;
                }
            }

            return result;
        }

        public void CleanupScreenPanels()
        {
            PanelSwitcher[] screenPanels = FindObjectsOfType<PanelSwitcher>();
            foreach (PanelSwitcher screenPanel in screenPanels)
            {
                screenPanel.SetPanelSource(null);
            }
        }

    }

}