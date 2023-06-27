// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Scenes.Lobby
{
    public class GrabProjectOpener : InteractableSceneObject<InteractableSceneObjectType>
    {
        [Tooltip("Tooltip item to show when user is interacting with object.")]
        public GameObject tooltipItem;

        [Tooltip("Mode navigator/manager to change modes with.")]
        public ModeNavigator modeNavigator;

        [Tooltip("Name of the XML project file.")]
        public string projectToOpen;

        public void StartTouching()
        {
            tooltipItem.SetActive(true);
        }

        public void StopTouching()
        {
            tooltipItem.SetActive(false);
        }

        public void StartUsing()
        {
            modeNavigator.OpenProject(Application.dataPath + "/" + projectToOpen, false);
        }
    }
}