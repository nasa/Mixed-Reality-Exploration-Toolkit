// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System.Collections.Generic;
using GSFC.ARVR.MRET.Selection;
using GSFC.ARVR.MRET.Common.Schemas;
using GSFC.ARVR.MRET.Infrastructure.Framework.Interactable;
using GSFC.ARVR.MRET.Infrastructure.Framework;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

public class AssemblyGrabber : Interactable, ISelectable
{
    [Tooltip("Root of assembly.")]
    public GameObject assemblyRoot;

    public List<AssemblyGrabber> otherGrabbers = new List<AssemblyGrabber>();
    
    private MeshRenderer[] objectRenderers;
    private Material[] objectMaterials;

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

    public bool grabbed { get; private set; }

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
        serializedPart.EnableInteraction = new bool[] { grabbable };
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

        highlightMaterial = MRET.HighlightMaterial;
    }

    public override void BeginGrab(InputHand hand)
    {
        // Check to see if any other grabbers are grabbing.
        foreach (AssemblyGrabber otherGrabber in otherGrabbers)
        {
            if (otherGrabber.grabbed)
            {
                EndGrab(hand);
                return;
            }
        }

        // Save parent for ungrabbing.
        originalParent = assemblyRoot.transform.parent;

        // Set assembly to be child of controller.
        assemblyRoot.transform.SetParent(hand.transform);

        grabbed = true;

        DisableAllEnvironmentScaling();
        DisableAllFlying();
    }

    public override void EndGrab(InputHand hand)
    {
        // Set parent back to original.
        if (originalParent)
        {
            assemblyRoot.transform.SetParent(originalParent);
        }

        grabbed = false;

        EnableAnyEnvironmentScaling();
        EnableAnyFlying();
    }

#region CONTEXTAWARECONTROL
    private void DisableAllEnvironmentScaling()
    {
        // TODO Check with Jeff.
    }

    private void EnableAnyEnvironmentScaling()
    {
        // TODO Check with Jeff.
    }

    private void DisableAllFlying()
    {
        // TODO Check with Jeff.
    }

    private void EnableAnyFlying()
    {
        // TODO Check with Jeff.
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

        // Highlight the entire part.
        foreach (MeshRenderer rend in transform.GetComponentsInChildren<MeshRenderer>())
        {
            int rendMatCount = rend.materials.Length;
            Material[] rendMats = new Material[rendMatCount];
            for (int j = 0; j < rendMatCount; j++)
            {
                rendMats[j] = MRET.SelectMaterial;
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
        Interactable aGrabToReturn = assemblyGrabber;

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