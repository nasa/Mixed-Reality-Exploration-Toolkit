// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using Siccity.GLTFUtility;
using GSFC.ARVR.MRET.Infrastructure.Components.AssetBundles;
using GSFC.ARVR.MRET.Infrastructure.Components.GLTFs;

namespace GSFC.ARVR.MRET.Infrastructure.Components.ModelLoading
{
    public class ModelLoading
    {
        public static GameObject ImportGLTF(string pathToModel)
        {
            return Importer.LoadFromFile(pathToModel);
        }

        /// <summary>
        /// Loads model from GLTF 2.0 file.
        /// </summary>
        /// <param name="pathToModel">Full path to model file.</param>
        /// <param name="onFinished">Called when model is loaded.</param>
        /// <param name="settings">Import settings.</param>
        public static void ImportGLTFAsync(string pathToModel, Action<GameObject, AnimationClip[]> onFinished, ImportSettings settings = null)
        {
            if (settings == null)
            {
                settings = new ImportSettings();
            }

            GLTFHelper.instance.LoadGLTF(pathToModel, onFinished, settings);
        }

        /// <summary>
        /// Loads model from an asset bundle.
        /// </summary>
        /// <param name="assetBundlePath">Full path to asset bundle.</param>
        /// <param name="prefabName">Name of prefab in asset bundle.</param>
        /// <param name="onFinished">Called when model is loaded.</param>
        public static void ImportAssetBundleModelAsync(string pathToAssetBundle, string prefabName, Action<object> onFinished)
        {
            AssetBundleHelper.instance.LoadAssetAsync(pathToAssetBundle, prefabName, typeof(GameObject), onFinished);
        }
    }
}