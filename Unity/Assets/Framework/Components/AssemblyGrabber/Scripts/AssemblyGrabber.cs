using UnityEngine;
using VRTK;
using System.Collections.Generic;
using GSFC.ARVR.MRET.Selection;
using GSFC.ARVR.MRET.Common.Schemas;

public class AssemblyGrabber : VRTK_InteractableObject, ISelectable
{
    [Tooltip("Material to highlight assembly with.")]
    public Material highlightMaterial;

    [Tooltip("Root of assembly.")]
    public GameObject assemblyRoot;

    public List<AssemblyGrabber> otherGrabbers = new List<AssemblyGrabber>();
    
    private MeshRenderer[] objectRenderers;
    private Material[] objectMaterials;
    private Transform originalParent;
    private SessionConfiguration sessionConfig;
    private Material selectMaterial;

    // Part information.
    public string assetBundle = "";
    public string description = "";
    public string id = "";
    public float minMass = 0;
    public float maxMass = 0;
    public string serializationName = "";
    public string notes = "";
    public string partFileName = "";
    public string partName = "";
    public PartTypePartType partType = PartTypePartType.Chassis;
    public string reference = "";
    public string subsystem = "";
    public string vendor = "";
    public string version = "";
    public Vector3 dimensions = Vector3.zero;

    // TODO: Don't look at me! This is extremely ugly code.
    public PartType Serialize()
    {
        PartType serializedPart = new PartType();
        serializedPart.AssetBundle = assetBundle;

        if (serializedPart.ChildParts != null)
        {
                PartsType childParts = new PartsType();
            List<PartType> childPartList = new List<PartType>();
            List<int> idList = new List<int>();
            foreach (InteractablePart iPart in GetComponentsInChildren<InteractablePart>())
            {
                childPartList.Add(iPart.Serialize());
            }
            serializedPart.ChildParts.Parts = childPartList.ToArray();
            serializedPart.ChildParts.ID = idList.ToArray();
        }
        serializedPart.Description = new string[] { description };
        serializedPart.EnableCollisions = new bool[] { false };
        serializedPart.EnableGravity = new bool[] { false };
        Rigidbody rBody = GetComponent<Rigidbody>();
        if (rBody)
        {
            serializedPart.EnableCollisions = new bool[] { rBody.isKinematic };
            serializedPart.EnableGravity = new bool[] { rBody.useGravity };
        }
        serializedPart.EnableInteraction = new bool[] { isGrabbable };
        //serializedPart.NonInteractable = ;
        serializedPart.ID = id;
        serializedPart.MaxMass = new float[] { maxMass };
        serializedPart.MinMass = new float[] { minMass };
        serializedPart.Name = serializationName;
        serializedPart.Notes = notes;
        serializedPart.PartFileName = new string[] { partFileName };
        serializedPart.PartName = new string[] { partName };
        serializedPart.PartTransform = new UnityTransformType();
        serializedPart.PartTransform.Position = new Vector3Type();
        serializedPart.PartTransform.Position.X = transform.position.x;
        serializedPart.PartTransform.Position.Y = transform.position.y;
        serializedPart.PartTransform.Position.Z = transform.position.z;
        serializedPart.PartTransform.Rotation = new QuaternionType();
        serializedPart.PartTransform.Rotation.X = transform.rotation.x;
        serializedPart.PartTransform.Rotation.Y = transform.rotation.y;
        serializedPart.PartTransform.Rotation.Z = transform.rotation.z;
        serializedPart.PartTransform.Rotation.W = transform.rotation.w;
        serializedPart.PartTransform.Scale = new Vector3Type();
        serializedPart.PartTransform.Scale.X = dimensions.x;
        serializedPart.PartTransform.Scale.Y = dimensions.y;
        serializedPart.PartTransform.Scale.Z = dimensions.z;
        serializedPart.PartType1 = new PartTypePartType[] { partType };
        serializedPart.Reference = reference;
        serializedPart.Subsystem = subsystem;
        serializedPart.Vendor = new string[] { vendor };
        serializedPart.Version = version;

        return serializedPart;
    }

    public void ResetMyTextures()
    {
        if (selected)
        {
            return;
        }

        int i = 0;
        foreach (MeshRenderer rend in objectRenderers)
        {
            int rendMatCount = rend.materials.Length;
            Material[] rendMats = new Material[rendMatCount];
            for (int j = 0; j < rendMatCount; j++)
            {
                rendMats[j] = objectMaterials[i++];
            }
            rend.materials = rendMats;
        }
    }

    public void ResetAllTextures()
    {
        if (selected)
        {
            return;
        }

        foreach (InteractablePart part in assemblyRoot.GetComponentsInChildren<InteractablePart>())
        {
            part.ResetTextures();
            VRTK_InteractObjectHighlighter highlighter = part.GetComponent<VRTK_InteractObjectHighlighter>();
            if (highlighter)
            {
                highlighter.Unhighlight();
            }
        }

        ResetMyTextures();

        foreach (AssemblyGrabber grabber in otherGrabbers)
        {
            grabber.ResetMyTextures();
            VRTK_InteractObjectHighlighter highlighter = grabber.GetComponent<VRTK_InteractObjectHighlighter>();
            if (highlighter)
            {
                highlighter.Unhighlight();
            }
        }
    }

    public void Start()
    {
        // Preserve information about original materials.
        objectRenderers = GetComponentsInChildren<MeshRenderer>();

        List<Material> objMatList = new List<Material>();
        foreach (MeshRenderer rend in objectRenderers)
        {
            foreach (Material mat in rend.materials)
            {
                objMatList.Add(mat);
            }
        }
        objectMaterials = objMatList.ToArray();

        /////////////////////////// Get Session Configuration. ///////////////////////////
        GameObject sessionManager = GameObject.Find("SessionManager");
        if (sessionManager)
        {
            sessionConfig = sessionManager.GetComponent<SessionConfiguration>();
            if (sessionConfig)
            {
                highlightMaterial = sessionConfig.highlightMaterial;
                selectMaterial = sessionConfig.selectMaterial;
            }
            else
            {
                Debug.LogError("[CollisionHandler->Start] Unable to get Session Configuration.");
            }
        }
        else
        {
            Debug.LogError("[CollisionHandler->Start] Unable to get Session Manager.");
        }
        //////////////////////////////////////////////////////////////////////////////////
    }

    public override void OnInteractableObjectTouched(InteractableObjectEventArgs e)
    {
        base.OnInteractableObjectTouched(e);

        if (!selected)
        {
            // Highlight the entire assembly.
            foreach (MeshRenderer rend in assemblyRoot.GetComponentsInChildren<MeshRenderer>())
            {
                int rendMatCount = rend.materials.Length;
                Material[] rendMats = new Material[rendMatCount];
                for (int j = 0; j < rendMatCount; j++)
                {
                    rendMats[j] = highlightMaterial;
                }
                rend.materials = rendMats;
            }
        }
    }

    public override void OnInteractableObjectUntouched(InteractableObjectEventArgs e)
    {
        base.OnInteractableObjectUntouched(e);

        if (!selected)
        {
            // Return the entire assembly back to its original materials.
            ResetAllTextures();
        }
    }

    public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
    {
        // Check to see if any other grabbers are grabbing.
        foreach (AssemblyGrabber otherGrabber in otherGrabbers)
        {
            if (otherGrabber.IsGrabbed())
            {
                OnInteractableObjectUngrabbed(e);
                return;
            }
        }

        // Save parent for ungrabbing.
        originalParent = assemblyRoot.transform.parent;

        // Set assembly to be child of controller.
        assemblyRoot.transform.SetParent(e.interactingObject.transform);

        DisableAllEnvironmentScaling();
        DisableAllFlying();
    }

    public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
    {
        // Set parent back to original.
        if (originalParent)
        {
            assemblyRoot.transform.SetParent(originalParent);
        }

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

#region Selection
    private bool selected = false;

    public void Select(bool hierarchical = true)
    {
        if (selected)
        {
            return;
        }

        selected = true;
       if (hierarchical) GetInteractablePartRoot(this).Select();
        /*if (GetComponentsInParent<ISelectable>().Length == 0)
        {
            foreach (InteractablePart selChild in GetInteractablePartRoot(this).GetComponentsInChildren<InteractablePart>(true))
            {
                Debug.Log("selecting");
                selChild.Select();
            }
        }*/

        // Highlight the entire part.
        foreach (MeshRenderer rend in transform.GetComponentsInChildren<MeshRenderer>())
        {
            int rendMatCount = rend.materials.Length;
            Material[] rendMats = new Material[rendMatCount];
            for (int j = 0; j < rendMatCount; j++)
            {
                rendMats[j] = selectMaterial;
            }
            rend.materials = rendMats;
        }
    }

    public void Deselect(bool hierarchical = true)
    {
        if (!selected)
        {
            return;
        }

        selected = false;
        if (hierarchical) GetInteractablePartRoot(this).Deselect();
        /*if (GetComponentsInParent<ISelectable>().Length == 0)
        {
            foreach (InteractablePart selChild in GetInteractablePartRoot(this).GetComponentsInChildren<InteractablePart>(true))
            {
                selChild.Deselect();
            }
        }*/

        ResetMyTextures();
        foreach (InteractablePart iPart in transform.GetComponentsInChildren<InteractablePart>())
        {
            iPart.ResetMyTextures();
        }

        foreach (AssemblyGrabber aGrab in transform.GetComponentsInChildren<AssemblyGrabber>())
        {
            aGrab.ResetMyTextures();
        }
    }

    private InteractablePart GetInteractablePartRoot(AssemblyGrabber assemblyGrabber)
    {
        VRTK_InteractableObject aGrabToReturn = assemblyGrabber;

        InteractablePart[] newIPart = { null };
        while ((newIPart = aGrabToReturn.gameObject.transform.GetComponentsInParent<InteractablePart>(true)) != null &&
            newIPart[0] != aGrabToReturn)
        {
            aGrabToReturn = newIPart[0];
        }

        return (aGrabToReturn is InteractablePart) ? (InteractablePart) aGrabToReturn : null;
    }
#endregion
}