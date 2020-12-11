using GSFC.ARVR.MRET.Selection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class InteractableDrawing : VRTK_InteractableObject, ISelectable
{

    private GameObject drawingPanel;
    private GameObject drawingPanelInfo = null;
    private bool clicked = false;
    public Transform headsetObject;
    public GameObject drawingPanelPrefab;

    public override void OnInteractableObjectUsed(InteractableObjectEventArgs e)
    {
        if (!drawingPanel)
        {
            LoadDrawingPanel(e.interactingObject, false);
        }
        else
        {
            drawingPanelInfo = new GameObject();
            drawingPanelInfo.transform.position = drawingPanel.transform.position;
            DestroyDrawingPanel();
        }
        /*DrawingPanel wsml = gameObject.transform.parent.GetComponent<DrawingPanel>();
        wsml.loadMenu(gameObject.transform, e.interactingObject.transform.rotation, e.interactingObject.transform.position);*/

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

    public void Deselect(bool hierarchical = true)
    {
        // Don't want to throw exceptions.
        //throw new System.NotImplementedException();
    }

    public void Select(bool hierarchical = true)
    {
        // Don't want to throw exceptions.
        //throw new System.NotImplementedException();
    }
}