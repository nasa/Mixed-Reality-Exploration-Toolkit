// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.XRC;
using GOV.NASA.GSFC.XR.XRUI.Keyboard;
using GOV.NASA.GSFC.XR.XRUI.Widget;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.User;
using GOV.NASA.GSFC.XR.MRET.Schema;
using GOV.NASA.GSFC.XR.MRET.Integrations.GMSEC.Collaboration;

namespace GOV.NASA.GSFC.XR.MRET.UI.Collaboration
{
    public class CollaborationJoinMenuManager : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(CollaborationJoinMenuManager);

        public const string DEFAULT_ALIAS = "New_User";

        public ScrollListManager sessionListDisplay;
        public VR_InputField aliasInput;
        public SessionSeeker sessionSeeker;
        public GameObject noSessionLabel;
        public Button refreshButton;

        private CollaborationManager collaborationManager;
        private bool needToUpdateSessionListDisplay = false;
        private int currentSelection = -1;
        private IRemoteSession[] availableXRCSessions;

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            collaborationManager = MRET.CollaborationManager;

            sessionListDisplay.SetTitle("Session Name");

            if (collaborationManager.engineType == CollaborationManager.EngineType.XRC)
            {
                int idx = collaborationManager.server.LastIndexOf(':');
                if (idx == -1)
                {
                    LogError("Invalid Server Format.", nameof(MRETStart));
                    return;
                }
                string[] serverParts = {
                collaborationManager.server.Substring(0, idx),
                collaborationManager.server.Substring(idx + 1)
            };
                if (collaborationManager.IsEngineStarted)
                {
                    collaborationManager.StopEngine();
                }
                collaborationManager.InitializeEngine(serverParts[0], int.Parse(serverParts[1]));
                if (!collaborationManager.StartEngine())
                {
                    LogWarning("Collaboration engine failed to startup.", nameof(MRETStart));
                }
            }
            else
            {
                sessionSeeker.server = collaborationManager.server;
                sessionSeeker.missionName = collaborationManager.missionName;
                sessionSeeker.satName = collaborationManager.satName;
            }
        }

        /// <seealso cref="MRETBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            if (needToUpdateSessionListDisplay)
            {
                needToUpdateSessionListDisplay = false;
                refreshButton.interactable = true;
                UpdateSessionListDisplay();
            }
        }

        public void RefreshSessions()
        {
            refreshButton.interactable = false;
            if (collaborationManager.engineType == CollaborationManager.EngineType.XRC)
            {
                needToUpdateSessionListDisplay = true;
            }
            else
            {
                System.Threading.Thread seekerThread = new System.Threading.Thread(() =>
                {
                    FindAvailableSessions();
                    System.Threading.Thread.Sleep(5000);
                    needToUpdateSessionListDisplay = true;
                });
                seekerThread.Start();
            }
        }

        public void Join()
        {
            if (currentSelection > -1)
            {
                if (collaborationManager.engineType == CollaborationManager.EngineType.XRC)
                {
                    // TODO: Once additional fields can be passed with the join messages, this will be much cleaner.
                    MRET.ModeNavigator.OpenProject(System.IO.Path.ChangeExtension(
                        System.IO.Path.Combine(MRET.ConfigurationManager.defaultProjectDirectory,
                        availableXRCSessions[currentSelection].Project), ProjectFileSchema.FILE_EXTENSION), true);

                    IUser localUser = collaborationManager.GetLocalUser();
                    if (localUser != null)
                    {
                        // Override the user alias if supplied.
                        // Text field takes precedence, then existing user alias, then default
                        localUser.Alias = !string.IsNullOrEmpty(aliasInput.ToString()) ? aliasInput.ToString() :
                            !string.IsNullOrEmpty(localUser.Alias) ? localUser.Alias : DEFAULT_ALIAS;

                        // Join the session
                        collaborationManager.JoinSession(availableXRCSessions[currentSelection].id, localUser);
                    }
                }
                else
                {
                    collaborationManager.EnterSlaveMode(sessionSeeker.availableSessions[currentSelection], aliasInput.text);
                }
            }
        }

        private void FindAvailableSessions()
        {
            sessionSeeker.SearchForSessions();
        }

        private void UpdateSessionListDisplay()
        {
            if (collaborationManager.engineType == CollaborationManager.EngineType.XRC)
            {
                int idx = 0;
                availableXRCSessions = collaborationManager.GetRemoteSessions();
                if (availableXRCSessions != null)
                {
                    foreach (XRCSessionInfo sess in availableXRCSessions)
                    {
                        int indexToSelect = idx;
                        UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
                        clickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetActiveSelection(indexToSelect); }));
                        sessionListDisplay.AddScrollListItem(availableXRCSessions[idx].Session
                            + " " + availableXRCSessions[idx].NumUsers + "Users", clickEvent);

                        idx++;
                    }
                }

                noSessionLabel.SetActive(idx == 0);
            }
            else
            {
                if (sessionSeeker.availableSessions.Count == 0)
                {
                    noSessionLabel.SetActive(true);
                }
                else
                {
                    noSessionLabel.SetActive(false);

                    for (int i = 0; i < sessionSeeker.availableSessions.Count; i++)
                    {
                        int indexToSelect = i;
                        UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
                        clickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetActiveSelection(indexToSelect); }));
                        sessionListDisplay.AddScrollListItem(sessionSeeker.availableSessions[i].sessionName
                            + " " + sessionSeeker.availableSessions[i].numUsers + "Users", clickEvent);
                    }
                }
            }
        }

        private void SetActiveSelection(int listID)
        {
            currentSelection = listID;
            sessionListDisplay.HighlightItem(listID);
        }
    }
}