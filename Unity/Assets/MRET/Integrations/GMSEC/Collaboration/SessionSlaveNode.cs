// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.GMSEC;
using GOV.NASA.GSFC.XR.MRET.Collaboration;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.GMSEC.Collaboration
{
    [System.Obsolete("Refer to " + nameof(GOV.NASA.GSFC.XR.MRET.Collaboration.CollaborationManager))]
    public class SessionSlaveNode : MonoBehaviour
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

        public GameObject defaultAvatarPrefab, avatarContainer;

        // A MonoGMSEC object is needed to interface with GMSEC.
        private MonoGMSEC gmsec;

        // A thread to handle incoming responses asynchronously.
        private System.Threading.Thread requestThread;

        private Queue entityResponseMessages = new Queue();

        void Start()
        {
            gmsec = gameObject.AddComponent<MonoGMSEC>();
        }

        private void Update()
        {
            if (entityResponseMessages.Count > 0)
            {
                GMSECMessage rpl = (GMSECMessage)entityResponseMessages.Dequeue();

                int i = 0;
                GMSECMessage.Field nameField = rpl.GetField("USER." + i + ".NAME");
                while (nameField != null)
                {
                    string newUserAvatar = rpl.GetField("USER." + i + ".AVATAR").GetValueAsString();
                    GameObject newUserObject = Instantiate(defaultAvatarPrefab);
                    SynchronizedUserDeprecated newUser = newUserObject.AddComponent<SynchronizedUserDeprecated>();
                    newUser.userObject = newUserObject;
                    newUser.userObject.transform.parent = avatarContainer.transform;

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

                    string newUserName = nameField.GetValueAsString();
                    newUser.userAlias = newUserName;
                    newUser.userObject.name = newUserName;

                    string[] posComponents = rpl.GetField("USER." + i + ".POSITION").GetValueAsString().Split('/');
                    newUser.userObject.transform.position = new Vector3(float.Parse(posComponents[0]),
                        float.Parse(posComponents[1]), float.Parse(posComponents[2]));

                    string[] rotComponents = rpl.GetField("USER." + i + ".ROTATION").GetValueAsString().Split('/');
                    newUser.userObject.transform.rotation = new Quaternion(float.Parse(rotComponents[0]),
                        float.Parse(rotComponents[1]), float.Parse(rotComponents[2]), float.Parse(rotComponents[3]));

                    string[] sclComponents = rpl.GetField("USER." + i + ".SCALE").GetValueAsString().Split('/');
                    newUser.userObject.transform.position = new Vector3(float.Parse(sclComponents[0]),
                        float.Parse(sclComponents[1]), float.Parse(sclComponents[2]));

                    nameField = rpl.GetField("USER." + ++i + ".NAME");
                }
            }
        }

        public void Connect(SessionInformation sessionInfo, string alias)
        {
            if (gmsec == null)
            {
                gmsec = gameObject.AddComponent<MonoGMSEC>();
            }

            Debug.Log("[SessionSlaveNode] Initializing Node");
            //gmsec.Initialize();

            //gmsec.CreateConfig();
            //gmsec.AddToConfig("connectionType", ConnectionTypeToString(connectionType));
            //gmsec.AddToConfig("server", server);
            //gmsec.AddToConfig("GMSEC-REQ-RESP", "OPEN-RESP");   // Enable GMSEC open response feature.

            // Connect to GMSEC Bus.
            //gmsec.Connect();

            // Set up the synchronization manager.
            synchronizationManager.server = server;
            synchronizationManager.missionName = missionName;
            synchronizationManager.satName = satName;
            synchronizationManager.groupID = sessionInfo.groupName;
            synchronizationManager.projectName = sessionInfo.projectName;
            synchronizationManager.userName = alias;
            synchronizationManager.Initialize();

            // Send entity request message to master.
            //gmsec.Subscribe("GMSEC." + missionName + "." + satName + ".RESP.ESTATE."
            //    + sessionInfo.groupName + "." + sessionInfo.projectName + "." + alias);
            //requestThread = new System.Threading.Thread(ListenForConnectionResponses);
            //requestThread.Start();
            SendGMSECEntityRequestMessage(sessionInfo, alias);

            Debug.Log("[SessionSlaveNode] Node Initialized");
        }

        public void Disconnect()
        {

        }

        private void SendGMSECEntityRequestMessage(SessionInformation sessionInfo, string alias)
        {
            requestThread = new System.Threading.Thread(() =>
            {
                GMSECMessage req = new GMSECMessage("GMSEC." + missionName.ToUpper() + "." + satName.ToUpper()
                    + ".REQ.ESTATE." + sessionInfo.groupName.ToUpper() + "." + sessionInfo.projectName.Replace(".mtproj", "").ToUpper()
                    + "." + alias.ToUpper(), GMSECMessage.MessageKind.REQUEST);
                req.AddField("HEADER-VERSION", (float)2010);
                req.AddField("MESSAGE-TYPE", "REQ");
                req.AddField("MESSAGE-SUBTYPE", "ESTATE");
                req.AddField("CONTENT-VERSION", (float)2018);
                req.AddField("REQUEST-TYPE", (short)1);
                req.AddField("SESSION-NAME", sessionInfo.sessionName);
                req.AddField("PROJECT-NAME", sessionInfo.projectName);
                req.AddField("GROUP-NAME", sessionInfo.groupName);
                req.AddField("TOKEN-INFO", alias);
                GMSECMessage rpl = gmsec.Request(req, 10000);

            // Will be dequeued in the Update() method on the main thread.
            entityResponseMessages.Enqueue(rpl);
            });
            requestThread.Start();

            /*// Create Request Message with Header.
            gmsec.CreateNewMessage("GMSEC." + missionName + "." + satName
                + ".REQ.ESTATE." + sessionInfo.groupName + "." + sessionInfo.projectName + "." + alias);
            gmsec.AddF32FieldToMessage("HEADER-VERSION", 2010);
            gmsec.AddStringFieldToMessage("MESSAGE-TYPE", "REQ");
            gmsec.AddStringFieldToMessage("MESSAGE-SUBTYPE", "ESTATE");
            gmsec.AddF32FieldToMessage("CONTENT-VERSION", 2018);
            gmsec.AddI16FieldToMessage("REQUEST-TYPE", 1);
            gmsec.AddStringFieldToMessage("SESSION-NAME", sessionInfo.sessionName);
            gmsec.AddStringFieldToMessage("PROJECT-NAME", sessionInfo.projectName);
            gmsec.AddStringFieldToMessage("GROUP-NAME", sessionInfo.groupName);
            gmsec.AddStringFieldToMessage("TOKEN-INFO", alias);
            gmsec.PublishMessage();*/
        }

        // Disconnect should be called to clean up connection resources.
        void OnDestroy()
        {
            if (requestThread != null)
            {
                requestThread.Abort();
            }

            if (gmsec)
            {
                //gmsec.Disconnect();
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