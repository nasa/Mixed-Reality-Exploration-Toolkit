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

// Only including the create methods, and using the "ImportUrdf" namespace and scripts
// (C) NASA Goddard Space Flight Center, 2021, Molly Goldstein <molly.goldstein@nasa.gov>

using System.Collections.Generic;
using UnityEngine;
using RosSharp.Urdf;

namespace RosSharp.ImportUrdf
{
    public static class ImportUrdfCollisionsExtensions
    {
        public static void CreateOne(Transform parent, Link.Collision collision)
        {
            GameObject collisionObject = new GameObject("unnamed");
            collisionObject.transform.SetParentAndAlign(parent);
            UrdfCollision urdfCollision = collisionObject.AddComponent<UrdfCollision>();
            urdfCollision.GeometryType = ImportUrdfGeometry.GetGeometryType(collision.geometry); //modified

            ImportUrdfGeometryCollision.Create(collisionObject.transform, urdfCollision.GeometryType, collision.geometry); //modified
            UrdfOrigin.ImportOriginData(collisionObject.transform, collision.origin);
        }
        public static void Create(Transform parent, List<Link.Collision> collisions = null)
        {
            GameObject collisionsObject = new GameObject("Collisions");
            collisionsObject.transform.SetParentAndAlign(parent);
            UrdfCollisions urdfCollisions = collisionsObject.AddComponent<UrdfCollisions>();

            collisionsObject.hideFlags = HideFlags.NotEditable;
            urdfCollisions.hideFlags = HideFlags.None;

            if (collisions != null)
            {
                foreach (Link.Collision collision in collisions)
                    CreateOne(urdfCollisions.transform, collision);
            }
        }
    }
}