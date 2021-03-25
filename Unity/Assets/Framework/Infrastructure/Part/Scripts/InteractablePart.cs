// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System.Collections.Generic;
using GSFC.ARVR.MRET.Selection;
using GSFC.ARVR.MRET.XRC;
using GSFC.ARVR.MRET.Common.Schemas;
using GSFC.ARVR.MRET.Infrastructure.Framework.SceneObject;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;
using GSFC.ARVR.MRET.Infrastructure.Framework;

public class InteractablePart : SceneObject, ISelectable
{
    public class InteractablePartSettings
    {
        public bool interactionEnabled;
        public bool collisionEnabled;
        public bool gravityEnabled;

        public static InteractablePartSettings defaultSettings
        {
            get
            {
                return new InteractablePartSettings(false, false, false);
            }
        }

        public InteractablePartSettings(bool interaction, bool collision, bool gravity)
        {
            interactionEnabled = interaction;
            collisionEnabled = collision;
            gravityEnabled = gravity;
        }
    }

    [Tooltip("Configuration Panel")]
    public GameObject partPanelPrefab;
    public GameObject grabCube, enclosure;
    public Transform headsetObject;
    public Color highlightColor;

    private GameObject partPanel;
    private GameObject partPanelInfo = null;
    private bool clicked = false;
    private int clickTimer = -1;
    private CollisionHandler collisionHandler;
    private MeshRenderer[] objectRenderers;
    public Material[] objectMaterials;
    private bool hasBeenPlaced = false;
    private UndoManager undoManager;
    private Vector3 lastSavedPosition;
    private Quaternion lastSavedRotation;
    private Vector3[] originalRendPositions;

    // Part information.
    public string assetBundle = "";
    public string description = "";
    public string id = "";
    public float minMass = 0;
    public float maxMass = 0;
    public float massContingency = 0;
    public string serializationName = "";
    public string notes = "";
    public string partFileName = "";
    public string partName = "";
    public PartTypePartType partType = PartTypePartType.Chassis;
    public string reference = "";
    public string subsystem = "";
    public string vendor = "";
    public string version = "";
    public float idlePower = 0, averagePower = 0, peakPower = 0;
    public float powerContingency = 0;
    public Vector3 dimensions = Vector3.zero;
    public List<string> dataPoints = new List<string>();
    public bool randomizeTexture = false;
    public System.Guid guid = System.Guid.NewGuid();

    // TODO: Don't look at me! I'm too ugly for human eyes.
    public PartType Serialize()
    {
        PartType serializedPart = new PartType();
        serializedPart.AssetBundle = assetBundle;
        List<PartType> childPartList = new List<PartType>();
        List<int> idList = new List<int>();
        foreach (InteractablePart iPart in GetComponentsInChildren<InteractablePart>())
        {
            if (iPart.transform != transform)
            {
                childPartList.Add(iPart.Serialize());
            }
        }
        serializedPart.ChildParts = new PartsType();
        serializedPart.ChildParts.Parts = childPartList.ToArray();
        serializedPart.ChildParts.ID = idList.ToArray();
        serializedPart.Description = new string[] { description };
        serializedPart.EnableCollisions = new bool[] { false };
        serializedPart.EnableGravity = new bool[] { false };
        Rigidbody rBody = GetComponent<Rigidbody>();
        if (rBody)
        {
            serializedPart.EnableCollisions = new bool[] { !rBody.isKinematic };
            serializedPart.EnableGravity = new bool[] { rBody.useGravity };
        }
        serializedPart.NonInteractable = !useable;
        serializedPart.EnableInteraction = new bool[] { grabbable };
        if (enclosure)
        {
            serializedPart.Enclosure = enclosure.GetComponent<AssemblyGrabber>().Serialize();
        }
        else
        {
            serializedPart.Enclosure = null;
        }
        serializedPart.ID = id;
        serializedPart.MaxMass = new float[] { maxMass };
        serializedPart.MinMass = new float[] { minMass };
        serializedPart.MassContingency = new float[] { massContingency };
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
        serializedPart.IdlePower = idlePower;
        serializedPart.AveragePower = averagePower;
        serializedPart.PeakPower = peakPower;
        serializedPart.PowerContingency = powerContingency;
        serializedPart.Reference = reference;
        serializedPart.Subsystem = subsystem;
        serializedPart.Vendor = new string[] { vendor };
        serializedPart.Version = version;
        serializedPart.RandomizeTexture = randomizeTexture;
        serializedPart.GUID = guid.ToString();

        return serializedPart;
    }

    public void Start()
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

        undoManager = FindObjectOfType<UndoManager>();

        lastSavedPosition = transform.position;
        lastSavedRotation = transform.rotation;

        EnableColliders();
    }

    private void Update()
    {
        if (clicked)
        {
            if (clickTimer < 0)
            {
                clickTimer = 10;
            }
            else if (clickTimer == 0)
            {
                clicked = false;
                clickTimer = -1;
            }
            else
            {
                clickTimer--;
            }
        }
    }

    public override void Use(InputHand hand)
    {
        base.Use(hand);

        if (!isPlacing)
        {
            if (clicked == true)
            {
                // Register as double-click.
                LoadPartPanel(hand.gameObject, true);
            }
            else
            {
                clicked = true;

                if (!partPanel)
                {
                    LoadPartPanel(hand.gameObject, false);
                }
                else
                {
                    partPanelInfo = new GameObject();
                    partPanelInfo.transform.position = partPanel.transform.position;
                    DestroyPartPanel();
                }
            }
        }
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

    protected override void BeginTouch(InputHand hand)
    {
        base.BeginTouch(hand);

        if (!useable)
        {
            return;
        }

        /*if (!selected)
        {
            // Highlight the entire part.
            foreach (MeshRenderer rend in GetComponentsInChildren<MeshRenderer>())
            {
                int rendMatCount = rend.materials.Length;
                Material[] rendMats = new Material[rendMatCount];
                for (int j = 0; j < rendMatCount; j++)
                {
                    rendMats[j] = highlightMaterial;
                }
                rend.materials = rendMats;
            }
        }*/
    }

    protected override void EndTouch()
    {
        base.EndTouch();

        if (!useable)
        {
            return;
        }

        /*if (!selected)
        {
            ResetMyTextures();
            foreach (InteractablePart iPart in GetComponentsInChildren<InteractablePart>())
            {
                iPart.ResetMyTextures();
            }
        }*/
    }

    public override void BeginGrab(InputHand hand)
    {
        if (!grabbable)
        {
            return;
        }

        base.BeginGrab(hand);

        StartAligning(hand.gameObject);

        collisionHandler = gameObject.AddComponent<CollisionHandler>();
        collisionHandler.enabled = false;
        collisionHandler.grabbingObj = hand.gameObject;
        collisionHandler.enabled = true;

        DisableAllEnvironmentScaling();
        DisableAllLocomotion();

        SynchronizedController cont = hand.gameObject.GetComponentInParent<SynchronizedController>();
        if (cont)
        {
            XRCUnity.UpdateEntityParent(guid.ToString(), cont.uuid.ToString());
        }
    }

    public override void EndGrab(InputHand hand)
    {
        if (!grabbable)
        {
            return;
        }

        base.EndGrab(hand);

        StopAligning();

        if (collisionHandler != null)
        {
            if (!selected)
            {
                collisionHandler.ResetTextures();
            }
            Destroy(collisionHandler);
        }

        SnapToConnector();

        if (lastSavedPosition != null && lastSavedRotation != null)
        {
            if (transform.position != lastSavedPosition && transform.rotation != lastSavedRotation)
            {
                undoManager.AddAction(ProjectAction.MoveObjectAction(transform.name, transform.position, transform.rotation, guid.ToString()),
                    ProjectAction.MoveObjectAction(transform.name, lastSavedPosition, lastSavedRotation));
                lastSavedPosition = transform.position;
                lastSavedRotation = transform.rotation;
            }
        }

        EnableAnyEnvironmentScaling();
        EnableAnyLocomotion();

        if (transform.parent)
        {
            InteractablePart iPt = transform.parent.GetComponentInParent<InteractablePart>();
            if (iPt)
            {
                XRCUnity.UpdateEntityParent(guid.ToString(), iPt.guid.ToString());
            }
            else
            {
                XRCUnity.UpdateEntityParent(guid.ToString(), "ROOT");
            }
        }

        EndTouch();
    }
    
    public void LoadPartPanel(GameObject controller, bool reinitialize)
    {
        if (!MRET.PartPanelEnabled)
        {
            return;
        }

        if (!partPanel)
        {
            partPanel = Instantiate(partPanelPrefab);
            ObjectPanelsMenuController panelsController = partPanel.GetComponent<ObjectPanelsMenuController>();
            if (panelsController)
            {
                panelsController.selectedObject = gameObject;
                panelsController.SetTitle(transform.name);
                panelsController.Initialize();
            }

            // If position hasn't been set or reinitializing, initialize panel position.
            if ((partPanelInfo == null) || reinitialize)
            {
                Collider objectCollider = GetComponent<Collider>();
                if (objectCollider)
                {
                    Vector3 selectedPosition = objectCollider.ClosestPointOnBounds(controller.transform.position);
                    
                    // Move panel between selected point and headset.
                    partPanel.transform.position = Vector3.Lerp(selectedPosition, headsetObject.transform.position, 0.1f);

                    // Try to move panel outside of object. If the headset is in the object, there is
                    // nothing we can do.
                    if (!objectCollider.bounds.Contains(headsetObject.transform.position))
                    {
                        while (objectCollider.bounds.Contains(partPanel.transform.position))
                        {
                            partPanel.transform.position = Vector3.Lerp(partPanel.transform.position,
                                headsetObject.transform.position, 0.1f);
                        }
                    }
                    partPanel.transform.position = Vector3.Lerp(partPanel.transform.position,
                                headsetObject.transform.position, 0.1f);
                }
                else
                {   // No mesh, so load panel close to controller.
                    partPanel.transform.position = controller.transform.position;
                }
            }
            else
            {
                partPanel.transform.position = partPanelInfo.transform.position;

                // Check if position is inside of object. If so, initialize it.
                Collider objectCollider = GetComponent<Collider>();
                if (objectCollider)
                {
                    if (objectCollider.bounds.Contains(partPanel.transform.position))
                    {
                        // Try to move panel outside of object. If the headset is in the object, there is
                        // nothing we can do.
                        if (!objectCollider.bounds.Contains(headsetObject.transform.position))
                        {
                            while (objectCollider.bounds.Contains(partPanel.transform.position))
                            {
                                partPanel.transform.position = Vector3.Lerp(partPanel.transform.position,
                                    headsetObject.transform.position, 0.1f);
                            }
                        }
                        partPanel.transform.position = Vector3.Lerp(partPanel.transform.position,
                                    headsetObject.transform.position, 0.1f);
                    }
                }
            }

            // Finally, make the panel a child of its gameobject and point it at the camera.
            partPanel.transform.rotation = Quaternion.LookRotation(headsetObject.transform.forward);
            partPanel.transform.SetParent(transform);
        }
    }

    public void DestroyPartPanel()
    {
        if (partPanel)
        {
            Destroy(partPanel);
        }
    }

    /// <summary>
    /// Temporary fix for a bug occurring with colliders being disabled.
    /// </summary>
    private void EnableColliders()
    {
        foreach (Collider coll in GetComponentsInChildren<Collider>())
        {
            coll.enabled = true;
        }
    }

#region SNAPPING
    private SnappingConnector connectorToSnapTo = null;

    public void SetCurrentSnappingConnector(SnappingConnector snappingConnector)
    {
        connectorToSnapTo = snappingConnector;
    }

    public void UnsetSnappingConnector(SnappingConnector snappingConnector)
    {
        if (connectorToSnapTo == snappingConnector)
        {
            connectorToSnapTo = null;
        }
    }

    public void SnapToConnector()
    {
        if (connectorToSnapTo)
        {
            connectorToSnapTo.Snap();
        }
    }

    public void StartAligning(GameObject controller)
    {
        if (MRET.ConfigurationManager.config.PlacementMode == false)
        {
            return;
        }

        GSFC.ARVR.MRET.Alignment.VRBuilderBehaviour bb = controller.AddComponent<GSFC.ARVR.MRET.Alignment.VRBuilderBehaviour>();
        bb.OutOfRangeDistance = 0.5f;
        //bb.PreviewLayer = MRET.previewLayer;

        EasyBuildSystem.Features.Scripts.Core.Base.Piece.PieceBehaviour pb = gameObject.AddComponent<EasyBuildSystem.Features.Scripts.Core.Base.Piece.PieceBehaviour>();

        pb.ChangeState(EasyBuildSystem.Features.Scripts.Core.Base.Piece.Enums.StateType.Preview);
        
        bb.ChangeMode(EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums.BuildMode.Placement);
        bb.placingObject = gameObject;

        foreach (Collider coll in GetComponentsInChildren<Collider>())
        {
            coll.enabled = false;
        }
    }

    public void StopAligning()
    {
        if (MRET.ConfigurationManager.config.PlacementMode == false)
        {
            return;
        }

        foreach (GSFC.ARVR.MRET.Alignment.VRBuilderBehaviour bb in FindObjectsOfType<GSFC.ARVR.MRET.Alignment.VRBuilderBehaviour>())
        {
            if (bb.placingObject)
            {
                if (bb.CurrentPreview)
                {
                    bb.placingObject.transform.position = bb.CurrentPreview.transform.position;
                    bb.placingObject.transform.rotation = bb.CurrentPreview.transform.rotation;
                    bb.ChangeMode(EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums.BuildMode.None);
                    Destroy(bb.CurrentPreview);
                }
                EasyBuildSystem.Features.Scripts.Core.Base.Piece.PieceBehaviour pb = bb.placingObject.GetComponent<EasyBuildSystem.Features.Scripts.Core.Base.Piece.PieceBehaviour>();
                if (pb)
                {
                    Destroy(pb);
                }

                bb.placingObject = null;
                Destroy(bb);
            }
        }

        foreach (Collider coll in GetComponentsInChildren<Collider>())
        {
            coll.enabled = true;
            if (coll.gameObject.layer == MRET.previewLayer)
            {
                coll.gameObject.layer = MRET.defaultLayer;
            }
        }
    }
#endregion

#region PLACEMENT
    private bool isPlacing = false;
    private Transform oldParent = null;
    private bool wasGrabbable = true, wasUsable = true, wasUseGravity = false, wasKinematic = false;
    public void StartPlacing(Transform placingParent = null)
    {
        foreach (InteractablePart iPart in gameObject.GetComponentsInChildren<InteractablePart>())
        {
            iPart.isPlacing = true;
        }

        oldParent = transform.parent;
        if (placingParent)
        {
            transform.SetParent(placingParent);
            transform.localPosition = Vector3.zero;
        }

        // Change these for grabbing purposes.
        wasGrabbable = grabbable;
        grabbable = false;
        wasUsable = useable;
        useable = true;
        Rigidbody rBody = gameObject.GetComponent<Rigidbody>();
        if (rBody == null)
        {
            rBody = gameObject.AddComponent<Rigidbody>();
        }
        wasUseGravity = rBody.useGravity;
        rBody.useGravity = false;
        wasKinematic = rBody.isKinematic;
        rBody.isKinematic = true;
    }

    public void StopPlacing()
    {
        if (!hasBeenPlaced)
        {
            StopAligning();

            hasBeenPlaced = true;
            foreach (InteractablePart iPart in gameObject.GetComponentsInChildren<InteractablePart>())
            {
                iPart.isPlacing = false;
            }
            transform.SetParent(oldParent);

            // Change these back.
            grabbable = wasGrabbable;
            useable = wasUsable;
            Rigidbody rBody = gameObject.GetComponent<Rigidbody>();
            if (rBody == null)
            {
                rBody = gameObject.AddComponent<Rigidbody>();
            }
            rBody.useGravity = wasUseGravity;
            rBody.isKinematic = wasKinematic;

            // If this is the root object, add an undo action.
            PartType part = Serialize();
            undoManager.AddAction(
                ProjectAction.AddObjectAction(part, transform.position, transform.rotation,
                new Vector3(part.PartTransform.Scale.X, part.PartTransform.Scale.Y, part.PartTransform.Scale.Z),
                new InteractablePartSettings(grabbable, !rBody.isKinematic, rBody.useGravity),
                part.GUID),
                ProjectAction.DeleteObjectAction(transform.name, part.GUID));

            lastSavedPosition = transform.position;
            lastSavedRotation = transform.rotation;
        }
    }

    public override void Place()
    {
        base.Place();
        StopPlacing();
    }
#endregion

#region CONTEXTAWARECONTROL
    private bool previousScalingState = false, previousLocomotionPauseState = false;
    private void DisableAllEnvironmentScaling()
    {
        /*VRTK_ControllerEvents[] cEvents = FindObjectsOfType<VRTK_ControllerEvents>();
        if (cEvents.Length == 2)
        {
            foreach (VRTK_ControllerEvents cEvent in cEvents)
            {
                ScaleObjectTransform sot = cEvent.GetComponentInChildren<ScaleObjectTransform>(true);
                previousScalingState = sot.enabled; // Inefficient, but it doesn't matter.
                sot.enabled = false;
            }
        }*/
        foreach (InputHand hand in MRET.InputRig.hands)
        {
            ScaleObjectTransform sot = hand.GetComponentInChildren<ScaleObjectTransform>(true);
            if (sot)
            {
                previousScalingState = sot.enabled;
                sot.enabled = false;
            }
        }
    }

    private void EnableAnyEnvironmentScaling()
    {
        /*VRTK_ControllerEvents[] cEvents = FindObjectsOfType<VRTK_ControllerEvents>();
        if (cEvents.Length == 2)
        {
            foreach (VRTK_ControllerEvents cEvent in cEvents)
            {
                ScaleObjectTransform sot = cEvent.GetComponentInChildren<ScaleObjectTransform>(true);
                sot.enabled = previousScalingState;
            }
            previousScalingState = false;
        }*/
        foreach (InputHand hand in MRET.InputRig.hands)
        {
            ScaleObjectTransform sot = hand.GetComponentInChildren<ScaleObjectTransform>(true);
            if (sot)
            {
                sot.enabled = previousScalingState;
            }
            previousScalingState = false;
        }
    }

    private void DisableAllLocomotion()
    {
        previousLocomotionPauseState = MRET.LocomotionManager.Paused;
        MRET.LocomotionManager.Paused = true;
    }

    private void EnableAnyLocomotion()
    {
        MRET.LocomotionManager.Paused = previousLocomotionPauseState;
        previousLocomotionPauseState = false;
    }
#endregion

#region EXPLODE

    public float explodeFactor = 2;
    public bool isExploded = false;

    public void Explode()
    {
        CaptureCurrentPositions();

        Vector3 centerPoint = transform.position;
        Renderer rootRend;
        if ((rootRend = GetComponent<Renderer>()) != null)
        {
            centerPoint = rootRend.bounds.center;
        }

        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
        {
            if (rend.transform == transform ||
                rend.GetComponent<ObjectPanelsMenuController>() ||
                rend.GetComponentInParent<ObjectPanelsMenuController>())
            {
                continue;
            }

            if (rend != null && rend.transform.parent != MRET.Project)
            {
                Vector3 dir = rend.transform.InverseTransformPoint(rend.bounds.center)
                    - rend.transform.InverseTransformPoint(centerPoint);
                rend.transform.localPosition = dir * explodeFactor +
                    (rend.transform.localPosition - rend.transform.InverseTransformPoint(centerPoint));
            }
        }

        isExploded = true;
    }

    public void Unexplode()
    {
        int rendPosIdx = 0;
        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
        {
            if (rend.transform == transform ||
                rend.GetComponent<ObjectPanelsMenuController>() ||
                rend.GetComponentInParent<ObjectPanelsMenuController>())
            {
                continue;
            }

            rend.transform.localPosition = originalRendPositions[rendPosIdx++];
        }

        isExploded = false;
    }

    private void CaptureCurrentPositions()
    {
        List<Vector3> rendPositions = new List<Vector3>();
        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
        {
            if (rend.transform == transform ||
                rend.GetComponent<ObjectPanelsMenuController>() ||
                rend.GetComponentInParent<ObjectPanelsMenuController>())
            {
                continue;
            }

            rendPositions.Add(rend.transform.localPosition);
        }
        originalRendPositions = rendPositions.ToArray();
    }

#endregion

#region Selection
    public void Select(bool hierarchical = true)
    {
        if (selected)
        {
            return;
        }
        
        selected = true;
        ISelectable[] sels = GetComponentsInParent<ISelectable>();
        if ((sels.Length == 0 || ( sels.Length == 1 && sels[0] == (ISelectable) this)) && hierarchical)
        {
            foreach (ISelectable selChild in GetInteractablePartRoot(this).GetComponentsInChildren<ISelectable>(true))
            {
                selChild.Select(!(selChild is AssemblyGrabber));
            }
        }

        // Highlight the entire part.
        foreach (MeshRenderer rend in GetComponentsInChildren<MeshRenderer>())
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
        ISelectable[] sels = GetComponentsInParent<ISelectable>();
        if ((sels.Length == 0 || (sels.Length == 1 && sels[0] == (ISelectable) this)) && hierarchical)
        {
            foreach (ISelectable selChild in GetInteractablePartRoot(this).GetComponentsInChildren<ISelectable>(true))
            {
                selChild.Deselect(!(selChild is AssemblyGrabber));
            }
        }

        ResetMyTextures();
        foreach (InteractablePart iPart in GetComponentsInChildren<InteractablePart>())
        {
            iPart.ResetMyTextures();
        }
    }

    private InteractablePart GetInteractablePartRoot(InteractablePart interactablePart)
    {
        InteractablePart iPartToReturn = interactablePart;

        InteractablePart[] newIPart;
        while ((newIPart = iPartToReturn.gameObject.transform.GetComponentsInParent<InteractablePart>(true)) != null &&
            newIPart[0] != iPartToReturn)
        {
            iPartToReturn = newIPart[0];
        }
        
        return iPartToReturn;
    }
#endregion
}