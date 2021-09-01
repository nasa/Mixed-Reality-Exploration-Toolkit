// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System.Collections.Generic;
using GSFC.ARVR.MRET.Infrastructure.Framework.SceneObject;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;
using GSFC.ARVR.MRET.Infrastructure.Framework;

public class InteractableTool : SceneObject
{
    public GameObject headsetFollower;

    private GameObject partPanel;
    private GameObject partPanelInfo = null;
    private ToolPassCollisionHandler collisionHandler;
    private MeshRenderer[] objectRenderers;
    private Material[] objectMaterials;
    private bool initialized = false;
    private bool originalGravityState = false, originalKinematicState = false;

    public void Start()
    {
        useable = true;

        if (initialized)
        {
            // Preserve information about original materials.
            objectRenderers = GetComponentsInChildren<MeshRenderer>();

            List<GameObject> meshObjList = new List<GameObject>();
            List<Material> objMatList = new List<Material>();
            foreach (MeshRenderer rend in objectRenderers)
            {
                meshObjList.Add(rend.gameObject);
                foreach (Material mat in rend.materials)
                {
                    objMatList.Add(mat);
                }
            }
            objectMaterials = objMatList.ToArray();
        }
        
        initialized = true;
    }

    public override void BeginGrab(InputHand hand)
    {
        base.BeginGrab(hand);

        hand.ToggleHandModel(false);

        foreach (Collider coll in GetComponentsInChildren<Collider>())
        {
            coll.isTrigger = true;
        }
        collisionHandler = gameObject.AddComponent<ToolPassCollisionHandler>();
        collisionHandler.enabled = false;
        collisionHandler.grabbingObj = hand.gameObject;
        collisionHandler.enabled = true;

        Rigidbody rBody = gameObject.GetComponent<Rigidbody>();
        if (rBody != null)
        {
            originalKinematicState = rBody.isKinematic;
            rBody.isKinematic = false;

            originalGravityState = rBody.useGravity;
            rBody.useGravity = false;
        }

        DisableAllEnvironmentScaling();
    }

    public override void EndGrab(InputHand hand)
    {
        base.EndGrab(hand);

        hand.ToggleHandModel(true);

        foreach (Collider coll in GetComponentsInChildren<Collider>())
        {
            coll.isTrigger = false;
        }
        collisionHandler.ResetTextures();
        Destroy(collisionHandler);

        Rigidbody rBody = gameObject.GetComponent<Rigidbody>();
        if (rBody != null)
        {
            rBody.isKinematic = originalKinematicState;
            rBody.useGravity = originalGravityState;
        }

        EnableAnyEnvironmentScaling();
    }

#region CONTEXTAWARECONTROL
    private bool previousScalingState = false, previousLocomotionPauseState = false;
    private void DisableAllEnvironmentScaling()
    {
        foreach (InputHand hand in MRET.InputRig.hands)
        {
            ScaleObjectTransform sot = hand.GetComponentInChildren<ScaleObjectTransform>(true);
            previousScalingState = sot.enabled; // Inefficient, but it doesn't matter.
            sot.enabled = false;
        }
    }

    private void EnableAnyEnvironmentScaling()
    {
        foreach (InputHand hand in MRET.InputRig.hands)
        {
            ScaleObjectTransform sot = hand.GetComponentInChildren<ScaleObjectTransform>(true);
            previousScalingState = sot.enabled; // Inefficient, but it doesn't matter.
            sot.enabled = previousScalingState;
        }
        previousScalingState = false;
    }
#endregion
}