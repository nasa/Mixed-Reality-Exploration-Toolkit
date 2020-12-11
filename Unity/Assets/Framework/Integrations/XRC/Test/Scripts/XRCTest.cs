using System;
using UnityEngine;
using UnityEngine.UI;
using GSFC.ARVR.XRC;
using GSFC.ARVR.MRET.XRC;

public class XRCTest : MonoBehaviour
{
    public const int EXIT_SUCCESS = 0;
    public const int EXIT_FAILURE = -1;

    public enum TestType { Master, Slave }

    public XRCManager xrcManager;

    public GameObject sessionButtonPrefab;
    public GameObject sessionButtonContainer;
    public float sessionButtonXPos = 0, sessionButtonZPos = 0;
    public float sessionButtonDistance = -32;
    public float sessionButtonOffset = -25;

    LogLevel logLevel = LogLevel.INFO;

    public TestType testType;

    public string projectName = "New_Project";
    public string groupName = "New_Group";
    public string sessionName = "New_Session";

    public string missionName = "UNSET";
    public string satelliteName = "UNSET";

    public string userAlias = "New_User";
    public SynchronizedUser.UserType userType = SynchronizedUser.UserType.VR;
    public SynchronizedUser.LabelColor userLabelColor = SynchronizedUser.LabelColor.black;

    public string serverHost = "127.0.0.1";
    [Range(1, 65535)]
    public int serverPort = 9100;
    public ConnectionTypes connectionType = ConnectionTypes.bolt;

    private bool updateSessionsList = false;
    private XRCSessionInfo[] availableXRCSessions;
    private int numFramesToWait = 30;
    private int numFramesWaited = 0;
    private int numFramesBetweenSessionCheck = 360;
    private int numFramesWaitedBetweenSessionChecks = 0;

    public void EntityCreationEventManager()
    {
        Debug.Log("Entity Creation Event Called.");
        EntityEventParameters evParams = XRCUnity.entityCreatedEventQueue.Dequeue();
        if (evParams != null)
        {
            Debug.Log("Bundle=" + evParams.bundle);
            Debug.Log("Category=" + evParams.category);
            Debug.Log("Parent UUID=" + evParams.parentUUID);
            Debug.Log("Tag=" + evParams.tag);
            Debug.Log("Type=" + evParams.type);
            Debug.Log("UUID=" + evParams.uuid);
        }
    }

    public void SessionJoiningEventManager()
    {
        Debug.Log("Session Joined Event Called.");
        foreach (SessionEntity userEntity in xrcManager.GetSessionUsers())
        {
            Debug.Log("*** User " + userEntity.uuid + " ***");
            Debug.Log("Tag=" + userEntity.tag);
            Debug.Log("Type=" + userEntity.type);
            Debug.Log("Color=" + userEntity.color);
            Debug.Log("Left Controller ID=" + userEntity.lcUUID);
            Debug.Log("Right Controller ID=" + userEntity.rcUUID);
            Debug.Log("Left Pointer ID=" + userEntity.lpUUID);
            Debug.Log("Right Pointer ID=" + userEntity.rpUUID);
        }
    }

    public void SessionParticipantAddedEventManager()
    {
        Debug.Log("Participant Added Event Called.");
        ActiveSessionEventParameters sessParams = XRCUnity.asParticipantAddedEventQueue.Dequeue();
        if (sessParams != null)
        {
            Debug.Log("Tag=" + sessParams.tag);
            Debug.Log("Type=" + sessParams.type);
            Debug.Log("Color=" + sessParams.color);
            Debug.Log("ID=" + sessParams.id);
            Debug.Log("Left Controller ID=" + sessParams.lcID);
            Debug.Log("Right Controller ID=" + sessParams.rcID);
            Debug.Log("Left Pointer ID=" + sessParams.lpID);
            Debug.Log("Right Pointer ID=" + sessParams.rpID);
        }
    }

    private void RunMasterTest()
    {
        Debug.Log("[XRCTest] Running as master node...");

        Guid userUUID = Guid.NewGuid();
        Debug.Log("User UUID is " + userUUID.ToString());

        Guid lcUUID = Guid.NewGuid();
        Debug.Log("Left Controller UUID is " + lcUUID.ToString());

        Guid rcUUID = Guid.NewGuid();
        Debug.Log("Right Controller UUID is " + rcUUID.ToString());

        Guid lpUUID = Guid.NewGuid();
        Debug.Log("Left Pointer UUID is " + lpUUID.ToString());

        Guid rpUUID = Guid.NewGuid();
        Debug.Log("Right Pointer UUID is " + rpUUID.ToString());

        Debug.Log("[XRCTest] Shutting down XRC...");
        XRCUnity.ShutDown();

        Debug.Log("[XRCTest] Initializing XRC" +
            ": server=" + serverHost +
            "; port=" + serverPort +
            "; connection=" + connectionType +
            "; mission=" + missionName +
            "; satellite=" + satelliteName +
            "; project=" + projectName +
            "; group=" + groupName +
            "; session=" + sessionName);
        XRCUnity.Initialize(serverHost, serverPort, connectionType,
            missionName, satelliteName, projectName, groupName, sessionName);

        Debug.Log("[XRCTest] Starting XRC...");
        xrcManager.StartXRC();

        Debug.Log("[XRCTest] Starting session...");
        XRCUnity.StartSession(userUUID.ToString(), userAlias, (int) userType,
            userLabelColor.ToString(), lcUUID.ToString(), rcUUID.ToString(), lpUUID.ToString(), rpUUID.ToString());

        Debug.Log("[XRCTest] Adding controllers to session...");
        XRCManager.AddControllersToSession(userAlias, userUUID.ToString(),
            lcUUID.ToString(), rcUUID.ToString(), lpUUID.ToString(), rpUUID.ToString());
    }

    private void RunSlaveTest()
    {
        Debug.Log("[XRCTest] Running as slave node...");

        Debug.Log("[XRCTest] Shutting down XRC...");
        XRCUnity.ShutDown();

        Debug.Log("[XRCTest] Initializing XRC...");
        XRCUnity.Initialize(serverHost, serverPort, connectionType,
            missionName, satelliteName, projectName, groupName, sessionName);

        Debug.Log("[XRCTest] Starting up XRC...");
        if (!XRCUnity.StartUp())
        {
            Debug.LogWarning("XRC failed to startup.");
        }

        updateSessionsList = true;
    }

    // Helper function for parsing the command line arguments
    void ParseArgs(string[] args)
    {
        foreach (string arg in args)
        {
            Debug.Log("[XRCTest] Command line arg: " + arg);

            // Determine if we have a key=value pair
            int t = arg.IndexOf("=");
            if (t != -1)
            {
                string key = arg.Substring(0, t);
                string value = arg.Substring(t + 1);

                key = key.ToLower();
                if (key.Length > 0 && value.Length > 0)
                {
                    if (key == "port")
                    {
                        try
                        {
                            serverPort = Convert.ToUInt16(value);
                            Debug.Log("[XRCTest] Port set: " + serverPort);
                        }
                        catch (FormatException e)
                        {
                            Console.Error.WriteLine("ERROR: " + e.Message);
                            Console.Error.Flush();
                            System.Environment.Exit(EXIT_FAILURE);
                        }
                    }
                    else if (key == "ip" || key == "server")
                    {
                        serverHost = value;
                        Debug.Log("[XRCTest] Server set: " + serverHost);
                    }
                    else if (key == "connectiontype")
                    {
                        try
                        {
                            connectionType = (ConnectionTypes)Enum.Parse(typeof(ConnectionTypes), value, true);
                            Debug.Log("[XRCTest] ConnectionType set: " + connectionType);
                        }
                        catch (ArgumentException e)
                        {
                            Console.Error.WriteLine("ERROR: " + e.Message);
                            Console.Error.Flush();
                            System.Environment.Exit(EXIT_FAILURE);
                        }
                    }
                    else if (key == "loglevel" || key == "debuglevel")
                    {
                        try
                        {
                            logLevel = (LogLevel)Enum.Parse(typeof(LogLevel), value, true);
                            Debug.Log("[XRCTest] LogLevel set: " + logLevel);
                        }
                        catch (ArgumentException e)
                        {
                            Console.Error.WriteLine("ERROR: " + e.Message);
                            Console.Error.Flush();
                            System.Environment.Exit(EXIT_FAILURE);
                        }
                    }
                    else if (key == "mission")
                    {
                        missionName = value;
                        Debug.Log("[XRCTest] MissionName set: " + missionName);
                    }
                    else if (key == "satellite")
                    {
                        satelliteName = value;
                        Debug.Log("[XRCTest] SatelliteName set: " + satelliteName);
                    }
                    else if (key == "project")
                    {
                        projectName = value;
                        Debug.Log("[XRCTest] ProjectName set: " + projectName);
                    }
                    else if (key == "group")
                    {
                        groupName = value;
                        Debug.Log("[XRCTest] GroupName set: " + groupName);
                    }
                    else if (key == "session")
                    {
                        sessionName = value;
                        Debug.Log("[XRCTest] SessionName set: " + sessionName);
                    }
                    else if (key == "useralias")
                    {
                        userAlias = value;
                        Debug.Log("[XRCTest] UserAlias set: " + userAlias);
                    }
                    else if (key == "usercolor")
                    {
                        userLabelColor = (SynchronizedUser.LabelColor)Enum.Parse(typeof(SynchronizedUser.LabelColor), value, true);
                        Debug.Log("[XRCTest] UserColor set: " + userLabelColor);
                    }
                    else if (key == "testtype")
                    {
                        testType = (TestType)Enum.Parse(typeof(TestType), value, true);
                        Debug.Log("[XRCTest] TestType set: " + testType);
                    }
                }
            }
        }
    }

    private void Awake()
    {
        string[] arguments = Environment.GetCommandLineArgs();
        ParseArgs(arguments);
    }

    private void Update()
    {
        if (numFramesWaited < numFramesToWait)
        {
            numFramesWaited++;
        }
        else if (numFramesWaited == numFramesToWait)
        {
            numFramesWaited++;
            switch (testType)
            {
                case TestType.Master:
                    RunMasterTest();
                    break;

                case TestType.Slave:
                    RunSlaveTest();
                    break;
            }
        }

        if (numFramesWaitedBetweenSessionChecks < numFramesBetweenSessionCheck)
        {
            numFramesWaitedBetweenSessionChecks++;
        }
        else if (updateSessionsList)
        {
            numFramesWaitedBetweenSessionChecks = 0;
            Debug.Log("[XRCTest] Updating session information...");

            // Destroy existing buttons.
            foreach (Transform button in sessionButtonContainer.transform)
            {
                Destroy(button.gameObject);
            }

            // Add new buttons.
            int idx = 0;
            availableXRCSessions = XRCUnity.GetRemoteSessions();
            foreach (XRCSessionInfo sessionInfo in availableXRCSessions)
            {
                int indexToSelect = idx;
                UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
                clickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { JoinSession(indexToSelect); }));

                GameObject btn = Instantiate(sessionButtonPrefab, new Vector3(sessionButtonXPos,
                    idx * sessionButtonDistance + sessionButtonOffset, sessionButtonZPos), Quaternion.identity);
                btn.transform.parent = sessionButtonContainer.transform;
                Text btnText = btn.GetComponentInChildren<Text>();
                if (btnText)
                {
                    btnText.text = availableXRCSessions[idx].session
                        + " " + availableXRCSessions[idx].numUsers + " Users";
                }
                Button btnBtn = btn.GetComponentInChildren<Button>();
                if (btnBtn)
                {
                    btnBtn.onClick.AddListener(() =>
                    {
                        clickEvent.Invoke();
                    });
                }

                idx++;
            }
        }
    }

    private void JoinSession(int idx)
    {
        updateSessionsList = false;

        Guid userUUID = Guid.NewGuid();
        Debug.Log("User UUID is " + userUUID.ToString());

        Guid lcUUID = Guid.NewGuid();
        Debug.Log("Left Controller UUID is " + lcUUID.ToString());

        Guid rcUUID = Guid.NewGuid();
        Debug.Log("Right Controller UUID is " + rcUUID.ToString());

        Guid lpUUID = Guid.NewGuid();
        Debug.Log("Left Pointer UUID is " + lpUUID.ToString());

        Guid rpUUID = Guid.NewGuid();
        Debug.Log("Right Pointer UUID is " + rpUUID.ToString());

        Debug.Log("[XRCTest] Joining session...");
        XRCUnity.JoinSession(availableXRCSessions[idx].id,
            userUUID.ToString(), userAlias, (int) userType,
            userLabelColor.ToString(), lcUUID.ToString(), rcUUID.ToString(), lpUUID.ToString(), rpUUID.ToString());
    }
}