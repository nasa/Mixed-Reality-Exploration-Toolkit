// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using GSFC.ARVR.MRET.Components.LineDrawing;
using GSFC.ARVR.MRET.Infrastructure.Components.ModelLoading;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.SceneObject
{
    /// <remarks>
    /// History:
    /// 4 April 2021: Created
    /// 13 December 2021: Added volumetric drawing fields.
    /// 5 January 2021: Fixed bug with destroying scene objects (DZB)
    /// </remarks>
    /// <summary>
    /// Manager for SceneObjects in MRET.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class SceneObjectManager : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(SceneObjectManager);
            }
        }

        /// <summary>
        /// Tag for climbables.
        /// </summary>
        public static readonly string CLIMBABLETAG = "CLIMBABLE";

        /// <summary>
        /// Container for all scene objects.
        /// </summary>
        public GameObject sceneObjectRoot;

        /// <summary>
        /// Prefab for a volumetric drawing segment.
        /// </summary>
        public GameObject volumetricDrawingSegmentPrefab;

        /// <summary>
        /// Prefab for a volumetric drawing corner.
        /// </summary>
        public GameObject volumetricDrawingCornerPrefab;

        private SceneObject.GrabBehavior _grabBehavior = Interactable.Interactable.GrabBehavior.Attach;
        public SceneObject.GrabBehavior grabBehavior
        {
            get
            {
                return _grabBehavior;
            }
            set
            {
                foreach (SceneObject sceneObject in sceneObjects.Values)
                {
                    Debug.Log("setting " + value);
                    sceneObject.grabBehavior = value;
                }
                _grabBehavior = value;
            }
        }

        /// <summary>
        /// Dictionary of maintained SceneObjects and their UUIDs.
        /// </summary>
        private Dictionary<Guid, SceneObject> sceneObjects = new Dictionary<Guid, SceneObject>();

        /// <summary>
        /// Maintained SceneObjects.
        /// </summary>
        public SceneObject[] SceneObjects
        {
            get
            {
                SceneObject[] rtn = new SceneObject[sceneObjects.Count];
                sceneObjects.Values.CopyTo(rtn, 0);
                return rtn;
            }
        }

        /// <summary>
        /// Maintained InteractableParts.
        /// </summary>
        public InteractablePart[] InteractableParts
        {
            get
            {
                List<InteractablePart> rtn = new List<InteractablePart>();
                foreach (KeyValuePair<Guid, SceneObject> kvp in sceneObjects)
                {
                    if (kvp.Value is InteractablePart)
                    {
                        rtn.Add((InteractablePart) kvp.Value);
                    }
                }
                return rtn.ToArray();
            }
        }

        /// <summary>
        /// Maintained InteractableNotes.
        /// </summary>
        public InteractableNote[] interactableNotes
        {
            get
            {
                List<InteractableNote> rtn = new List<InteractableNote>();
                foreach (KeyValuePair<Guid, SceneObject> kvp in sceneObjects)
                {
                    if (kvp.Value is InteractableNote)
                    {
                        rtn.Add((InteractableNote) kvp.Value);
                    }
                }
                return rtn.ToArray();
            }
        }

        /// <summary>
        /// Maintained LineDrawings.
        /// </summary>
        public ARVR.MRET.Components.LineDrawing.LineDrawing[] lineDrawings
        {
            get
            {
                List<ARVR.MRET.Components.LineDrawing.LineDrawing> rtn = new List<ARVR.MRET.Components.LineDrawing.LineDrawing>();
                foreach(KeyValuePair<Guid, SceneObject> kvp in sceneObjects)
                {
                    if (kvp.Value is ARVR.MRET.Components.LineDrawing.LineDrawing)
                    {
                        rtn.Add((ARVR.MRET.Components.LineDrawing.LineDrawing) kvp.Value);
                    }
                }
                return rtn.ToArray();
            }
        }

        public void Initialize()
        {
            DataManager.instance.SaveValue(SceneObject.motionConstraintsKey, false);
        }

        /// <summary>
        /// Get a SceneObject by its UUID.
        /// </summary>
        /// <param name="uuidToFind">UUID of the SceneObject in question.</param>
        /// <returns>The corresponding SceneObject or null.</returns>
        public SceneObject GetSceneObjectByUUID(Guid uuidToFind)
        {
            SceneObject sceneObject;
            sceneObjects.TryGetValue(uuidToFind, out sceneObject);
            return sceneObject;
        }

        /// <summary>
        /// Temporary for registering scene objects.
        /// </summary>
        /// <param name="objectToRegister">Object to register.</param>
        /// <param name="id">ID to assign.</param>
        public void RegisterSceneObject(SceneObject objectToRegister, Guid id)
        {
            sceneObjects.Add(id, objectToRegister);
        }

        /// <summary>
        /// Create a SceneObject.
        /// </summary>
        /// <param name="sceneObjectName">Name of the SceneObject.</param>
        /// <param name="parent">Parent for the SceneObject.</param>
        /// <param name="localPosition">Local position of the SceneObject.</param>
        /// <param name="localRotation">Local rotation of the SceneObject.</param>
        /// <param name="localScale">Local scale of the SceneObject.</param>
        /// <param name="uuid">UUID of the SceneObject.</param>
        /// <returns>The created SceneObject.</returns>
        public SceneObject CreateSceneObject(string sceneObjectName, SceneObject parent,
            Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Guid? uuid = null)
        {
            SceneObject newSceneObject = SceneObject.Create();
            newSceneObject.gameObject.name = sceneObjectName;

            if (parent == null)
            {
                newSceneObject.transform.SetParent(sceneObjectRoot.transform);
            }
            else
            {
                newSceneObject.transform.SetParent(parent.transform);
            }
            newSceneObject.transform.localPosition = localPosition;
            newSceneObject.transform.localRotation = localRotation;
            newSceneObject.transform.localScale = localScale;

            if (uuid.HasValue)
            {
                newSceneObject.uuid = uuid.Value;
            }
            else
            {
                newSceneObject.uuid = Guid.NewGuid();
            }
            newSceneObject.grabBehavior = grabBehavior;

            sceneObjects.Add(newSceneObject.uuid, newSceneObject);

            return newSceneObject;
        }

        /// <summary>
        /// Create a SceneObject.
        /// </summary>
        /// <param name="sceneObjectName">Name of the SceneObject.</param>
        /// <param name="parent">UUID for the parent for the SceneObject.</param>
        /// <param name="localPosition">Local position of the SceneObject.</param>
        /// <param name="localRotation">Local rotation of the SceneObject.</param>
        /// <param name="localScale">Local scale of the SceneObject.</param>
        /// <param name="uuid">UUID of the SceneObject.</param>
        /// <returns>The created SceneObject.</returns>
        public SceneObject CreateSceneObject(string sceneObjectName, Guid parentUUID,
            Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Guid? uuid = null)
        {
            SceneObject parent = GetSceneObjectByUUID(parentUUID);
            if (parent == null)
            {
                Debug.LogWarning("[SceneObjectManager->CreateSceneObject] Parent UUID not found. Setting parent to null.");
            }

            return CreateSceneObject(sceneObjectName, parent, localPosition, localRotation, localScale, uuid);
        }

        /// <summary>
        /// Destroy a SceneObject.
        /// </summary>
        /// <param name="sceneObjectToDestroy">UUID of the SceneObject to destroy.</param>
        public void DestroySceneObject(Guid sceneObjectToDestroy)
        {
            if (sceneObjects.ContainsKey(sceneObjectToDestroy))
            {
                SceneObject soToDestroy = sceneObjects[sceneObjectToDestroy];
                sceneObjects.Remove(sceneObjectToDestroy);
                Destroy(soToDestroy.gameObject);
            }
            else
            {
                Debug.LogWarning("[SceneObjectManager->DestroySceneObject] SceneObject not found.");
            }
        }

        /// <summary>
        /// Create an InteractablePart.
        /// </summary>
        /// <param name="partName">Name of the InteractablePart.</param>
        /// <param name="parent">Parent for the InteractablePart.</param>
        /// <param name="localPosition">Local position of the InteractablePart.</param>
        /// <param name="localRotation">Local rotation of the InteractablePart.</param>
        /// <param name="localScale">Local scale of the InteractablePart.</param>
        /// <param name="uuid">UUID of the InteractablePart.</param>
        /// <returns>The created InteractablePart.</returns>
        public InteractablePart CreateInteractablePart(string partName, SceneObject parent,
            Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Guid? uuid = null)
        {
            InteractablePart newInteractablePart = InteractablePart.Create();
            newInteractablePart.gameObject.name = partName;

            if (parent == null)
            {
                newInteractablePart.transform.SetParent(sceneObjectRoot.transform);
            }
            else
            {
                newInteractablePart.transform.SetParent(parent.transform);
            }
            newInteractablePart.transform.localPosition = localPosition;
            newInteractablePart.transform.localRotation = localRotation;
            newInteractablePart.transform.localScale = localScale;

            if (uuid.HasValue)
            {
                newInteractablePart.uuid = uuid.Value;
            }
            else
            {
                newInteractablePart.uuid = Guid.NewGuid();
            }
            newInteractablePart.grabBehavior = grabBehavior;
            newInteractablePart.shadeForLimitViolations = false;

            sceneObjects.Add(newInteractablePart.uuid, newInteractablePart);

            return newInteractablePart;
        }

        public InteractablePart CreateInteractablePartWithScale(string partName, string AssetBundle, SceneObject parent,
            Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Guid? uuid = null)
        {
            InteractablePart newInteractablePart = InteractablePart.Create(partName, AssetBundle, parent, Vector3.one, uuid);
            newInteractablePart.gameObject.name = partName;

            if (parent == null)
            {
                newInteractablePart.transform.SetParent(sceneObjectRoot.transform);
            }
            else
            {
                newInteractablePart.transform.SetParent(parent.transform);
            }
            newInteractablePart.transform.localPosition = localPosition;
            newInteractablePart.transform.localRotation = localRotation;
            newInteractablePart.transform.localScale = localScale;

            if (uuid.HasValue)
            {
                newInteractablePart.uuid = uuid.Value;
            }
            else
            {
                newInteractablePart.uuid = Guid.NewGuid();
            }
            newInteractablePart.grabBehavior = grabBehavior;
            newInteractablePart.shadeForLimitViolations = false;

            sceneObjects.Add(newInteractablePart.uuid, newInteractablePart);

            return newInteractablePart;
        }

        public InteractablePart CreateInteractablePartWithSize(string partName, string AssetBundle, SceneObject parent,
            Vector3 localPosition, Quaternion localRotation, Vector3 size, Guid? uuid = null)
        {
            InteractablePart newInteractablePart = InteractablePart.Create(partName, AssetBundle, parent, size, uuid);
            newInteractablePart.gameObject.name = partName;

            if (parent == null)
            {
                newInteractablePart.transform.SetParent(sceneObjectRoot.transform);
            }
            else
            {
                newInteractablePart.transform.SetParent(parent.transform);
            }
            newInteractablePart.transform.localPosition = localPosition;
            newInteractablePart.transform.localRotation = localRotation;

            if (uuid.HasValue)
            {
                newInteractablePart.uuid = uuid.Value;
            }
            else
            {
                newInteractablePart.uuid = Guid.NewGuid();
            }
            newInteractablePart.grabBehavior = grabBehavior;
            newInteractablePart.shadeForLimitViolations = false;

            sceneObjects.Add(newInteractablePart.uuid, newInteractablePart);

            return newInteractablePart;
        }

        /// <summary>
        /// Creates a LineDrawing.
        /// </summary>
        /// <param name="drawingName">Name of the LineDrawing.</param>
        /// <param name="parent">Parent for the LineDrawing.</param>
        /// <param name="localPosition">Local position of the LineDrawing.</param>
        /// <param name="localRotation">Local rotation of the LineDrawing.</param>
        /// <param name="localScale">Local scale of the LineDrawing.</param>
        /// <param name="type">Type of the LineDrawing.</param>
        /// <param name="width">Width or diameter of the LineDrawing.</param>
        /// <param name="color">Color of the LineDrawing.</param>
        /// <param name="positions">Positions of points in the LineDrawing.</param>
        /// <param name="uuid">UUID of the LineDrawing.</param>
        /// <returns>The created LineDrawing.</returns>
        public ARVR.MRET.Components.LineDrawing.LineDrawing CreateLineDrawing(string drawingName,
            SceneObject parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale,
            LineDrawing.LineDrawingManager.DrawingType type, float width, Color32 color,
            Vector3[] positions, Guid? uuid = null)
        {
            ARVR.MRET.Components.LineDrawing.LineDrawing newLineDrawing = null;
            switch (type)
            {
                case LineDrawing.LineDrawingManager.DrawingType.Basic:
                    newLineDrawing = ARVR.MRET.Components.LineDrawing.LineDrawing.Create(
                        drawingName, width, color, positions, uuid);
                    break;

                case LineDrawing.LineDrawingManager.DrawingType.Volumetric:
                    newLineDrawing = VolumetricDrawing.Create(drawingName,
                        width, color, positions, uuid);
                    break;

                default:
                    Debug.LogError("[SceneObjectManager->CreateLineDrawing] Invalid drawing type.");
                    return null;
            }

            if (parent == null)
            {
                newLineDrawing.transform.SetParent(sceneObjectRoot.transform);
            }
            else
            {
                newLineDrawing.transform.SetParent(parent.transform);
            }
            newLineDrawing.transform.localPosition = localPosition;
            newLineDrawing.transform.localRotation = localRotation;
            newLineDrawing.transform.localScale = localScale;

            newLineDrawing.grabBehavior = grabBehavior;

            sceneObjects.Add(newLineDrawing.uuid, newLineDrawing);

            return newLineDrawing;
        }

        /// <summary>
        /// Create an InteractablePart.
        /// </summary>
        /// <param name="partName">Name of the InteractablePart.</param>
        /// <param name="parent">UUID of the parent for the InteractablePart.</param>
        /// <param name="localPosition">Local position of the InteractablePart.</param>
        /// <param name="localRotation">Local rotation of the InteractablePart.</param>
        /// <param name="localScale">Local scale of the InteractablePart.</param>
        /// <param name="uuid">UUID of the InteractablePart.</param>
        /// <returns>The created InteractablePart.</returns>
        public InteractablePart CreateInteractablePart(string partName, Guid parentUUID,
            Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Guid? uuid = null)
        {
            SceneObject parent = GetSceneObjectByUUID(parentUUID);
            if (parent == null)
            {
                Debug.LogWarning("[SceneObjectManager->CreateInteractablePart] Parent UUID not found. Setting parent to null.");
            }

            return CreateInteractablePart(partName, parent, localPosition, localRotation, localScale, uuid);
        }

        /// <summary>
        /// Create an InteractableNote.
        /// </summary>
        /// <param name="noteName">Name of the InteractableNote.</param>
        /// <param name="parent">Parent for the InteractableNote.</param>
        /// <param name="localPosition">Local position of the InteractableNote.</param>
        /// <param name="localRotation">Local rotation of the InteractableNote.</param>
        /// <param name="localScale">Local scale of the InteractableNote.</param>
        /// <param name="uuid">UUID of the InteractableNote.</param>
        /// <returns>The created InteractableNote.</returns>
        public InteractableNote CreateInteractableNote(string noteName, SceneObject parent,
            Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Guid? uuid = null)
        {
            InteractableNote newInteractableNote = InteractableNote.Create();
            newInteractableNote.gameObject.name = noteName;

            if (parent == null)
            {
                newInteractableNote.transform.SetParent(sceneObjectRoot.transform);
            }
            else
            {
                newInteractableNote.transform.SetParent(parent.transform);
            }
            newInteractableNote.transform.localPosition = localPosition;
            newInteractableNote.transform.localRotation = localRotation;
            newInteractableNote.transform.localScale = localScale;

            if (uuid.HasValue)
            {
                newInteractableNote.uuid = uuid.Value;
            }
            else
            {
                newInteractableNote.uuid = Guid.NewGuid();
            }
            newInteractableNote.grabBehavior = grabBehavior;

            sceneObjects.Add(newInteractableNote.uuid, newInteractableNote);

            return newInteractableNote;
        }

        /// <summary>
        /// Create an InteractableNote.
        /// </summary>
        /// <param name="noteName">Name of the InteractableNote.</param>
        /// <param name="parent">UUID of the parent for the InteractableNote.</param>
        /// <param name="localPosition">Local position of the InteractableNote.</param>
        /// <param name="localRotation">Local rotation of the InteractableNote.</param>
        /// <param name="localScale">Local scale of the InteractableNote.</param>
        /// <param name="uuid">UUID of the InteractableNote.</param>
        /// <returns>The created InteractableNote.</returns>
        public InteractableNote CreateInteractableNote(string noteName, Guid parentUUID,
            Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Guid? uuid = null)
        {
            SceneObject parent = GetSceneObjectByUUID(parentUUID);
            if (parent == null)
            {
                Debug.LogWarning("[SceneObjectManager->CreateInteractableNote] Parent UUID not found. Setting parent to null.");
            }

            return CreateInteractableNote(noteName, parent, localPosition, localRotation, localScale, uuid);
        }

        /// <summary>
        /// Create an InteractableDrawing.
        /// </summary>
        /// <param name="drawingName">Name of the InteractableDrawing.</param>
        /// <param name="parent">Parent for the InteractableDrawing.</param>
        /// <param name="localPosition">Local position of the InteractableDrawing.</param>
        /// <param name="localRotation">Local rotation of the InteractableDrawing.</param>
        /// <param name="localScale">Local scale of the InteractableDrawing.</param>
        /// <param name="uuid">UUID of the InteractableDrawing.</param>
        /// <returns>The created InteractableDrawing.</returns>
        public InteractableDrawing CreateInteractableDrawing(string drawingName, SceneObject parent,
            Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Guid? uuid = null)
        {
            InteractableDrawing newInteractableDrawing = InteractableDrawing.Create();
            newInteractableDrawing.gameObject.name = drawingName;

            newInteractableDrawing.transform.SetParent(parent.transform);
            newInteractableDrawing.transform.localPosition = localPosition;
            newInteractableDrawing.transform.localRotation = localRotation;
            newInteractableDrawing.transform.localScale = localScale;

            if (uuid.HasValue)
            {
                newInteractableDrawing.uuid = uuid.Value;
            }
            else
            {
                newInteractableDrawing.uuid = Guid.NewGuid();
            }
            newInteractableDrawing.grabBehavior = grabBehavior;

            sceneObjects.Add(newInteractableDrawing.uuid, newInteractableDrawing);

            return newInteractableDrawing;
        }

        /// <summary>
        /// Create an InteractableDrawing.
        /// </summary>
        /// <param name="drawingName">Name of the InteractableDrawing.</param>
        /// <param name="parent">UUID of the parent for the InteractableDrawing.</param>
        /// <param name="localPosition">Local position of the InteractableDrawing.</param>
        /// <param name="localRotation">Local rotation of the InteractableDrawing.</param>
        /// <param name="localScale">Local scale of the InteractableDrawing.</param>
        /// <param name="uuid">UUID of the InteractableDrawing.</param>
        /// <returns>The created InteractableDrawing.</returns>
        public InteractableDrawing CreateInteractableDrawing(string drawingName, Guid parentUUID,
            Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Guid? uuid = null)
        {
            SceneObject parent = GetSceneObjectByUUID(parentUUID);
            if (parent == null)
            {
                Debug.LogWarning("[SceneObjectManager->CreateInteractableDrawing] Parent UUID not found. Setting parent to null.");
            }

            return CreateInteractableDrawing(drawingName, parent, localPosition, localRotation, localScale, uuid);
        }

        /// <summary>
        /// Adds a climbable to an existing SceneObject.
        /// </summary>
        /// <param name="sceneObject">UUID of the scene object to add the climbable to.</param>
        /// <param name="colliderVolumes">A list of collider volumes to grab for climbing. A list of Vector3,Vector3 tuples.</param>
        public void AddClimbableToSceneObject(Guid sceneObject, List<Tuple<Vector3, Vector3>> colliderVolumes)
        {
            AddClimbableToSceneObject(GetSceneObjectByUUID(sceneObject), colliderVolumes);
        }

        /// <summary>
        /// Adds a climbable to an existing SceneObject.
        /// </summary>
        /// <param name="sceneObject">The scene object to add the climbable to.</param>
        /// <param name="colliderVolumes">A list of collider volumes to grab for climbing. A list of Vector3,Vector3 tuples.</param>
        public void AddClimbableToSceneObject(SceneObject sceneObject, List<Tuple<Vector3, Vector3>> colliderVolumes)
        {
            if (sceneObject == null)
            {
                Debug.LogWarning("[SceneObjectManager->AddClimbableToSceneObject] Invalide scene object.");
                return;
            }

            if (colliderVolumes == null)
            {
                Debug.LogWarning("[SceneObjectManager->AddClimbableToSceneObject] Invalid collider volumes.");
                return;
            }

            GameObject climbableObject = new GameObject("Climbable");
            climbableObject.tag = CLIMBABLETAG;
            climbableObject.transform.parent = sceneObject.transform;
            climbableObject.transform.localPosition = Vector3.zero;
            climbableObject.transform.localRotation = Quaternion.identity;
            climbableObject.transform.localScale = Vector3.one;

            foreach (Tuple<Vector3, Vector3> colliderVolume in colliderVolumes)
            {
                if (colliderVolume.Item1 == null || colliderVolume.Item2 == null)
                {
                    Debug.LogWarning("[SceneObjectManager->AddClimbableToSceneObject] Invalid collider volume, skipping.");
                    continue;
                }

                BoxCollider collider = climbableObject.gameObject.AddComponent<BoxCollider>();
                collider.center = colliderVolume.Item1;
                collider.size = colliderVolume.Item2;
            }
        }

        /// <summary>
        /// Removes climbables from the given SceneObject.
        /// </summary>
        /// <param name="sceneObject">UUID of scene object to remove climbables from.</param>
        public void RemoveClimbablesFromSceneObject(Guid sceneObject)
        {
            RemoveClimbablesFromSceneObject(GetSceneObjectByUUID(sceneObject));
        }

        /// <summary>
        /// Removes climbables from the given SceneObject.
        /// </summary>
        /// <param name="sceneObject">Scene object to remove climbables from.</param>
        public void RemoveClimbablesFromSceneObject(SceneObject sceneObject)
        {
            if (sceneObject == null)
            {
                Debug.LogWarning("[SceneObjectManager->RemoveClimbablesFromSceneObject] Invalid scene object.");
                return;
            }

            foreach (Transform child in sceneObject.transform)
            {
                if (child.tag == CLIMBABLETAG)
                {
                    Destroy(child);
                }
            }
        }

        /// <summary>
        /// Toggles motion constraints.
        /// </summary>
        /// <param name="on">Whether to turn motion constraints on or off.</param>
        public void ToggleMotionConstraints(bool on)
        {
            Debug.Log("yo " + on);
            grabBehavior = on ? Interactable.Interactable.GrabBehavior.Constrained :
                Interactable.Interactable.GrabBehavior.Attach;
        }

        private void AddRaycastMeshColliders(GameObject obj)
        {
            foreach (MeshFilter mesh in obj.GetComponentsInChildren<MeshFilter>())
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
    }
}