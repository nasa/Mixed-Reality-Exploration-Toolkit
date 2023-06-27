// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEditor;
using GOV.NASA.GSFC.XR.XRUI.WorldSpaceMenu;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.XRUI
{
    /// <remarks>
    /// History:
    /// 6 April 2021: Created
    /// </remarks>
    /// <summary>
    /// Contains Unity Editor tools for MRET UI.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class UIMenu : MonoBehaviour
    {
        /// <summary>
        /// Returns the path to the XRUI directory.
        /// </summary>
        static string XRUIRootDirectory
        {
            get
            {
                // TODO: There has to be a better way to get the path.
                GameObject tempObject = new GameObject();
                WorldSpaceMenuManager instance = tempObject.AddComponent<WorldSpaceMenuManager>();
                string scriptLocation = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(instance));
                DestroyImmediate(tempObject);

                return System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(scriptLocation));
            }
        }

        /// <summary>
        /// Returns the path to the XRUI prefabs directory.
        /// </summary>
        static string XRUIPrefabsDirectory
        {
            get
            {
                return System.IO.Path.Combine(XRUIRootDirectory, "Prefabs");
            }
        }

        /// <summary>
        /// Instantiates a prefab.
        /// </summary>
        /// <param name="prefabPath">Path to prefab to instantiate.</param>
        /// <param name="instanceName">Name to apply to the instance.</param>
        /// <returns></returns>
        static GameObject InstantiatePrefabObject(string prefabPath, string instanceName)
        {
            Object prefabObject = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
            if (prefabObject == null)
            {
                Debug.LogError("[XRUI] Prefab not found. Check your installation.");
                return null;
            }

            GameObject instance = (GameObject) Instantiate(prefabObject);
            if (instance != null)
            {
                instance.name = instanceName;
            }

            return instance;
        }

        /// <summary>
        /// Instantiates an XRUI prefab.
        /// </summary>
        /// <param name="prefabName">Name of prefab to instantiate.</param>
        /// <param name="instanceName">Name to apply to the instance</param>
        /// <returns></returns>
        static GameObject InstantiateXRUIPrefabObject(string prefabName, string instanceName)
        {
            return InstantiatePrefabObject(System.IO.Path.Combine(
                XRUIPrefabsDirectory, prefabName), instanceName);
        }

        /// <summary>
        /// Instantiates a standard menu panel.
        /// </summary>
        [MenuItem("MRET/UI/World Space Menu/Standard")]
        static void CreateWorldSpaceMenuPanel()
        {
            InstantiateXRUIPrefabObject("StandardPanel.prefab", "Standard Panel");
        }

        /// <summary>
        /// Instantiates a wide menu panel.
        /// </summary>
        [MenuItem("MRET/UI/World Space Menu/Wide")]
        static void CreateWideWorldSpaceMenuPanel()
        {
            InstantiateXRUIPrefabObject("WidePanel.prefab", "Wide Panel");
        }

        /// <summary>
        /// Instantiates a file browser panel.
        /// </summary>
        [MenuItem("MRET/UI/World Space Menu/File Browser")]
        static void CreateFileBrowserPanel()
        {
            InstantiatePrefabObject(
                "Assets/MRET/UI/Tools/FileBrowser/Assets/Prefabs/FileBrowser.prefab",
                "File Browser");
        }
    }
}