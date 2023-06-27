// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem
{
    /// <remarks>
    /// History:
    /// 27 October 2020: Created
    /// </remarks>
    /// <summary>
    /// InputManager is a manager class that handles capturing input from devices and SDKs.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager instance { get; private set; }

        // TODO: Protect this better.
        /// <summary>
        /// Enumeration for input modes (SDKs).
        /// </summary>
        public enum InputMode { Desktop, OpenVR }
        /// <summary>
        /// The mode that the application will run in.
        /// </summary>
        [Tooltip("The mode that the application will run in.")]
        public InputMode inputMode = InputMode.Desktop;

        // TODO: These could instead be set up entirely from script, but I'm punting to do this when
        // we add editor tools.
        /// <summary>
        /// OpenVR rig prefab.
        /// </summary>
        [Tooltip("OpenVR rig prefab.")]
        public GameObject openVRRigPrefab;
        /// <summary>
        /// Desktop rig prefab.
        /// </summary>
        [Tooltip("Desktop rig prefab.")]
        public GameObject desktopRigPrefab;

        /// <summary>
        /// The input rig that is currently active in the scene.
        /// </summary>
        public InputRig activeRig { get; private set; }

        private void Awake()
        {
            instance = this;
            InitializeRig();
        }

        /// <summary>
        /// Used to initialize the rig as specified under inputMode.
        /// </summary>
        public void InitializeRig()
        {
            if (activeRig != null)
            {
                Debug.LogWarning("[InputManager] Rig already initialized.");
            }

            // If the rig is null, initialize the rig for the selected input mode.
            switch (inputMode)
            {
                case InputMode.OpenVR:
                    SetUpRig(openVRRigPrefab);
                    break;

                case InputMode.Desktop:
                default:
                    SetUpRig(desktopRigPrefab);
                    break;
            }
        }

        /// <summary>
        /// Used to destroy the active rig.
        /// </summary>
        public void DestroyRig()
        {
            // If the rig is not null, remove it.
            if (activeRig)
            {
                activeRig.Remove();
            }
        }

        /// <summary>
        /// Performs instantiation of rig prefab and captures necessary references.
        /// </summary>
        /// <param name="prefab"> Prefab to use for rig. </param>
        private void SetUpRig(GameObject prefab)
        {
            // Instantiate the rig prefab.
            GameObject activeRigGO = (GameObject) Instantiate(prefab);

            if (activeRigGO == null)
            {
                Debug.LogError("[InputManager] Error setting up rig.");
                return;
            }

            // If it isn't null, get the component.
            InputRig activeRig = activeRigGO.GetComponent<InputRig>();

            if (activeRig == null)
            {
                Debug.LogError("[InputManager] Rig reference error.");
                return;
            }

            // If the component isn't null, initialize it.
            activeRig.Initialize(InputHand.ControllerMode.Controller);
        }
    }
}