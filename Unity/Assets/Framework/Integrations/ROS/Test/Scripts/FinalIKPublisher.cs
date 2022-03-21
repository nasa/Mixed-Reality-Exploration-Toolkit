// Copyright � 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.IO;
using UnityEngine;
using RosSharp.ImportUrdf;
using RosSharp.Urdf;
using RosSharp.RosBridgeClient;
using RosSharp.FinalIK;

public class FinalIKPublisher : MonoBehaviour
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

        // This is the test for the publisher, so add the publisher component, set the topic to "/joint_states"
        // and enable the JointStateReaders for the publisher to use
        JointStatePublisher publisher = parent.AddComponent<JointStatePublisher>();
        publisher.Topic = "/joint_states";
        patcher.SetPublishJointStates(true);

        // Create a primitive Cube object for the NBV arm to target
        // Move this cube around to see the robot arm move
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.localPosition = new Vector3(0, 1, 1);
        cube.transform.localScale = new Vector3(0.1f, 0.1F, 0.1f);

        // Initialize the CCD IK component for the movement of the robot arm
        CCDIKManager.InitializeCCDIK(urdfRobot, cube.transform);

    }
}
