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

using System.Collections.Generic;
using UnityEngine;
using RosSharp.Urdf;

namespace RosSharp.ImportUrdf
{
    public static class ImportUrdfVisualsExtensions
    {

        public static void CreateOne(Transform parent, Link.Visual visual)
        {
            GameObject visualObject = new GameObject(visual.name ?? "unnamed");
            visualObject.transform.SetParentAndAlign(parent);
            UrdfVisual urdfVisual = visualObject.AddComponent<UrdfVisual>();

            urdfVisual.GeometryType = ImportUrdfGeometry.GetGeometryType(visual.geometry); //modified
            ImportUrdfGeometryVisual.Create(visualObject.transform, urdfVisual.GeometryType, visual.geometry); //modifed

            ImportUrdfMaterial.SetUrdfMaterial(visualObject); //modified
            UrdfOrigin.ImportOriginData(visualObject.transform, visual.origin);
        }
    
        public static void Create(Transform parent, List<Link.Visual> visuals = null)
        {
            GameObject visualsObject = new GameObject("Visuals");
            visualsObject.transform.SetParentAndAlign(parent);
            UrdfVisuals urdfVisuals = visualsObject.AddComponent<UrdfVisuals>();

            visualsObject.hideFlags = HideFlags.NotEditable;
            urdfVisuals.hideFlags = HideFlags.None;

            if (visuals != null)
            {
                foreach (Link.Visual visual in visuals)
                    CreateOne(urdfVisuals.transform, visual);
            }
        }
    }
}