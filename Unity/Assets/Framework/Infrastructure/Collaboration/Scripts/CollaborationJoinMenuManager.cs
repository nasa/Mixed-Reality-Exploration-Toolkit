// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GSFC.ARVR.XRC;
using GSFC.ARVR.MRET.XRC;
using GSFC.ARVR.MRET.Common.Schemas;
using GSFC.ARVR.MRET.Components.UI;

public class CollaborationJoinMenuManager : MonoBehaviour
{
    public ScrollListManager sessionListDisplay;
    public VR_InputField aliasInput;
    public SessionSeeker sessionSeeker;
    public GameObject noSessionLabel;
    public Button refreshButton;
    public CollaborationManager collaborationManager;

    private bool needToUpdateSessionListDisplay = false;
    private int currentSelection = -1;
    private XRCSessionInfo[] availableXRCSessions;

    void Start()
    {
        sessionListDisplay.SetTitle("Session Name");
        collaborationManager = FindObjectOfType<CollaborationManager>();
        if (collaborationManager != null)
        {
            if (collaborationManager.engineType == CollaborationManager.EngineType.XRC)
            {
                string[] serverParts = collaborationManager.server.Split(':');
                if (serverParts.Length == 2)
                {
                    XRCUnity.ShutDown();
                    XRCUnity.Initialize(serverParts[0], int.Parse(serverParts[1]), XRCManager.GMSECToXRCConnType(collaborationManager.connectionType),
                        collaborationManager.missionName, collaborationManager.satName);
                    if (!XRCUnity.StartUp())
                    {
                        Debug.LogWarning("[CollaborationJoinMenuManager] XRC failed to startup.");
                    }
                }
            }
            else
            {
                sessionSeeker.server = collaborationManager.server;
                sessionSeeker.missionName = collaborationManager.missionName;
                sessionSeeker.satName = collaborationManager.satName;
            }
        }
        else
        {
            Debug.LogError("[CollaborationJoinMenuManager] CollaborationManager not found.");
        }
    }

    void Update()
    {
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
            if (aliasInput.text == "")
            {
                aliasInput.text = "New_User";
            }

            if (collaborationManager.engineType == CollaborationManager.EngineType.XRC)
            {
                UnityProject.instance.userAlias = aliasInput.text;
                UnityProject.instance.userLabelColor =
                SynchronizedUser.LabelColorFromString(SynchronizedUser.GetRandomColor());
                // TODO: Once additional fields can be passed with the join messages, this will be much cleaner.
                ModeNavigator.instance.OpenProject(System.IO.Path.ChangeExtension(
                    System.IO.Path.Combine(ConfigurationManager.instance.defaultProjectDirectory,
                    availableXRCSessions[currentSelection].project), ProjectFileSchema.fileExtension), true);

                XRCUnity.JoinSession(availableXRCSessions[currentSelection].id,
                    UnityProject.instance.userUUID.ToString(), aliasInput.text,
                    (int) SynchronizedUser.UserType.VR,
                    UnityProject.instance.userLabelColor.ToString(),
                    UnityProject.instance.lcUUID.ToString(),
                    UnityProject.instance.rcUUID.ToString(),
                    UnityProject.instance.lpUUID.ToString(),
                    UnityProject.instance.rpUUID.ToString()); // TODO type
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
            availableXRCSessions = XRCUnity.GetRemoteSessions();
            if (availableXRCSessions != null)
            {
                foreach (XRCSessionInfo sess in availableXRCSessions)
                {
                    int indexToSelect = idx;
                    UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
                    clickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetActiveSelection(indexToSelect); }));
                    sessionListDisplay.AddScrollListItem(availableXRCSessions[idx].session
                        + " " + availableXRCSessions[idx].numUsers + "Users", clickEvent);

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