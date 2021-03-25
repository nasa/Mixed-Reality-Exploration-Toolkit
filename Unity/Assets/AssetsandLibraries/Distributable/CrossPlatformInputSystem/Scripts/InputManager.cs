// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem
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
        /// The mode that MRET will run in.
        /// </summary>
        [Tooltip("The mode that MRET will run in.")]
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
            InitializeRig(true);
        }

        /// <summary>
        /// Used to initialize the rig as specified under inputMode.
        /// </summary>
        public void InitializeRig(bool avatarActive)
        {
            if (activeRig != null)
            {
                Debug.LogWarning("[InputManager] Rig already initialized.");
            }

            // If the rig is null, initialize the rig for the selected input mode.
            switch (inputMode)
            {
                case InputMode.OpenVR:
                    SetUpRig(openVRRigPrefab, avatarActive);
                    break;

                case InputMode.Desktop:
                default:
                    SetUpRig(desktopRigPrefab, avatarActive);
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
        private void SetUpRig(GameObject prefab, bool avatarActive)
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
            activeRig.Initialize(avatarActive, InputHand.ControllerMode.Controller);
        }
    }
}