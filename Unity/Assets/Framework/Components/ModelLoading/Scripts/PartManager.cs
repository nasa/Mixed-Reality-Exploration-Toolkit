// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;
using GSFC.ARVR.MRET.Common.Schemas;
using GSFC.ARVR.MRET.Infrastructure.Components.ModelLoading;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.SceneObject
{
    public class PartManager : MRETBehaviour
    {
        public GameObject partPanelPrefab, grabCubePrefab;
        public GameObject projectObjectContainer;
        public Color partTouchHighlightColor;

        /// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName
        {
            get
            {
                return nameof(PartManager);
            }
        }

        public void InstantiatePartInEnvironment(PartType part, Transform parent)
        {
            StartCoroutine(InstantiateGameObjectAsync(part, parent, false));
        }

        public void InstantiatePart(PartType part, Transform parent)
        {
            StartCoroutine(InstantiateGameObjectAsync(part, parent, true));
        }

        public void Initialize()
        {

        }

        // Note: The plan is to have preprocessing done on the serialized part info
        // if it is using an older schema version so that the logic at this level
        // is uniform.
        public void LoadPart(PartType serializedPartInfo, Transform parent, bool placingMode, Action<GameObject> onLoaded)
        {
            // Load based upon how the asset bundle is specified
            if (serializedPartInfo.AssetBundle == "GLTF")
            {
                // Load as GLTF.
                Action<GameObject, AnimationClip[]> action = (GameObject loadedPart, AnimationClip[] anims) => {
                    OnPartLoaded(loadedPart, null, serializedPartInfo, onLoaded, placingMode, anims);
                };
                ModelLoading.ImportGLTFAsync(serializedPartInfo.Name, action);
            }
            else if (serializedPartInfo.AssetBundle == "NULL")
            {
                // Load empty part.
                InstantiateEmptyGameObjectAsync(serializedPartInfo, parent, placingMode);
                OnPartLoaded(null, null, serializedPartInfo, onLoaded, placingMode);
            }
            else
            {
                // TODO: Revisit with schema update. PartName can be null, but is currently being used as the asset name in the
                // bundle. Default to Name if not specified.
                string assetName = (serializedPartInfo.PartName == null) ? serializedPartInfo.Name : serializedPartInfo.PartName[0];

                // Load from asset bundle.
                Action<object> action = (object loadedPart) => {
                    OnPartLoaded((GameObject) loadedPart, null, serializedPartInfo, onLoaded, placingMode);
                };
                ModelLoading.ImportAssetBundleModelAsync(Application.dataPath
                + "/StreamingAssets/Windows/" + serializedPartInfo.AssetBundle, assetName, action);
            }
        }

        public void OnPartLoaded(GameObject loadedPart, Transform parent, PartType part,
            Action<GameObject> onLoaded, bool placingMode, AnimationClip[] animations = null)
        {
            StartCoroutine(OnPartLoadedAsync(loadedPart, parent, part, onLoaded, placingMode, animations));
        }

        public static void AddRaycastMeshColliders(GameObject part)
        {
            foreach (MeshFilter mesh in part.GetComponentsInChildren<MeshFilter>())
            {
                GameObject raycastColliderObj = new GameObject("RaycastCollider");
                raycastColliderObj.transform.parent = mesh.transform;
                raycastColliderObj.transform.localPosition = Vector3.zero;
                raycastColliderObj.transform.localRotation = Quaternion.identity;
                raycastColliderObj.transform.localScale = Vector3.one;
                raycastColliderObj.layer = MRET.raycastLayer;

                if (mesh)
                {
                    MeshCollider mcoll = raycastColliderObj.gameObject.AddComponent<MeshCollider>();
                    if (mcoll)
                    {
                        mcoll.sharedMesh = mesh.mesh;
                    }
                }
            }
        }

        public IEnumerator OnPartLoadedAsync(GameObject loadedPart, Transform parent, PartType part,
            Action<GameObject> onLoaded, bool placingMode, AnimationClip[] animations = null)
        {
            if (loadedPart == null)
            {
                loadedPart = new GameObject(part.Name);
            }
            
            part.transform = loadedPart.transform;
            loadedPart.AddComponent<InteractablePart>();

            GameObject grabCube = null;

            if (part.ChildParts.Parts != null && part.ChildParts.Parts.Length > 0)
            {
                // Instantiate children.
                foreach (PartType pt in part.ChildParts.Parts)
                {
                    yield return InstantiateGameObjectAsync(pt, part.transform, false);
                }

                // Wait for all children to be instantiated.
                foreach (PartType pt in part.ChildParts.Parts)
                {
                    while (!pt.loaded)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }
                }

                // Instantiate any enclosures if this is the parent.
                if (parent == null)
                {
                    // Instantiate enclosure.
                    if (part.Enclosure != null)
                    {
                        if (part.Enclosure.AssetBundle != null)
                        {
                            yield return InstantiateEnclosureAsync(part.Enclosure, part.transform);
                        }
                        else
                        {
                            yield return InstantiateEmptyGameObjectAsync(part.Enclosure, part.transform, placingMode);
                        }

                        // Wait for enclosure to be instantiated.
                        if (part.Enclosure != null)
                        {
                            while (!part.Enclosure.loaded)
                            {
                                yield return new WaitForSeconds(0.1f);
                            }
                        }
                    }
                }

                // Instantiate grab cube.
                grabCube = Instantiate(grabCubePrefab);
                grabCube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                grabCube.transform.SetParent(part.transform);
                grabCube.GetComponent<AssemblyGrabber>().assemblyRoot = part.transform.gameObject;
                if (part.Enclosure != null)
                {
                    grabCube.GetComponent<AssemblyGrabber>().otherGrabbers.Add(part.Enclosure.transform.GetComponent<AssemblyGrabber>());

                    AssemblyGrabber grabber = part.Enclosure.transform.GetComponent<AssemblyGrabber>();
                    grabber.otherGrabbers.Add(grabCube.GetComponent<AssemblyGrabber>());
                    grabber.assetBundle = part.Enclosure.AssetBundle;
                    if (part.Enclosure.Description != null)
                    {
                        grabber.description = (part.Enclosure.Description[0] == null) ? "" : part.Enclosure.Description[0];
                    }
                    grabber.dimensions = new Vector3(part.Enclosure.PartTransform.Scale.X, part.Enclosure.PartTransform.Scale.Y, part.Enclosure.PartTransform.Scale.Z);
                    grabber.id = part.Enclosure.ID;
                    if (part.Enclosure.MinMass != null)
                    {
                        grabber.minMass = part.Enclosure.MinMass[0];
                    }
                    if (part.Enclosure.MaxMass != null)
                    {
                        grabber.maxMass = part.Enclosure.MaxMass[0];
                    }
                    grabber.serializationName = part.Enclosure.Name;
                    grabber.gameObject.name = part.Enclosure.Name;
                    grabber.notes = part.Enclosure.Notes;
                    if (part.Enclosure.PartFileName != null)
                    {
                        grabber.partFileName = (part.Enclosure.PartFileName[0] == null) ? "" : part.Enclosure.PartFileName[0];
                    }
                    if (part.Enclosure.PartName != null)
                    {
                        grabber.partName = (part.Enclosure.PartName[0] == null) ? "" : part.Enclosure.PartName[0];
                    }
                    if (part.Enclosure.PartType1 != null)
                    {
                        grabber.partType = (part.Enclosure.PartType1.Length < 1) ? PartTypePartType.Chassis : part.Enclosure.PartType1[0];
                    }
                    grabber.reference = part.Enclosure.Reference;
                    grabber.subsystem = part.Enclosure.Subsystem;
                    if (part.Enclosure.Vendor != null)
                    {
                        grabber.vendor = (part.Enclosure.Vendor[0] == null) ? "" : part.Enclosure.Vendor[0];
                    }
                    grabber.version = part.Enclosure.Version;
                }
                part.transform.GetComponent<InteractablePart>().grabCube = grabCube;

                // If this isn't the root object, hide the grab cube.
                if (parent != null)
                {
                    grabCube.SetActive(false);
                }
            }

            onLoaded.Invoke(loadedPart);

            // Store part information.
            InteractablePart iPart = part.transform.GetComponent<InteractablePart>();
            iPart.assetBundle = part.AssetBundle;
            if (part.Description != null)
            {
                iPart.description = (part.Description[0] == null) ? "" : part.Description[0];
            }
            iPart.dimensions = new Vector3(part.PartTransform.Scale.X, part.PartTransform.Scale.Y, part.PartTransform.Scale.Z);
            iPart.id = part.ID;
            if (part.MinMass != null)
            {
                iPart.minMass = part.MinMass[0];
            }
            if (part.MaxMass != null)
            {
                iPart.maxMass = part.MaxMass[0];
            }
            if (part.MassContingency != null)
            {
                iPart.massContingency = part.MassContingency[0];
            }
            iPart.serializationName = part.Name;
            iPart.gameObject.name = part.Name;
            iPart.notes = part.Notes;
            if (part.PartFileName != null)
            {
                iPart.partFileName = (part.PartFileName[0] == null) ? "" : part.PartFileName[0];
            }
            if (part.PartName != null)
            {
                iPart.partName = (part.PartName[0] == null) ? "" : part.PartName[0];
            }
            if (part.PartType1 != null)
            {
                iPart.partType = (part.PartType1.Length < 1) ? PartTypePartType.Chassis : part.PartType1[0];
            }
            iPart.idlePower = part.IdlePower;
            iPart.averagePower = part.AveragePower;
            iPart.peakPower = part.PeakPower;
            iPart.powerContingency = part.PowerContingency;
            iPart.reference = part.Reference;
            iPart.subsystem = part.Subsystem;
            if (part.Vendor != null)
            {
                iPart.vendor = (part.Vendor[0] == null) ? "" : part.Vendor[0];
            }
            iPart.version = part.Version;
            iPart.randomizeTexture = part.RandomizeTexture;
            if (part.GUID != null)
            {
                iPart.guid = new Guid(part.GUID);
            }

            if (placingMode == false)
            {
                // Instantiate in environment.
            }
            else
            {
                if (parent == null)
                {
                    // This is the root object. Either attach to its attach to object or
                    // start placing it.
                    Transform transformToAttachTo = null;
                    if (part.AttachToName != null && part.AttachToName != "")
                    {
                        transformToAttachTo = projectObjectContainer.transform.Find(part.AttachToName);
                    }

                    // Check that the transform position/rotation/scale contains all information.
                    if (part.AttachToTransform == null || part.AttachToTransform.Position == null
                        || part.AttachToTransform.Rotation == null)
                    {
                        transformToAttachTo = null;
                    }

                    if (transformToAttachTo)
                    {
                        if (part.IsOnlyAttachment)
                        {
                            // If this is the only attachment, destroy all existing parts at same level.
                            foreach (Transform tr in transformToAttachTo)
                            {
                                if (tr != transformToAttachTo && tr.GetComponent<InteractablePart>() != null)
                                {
                                    Destroy(tr.gameObject);
                                }
                            }
                        }

                        // If there is a transform to attach to, attach it.
                        part.transform.SetParent(transformToAttachTo);
                        part.transform.localPosition = new Vector3(part.AttachToTransform.Position.X,
                            part.AttachToTransform.Position.Y, part.AttachToTransform.Position.Z);
                        part.transform.localRotation = new Quaternion(part.AttachToTransform.Rotation.X,
                            part.AttachToTransform.Rotation.Y, part.AttachToTransform.Rotation.Z,
                            part.AttachToTransform.Rotation.W);

                        // If attachment is static, move colliders to its parent.
                        if (part.StaticAttachment)
                        {
                            // Remove colliders from that object to prevent moving of that part.
                            foreach (Collider coll in part.transform.GetComponentsInChildren<Collider>())
                            {
                                Destroy(coll);
                            }

                            // Generate a box collider for selecting the attachment.
                            // Recalculate the bounds.
                            Bounds bou = new Bounds(part.transform.position, Vector3.zero);
                            foreach (Renderer ren in part.transform.GetComponentsInChildren<Renderer>())
                            {
                                bou.Encapsulate(ren.bounds);
                            }

                            BoxCollider collider = part.transform.gameObject.AddComponent<BoxCollider>();
                            collider.size = Vector3.Scale(bou.size,
                                new Vector3(1 / part.transform.localScale.x, 1 / part.transform.localScale.y, 1 / part.transform.localScale.z));
                            collider.center = part.transform.InverseTransformPoint(bou.center);
                        }
                    }
                    else
                    {
                        GameObject controller = MRET.InputRig.placingHand.gameObject;
                        part.transform.GetComponent<InteractablePart>().StartPlacing(controller.transform);
                    }
                }
            }
        }

        void FinishGameObjectInstantiation(GameObject obj, PartType part, Transform parent, bool placingMode)
        {
            if (obj == null)
            {
                Debug.LogError("Error loading gameobject.");
                return;
            }

            // Reset object rotation (for accurate render bounds).
            obj.transform.eulerAngles = Vector3.zero;

            Bounds bou = new Bounds(obj.transform.position, Vector3.zero);

            // TODO: hierarchical scaling is currently incorrect.
            // Create new bounds and add the bounds of all child objects together.
            if (part.AssetBundle != "NULL")
            {
                foreach (Renderer ren in obj.GetComponentsInChildren<Renderer>())
                {
                    bou.Encapsulate(ren.bounds);
                }

                Vector3 size = bou.size;
                Vector3 rescale = obj.transform.localScale;
                Vector3 dimensions = new Vector3(part.PartTransform.Scale.X, part.PartTransform.Scale.Y, part.PartTransform.Scale.Z);

                rescale.x = dimensions.x * rescale.x / size.x;
                rescale.y = dimensions.y * rescale.y / size.y;
                rescale.z = dimensions.z * rescale.z / size.z;

                obj.transform.localScale = rescale;
            }

            Collider collider = obj.GetComponent<Collider>();
            if (collider == null)
            {
                switch (MRET.ConfigurationManager.colliderMode)
                {
                    case ConfigurationManager.ColliderMode.Box:
                        Debug.Log("No collider detected. Generating box collder...");
                        // Recalculate the bounds.
                        bou = new Bounds(obj.transform.position, Vector3.zero);
                        foreach (Renderer ren in obj.GetComponentsInChildren<Renderer>())
                        {
                            bou.Encapsulate(ren.bounds);
                        }

                        collider = obj.AddComponent<BoxCollider>();
                        ((BoxCollider)collider).size = Vector3.Scale(bou.size,
                            new Vector3(1 / obj.transform.localScale.x, 1 / obj.transform.localScale.y, 1 / obj.transform.localScale.z));
                        ((BoxCollider)collider).center = obj.transform.InverseTransformPoint(bou.center);
                        obj.layer = MRET.previewLayer;
                        break;

                    case ConfigurationManager.ColliderMode.NonConvex:
                        Debug.Log("No collider detected. Generating non-convex colliders...");
                        foreach (MeshFilter mesh in obj.GetComponentsInChildren<MeshFilter>())
                        {
                            NonConvexMeshCollider ncmc = mesh.gameObject.AddComponent<NonConvexMeshCollider>();
                            ncmc.boxesPerEdge = 20;
                            ncmc.Calculate();
                            mesh.gameObject.layer = MRET.previewLayer;
                        }
                        break;

                    case ConfigurationManager.ColliderMode.None:
                    default:
                        Debug.Log("No collider detected. Not generating collider.");
                        break;
                }
            }
            else
            {
                Debug.Log("Collider already found.");
                foreach (Collider coll in obj.GetComponents<Collider>())
                {
                    coll.enabled = true;
                }
            }

            // Add colliders to be used for raycasting.
            AddRaycastMeshColliders(obj);

            if (part.AssetBundle != "NULL")
            {
                obj.transform.position = new Vector3(part.PartTransform.Position.X, part.PartTransform.Position.Y, part.PartTransform.Position.Z);
                obj.transform.rotation = new Quaternion(part.PartTransform.Rotation.X, part.PartTransform.Rotation.Y, part.PartTransform.Rotation.Z, part.PartTransform.Rotation.W);
            }
            obj.transform.SetParent((parent == null) ? projectObjectContainer.transform : parent);

            // Get/Set all Mandatory Components.
            ApplyStandardPropertiesToPart(obj, part, placingMode);

            InteractablePart iPart = obj.GetComponent<InteractablePart>();
            if (iPart != null)
            {
                if (iPart.grabCube != null)
                {
                    PlaceGrabCube(iPart.grabCube);
                }
            }

            part.transform = obj.transform;
            part.loaded = true;
        }

        void FinishEnclosureInstantiation(GameObject obj, PartType part, Transform parent)
        {
            if (obj == null)
            {
                Debug.LogError("Error loading gameobject.");
                return;
            }

            obj.transform.position = new Vector3(part.PartTransform.Position.X, part.PartTransform.Position.Y, part.PartTransform.Position.Z);
            obj.transform.rotation = Quaternion.Euler(part.PartTransform.Rotation.X, part.PartTransform.Rotation.Y, part.PartTransform.Rotation.Z);
            obj.transform.localScale = new Vector3(part.PartTransform.Scale.X, part.PartTransform.Scale.Y, part.PartTransform.Scale.Z);

            obj.transform.SetParent((parent == null) ? projectObjectContainer.transform : parent);

            // Ensure that a rigidbody is attached.
            Rigidbody rBody = obj.GetComponent<Rigidbody>();
            if (rBody == null)
            {
                rBody = obj.AddComponent<Rigidbody>();
                rBody.mass = 1;
                rBody.angularDrag = 0.99f;
                rBody.drag = 0.99f;
                rBody.useGravity = false;
                rBody.isKinematic = false;
            }

            // Set up enclosure.
            part.transform = obj.transform;
            AssemblyGrabber enclosureGrabber = obj.AddComponent<AssemblyGrabber>();
            enclosureGrabber.grabbable = true;
            enclosureGrabber.useable = false;
            enclosureGrabber.enabled = true;
            enclosureGrabber.assemblyRoot = parent.gameObject;
            parent.GetComponent<InteractablePart>().enclosure = part.transform.gameObject;
            if (part.EnableCollisions[0])
            {
                rBody.isKinematic = false;
                if (part.EnableGravity[0])
                {
                    rBody.useGravity = true;
                }
            }
            else
            {
                rBody.isKinematic = true;
            }

            part.transform = obj.transform;
            part.loaded = true;
        }

        protected IEnumerator InstantiateGameObjectAsync(PartType part, Transform parent, bool placingMode)
        {
            // Load asset from assetBundle.
            Action<object> action = (object loaded) =>
            {
                FinishGameObjectInstantiation((GameObject) loaded, part, parent, placingMode);
            };
            LoadPart(part, parent, placingMode, action);

            yield return null;
        }

        protected IEnumerator InstantiateEmptyGameObjectAsync(PartType part, Transform parent, bool placingMode)
        {
            // Instantiate empty game object as child of parent and position it.
            GameObject obj = new GameObject(part.Name);
            obj.transform.position = new Vector3(part.PartTransform.Position.X, part.PartTransform.Position.Y, part.PartTransform.Position.Z);
            obj.transform.rotation = Quaternion.Euler(part.PartTransform.Rotation.X, part.PartTransform.Rotation.Y, part.PartTransform.Rotation.Z);
            obj.transform.localScale = new Vector3(part.PartTransform.Scale.X, part.PartTransform.Scale.Y, part.PartTransform.Scale.Z);
            obj.transform.SetParent((parent == null) ? projectObjectContainer.transform : parent);

            // Get/Set all Mandatory Components.
            ApplyStandardPropertiesToPart(obj, part, placingMode);

            part.transform = obj.transform;
            part.loaded = true;
            yield return null;
        }

        protected IEnumerator InstantiateEnclosureAsync(PartType part, Transform parent)
        {
            // Load asset from assetBundle.
            Action<object> action = (object loaded) =>
            {
                FinishEnclosureInstantiation((GameObject) loaded, part, parent);
            };
            LoadPart(part, parent, false, action);

            yield return null;
        }

        private void ApplyStandardPropertiesToPart(GameObject obj, PartType part, bool placingMode)
        {
            // Apply default textures.
            if (part.RandomizeTexture)
            {
                int texToApply = UnityEngine.Random.Range(0, 8);
                foreach (MeshRenderer rend in obj.GetComponentsInChildren<MeshRenderer>())
                {
                    rend.material = MRET.DefaultPartMaterials[texToApply];
                }
            }

            // Get/Set all Mandatory Components.
            Rigidbody rBody = obj.GetComponent<Rigidbody>();
            if (rBody == null)
            {
                rBody = obj.AddComponent<Rigidbody>();
                rBody.mass = 1;
                rBody.angularDrag = 0.99f;
                rBody.drag = 0.99f;
                rBody.useGravity = false;
                rBody.isKinematic = false;
            }

            InteractablePart interactablePart = obj.GetComponent<InteractablePart>();
            if (placingMode)
            {
                interactablePart.StartAligning(MRET.InputRig.placingHand.gameObject);
            }
            interactablePart.grabbable = true;
            interactablePart.useable = true;
            interactablePart.headsetObject = MRET.InputRig.head.transform;
            interactablePart.partPanelPrefab = partPanelPrefab;
            interactablePart.highlightColor = partTouchHighlightColor;
            interactablePart.enabled = true;

            // Apply configurations.
            interactablePart.grabbable = part.EnableInteraction[0];
            interactablePart.useable = !part.NonInteractable;
            if (part.EnableCollisions[0])
            {
                rBody.isKinematic = false;
                if (part.EnableGravity[0])
                {
                    rBody.useGravity = true;
                }
            }
            else
            {
                rBody.isKinematic = true;
            }
        }

        private const int MAXPLACEMENTITERATIONS = 1000;
        private void PlaceGrabCube(GameObject grabCube)
        {
            int iterations = 0;

            // Get parent and initialize grab cube at center of assembly.
            Transform parentTransform = grabCube.transform.parent;
            Bounds bou = new Bounds(parentTransform.position, Vector3.zero);
            bool init = false;
            foreach (Renderer ren in parentTransform.GetComponentsInChildren<Renderer>())
            {
                if (ren.gameObject == grabCube)
                {
                    // Exclude assembly grabbers.
                    continue;
                }

                if (!init)
                {
                    // Initialize bounds.
                    bou = new Bounds(ren.bounds.center, Vector3.zero);
                    init = true;
                }
                else
                {
                    // Expand bounds.
                    bou.Encapsulate(ren.bounds);
                }
            }
            grabCube.transform.localPosition = bou.center;

            // Continuously move out grab cube until it is not encapsulated by part of the assembly.
            Collider[] assemblyColliders = parentTransform.GetComponentsInChildren<Collider>();
            foreach (Collider colliderToCheck in assemblyColliders)
            {
                if (!colliderToCheck.transform.IsChildOf(grabCube.transform))
                {
                    while (colliderToCheck.bounds.Contains(grabCube.transform.position))
                    {   // The grab cube is within an assembly object, so it will be moved further out.
                        grabCube.transform.position = new Vector3(grabCube.transform.position.x + 0.5f,
                            grabCube.transform.position.y + 0.5f, grabCube.transform.position.z + 0.5f);

                        if (++iterations > MAXPLACEMENTITERATIONS)
                        {
                            Debug.LogWarning("[PartManager->PlaceGrabCube] Unable to move grab cube out of object.");
                            break;
                        }
                    }
                }
            }
        }

        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            if (state == IntegrityState.Success)
            {
                if (partPanelPrefab == null)
                {
                    Debug.LogError("[" + ClassName + "->IntegrityCheck] Part Panel Prefab not assigned.");
                }
                else if (grabCubePrefab == null)
                {
                    Debug.LogError("[" + ClassName + "->IntegrityCheck] Grab Cube Prefab not assigned.");
                }
                else if (projectObjectContainer == null)
                {
                    Debug.LogError("[" + ClassName + "->IntegrityCheck] Project Object Container not assigned.");
                }
                else if (partTouchHighlightColor == null)
                {
                    Debug.LogError("[" + ClassName + "->IntegrityCheck] Part Touch Highlight Color not assigned.");
                }
                else
                {
                    return IntegrityState.Success;
                }
            }
            return IntegrityState.Failure;
        }
    }
}