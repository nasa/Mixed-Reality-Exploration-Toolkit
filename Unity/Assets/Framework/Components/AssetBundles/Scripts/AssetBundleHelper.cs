// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GSFC.ARVR.MRET.Infrastructure.Components.AssetBundles
{
    /// <remarks>
    /// History:
    /// 22 January 2021: Created
    /// 25 March 2021: Added support to load asset bundle dependencies obtained from the streaming
    ///     assets manifest. Migrated to using MRETBehaviour and added documentation. (J. Hosler)
    /// </remarks>
	///
	/// <summary>
	/// AssetBundleHelper
	///
	/// Asset bundle helper class providing asset bundle management and functions for loading assets
    /// 
    /// Author: Dylan Baker
	/// </summary>
	/// 
    public class AssetBundleHelper : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(AssetBundleHelper);

        public static AssetBundleHelper instance;

        private Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();

        private List<string> loadingBundles = new List<string>();

        private string streamingAssetsUri = "";
        private string manifestUri = "";
        private AssetBundleManifest manifest;

        /// <seealso cref="MRETUpdateBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) ||
                (manifest == null)
                    ? IntegrityState.Failure   // Fail if base class fails, OR manifest is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            // Assign the instance
            instance = this;

            // Build the URIs we will use to obtain out streaming assets and manifest
            streamingAssetsUri = Application.streamingAssetsPath + "/Windows";
            manifestUri = streamingAssetsUri + "/Windows";
            var manifestBundle = AssetBundle.LoadFromFile(manifestUri);
            if (manifestBundle)
            {
                // Obtain the manifest: https://forum.unity.com/threads/help-to-load-manifest.538700/
                manifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }
            else
            {
                // Report the issue
                Debug.LogError(DateTime.Now.ToString() +
                    " [" + ClassName + "] Streaming asset manifest file could not be located for asset bundle dependency processing: " +
                    manifestUri);
            }
        }

        /// <summary>
        /// Clears out the loaded bundles list and unloads all bundles.
        /// </summary>
        public void UnloadAllBundles()
        {
            foreach (KeyValuePair<string, AssetBundle> bdl in loadedBundles)
            {
                bdl.Value.Unload(false);
            }

            loadedBundles = new Dictionary<string, AssetBundle>();
        }

        /// <summary>
        /// Loads an asset from an asset bundle with caching.
        /// </summary>
        /// <param name="assetBundlePath">Full path to asset bundle.</param>
        /// <param name="assetName">Name of prefab in asset bundle.</param>
        /// <param name="assetType">Type of prefab in asset bundle.</param>
        /// <param name="onFinished">Called when asset is loaded.</param>
        public void LoadAssetAsync(string assetBundlePath, string assetName, Type assetType, Action<object> onFinished)
        {
            StartCoroutine(LoadAssetAsyncInternal(assetBundlePath, assetName, assetType, onFinished));
        }

        /// <summary>
        /// Loads a scene from an asset bundle with caching.
        /// </summary>
        /// <param name="assetBundlePath">Full path to asset bundle.</param>
        /// <param name="sceneName">Name of scene in asset bundle.</param>
        /// <param name="additive">Whether or not to load scene additively.</param>
        /// <param name="onFinished">Called when scene is loaded.</param>
        public void LoadSceneAsync(string assetBundlePath, string sceneName, bool additive, Action<bool> onFinished)
        {
            StartCoroutine(LoadSceneAsyncInternal(assetBundlePath, sceneName, additive, onFinished));
        }

        /// <summary>
        /// Loads an asset bundle asychronously, managing the bundles loading and loaded.
        /// </summary>
        /// <param name="assetBundlePath">Full path to asset bundle.</param>
        /// <param name="onFinished">Called when the bundle is loaded (true), or fails (false).</param>
        private IEnumerator LoadAssetBundleAsyncInternal(string assetBundlePath, Action<bool> onFinished)
        {
            // Wait for bundle to be loaded if it is in progress
            while (loadingBundles.Contains(assetBundlePath))
            {
                yield return null;
            }

            // Get asset bundle
            if (!loadedBundles.ContainsKey(assetBundlePath))
            {
                // Begin loading the asset bundle into cache
                Debug.Log(DateTime.Now.ToString() + " [" + ClassName + "] Bundle "
                    + assetBundlePath + " not found in cache. Loading from file.");
                loadingBundles.Add(assetBundlePath);

                AssetBundleCreateRequest cReq = AssetBundle.LoadFromFileAsync(assetBundlePath);
                yield return cReq;

                AssetBundle bundle = cReq.assetBundle;
                if (bundle == null)
                {
                    Debug.LogWarning(DateTime.Now.ToString()
                        + " [" + ClassName + "] Failed to load bundle. "
                        + assetBundlePath + " Aborting.");

                    onFinished.Invoke(false);
                    yield break;
                }

                // Process all the dependencies if we have a manifest
                if (manifest)
                {
                    string[] dependencies = manifest.GetAllDependencies(bundle.name);
                    foreach (string dependency in dependencies)
                    {
                        void action(bool loaded)
                        {
                            if (loaded == false)
                            {
                                Debug.LogError("[" + ClassName + "] Error loading dependency: " + dependency);
                            }
                        }

                        // Recursively load the dependency
                        StartCoroutine(LoadAssetBundleAsyncInternal(streamingAssetsUri + "/" + dependency, action));
                    }
                }

                // Store the bundle and mark as loaded
                loadedBundles.Add(assetBundlePath, bundle);
                loadingBundles.Remove(assetBundlePath);
            }

            // Finished successfully
            onFinished.Invoke(true);
        }

        /// <summary>
        /// Loads asset from the supplied asset bundle path and supplies the asset to the provided
        /// <code>Action</code> delegate, or null on error.
        /// </summary>
        /// <param name="assetBundlePath">Full path to asset bundle.</param>
        /// <param name="assetName">Name of asset in asset bundle.</param>
        /// <param name="assetType">Type of asset in asset bundle, i.e. GameObject, Material, etc.</param>
        /// <param name="onFinished">Called when asset is loaded, providing the asset reference.</param>
        private IEnumerator LoadAssetAsyncInternal(string assetBundlePath, string assetName, Type assetType, Action<object> onFinished)
        {
            bool error = false;
            void action(bool loaded)
            {
                error = !loaded;
                if (error)
                {
                    Debug.LogError("[" + ClassName + "] Error loading asset: " + assetName);
                }
            }

            // Load the dependency
            StartCoroutine(LoadAssetBundleAsyncInternal(assetBundlePath, action));

            // Wait until the bundle is loaded or an error is encountered
            while (!loadedBundles.ContainsKey(assetBundlePath))
            {
                // Make sure we didn't get an error loading the asset bundle
                if (error)
                {
                    onFinished.Invoke(null);
                    yield break;
                }
                else
                {
                    yield return null;
                }
            }

            // Get the asset bundle
            AssetBundle bundleWithAsset = loadedBundles[assetBundlePath];

            // Get the asset from asset bundle
            Debug.Log(DateTime.Now.ToString() + " [" + ClassName + "] Loading "
                    + assetName + " from bundle " + assetBundlePath + ".");

            object invokeObj = null;
            AssetBundleRequest lReq;
            if (assetType == typeof(GameObject))
            {
                lReq = bundleWithAsset.LoadAssetAsync<GameObject>(assetName);
                yield return lReq;

                if (lReq.asset != null)
                {
                    GameObject result = Instantiate(lReq.asset as GameObject);
                    invokeObj = result;
                }
            }
            else if (assetType == typeof(Material))
            {
                lReq = bundleWithAsset.LoadAssetAsync<Material>(assetName);
                yield return lReq;

                if (lReq.asset != null)
                {
                    Material result = Instantiate(lReq.asset as Material);
                    invokeObj = result;
                }
            }
            else
            {
                Debug.LogWarning(DateTime.Now.ToString() +
                    " [" + ClassName + "] Unknown asset type " + assetType + ".");
            }

            if (invokeObj == null)
            {
                Debug.LogWarning(DateTime.Now.ToString() +
                    " [" + ClassName + "] Failed to load " + assetName + " from " + assetBundlePath + ".");
            }
            else
            {
                Debug.Log(DateTime.Now.ToString() + " [" + ClassName + "] " + assetName + " loaded.");
            }

            // Supply the reference to the action delegate
            onFinished.Invoke(invokeObj);
        }

        /// <summary>
        /// Loads the scene from the supplied asset bundle path and indicates successful loading to the provided
        /// <code>Action</code> delegate.
        /// </summary>
        /// <param name="assetBundlePath">Full path to asset bundle.</param>
        /// <param name="sceneName">Name of scene to load.</param>
        /// <param name="additive">Whether or not to load scene additively.</param>
        /// <param name="onFinished">Called when scene is loaded, supplying a boolean value indicating success.</param>
        private IEnumerator LoadSceneAsyncInternal(string assetBundlePath, string sceneName, bool additive, Action<bool> onFinished)
        {
            bool error = false;
            void action(bool loaded)
            {
                error = !loaded;
                if (error)
                {
                    Debug.LogError("[" + ClassName + "] Error loading scene: " + sceneName);
                }
            }

            // Load the dependency
            StartCoroutine(LoadAssetBundleAsyncInternal(assetBundlePath, action));

            // Wait until the bundle is loaded or an error is encountered
            while (!loadedBundles.ContainsKey(assetBundlePath))
            {
                // Make sure we didn't get an error loading the asset bundle
                if (error)
                {
                    onFinished.Invoke(false);
                    yield break;
                }
                else
                {
                    yield return null;
                }
            }

            // Load the scene from the asset bundle
            Debug.Log(DateTime.Now.ToString() +
                " [" + ClassName + "] Loading " + sceneName + " from bundle " + assetBundlePath + ".");
            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);

            // Wait for scene to load
            while (!load.isDone)
            {
                yield return null;
            }

            // Notify the action delegate
            Debug.Log(DateTime.Now.ToString() +
                " [" + ClassName + "] Finished loading scene: " + sceneName);
            onFinished.Invoke(true);
        }
    }
}