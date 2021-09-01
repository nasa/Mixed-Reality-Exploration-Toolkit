/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using Assets.VDE.Communication;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.VDE.UI
{
    internal class UI
    {
        Data data;
        internal HUD.HUD hud;
        int currentMaterialIndex = 0;
        internal FramesPerSecond fps { get; private set; }
        internal string textRendererTagName { get; private set; } = "bladeText";
        internal string dataShapeTagName { get; private set; } = "nodeGroup";
        internal string nodeTagName { get; private set; } = "node";
        internal Factory nodeFactory;
        internal ConcurrentDictionary<int, Shape> shapesInFocus = new ConcurrentDictionary<int, Shape> { };
        internal float maxTimeForUpdatePerFrame = 0.023F;
        internal int timeToWaitInUpdatePerFrame = 69;

        public enum Action
        {
            notSet,
            On,
            Off,
            SetText,
            SetProgress,
            ToggleProgress,
            AddNotification,
        }
        internal UI(Data data)
        {
            this.data = data;
            hud = data.VDE.hud;
        }

        internal void SetNF(Factory nodeFactory)
        {
            this.nodeFactory = nodeFactory;
            this.nodeFactory.Init(data);
        }
        internal void SetFPS(FramesPerSecond framesPerSecond)
        {
            fps = framesPerSecond;
        }

        internal float GetConf(string variable)
        {
            if (data == null || data.config == null || data.config.UI == null)
            {
                return 0F;
            }
            if (data.config.UI.TryGetValue(variable, out string responseRaw))
            {
                if (float.TryParse(responseRaw, out float response))
                {
                    return response;
                }
            }
            throw new Message() {
                LogingEvent = Log.Event.ToServer,
                message = "unable to parse " + variable + " from UI conf"
            };
        }

        internal void SetToNextMaterial()
        {
            int setTo = 0;
            if (data.VDE.node.Length > currentMaterialIndex + 1)
            {
                setTo = currentMaterialIndex + 1;
            }
            currentMaterialIndex = setTo;
            data.VDE.StartCoroutine(data.entities.SetMaterialsTo(setTo));
            data.VDE.StartCoroutine(data.links.SetMaterialsTo(setTo));
        }
        internal void ShapeIsInView(Shape shape)
        {
            shapesInFocus.TryAdd(shape.entity.id,shape);
        }
        internal void ShapeIsNotInView(Shape shape)
        {
            shapesInFocus.TryRemove(shape.entity.id, out _);
        }
        /// <summary>
        /// Sleep for a while.
        /// </summary>
        /// <param name="maxDuration">Max length in milliseconds to sleep; final sleep length will be a random between minDuration and this number.</param>
        /// <param name="minDuration">Min length in milliseconds to sleep; final sleep length will be a random between maxDuration and this number. Default = 1.</param>
        /// <returns></returns>
        internal async Task Sleep(int maxDuration, int minDuration = 1)
        {
            await Task.Delay(data.random.Next(minDuration, maxDuration));
        }
    }
}
