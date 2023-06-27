// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Part
{
    public class PartManager : MRETSerializableManager<PartManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(PartManager);

        public GameObject grabCubePrefab;

        /// <seealso cref="MRETManager{M}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();
        }

        #region MRETBehaviour
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            if (state == IntegrityState.Success)
            {
                if (grabCubePrefab == null)
                {
                    LogError("Grab Cube Prefab not assigned.", nameof(IntegrityCheck));
                    state = IntegrityState.Failure;
                }
            }
            return state;
        }
        #endregion MRETBehaviour

        #region Serializable Instantiation
        /// <seealso cref="MRETSerializableManager{M}.GetDefaultSerializableContainer{T}(T)"/>
        protected override Transform GetDefaultSerializableContainer<T>(T serialized)
        {
            return ProjectManager.PartsContainer.transform;
        }

        /// <summary>
        /// Instantiates the part from the supplied serialized part.
        /// </summary>
        /// <param name="serializedPart">The <code>PartType</code> class instance
        ///     containing the serialized representation of the part to instantiate</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated
        ///     part. If not provided, one will be created.</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated part.
        ///     If null, the default project parts container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        /// <param name="finishPartInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish the
        ///     part instantiation. Called before the onLoaded action is called. If not specified, a
        ///     default logging behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the
        ///     finishPartInstantiation method to provide additional context</param>
        protected void InstantiatePart(PartType serializedPart, GameObject go = null,
            Transform container = null, Action<InteractablePart> onLoaded = null,
            FinishSerializableInstantiationDelegate<PartType, InteractablePart> finishPartInstantiation = null,
            params object[] context)
        {
            // Check the context for hand placement
            bool placingByHand = (context.Length > 0) && (context[0] is bool value) && value;

            // Instantiate and deserialize
            InstantiateSerializable(serializedPart, go, container, onLoaded,
                finishPartInstantiation, placingByHand);
        }

        /// <summary>
        /// Instantiates the part from the supplied serialized part.
        /// </summary>
        /// <param name="serializedPart">The <code>PartType</code> class instance
        ///     containing the serialized representation of the part to instantiate</param>
        /// <param name="placingByHand">Indicates if the user will be placing the part by hand.
        ///     Default is false.</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated part.
        ///     If null, the default project parts container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        public void InstantiatePart(PartType serializedPart, bool placingByHand = false,
            Transform container = null, Action<InteractablePart> onLoaded = null)
        {
            // Instantiate and deserialize
            InstantiatePart(serializedPart, null, container, onLoaded,
                FinishPartInstantiation, placingByHand);
        }

        /// <summary>
        /// Instantiates the parts from the supplied serialized parts.
        /// </summary>
        /// <param name="serializedParts">The <code>PartsType</code> class instance
        ///     containing the serialized representation of the part group to instantiate.</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the
        ///     instantiated part group. If null, one will be created.</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated part.
        ///     If null, the default project parts container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion.</param>
        public void InstantiateParts(PartsType serializedParts, GameObject go = null,
            Transform container = null, Action<InteractablePartGroup> onLoaded = null)
        {
            // Instantiate and deserialize
            InstantiateSerializable(serializedParts, go, container, onLoaded);
        }

        /// <summary>
        /// Creates an interactable part.
        /// </summary>
        /// <param name="partName">Name of the part.</param>
        /// <param name="parent">Parent for the part.</param>
        /// <param name="localPosition">Local position of the part.</param>
        /// <param name="localRotation">Local rotation of the part.</param>
        /// <param name="localScale">Local scale of the part.</param>
        /// <returns>A <code>InteractablePart</code> instance.</returns>
        /// <see cref="InteractablePart"/>
        public InteractablePart CreatePart(string partName,
            GameObject parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            InteractablePart newPart = InteractablePart.Create(partName);

            // Additional settings if valid reference
            if (newPart != null)
            {
                // Parent
                if (parent == null)
                {
                    newPart.transform.SetParent(ProjectManager.PartsContainer.transform);
                }
                else
                {
                    newPart.transform.SetParent(parent.transform);
                }

                // Transform
                newPart.transform.localPosition = localPosition;
                newPart.transform.localRotation = localRotation;
                newPart.transform.localScale = localScale;

                // Set the grab behavior
                newPart.GrabBehavior = ProjectManager.SceneObjectManager.GrabBehavior;
                newPart.ShadeForLimitViolations = false;

                // Record the action
                var serializedPart = newPart.CreateSerializedType();
                newPart.Serialize(serializedPart);
                ProjectManager.UndoManager.AddAction(
                    new AddSceneObjectAction(serializedPart),
                    new DeleteIdentifiableObjectAction(newPart.id));
            }

            return newPart;
        }

        /// <seealso cref="MRETSerializableManager{M}.FinishSerializableInstantiation{T, I}"/>
        private void FinishPartInstantiation(PartType serializedPart, InteractablePart interactablePart,
            params object[] context)
        {
            // Grab the name if available
            string partName = !string.IsNullOrEmpty(serializedPart?.Name)
                ? "\"" + serializedPart.Name + "\" "
                : "";

            // Check for success or failure
            if (interactablePart != default)
            {
                Log("Part " + partName + "instantiation complete", nameof(FinishPartInstantiation));

                // Check if we are placing by hand. The indicator is supplied as the first argument
                // of the optional context params during the part instantiation.
                bool placingByHand = (context.Length > 0) && (context[0] is bool value) && value;
                if (placingByHand)
                {
                    // Start placing
                    interactablePart.BeginPlacing(MRET.InputRig.placingHand.gameObject);
                }
            }
            else
            {
                // Log the error
                LogError("Part " + partName + "instantiation failed", nameof(FinishPartInstantiation));
            }
        }
        #endregion Serializable Instantiation

    }
}