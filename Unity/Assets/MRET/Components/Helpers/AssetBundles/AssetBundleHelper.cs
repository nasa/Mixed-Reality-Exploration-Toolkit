// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET.Helpers
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
	/// Asset bundle helper class providing asset bundle management and functions
    /// for loading assets
    /// 
    /// Author: Dylan Baker
	/// </summary>
	/// 
    public class AssetBundleHelper : MRETSingleton<AssetBundleHelper>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(AssetBundleHelper);

        private string streamingAssetsUri = "";
        private string manifestUri = "";
        private AssetBundleManifest manifest;
        private Queue<object> _asynchAssetLoaders = new Queue<object>();
        private Queue<object> _asynchSceneLoaders = new Queue<object>();

        private Dictionary<string, Bundle> loadedBundles = new Dictionary<string, Bundle>();

        /// <summary>
        /// Internal bundle class for caching and accessing asset bundles
        /// </summary>
        public class Bundle
        {
            private string key = null;
            private bool needsLoading = true;
            private bool stillLoading = false;
            private bool error = false;
            private string errorMessage = null;
            private AssetBundle asset = null;

            public string Name { get => HasAsset ? Asset.name : null; }
            public bool NeedsLoading { get => needsLoading; }
            public bool StillLoading { get => stillLoading; }
            public bool StartLoading() => stillLoading = true;
            public bool FinishedLoading
            {
                get
                {
                    bool result = HasAsset || HasError(false);
                    if (result)
                    {
                        string[] bundleDependencyNames = Instance.manifest.GetAllDependencies(Name);
                        foreach (string dependencyName in bundleDependencyNames)
                        {
                            result &= Instance.GetBundle(dependencyName).FinishedLoading;
                        }
                    }
                    return result;
                }
            }

            private bool HasDependencyError
            {
                get
                {
                    bool result = false;
                    string[] bundleDependencyNames = Instance.manifest.GetAllDependencies(Name);
                    foreach (string dependencyName in bundleDependencyNames)
                    {
                        result |= Instance.GetBundle(dependencyName).HasDependencyError;
                    }
                    return result;
                }
            }

            public bool HasError(bool includeDependencies = true)
            {
                return error | (includeDependencies && HasDependencyError);
            }

            public string ErrorMessage { get => errorMessage; }
            public bool HasAsset { get => Asset != null; }
            public AssetBundle Asset { get => asset; }

            public Action<AssetBundle> LoadedAction => BundleLoadedAction;

            private void BundleLoadedAction(AssetBundle bundle)
            {
                asset = bundle;
                if (!HasAsset)
                {
                    Error("Error loading bundle: " + key);
                    Debug.LogError("********** " + ErrorMessage);
                }
                needsLoading = false;
                stillLoading = false;
            }

            /// <summary>
            /// Clears the serialization state
            /// </summary>
            public void Clear()
            {
                error = false;
                errorMessage = "";
                asset = null;
            }

            /// <summary>
            /// Changes the state to an error condition.
            /// </summary>
            /// <param name="message">The error message</param>
            /// <param name="complete">Indicates if the serialization should be marked as complete</param>
            public void Error(string message)
            {
                error = true;
                errorMessage = message;
                asset = null;
            }

            public Bundle(string assetBundleName)
            {
                key = assetBundleName;
                needsLoading = true;
                stillLoading = false;
            }
        }

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
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

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            // Build the URIs we will use to obtain out streaming assets and manifest
#if WINDOWS_UWP
            streamingAssetsUri = Windows.Storage.ApplicationData.Current.LocalFolder.Path + "/MRET/UWP";
            manifestUri = streamingAssetsUri + "/UWP";
#else
            streamingAssetsUri = MRET.ConfigurationManager.defaultAssetBundlesDirectory;
            manifestUri = Path.Combine(streamingAssetsUri, "Windows");
#endif
            var manifestBundle = AssetBundle.LoadFromFile(manifestUri);
            if (manifestBundle)
            {
                // Obtain the manifest: https://forum.unity.com/threads/help-to-load-manifest.538700/
                manifest = manifestBundle.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));
            }
            else
            {
                // Report the issue
                LogError("Streaming asset manifest file could not be located for asset bundle dependency processing: " +
                    manifestUri, nameof(MRETStart));
            }
        }

        /// <summary>
        /// Helper function for looking up and creating out internal bundle
        /// references.
        /// </summary>
        /// <param name="assetBundleName">The name of the asset bundle to query</param>
        /// <returns>A <code>Bundle</code> reference associated with the supplied asset
        ///     bundle name</returns>
        private Bundle GetBundle(string assetBundleName)
        {
            if (!loadedBundles.TryGetValue(assetBundleName, out Bundle result))
            {
                result = new Bundle(assetBundleName);
                loadedBundles.Add(assetBundleName, result);
            }
            return result;
        }

        /// <summary>
        /// A helper function to check to see if an asset bundle needs loading.
        /// </summary>
        /// <param name="assetBundleName">The asset bundle name to query</param>
        /// <returns>An indicator that the bundle needs loading</returns>
        private bool BundleNeedsLoading(string assetBundleName) => (GetBundle(assetBundleName).NeedsLoading);

        /// <summary>
        /// A helper function to check to see if an asset bundle is still loading.
        /// </summary>
        /// <param name="assetBundleName">The asset bundle name to query</param>
        /// <returns>An indicator that the bundle is still loading</returns>
        private bool BundleStillLoading(string assetBundleName) => (GetBundle(assetBundleName).StillLoading);

        /// <summary>
        /// Clears out the loaded bundles list and unloads all bundles.
        /// </summary>
        public void UnloadAllBundles()
        {
            foreach (KeyValuePair<string, Bundle> bundlePair in loadedBundles)
            {
                if (bundlePair.Value.Asset != null) bundlePair.Value.Asset.Unload(false);
            }

            loadedBundles.Clear();
        }

        /// <summary>
        /// Asychronously loads asset from the supplied asset bundle path. Loaded
        /// asset is supplied to the caller via the provided action delegate.
        /// </summary>
        /// <typeparam name="T">Type of asset to load, i.e. GameObject, Material, etc.</typeparam>
        /// <param name="assetBundlePath">Full path to asset bundle.</param>
        /// <param name="assetName">Name of asset in asset bundle.</param>
        /// <param name="onFinished"><code>Action</code> delegate containing the loaded asset.
        ///     On error, the supplied asset will be null.</param>
        public void LoadAssetAsync<T>(string assetBundlePath, string assetName, Action<T> onFinished)
            where T : UnityEngine.Object
        {
            StartCoroutine(LoadAssetAsyncInternal(assetBundlePath, assetName, onFinished));
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
        /// <param name="assetBundleName">Name of the asset bundle (excludes path).</param>
        /// <param name="onFinished">Called when the bundle is loaded (true), or fails (false).</param>
        private IEnumerator LoadAssetBundleAsyncInternal(string assetBundleName, Action<AssetBundle> onFinished)
        {
            // Jump out if the bundle is still loading
            while (BundleStillLoading(assetBundleName))
            {
                new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);
                yield return null;
            }

            // Log the cache
            string cacheList = "";
            foreach (string key in loadedBundles.Keys)
            {
                Bundle cachedBundle = GetBundle(key);
                cacheList += key + ";\t" +
                    "Needs Loading: " + cachedBundle.StillLoading + ";\t" +
                    "Has Asset: " + cachedBundle.HasAsset + ";\t" +
                    "Has Error: " + cachedBundle.HasError(false) + ";\t" +
                    "Has Error in Dependencies: " + cachedBundle.HasError() +
                    "\n";
            }
            Log("Asset bundle cache: \n" + cacheList, nameof(LoadAssetBundleAsyncInternal));

            // Get bundle reference
            Bundle bundle = GetBundle(assetBundleName);
            if (bundle.NeedsLoading)
            {
                // Begin loading the asset bundle into cache
                Log("Asset bundle " + assetBundleName + " not found in cache. Loading from file.",
                    nameof(LoadAssetBundleAsyncInternal));

                // Mark bundle for loading
                bundle.StartLoading();

                // Asychronously load the asset bundle
                string assetBundlePath = Path.Combine(streamingAssetsUri, assetBundleName);
                AssetBundleCreateRequest cReq = AssetBundle.LoadFromFileAsync(assetBundlePath);
                yield return cReq;

                AssetBundle assetBundle = cReq.assetBundle;
                if (assetBundle == null)
                {
                    string errorMessage = "Failed to load bundle. " + assetBundleName + " Aborting.";
                    LogWarning(errorMessage, nameof(LoadAssetBundleAsyncInternal));
                    bundle.Error(errorMessage);
                    loadedBundles.Remove(assetBundleName);
                    onFinished.Invoke(null);
                    yield break;
                }

                // Assign the asset bundle
                bundle.LoadedAction.Invoke(assetBundle);

                // Start coroutines for each dependency
                string[] bundleDependencyNames = manifest.GetAllDependencies(assetBundle.name);

                // Make sure the bundles are created before starting the dependency loading.
                // Can't happen in loop below or the dictionary may alter in the middle of the loop.
                foreach (string dependencyName in bundleDependencyNames) GetBundle(dependencyName);

                // Start the loading for each bundle
                foreach (string dependencyName in bundleDependencyNames)
                {
                    // Get the dependency bundle reference
                    Bundle dependency = GetBundle(dependencyName);

                    // Recursively load the dependency
                    StartCoroutine(LoadAssetBundleAsyncInternal(dependencyName, dependency.LoadedAction));
                }

                // Wait for dependency loading completion
                while (!bundle.FinishedLoading)
                {
                    new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);
                    yield return bundle;
                }

                // Check for error condition
                if (bundle.HasError())
                {
                    LogError("Error loading asset bundle: " + assetBundleName,
                        nameof(LoadAssetAsyncInternal));
                    onFinished.Invoke(null);
                    yield break;
                }

                Log("Asset bundle loaded and cached: " + assetBundleName, nameof(LoadAssetBundleAsyncInternal));
            }

            // Get the bundle reference if it exists
            AssetBundle result = bundle.Asset;

            // Finished successfully
            onFinished.Invoke(result);
        }

        /// <summary>
        /// Loads asset from the supplied asset bundle path and supplies the asset to the provided
        /// <code>Action</code> delegate, or null on error.
        /// </summary>
        /// <typeparam name="T">Type of asset to load, i.e. GameObject, Material, etc.</typeparam>
        /// <param name="assetBundleName">Name of the asset bundle (excludes path).</param>
        /// <param name="assetName">Name of asset in asset bundle.</param>
        /// <param name="onFinished">Called when asset is loaded, providing the asset reference.</param>
        /// <returns><code>IEnumerator</code> for reentrance from coroutines</returns>
        private IEnumerator LoadAssetAsyncInternal<T>(string assetBundleName, string assetName, Action<T> onFinished)
            where T : UnityEngine.Object
        {
            // Create a coroutine friendly semaphore
            var asynchAssetLoader = new object();
            _asynchAssetLoaders.Enqueue(asynchAssetLoader);
            while (_asynchAssetLoaders.Peek() != asynchAssetLoader)
            {
                new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);
                yield return null;
            }

            Log("Loading asset: " + assetName, nameof(LoadAssetAsyncInternal));

            // Asynchronously load the dependency
            Bundle bundle = GetBundle(assetBundleName);
            StartCoroutine(LoadAssetBundleAsyncInternal(assetBundleName, bundle.LoadedAction));

            // Wait for dependency loading completion
            while (!bundle.FinishedLoading)
            {
                new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);
                yield return bundle;
            }

            // Check for error condition
            if (bundle.HasError())
            {
                LogError("Error loading asset: " + assetName,
                    nameof(LoadAssetAsyncInternal));
                onFinished.Invoke(null);
                yield break;
            }

            // Get the asset from asset bundle
            Log("Loading " + assetName + " from bundle: " + assetBundleName,
                nameof(LoadAssetAsyncInternal));

            // Tell the bundle to load the asset
            AssetBundleRequest lReq = bundle.Asset.LoadAssetAsync<T>(assetName);
            yield return lReq;

            // Instantiate the loaded asset
            T result = null;
            if (lReq.asset != null)
            {
                result = Instantiate(lReq.asset as T);
            }

            // Report the results
            if (result == null)
            {
                LogWarning("Failed to load '" + assetName + "' of type '" + typeof(T) +
                    "' from asset bundle: " + assetBundleName,
                    nameof(LoadAssetAsyncInternal));
            }
            else
            {
                Log("Asset loaded: " + assetName, nameof(LoadAssetAsyncInternal));
            }

            // Supply the reference to the action delegate
            onFinished.Invoke(result);

            // Release the coroutine friendly semaphore
            _asynchAssetLoaders.Dequeue();
        }

        /// <summary>
        /// Loads the scene from the supplied asset bundle path and indicates successful loading to the provided
        /// <code>Action</code> delegate.
        /// </summary>
        /// <param name="assetBundleName">Name of the asset bundle (excludes path).</param>
        /// <param name="sceneName">Name of scene to load.</param>
        /// <param name="additive">Whether or not to load scene additively.</param>
        /// <param name="onFinished">Called when scene is loaded, supplying a boolean value indicating success.</param>
        private IEnumerator LoadSceneAsyncInternal(string assetBundleName, string sceneName, bool additive, Action<bool> onFinished)
        {
            // Create a coroutine friendly semaphore
            var asynchSceneLoader = new object();
            _asynchSceneLoaders.Enqueue(asynchSceneLoader);
            while (_asynchSceneLoaders.Peek() != asynchSceneLoader)
            {
                new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);
                yield return null;
            }

            Log("Loading scene: " + sceneName, nameof(LoadSceneAsyncInternal));

            // Asynchronously load the dependency
            Bundle bundle = GetBundle(assetBundleName);
            StartCoroutine(LoadAssetBundleAsyncInternal(assetBundleName, bundle.LoadedAction));

            // Wait for dependency loading completion
            while (!bundle.FinishedLoading)
            {
                new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);
                yield return bundle;
            }

            // Check for error condition
            if (bundle.HasError())
            {
                LogError("Error loading scene: " + sceneName, nameof(LoadSceneAsyncInternal));
                onFinished.Invoke(false);
                yield break;
            }

            // Load the scene from the asset bundle
            Log("Loading '" + sceneName + "' from asset bundle: " + assetBundleName,
                nameof(LoadSceneAsyncInternal));
            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName,
                additive ? LoadSceneMode.Additive : LoadSceneMode.Single);

            // Check for an invalid scene
            bool validScene = false;
            if (load != null)
            {
                // Wait for scene to load
                while (!load.isDone)
                {
                    new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);
                    yield return null;
                }

                Log("Finished loading scene: " + sceneName, nameof(LoadSceneAsyncInternal));

                // Mark as valid
                validScene = true;
            }
            else
            {
                LogError("Scene name is invalid: " + sceneName, nameof(LoadSceneAsyncInternal));
            }

            // Notify the action delegate
            onFinished?.Invoke(validScene);

            // Release the coroutine friendly semaphore
            _asynchSceneLoaders.Dequeue();
        }
    }
}