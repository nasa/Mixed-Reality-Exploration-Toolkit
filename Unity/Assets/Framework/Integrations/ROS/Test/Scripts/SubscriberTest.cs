// Copyright � 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.IO;
using UnityEngine;
using RosSharp.ImportUrdf;
using RosSharp.Urdf;
using RosSharp.RosBridgeClient;

public class SubscriberTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Create a new GameObject for the robot to be the parent object and add the ROS connector
        GameObject parent = new GameObject("robot");
        RosConnector rosConnector = parent.AddComponent<RosConnector>();
        // This is the URL for the machine originally used for testing.
        // Change this to match the URL of the target ROS bridger server
        rosConnector.RosBridgeServerUrl = "ws://192.168.56.101:9090";

        // Set the full path static variable of the importer to the parent directory of the urdf
        // This test uses the NBV arm urdf and stl files found within the Tests directory
        RuntimeImportManager.fullPath = Path.GetDirectoryName(Application.dataPath + @"\Framework\Integrations\ROS\Test\Urdf\nbv\");

        // Import the robot to the scene and return the UrdfRobot component for the JointStatePatcher to use
        UrdfRobot urdfRobot = RuntimeImportManager.Import(parent.transform, "robot_description.urdf");
        JointStatePatcher patcher = parent.AddComponent<JointStatePatcher>();
        patcher.UrdfRobot = urdfRobot;

        // Set all Rigidbodies in the robot to use kinematics.
        urdfRobot.SetRigidbodiesIsKinematic(true);

        // This is the test for the subscriber, so add the subscriber component, set the topic to "/joint_states"
        // and enable the JointStateWriters for the subscriber to use
        JointStateSubscriber subscriber = parent.AddComponent<JointStateSubscriber>();
        subscriber.Topic = "/joint_states";
        patcher.SetSubscribeJointStates(true);
    }
}
