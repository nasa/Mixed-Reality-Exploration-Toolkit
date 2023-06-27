// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.UI.HUD;

namespace GOV.NASA.GSFC.XR.XRUI.WorldSpaceMenu
{
    public class WorldSpaceMenuLoader : MonoBehaviour
    {
        [Tooltip("Prefab of menu to be loaded in world space.")]
        public GameObject worldSpaceMenuPrefab;

        [Tooltip("Whether or not this menu will be parented by the controller menu.")]
        public bool childOfControllerMenu = true;

        [Tooltip("Whether or not this menu will be loaded in the HUD.")]
        public bool hudMenu = false;

        [Tooltip("The maxmimum allowed distance between the desired offset of the display " +
            "and the actual offset of the display before the display is repositioned to the " +
            "desired offset in front of the tracked headset.")]
        public float hudOffsetThreshold = 0.3f;

        [Tooltip("The desired offset from the tracked headset.")]
        public Vector3 hudOffset = new Vector3(0, 0, 0.75f);
        
        private GameObject instantiatedMenu = null;

        public void InstantiateMenu()
        {
            InstantiateMenu_RTN();
        }

        public GameObject InstantiateMenu_RTN()
        {
            if (!childOfControllerMenu)
            {
                GameObject newMenu = Instantiate(worldSpaceMenuPrefab);
                ConfigureDisplay(newMenu);

                return newMenu;
            }

            if (worldSpaceMenuPrefab && !instantiatedMenu)
            {
                instantiatedMenu = Instantiate(worldSpaceMenuPrefab);
                ConfigureDisplay(instantiatedMenu);
            }
            else if (instantiatedMenu && !instantiatedMenu.activeSelf)
            {
                instantiatedMenu.SetActive(true);
                ConfigureDisplay(instantiatedMenu);
            }

            return instantiatedMenu;
        }

        /// <summary>
        /// Configures the supplied display game object
        /// </summary>
        /// <param name="menuGO">The <code>GameObject</code> of the instantiated display</param>
        protected virtual void ConfigureDisplay(GameObject menuGO)
        {
            // Check if this menu is being placed in the HUD
            if (hudMenu)
            {
                // Configure the HUD
                FrameHUD frameHUD = menuGO.GetComponent<FrameHUD>();
                if (frameHUD == null)
                {
                    frameHUD = menuGO.AddComponent<FrameHUD>();
                    frameHUD.offsetThreshold = hudOffsetThreshold;
                    frameHUD.offset = hudOffset;
                }
            }
            else
            {
                // We want the ControllerMenu->MenuObject position. Default to the transform of this game object
                Vector3 rootPosition = transform.position;
                ControllerMenu.ControllerMenu controllerMenu = gameObject.GetComponentInParent<ControllerMenu.ControllerMenu>();
                if ((controllerMenu != null) && (controllerMenu.menuObject != null))
                {
                    rootPosition = controllerMenu.menuObject.transform.position;
                }
                menuGO.transform.position = rootPosition + new Vector3(0, 0.20f, 0.1f); // TODO: Arbitrary Y offset
                menuGO.transform.rotation =
                    Quaternion.LookRotation((MRET.MRET.InputRig.head.transform.position
                    - menuGO.transform.position) * -1, Vector3.up);
            }
        }

        public void DestroyMenu()
        {
            Destroy(instantiatedMenu);
        }
    }
}