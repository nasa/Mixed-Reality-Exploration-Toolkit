// Copyright � 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.IO;
using UnityEngine;
using RosSharp.ImportUrdf;
using RosSharp.Urdf;
using RosSharp.RosBridgeClient;


public class PublisherTest : MonoBehaviour
{
    private GameObject end; // the joint that will move in this test

    private int rot = -101;

    private bool forward = true;
    
    // Start is called before the first frame update
    // Follows the RosManager's Start() method closely
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

        // Find the end of the NBV arm for the script to rotate in Update()
        end = GameObject.Find("end");
    }

    // Update is called once per frame
    // Rotate the end joint of the NBV arm back and forth, check to see if the same motion driven here appears on the ROS side
    void Update()
    {
        if(forward)
        {
            if( rot < 101)
            {
                end.transform.rotation =  Quaternion.Euler(rot, 0, 90);
                rot++;
            }
            else
            {
                forward = false;
            }
        }
        else
        {
            if ( rot > -101)
            {
                end.transform.rotation =  Quaternion.Euler(rot, 0, 90);
                rot--;
            }
            else
            {
                forward = true;
            }
        }
    }
}
