// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.GMSEC;
using GOV.NASA.GSFC.XR.MRET.Collaboration;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.GMSEC.Collaboration
{
    [System.Obsolete("Refer to " + nameof(GOV.NASA.GSFC.XR.MRET.Collaboration.CollaborationManager))]
    public class SessionMasterNode : MonoBehaviour
    {
        public enum ConnectionTypes { bolt, mb, amq383, amq384, ws71, ws75, ws80 };

        [Tooltip("The middleware type to use.")]
        public ConnectionTypes connectionType = ConnectionTypes.bolt;

        [Tooltip("The server address and port number.")]
        public string server = "localhost:9100";

        [Tooltip("The mission name to use.")]
        public string missionName = "UNSET";

        [Tooltip("The satellite name to use.")]
        public string satName = "UNSET";

        [Tooltip("The synchronization manager.")]
        public SynchronizationManagerDeprecated synchronizationManager;

        public GameObject headsetFollower;

        public GameObject defaultAvatarPrefab, userContainer;

        public List<SynchronizedUserDeprecated> synchronizedUsers = new List<SynchronizedUserDeprecated>();

        // A MonoGMSEC object is needed to interface with GMSEC.
        private MonoGMSEC gmsec;

        private bool subscribed = false;

        private SessionInformation currentSessionInfo;

        void Start()
        {
            gmsec = gameObject.AddComponent<MonoGMSEC>();
        }

        void Update()
        {
            if (subscribed)
            {
                ReceiveMessage();
            }
        }

        public void StartRunning(SessionInformation sessionInfo, string alias)
        {
            if (gmsec == null)
            {
                gmsec = gameObject.AddComponent<MonoGMSEC>();
            }

            Debug.Log("[SessionMasterNode] Initializing Node");
            gmsec.Initialize();

            gmsec.CreateConfig();
            gmsec.AddToConfig("connectionType", ConnectionTypeToString(connectionType));
            gmsec.AddToConfig("server", server);
            gmsec.AddToConfig("GMSEC-REQ-RESP", "OPEN-RESP");   // Enable GMSEC open response feature.

            // Connect to GMSEC Bus.
            gmsec.Connect();

            // Set up the synchronization manager.
            synchronizationManager.server = server;
            synchronizationManager.missionName = missionName;
            synchronizationManager.satName = satName;
            synchronizationManager.groupID = sessionInfo.groupName;
            synchronizationManager.projectName = sessionInfo.projectName;
            synchronizationManager.userName = alias;
            synchronizationManager.Initialize();

            currentSessionInfo = sessionInfo;
            gmsec.Subscribe("GMSEC." + missionName.ToUpper() + "." + satName.ToUpper() + ".REQ.ESTATE."
                + currentSessionInfo.groupName.ToUpper() + "." + currentSessionInfo.projectName.Replace(".mtproj", "").ToUpper() + ".*");
            subscribed = true;

            Debug.Log("[SessionMasterNode] Node Initialized");
        }

        public void SendEntityStateResponse(GMSECMessage msg)
        {
            // Update number of users.
            currentSessionInfo.numUsers = synchronizedUsers.Count;

            // Set Reply Subject.
            string rplSubject = "GMSEC." + missionName.ToUpper() + "." + satName.ToUpper() + ".RESP.ESTATE."
                + currentSessionInfo.groupName.ToUpper() + "." + currentSessionInfo.projectName.Replace(".mtproj", "").ToUpper();

            // Create Reply Message.
            GMSECMessage reply = new GMSECMessage(rplSubject, GMSECMessage.MessageKind.REPLY);
            reply.AddField("HEADER-VERSION", (float)2010);
            reply.AddField("MESSAGE-TYPE", "RESP");
            reply.AddField("MESSAGE-SUBTYPE", "ESTATE");
            reply.AddField("CONTENT-VERSION", (float)2018);
            reply.AddField("RESPONSE-STATUS", (short)1);

            for (int i = 0; i < synchronizedUsers.Count; i++)
            {
                reply.AddField("USER." + i + ".NAME", synchronizedUsers[i].userAlias);
                reply.AddField("USER." + i + ".AVATAR", synchronizedUsers[i].avatarName);

                Vector3 userPosition = synchronizedUsers[i].userObject.transform.position;
                reply.AddField("USER." + i + ".POSITION", userPosition.x + "/"
                    + userPosition.y + "/" + userPosition.z);

                Quaternion userRotation = synchronizedUsers[i].userObject.transform.rotation;
                reply.AddField("USER." + i + ".ROTATION", userRotation.x + "/"
                    + userRotation.y + "/" + userRotation.z + "/" + userRotation.w);

                Vector3 userScale = synchronizedUsers[i].userObject.transform.localScale;
                reply.AddField("USER." + i + ".SCALE", userScale.x + "/" + userScale.y
                    + "/" + userScale.z);
            }

            // Send Reply.
            gmsec.Reply(msg, reply);

            // Add user to scene.
            GameObject newUserAvatar = Instantiate(defaultAvatarPrefab);
            string newUserName = msg.GetField("TOKEN-INFO").GetValueAsString();
            SynchronizedUserDeprecated newUser = newUserAvatar.AddComponent<SynchronizedUserDeprecated>();
            newUser.avatarName = newUserName == null ? "New_User" : newUserName;
            newUserAvatar.transform.position = Vector3.zero;
            newUserAvatar.transform.parent = userContainer.transform;
            newUserAvatar.name = newUser.avatarName;
            newUser.userObject = newUserAvatar;

            SynchronizedControllerDeprecated leftController = newUser.transform.Find("Left").gameObject.AddComponent<SynchronizedControllerDeprecated>();
            SynchronizedControllerDeprecated rightController = newUser.transform.Find("Right").gameObject.AddComponent<SynchronizedControllerDeprecated>();
            leftController.controllerSide = SynchronizedControllerDeprecated.ControllerSide.Left;
            rightController.controllerSide = SynchronizedControllerDeprecated.ControllerSide.Right;
            leftController.synchronizedUser = newUser;
            rightController.synchronizedUser = newUser;

            SynchronizedPointerDeprecated leftPointer = newUser.transform.Find("Left/Laser").gameObject.AddComponent<SynchronizedPointerDeprecated>();
            SynchronizedPointerDeprecated rightPointer = newUser.transform.Find("Right/Laser").gameObject.AddComponent<SynchronizedPointerDeprecated>();
            leftPointer.synchronizedController = leftController;
            rightPointer.synchronizedController = rightController;
            leftPointer.lineRenderer = newUser.transform.Find("Left/Laser").GetComponent<LineRenderer>();
            rightPointer.lineRenderer = newUser.transform.Find("Right/Laser").GetComponent<LineRenderer>();

            synchronizedUsers.Add(newUser);
        }

        void ReceiveMessage()
        {
            GMSECMessage msg = gmsec.Receive(0);
            if (msg != null)
            {
                if (msg.GetKind() == GMSECMessage.MessageKind.REQUEST)
                {
                    SendEntityStateResponse(msg);
                }
            }
        }

        // Disconnect should be called to clean up connection resources.
        void OnDestroy()
        {
            if (gmsec)
            {
                gmsec.Disconnect();
            }
        }

        #region Helpers
        private string ConnectionTypeToString(ConnectionTypes rawConnType)
        {
            string connType = "gmsec_bolt";

            switch (rawConnType)
            {
                case ConnectionTypes.amq383:
                    connType = "gmsec_activemq383";
                    break;

                case ConnectionTypes.amq384:
                    connType = "gmsec_activemq384";
                    break;

                case ConnectionTypes.bolt:
                    connType = "gmsec_bolt";
                    break;

                case ConnectionTypes.mb:
                    connType = "gmsec_mb";
                    break;

                case ConnectionTypes.ws71:
                    connType = "gmsec_websphere71";
                    break;

                case ConnectionTypes.ws75:
                    connType = "gmsec_websphere75";
                    break;

                case ConnectionTypes.ws80:
                    connType = "gmsec_websphere80";
                    break;

                default:
                    connType = "gmsec_bolt";
                    break;
            }

            return connType;
        }
        #endregion
    }
}