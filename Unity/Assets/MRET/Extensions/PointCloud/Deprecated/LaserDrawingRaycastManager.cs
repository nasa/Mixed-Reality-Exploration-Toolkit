// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing.Legacy;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;
#if MRET_EXTENSION_POINTCLOUDVIEWER
// PointCloudViewer and unitycodercom_PointCloudBinaryViewer namespace will be erased since I will be merging PointCloudManager into Pointcloud namespace
using PointCloudViewer;
#endif

namespace GOV.NASA.GSFC.XR.MRET.Extensions.PointCloud
{
    public class LaserDrawingRaycastManager : MonoBehaviour
    {
        public bool intersectionStatus
        {
            get
            {
                return isIntersecting;
            }
        }

        private bool isIntersecting = false;
        public Vector3 raycastPoint;
        public GameObject intersectingObject;
        public GameObject hitObj;
        public Vector3 hitPos;
        public GameObject objectFromPointCloudViewer;
#if MRET_EXTENSION_POINTCLOUDVIEWER
        public PointCloudManager pointCloudManager;
#endif
        public DrawLineManager drawLineManager;
        private LayerMask raycastLayerMask;

        private void Start()
        {
#if MRET_EXTENSION_POINTCLOUDVIEWER
            // subscribe to event listeners
            if (pointCloudManager != null)
            {
                PointCloudManager.PointWasSelected -= PointSelected; // unsubscribe just in case
                PointCloudManager.PointWasSelected += PointSelected;
            }

            raycastLayerMask = LayerMask.GetMask(LayerMask.LayerToName(MRET.raycastLayer));
#else
            Debug.LogWarning("PointCloudViewer is unavailable");
#endif
        }

        void Update()
        {
            // TODO: make transition of previewLine (from drawLineManager) between game objects more smooth
            if (DrawLineManager.CaptureTypes.Laser == drawLineManager.captureType)
            {
                /*
                 * NOTES so I don't forget:
                 * 1. Work on getting isTouching working as a condition here (started by Dylan here)
                 * 2. Work on creating a separate GameObject that is just a MeshRender of the same pointcloud for dragging around and other purposes
                 * 3. Undo modifications made to PointCloudTools code and instead create scripts to interface with the code (because of licensing issues)
                 * 4. Work on tiling system
                 */


                RaycastHit hit;
                bool wasHit = false;
                if ((wasHit = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, raycastLayerMask)) == false)
                {
                    wasHit = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity);
                }

                if (wasHit == false)
                {
                    // this object gets populated if you are inside of the point cloud
                    if (objectFromPointCloudViewer != null)
                    {
                        hitObj = objectFromPointCloudViewer;
                        //hitPos = objectFromtransform.position;
                        //Debug.Log("ObjectfromPointCloudViewer available: " + hitObj);
                        Physics.queriesHitBackfaces = true;

                    }

                    // cool thing; pointCloudManager can check to see if something intersects with bounds of a gameobject
                    //if (pointCloudManager.BoundsIntersectsCloud(VRTK_SharedMethods.GetBounds(gameObject.transform, null, null)))
                    //{
                    //    Debug.Log("Bound intersects cloud");
                    //    Debug.Log("VRTK getBounds: " + VRTK_SharedMethods.GetBounds(gameObject.transform, null, null));
                    //}
                    //else
                    //{
                    //    Debug.Log("Bound does NOT intersect cloud");
                    //    Debug.Log("VRTK getBounds: " + VRTK_SharedMethods.GetBounds(gameObject.transform, null, null));
                    //}

                }
                else
                {
                    hitPos = hit.point;
                    hitObj = hit.collider.gameObject;
                    //Physics.queriesHitBackfaces = false;
                }

                if (hitObj != null)
                {
                    if (hitObj.tag == "pointcloud" && drawLineManager.captureType == DrawLineManager.CaptureTypes.Laser)
                    {
#if MRET_EXTENSION_POINTCLOUDVIEWER
                        // disabling box collider to get accurate ray, not sure if needed
                        //hitObj.gameObject.GetComponent<BoxCollider>().enabled = false;

                        // getting point from measurement tool of point cloud
                        Ray ray = new Ray(transform.position, transform.forward);
                        pointCloudManager.RunPointPickingThread(ray);
#endif
                    }
                    else if (hitObj.tag != "pointcloud") //&& hitObj.layer == SessionConfiguration.raycastLayer)
                    {
                        if (hitObj.layer == MRET.raycastLayer || hitObj.GetComponent<InteractablePart>() == null)
                        {
                            intersectingObject = hitObj;
                            isIntersecting = true;
                            raycastPoint = hitPos;
                            hitObj = null;
                        }
                        else
                        {
                            isIntersecting = false;
                        }

                    }
                }
                else
                {
                    isIntersecting = false;
                }
            }
        }


        // gets called when PointWasSelected event fires in BinaryViewerDX11
        void PointSelected(Vector3 pos)
        {
            raycastPoint = pos;
            intersectingObject = hitObj;
            isIntersecting = true;
            hitObj = null;
        }
    }
}