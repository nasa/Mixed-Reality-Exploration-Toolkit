# ROS# Integration

The goal of the integration is to generalize the ROS adapter prototype developed by the UMD SSL to work with arbitrary *robot arms* or any object that has joint states that can be read and written.

This integration provides MRET with the ability to publish and subscribe to joint states through a ROS bridge server running on a different machine.

## Getting Started

These instructions will explain how to get the ROS# bridge running on an Ubuntu VM,
run the tests from Unity, and create a project file.

### Installing

To configure ROS on a Virtual Machine, follow the first three [Installation and configuration](https://github.com/siemens/ros-sharp/wiki/User_Inst_InstallationAndConfiguration#1-installation-and-configuration) tutorials on the ROS# GitHub.

The required tutorials are:
1. [Unity on Windows](https://github.com/siemens/ros-sharp/wiki/User_Inst_Unity3DOnWindows)
2. [Ubuntu on Oracle VM](https://github.com/siemens/ros-sharp/wiki/User_Inst_UbuntuOnOracleVM)
3. [ROS on Ubuntu](https://github.com/siemens/ros-sharp/wiki/User_Inst_ROSOnUbuntu)


## Running the tests

There are three test scenes under the **Test/Scenes** directory:
- SubscriberTest
- PublisherTest
- FinalIKPublisherTest

The installation steps must be followed to have ROS up and running on a different machine for these tests.

### Tests

Before running these tests, make sure the launch files under **Standard_MRET_Content\Content\Robots\ROS Launch** in this directory are copied to the **src/file_server/launch** directory of your Catkin workspace on the ROS machine. Then run `catkin_make`.

Also make sure the VPN is disconnected before testing.

#### Subscriber Test

This tests the subscriber functionality of the ROS# integration with MRET.
This uses the **display-publisher.launch** file, which publishes states to the "/joint_states" topic.

Steps:

1. In the ROS machine, open a terminal and enter the follow commands:
    ```
    source ~/catkin_ws/devel/setup.bash
    roslaunch file_server display-publisher.launch model:="<path>/<to>/nbv/urdf/nbv.urdf.xacro"
 
*Change the path to match the location of the URDF file on the ROS machine.*

2. In a new terminal window on the ROS machine, enter the following commands:
    ```
    source ~/catkin_ws/devel/setup.bash
    roslaunch file_server ros_sharp_communication.launch

3. Click play in Unity.
4. The RViz and joint state publisher GUI should have appeared after launching in ROS. Change the joint states the robot and see that the robot in Unity is also changing.

#### Publisher Tests

This tests the publisher functionality of the ROS# integration with MRET.
This uses the **display-subscribe.launch** file, which subscribes to the states on the "/joint_states" topic.

Steps:

1. In the ROS machine, open a terminal and enter the follow commands:
    ```
    source ~/catkin_ws/devel/setup.bash
    roslaunch file_server display-subscriber.launch model:="<path>/<to>/nbv/urdf/nbv.urdf.xacro"

*Change the path to match the location of the URDF file on the ROS machine.*
    
2. In a new terminal window on the ROS machine, enter the following commands:
    ```
    source ~/catkin_ws/devel/setup.bash
    roslaunch file_server ros_sharp_communication.launch

3. Click play in Unity.
4. For FinalIKPublisherTest, view the robot in the Scene view. Click on the **Cube** GameObject and drag it around. The robot should follow the cube, and the changing joint states should publish to the ROS server.


### Part XML Schema

To use a robot Part with this integration in MRET, the following fields must be added to the Part in the XML file:

    <mt:ROSConnectionType>
        <mt:ROSBridgeServerURL>ws://192.168.56.101:9090</mt:ROSBridgeServerURL>
        <mt:Protocol>WebSocketDotNET</mt:Protocol>
        <mt:Serializer>Microsoft</mt:Serializer>
        <!-- Use this field to subscribe to a joint state topic, i.e. "/joint_states" -->
        <mt:JointStateSubscriberTopic>/joint_states</mt:JointStateSubscriberTopic>
        <!-- Use this field to publish to a joint state topic, i.e. "/joint_states" -->
        <mt:JointStatePublisherTopic>/joint_states</mt:JointStatePublisherTopic>
        <mt:UrdfPath>c:\Users\mtgoldst\Documents\Test ROS Connection\Assets\Urdf\nbv\robot_description.urdf</mt:UrdfPath>
    </mt:ROSConnectionType>
Change the field values to match the correct configuration.

*If both a subscriber and a publisher exist for the robot, make sure the topic names are different.*

See **Test/ExampleProject.mret** for an example project file that uses an AssetBundle consisting of an "Empty" scene with a plane.

## To Do
The controls for the robot arm, with the intention of publishing changing joint states, have not been fully flushed out. The FinalIK only works in the Scene view and not within MRET yet, but the functionality to
publish the joint states is there. The controls from within MRET just need to be worked on, and the requirements need to be determined.

## Authors

  - **Molly Goldstein** (molly.goldstein@nasa.gov) - *wrote MRET integration scripts and modified parts of the ROS# package*
  - **Dr. Martin Bischoff** (martin.bischoff@siemens.com) - *author of the ROS# package*


## License

[ROS# license](https://github.com/siemens/ros-sharp#licensing)

## Acknowledgments
  - Natalie Condzal (natalie.m.condzal@jpl.nasa.gov)  [University of Maryland's Space Systems Laboratory](https://ssl.umd.edu/)
  - [Unity-Technologies: URDF-Importer](https://github.com/Unity-Technologies/URDF-Importer#:~:text=URDF%20Importer%20allows%20you%20to%20import%20a%20robot,it%20into%20Unity%20using%20PhyX%204.0%20articulation%20bodies.)
