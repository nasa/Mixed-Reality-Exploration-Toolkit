// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects
{
    /// <remarks>
    /// History:
    /// 18 November 2020: Created
    /// 6 January 2022: Added touch hold (DZB)
    /// 26 January 2022: Made trigger methods overridable (DZB)
    /// 11 October 2022: Renamed to SceneObjectDeprecated in preparation for the new schema hierarchy
    /// </remarks>
    /// 
    /// <summary>
    /// Base scene object
    /// 
    /// Author: Dylan Z. Baker
    /// Author: Jeffrey Hosler (refactored)
    /// </summary>
    [System.Obsolete("Refer to " + nameof(GOV.NASA.GSFC.XR.MRET.SceneObjects.SceneObject))]
    public class SceneObjectDeprecated : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SceneObjectDeprecated);

        /// <summary>
        /// Behavior to use on touch.
        /// </summary>
        public enum TouchBehavior { Highlight, Hold, Custom }

        /// <summary>
        /// Behavior to use on grab.
        /// </summary>
        public enum GrabBehavior { Attach, Constrained, Custom }

        [Tooltip("Behavior to use on touch.")]
        public TouchBehavior touchBehavior = TouchBehavior.Highlight;

        [Tooltip("Behavior to use on grab.")]
        public GrabBehavior grabBehavior = GrabBehavior.Attach;

        /// <summary>
        /// Threshold when in touch hold behavior after which a
        /// touch hold is considered to have occurred.
        /// </summary>
        public float touchHoldThreshold = 1f;

        protected bool lockStatus = false;
        /// <summary>
        /// Whether or not the interactable is locked.
        /// </summary>
        public bool locked
        {
            set
            {
                lockStatus = value;
            }
            get
            {
                return lockStatus;
            }
        }

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

        /// <summary>
        /// Finds the InteractableSceneObject that the provided GameObject is part of.
        /// </summary>
        /// <param name="go">GameObject to perform the search for.</param>
        /// <returns>If InteractableSceneObject found, the InteractableSceneObject that the GameObject is part of. Otherwise, null.</returns>
        public static SceneObjectDeprecated GetInteractableForGameObject(GameObject go)
        {
            // Start with the current object and search each of the parents. When a InteractableSceneObject
            // is found on the parent, return that InteractableSceneObject. If no parent is found, return null.
            Transform currentTransform = go.transform;
            while (currentTransform != null)
            {
                SceneObjectDeprecated foundInteractable = currentTransform.GetComponent<SceneObjectDeprecated>();
                if (foundInteractable != null)
                {
                    return foundInteractable;
                }
                currentTransform = currentTransform.parent;
            }
            return null;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            InputHand inputHand = other.GetComponent<InputHand>();
            if (inputHand != null)
            {
                BeginTouch(inputHand);
            }
        }

        protected virtual void OnTriggerExit(Collider other)
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
                case TouchBehavior.Hold:
                    lastTime = DateTime.UtcNow;
                    holdCount = 0;
                    goto case TouchBehavior.Highlight;

                case TouchBehavior.Highlight:
                    // Alter this object's materials.
                    if (savedMaterialInfo != null)
                    {
                        RestoreObjectMaterials();
                    }
                    SaveObjectMaterials(false);
                    ReplaceObjectMaterials(MRET.HighlightMaterial, false);

                    // Save the touching hand.
                    touchingHand = hand;
                    break;

                case TouchBehavior.Custom:
                default:
                    LogWarning("Not implemented for SceneObjectDeprecated.", nameof(BeginTouch));
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
                case TouchBehavior.Hold:
                    holdCount = -1;
                    touchHolding = false;
                    goto case TouchBehavior.Highlight;

                case TouchBehavior.Highlight:
                    // Restore this object's materials.
                    RestoreObjectMaterials();
                    break;

                case TouchBehavior.Custom:
                default:
                    LogWarning("Not implemented for SceneObjectDeprecated.", nameof(EndTouch));
                    break;
            }
        }

        /// <summary>
        /// Begin to touch hold the provided hand.
        /// </summary>
        protected virtual void BeginTouchHold(InputHand hand)
        {

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
                    LogWarning("Not implemented for SceneObjectDeprecated.", nameof(BeginGrab));
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
                    LogWarning("Not implemented for SceneObjectDeprecated.", nameof(EndGrab));
                    break;
            }
        }

        public virtual void Use(InputHand hand)
        {
            LogWarning("Not implemented for SceneObjectDeprecated.", nameof(Use));
        }

        /// <summary>
        /// Unuse performed on the provided hand.
        /// </summary>
        /// <param name="hand">Hand that performed the unuse.</param>
        public virtual void Unuse(InputHand hand)
        {

        }

        /// <summary>
        /// Gets all the GameObjects that are part of this SceneObjectDeprecated.
        /// </summary>
        /// <param name="includeChildInteractables">Whether or not to include GameObjects of children.</param>
        /// <returns>Array of GameObjects that are part of this SceneObjectDeprecated.</returns>
        protected virtual GameObject[] GetAllGameObjects(bool includeChildInteractables)
        {
            List<GameObject> rtnObjects = new List<GameObject>();

            // Go through each child.
            foreach (Transform t in transform)
            {
                // If child InteractableSceneObject are included, automatically add.
                if (includeChildInteractables)
                {
                    rtnObjects.Add(t.gameObject);
                }
                // If not, check if it belongs to this InteractableSceneObject.
                else if (GetInteractableForGameObject(t.gameObject) == this)
                {
                    rtnObjects.Add(t.gameObject);
                }
            }
            return rtnObjects.ToArray();
        }

        /// <summary>
        /// Gets all the MeshRenderers that are part of this SceneObjectDeprecated.
        /// </summary>
        /// <param name="includeChildSceneObjects">Whether or not to include GameObjects of children.</param>
        /// <returns>Array of MeshRenderers that are part of this SceneObjectDeprecated.</returns>
        protected virtual MeshRenderer[] GetAllMeshRenderers(bool includeChildInteractables)
        {
            List<MeshRenderer> rtnObjects = new List<MeshRenderer>();

            // Go through each child.
            foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>())
            {
                // If child InteractableSceneObject are included, automatically add.
                if (includeChildInteractables)
                {
                    rtnObjects.Add(m);
                }
                // If not, check if it belongs to this InteractableSceneObject.
                else if (GetInteractableForGameObject(m.gameObject) == this)
                {
                    rtnObjects.Add(m);
                }
            }
            return rtnObjects.ToArray();
        }

        /// <summary>
        /// Count of how long touch hold has been active.
        /// </summary>
        protected float holdCount = -1;

        /// <summary>
        /// Last time hold was counted.
        /// </summary>
        protected DateTime lastTime;

        /// <summary>
        /// Whether or not touch holding is occurring.
        /// </summary>
        protected bool touchHolding = false;

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETUpdate();

            // FIXME: Should this be initialized like this, or do we want a prefab
            // template that is instantiated by a manager that contains the correct frequency?
            updateRate = UpdateFrequency.HzMaximum;
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            if (touchBehavior == TouchBehavior.Hold)
            {
                if (holdCount >= 0)
                {
                    DateTime time = DateTime.UtcNow;
                    TimeSpan span = time - lastTime;
                    holdCount += (float)span.TotalSeconds;
                    if (holdCount > touchHoldThreshold && !touchHolding)
                    {
                        touchHolding = true;
                        BeginTouchHold(touchingHand);
                    }
                    lastTime = time;
                }
            }
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

        // This is the new method that should be used for all checks of grabbing
        // Works for both Regular grabbing, and for Constrained grabbing
        /// <summary>
        /// Checks whether this object is being grabbed by the given InputHand.
        /// </summary>
        /// <param name="hand"> The hand to check for grabbing.</param>
        /// <returns>True if is being grabbed, false if not.</returns>
        public virtual bool IsGrabbedBy(InputHand hand)
        {
            if (hand == null || hand.transform == null) return false;
            if (transform.IsChildOf(hand.transform))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attach to the given object.
        /// </summary>
        /// <param name="attachToTransform">Object to attach to.</param>
        protected virtual void AttachTo(Transform attachToTransform)
        {
            originalParent = transform.parent;
            transform.SetParent(attachToTransform);
        }

        /// <summary>
        /// Attach to original parent.
        /// </summary>
        protected virtual void Detach()
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
                if (rendMatInfo != null && rendMatInfo.Item1 != null && rendMatInfo.Item2 != null)
                {
                    rendMatInfo.Item1.materials = rendMatInfo.Item2;
                }
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
        #endregion Material Highlighting
    }
}