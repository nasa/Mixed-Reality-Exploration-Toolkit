// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
#if MRET_EXTENSION_SICCITYGLTF
using Siccity.GLTFUtility;
#endif

namespace GOV.NASA.GSFC.XR.MRET.Extensions.GLTF
{
    /// <remarks>
    /// History:
    /// 18 February 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// GltfExt
	///
	/// MRET GLTF extensions
	///
    /// Author: Dylan Z. Baker
	/// </summary>
	/// 
	public class GltfExt : MRETSingleton<GltfExt>
    {
        /// <summary>
        /// X coordinate of the location to cache model prefabs.
        /// </summary>
        private const float CACHEXLOCATION = -9999;

        /// <summary>
        /// Y coordinate of the location to cache model prefabs.
        /// </summary>
        private const float CACHEYLOCATION = -9999;

        /// <summary>
        /// Z coordinate of the location to cache model prefabs.
        /// </summary>
        private const float CACHEZLOCATION = -9999;

		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(GltfExt);

        /// <summary>
        /// Container for loaded model prefabs.
        /// </summary>
        public GameObject modelPrefabHolder;

        /// <summary>
        /// Dictionary of loaded gameobject mode prefabs.
        /// </summary>
        private Dictionary<string, GameObject> loadedGameObjects = new Dictionary<string, GameObject>();

		/// <seealso cref="MRETBehaviour.IntegrityCheck"/>
		protected override IntegrityState IntegrityCheck()
		{
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            if (state == IntegrityState.Success)
            {
                if (modelPrefabHolder == null)
                {
                    Debug.LogError("[" + ClassName + "->IntegrityCheck] Model Prefab Holder not assigned.");
                }
                else
                {
                    return IntegrityState.Success;
                }
            }
            return IntegrityState.Failure;
		}

        /// <summary>
        /// Load a GLTF model.
        /// </summary>
        /// <param name="gltfModelPath">File path</param>
        /// <param name="onFinished">Action to perform when finished.</param>
        /// <param name="settings">Import settings.</param>
		public void LoadGLTF(string gltfModelPath, Action<GameObject, AnimationClip[]> onFinished
#if MRET_EXTENSION_SICCITYGLTF
            , ImportSettings settings = null
#endif
            )
        {
            // Already loaded.
            if (loadedGameObjects.ContainsKey(gltfModelPath))
            {
                InstantiateGLTF(gltfModelPath, null, onFinished);
                return;
            }

            // Setup the action callback
            Action<GameObject, AnimationClip[]> callback = (GameObject gobj, AnimationClip[] acl) =>
            {
                OnGLTFLoad(gltfModelPath, gobj, acl, onFinished);
            };

#if MRET_EXTENSION_SICCITYGLTF
            // Make sure to have valid import settings
            if (settings == null)
            {
                settings = new ImportSettings();
            }

            // Need to load new instance.
            Importer.ImportGLTFAsync(gltfModelPath, settings, onFinished);
#else
            onFinished?.Invoke(null, null);
#endif
        }

        /// <summary>
        /// Load a GLTF from a byte array.
        /// </summary>
        /// <param name="key">The unique key identified of the GLTF</param>
        /// <param name="gltfBytes">The <code>byte[]</code> array containing the GLTF data</param>
        /// <param name="onFinished">Action to perform when finished.</param>
        /// <param name="settings">Import settings.</param>
		public async void LoadGLTF(string key, byte[] gltfBytes, Action<GameObject, AnimationClip[]> onFinished
#if MRET_EXTENSION_SICCITYGLTF
            , ImportSettings settings = null
#endif
            )
        {
            // Load as an asychronous task
            await Task.Run(() =>
            {
                try
                {
                    // Already loaded
                    if (loadedGameObjects.ContainsKey(key))
                    {
                        InstantiateGLTF(key, null, onFinished);
                        return;
                    }

#if MRET_EXTENSION_SICCITYGLTF
                    // Make sure to have valid import settings
                    if (settings == null)
                    {
                        settings = new ImportSettings();
                    }

                    // Load from bytes
                    GameObject gltfGO = Importer.LoadFromBytes(gltfBytes, settings, out AnimationClip[] acl);
#else
                    GameObject gltfGO = null;
                    AnimationClip[] acl = null;
#endif
                    OnGLTFLoad(key, gltfGO, acl, onFinished);
                }
                catch (Exception e)
                {
                    LogError("A problem occurred attempting to load the '" + key + "' GLTF from the byte array: " + e.Message);
                    onFinished?.Invoke(null, null);
                }

            });
        }

        /// <summary>
        /// Called when a new GLTF is loaded.
        /// </summary>
        /// <param name="gltfModelPath">Path of the file that was loaded.</param>
        /// <param name="loaded">Loaded object.</param>
        /// <param name="clips">Animation clips.</param>
        /// <param name="callback">Callback to call.</param>
        public void OnGLTFLoad(string gltfModelPath, GameObject loaded, AnimationClip[] clips, Action<GameObject, AnimationClip[]> callback)
        {
            if (loaded != null)
            {
                loadedGameObjects.Add(gltfModelPath, loaded);
                loaded.transform.position = new Vector3(CACHEXLOCATION, CACHEYLOCATION, CACHEZLOCATION);
                loaded.transform.SetParent(modelPrefabHolder.transform);
                loaded.SetActive(false);
                InstantiateGLTF(gltfModelPath, clips, callback);
            }
            else
            {
                callback?.Invoke(null, null);
            }
        }

        /// <summary>
        /// Called to instantiate a GLTF model.
        /// </summary>
        /// <param name="gltfModelPath">Path of the loaded model.</param>
        /// <param name="clips">Animation clips.</param>
        /// <param name="onFinished">Callback to call.</param>
        private void InstantiateGLTF(string gltfModelPath, AnimationClip[] clips, Action<GameObject, AnimationClip[]> onFinished)
        {
            GameObject goInstance = Instantiate(loadedGameObjects[gltfModelPath]);
            goInstance.SetActive(true);
            onFinished?.Invoke(goInstance, clips);
        }
    }
}