// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GSFC.ARVR.MRET.Infrastructure.Components.AssetBundles
{
    public class AssetBundleHelper : MonoBehaviour
    {
        public static AssetBundleHelper instance;

        private Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();

        private List<string> loadingBundles = new List<string>();

        void Start()
        {
            instance = this;
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
        /// Loads asset from an asset bundle with caching.
        /// </summary>
        /// <param name="assetBundlePath">Full path to asset bundle.</param>
        /// <param name="prefabName">Name of prefab in asset bundle.</param>
        /// <param name="onFinished">Called when asset is loaded.</param>
        public void LoadAssetAsync(string assetBundlePath, string prefabName, Type objectType, Action<object> onFinished)
        {
            StartCoroutine(LoadAssetAsyncInternal(assetBundlePath, prefabName, objectType, onFinished));
        }

        /// <summary>
        /// Loads asset from an asset bundle with caching.
        /// </summary>
        /// <param name="assetBundlePath">Full path to asset bundle.</param>
        /// <param name="prefabName">Name of scene in asset bundle.</param>
        /// <param name="additive">Whether or not to load scene additively.</param>
        /// <param name="onFinished">Called when scene is loaded.</param>
        public void LoadSceneAsync(string assetBundlePath, string prefabName, bool additive, Action<bool> onFinished)
        {
            StartCoroutine(LoadSceneAsyncInternal(assetBundlePath, prefabName, additive, onFinished));
        }

        private IEnumerator LoadAssetAsyncInternal(string assetBundlePath, string objectName, Type objectType, Action<object> onFinished)
        {
            // Wait for bundle to be loaded if it is in progress.
            while (loadingBundles.Contains(assetBundlePath))
            {
                yield return null;
            }

            // Get asset bundle.
            AssetBundle bundleWithAsset = null;
            if (loadedBundles.ContainsKey(assetBundlePath))
            {
                Debug.Log(DateTime.Now.ToString() + " [AssetBundleHelper] Bundle "
                    + assetBundlePath + " already cached. Using cached bundle.");

                bundleWithAsset = loadedBundles[assetBundlePath];
            }
            else
            {
                Debug.Log(DateTime.Now.ToString() + " [AssetBundleHelper] Bundle "
                    + assetBundlePath + " not found in cache. Loading from file.");

                loadingBundles.Add(assetBundlePath);
                AssetBundleCreateRequest cReq = AssetBundle.LoadFromFileAsync(assetBundlePath);
                yield return cReq;

                bundleWithAsset = cReq.assetBundle;
                if (bundleWithAsset == null)
                {
                    Debug.Log(DateTime.Now.ToString()
                        + " [AssetBundleHelper] Failed to load bundle. "
                        + assetBundlePath + " Aborting.");

                    onFinished.Invoke(null);
                    yield break;
                }

                loadedBundles.Add(assetBundlePath, bundleWithAsset);
                loadingBundles.Remove(assetBundlePath);
            }

            // Get asset from asset bundle.
            Debug.Log(DateTime.Now.ToString() + " [AssetBundleHelper] Loading "
                    + objectName + " from bundle " + assetBundlePath + ".");

            object invokeObj = null;
            AssetBundleRequest lReq;
            if (objectType == typeof(GameObject))
            {
                lReq = bundleWithAsset.LoadAssetAsync<GameObject>(objectName);
                yield return lReq;

                if (lReq.asset != null)
                {
                    GameObject result = Instantiate(lReq.asset as GameObject);
                    invokeObj = result;
                }
            }
            else if (objectType == typeof(Material))
            {
                lReq = bundleWithAsset.LoadAssetAsync<Material>(objectName);
                yield return lReq;

                if (lReq.asset != null)
                {
                    Material result = Instantiate(lReq.asset as Material);
                    invokeObj = result;
                }
            }
            else
            {
                Debug.LogWarning(DateTime.Now.ToString() + " [AssetBundleHelper] Unknown objectType "
                    + objectType + ".");
            }

            if (invokeObj == null)
            {
                Debug.LogWarning(DateTime.Now.ToString() + " [AssetBundleHelper] Failed to load "
                    + objectName + " from " + assetBundlePath + ".");
            }
            else
            {
                Debug.Log(DateTime.Now.ToString() + " [AssetBundleHelper] " + objectName + " loaded.");
            }

            onFinished.Invoke(invokeObj);
        }

        private IEnumerator LoadSceneAsyncInternal(string assetBundlePath, string sceneName, bool additive, Action<bool> onFinished)
        {
            // Wait for bundle to be loaded if it is in progress.
            while (loadingBundles.Contains(assetBundlePath))
            {
                yield return null;
            }

            // Get asset bundle.
            AssetBundle bundleWithAsset = null;
            if (loadedBundles.ContainsKey(assetBundlePath))
            {
                Debug.Log(DateTime.Now.ToString() + " [AssetBundleHelper] Bundle "
                    + assetBundlePath + " already cached. Using cached bundle.");

                bundleWithAsset = loadedBundles[assetBundlePath];
            }
            else
            {
                Debug.Log(DateTime.Now.ToString() + " [AssetBundleHelper] Bundle "
                    + assetBundlePath + " not found in cache. Loading from file.");

                loadingBundles.Add(assetBundlePath);
                AssetBundleCreateRequest cReq = AssetBundle.LoadFromFileAsync(assetBundlePath);
                yield return cReq;

                bundleWithAsset = cReq.assetBundle;
                if (bundleWithAsset == null)
                {
                    Debug.Log(DateTime.Now.ToString()
                        + " [AssetBundleHelper] Failed to load bundle. "
                        + assetBundlePath + " Aborting.");

                    onFinished.Invoke(false);
                    yield break;
                }

                loadedBundles.Add(assetBundlePath, bundleWithAsset);
                loadingBundles.Remove(assetBundlePath);
            }

            // Get asset from asset bundle.
            Debug.Log(DateTime.Now.ToString() + " [AssetBundleHelper] Loading "
                    + sceneName + " from bundle " + assetBundlePath + ".");

            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);

            // Wait for scene to load.
            while (!load.isDone)
            {
                yield return null;
            }

            Debug.Log(DateTime.Now.ToString()
                        + " [AssetBundleHelper] Finished loading scene "
                        + sceneName + ".");

            onFinished.Invoke(true);
        }
    }
}