// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing.Legacy;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Note;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects
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
    [System.Obsolete("Refer to " + nameof(GOV.NASA.GSFC.XR.MRET.SceneObjectManager))]
    public class SceneObjectManagerDeprecated : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SceneObjectManagerDeprecated);

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

        private SceneObjectDeprecated.GrabBehavior _grabBehavior = SceneObjectDeprecated.GrabBehavior.Attach;
        public SceneObjectDeprecated.GrabBehavior grabBehavior
        {
            get
            {
                return _grabBehavior;
            }
            set
            {
                foreach (SceneObjectDeprecated sceneObject in sceneObjects.Values)
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
        private Dictionary<Guid, InteractableSceneObjectDeprecated> sceneObjects = new Dictionary<Guid, InteractableSceneObjectDeprecated>();

        /// <summary>
        /// Maintained SceneObjects.
        /// </summary>
        public InteractableSceneObjectDeprecated[] SceneObjects
        {
            get
            {
                InteractableSceneObjectDeprecated[] rtn = new InteractableSceneObjectDeprecated[sceneObjects.Count];
                sceneObjects.Values.CopyTo(rtn, 0);
                return rtn;
            }
        }

        /// <summary>
        /// Maintained InteractableParts.
        /// </summary>
        public InteractablePartDeprecated[] InteractableParts
        {
            get
            {
                List<InteractablePartDeprecated> rtn = new List<InteractablePartDeprecated>();
                foreach (KeyValuePair<Guid, InteractableSceneObjectDeprecated> kvp in sceneObjects)
                {
                    if (kvp.Value is InteractablePartDeprecated)
                    {
                        rtn.Add((InteractablePartDeprecated) kvp.Value);
                    }
                }
                return rtn.ToArray();
            }
        }

        /// <summary>
        /// Maintained InteractableNotes.
        /// </summary>
        public InteractableNoteDeprecated[] interactableNotes
        {
            get
            {
                List<InteractableNoteDeprecated> rtn = new List<InteractableNoteDeprecated>();
                foreach (KeyValuePair<Guid, InteractableSceneObjectDeprecated> kvp in sceneObjects)
                {
                    if (kvp.Value is InteractableNoteDeprecated)
                    {
                        rtn.Add((InteractableNoteDeprecated) kvp.Value);
                    }
                }
                return rtn.ToArray();
            }
        }

        /// <summary>
        /// Maintained LineDrawings.
        /// </summary>
        public LineDrawingDeprecated[] lineDrawings
        {
            get
            {
                List<LineDrawingDeprecated> rtn = new List<LineDrawingDeprecated>();
                foreach(KeyValuePair<Guid, InteractableSceneObjectDeprecated> kvp in sceneObjects)
                {
                    if (kvp.Value is LineDrawingDeprecated)
                    {
                        rtn.Add((LineDrawingDeprecated) kvp.Value);
                    }
                }
                return rtn.ToArray();
            }
        }

        public void Initialize()
        {
            MRET.DataManager.SaveValue(InteractableSceneObjectDeprecated.motionConstraintsKey, false);
        }

        /// <summary>
        /// Get a SceneObjectDeprecated by its UUID.
        /// </summary>
        /// <param name="uuidToFind">UUID of the SceneObjectDeprecated in question.</param>
        /// <returns>The corresponding SceneObjectDeprecated or null.</returns>
        public InteractableSceneObjectDeprecated GetSceneObjectByUUID(Guid uuidToFind)
        {
            InteractableSceneObjectDeprecated sceneObject;
            sceneObjects.TryGetValue(uuidToFind, out sceneObject);
            return sceneObject;
        }

        /// <summary>
        /// Temporary for registering scene objects.
        /// </summary>
        /// <param name="objectToRegister">Object to register.</param>
        /// <param name="id">ID to assign.</param>
        public void RegisterSceneObject(InteractableSceneObjectDeprecated objectToRegister, Guid id)
        {
            sceneObjects.Add(id, objectToRegister);
        }

        /// <summary>
        /// Create a SceneObjectDeprecated.
        /// </summary>
        /// <param name="sceneObjectName">Name of the SceneObjectDeprecated.</param>
        /// <param name="parent">Parent for the SceneObjectDeprecated.</param>
        /// <param name="localPosition">Local position of the SceneObjectDeprecated.</param>
        /// <param name="localRotation">Local rotation of the SceneObjectDeprecated.</param>
        /// <param name="localScale">Local scale of the SceneObjectDeprecated.</param>
        /// <param name="uuid">UUID of the SceneObjectDeprecated.</param>
        /// <returns>The created SceneObjectDeprecated.</returns>
        public InteractableSceneObjectDeprecated CreateSceneObject(string sceneObjectName, InteractableSceneObjectDeprecated parent,
            Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Guid? uuid = null)
        {
            InteractableSceneObjectDeprecated newSceneObject = InteractableSceneObjectDeprecated.Create();
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
        /// Create a SceneObjectDeprecated.
        /// </summary>
        /// <param name="sceneObjectName">Name of the SceneObjectDeprecated.</param>
        /// <param name="parent">UUID for the parent for the SceneObjectDeprecated.</param>
        /// <param name="localPosition">Local position of the SceneObjectDeprecated.</param>
        /// <param name="localRotation">Local rotation of the SceneObjectDeprecated.</param>
        /// <param name="localScale">Local scale of the SceneObjectDeprecated.</param>
        /// <param name="uuid">UUID of the SceneObjectDeprecated.</param>
        /// <returns>The created SceneObjectDeprecated.</returns>
        public InteractableSceneObjectDeprecated CreateSceneObject(string sceneObjectName, Guid parentUUID,
            Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Guid? uuid = null)
        {
            InteractableSceneObjectDeprecated parent = GetSceneObjectByUUID(parentUUID);
            if (parent == null)
            {
                Debug.LogWarning("[SceneObjectManager->CreateSceneObject] Parent UUID not found. Setting parent to null.");
            }

            return CreateSceneObject(sceneObjectName, parent, localPosition, localRotation, localScale, uuid);
        }

        /// <summary>
        /// Destroy a SceneObjectDeprecated.
        /// </summary>
        /// <param name="sceneObjectToDestroy">UUID of the SceneObjectDeprecated to destroy.</param>
        public void DestroySceneObject(Guid sceneObjectToDestroy)
        {
            if (sceneObjects.ContainsKey(sceneObjectToDestroy))
            {
                InteractableSceneObjectDeprecated soToDestroy = sceneObjects[sceneObjectToDestroy];
                sceneObjects.Remove(sceneObjectToDestroy);
                Destroy(soToDestroy.gameObject);
            }
            else
            {
                Debug.LogWarning("[SceneObjectManager->DestroySceneObject] SceneObjectDeprecated not found.");
            }
        }

        /// <summary>
        /// Create an InteractablePartDeprecated.
        /// </summary>
        /// <param name="partName">Name of the InteractablePartDeprecated.</param>
        /// <param name="parent">Parent for the InteractablePartDeprecated.</param>
        /// <param name="localPosition">Local position of the InteractablePartDeprecated.</param>
        /// <param name="localRotation">Local rotation of the InteractablePartDeprecated.</param>
        /// <param name="localScale">Local scale of the InteractablePartDeprecated.</param>
        /// <param name="uuid">UUID of the InteractablePartDeprecated.</param>
        /// <returns>The created InteractablePartDeprecated.</returns>
        public InteractablePartDeprecated CreateInteractablePart(string partName, InteractableSceneObjectDeprecated parent,
            Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Guid? uuid = null)
        {
            InteractablePartDeprecated newInteractablePart = InteractablePartDeprecated.Create();
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

        public InteractablePartDeprecated CreateInteractablePartWithScale(string partName, string AssetBundle, InteractableSceneObjectDeprecated parent,
            Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Guid? uuid = null)
        {
            InteractablePartDeprecated newInteractablePart = InteractablePartDeprecated.Create(partName, AssetBundle, parent, Vector3.one, uuid);
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

        public InteractablePartDeprecated CreateInteractablePartWithSize(string partName, string AssetBundle, InteractableSceneObjectDeprecated parent,
            Vector3 localPosition, Quaternion localRotation, Vector3 size, Guid? uuid = null)
        {
            InteractablePartDeprecated newInteractablePart = InteractablePartDeprecated.Create(partName, AssetBundle, parent, size, uuid);
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
        /// Creates a LineDrawingDeprecated.
        /// </summary>
        /// <param name="drawingName">Name of the LineDrawingDeprecated.</param>
        /// <param name="parent">Parent for the LineDrawingDeprecated.</param>
        /// <param name="localPosition">Local position of the LineDrawingDeprecated.</param>
        /// <param name="localRotation">Local rotation of the LineDrawingDeprecated.</param>
        /// <param name="localScale">Local scale of the LineDrawingDeprecated.</param>
        /// <param name="type">Type of the LineDrawingDeprecated.</param>
        /// <param name="width">Width or diameter of the LineDrawingDeprecated.</param>
        /// <param name="color">Color of the LineDrawingDeprecated.</param>
        /// <param name="positions">Positions of points in the LineDrawingDeprecated.</param>
        /// <param name="uuid">UUID of the LineDrawingDeprecated.</param>
        /// <returns>The created LineDrawingDeprecated.</returns>
        public LineDrawingDeprecated CreateLineDrawing(string drawingName,
            SceneObjectDeprecated parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale,
            LineDrawingManagerDeprecated.DrawingType type, float width, Color32 color,
            Vector3[] positions, Guid? uuid = null)
        {
            LineDrawingDeprecated newLineDrawing = null;
            switch (type)
            {
                case LineDrawingManagerDeprecated.DrawingType.Basic:
                    newLineDrawing = LineDrawingDeprecated.Create(
                        drawingName, width, color, positions, uuid);
                    break;

                case LineDrawingManagerDeprecated.DrawingType.Volumetric:
                    newLineDrawing = VolumetricDrawingDeprecated.Create(drawingName,
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
        /// Create an InteractablePartDeprecated.
        /// </summary>
        /// <param name="partName">Name of the InteractablePartDeprecated.</param>
        /// <param name="parent">UUID of the parent for the InteractablePartDeprecated.</param>
        /// <param name="localPosition">Local position of the InteractablePartDeprecated.</param>
        /// <param name="localRotation">Local rotation of the InteractablePartDeprecated.</param>
        /// <param name="localScale">Local scale of the InteractablePartDeprecated.</param>
        /// <param name="uuid">UUID of the InteractablePartDeprecated.</param>
        /// <returns>The created InteractablePartDeprecated.</returns>
        public InteractablePartDeprecated CreateInteractablePart(string partName, Guid parentUUID,
            Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Guid? uuid = null)
        {
            InteractableSceneObjectDeprecated parent = GetSceneObjectByUUID(parentUUID);
            if (parent == null)
            {
                Debug.LogWarning("[SceneObjectManager->CreateInteractablePart] Parent UUID not found. Setting parent to null.");
            }

            return CreateInteractablePart(partName, parent, localPosition, localRotation, localScale, uuid);
        }

        /// <summary>
        /// Create an InteractableNoteDeprecated.
        /// </summary>
        /// <param name="noteName">Name of the InteractableNoteDeprecated.</param>
        /// <param name="parent">Parent for the InteractableNoteDeprecated.</param>
        /// <param name="localPosition">Local position of the InteractableNoteDeprecated.</param>
        /// <param name="localRotation">Local rotation of the InteractableNoteDeprecated.</param>
        /// <param name="localScale">Local scale of the InteractableNoteDeprecated.</param>
        /// <param name="uuid">UUID of the InteractableNoteDeprecated.</param>
        /// <returns>The created InteractableNoteDeprecated.</returns>
        public InteractableNoteDeprecated CreateInteractableNote(string noteName, SceneObjectDeprecated parent,
            Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Guid? uuid = null)
        {
            InteractableNoteDeprecated newInteractableNote = InteractableNoteDeprecated.Create();
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
        /// Create an InteractableNoteDeprecated.
        /// </summary>
        /// <param name="noteName">Name of the InteractableNoteDeprecated.</param>
        /// <param name="parent">UUID of the parent for the InteractableNoteDeprecated.</param>
        /// <param name="localPosition">Local position of the InteractableNoteDeprecated.</param>
        /// <param name="localRotation">Local rotation of the InteractableNoteDeprecated.</param>
        /// <param name="localScale">Local scale of the InteractableNoteDeprecated.</param>
        /// <param name="uuid">UUID of the InteractableNoteDeprecated.</param>
        /// <returns>The created InteractableNoteDeprecated.</returns>
        public InteractableNoteDeprecated CreateInteractableNote(string noteName, Guid parentUUID,
            Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Guid? uuid = null)
        {
            SceneObjectDeprecated parent = GetSceneObjectByUUID(parentUUID);
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
        public InteractableDrawingDeprecated CreateInteractableDrawing(string drawingName, InteractableSceneObjectDeprecated parent,
            Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Guid? uuid = null)
        {
            InteractableDrawingDeprecated newInteractableDrawing = InteractableDrawingDeprecated.Create();
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
        public InteractableDrawingDeprecated CreateInteractableDrawing(string drawingName, Guid parentUUID,
            Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Guid? uuid = null)
        {
            InteractableSceneObjectDeprecated parent = GetSceneObjectByUUID(parentUUID);
            if (parent == null)
            {
                Debug.LogWarning("[SceneObjectManager->CreateInteractableDrawing] Parent UUID not found. Setting parent to null.");
            }

            return CreateInteractableDrawing(drawingName, parent, localPosition, localRotation, localScale, uuid);
        }

        /// <summary>
        /// Adds a climbable to an existing SceneObjectDeprecated.
        /// </summary>
        /// <param name="sceneObject">UUID of the scene object to add the climbable to.</param>
        /// <param name="colliderVolumes">A list of collider volumes to grab for climbing. A list of Vector3,Vector3 tuples.</param>
        public void AddClimbableToSceneObject(Guid sceneObject, List<Tuple<Vector3, Vector3>> colliderVolumes)
        {
            AddClimbableToSceneObject(GetSceneObjectByUUID(sceneObject), colliderVolumes);
        }

        /// <summary>
        /// Adds a climbable to an existing SceneObjectDeprecated.
        /// </summary>
        /// <param name="sceneObject">The scene object to add the climbable to.</param>
        /// <param name="colliderVolumes">A list of collider volumes to grab for climbing. A list of Vector3,Vector3 tuples.</param>
        public void AddClimbableToSceneObject(SceneObjectDeprecated sceneObject, List<Tuple<Vector3, Vector3>> colliderVolumes)
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
        /// Removes climbables from the given SceneObjectDeprecated.
        /// </summary>
        /// <param name="sceneObject">UUID of scene object to remove climbables from.</param>
        public void RemoveClimbablesFromSceneObject(Guid sceneObject)
        {
            RemoveClimbablesFromSceneObject(GetSceneObjectByUUID(sceneObject));
        }

        /// <summary>
        /// Removes climbables from the given SceneObjectDeprecated.
        /// </summary>
        /// <param name="sceneObject">Scene object to remove climbables from.</param>
        public void RemoveClimbablesFromSceneObject(SceneObjectDeprecated sceneObject)
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
            grabBehavior = on ? InteractableSceneObjectDeprecated.GrabBehavior.Constrained :
                InteractableSceneObjectDeprecated.GrabBehavior.Attach;
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