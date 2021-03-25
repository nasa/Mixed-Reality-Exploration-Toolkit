// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.Framework.SceneObject;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

public class InteractableDrawing : SceneObject
{

    private GameObject drawingPanel;
    private GameObject drawingPanelInfo = null;
    public Transform headsetObject;
    public GameObject drawingPanelPrefab;

    public override void Use(InputHand hand)
    {
        if (!drawingPanel)
        {
            LoadDrawingPanel(hand.gameObject, false);
        }
        else
        {
            drawingPanelInfo = new GameObject();
            drawingPanelInfo.transform.position = drawingPanel.transform.position;
            DestroyDrawingPanel();
        }
    }
    
    public void LoadDrawingPanel(GameObject controller, bool reinitialize)
    {
        if (!drawingPanel)
        {
            drawingPanel = Instantiate(drawingPanelPrefab);

            if ((drawingPanelInfo == null) || reinitialize)
            {
                Collider objectCollider = GetComponent<Collider>();
                if (objectCollider)
                {
                    Vector3 selectedPosition = objectCollider.ClosestPointOnBounds(controller.transform.position);

                    // Move panel between selected point and headset.
                    drawingPanel.transform.position = Vector3.Lerp(selectedPosition, headsetObject.transform.position, 0.1f);

                    // Try to move panel outside of object. If the headset is in the object, there is
                    // nothing we can do.
                    if (!objectCollider.bounds.Contains(headsetObject.transform.position))
                    {
                        while (objectCollider.bounds.Contains(drawingPanel.transform.position))
                        {
                            drawingPanel.transform.position = Vector3.Lerp(drawingPanel.transform.position,
                                headsetObject.transform.position, 0.1f);
                        }
                    }
                    drawingPanel.transform.position = Vector3.Lerp(drawingPanel.transform.position,
                                headsetObject.transform.position, 0.1f);
                }
                else
                {   // No mesh, so load panel close to controller.
                    drawingPanel.transform.position = controller.transform.position;
                }
            }
            else
            {
                drawingPanel.transform.position = drawingPanelInfo.transform.position;

                // Check if position is inside of object. If so, initialize it.
                Collider objectCollider = GetComponent<Collider>();
                if (objectCollider)
                {
                    if (objectCollider.bounds.Contains(drawingPanel.transform.position))
                    {
                        // Try to move panel outside of object. If the headset is in the object, there is
                        // nothing we can do.
                        if (!objectCollider.bounds.Contains(headsetObject.transform.position))
                        {
                            while (objectCollider.bounds.Contains(drawingPanel.transform.position))
                            {
                                drawingPanel.transform.position = Vector3.Lerp(drawingPanel.transform.position,
                                    headsetObject.transform.position, 0.1f);
                            }
                        }
                        drawingPanel.transform.position = Vector3.Lerp(drawingPanel.transform.position,
                                    headsetObject.transform.position, 0.1f);
                    }
                }
            }

            // Finally, make the panel a child of its gameobject and point it at the camera.
            drawingPanel.transform.rotation = Quaternion.LookRotation(headsetObject.transform.forward);
            drawingPanel.transform.SetParent(transform);
        }
    }

    public void DestroyDrawingPanel()
    {
        if (drawingPanel)
        {
            Destroy(drawingPanel);
        }
    }
}