// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using RockVR.Video;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace GSFC.ARVR.MRET.Infrastructure.Framework.VirtualCameras
{
    /// <remarks>
    /// History:
    /// June 2021: V1 Added into MRET
    /// </remarks>
    /// <summary>
    /// VirtualCameraManager is a class that provides
    /// top-level control of The Virtual Camera System in MRET.
    /// Author: Jonathan T. Reynolds
    /// </summary>
    [ExecuteInEditMode]
    public class VirtualCameraManager : MRETUpdateBehaviour
    {

        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(VirtualCameraManager);
            }
        }

        [Header("Main Scene Controlled Variables")]
        [Tooltip("When added by the Wizard the camera added is whatever is tagged as the MainCamera, change this to the Rig Camera")]
        public GameObject RigHMDCamera;


        [Header("Preset Cameras and displays"),]
        [Tooltip("Any preset cameras will not be removed from scene when calling add/remove on Cam")]
        public List<GameObject> PresetCameras = new List<GameObject>();
        [Tooltip("Any preset displays will not be removed when calling add/remove on display")]
        public List<GameObject> PresetDisplays = new List<GameObject>();

        [Header("Camera and Display Links")]
        public List<CamDisplayLink> CamToDisplays = new List<CamDisplayLink>();

        #region PRESET_VARS
        [Header("Default Preset Variables")]
        [Tooltip("Container for all the base preset variables.")]
        [SerializeField]
        public VCMPresetVariables VCMPresetVariables = new VCMPresetVariables();


        protected List<CamDataContainer> VirtualCameras = new List<CamDataContainer>();

        protected List<DisplayDataContainer> VirtualDisplays = new List<DisplayDataContainer>();


        //InfoTracking
        Transform _CurrentTransform;
        int _CurrentCameraIndex = 0;


        #endregion

        //==================================
        //==================================
        //
        //==================================
        //==================================

        #region UNITY_SPECIFIC_FUNCTIONS
        /// <summary>
        /// Adds any preset cams to the virtual cam list. 
        /// </summary>
        protected override void MRETStart()
        {
            if (Application.isPlaying)
            {
                foreach (var cam in PresetCameras)
                {
                    AddVirtualCamera(cam);
                }

                foreach (var display in PresetDisplays)
                {
                    AddVirtualDisplay(display);
                }
            }

        }

        protected override void MRETUpdate()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                CleanUpAllReferences();
            }
#endif
        }

        #endregion

        //==================================
        //==================================
        //
        //==================================
        //==================================

        #region VIRTUAL_CAMERA_FUNCTIONS

        #region ADD/REMOVE_CAMS
        /// <summary>
        /// This is the Add Camera function.
        /// This is intended to be used at runtime. 
        /// </summary>
        public void AddVirtualCamera()
        {
#if UNITY_EDITOR
            GameObject newCam = (GameObject)PrefabUtility.InstantiatePrefab(VCMPresetVariables.BaseVirtualCamera);
            AddVirtualCamera(newCam);
#endif
        }

        /// <summary>
        /// This adds the cameras to the virtual Camera list. 
        /// </summary>
        /// <param name="newCam"></param>
        private void AddVirtualCamera(GameObject newCam)
        {
            VCMPresetVariables.BaseVirtualCamera.transform.position = RigHMDCamera.transform.position;
            newCam.transform.position += RigHMDCamera.transform.forward.normalized;

            CamDataContainer addCam = VirtualCameraUtilities.GenCamDataContainer(newCam, new CAMUUID(VirtualCameras.Count));

            VirtualCameras.Add(addCam);
        }

        /// <summary>
        /// This function is to add camera to scene from the editor for easy camera setup. 
        /// This is intended to be used in editor.
        /// </summary>
        public void AddPresetCamera(Vector3 spawnpoint)
        {
#if UNITY_EDITOR
            GameObject newCam = (GameObject)PrefabUtility.InstantiatePrefab(VCMPresetVariables.BaseVirtualCamera);
            newCam.transform.position = spawnpoint;
            newCam.name = "(NEW)" + VCMPresetVariables.BaseVirtualCamera.name;
            PresetCameras.Add(newCam);
#endif
        }

        /// <summary>
        /// This is simply to add a preset cam to list, purely just a game object with nothing else.
        /// This is intended to be used in editor. 
        /// </summary>
        /// <param name="newCam"></param>
        public void AddPresetCamera(GameObject newCam)
        {
            PresetCameras.Add(newCam);
        }

        /// <summary>
        /// this will remove the last camera in the VirtualCameras list. 
        /// This is intended to be used at runtime. 
        /// </summary>
        public void RemoveVirtualCamera()
        {
            RemoveVirtualCamera(VirtualCameras.Count - 1);
        }

        /// <summary>
        /// This will remove a camera with given index from the PresetCamera list.
        /// This is intended to be used in editor. 
        /// </summary>
        /// <param name="index"></param>
        public void RemovePresetCamera(int index)
        {
            PresetCameras.RemoveAt(index);
        }

        /// <summary>
        /// This removes the camera based on the index given 
        /// This is intended to be used at runtime. 
        /// </summary>
        /// <param name="index"></param>
        public void RemoveVirtualCamera(int index)
        {
            if (index < PresetCameras.Count + 1)
            {
                Debug.Log("[VirtualCameraManager] No more Cameras to remove.");
                return;
            }
            if (index > VirtualCameras.Count + 1)
            {
                Debug.Log("[VirtualCameraManager] Index out of bounds.");
                return;
            }



            //index = num of preset cameras and no other cameras
            if (index == PresetCameras.Count && !(index + 1 < VirtualCameras.Count))
            {
                SwitchRecordingCamera(0);
                VirtualCameraUtilities.DestroyCamOnIndex(index, VirtualCameras);
            }
            //index = num of preset cameras and other cameras
            else if (index == PresetCameras.Count && index + 1 < VirtualCameras.Count)
            {
                SwitchRecordingCamera(index + 1);
                VirtualCameraUtilities.DestroyCamOnIndex(index, VirtualCameras);
            }
            //index = n and other cameras
            else if (index + 1 < VirtualCameras.Count)
            {
                SwitchRecordingCamera(index + 1);
                VirtualCameraUtilities.DestroyCamOnIndex(index, VirtualCameras);
            }
            //index = end list
            else
            {
                SwitchRecordingCamera(PresetCameras.Count);
                VirtualCameraUtilities.DestroyCamOnIndex(index, VirtualCameras);
            }
        }

        /// <summary>
        /// This will remove a given Virtual Camera from the list given it's uuid
        /// This is intended to be used at runtime. 
        /// </summary>
        /// <param name="uuid"></param>
        public void RemoveVirtualCamera(CAMUUID uuid)
        {
            RemoveVirtualCamera(GetVirtualCamIndexByUUID(uuid));
        }

        #endregion

        #region SWITCHING_RECORD_CAM
        /// <summary>
        /// This Cycles to the next camera, wrapping if needed. 
        /// This is intended to be used at runtime. 
        /// </summary>
        public void SwitchRecordingCamera()
        {
            SwitchRecordingCamera(_CurrentCameraIndex + 1);
        }


        /// <summary>
        /// This switched the camera based on the given index.
        /// This is intended to be used at runtime. 
        /// </summary>
        /// <param name="index"></param>
        public void SwitchRecordingCamera(int index)
        {
            // Disable Previous Camera
            CamDataContainer previousAttachPointGO = VirtualCameras[_CurrentCameraIndex];
            BaseVirtualCamera recordAttachComponent = previousAttachPointGO.cameraBaseScript;
            if (recordAttachComponent != null)
            {
                recordAttachComponent.SetCamActive(false);
                if (recordAttachComponent.recordActive)
                {
                    recordAttachComponent.SetRecordActive(false);
                }
            }

            _CurrentCameraIndex = index % VirtualCameras.Count;


            // Enable new Camera
            CamDataContainer attachmentPointGO = VirtualCameras[_CurrentCameraIndex];
            recordAttachComponent = attachmentPointGO.cameraBaseScript;
            if (recordAttachComponent != null)
            {
                recordAttachComponent.SetCamActive(true);
                if (!recordAttachComponent.recordActive)
                {
                    recordAttachComponent.SetRecordActive(true);
                }
                _CurrentTransform = recordAttachComponent.coreLinkComponents.RecordAttachPoint;
            }
            else
            {
                _CurrentTransform = attachmentPointGO.cameraGO.transform;
            }


            VirtualCameraUtilities.Snap(_CurrentTransform, VCMPresetVariables.RecordCamTransform);
        }

        /// <summary>
        /// This will switch recording camera based on it's given uuid
        /// This is intended to be used at runtime. 
        /// </summary>
        /// <param name="camUUID"></param>
        public void SwitchRecordingCamera(CAMUUID camUUID)
        {
            SwitchRecordingCamera(GetVirtualCamIndexByUUID(camUUID));
        }



        #endregion

        #region CAM_BASE_UTILITIES

        /// <summary>
        /// This will turn on a camera feed with given index and what state it needs to be in. 
        /// This is intended to be used at runtime. (but can also be used in editor)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="active"></param>
        public void TurnOnCameraFeed(int index, bool active)
        {
            int current_cam_idx = index % VirtualCameras.Count;

            BaseVirtualCamera bsv = VirtualCameras[current_cam_idx].cameraBaseScript;
            bsv.SetFeedActive(active);
        }

        /// <summary>
        /// This will Turn on the camera feed with given uuid and what state it should be in.
        /// This is intended to be used at runtime. (but can also be used in editor)
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="active"></param>
        public void TurnOnCameraFeed(CAMUUID uid, bool active)
        {
            TurnOnCameraFeed(GetVirtualCamIndexByUUID(uid), active);
        }

        /// <summary>
        /// This will return camera based on the UID and will return -1 if not found.
        /// This is intended to be used at runtime.
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        //TODO: change this, camUUID should be a representation of index in VirtualCameras list
        public int GetVirtualCamIndexByUUID(CAMUUID UID)
        {
            return VirtualCameras.FindIndex((x) => x.camUUID == UID);
        }

        /// <summary>
        /// Given a game object it will check to see if that item is in the Virtual Camera list. 
        /// This is intended to be used at runtime. 
        /// </summary>
        /// <param name="GO"></param>
        /// <returns>This will return the index found, if nothign found it will return -1</returns>
        public int GetVirtualCamIndexbyGO(GameObject GO)
        {
            return VirtualCameras.FindIndex((x) => x.cameraGO == GO);
        }

        /// <summary>
        /// This will scan through Preset Cameras and check to see if the game object exists in the list. 
        /// This is intended to be used in editor. (but can also be used at runtime)
        /// </summary>
        /// <param name="GO"></param>
        /// <returns>This will return the index found. if not it will return -1</returns>
        public int CheckGOInPresetCams(GameObject GO)
        {
            return VirtualCameraUtilities.CheckGOInList(GO, PresetCameras);
        }

        #endregion

        #region CAM_CLEANUP_UTILITIES

        /// <summary>
        /// This will clean up all null items found in PresetCamera list
        /// Can be used in editor and at runtime. 
        /// </summary>
        public void CleanUpPresetCameraList()
        {
            PresetCameras.RemoveAll(item => item == null);
        }


        /// <summary>
        /// This will clean up all null items found in the VirtualCameraList
        /// Can be used in editor and at runtime.
        /// </summary>
        public void CleanUpVirtualCameraList()
        {
            VirtualCameras.RemoveAll(item => item == null);
        }

        #endregion

        #endregion

        //==================================
        //==================================
        //
        //==================================
        //==================================

        #region VIRTUAL_DISPLAY_FUNCTIONS

        #region ADD/REMOVE_DISPLAYS
        /// <summary>
        /// This will add a display to the Virtual Display list
        /// This is intended to be used at runtime. 
        /// </summary>
        public void AddVirtualDisplay()
        {
#if UNITY_EDITOR
            GameObject newDisplay = (GameObject)PrefabUtility.InstantiatePrefab(VCMPresetVariables.BaseVirtualDisplay);
            AddVirtualDisplay(newDisplay);
#endif
        }


        /// <summary>
        /// This adds to the preset display list.
        /// This is intended to be used in editor. (but it can also be used at runtime.)
        /// </summary>
        /// <param name="spawnpoint"></param>
        public void AddPresetDisplay(Vector3 spawnpoint)
        {
#if UNITY_EDITOR
            GameObject presetDisplay = (GameObject)PrefabUtility.InstantiatePrefab(VCMPresetVariables.BaseVirtualDisplay);
            presetDisplay.transform.position = spawnpoint;
            presetDisplay.name = "(NEW)" + VCMPresetVariables.BaseVirtualDisplay.name;
            PresetDisplays.Add(presetDisplay);
#endif
        }


        /// <summary>
        /// This will add a basic game object as a Display. 
        /// WARNING: this WILL modify the base mesh texture to become a display if it's not a BaseVirtualDisplay
        /// This is intended to be used in editor. (but can also be used at runtime)
        /// </summary>
        /// <param name="newDisplay"></param>
        public void AddPresetDisplay(GameObject newDisplay)
        {
            PresetDisplays.Add(newDisplay);
        }

        /// <summary>
        /// This adds a virtual Display based on a given game object. This function is intended to be used at runtime
        /// </summary>
        /// <param name="newDisplay"></param>
        private void AddVirtualDisplay(GameObject newDisplay)
        {
            VCMPresetVariables.BaseVirtualCamera.transform.position = RigHMDCamera.transform.position;
            newDisplay.transform.position += RigHMDCamera.transform.forward.normalized;

            DisplayDataContainer addDisplay = VirtualCameraUtilities.GenDisplayDataContainer(newDisplay, new DISPLAYUUID(VirtualDisplays.Count));

            VirtualDisplays.Add(addDisplay);
        }


        /// <summary>
        /// This removes the last display in the Virtual display list. 
        /// This is intended to be used at runtime. 
        /// </summary>
        public void RemoveVirtualDisplay()
        {
            RemoveVirtualDisplay(VirtualDisplays.Count - 1);
        }

        /// <summary>
        /// This removes a virtual Display based on index
        /// This is intended to be used at runtime. 
        /// </summary>
        /// <param name="index"></param>
        public void RemoveVirtualDisplay(int index)
        {
            VirtualCameraUtilities.DestroyDisplayOnIndex(index, VirtualDisplays);
        }


        /// <summary>
        /// This removes a Virtual Display by UUID
        /// This is intended to be used at runtime. 
        /// </summary>
        /// <param name="uuid"></param>
        public void RemoveVirtualDisplay(DISPLAYUUID uuid)
        {
            RemoveVirtualDisplay(GetVirtualDisplayIndexByUUID(uuid));
        }


        /// <summary>
        /// This removes a preset Display based on index.
        /// This is intended to be used in editor. 
        /// </summary>
        /// <param name="index"></param>
        public void RemovePresetDisplay(int index)
        {
            PresetDisplays.RemoveAt(index);
        }

        #endregion

        #region DISPLAY_BASE_UTILITIES


        /// <summary>
        /// This will get the index of VirtualDisplay by it's unique identifier.
        /// This is intended to be used at runtime. (but can also be used in editor.)
        /// </summary>
        /// <param name="UID"></param>
        /// <returns>index of VirtualDisplay found it will be -1 if not found</returns>
        public int GetVirtualDisplayIndexByUUID(DISPLAYUUID UID)
        {

            return VirtualDisplays.FindIndex((x) => x.disUUID == UID);

        }

        /// <summary>
        /// Given a game object. it will scrub through the preset Displays and check to see if it's in the list. 
        /// This is intended to be used in editor. (but can also be used at runtime)
        /// </summary>
        /// <param name="GO"></param>
        /// <returns>the index of the preserDisplay found. It will be -1 if not found.</returns>
        public int CheckGOInPresetDisplays(GameObject GO)
        {
            return VirtualCameraUtilities.CheckGOInList(GO, PresetDisplays);
        }

        #endregion

        #region DISPLAY_CLEAUP_UTILITIES


        /// <summary>
        /// This will remove all null items from the presetDisplay list
        /// This can be used at both runtime and in editor.
        /// </summary>
        public void CleanUpPresetDisplayList()
        {
            PresetDisplays.RemoveAll(item => item == null);
        }


        /// <summary>
        /// This will remove all null items from the Virtual Display list.
        /// This can be used at both runtime and in editor.
        /// </summary>
        public void CleanUpVirtualDisplayList()
        {
            VirtualDisplays.RemoveAll(item => item == null);
        }

        #endregion


        #endregion

        //==================================
        //==================================
        //
        //==================================
        //==================================

        #region RECORDING_CTRL_FUNCTIONS

        /// <summary>
        /// This will enable recording on the currently selected VirtualCamera
        /// </summary>
        public void StartRecording()
        {
            VCMPresetVariables.RecordCameraCtrl.StartCapture();
            VirtualCameras[_CurrentCameraIndex].cameraBaseScript.SetRecordActive(true);
        }


        /// <summary>
        /// This stops the capture and turns off the Record icon on the currently recording camera.
        /// </summary>
        public void StopRecording()
        {
            VCMPresetVariables.RecordCameraCtrl.StopCapture();
            VirtualCameras[_CurrentCameraIndex].cameraBaseScript.SetRecordActive(false);
        }

        /// <summary>
        /// This is a temporary pause on the capture, it can be resumed by calling this function again. 
        /// </summary>
        public void PauseCapture()
        {
            VCMPresetVariables.RecordCameraCtrl.ToggleCapture();
        }


        /// <summary>
        /// This get the current record status of the camera
        /// </summary>
        /// <returns>this will return the status type of the camera</returns>
        public VideoCaptureCtrl.StatusType GetRecordStatus()
        {
            return VCMPresetVariables.RecordCameraCtrl.status;
        }

        /// <summary>
        /// This will set the audio capture component of the video capture control
        /// </summary>
        /// <param name="audioCapture"></param>
        public void SetAudioCapture(AudioCapture audioCapture)
        {
            VCMPresetVariables.RecordCameraCtrl.audioCapture = audioCapture;
        }


        /// <summary>
        /// This get the Audio Capture component of the Video Capture control.
        /// </summary>
        /// <returns></returns>
        public AudioCapture GetAudioCapture()
        {
            return VCMPresetVariables.RecordCameraCtrl.audioCapture;
        }



        #endregion

        //==================================
        //==================================
        //
        //==================================
        //==================================

        #region GENETAL_UTILITIES

        /// <summary>
        /// Because of the way the camera lists and stuff are set up when linking we have to do a few different checks
        /// Need to check first if cam is already in list, and if it is if there's any new displays. 
        /// This can be used at both runtime and in editor.
        /// </summary>
        /// <param name="CamToLinkTo"></param>
        /// <param name="baseVirtualDisplays"></param>
        /// <param name="run_checks">this parameter should be flagged false if you ran list checking yourself. otherwise this should be
        /// left on to prevent adding of similar cameras and displays</param>
        /// <returns>This returns true if it was able to add to list and false if it wasn't</returns>
        //TODO: This could be optimized by going back through the structure and implimenting HashSets but that may not work with how things are set up.
        public bool AddCamDisplayLink(BaseVirtualCamera CamToLinkTo, List<BaseVirtualDisplay> baseVirtualDisplays, bool run_checks = true)
        {
            if (run_checks)
            {
                CamDisplayLink searchCam = GetCamDisplayLink(CamToLinkTo);
                if (searchCam != null)
                {
                    return AddToExistingCamDisplayLink(searchCam, baseVirtualDisplays);
                }
            }

            CamToDisplays.Add(new CamDisplayLink(CamToLinkTo, baseVirtualDisplays));
            return true;
        }

        /// <summary>
        /// When checks are active this will add only non-duplicate displays to the list.
        /// 
        /// otierwise it will just take what displays to add and will add them to the linkage.FeedOutputs
        /// </summary>
        /// <param name="linkage"></param>
        /// <param name="displaysToAdd"></param>
        /// <param name="run_checks">should only be false if you already checked for duplicates.</param>
        /// <returns></returns>
        public bool AddToExistingCamDisplayLink(CamDisplayLink linkage, List<BaseVirtualDisplay> displaysToAdd, bool run_checks = true)
        {
            if (run_checks)
            {
                List<BaseVirtualDisplay> non_duplicate_displays = new List<BaseVirtualDisplay>();
                foreach (BaseVirtualDisplay newdis in displaysToAdd)
                {
                    if (linkage.FeedOutputs.Find((x) => x.GetHashCode() == newdis.GetHashCode()) == null)
                    {
                        non_duplicate_displays.Add(newdis);
                    }
                }

                if (non_duplicate_displays.Count == 0)
                {
                    Debug.Log("[VirtualCameraManager] Displays Selected already added to List");
                    return false;
                }

                linkage.FeedOutputs.AddRange(non_duplicate_displays);
            }


            linkage.FeedOutputs.AddRange(displaysToAdd);
            return true;
        }

        /// <summary>
        /// This removes the selected camDisplayLink based on it's given amount
        /// 
        /// can be used in editor and at runtime.
        /// </summary>
        /// <param name="camDisplayLink"></param>
        //TODO: Set this up to clean references, reset materials and remove render textures?
        public void RemoveCamDisplayLink(CamDisplayLink camDisplayLink)
        {
            CamToDisplays.Remove(camDisplayLink);
        }

        /// <summary>
        /// This will remove all displays fromt he given cam link, 
        /// 
        /// Can be used in editor and at runtime
        /// </summary>
        /// <param name="camDisplayLink"></param>
        //TODO: ensure you clean up references, reset materials and remove any proper linking components
        public void RemoveDisplaysFromCamDisplayLink(CamDisplayLink camDisplayLink)
        {
            camDisplayLink.FeedOutputs.Clear();
        }


        /// <summary>
        /// This will return the CamDisplayLink if found
        /// </summary>
        /// <param name="camToCheck"></param>
        /// <returns>Cam Display Link on return null if not found</returns>
        public CamDisplayLink GetCamDisplayLink(BaseVirtualCamera camToCheck)
        {
            return CamToDisplays.Find((x) => x.CamFeed == camToCheck);
        }

        #endregion

        #region GENERAL_CLEANUP_UTILITIES

        /// <summary>
        /// This is a base function that will clean up all references. BE CAREFUL USING THIS.
        /// This can be used at both runtime and in editor.
        /// </summary>
        public void CleanUpAllReferences()
        {
            //TODO: change the Cam to display Cleanup to be Dynamic and not called on update here. 
            CleanUpCamToDisplays();


            CleanUpPresetCameraList();
            CleanUpVirtualCameraList();
            CleanUpPresetDisplayList();
            CleanUpVirtualDisplayList();
        }


        /// <summary>
        /// This will clean up the cam to displays list.
        /// This can be used at both runtime and in editor.
        /// </summary>
        public void CleanUpCamToDisplays()
        {
            VirtualCameraUtilities.CleanUpCamLinkCams(CamToDisplays);
            VirtualCameraUtilities.CleanUpCamLinkDisplays(CamToDisplays);
        }
        #endregion


    }


    /// <summary>
    /// This is the class container that represents all the core preset variables that should 
    /// be Preset in the VirtualCameraManager. 
    /// </summary>
    [Serializable]
    public class VCMPresetVariables
    {
        public GameObject BaseVirtualCamera;
        public GameObject BaseVirtualDisplay;
        public Transform RecordCamTransform;
        public VideoCaptureCtrl RecordCameraCtrl;
    }


    /// <summary>
    /// This is the CameraDisplay pairing that will be held in a list. Think of this similar to a Dictionary but instead in a list. 
    //TODO: ensure this acts like a dictionary and we don't have more than one reference to a camera in the list
    /// </summary>
    [Serializable]
    public class CamDisplayLink
    {
        public BaseVirtualCamera CamFeed;
        public List<BaseVirtualDisplay> FeedOutputs;

        /// <summary>
        /// This will take the camera given and all it's links and set up the connection between them and add it to a list.
        /// </summary>
        /// <param name="camToLink"></param>
        /// <param name="outputs"></param>
        public CamDisplayLink(BaseVirtualCamera camToLink, List<BaseVirtualDisplay> outputs)
        {
            CamFeed = camToLink;
            FeedOutputs = outputs;

            CamFeed.SetFeedRenderTexture();
            foreach (var feedOutputs in FeedOutputs)
            {
                feedOutputs.SetRenderTexture(CamFeed.coreLinkComponents.CameraFeed.targetTexture);
            }
        }
    }


}

