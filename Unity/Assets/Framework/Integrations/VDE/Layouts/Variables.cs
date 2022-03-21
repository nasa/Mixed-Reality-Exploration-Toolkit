/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.VDE.Layouts
{
    internal class Variables
    {
        internal Dictionary<string, int> indrek = new Dictionary<string, int> { };
        internal Dictionary<string, bool> flags = new Dictionary<string, bool> { };
        internal Dictionary<string, float> floats = new Dictionary<string, float> { };
        internal Dictionary<string, Color> colours = new Dictionary<string, Color> { };
        internal Dictionary<string, Vector3> vectors = new Dictionary<string, Vector3> { };
        internal Dictionary<string, Vector3[]> moarVectors = new Dictionary<string, Vector3[]> { };
        internal Dictionary<UI.Joint.Type, Joint> joints = new Dictionary<UI.Joint.Type, Joint> { };
        internal class Joint
        {
            internal float
                damper = 0F,
                tolerance = 0.01F,
                minDistance = 0.1F,
                maxDistance = 0.1F,
                rigidBodyMass = 20F,
                rigidBodyDrag = 99F,
                springStrength = 1000,
                breakForce = Mathf.Infinity,
                breakTorque = Mathf.Infinity;

            internal bool enableCollision = false;

            internal Joint(Layout owner, UI.Joint.Type type)
            {
                damper          = owner.GetRigidJointValueFromConf(type, "JointDamper");
                rigidBodyMass   = owner.GetRigidJointValueFromConf(type, "RigidBodyMass");
                rigidBodyDrag   = owner.GetRigidJointValueFromConf(type, "RigidBodyDrag");
                tolerance       = owner.GetRigidJointValueFromConf(type, "JointTolerance");
                breakForce      = owner.GetRigidJointValueFromConf(type, "JointBreakForce");
                minDistance     = owner.GetRigidJointValueFromConf(type, "JointMinDistance");
                maxDistance     = owner.GetRigidJointValueFromConf(type, "JointMaxDistance");
                springStrength  = owner.GetRigidJointValueFromConf(type, "JointSpringStrength");
                enableCollision = owner.GetRigidJointValueFromConf(type, "ShouldCollideWithJointBody") > 0;
            }
        }
        internal Variables(Layout owner)
        {
            foreach (UI.Joint.Type jointType in Enum.GetValues(typeof(UI.Joint.Type)))
            {
                joints.Add(jointType, new Joint(owner, jointType));
            }
            colours.Add("groupColour", new Color(1, 1, 1, owner.GetValueFromConf("groupShapeAlpha")));
            colours.Add("nodeColour", new Color(1, 1, 1, owner.GetValueFromConf("nodeShapeAlpha")));

            vectors.Add("groupMargin",  Vector3.one * owner.GetValueFromConf("groupShapeMargin"));
            vectors.Add("groupPadding", Vector3.one * owner.GetValueFromConf("groupShapePadding"));
            vectors.Add("nodeMargin",   Vector3.one * owner.GetValueFromConf("nodeShapeMargin"));
            vectors.Add("nodePadding",  Vector3.one * owner.GetValueFromConf("nodeShapePadding"));
            vectors.Add("nodeSize",     Vector3.one * owner.GetValueFromConf("nodeSize"));
            vectors.Add("nodeOffset",   Vector3.one * owner.GetValueFromConf("nodeOffset"));
            vectors.Add("defaultScale", Vector3.one * owner.GetValueFromConf("defaultScale"));

            vectors.Add("dashboardCenter", new Vector3(
                owner.GetValueFromConf("dashboardCenterX"),
                owner.GetValueFromConf("dashboardCenterY"),
                owner.GetValueFromConf("dashboardCenterZ")));

            vectors.Add("notificationAreaOffset", new Vector3(
                owner.GetValueFromConf("notificationAreaOffsetX"),
                owner.GetValueFromConf("notificationAreaOffsetY"),
                owner.GetValueFromConf("notificationAreaOffsetZ")));
            vectors.Add("notificationPosition", new Vector3(
                owner.GetValueFromConf("notificationPositionX"),
                owner.GetValueFromConf("notificationPositionY"),
                owner.GetValueFromConf("notificationPositionZ")));
            vectors.Add("HUDPosition", new Vector3(
                owner.GetValueFromConf("HUDPositionX"),
                owner.GetValueFromConf("HUDPositionY"),
                owner.GetValueFromConf("HUDPositionZ")));
            vectors.Add("grabOffsetFromHand", new Vector3(
                owner.GetValueFromConf("grabOffsetFromHandX"),
                owner.GetValueFromConf("grabOffsetFromHandY"),
                owner.GetValueFromConf("grabOffsetFromHandZ")));

            floats.Add("timeToGazeFocus", owner.GetValueFromConf("timeToGazeFocus"));
            floats.Add("timeToGazeFocusOut", owner.GetValueFromConf("timeToGazeFocusOut"));
            floats.Add("notificationScale", owner.GetValueFromConf("notificationScale"));  
            floats.Add("notificationOffset", owner.GetValueFromConf("notificationOffset"));          
            floats.Add("notificationConnectorWidth", owner.GetValueFromConf("notificationConnectorWidth"));
            indrek.Add("notificationConnectorPositions", (int)owner.GetValueFromConf("notificationConnectorPositions"));
            indrek.Add("showNotificationsOnDashboard", (int)owner.GetValueFromConf("showNotificationsOnDashboard"));
            
            floats.Add("cameraColliderRadius", owner.GetValueFromConf("cameraColliderRadius"));
            floats.Add("groupPadding", owner.GetValueFromConf("groupShapePadding"));
            floats.Add("groupLabelVisibleMinDistanceFromCamera", owner.GetValueFromConf("groupLabelVisibleMinDistanceFromCamera"));
            floats.Add("groupLabelVisibleMaxDistanceFromCamera", owner.GetValueFromConf("groupLabelVisibleMaxDistanceFromCamera"));
            floats.Add("nodeLabelVisibleSinceDistanceFromCamera", owner.GetValueFromConf("nodeLabelVisibleSinceDistanceFromCamera"));
            floats.Add("maxTextSize", owner.GetValueFromConf("maxTextSize"));
            floats.Add("maxTextSizeDistance", owner.GetValueFromConf("maxTextSizeDistance"));
            floats.Add("minTextSize", owner.GetValueFromConf("minTextSize"));
            floats.Add("minTextSizeDistance", owner.GetValueFromConf("minTextSizeDistance"));

            floats.Add("dashboardSize", owner.GetValueFromConf("dashboardSize"));
            floats.Add("dashboardAngle",owner.GetValueFromConf("dashboardAngle"));
            floats.Add("shallowNodeOffset", owner.GetValueFromConf("shallowNodeOffset"));
            floats.Add("nodeOffset", owner.GetValueFromConf("nodeOffset"));

            indrek.Add("showLabelsMaxDepth", (int)owner.GetValueFromConf("showLabelsMaxDepth"));
            indrek.Add("maxNodesInShape", (int)owner.GetValueFromConf("maxNodesInShape"));
            indrek.Add("minNodesInRow", (int)owner.GetValueFromConf("minNodesInRow"));
            indrek.Add("maxEdgesInView", (int)owner.GetValueFromConf("maxEdgesInView"));            
            indrek.Add("targetFPS", ((int)owner.GetValueFromConf("targetFPS") > 0) ? (int)owner.GetValueFromConf("targetFPS") : 69);

            moarVectors.Add("dashboardCorners", new Vector3[]
            {
                vectors["dashboardCenter"] + new Vector3(
                   -floats["dashboardSize"],
                    0,
                    0),
                vectors["dashboardCenter"] + new Vector3(
                   -floats["dashboardSize"],
                    floats["dashboardSize"],
                    floats["dashboardAngle"]),
                vectors["dashboardCenter"] + new Vector3(
                    floats["dashboardSize"],
                    floats["dashboardSize"],
                    floats["dashboardAngle"]),
                vectors["dashboardCenter"] + new Vector3(
                    floats["dashboardSize"],
                    0,
                    0)
            });

            flags.Add("useJoints", owner.GetValueFromConf("useJoints") > 0);
            flags.Add("showLables", owner.GetValueFromConf("showLables") > 0);
            flags.Add("adjustToFPS", owner.GetValueFromConf("adjustToFPS") > 0);            
            flags.Add("edgeCollidersEnabled", owner.GetValueFromConf("edgeCollidersEnabled") > 0);
        }
    }
}
