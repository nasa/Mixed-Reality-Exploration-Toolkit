// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Threading.Tasks;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Extensions.GLTF;
#if MRET_EXTENSION_MROPENXR
using Microsoft.MixedReality.OpenXR;
#endif

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK
{
    public class WMRControllerModel : MonoBehaviour, IControllerModel
    {

        /// <seealso cref="IControllerModel.Initialize(InputHand)"/>
        public void Initialize(InputHand hand)
        {
            // MRTK handles the loading of the model, so we don't have to do
            // it at all. Should something change, we can revisit, but for now
            // do nothing on initialize.
            LoadMixedRealityController(hand);
        }

        /// <summary>
        /// Loads the mixed reality controller associated with the supplied hand
        /// </summary>
        /// <param name="hand">THe <code>InputHand</code> associated with the controller</param>
        public async void LoadMixedRealityController(InputHand hand)
        {
            if (hand == null)
            {
                Debug.LogError("Supplied hand is null. Controller model cannot be loaded");
                return;
            }

#if MRET_EXTENSION_MROPENXR
            if (!ControllerModel.IsSupported)
            {
                Debug.LogError("Controller model feature not supported");
                return;
            }

            Debug.Log("Loading Mixed Reality Controller for hand: " + hand.name);

            // Determine which controller we are loading
            ControllerModel controller = (hand.handedness == InputHand.Handedness.left)
                ? ControllerModel.Left
                : ControllerModel.Right;

            // Try to load the model associated with the hand 
            if (controller.TryGetControllerModelKey(out ulong modelKey))
            {
                Task<byte[]> controllerModelTask = controller.TryGetControllerModel(modelKey);
                await controllerModelTask;

                if (controllerModelTask.Status == TaskStatus.RanToCompletion)
                {
                    Debug.Log("Loaded controller data");

                    Action<GameObject, AnimationClip[]> callback = (GameObject modelGO, AnimationClip[] acl) =>
                    {
                        if (modelGO != null)
                        {
                            modelGO.transform.SetParent(transform);
                            modelGO.transform.localPosition = Vector3.zero;
                            modelGO.transform.localRotation = Quaternion.identity;

                            Debug.Log("Controller model loaded: " + hand.name);
                        }
                        else
                        {
                            Debug.LogError("Failed to load model");
                        }
                    };

                    // Attemp to load the model from the data
                    GltfExt.Instance.LoadGLTF(hand.name + "Controller", controllerModelTask.Result, callback);
                }
                else
                {
                    Debug.LogError("Failed to load controller model");
                }
            }
            else
            {
                Debug.LogError("Error getting controller model key");
            }
#endif
        }

    }
}