// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Extensions.GLTF;

namespace GOV.NASA.GSFC.XR.MRET.Helpers
{
    public abstract class ModelLoading
    {
        /// <summary>
        /// Loads model from GLTF 2.0 file.
        /// </summary>
        /// <param name="pathToModel">Full path to model file.</param>
        /// <param name="onFinished">Called when model is loaded.</param>
        /// <param name="settings">Import settings.</param>
        public static void ImportGLTFAsync(string pathToModel, Action<GameObject, AnimationClip[]> onFinished)
        {
            GltfExt.Instance.LoadGLTF(pathToModel, onFinished);
        }
    }
}