// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.Interactable
{
    /// <remarks>
    /// History:
    /// 18 November 2020: Created
    /// </remarks>
    /// <summary>
    /// Base class for all MRET interactable objects.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class Interactable : MonoBehaviour
    {
        /// <summary>
        /// Behavior to use on touch.
        /// </summary>
        public enum TouchBehavior { Highlight, Custom }

        /// <summary>
        /// Behavior to use on grab.
        /// </summary>
        public enum GrabBehavior { Attach, Custom }

        [Tooltip("Behavior to use on touch.")]
        public TouchBehavior touchBehavior = TouchBehavior.Highlight;

        [Tooltip("Behavior to use on grab.")]
        public GrabBehavior grabBehavior = GrabBehavior.Attach;

        [Tooltip("Material to use for highlighting (only if touchBehavior is set to highlight).")]
        public Material highlightMaterial;

        protected bool grabEnabled = true;
        /// <summary>
        /// Whether or not the interactable is grabbable.
        /// </summary>
        public bool grabbable
        {
            set
            {
                // TODO.
                int x = 1;
                grabEnabled = value;
            }
            get
            {
                return grabEnabled;
            }
        }

        protected bool useEnabled;
        /// <summary>
        /// Whether or not the interactable is useable.
        /// </summary>
        public bool useable
        {
            set
            {
                // TODO.
                int x = 1;
                useEnabled = value;
            }
            get
            {
                return useEnabled;
            }
        }

        private void Start()
        {
            highlightMaterial = MRET.HighlightMaterial;
        }

        /// <summary>
        /// Finds the SceneObject that the provided GameObject is part of.
        /// </summary>
        /// <param name="go">GameObject to perform the search for.</param>
        /// <returns>If SceneObject found, the SceneObject that the GameObject is part of. Otherwise, null.</returns>
        public static Interactable GetInteractableForGameObject(GameObject go)
        {
            // Start with the current object and search each of the parents. When a SceneObject
            // is found on the parent, return that SceneObject. If no parent is found, return null.
            Transform currentTransform = go.transform;
            while (currentTransform != null)
            {
                Interactable foundInteractable = currentTransform.GetComponent<Interactable>();
                if (foundInteractable != null)
                {
                    return foundInteractable;
                }
                currentTransform = currentTransform.parent;
            }
            return null;
        }

        private void OnTriggerEnter(Collider other)
        {
            InputHand inputHand = other.GetComponent<InputHand>();
            if (inputHand != null)
            {
                BeginTouch(inputHand);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            InputHand inputHand = other.GetComponent<InputHand>();
            if (inputHand != null)
            {
                if (inputHand == touchingHand)
                {
                    EndTouch();
                }
            }
        }

        /// <summary>
        /// Begin to touch the provided hand. Behavior will
        /// be dictated by the configured touchBehavior.
        /// </summary>
        /// <param name="hand">Hand to touch.</param>
        protected virtual void BeginTouch(InputHand hand)
        {
            switch (touchBehavior)
            {
                case TouchBehavior.Highlight:
                    // Alter this object's materials.
                    if (savedMaterialInfo != null)
                    {
                        RestoreObjectMaterials();
                    }
                    SaveObjectMaterials(false);
                    ReplaceObjectMaterials(highlightMaterial, false);

                    // Save the touching hand.
                    touchingHand = hand;
                    break;

                case TouchBehavior.Custom:
                default:
                    Debug.LogWarning("BeginTouch() not implemented for SceneObject.");
                    break;
            }
        }

        /// <summary>
        /// Stop touching the current touching hand. Behavior will
        /// be dictated by the configured touchBehavior.
        /// </summary>
        protected virtual void EndTouch()
        {
            switch (touchBehavior)
            {
                case TouchBehavior.Highlight:
                    // Restore this object's materials.
                    RestoreObjectMaterials();
                    break;

                case TouchBehavior.Custom:
                default:
                    Debug.LogWarning("EndTouch() not implemented for SceneObject.");
                    break;
            }
        }

        public virtual void BeginGrab(InputHand hand)
        {
            if (!grabbable)
            {
                return;
            }

            switch (grabBehavior)
            {
                case GrabBehavior.Attach:
                    AttachTo(hand.transform);
                    break;

                case GrabBehavior.Custom:
                default:
                    Debug.LogWarning("BeginGrab() not implemented for SceneObject.");
                    break;
            }
        }

        public virtual void EndGrab(InputHand hand)
        {
            if (!grabbable)
            {
                return;
            }

            switch (grabBehavior)
            {
                case GrabBehavior.Attach:
                    Detach();
                    break;

                case GrabBehavior.Custom:
                default:
                    Debug.LogWarning("EndGrab() not implemented for SceneObject.");
                    break;
            }
        }

        public virtual void Use(InputHand hand)
        {
            Debug.LogWarning("Use() not implemented for Interactable.");
        }

        /// <summary>
        /// Gets all the GameObjects that are part of this Interactable.
        /// </summary>
        /// <param name="includeChildInteractables">Whether or not to include GameObjects of children.</param>
        /// <returns>Array of GameObjects that are part of this Interactable.</returns>
        protected virtual GameObject[] GetAllGameObjects(bool includeChildInteractables)
        {
            List<GameObject> rtnObjects = new List<GameObject>();

            // Go through each child.
            foreach (Transform t in transform)
            {
                // If child SceneObject are included, automatically add.
                if (includeChildInteractables)
                {
                    rtnObjects.Add(t.gameObject);
                }
                // If not, check if it belongs to this SceneObject.
                else if (GetInteractableForGameObject(t.gameObject) == this)
                {
                    rtnObjects.Add(t.gameObject);
                }
            }
            return rtnObjects.ToArray();
        }

        /// <summary>
        /// Gets all the MeshRenderers that are part of this Interactable.
        /// </summary>
        /// <param name="includeChildSceneObjects">Whether or not to include GameObjects of children.</param>
        /// <returns>Array of MeshRenderers that are part of this Interactable.</returns>
        protected virtual MeshRenderer[] GetAllMeshRenderers(bool includeChildInteractables)
        {
            List<MeshRenderer> rtnObjects = new List<MeshRenderer>();

            // Go through each child.
            foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>())
            {
                // If child SceneObject are included, automatically add.
                if (includeChildInteractables)
                {
                    rtnObjects.Add(m);
                }
                // If not, check if it belongs to this SceneObject.
                else if (GetInteractableForGameObject(m.gameObject) == this)
                {
                    rtnObjects.Add(m);
                }
            }
            return rtnObjects.ToArray();
        }

#region Placement

        public virtual void Place()
        {

        }

#endregion

#region Attach Grabbing
        /// <summary>
        /// Parent before attaching.
        /// </summary>
        protected Transform originalParent;

        /// <summary>
        /// Attach to the given object.
        /// </summary>
        /// <param name="attachToTransform">Object to attach to.</param>
        protected void AttachTo(Transform attachToTransform)
        {
            originalParent = transform.parent;
            transform.SetParent(attachToTransform);
        }

        /// <summary>
        /// Attach to original parent.
        /// </summary>
        protected void Detach()
        {
            Vector3 origPos = transform.position;
            Quaternion origRot = transform.rotation;

            transform.SetParent(originalParent);

            transform.position = origPos;
            transform.rotation = origRot;
        }
#endregion

#region Material Highlighting
        // Information about saved materials (for material swapping).
        protected Tuple<MeshRenderer, Material[]>[] savedMaterialInfo;

        protected InputHand touchingHand;

        protected void SaveObjectMaterials(bool includeChildInteractables = false)
        {
            List<Tuple<MeshRenderer, Material[]>> rendMatList = new List<Tuple<MeshRenderer, Material[]>>();
            foreach (MeshRenderer rend in GetAllMeshRenderers(includeChildInteractables))
            {
                List<Material> mats = new List<Material>();
                foreach (Material mat in rend.materials)
                {
                    mats.Add(mat);
                }
                rendMatList.Add(new Tuple<MeshRenderer, Material[]>(rend, mats.ToArray()));
            }
            savedMaterialInfo = rendMatList.ToArray();
        }

        protected void RestoreObjectMaterials()
        {
            if (savedMaterialInfo == null)
            {
                return;
            }

            foreach (Tuple<MeshRenderer, Material[]> rendMatInfo in savedMaterialInfo)
            {
                rendMatInfo.Item1.materials = rendMatInfo.Item2;
            }

            savedMaterialInfo = null;
        }

        protected void ReplaceObjectMaterials(Material matToUse, bool includeChildInteractables = false)
        {
            foreach (MeshRenderer rend in GetAllMeshRenderers(includeChildInteractables))
            {
                int rendMatCount = rend.materials.Length;
                Material[] rendMats = new Material[rendMatCount];
                for (int i = 0; i < rendMatCount; i++)
                {
                    rendMats[i] = matToUse;
                }
                rend.materials = rendMats;
            }
        }
#endregion
    }
}