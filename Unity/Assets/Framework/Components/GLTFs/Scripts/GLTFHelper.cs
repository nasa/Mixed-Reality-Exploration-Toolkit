// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using Siccity.GLTFUtility;

namespace GSFC.ARVR.MRET.Infrastructure.Components.GLTFs
{
    /// <remarks>
    /// History:
    /// 18 February 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// GLTFHelper
	///
	/// Helper class for GLTF model handling.
	///
    /// Author: Dylan Z. Baker
	/// </summary>
	/// 
	public class GLTFHelper : MRETBehaviour
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
		public override string ClassName
		{
			get
			{
				return nameof(GLTFHelper);
			}
		}

        /// <summary>
        /// Instance of the helper.
        /// </summary>
        public static GLTFHelper instance;

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

		/// <seealso cref="MRETBehaviour.MRETAwake"/>
		protected override void MRETAwake()
		{
			// Take the inherited behavior
			base.MRETAwake();

            instance = this;
		}
		
        /// <summary>
        /// Load a GLTF model.
        /// </summary>
        /// <param name="gltfModelPath">File path</param>
        /// <param name="onFinished">Action to perform when finished.</param>
        /// <param name="settings">Import settings.</param>
		public void LoadGLTF(string gltfModelPath, Action<GameObject, AnimationClip[]> onFinished, ImportSettings settings)
        {
            // Already loaded.
            if (loadedGameObjects.ContainsKey(gltfModelPath))
            {
                InstantiateGLTF(gltfModelPath, onFinished);
                return;
            }

            // Need to load new instance.
            Action<GameObject, AnimationClip[]> callback = (GameObject gobj, AnimationClip[] acl) => { OnGLTFLoad(gltfModelPath, gobj, acl, onFinished); };
            Importer.ImportGLTFAsync(gltfModelPath, settings, onFinished);
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
            loadedGameObjects.Add(gltfModelPath, loaded);
            loaded.transform.position = new Vector3(CACHEXLOCATION, CACHEYLOCATION, CACHEZLOCATION);
            loaded.transform.SetParent(modelPrefabHolder.transform);
            loaded.SetActive(false);
            InstantiateGLTF(gltfModelPath, callback);
        }

        /// <summary>
        /// Called to instantiate a GLTF model.
        /// </summary>
        /// <param name="gltfModelPath">Path of the loaded model.</param>
        /// <param name="onFinished">Callback to call.</param>
        private void InstantiateGLTF(string gltfModelPath, Action<GameObject, AnimationClip[]> onFinished)
        {
            GameObject goInstance = Instantiate(loadedGameObjects[gltfModelPath]);
            goInstance.SetActive(true);
            onFinished.Invoke(goInstance, null);
        }
	}
}