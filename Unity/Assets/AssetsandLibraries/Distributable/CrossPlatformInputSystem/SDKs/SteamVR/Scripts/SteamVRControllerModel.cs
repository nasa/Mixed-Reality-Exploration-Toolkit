// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Dummiesman;

namespace GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem.SDK
{
    /// <remarks>
    /// History:
    /// 27 October 2020: Created
    /// </remarks>
    /// <summary>
    /// Loads controller models from SteamVR render model folder.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class SteamVRControllerModel : MonoBehaviour
    {
#region JSON
        private class SteamVRControllerModelInfo
        {
            public class SteamVRControllerModelComponentLocal
            {
                public float[] origin;
                public float[] rotate_xyz;
            }

            public class SteamVRControllerModelMotion
            {
                public string type;
                public int controller_axis;
                public int controller_axis_component;
                public int controller_button;
                public string component_path;
                public string x_path;
                public float[] center;
                public float[] rotate_xyz;
                public float[] press_rotation_x;
                public float[] press_rotation_y;
                public float[] press_rotation_translate;
                public float[] touch_translate;
                public float[] touch_translate_x;
                public float[] touch_translate_y;
                public float[] value_mapping;
                public float[] pivot;
                public float[] axis;
            }

            public class SteamVRControllerModelVisibility
            {
                [JsonProperty("default")]
                public bool def;
                public bool scroll;
            }

            public class SteamVRControllerModelComponentInfo
            {
                public string filename;
                public int match_priority;
                public SteamVRControllerModelMotion motion;
                public SteamVRControllerModelComponentLocal component_local;
                public SteamVRControllerModelVisibility visibility;
            }

            public string thumbnail;
            public Dictionary<string, SteamVRControllerModelComponentInfo> components;
        }
#endregion

        /// <summary>
        /// Name of the controller.
        /// </summary>
        [Tooltip("Name of the controller.")]
        public string controllerName;

        void Start()
        {
            ReadJSON();
        }

        /// <summary>
        /// Read the controller model from JSON and load it.
        /// </summary>
        private void ReadJSON()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {   // Windows.
                string x86Path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                if (string.IsNullOrEmpty(x86Path))
                {
                    Debug.LogError("[SteamVRControllerModel->ReadJSON] Unable to find x86 path.");
                    return;
                }

                string steamVRPath = Path.Combine(x86Path, "Steam/steamapps/common/SteamVR");
                if (!Directory.Exists(steamVRPath))
                {
                    Debug.LogError("[SteamVRControllerModel->ReadJSON] Unable to find SteamVR path.");
                    return;
                }

                string jsonPath = Path.Combine(steamVRPath, "resources/rendermodels/" + controllerName + "/" + controllerName + ".json");
                if (!File.Exists(jsonPath))
                {
                    Debug.LogError("[SteamVRControllerModel->ReadJSON] Unable to find JSON path.");
                    return;
                }

                string rawJSON = File.ReadAllText(jsonPath);
                rawJSON = rawJSON.Remove(rawJSON.LastIndexOf("}"), 1);
                SetUpControllerModel(JsonConvert.DeserializeObject<SteamVRControllerModelInfo>(rawJSON));
            }
        }

        /// <summary>
        /// Set up properties of the controller model.
        /// </summary>
        /// <param name="info">Controller model infomation.</param>
        private void SetUpControllerModel(SteamVRControllerModelInfo info)
        {
            if (info.components == null)
            {
                return;
            }

            foreach (KeyValuePair<string, SteamVRControllerModelInfo.SteamVRControllerModelComponentInfo> componentInfo in info.components)
            {
                if (componentInfo.Value == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(componentInfo.Value.filename))
                {
                    continue;
                }

                GameObject component = LoadModel(controllerName, componentInfo.Value.filename, componentInfo.Value.filename.Replace(".obj", ".mtl"));
                if (component != null)
                {
                    component.transform.SetParent(transform);
                    component.transform.localPosition = Vector3.zero;
                    component.transform.localRotation = Quaternion.identity;

                    if (componentInfo.Value.visibility != null)
                    {
                        component.SetActive(componentInfo.Value.visibility.def);
                    }
                    continue;
                }
            }
        }

        /// <summary>
        /// Load an obj model.
        /// </summary>
        /// <param name="modelName">Name of the model.</param>
        /// <param name="modelFile">Model file name.</param>
        /// <param name="materialFile">Material file name.</param>
        /// <returns>The loaded gameobject or null.</returns>
        private GameObject LoadModel(string modelName, string modelFile, string materialFile)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {   // Windows.
                string x86Path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                if (string.IsNullOrEmpty(x86Path))
                {
                    Debug.LogError("[SteamVRControllerModel->LoadModel] Unable to find x86 path.");
                    return null;
                }

                string steamVRPath = Path.Combine(x86Path, "Steam/steamapps/common/SteamVR");
                if (!Directory.Exists(steamVRPath))
                {
                    Debug.LogError("[SteamVRControllerModel->LoadModel] Unable to find SteamVR path.");
                    return null;
                }

                string modelPath = Path.Combine(steamVRPath, "resources/rendermodels/" + modelName + "/" + modelFile);
                if (!File.Exists(modelPath))
                {
                    Debug.LogError("[SteamVRControllerModel->LoadModel] Unable to find model path.");
                    return null;
                }

                string materialPath = Path.Combine(steamVRPath, "resources/rendermodels/" + modelName + "/" + materialFile);
                if (!File.Exists(materialPath))
                {
                    Debug.LogError("[SteamVRControllerModel->LoadModel] Unable to find material path.");
                }

                return new OBJLoader().Load(modelPath, materialPath);
            }

            return null;
        }
    }
}