// Copyright � 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.IO;
using System.Collections;
using UnityEngine;
using GSFC.ARVR.MRET.Common.Schemas;
using GSFC.ARVR.MRET.Infrastructure.Components.ModelLoading;
using RosSharp.RosBridgeClient;
using RosSharp.Urdf;
using RosSharp.ImportUrdf;
using RosSharp.FinalIK;

public class RosManager : MonoBehaviour
{
    //TODO: have datatype to keep track of ROS connections

    // This function adds the ROS connection for the bridge and instantiates either a publisher or subscriber
    // based on which topic is given in the Part XML.
    public static void AddRosConnection(GameObject obj, ROSConnectionType part)
    {
        // Initialize ROS connection to the bridge using the parameters from the XML file
        RosConnector rosConnector = obj.AddComponent<RosConnector>();
        rosConnector.SecondsTimeout = 10;
        rosConnector.Serializer = (RosSocket.SerializerEnum)part.Serializer;
        rosConnector.protocol = (RosSharp.RosBridgeClient.Protocols.Protocol)part.Protocol;
        rosConnector.RosBridgeServerUrl = part.ROSBridgeServerURL;

        // If a valid URDF path is given, import the robot as a GameObject in the scene
        if (!string.IsNullOrEmpty(part.UrdfPath) && Path.GetExtension(part.UrdfPath).Equals(".urdf"))
        {
            // Set the full path static variable of the importer to the parent directory of the urdf
            RuntimeImportManager.fullPath = Path.GetDirectoryName(part.UrdfPath);

            // Import the robot to the scene and return the UrdfRobot component for the JointStatePatcher to use
            UrdfRobot robot = RuntimeImportManager.Import(obj.transform, Path.GetFileName(part.UrdfPath));
            JointStatePatcher patcher = obj.AddComponent<JointStatePatcher>();
            patcher.UrdfRobot = robot;

            // Set all Rigidbodies in the robot to use kinematics.
            robot.SetRigidbodiesIsKinematic(true);

            // If a topic for the subscriber is given in the Part XML, create the subscriber component,
            // set the specified topic, and enable the JointStateWriters and subscribing joints from the patcher
            // (Calling the patcher method from the RosSharp.RosBridgeCommunication namespace)
            if (!string.IsNullOrEmpty(part.JointStateSubscriberTopic))
            {
                JointStateSubscriber subscriber;
                if (obj.transform.GetComponent<JointStateSubscriber>())
                {
                    subscriber = obj.transform.GetComponent<JointStateSubscriber>();
                }
                else
                {
                    subscriber = obj.AddComponent<JointStateSubscriber>();
                }
                subscriber.Topic = part.JointStateSubscriberTopic;
                patcher.SetSubscribeJointStates(true);
            }
            else
            {
                patcher.SetSubscribeJointStates(false);
            }

            // If a topic for the publisher is given in the Part XML, create the publisher component,
            // set the specified topic, and enable the JointStateReaders and publishing joints from the patcher
            // (Calling the patcher method from the RosSharp.RosBridgeCommunication namespace)
            if (!string.IsNullOrEmpty(part.JointStatePublisherTopic))
            {
                JointStatePublisher publisher;
                if (obj.transform.GetComponent<JointStatePublisher>())
                {
                    publisher = obj.transform.GetComponent<JointStatePublisher>();
                }
                else
                {
                    publisher = obj.AddComponent<JointStatePublisher>();
                }
                publisher.Topic = part.JointStatePublisherTopic;
                patcher.SetPublishJointStates(true);

                // Set up the inverse kinematics for the robot arm
                // The robot will have a cube as a target which can be moved around in the scene view
                // and the joint states will be published as the robot moves
                CCDIKManager.InitializeCCDIK(robot);    
            }
            else
            {
                patcher.SetPublishJointStates(false);
            }
        }
        else
        {
            Debug.LogError("UrdfPath in your Part xml does not have file extension \".urdf\".");
        }
    }
}
