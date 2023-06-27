// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using GOV.NASA.GSFC.XR.Utilities.Collider;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Helpers;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.Tools.Selection;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_1;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing.Legacy;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part.Alignment;
using GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Part;
using GOV.NASA.GSFC.XR.MRET.Integrations.CPIS.Transforms;
using GOV.NASA.GSFC.XR.MRET.Integrations.XRC;
using GOV.NASA.GSFC.XR.MRET.Extensions.EasyBuildSystem.Alignment;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Part
{
    [System.Obsolete("Refer to " + nameof(GOV.NASA.GSFC.XR.MRET.SceneObjects.Part.InteractablePart))]
    public class InteractablePartDeprecated : InteractableSceneObjectDeprecated, ISelectable
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(InteractablePartDeprecated);
            }
        }

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

        // Multipliers for the explode calculations
        public const float EXPLODE_MIN_FACTOR = 0.1f;
        public const float EXPLODE_MAX_FACTOR = 0.2f;

        public enum PartShadingMode { MeshDefault, MeshLimits }
        [Tooltip("Configuration Panel")]
        public GameObject partPanelPrefab;
        public GameObject grabCube, enclosure;
        public Transform headsetObject;
        public Color highlightColor;
        public PartShadingMode shadingMode = PartShadingMode.MeshDefault;
        public bool shadeForLimitViolations = false;

        private GameObject partPanel;
        private GameObject partPanelInfo = null;
        private bool clicked = false;
        private int clickTimer = -1;
        private CollisionHandler collisionHandler;
        private MeshRenderer[] objectRenderers;
        public Material[] objectMaterials;
        private bool hasBeenPlaced = false;
        private Vector3 lastSavedPosition;
        private Quaternion lastSavedRotation;
        private Vector3[] originalRendPositions;

        private DataManager.DataValueEvent dataPointChangeEvent;

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
        public bool randomizeTexture = false;
        public System.Guid guid = System.Guid.NewGuid();
        public Preferences.ExplodeMode explodeMode = Preferences.ExplodeMode.Relative;
        public float explodeScaleValue = 1f;

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)
                    ? IntegrityState.Failure      // Fail is base class fails or anything is null
                    : IntegrityState.Success);    // Otherwise, our integrity is valid
        }

        public new static InteractablePartDeprecated Create()
        {
            GameObject interactablePartGameObject = new GameObject();
            InteractablePartDeprecated interactablePart = interactablePartGameObject.AddComponent<InteractablePartDeprecated>();
            return interactablePart;
        }

        public static InteractablePartDeprecated Create(string partName, string AssetBundle, InteractableSceneObjectDeprecated parent,
            Vector3 size, Guid? uuid = null)
        {
            GameObject interactablePartGameObject = new GameObject();
            InteractablePartDeprecated interactablePart = interactablePartGameObject.AddComponent<InteractablePartDeprecated>();

            // Load from asset bundle.
            Action<GameObject> action = (GameObject loadedPart) =>
            {
                ApplyModelToPart(interactablePart, loadedPart, size);
            };

            AssetBundleHelper.Instance.LoadAssetAsync(AssetBundle, partName, action);

            return interactablePart;
        }

        // TODO: Don't look at me! I'm too ugly for human eyes.
        public PartType Serialize()
        {
            PartType serializedPart = new PartType();
            serializedPart.AssetBundle = assetBundle;
            List<PartType> childPartList = new List<PartType>();
            List<int> idList = new List<int>();
            foreach (InteractablePartDeprecated iPart in GetComponentsInChildren<InteractablePartDeprecated>())
            {
                if (iPart.transform != transform)
                {
                    childPartList.Add(iPart.Serialize());
                }
            }
            serializedPart.ChildParts = new PartsType();
            serializedPart.ChildParts.Parts = childPartList.ToArray();
            serializedPart.ChildParts.ID = idList.ToArray();
            serializedPart.Description = description;
            serializedPart.EnableCollisions = false;
            serializedPart.EnableGravity = false;
            Rigidbody rBody = GetComponent<Rigidbody>();
            if (rBody)
            {
                serializedPart.EnableCollisions = !rBody.isKinematic;
                serializedPart.EnableGravity = rBody.useGravity;
            }
            serializedPart.NonInteractable = !useable;
            serializedPart.EnableInteraction = grabbable;
            if (enclosure)
            {
                serializedPart.Enclosure = enclosure.GetComponent<AssemblyGrabberDeprecated>().Serialize();
            }
            else
            {
                serializedPart.Enclosure = null;
            }
            serializedPart.ID = id;
            serializedPart.MaxMass = maxMass;
            serializedPart.MinMass = minMass;
            serializedPart.MassContingency = massContingency;
            serializedPart.Name = serializationName;
            serializedPart.Notes = notes;
            serializedPart.PartFileName = partFileName;
            serializedPart.PartName = partName;
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
            serializedPart.PartTransform.Scale = new NonNegativeFloat3Type();
            serializedPart.PartTransform.Scale.X = dimensions.x;
            serializedPart.PartTransform.Scale.Y = dimensions.y;
            serializedPart.PartTransform.Scale.Z = dimensions.z;
            serializedPart.PartType1 = partType;
            serializedPart.IdlePower = idlePower;
            serializedPart.AveragePower = averagePower;
            serializedPart.PeakPower = peakPower;
            serializedPart.PowerContingency = powerContingency;
            serializedPart.Reference = reference;
            serializedPart.Subsystem = subsystem;
            serializedPart.Vendor = vendor;
            serializedPart.Version = version;
            serializedPart.RandomizeTexture = randomizeTexture;
            serializedPart.GUID = guid.ToString();

            return serializedPart;
        }

        protected override void MRETStart()
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

            lastSavedPosition = transform.position;
            lastSavedRotation = transform.rotation;

            // Initialize from preferences
            explodeMode = MRET.ConfigurationManager.preferences.globalExplodeMode;

            EnableColliders();
        }

        protected override void MRETUpdate()
        {
            base.MRETUpdate();

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

        protected override void MRETOnDestroy()
        {
            base.MRETOnDestroy();

            DestroyPartPanel();
        }

        public override void Use(InputHand hand)
        {
            base.Use(hand);

            if (!isPlacing && CanPerformInteraction())
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
        }

        protected override void EndTouch()
        {
            base.EndTouch();

            if (!useable)
            {
                return;
            }
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

            SynchronizedControllerDeprecated cont = hand.gameObject.GetComponentInParent<SynchronizedControllerDeprecated>();
            if (cont)
            {
                XRCUnityDeprecated.UpdateEntityParent(guid.ToString(), cont.uuid.ToString());
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
                    ProjectManager.UndoManagerDeprecated.AddAction(
                        ProjectActionDeprecated.MoveObjectAction(transform.name, transform.position, transform.rotation, guid.ToString()),
                        ProjectActionDeprecated.MoveObjectAction(transform.name, lastSavedPosition, lastSavedRotation));
                    lastSavedPosition = transform.position;
                    lastSavedRotation = transform.rotation;
                }
            }

            EnableAnyEnvironmentScaling();

            if (transform.parent)
            {
                InteractablePartDeprecated iPt = transform.parent.GetComponentInParent<InteractablePartDeprecated>();
                if (iPt)
                {
                    XRCUnityDeprecated.UpdateEntityParent(guid.ToString(), iPt.guid.ToString());
                }
                else
                {
                    XRCUnityDeprecated.UpdateEntityParent(guid.ToString(), "ROOT");
                }
            }

            EndTouch();
        }

        public void LoadPartPanel(GameObject controller, bool reinitialize)
        {
            if (!ProjectManager.ObjectConfigurationPanelEnabled)
            {
                return;
            }

            if (!partPanel)
            {
                // TODO: Enclosures do not have prefab panels, but should they?
                if (partPanelPrefab == null)
                {
                    LogWarning("Part does not have a part panel prefab defined.", nameof(LoadPartPanel));
                    return;
                }

                // Instantiate the part panel
                partPanel = Instantiate(partPanelPrefab);

                // If position hasn't been set or reinitializing, initialize panel position.
                if ((partPanelInfo == null) || reinitialize)
                {
                    Collider objectCollider = GetComponent<Collider>();
                    if (objectCollider)
                    {
                        //Vector3 selectedPosition = objectCollider.ClosestPointOnBounds(controller.transform.position);

                        // Move panel between selected point and headset.
                        partPanel.transform.position = Vector3.Lerp(controller.transform.position, headsetObject.transform.position, 0.1f);

                        // Try to move panel outside of object. If the headset is in the object, there is
                        // nothing we can do.
                        if (!objectCollider.bounds.Contains(headsetObject.transform.position))
                        {
                            while (ExistsWithinPart(partPanel.transform.position))
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
                        if (ExistsWithinPart(partPanel.transform.position))
                        {
                            // Try to move panel outside of object. If the headset is in the object, there is
                            // nothing we can do.
                            if (!objectCollider.bounds.Contains(headsetObject.transform.position))
                            {
                                while (ExistsWithinPart(partPanel.transform.position))
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
                //partPanel.transform.SetParent(transform);
                partPanel.transform.SetParent(null);

                ObjectPanelsMenuControllerDeprecated panelsController = partPanel.GetComponent<ObjectPanelsMenuControllerDeprecated>();
                if (panelsController)
                {
                    panelsController.selectedObject = gameObject;
                    panelsController.SetTitle(transform.name);
                    panelsController.Initialize();
                }
            }
        }

        public void DestroyPartPanel()
        {
            if (partPanel)
            {
                Destroy(partPanel);
            }
        }

        private bool ExistsWithinPart(Vector3 pos)
        {
            Bounds bou = new Bounds();
            foreach (Collider coll in GetComponentsInChildren<Collider>())
            {
                bou.Encapsulate(coll.bounds);
            }
            return bou.Contains(pos);
        }

        private bool ExistsWithinAssembly(Vector3 pos)
        {
            InteractablePartDeprecated ipt = this;
            foreach (InteractablePartDeprecated ip in GetComponentsInParent<InteractablePartDeprecated>())
            {
                if (ip != this)
                {
                    ipt = ip;
                    break;
                }
            }

            if (ipt)
            {
                Bounds bou = new Bounds();
                foreach (Collider coll in ipt.GetComponentsInChildren<Collider>())
                {
                    bou.Encapsulate(coll.bounds);
                }
                return bou.Contains(pos);
            }

            return false;
        }

        /// <summary>
        /// Determines whether or not the part can be interacted with.
        /// </summary>
        /// <returns>True if it can, false if it can't.</returns>
        private bool CanPerformInteraction()
        {
            return !((bool)MRET.DataManager.FindPoint(DrawLineManager.ISDRAWINGFLAGKEY));
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

        private static void ApplyModelToPart(InteractablePartDeprecated part, GameObject model, Vector3? partSize = null)
        {
            Quaternion originalRotation = part.transform.rotation;
            model.transform.SetParent(part.transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;

            Bounds bou;

            if (partSize != null)
            {
                // Reset object rotation (for accurate render bounds).
                part.transform.eulerAngles = Vector3.zero;

                bou = new Bounds(part.transform.position, Vector3.zero);

                // TODO: hierarchical scaling is currently incorrect.
                // Create new bounds and add the bounds of all child objects together.
                foreach (Renderer ren in part.GetComponentsInChildren<Renderer>())
                {
                    bou.Encapsulate(ren.bounds);
                }

                Vector3 size = bou.size;
                Vector3 rescale = part.transform.localScale;
                Vector3 dimensions = new Vector3(
                    partSize.Value.x, partSize.Value.y, partSize.Value.z);

                rescale.x = dimensions.x * rescale.x / size.x;
                rescale.y = dimensions.y * rescale.y / size.y;
                rescale.z = dimensions.z * rescale.z / size.z;

                part.transform.localScale = rescale;
                part.transform.rotation = originalRotation;
            }

            // Use the collider utility in case an alternate solution to Collider is used for non-convex
            // colliders
            if (!ColliderUtil.HasColliderInChildren(part.gameObject))
            {
                switch (MRET.ConfigurationManager.colliderMode)
                {
                    case ConfigurationManager.ColliderMode.Box:
                        part.Log("No collider detected. Generating box collder...", nameof(ApplyModelToPart));
                        // Recalculate the bounds.
                        bou = new Bounds(part.transform.position, Vector3.zero);
                        foreach (Renderer ren in part.GetComponentsInChildren<Renderer>())
                        {
                            bou.Encapsulate(ren.bounds);
                        }

                        BoxCollider collider = part.gameObject.AddComponent<BoxCollider>();
                        collider.size = Vector3.Scale(bou.size,
                            new Vector3(1 / part.transform.localScale.x,
                            1 / part.transform.localScale.y,
                            1 / part.transform.localScale.z));
                        collider.center = part.transform.InverseTransformPoint(bou.center);
                        part.gameObject.layer = MRET.previewLayer;
                        break;

                    case ConfigurationManager.ColliderMode.NonConvex:
                        part.Log("No collider detected. Generating non-convex colliders...", nameof(ApplyModelToPart));
                        ColliderUtil.CreateNonConvexMeshColliders(part.gameObject);
                        foreach (MeshFilter mesh in part.gameObject.GetComponentsInChildren<MeshFilter>())
                        {
                            mesh.gameObject.layer = MRET.previewLayer;
                        }
                        break;

                    case ConfigurationManager.ColliderMode.None:
                    default:
                        part.Log("No collider detected. Not generating collider.", nameof(ApplyModelToPart));
                        break;
                }
            }
            else
            {
                part.Log("Collider already found.", nameof(ApplyModelToPart));
                foreach (Collider coll in part.GetComponents<Collider>())
                {
                    coll.enabled = true;
                }
            }

            // TODO: Randomize textures.

            Rigidbody rBody = part.gameObject.GetComponent<Rigidbody>();
            if (rBody == null)
            {
                rBody = part.gameObject.AddComponent<Rigidbody>();
                rBody.mass = 1;
                rBody.angularDrag = 0.99f;
                rBody.drag = 0.99f;
                rBody.useGravity = false;
                rBody.isKinematic = false;
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

#if MRET_EXTENSION_EASYBUILDSYSTEM
            VRBuilderBehaviour bb = controller.AddComponent<VRBuilderBehaviour>();
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
#else
            LogWarning("Easy Build System is unavailable", nameof(StartAligning));
#endif
        }

        public void StopAligning()
        {
            if (MRET.ConfigurationManager.config.PlacementMode == false)
            {
                return;
            }

#if MRET_EXTENSION_EASYBUILDSYSTEM
            foreach (VRBuilderBehaviour bb in FindObjectsOfType<VRBuilderBehaviour>())
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
#else
            LogWarning("Easy Build System is unavailable", nameof(StopAligning));
#endif
        }
#endregion

#region PLACEMENT
        private bool isPlacing = false;
        private Transform oldParent = null;
        private bool wasGrabbable = true, wasUsable = true, wasUseGravity = false, wasKinematic = false;
        public void StartPlacing(Transform placingParent = null)
        {
            foreach (InteractablePartDeprecated iPart in gameObject.GetComponentsInChildren<InteractablePartDeprecated>())
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
                foreach (InteractablePartDeprecated iPart in gameObject.GetComponentsInChildren<InteractablePartDeprecated>())
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
                ProjectManager.UndoManagerDeprecated.AddAction(
                    ProjectActionDeprecated.AddObjectAction(part, transform.position, transform.rotation,
                    new Vector3(part.PartTransform.Scale.X, part.PartTransform.Scale.Y, part.PartTransform.Scale.Z),
                    new InteractablePartDeprecated.InteractablePartSettings(grabbable, !rBody.isKinematic, rBody.useGravity),
                    part.GUID),
                    ProjectActionDeprecated.DeleteObjectAction(transform.name, part.GUID));

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

#endregion

#region EXPLODE

        protected enum ExplodeDirection
        {
            Expanding = 1,
            Collapsing = -1
        }

        public bool explodeActive = false;

        private float _explodeFactor = 1f;
        public float ExplodeFactor
        {
            get => _explodeFactor;
        }

        protected class ExplodeChild
        {
            public GameObject gameObject = null;
            public Vector3 originalPosition = Vector3.zero;
            public float originalDistanceFromParent = 0f;
            public Vector3 explodeVector = Vector3.zero;
            public Space explodeVectorSpace = Space.World;

            public void RestorePosition()
            {
                if (gameObject)
                {
                    gameObject.transform.position = originalPosition;
                }
            }
        }

        private List<ExplodeChild> explodeChildren = new List<ExplodeChild>();

        /// <summary>
        /// Called to initialize the exploding. This must be called before <code>Explode</code>
        /// or <code>Unexplode</code>.
        /// </summary>
        /// <see cref="StopExplode"/>
        public void StartExplode()
        {
            if (explodeActive)
            {
                return;
            }

            // Save the state of the explode children because we are going to alter their positions
            // and need to restore them later.

            // Start with an empty list
            explodeChildren.Clear();

            // We only care about renderable children
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            {
                // Make sure we have a renderer and skip this gameobject. We only want the children.
                if (renderer && (renderer.gameObject != this.gameObject))
                {
                    // Save the child position
                    ExplodeChild explodeChild = new ExplodeChild()
                    {
                        gameObject = renderer.gameObject,
                        originalPosition = renderer.gameObject.transform.position,
                        originalDistanceFromParent = Vector3.Distance(renderer.gameObject.transform.position, transform.position),

                        // Initialize the explode vector based upon the explode mode
                        explodeVector = (explodeMode == Preferences.ExplodeMode.Relative) ?
                            renderer.gameObject.transform.position - transform.position :
                            RandomExplodeVector(EXPLODE_MIN_FACTOR * explodeScaleValue, EXPLODE_MAX_FACTOR * explodeScaleValue),
                        explodeVectorSpace = (explodeMode == Preferences.ExplodeMode.Relative) ? Space.World : Space.Self
                    };

                    // Add explode child info to our master array
                    explodeChildren.Add(explodeChild);
                }
            }

            // Mark exploded as being active
            explodeActive = true;
        }

        /// <summary>
        /// Translates all the explode children in the supplied direction
        /// </summary>
        /// <param name="direction">The <code>ExplodeDirection</code> defining the direction of the translation</param>
        protected void ExplodeTranslate(ExplodeDirection direction)
        {
            // Move each child along a positive random vector. 
            // Ensure that the part moving is not just the collider child.
            foreach (ExplodeChild explodeChild in explodeChildren)
            {
                // Check assertions
                if (explodeChild.gameObject)
                {
                    // Translate the child along the explode vector in the specified direction
                    Vector3 translateVector = explodeChild.explodeVector * UnityEngine.Time.deltaTime * (float)direction;

                    // Perform the translate
                    explodeChild.gameObject.transform.Translate(translateVector, explodeChild.explodeVectorSpace);

                    // Calculate the new distance from parent
                    float distanceFromParent = Vector3.Distance(explodeChild.gameObject.transform.position, transform.position);

                    // Calculate the explosion scale
                    _explodeFactor = distanceFromParent / explodeChild.originalDistanceFromParent;
                }
            }
        }

        /// <summary>
        /// Called to explode the children in the expanding direction
        /// </summary>
        public void Explode()
        {
            if (!explodeActive)
            {
                return;
            }

            // Expand the children
            ExplodeTranslate(ExplodeDirection.Expanding);
        }

        /// <summary>
        /// Called to explode the children in the collapsing direction
        /// </summary>
        public void Unexplode()
        {
            if (!explodeActive)
            {
                return;
            }

            // Collapse the children
            ExplodeTranslate(ExplodeDirection.Collapsing);
        }

        /// <summary>
        /// Called to stop the exploding.
        /// </summary>
        /// <see cref="StartExplode"/>
        public void StopExplode()
        {
            if (!explodeActive)
            {
                return;
            }

            // Set each child to it's original position and clear the list
            foreach (ExplodeChild explodeChild in explodeChildren)
            {
                explodeChild.RestorePosition();
            }

            // Clear the list
            explodeChildren.Clear();

            // Mark explode as inactive
            explodeActive = false;
        }

        /// <summary>
        /// Generates a random explode vector. Y value is set to 0 so explosion only happens horizontally
        /// </summary>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>A random explode <code>Vector3</code></returns>
        public Vector3 RandomExplodeVector(float min, float max)
        {

            var x = Random.Range(min, max);
            var y = Random.Range(0, 0);
            var z = Random.Range(min, max);

            return new Vector3(x, y, z);
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
            if ((sels.Length == 0 || (sels.Length == 1 && sels[0] == (ISelectable)this)) && hierarchical)
            {
                foreach (ISelectable selChild in GetInteractablePartRoot(this).GetComponentsInChildren<ISelectable>(true))
                {
                    selChild.Select(!(selChild is AssemblyGrabberDeprecated));
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
            if ((sels.Length == 0 || (sels.Length == 1 && sels[0] == (ISelectable)this)) && hierarchical)
            {
                foreach (ISelectable selChild in GetInteractablePartRoot(this).GetComponentsInChildren<ISelectable>(true))
                {
                    selChild.Deselect(!(selChild is AssemblyGrabber));
                }
            }

            ResetMyTextures();
            foreach (InteractablePartDeprecated iPart in GetComponentsInChildren<InteractablePartDeprecated>())
            {
                iPart.ResetMyTextures();
            }
        }

        private InteractablePartDeprecated GetInteractablePartRoot(InteractablePartDeprecated interactablePart)
        {
            InteractablePartDeprecated iPartToReturn = interactablePart;

            InteractablePartDeprecated[] newIPart;
            while ((newIPart = iPartToReturn.gameObject.transform.GetComponentsInParent<InteractablePartDeprecated>(true)) != null &&
                newIPart[0] != iPartToReturn)
            {
                iPartToReturn = newIPart[0];
            }

            return iPartToReturn;
        }
#endregion

#region DATA POINTS
        /* private List<string> _dataPoints = new List<string>();
        public List<string> dataPoints
        {
            get
            {
                return _dataPoints;
            }

            private set
            {
                _dataPoints = value;
            }
        }*/
        public List<string> dataPoints = new List<string>();

        public bool AddDataPoint(string dataPoint)
        {
            if (dataPoints.Contains(dataPoint))
            {
                LogWarning("Point " + dataPoint + " already exists on " + partName + ".", nameof(AddDataPoint));
                return false;
            }

            // TODO MRETStart is not being called in time.
            if (dataPointChangeEvent == null)
            {
                dataPointChangeEvent = new DataManager.DataValueEvent();
                dataPointChangeEvent.AddListener(HandleDataPointChange);
            }

            dataPoints.Add(dataPoint);
            MRET.DataManager.SubscribeToPoint(dataPoint, dataPointChangeEvent);
            return true; //FIXME: at somepoint after subscribing and before the event triggers, the thresholds are cleared
        }

        public bool RemoveDataPoint(string dataPoint)
        {
            if (dataPoints.Contains(dataPoint))
            {
                dataPoints.Remove(dataPoint);
                MRET.DataManager.UnsubscribeFromPoint(dataPoint, dataPointChangeEvent);
                return true;
            }

            LogWarning("Point " + dataPoint + " does not exist on " + partName + ".", nameof(RemoveDataPoint));
            return false;
        }

        //TODO:Fix limitstate handling for arbitrary colors, because it no longer has 'nominalstate'
        public void HandleDataPointChange(DataManager.DataValue dataValue)
        {
            DataManager.DataValue.LimitState limitState = dataValue.limitState;
            if (limitState == DataManager.DataValue.LimitState.Undefined)
            {
                HandleUndefinedLimitState();
            }
            else
            {
                HandleLimitViolation(limitState);
            }
        }

        private void HandleLimitViolation(DataManager.DataValue.LimitState limitState)
        {
            if (shadeForLimitViolations)
            {
                if (shadingMode == PartShadingMode.MeshLimits)
                {
                    RevertMeshLimitShader();
                }
                shadingMode = PartShadingMode.MeshLimits;
                ApplyMeshLimitShader(limitState);
            }
        }

        private void HandleNominalLimitState()
        {
            if (shadingMode == PartShadingMode.MeshLimits)
            {
                shadingMode = PartShadingMode.MeshDefault;
                RevertMeshLimitShader();
            }
        }

        private void HandleUndefinedLimitState()
        {
            if (shadingMode == PartShadingMode.MeshLimits)
            {
                shadingMode = PartShadingMode.MeshDefault;
                RevertMeshLimitShader();
            }
        }

        private Dictionary<MeshRenderer, Material[]> revertMaterials = null;
        private void ApplyMeshLimitShader(DataManager.DataValue.LimitState limitType)
        {
            if (revertMaterials != null)
            {
                LogError("Revert materials not empty. Will not apply mesh limit shader.", nameof(ApplyMeshLimitShader));
                return;
            }

            RestoreObjectMaterials();
            SaveObjectMaterials();

            revertMaterials = new Dictionary<MeshRenderer, Material[]>();
            foreach (MeshRenderer rend in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                revertMaterials.Add(rend, rend.materials);

                rend.materials = new Material[] { MRET.LimitMaterial };
                //TODO: revisit this switch statement with Unity Color if we can replace DataManager.DataValue.LimitState
                Color limitColor = Color.black;
                switch (limitType)
                {
                    case DataManager.DataValue.LimitState.Blue:
                        limitColor = Color.blue;
                        break;
                    case DataManager.DataValue.LimitState.Cyan:
                        limitColor = Color.cyan;
                        break;
                    case DataManager.DataValue.LimitState.DarkGray:
                        limitColor = new Color(0.25f, 0.25f, 0.25f);
                        break;
                    case DataManager.DataValue.LimitState.Gray:
                        limitColor = Color.gray;
                        break;
                    case DataManager.DataValue.LimitState.Green:
                        limitColor = Color.green;
                        break;
                    case DataManager.DataValue.LimitState.LightGray:
                        limitColor = new Color(0.75f, 0.75f, 0.75f);
                        break;
                    case DataManager.DataValue.LimitState.Magenta:
                        limitColor = Color.magenta;
                        break;
                    case DataManager.DataValue.LimitState.Orange:
                        limitColor = new Color(1f, 0.647058824f, 0f);
                        break;
                    case DataManager.DataValue.LimitState.Pink:
                        limitColor = new Color(1f, 0.752941176f, 0.796078431f);
                        break;
                    case DataManager.DataValue.LimitState.Red:
                        limitColor = Color.red;
                        break;
                    case DataManager.DataValue.LimitState.White:
                        limitColor = Color.white;
                        break;
                    case DataManager.DataValue.LimitState.Yellow:
                        limitColor = Color.yellow;
                        break;
                    case DataManager.DataValue.LimitState.Undefined:
                    case DataManager.DataValue.LimitState.Black:
                    default:
                        limitColor = Color.black;
                        break;
                }
                rend.materials[0].color = limitColor;
            }
        }

        private void RevertMeshLimitShader()
        {
            if (revertMaterials == null)
            {
                LogError("No revert materials. Will not revert materials.", nameof(RevertMeshLimitShader));
                return;
            }

            RestoreObjectMaterials();
            //foreach (KeyValuePair<MeshRenderer, Material[]> rendMat in revertMaterials)
            //{
            //    rendMat.Key.materials = rendMat.Value;
            //}

            revertMaterials = null;
        }

#endregion
    }
}