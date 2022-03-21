/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using Assets.VDE.Layouts;
using System;
using System.Collections.Generic;
using Assets.VDE.UI;
using UnityEngine;

namespace Assets.VDE.Communication
{
    [Serializable]
    internal class Message : Exception
    {
        internal Link.Event LinkEvent;
        internal Log.Event LogingEvent;
        internal UI.Joint.Event JointEvent;
        internal UI.HUD.HUD.Event HUDEvent;
        internal Telemetry.Type TelemetryType;
        internal Layouts.Layouts.LayoutEvent LayoutEvent;
        internal Layouts.Layouts.EventOrigin EventOrigin;

        internal Fatals Fatal;
        internal enum Fatals
        {
            OK,
            XRdeviceNotFound,
            MissingDispatcher,
            ErrorDecodingJson,
            ErrorAddingLayersAndTags,
            ErrorLoadingConfiguration,
            ErrorLoadingValueFromConfiguration
        };

        internal int number;
        internal object[] obj;
        internal Layout layout;
        internal Entity from, to;
        internal string message;
        internal Entities.Event EntityEvent;
        internal Telemetry telemetry;
        internal List<float> floats = new List<float>() { };
        internal Color color;

        internal Message() { }
    }
}
