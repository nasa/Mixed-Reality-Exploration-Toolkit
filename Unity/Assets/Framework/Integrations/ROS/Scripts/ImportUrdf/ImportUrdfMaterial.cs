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

// Using the "ImportUrdf" namespace and only need material setting methods
// (C) NASA Goddard Space Flight Center, 2021, Molly Goldstein <molly.goldstein@nasa.gov>

using UnityEngine;

namespace RosSharp.ImportUrdf
{
    public class ImportUrdfMaterial
    {
        public static void SetUrdfMaterial(GameObject gameObject)
        {
            Renderer renderer = gameObject.GetComponentInChildren<Renderer>();
            if (renderer != null && renderer.sharedMaterial == null)
            {
                var defaultMaterial = Resources.Load("ROSDefault") as Material;
                SetMaterial(gameObject, defaultMaterial);
            }
        }
        private static void SetMaterial(GameObject gameObject, Material material)
        {
            var renderers = gameObject.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
                renderer.sharedMaterial = material;
        }
    }
}