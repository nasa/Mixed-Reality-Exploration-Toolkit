/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using UnityEngine;

namespace Assets.VDE.UI.Input
{
    internal struct Event
    {
        internal GameObject GameObject;
        internal Quaternion Quaternion;
        internal Function function;
        internal Vector3 Vector3;
        internal Vector2 Vector2;
        internal float Float;
        internal object obj;
        internal bool Bool;
        internal Type type;

        public Entity Entity { get; internal set; }

        internal enum Type
        {
            NotSet,

            GameObject,
            Quaternion,
            Vector3,
            Vector2,
            Float,
            Bool
        }
        internal enum Function
        {
            NotSet,

            Move,
            Grip,
            Select,
            PointAt,
            GazingAt,
            GazePoint,
            GazeDirection,
            
            ToggleHUD,
            SwitchSki,
            SwitchMaterial,

            UpdateShapes,
            SetDirectors,
            UpdateLinks,
            LazerState,
            Grabbing,

            Exit,
            ToggleEdges,
            DestroyTheMirror,
            ExportObjectWithCoordinates,
            ToggleNotifications,
            ToggleLabels,
            ScaleVDE,
            PositionVDE
        }
    }
}
