// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.SceneObject
{
    /// <remarks>
    /// History:
    /// 4 April 2021: Created
    /// </remarks>
    /// <summary>
    /// Manager for SceneObjects in MRET.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class SceneObjectManager : MonoBehaviour
    {
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
        /// Maintained InteractableDrawings.
        /// </summary>
        public InteractableDrawing[] interactableDrawings
        {
            get
            {
                List<InteractableDrawing> rtn = new List<InteractableDrawing>();
                foreach(KeyValuePair<Guid, SceneObject> kvp in sceneObjects)
                {
                    if (kvp.Value is InteractableDrawing)
                    {
                        rtn.Add((InteractableDrawing) kvp.Value);
                    }
                }
                return rtn.ToArray();
            }
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

            newSceneObject.transform.SetParent(parent.transform);
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
                sceneObjects.Remove(sceneObjectToDestroy);
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

            newInteractablePart.transform.SetParent(parent.transform);
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

            sceneObjects.Add(newInteractablePart.uuid, newInteractablePart);

            return newInteractablePart;
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

            newInteractableNote.transform.SetParent(parent.transform);
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
    }
}