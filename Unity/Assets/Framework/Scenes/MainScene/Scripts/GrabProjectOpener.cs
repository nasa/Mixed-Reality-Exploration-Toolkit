// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Common.Schemas;
using GSFC.ARVR.MRET.Infrastructure.Framework.Interactable;

public class GrabProjectOpener : Interactable
{
    [Tooltip("Tooltip item to show when user is interacting with object.")]
    public GameObject tooltipItem;

    [Tooltip("Mode navigator/manager to change modes with.")]
    public ModeNavigator modeNavigator;

    [Tooltip("Project Manager to use to load project.")]
    public UnityProject projectManager;

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