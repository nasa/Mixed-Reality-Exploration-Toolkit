// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET
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
    public class SceneObjectManager : MRETManager<SceneObjectManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SceneObjectManager);

        /// <summary>
        /// Tag for climbables.
        /// </summary>
        public static readonly string CLIMBABLETAG = "CLIMBABLE";

        /// <summary>
        /// Container for all scene objects.
        /// </summary>
        public GameObject sceneObjectRoot;

        private IInteractable.GrabBehaviors _grabBehavior = IInteractable.GrabBehaviors.Attach;
        public IInteractable.GrabBehaviors GrabBehavior
        {
            get
            {
                return _grabBehavior;
            }
            set
            {
                foreach (IInteractable interactable in MRET.UuidRegistry.Interactables)
                {
                    Log("setting " + value, nameof(GrabBehavior));
                    interactable.GrabBehavior = value;
                }
                _grabBehavior = value;
            }
        }

        /// <seealso cref="MRETManager{M}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();

            MRET.DataManager.SaveValue(InteractableSceneObjectDeprecated.motionConstraintsKey, false);
        }

        /// <summary>
        /// Helper function to obtain the parent of type <code>T</code> in
        /// the supplied game object.
        /// </summary>
        /// <typeparam name="T">Defines the desired parent type derived from
        ///     <code>ISceneObject</code></typeparam>
        /// <param name="go">The <code>GameObject</code> to begin the search</param>
        /// <returns>The <code>T</code> parent or null if not found</returns>
        public T GetParent<T>(GameObject go)
            where T : ISceneObject
        {
            if (go != null)
            {
                Transform p = go.transform.parent;
                while (p != null)
                {
                    T so = p.GetComponent<T>();
                    if (so != null)
                    {
                        return so;
                    }
                    p = p.parent;
                }
            }

            return default;
        }

        /// <summary>
        /// Helper function to obtain the children of type <code>T</code> in
        /// the supplied game object.
        /// </summary>
        /// <typeparam name="T">Defines the desired child type derived from
        ///     <code>ISceneObject</code></typeparam>
        /// <param name="go">The <code>GameObject</code> to begin the search</param>
        /// <returns>The array of <code>T</code> children. Empty array if none found.</returns>
        public T[] GetChildren<T>(GameObject go)
            where T : ISceneObject
        {
            List<T> childList = new List<T>();
            if (go != null)
            {
                foreach (Transform t in transform)
                {
                    T child = t.GetComponent<T>();
                    if (child != null)
                    {
                        childList.Add(child);
                    }
                }
            }
            return childList.ToArray();
        }

        /// <summary>
        /// Adds a climbable to an existing interactable.
        /// </summary>
        /// <param name="interactable">The <code>IInteractable</code> to add the climbable to.</param>
        /// <param name="colliderVolumes">A list of collider volumes to grab for climbing. A list of Vector3,Vector3 tuples.</param>
        public void AddClimbableToSceneObject(IInteractable interactable, List<Tuple<Vector3, Vector3>> colliderVolumes)
        {
            if (interactable == null)
            {
                LogWarning("Invalid interactable scene object.", nameof(AddClimbableToSceneObject));
                return;
            }

            if (colliderVolumes == null)
            {
                LogWarning("Invalid collider volumes.", nameof(AddClimbableToSceneObject));
                return;
            }

            GameObject climbableObject = new GameObject("Climbable");
            climbableObject.tag = CLIMBABLETAG;
            climbableObject.transform.parent = interactable.transform;
            climbableObject.transform.localPosition = Vector3.zero;
            climbableObject.transform.localRotation = Quaternion.identity;
            climbableObject.transform.localScale = Vector3.one;

            foreach (Tuple<Vector3, Vector3> colliderVolume in colliderVolumes)
            {
                if (colliderVolume.Item1 == null || colliderVolume.Item2 == null)
                {
                    LogWarning("Invalid collider volume, skipping.", nameof(AddClimbableToSceneObject));
                    continue;
                }

                BoxCollider collider = climbableObject.gameObject.AddComponent<BoxCollider>();
                collider.center = colliderVolume.Item1;
                collider.size = colliderVolume.Item2;
            }
        }

        /// <summary>
        /// Removes climbables from the given interactable.
        /// </summary>
        /// <param name="interactable">The <code>IInteractable</code> to remove climbables from.</param>
        public void RemoveClimbablesFromSceneObject(IInteractable interactable)
        {
            if (interactable == null)
            {
                LogWarning("Invalid interactable scene object.", nameof(RemoveClimbablesFromSceneObject));
                return;
            }

            foreach (Transform child in interactable.transform)
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
            GrabBehavior = on ? IInteractable.GrabBehaviors.Constrained :
                IInteractable.GrabBehaviors.Attach;
        }

    }
}