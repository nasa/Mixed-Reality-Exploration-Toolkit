using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GSFC.ARVR.MRET;


namespace GSFC.ARVR.MRET.Infrastructure.Framework.VirtualCameras
{
    /// <remarks>
    /// History:
    /// June 2021: V1 Added into MRET
    /// </remarks>
    /// <summary>
    /// VirtualCameraUtilities is a class that provides
    /// top-level control of The Virtual Camera Utilities in MRET.
    /// Author: Jonathan T. Reynolds
    /// </summary>
    [ExecuteInEditMode]
    public class VirtualCameraUtilities : MRETBehaviour
    {

        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(VirtualCameraUtilities);
            }
        }

        /// <summary>
        /// Spawn distance from the editor camera
        /// </summary>
        public static int onCamSpawnDistance = 2;

        /// <summary>
        /// this is a base value used across the VirtualCameraManager
        /// </summary>
        public static Vector2Int newFeedRenderTextureSize = new Vector2Int(1920, 1080);

        /// <summary>
        /// Given a new Cam prefab generate a UUID for it and return a data container for it. 
        /// </summary>
        /// <param name="newCam"></param>
        /// <returns></returns>
        public static CamDataContainer GenCamDataContainer(GameObject newCam, CAMUUID newUUID)
        {
            CamDataContainer addCam = new CamDataContainer();
            BaseVirtualCamera baseVirtual = newCam.GetComponent<BaseVirtualCamera>();

            if (baseVirtual == null)
            {
                Debug.Log("[VirtualCameraUtilities] Camera Added doesn't inherit from BaseVirtualCamera");
                return addCam;
            }
            
            addCam.cameraBaseScript = baseVirtual;
            addCam.camUUID = newUUID;

            return addCam;
        }


        /// <summary>
        /// This 
        /// </summary>
        /// <param name="newDisplay"></param>
        /// <param name="newUUID"></param>
        /// <returns></returns>
        public static DisplayDataContainer GenDisplayDataContainer(GameObject newDisplay, DISPLAYUUID newUUID)
        {
            DisplayDataContainer addDisplay = new DisplayDataContainer();
            BaseVirtualDisplay baseVirtual = newDisplay.GetComponent<BaseVirtualDisplay>();

            if (baseVirtual == null)
            {
                Debug.Log("[VirtualCameraUtilities] Camera Added doesn't inherit from BaseVirtualDisplay");
                return addDisplay;
            }

            addDisplay.displayBaseScript = baseVirtual;
            addDisplay.disUUID = newUUID;

            return addDisplay;
        }


        /// <summary>
        /// removes camera from the Vitual Camera List and destroys the game object. 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="VirtualCameras"></param>
        public static void DestroyCamOnIndex(int index, List<CamDataContainer> VirtualCameras)
        {
            CamDataContainer toDestroy = VirtualCameras[index];
            VirtualCameras.RemoveAt(index);
            Destroy(toDestroy.cameraGO);
        }


        /// <summary>
        /// This removes and destroys the Display in the Virtual display list based on the given index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="VirtualDisplays"></param>
        public static void DestroyDisplayOnIndex(int index, List<DisplayDataContainer> VirtualDisplays)
        {
            DisplayDataContainer toDestroy = VirtualDisplays[index];
            VirtualDisplays.RemoveAt(index);
            Destroy(toDestroy.displayGO);
        }

        /// <summary>
        /// Simple function to snap the recording camera to camera target
        /// </summary>
        /// <param name="toTransform"></param>
        public static void Snap(Transform toTransform, Transform RecordCamTransform)
        {
            RecordCamTransform.position = toTransform.position;
            RecordCamTransform.rotation = toTransform.rotation;
        }


        /// <summary>
        /// This is a special funtion that mirrors CleanUpList however because it
        /// needs to check the cam Feed Specifically this needs to be unique
        /// </summary>
        public static void CleanUpCamLinkCams(List<CamDisplayLink> CamToDisplays)
        {
            CamToDisplays.RemoveAll(item => item.CamFeed == null);
        }

        /// <summary>
        /// This function handles cleaning up the display links in the camera,
        /// so if a display is removed it ensures a quick list management on removal of item.
        /// </summary>
        public static void CleanUpCamLinkDisplays(List<CamDisplayLink> CamToDisplays)
        {
            foreach (var link in CamToDisplays)
            {
                link.FeedOutputs.RemoveAll(item => item == null);
            }
        }


        /// <summary>
        /// Given a game object checks to see if it's in the list. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="G"></param>
        /// <param name="L"></param>
        /// <returns></returns>
        public static int CheckGOInList(GameObject G, List<GameObject> L)
        {
            for (int i = 0; i < L.Count; i++)
            {
                if (G.GetHashCode() == L[i].GetHashCode())
                {
                    return i;
                }
            }


            return -1;
        }


    }

    /// <summary>
    /// Base data container that contains the Game object and a base virtual camera script. 
    /// This was created to save time by using GetComponent(BaseCameraScript) upfront and having a container
    /// to store both the game object reference and the script reference. 
    /// </summary>
    public class CamDataContainer
    {
        public GameObject cameraGO = null;
        public BaseVirtualCamera cameraBaseScript = null;
        public CAMUUID camUUID = null;

        public CamDataContainer(GameObject go, BaseVirtualCamera camBaseScript, CAMUUID camID)
        {
            cameraGO = go;
            cameraBaseScript = camBaseScript;
            camUUID = camID;
        }

        public CamDataContainer()
        {
            cameraGO = null;
            cameraBaseScript = null;
            camUUID = null;
        }
    }


    /// <summary>
    /// This serves the same purpose as CamDataContainer. See that for details. <see cref="CamDataContainer"/>
    /// </summary>
    public class DisplayDataContainer
    {
        public GameObject displayGO = null;
        public BaseVirtualDisplay displayBaseScript = null;
        public DISPLAYUUID disUUID = null;

        public DisplayDataContainer(GameObject go, BaseVirtualDisplay disBaseScript, DISPLAYUUID disID)
        {
            displayGO = go;
            displayBaseScript = disBaseScript;
            disUUID = disID;
        }

        public DisplayDataContainer()
        {
            displayGO = null;
            displayBaseScript = null;
            disUUID = null;
        }   
    }       
}
