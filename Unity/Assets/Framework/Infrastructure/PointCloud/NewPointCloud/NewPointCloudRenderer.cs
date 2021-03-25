// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using unitycodercom_PointCloudBinaryViewer;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

public class NewPointCloudRenderer : MonoBehaviour
{

    //mark's stuff
    public GameObject leftController, rightController;
    
    BoxCollider pointCloudBoxCollider;
    public DrawLineManager drawLineManager;
    Rigidbody pointCloudRigidBody;
    PointCloudViewerDX11 pointCloudViewer;
    bool triggerStayCalled = false;
    

    // Start is called before the first frame update
    void Start()
    {
        pointCloudViewer = gameObject.GetComponent<PointCloudViewerDX11>();
        if(pointCloudViewer != null)
        {
            pointCloudViewer.OnLoadingComplete += onLoadComplete;
        }
        else
        {
            Debug.Log("Hmm, it seems like this gameObject isn't actually a point cloud?");
        }

        if (!drawLineManager)
        {
            GameObject drawLineManagerObject = GameObject.Find("DrawLineManager");
            DrawLineManager drawLineManagerScript = drawLineManagerObject.GetComponent<DrawLineManager>();
            if (drawLineManagerScript)
            {
                drawLineManager = drawLineManagerScript;
            }
        }
        if (!leftController)
        {
            leftController = GameObject.Find("LeftController");
        }
        if (!rightController)
        {
            rightController = GameObject.Find("RightController");
        }

        pointCloudViewer.useCommandBuffer = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void onLoadComplete(object cloudFilePath)
    {
        // debugging
        Debug.Log("onLoadComplete called, creating box collider now");
        // check if box collider on point cloud exists, if not, create it
        pointCloudBoxCollider = gameObject.GetComponent<BoxCollider>();
        if (pointCloudBoxCollider == null)
        {
            // create box collider
            PointCloudViewerDX11 pointCloudViewerDxElevenScript = gameObject.GetComponent<PointCloudViewerDX11>();
            if(pointCloudViewerDxElevenScript)
            {
                Bounds pointCloudBounds = pointCloudViewerDxElevenScript.GetBounds();
                pointCloudBoxCollider = gameObject.AddComponent<BoxCollider>();
                pointCloudBoxCollider.center = pointCloudBounds.center;
                pointCloudBoxCollider.size = (pointCloudBounds.extents * 2);
                pointCloudBoxCollider.isTrigger = true;
            }
            else
            {
                Debug.Log("Hmm, creating point cloud box collider went wrong. Seems like there's no point cloud viewer script attached to this gameobject");
            }
            
        }

        pointCloudRigidBody = gameObject.GetComponent<Rigidbody>();
        if (pointCloudRigidBody == null)
        {
            pointCloudRigidBody = gameObject.AddComponent<Rigidbody>();
            pointCloudRigidBody.isKinematic = true;
        }
    }

    // mark's collider trigger functions
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("OntriggerEnter called. Object collided with: " + other.gameObject);
        InputHand controllerScript = other.gameObject.GetComponent<InputHand>();
        if (controllerScript != null)
        {
            Debug.Log("onTriggerEnter called (and it's the controller)");
            // means we (controller) is inside point cloud
            if (drawLineManager.captureType == DrawLineManager.CaptureTypes.Laser)
            {
                other.GetComponent<LaserDrawingRaycastManager>().objectFromPointCloudViewer = gameObject;
            }
        }


    }

    /*
     * Note to self:
     * It seems like if onTriggerEnter is called before laserdrawingraycastmanager is activated
     * (aka you're already inside the point cloud whenever you activate the laser)
     * you need to remove yourself and re-enter for the code to work
     * Figure workaround for this (can use onTriggerStay but that seems inefficient)
     */

    // this function only occurs once in the case that you are already inside the point cloud when you
    // activate LaserDrawingRaycastManager
    private void OnTriggerStay(Collider other)
    {
        if(!triggerStayCalled)
        {
            InputHand controllerScript = other.gameObject.GetComponent<InputHand>();
            if(controllerScript != null)
            {
                // means we (controller) is inside point cloud
                if (drawLineManager.captureType == DrawLineManager.CaptureTypes.Laser)
                {
                    // debugging
                    Debug.Log("onTriggerStay called");
                    other.GetComponent<LaserDrawingRaycastManager>().objectFromPointCloudViewer = gameObject;
                    triggerStayCalled = true;
                }
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        InputHand controllerScript = other.gameObject.GetComponent<InputHand>();
        if (controllerScript != null)
        {
            Debug.Log("onTriggerExit called (and it's the controller)");
            // means we (controller) are now outside point cloud
            other.GetComponent<LaserDrawingRaycastManager>().objectFromPointCloudViewer = null;
            triggerStayCalled = false;
        }
    }

    private void OnDestroy()
    {
        pointCloudViewer.OnLoadingComplete += onLoadComplete;
    }
}
