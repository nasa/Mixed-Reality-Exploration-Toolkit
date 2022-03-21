/*
© Siemens AG, 2018-2019
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
// Using STL Importer instead of AssetDatabase loader
// (C) NASA Goddard Space Flight Center, 2021, Molly Goldstein <molly.goldstein@nasa.gov>

using UnityEngine;
using System.IO;
using RosSharp.Urdf;

namespace RosSharp.ImportUrdf
{
    public class ImportUrdfGeometryCollision : ImportUrdfGeometry
    {
        public static void Create(Transform parent, GeometryTypes geometryType, Link.Geometry geometry = null)
        {
            GameObject geometryGameObject = null;

            switch (geometryType)
            {
                case GeometryTypes.Box:
                    geometryGameObject = new GameObject(geometryType.ToString());
                    geometryGameObject.AddComponent<BoxCollider>();
                    break;
                case GeometryTypes.Cylinder:
                    geometryGameObject = CreateCylinderCollider();
                    break;
                case GeometryTypes.Sphere:
                    geometryGameObject = new GameObject(geometryType.ToString());
                    geometryGameObject.AddComponent<SphereCollider>();
                    break;
                case GeometryTypes.Mesh:
                    if (geometry != null)
                        geometryGameObject = CreateMeshCollider(geometry.mesh, true);
                    else
                    {
                        geometryGameObject = new GameObject(geometryType.ToString());
                        geometryGameObject.AddComponent<MeshCollider>();
                    }
                    break;
            }

            if(geometryGameObject != null)
            {
                geometryGameObject.transform.SetParentAndAlign(parent);
                if (geometry != null)
                    SetScale(parent, geometry, geometryType);
            }
        }

        // Modified this function to use STL Importer instead of AssetDatabase loader
        private static GameObject CreateMeshCollider(Link.Geometry.Mesh mesh, bool setConvex = false)
        {
            if (!mesh.filename.StartsWith(@"package://"))
            {
                Debug.LogWarning(mesh.filename + " is not a valid URDF package file path. Path should start with \"package://\".");
                return null;
            }

            // Concatenate path from the URDF file for the STL with the path to the directory on the file system
            var path = Path.Combine(RuntimeImportManager.fullPath, mesh.filename.Substring(10).SetSeparatorChar());

            // Import the STL as a mesh to be added to a new GameObject
            // Name is the file name of the .stl file
            Mesh meshNew = StlImporter.ImportOneMesh(path);
            meshNew.name = Path.GetFileNameWithoutExtension(path);

            // Create the new GameObject for the mesh, add the MeshCollider component, and set sharedMesh to the imported mesh
            GameObject meshObject = new GameObject(Path.GetFileNameWithoutExtension(path));
            MeshCollider meshCollider = meshObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshNew;

            meshCollider.convex = setConvex;

            return mesh == null ? null : meshObject;
        }

        private static GameObject CreateCylinderCollider()
        {
            GameObject gameObject = new GameObject("Cylinder");
            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();

            Link.Geometry.Cylinder cylinder = new Link.Geometry.Cylinder(0.5, 2); //Default unity cylinder sizes

            meshCollider.sharedMesh = CreateCylinderMesh(cylinder);
            meshCollider.convex = true;

            return gameObject;
        }
    }
}