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

// Only including the create methods
// Using the "ImportUrdf" namespace and scripts
// (C) NASA Goddard Space Flight Center, 2021, Molly Goldstein <molly.goldstein@nasa.gov>

using UnityEngine;
using RosSharp.Urdf;

namespace RosSharp.ImportUrdf
{
    public static class ImportUrdfLinkExtensions
    {
        public static UrdfLink Create(Transform parent, Link link = null, RosSharp.Urdf.Joint joint = null)
        {
            GameObject linkObject = new GameObject("link");
            linkObject.transform.SetParentAndAlign(parent);
            UrdfLink urdfLink = linkObject.AddComponent<UrdfLink>();

            ImportUrdfVisualsExtensions.Create(linkObject.transform, link?.visuals); //modified
            ImportUrdfCollisionsExtensions.Create(linkObject.transform, link?.collisions); //modified
            
            if (link != null)
                urdfLink.ImportLinkData(link, joint);
           /* else
            {
                UrdfInertial.Create(linkObject);
                UnityEditor.EditorGUIUtility.PingObject(linkObject);
            }*/

            return urdfLink;
        }

        private static void ImportLinkData(this UrdfLink urdfLink, Link link, RosSharp.Urdf.Joint joint)
        {
            if (link.inertial == null && joint == null)
                urdfLink.IsBaseLink = true;

            urdfLink.gameObject.name = link.name;

            if (joint?.origin != null)
                UrdfOrigin.ImportOriginData(urdfLink.transform, joint.origin);

            if (link.inertial != null)
            {
                UrdfInertial.Create(urdfLink.gameObject, link.inertial);

                if (joint != null)
                    UrdfJoint.Create(urdfLink.gameObject, UrdfJoint.GetJointType(joint.type), joint);
            }
            else if (joint != null)
                Debug.LogWarning("No Joint Component will be created in GameObject \"" + urdfLink.gameObject.name + "\" as it has no Rigidbody Component.\n"
                                 + "Please define an Inertial for Link \"" + link.name + "\" in the URDF file to create a Rigidbody Component.\n", urdfLink.gameObject);

            foreach (RosSharp.Urdf.Joint childJoint in link.joints)
            {
                Link child = childJoint.ChildLink;
                Create(urdfLink.transform, child, childJoint);
            }
        } 
    }
}
