/*
© Siemens AG, 2018
Author: Suzannah Smith (suzannah.smith@siemens.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

<http://www.apache.org/licenses/LICENSE-2.0>.

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

// Using the "ImportUrdf" namespace and scripts
// Repurposing the "Create" method from UrdfRobotExtension.cs and renaming to "Import"
// Changing function return type to UrdfRobot
// (C) NASA Goddard Space Flight Center, 2021, Molly Goldstein <molly.goldstein@nasa.gov>

using System.IO;
using UnityEngine;
using RosSharp.Urdf;

namespace RosSharp.ImportUrdf
{
    public class RuntimeImportManager : MonoBehaviour
    {
        public static string fullPath;
        public static UrdfRobot Import(Transform parent, string filename)
        {
            //Create a Robot object from the URDF file
            Robot robot = new Robot(Path.Combine(fullPath, filename));

            // Create a GameObject for the robot, and set the Part as the parent of the object
            GameObject robotGameObject = new GameObject(robot.name);
            robotGameObject.transform.SetParent(parent);

            // Add the UrdfRobot component to the robot GameObject
            UrdfRobot urdfRobot = robotGameObject.AddComponent<UrdfRobot>();

            // Add link and joint configurations to the robot GameObject
            UrdfPlugins.Create(robotGameObject.transform, robot.plugins);
            ImportUrdfLinkExtensions.Create(robotGameObject.transform, robot.root);

            return urdfRobot;
        }
    }
}