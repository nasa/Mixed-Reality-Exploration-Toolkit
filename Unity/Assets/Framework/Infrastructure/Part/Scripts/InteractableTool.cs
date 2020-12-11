using UnityEngine;
using VRTK;
using System.Collections.Generic;

public class InteractableTool : VRTK_InteractableObject
{
    public GameObject headsetFollower;

    private GameObject partPanel;
    private GameObject partPanelInfo = null;
    private ToolPassCollisionHandler collisionHandler;
    private MeshRenderer[] objectRenderers;
    private Material[] objectMaterials;
    private bool initialized = false;

    public void Start()
    {
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

            // Add touch highlighter scripts.
            VRTK_InteractObjectHighlighter highlighter = gameObject.AddComponent<VRTK_InteractObjectHighlighter>();
            highlighter.touchHighlight = touchHighlightColor;
            highlighter.objectToMonitor = this;
            highlighter.objectToHighlight = gameObject;
            VRTK.Highlighters.VRTK_OutlineObjectCopyHighlighter outline =
                gameObject.AddComponent<VRTK.Highlighters.VRTK_OutlineObjectCopyHighlighter>();
            outline.active = true;
            outline.unhighlightOnDisable = true;
            outline.thickness = 0.4f;
            outline.customOutlineModels = meshObjList.ToArray();
        }
        
        initialized = true;
    }

    public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
    {
        base.OnInteractableObjectGrabbed(e);

        foreach (Collider coll in GetComponentsInChildren<Collider>())
        {
            coll.isTrigger = true;
        }
        collisionHandler = gameObject.AddComponent<ToolPassCollisionHandler>();
        collisionHandler.enabled = false;
        collisionHandler.grabbingObj = e.interactingObject;
        collisionHandler.enabled = true;

        DisableAllEnvironmentScaling();
        DisableAllFlying();
    }

    public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
    {
        base.OnInteractableObjectUngrabbed(e);

        foreach (Collider coll in GetComponentsInChildren<Collider>())
        {
            coll.isTrigger = false;
        }
        collisionHandler.ResetTextures();
        Destroy(collisionHandler);

        EnableAnyEnvironmentScaling();
        EnableAnyFlying();
    }

#region CONTEXTAWARECONTROL
    private bool previousScalingState = false, previousFlyingState = false;
    private void DisableAllEnvironmentScaling()
    {
        VRTK_ControllerEvents[] cEvents = FindObjectsOfType<VRTK_ControllerEvents>();
        if (cEvents.Length == 2)
        {
            foreach (VRTK_ControllerEvents cEvent in cEvents)
            {
                ScaleObjectTransform sot = cEvent.GetComponentInChildren<ScaleObjectTransform>(true);
                previousScalingState = sot.enabled; // Inefficient, but it doesn't matter.
                sot.enabled = false;
            }
        }
    }

    private void EnableAnyEnvironmentScaling()
    {
        VRTK_ControllerEvents[] cEvents = FindObjectsOfType<VRTK_ControllerEvents>();
        if (cEvents.Length == 2)
        {
            foreach (VRTK_ControllerEvents cEvent in cEvents)
            {
                ScaleObjectTransform sot = cEvent.GetComponentInChildren<ScaleObjectTransform>(true);
                sot.enabled = previousScalingState;
            }
            previousScalingState = false;
        }
    }

    private void DisableAllFlying()
    {
        VRTK_ControllerEvents[] cEvents = FindObjectsOfType<VRTK_ControllerEvents>();
        if (cEvents.Length == 2)
        {
            foreach (VRTK_ControllerEvents cEvent in cEvents)
            {
                SimpleMotionController smc = cEvent.GetComponentInChildren<SimpleMotionController>(true);
                previousFlyingState = smc.motionEnabled; // Inefficient, but it doesn't matter.
                smc.motionEnabled = false;
            }
        }
    }

    private void EnableAnyFlying()
    {
        VRTK_ControllerEvents[] cEvents = FindObjectsOfType<VRTK_ControllerEvents>();
        if (cEvents.Length == 2)
        {
            foreach (VRTK_ControllerEvents cEvent in cEvents)
            {
                SimpleMotionController smc = cEvent.GetComponentInChildren<SimpleMotionController>(true);
                smc.motionEnabled = previousFlyingState;
            }
            previousFlyingState = false;
        }
    }
#endregion
}