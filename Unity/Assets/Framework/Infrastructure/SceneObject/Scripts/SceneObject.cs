// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.SceneObject
{
    /// <remarks>
    /// History:
    /// 17 November 2020: Created
    /// 18 November 2020: Inherit from Interactable
    /// </remarks>
    /// <summary>
    /// Base class for all MRET Scene Objects.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class SceneObject : Interactable.Interactable
    {
        /// <summary>
        /// The parent SceneObject.
        /// </summary>
        public SceneObject parent
        {
            get
            {
                Transform p = transform.parent;
                while (p != null)
                {
                    SceneObject so = p.GetComponent<SceneObject>();
                    if (so != null)
                    {
                        return so;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// The children SceneObjects.
        /// </summary>
        public SceneObject[] children
        {
            get
            {
                List<SceneObject> childObjects = new List<SceneObject>();
                foreach (SceneObject sceneObject in GetComponentsInChildren<SceneObject>())
                {
                    if (sceneObject != this)
                    {
                        childObjects.Add(sceneObject);
                    }
                }
                return childObjects.ToArray();
            }
        }

        public Guid uuid;

        protected bool selected = false;

        private void Start()
        {
            highlightMaterial = MRET.HighlightMaterial;
        }

        public static SceneObject Create()
        {
            GameObject sceneObjectGameObject = new GameObject();
            return sceneObjectGameObject.AddComponent<SceneObject>();
        }

        /// <summary>
        /// Finds the SceneObject that the provided GameObject is part of.
        /// </summary>
        /// <param name="go">GameObject to perform the search for.</param>
        /// <returns>If SceneObject found, the SceneObject that the GameObject is part of. Otherwise, null.</returns>
        public static SceneObject GetSceneObjectForGameObject(GameObject go)
        {
            // Start with the current object and search each of the parents. When a SceneObject
            // is found on the parent, return that SceneObject. If no parent is found, return null.
            Transform currentTransform = go.transform;
            while (currentTransform != null)
            {
                SceneObject foundSceneObject = currentTransform.GetComponent<SceneObject>();
                if (foundSceneObject != null)
                {
                    return foundSceneObject;
                }
                currentTransform = currentTransform.parent;
            }
            return null;
        }

        public override void Use(InputHand hand)
        {
            Debug.LogWarning("Use() not implemented for SceneObject.");
        }

        /// <summary>
        /// Gets all the GameObjects that are part of this SceneObject.
        /// </summary>
        /// <param name="includeChildSceneObjects"></param>
        /// <returns>Array of GameObjects that are part of this SceneObject.</returns>
        protected override GameObject[] GetAllGameObjects(bool includeChildSceneObjects)
        {
            List<GameObject> rtnObjects = new List<GameObject>();

            // Go through each child.
            foreach (Transform t in transform)
            {
                // If child SceneObject are included, automatically add.
                if (includeChildSceneObjects)
                {
                    rtnObjects.Add(t.gameObject);
                }
                // If not, check if it belongs to this SceneObject.
                else if (GetSceneObjectForGameObject(t.gameObject) == this)
                {
                    rtnObjects.Add(t.gameObject);
                }
            }
            return rtnObjects.ToArray();
        }

        /// <summary>
        /// Gets all the MeshRenderers that are part of this SceneObject.
        /// </summary>
        /// <param name="includeChildSceneObjects"></param>
        /// <returns>Array of MeshRenderers that are part of this SceneObject.</returns>
        protected override MeshRenderer[] GetAllMeshRenderers(bool includeChildSceneObjects)
        {
            List<MeshRenderer> rtnObjects = new List<MeshRenderer>();

            // Go through each child.
            foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>())
            {
                // If child SceneObject are included, automatically add.
                if (includeChildSceneObjects)
                {
                    rtnObjects.Add(m);
                }
                // If not, check if it belongs to this SceneObject.
                else if (GetSceneObjectForGameObject(m.gameObject) == this)
                {
                    rtnObjects.Add(m);
                }
            }
            return rtnObjects.ToArray();
        }

        /// <summary>
        /// Begin to touch the provided hand. Behavior will
        /// be dictated by the configured touchBehavior.
        /// </summary>
        /// <param name="hand">Hand to touch.</param>
        protected override void BeginTouch(InputHand hand)
        {
            if (!useable)
            {
                return;
            }

            // Don't change touch if being grabbed.
            if (touchingHand != null)
            {
                if (transform.IsChildOf(touchingHand.transform))
                {
                    return;
                }
            }

            switch (touchBehavior)
            {
                case TouchBehavior.Highlight:
                    if (!selected)
                    {
                        //only highlight if unlocked
                        if (!locked)
                        {
                            // Alter this object's materials.
                            if (savedMaterialInfo != null)
                            {
                                RestoreObjectMaterials();
                            }
                            SaveObjectMaterials(true);
                            ReplaceObjectMaterials(MRET.HighlightMaterial, true);
                        }

                        // Save the touching hand.
                        touchingHand = hand;
                    }
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
        protected override void EndTouch()
        {
            if (!useable)
            {
                return;
            }

            // Don't stop touching if still being grabbed.
            if (touchingHand != null)
            {
                if (transform.IsChildOf(touchingHand.transform))
                {
                    return;
                }
            }

            switch (touchBehavior)
            {
                case TouchBehavior.Highlight:
                    if (!selected)
                    {
                        // Restore this object's materials.
                        RestoreObjectMaterials();
                    }
                    break;

                case TouchBehavior.Custom:
                default:
                    Debug.LogWarning("EndTouch() not implemented for SceneObject.");
                    break;
            }
        }
        public override void BeginGrab(InputHand hand)
        {
            if (!locked)
            {
                base.BeginGrab(hand);
            }
        }

    }
}