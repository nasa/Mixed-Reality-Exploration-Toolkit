/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using Assets.VDE.Communication;
using Assets.VDE.UI;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.VDE
{
    public class Data
    {
        internal UI.UI UI;
        internal Links links;
        internal Config config;
        internal readonly VDE VDE;
        internal Entities entities;
        internal Messenger messenger;
        internal Layouts.Layouts layouts;
        internal System.Random random = new System.Random();
        internal List<UnityEngine.Object> burnOnDestruction = new List<UnityEngine.Object> { };
        internal ConcurrentDictionary<string, float> stats = new ConcurrentDictionary<string, float> { };

        /// <summary>
        /// VDE threads will monitor this and kill themselves off, once mr. Forrest has stopped running around.
        /// </summary>
        internal bool forrestIsRunning = true;

        public Data(VDE VDE)
        {
            this.VDE = VDE;
            entities = new Entities(this);
            links = new Links(this);
            UI = new UI.UI(this);
            layouts = new Layouts.Layouts(this);
        }

        internal void SetLinksContainer(GameObject linksContainer)
        {
            links.SetContainer(linksContainer);
        }

        internal void SetMessenger(Messenger messenger)
        {
            this.messenger = messenger;
            this.messenger.Init(this);
            entities.SetMessenger(messenger);
            links.SetMessenger(messenger);
        }

        internal void LoadLocalConfigAndData()
        {
            Debug.Log("LoadLocalConfigAndData");
            TextAsset configRaw = (TextAsset) Resources.Load(VDE.nameOfBakedConfigResource);
            TextAsset entityRaw = (TextAsset) Resources.Load(VDE.nameOfBakedEntitiesResource);
            TextAsset linkRaw = (TextAsset) Resources.Load(VDE.nameOfBakedLinksResource);
            TextAsset positionsRaw = (TextAsset) Resources.Load(VDE.nameOfBakedPositionsResource);
            if (!(configRaw is null) && !(configRaw.text is null) && configRaw.text.Length > 0)
            {
                config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(configRaw.text);

                if (config.VDE.ContainsKey("server"))
                {
                    VDE.serverAddress = config.VDE["server"];
                }
                layouts.InitializeLayouts();
                /*
                            if (positionsRaw is null)
                            {
                                entities.Import(entityRaw.text);
                            } 
                            else
                            {
                                entities.ImportWithShapesAndPositions(entityRaw.text, positionsRaw.text);
                            }
                */
                if (!(entityRaw is null) && !(entityRaw.text is null) && entityRaw.text.Length > 0)
                {
                    entities.Import(entityRaw.text);
                }
                else
                {
                    messenger.Post(new Message()
                    {
                        HUDEvent = Assets.VDE.UI.HUD.HUD.Event.SetText,
                        message = "No entities found at: " + VDE.nameOfBakedEntitiesResource
                    });
                }

                if (!(linkRaw is null) && !(linkRaw.text is null) && linkRaw.text.Length > 0)
                {
                    links.Import(linkRaw.text);
                }
            }
            else
            {
                messenger.Post(new Message()
                {
                    HUDEvent = Assets.VDE.UI.HUD.HUD.Event.SetText,
                    message = "No configuration found at: " + VDE.nameOfBakedConfigResource
                });
            }
        }

        internal void DestroyDestroyables()
        {
            foreach (UnityEngine.Object item in burnOnDestruction)
            {
                UnityEngine.Object.Destroy(item);
            }
        }
    }
}
