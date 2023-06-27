// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.Utilities.Collider;
using GOV.NASA.GSFC.XR.MRET.Helpers;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing.Legacy;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_1;
using GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.Extensions.Ros;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Pin
{
    /// <remarks>
    /// History:
    /// 3 October 2022: Created
    /// </remarks>
    ///
    /// <summary>
    /// PinManagerDeprecated
    ///
    /// Manages Individual pins and pin and path relationships.
    /// <Works with cref="PinPanelControllerDeprecated"/>
    ///
    /// Author: Sean Letavish
    /// </summary>
    /// 
    public class PinManagerDeprecated : MRETBehaviour
    {
        public override string ClassName
        {
            get
            {
                return nameof(PinManagerDeprecated);
            }
        }

        [Tooltip("Determines what prefab is being used for pins")] ///REDO
        public bool pinToggle = true;

        [Tooltip("Boolean to determine if future pins will be combined as a path")]
        public bool pathModeEnabled = false;

        [Tooltip("Physcial size of the line mesh for the path")]
        public float PathLineSize = 0.05f;

        [Tooltip("Determines whether path is volumetric or basic")]
        public bool PathTypeVolumetric = true;

        [Tooltip("Text object that displays the pin number")]
        public GameObject PinPositionText;

        [Tooltip("Segment measurement text")]
        public GameObject segmentPathText;

        [Tooltip("Number of segements. Will be one less than number of pins")]
        public int PathSegments;

        [Tooltip("Pin Panel for IoT settings")]
        public GameObject individualPinPanel;

        [Tooltip("Data display panel")]
        public GameObject DataDisplayPanel;

        [Tooltip("Color for all pins and the pathline in a path")]
        public Color pinAndPathColor = Color.white;

        [Tooltip("Translucent material for the pin markers that are attached ")]
        public Material holoMaterial;

        //Measurement text from path
        MeasurementTextDeprecated mtext;

        //Toggle to see if measurement mode is enabled
        bool measurementenabled = false;

        //Number of paths in scene
        public int pathNumber = -1;

        //List of paths in scene
        public List<PathDeprecated> paths = new List<PathDeprecated>();

        //Number of pins in scene
        public int pinCount = 0;

        //Toggle for segment measurement mode
        public bool segmentedMeasurementEnabled = false;

        //Measurement that will be destroyed
        MeasurementTextDeprecated measurementToDestroy;

        //ID for IoT
        public string PinID;

        //Pin name
        public static string PinName;

        /// <summary>
        /// Initialize method necessary for MRET 
        /// <seealso cref="MRET"/>
        /// </summary>
        public void Initialize()
        {

        }

        /// <summary>
        /// Creates a new line drawing in the scene and adds it to Path
        /// </summary>
        public void CreateNewPath()
        {
            pathNumber = pathNumber + 1;
            pinCount = 0;
            pathModeEnabled = true;
            LineDrawingDeprecated LD = null;

            if (PathTypeVolumetric)
            {
                Vector3[] positions = new Vector3[0];
                LD = ProjectManager.SceneObjectManagerDeprecated.CreateLineDrawing("Pathnumber" + pathNumber, null, Vector3.zero, Quaternion.Euler(0, 0, 0), Vector3.one, LineDrawingManagerDeprecated.DrawingType.Volumetric, PathLineSize, pinAndPathColor, positions);

            }

            else if (!PathTypeVolumetric)
            {
                Vector3[] positions = new Vector3[0];
                LD = ProjectManager.SceneObjectManagerDeprecated.CreateLineDrawing("Pathnumber" + pathNumber, null, Vector3.zero, Quaternion.Euler(0, 0, 0), Vector3.one, LineDrawingManagerDeprecated.DrawingType.Basic, PathLineSize, pinAndPathColor, positions);
            }

            PathDeprecated path = new PathDeprecated(LD);
            paths.Add(path);

            if (measurementenabled)
            {
                DisplayMeasurements();
            }
        }

        /// <summary>
        /// Disables boolean
        /// <seealso cref="PinPanelControllerDeprecated"/>
        /// </summary>
        public void EndPath()
        {
            pathModeEnabled = false;
        }

        /// <summary>
        /// CURRENT WORK IN PROGRESS
        /// Toggles between pin and flag
        /// </summary>
        public void PinToggle()
        {
            pinToggle = !pinToggle; //Pin==true, Pin prefab will be used. Pin==false, Flag prefab will be used
            DestroyHoloPin();
        }

        /// <summary>
        /// Set 'color' determines what color the pins and their corresponding path with be
        /// </summary>
        public void SetWhite()
        {
            pinAndPathColor = Color.white;
        }

        public void SetGreen()
        {
            pinAndPathColor = Color.green;
        }

        public void SetBlue()
        {
            pinAndPathColor = Color.blue;
        }

        public void SetRed()
        {
            pinAndPathColor = Color.red;
        }

        /// <summary>
        /// Deletes the last pin that was created in the scene, and updates the path if necessary
        /// </summary>
        public void DeleteLastPin()
        {

            List<InteractablePinDeprecated> pins = new List<InteractablePinDeprecated>();



            foreach (InteractablePinDeprecated pin in MRET.ProjectDeprecated.projectObjectContainer.GetComponentsInChildren<InteractablePinDeprecated>())
            {
                pins.Add(pin);
            }

            InteractablePinDeprecated pinToDestroy = pins.Last();

            if (pathNumber >= 0)
            {
                if (paths[pathNumber].segmentMeasurements.Count != 0)
                {
                    measurementToDestroy = paths[pathNumber].segmentMeasurements.Last();
                }
            }


            foreach (PathDeprecated path in paths)
            {
                path.DeletePin(pinToDestroy);

                if (pathNumber >= 0)
                {
                    if (paths[pathNumber].segmentMeasurements.Count != 0)
                    {
                        path.DeleteSegmentMeasurement(measurementToDestroy);
                    }
                }
                UpdatePath(path);
            }

            Destroy(pins.Last().gameObject);
            if (pathNumber >= 0)
            {
                if (paths[pathNumber].segmentMeasurements.Count != 0)
                {
                    Destroy(measurementToDestroy.gameObject);
                }
            }
            pinCount--;
            PathSegments--;

            if (measurementenabled && pins.Count > 2)
            {
                InteractablePinDeprecated lastPin = pins[pins.Count - 2];
                mtext = paths[pathNumber].mtext;
                mtext.transform.position = lastPin.transform.position;
                mtext.transform.position = new Vector3(mtext.transform.position.x + 0.25f, mtext.transform.position.y, mtext.transform.position.z);
            }
        }

        /// <summary>
        /// Detroys the holographic marker attached to the hand. Will need to be reworked
        /// </summary>
        public void DestroyHoloPin()
        {

            foreach (PinMarkerDeprecated pin in MRET.InputRig.GetComponentsInChildren<PinMarkerDeprecated>(true))
            {
                
                Destroy(pin.gameObject);
                
            }
        }

        /// <summary>
        /// Calls the update function of Path
        /// </summary>
        /// <param name="path"></param>
        public void UpdatePath(PathDeprecated path)
        {
            path.Update();
        }

        /// <summary>
        /// Updates the current path
        /// </summary>
        public void UpdateCurrentPath()
        {
            paths[pathNumber].Update();
        }

        /// <summary>
        /// Adds a pin to the current path. If measurements are enabled, 
        /// the measurement text object of the path will follow the user's 
        /// most recent pin placement
        /// </summary>
        /// <param name="pin"></param>
        public void AddPin(InteractablePinDeprecated pin)
        {
            if (pin && pathModeEnabled)
            {
                paths[pathNumber].AddPin(pin);
                GameObject pinTextGO = Instantiate(PinPositionText, FindHighestVert(pin.GetComponent<MeshFilter>()), Quaternion.Euler(Vector3.zero));
                pinTextGO.transform.SetParent(pin.transform);
                pinTextGO.transform.localScale = new Vector3(0.1f, 0.01f, 0.01f);
                pinTextGO.GetComponentInChildren<Text>().color = pinAndPathColor;
                pinTextGO.GetComponentInChildren<Text>().text = pinCount.ToString();
                pinCount++;
                PathSegments = pinCount - 1;
                UpdateCurrentPath();

                if (measurementenabled)
                {
                    mtext = paths[pathNumber].mtext;
                    mtext.transform.position = pin.transform.position;
                    mtext.transform.position = new Vector3(mtext.transform.position.x + 0.25f, mtext.transform.position.y, mtext.transform.position.z);
                }

                if (segmentedMeasurementEnabled)
                {
                    paths[pathNumber].SegmentPins();

                    if(paths[pathNumber].PinA!=new Vector3(0,0,0) && paths[pathNumber].PinB!=new Vector3(0,0,0))
                    {
                        float averagex = (paths[pathNumber].PinA.x + paths[pathNumber].PinB.x) / 2;
                        float averagey = (paths[pathNumber].PinA.y + paths[pathNumber].PinB.y) / 2;
                        float averagez = (paths[pathNumber].PinA.z + paths[pathNumber].PinB.z) / 2;
                        paths[pathNumber].segmentMeasurements[PathSegments].transform.position = new Vector3(averagex, averagey, averagez);
                    }

                    else if (paths[pathNumber].PinA != new Vector3(0, 0, 0) && paths[pathNumber].PinB == new Vector3(0, 0, 0))
                    {
                        paths[pathNumber].segmentMeasurements[PathSegments].transform.position = pin.transform.position;
                    }
                        
                }

            }

        }

        public void SetPathType()
        {
            PathTypeVolumetric = !PathTypeVolumetric;
        }

        public void DisplayMeasurements()
        {
            measurementenabled = true;
            foreach (PathDeprecated path in paths)
            {
                path.DisplayMeasurements();
            }
        }

        public void DisableMeasurements()
        {
            measurementenabled = false;
            foreach (PathDeprecated path in paths)
            {
                path.DisableMeasurements();
            }
        }

        /// <summary>
        /// Finds the highest polygon of any mesh
        /// </summary>
        /// <param name="targetMesh"></param>
        /// <returns></returns>
        Vector3 FindHighestVert(MeshFilter targetMesh)
        {
            var maxBounds = targetMesh.sharedMesh.bounds.max;
            Matrix4x4 localToWorld = targetMesh.transform.localToWorldMatrix;
            Vector3 high = localToWorld.MultiplyPoint3x4(maxBounds);
            return high;
        }

        public void InstantiatePartInEnvironment(PartType part, Transform parent)
        {
            StartCoroutine(InstantiateGameObjectAsync(part, parent, false));
        }

        public void InstantiatePin(PartType part, Transform parent)
        {
            StartCoroutine(InstantiateGameObjectAsync(part, parent, true));
        }

        // Note: The plan is to have preprocessing done on the serialized part info
        // if it is using an older schema version so that the logic at this level
        // is uniform.
        public void LoadPart(PartType serializedPartInfo, Transform parent, bool placingMode, Action<GameObject> onLoaded)
        {
            PinID = serializedPartInfo.ID;


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
                string assetName = (serializedPartInfo.PartName == null) ? serializedPartInfo.Name : serializedPartInfo.PartName;

                // Load from asset bundle.
                Action<GameObject> action = (GameObject loadedPart) => {
                    OnPartLoaded(loadedPart, null, serializedPartInfo, onLoaded, placingMode);
                };
                AssetBundleHelper.Instance.LoadAssetAsync(serializedPartInfo.AssetBundle, assetName, action);
            }
        }

        public void OnPartLoaded(GameObject loadedPart, Transform parent, PartType part,
            Action<GameObject> onLoaded, bool placingMode, AnimationClip[] animations = null)
        {
            StartCoroutine(OnPartLoadedAsync(loadedPart, parent, part, onLoaded, placingMode, animations));
        }


        public IEnumerator OnPartLoadedAsync(GameObject loadedPart, Transform parent, PartType part,
            Action<GameObject> onLoaded, bool placingMode, AnimationClip[] animations = null)
        {
            if (loadedPart == null)
            {
                loadedPart = new GameObject(part.Name);
            }

            part.transform = loadedPart.transform;

            PinMarkerDeprecated newPin = loadedPart.AddComponent<PinMarkerDeprecated>();


            GameObject grabCube = null;

            InteractablePinDeprecated iPin = part.transform.GetComponent<InteractablePinDeprecated>();
            // TODO: remove this, it's Temporary for the mms demo.
           


            if (part.ChildParts != null && part.ChildParts.Parts != null && part.ChildParts.Parts.Length > 0)
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

                
                grabCube.transform.SetParent(part.transform);
                grabCube.GetComponent<AssemblyGrabberDeprecated>().assemblyRoot = part.transform.gameObject;
                if (part.Enclosure != null)
                {
                    grabCube.GetComponent<AssemblyGrabberDeprecated>().otherGrabbers.Add(part.Enclosure.transform.GetComponent<AssemblyGrabberDeprecated>());

                    AssemblyGrabberDeprecated grabber = part.Enclosure.transform.GetComponent<AssemblyGrabberDeprecated>();
                    grabber.otherGrabbers.Add(grabCube.GetComponent<AssemblyGrabberDeprecated>());
                    grabber.assetBundle = part.Enclosure.AssetBundle;
                    if (part.Enclosure.Description != null)
                    {
                        grabber.description = (part.Enclosure.Description[0] == null) ? "" : part.Enclosure.Description;
                    }
                    grabber.dimensions = new Vector3(part.Enclosure.PartTransform.Scale.X, part.Enclosure.PartTransform.Scale.Y, part.Enclosure.PartTransform.Scale.Z);
                    grabber.id = part.Enclosure.ID;
                    grabber.minMass = part.Enclosure.MinMass;
                    grabber.maxMass = part.Enclosure.MaxMass;
                    grabber.serializationName = part.Enclosure.Name;
                    grabber.gameObject.name = part.Enclosure.Name;
                    grabber.notes = part.Enclosure.Notes;
                    if (!string.IsNullOrEmpty(part.Enclosure.PartFileName))
                    {
                        grabber.partFileName = part.Enclosure.PartFileName;
                    }
                    if (!string.IsNullOrEmpty(part.Enclosure.PartName))
                    {
                        grabber.partName = part.Enclosure.PartName;
                    }
                    grabber.partType = part.Enclosure.PartType1;
                    grabber.reference = part.Enclosure.Reference;
                    grabber.subsystem = part.Enclosure.Subsystem;
                    if (!string.IsNullOrEmpty(part.Enclosure.Vendor))
                    {
                        grabber.vendor = part.Enclosure.Vendor;
                    }
                    grabber.version = part.Enclosure.Version;
                }
                part.transform.GetComponent<InteractablePartDeprecated>().grabCube = grabCube;

                // If this isn't the root object, hide the grab cube.
                if (parent != null)
                {
                    grabCube.SetActive(false);
                }
            }

            onLoaded.Invoke(loadedPart);

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
                                if (tr != transformToAttachTo && tr.GetComponent<InteractablePartDeprecated>() != null)
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

            // Use the collider utility in case an alternate solution to Collider is used for non-convex
            // colliders
            if (!ColliderUtil.HasColliderInChildren(obj))
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

                        BoxCollider collider = obj.AddComponent<BoxCollider>();
                        collider.size = Vector3.Scale(bou.size,
                            new Vector3(1 / obj.transform.localScale.x, 1 / obj.transform.localScale.y, 1 / obj.transform.localScale.z));
                        collider.center = obj.transform.InverseTransformPoint(bou.center);
                        obj.layer = MRET.previewLayer;
                        break;

                    case ConfigurationManager.ColliderMode.NonConvex:
                        Debug.Log("No collider detected. Generating non-convex colliders...");
                        ColliderUtil.CreateNonConvexMeshColliders(gameObject);
                        foreach (MeshFilter mesh in gameObject.GetComponentsInChildren<MeshFilter>())
                        {
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


            if (part.AssetBundle != "NULL")
            {
                obj.transform.position = new Vector3(part.PartTransform.Position.X, part.PartTransform.Position.Y, part.PartTransform.Position.Z);
                obj.transform.rotation = new Quaternion(part.PartTransform.Rotation.X, part.PartTransform.Rotation.Y, part.PartTransform.Rotation.Z, part.PartTransform.Rotation.W);
            }
           

            // Get/Set all Mandatory Components.
            ApplyStandardPropertiesToPart(obj, part, placingMode);

            InteractablePartDeprecated iPart = obj.GetComponent<InteractablePartDeprecated>();
            if (iPart != null)
            {
                if (iPart.grabCube != null)
                {
                    PlaceGrabCube(iPart.grabCube);
                }
            }

            part.transform = obj.transform;

            // If there is a ROSConnectionType specified in the Part XML, call the ROS manager to initialize the objects and components
            // to set up the desired ROS connection.
            if (part.ROSConnection != null)
            {
                RosManagerDeprecated.AddRosConnection(obj, part.ROSConnection);
            }

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

           

            // Set up enclosure.
            part.transform = obj.transform;
            AssemblyGrabberDeprecated enclosureGrabber = obj.AddComponent<AssemblyGrabberDeprecated>();
            enclosureGrabber.grabbable = true;
            enclosureGrabber.useable = false;
            enclosureGrabber.enabled = true;
            enclosureGrabber.assemblyRoot = parent.gameObject;
            parent.GetComponent<InteractablePartDeprecated>().enclosure = part.transform.gameObject;


            part.transform = obj.transform;
            part.loaded = true;
        }

        protected IEnumerator InstantiateGameObjectAsync(PartType part, Transform parent, bool placingMode)
        {
            // Load asset from assetBundle.
            Action<object> action = (object loaded) =>
            {
                FinishGameObjectInstantiation((GameObject)loaded, part, parent, placingMode);
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
           // obj.transform.SetParent((parent == null) ? projectObjectContainer.transform : parent);

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
                FinishEnclosureInstantiation((GameObject)loaded, part, parent);
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
                    rend.material = ProjectManager.DefaultPartMaterials[texToApply];
                }
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

    }

}

