// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.Utilities.Transforms;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Project;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Part
{
    /// <remarks>
    /// History:
    /// 20 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// InteractablePartGroup
	///
	/// An interactable part group in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
    /// 
    public class InteractablePartGroup : Group<PartsType, InteractablePartGroup, InteractablePartGroup, PartType, InteractablePart, InteractablePart>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(InteractablePartGroup);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private PartsType serializedParts;

        /// <summary>
        /// The enclosure for this group. NULL if not defined
        /// </summary>
        public InteractableEnclosure Enclosure { get => _enclosure; }
        private InteractableEnclosure _enclosure = null;

        public AssemblyGrabber AssemblyGrabber { get => _assemblyGrabber; }
        private AssemblyGrabber _assemblyGrabber = null;

        /// <summary>
        /// Indicates whether or not the assembly is grabbable.
        /// </summary>
        public bool AssemblyGrabbable { get; protected set; }

        /// <summary>
        /// Indicates whether or not the assembly is usable.
        /// </summary>
        public bool AssemblyUsable { get; protected set; }

        protected GameObject grabCube;

        #region MRETUpdateBehaviour
        protected override void MRETAwake()
        {
            base.MRETAwake();

            // Initialize the defaults
            AssemblyGrabbable = InteractablePartGroupDefaults.ASSEMBLY_GRABBABLE;
            AssemblyUsable = InteractablePartGroupDefaults.ASSEMBLY_USABLE;

            // Make sure we have an assemby grabber
            _assemblyGrabber = gameObject.GetComponent<AssemblyGrabber>();
            if (_assemblyGrabber == null)
            {
                // Create the grabber
                _assemblyGrabber = gameObject.AddComponent<AssemblyGrabber>();
            }

            // Set the grabber defaults
            _assemblyGrabber.name = name;

            // Instantiate the grab cube
            grabCube = Instantiate(ProjectManager.PartManager.grabCubePrefab, transform);
            grabCube.name = "GrabCube";
            grabCube.SetActive(false);
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            // Take the inherited behavior
            base.MRETOnDestroy();

            // Destroy the assembly grabber
            Destroy(_assemblyGrabber);

            // Destroy the grabcube
            Destroy(grabCube);
        }
        #endregion MRETUpdateBehaviour

        /// <summary>
        /// Called prior to an enclosure being destroyed. This method is available
        /// to subclasses to perform any necessary processing prior to the enclosure being
        /// destroyed.<br>
        /// </summary>
        /// <param name="child">The child being destroyed</param>
        protected virtual void BeforeDestroyEnclosure(InteractableEnclosure enclosure)
        {
        }

        /// <summary>
        /// Destroys the supplied enclosure.
        /// </summary>
        /// <param name="enclosure">The enclosure to destroy</param>
        public void DestroyEnclosure(InteractableEnclosure enclosure)
        {
            // Check for a valid reference
            if (enclosure != null)
            {
                // Provide the enclosure to subclasses prior to being destroyed
                BeforeDestroyEnclosure(enclosure);

                // Make sure to remove internal reference
                if (enclosure == _enclosure) _enclosure = null;

                // Destroy the enclosure
                Destroy(enclosure);
            }
        }

        /// <summary>
        /// Destroys the enclosures directly under this object instance.<br>
        /// </summary>
        public void DestroyEnclosures()
        {
            // Remove any reference to an enclosure
            _enclosure = null;

            foreach (Transform transform in transform)
            {
                InteractableEnclosure enclosure = transform.GetComponent<InteractableEnclosure>();
                if (enclosure)
                {
                    // Destroy the enclosure
                    Destroy(enclosure);
                }
            }
        }

        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.BeforeDestroyChildGroup(GI)"/>
        protected override void BeforeDestroyChildGroup(InteractablePartGroup childGroup)
        {
            base.BeforeDestroyChildGroup(childGroup);

            // Destroy the enclosures in the group
            childGroup.DestroyEnclosures();
        }

        #region Serialization
        /// <summary>
        /// Asynchronously deserializes the supplied serialized assembly interaction settings and updates the supplied
        /// state with the result.
        /// </summary>
        /// <param name="serializedInteractions">The serialized <code>InteractionSettingsType</code> to deserialize</param>
        /// <param name="interactionsDeserializationState">The <code>SerializationState</code> to populate
        ///     with the result.</param>
        /// 
        /// <see cref="InteractionSettingsType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected virtual IEnumerator DeserializeAssemblyInteractions(InteractionSettingsType serializedInteractions,
            SerializationState interactionsDeserializationState)
        {
            // Interaction settings are optional, but we still need to deserialize the default settings
            if (serializedInteractions is null)
            {
                // Create the defaults
                serializedInteractions = new InteractionSettingsType();
            }

            // Deserialize the interactions
            bool usable = AssemblyUsable;
            bool grabbable = AssemblyGrabbable;
            SchemaUtil.DeserializeInteractions(serializedInteractions, ref usable, ref grabbable);
            AssemblyUsable = usable;
            AssemblyGrabbable = grabbable;

            // Mark as complete
            interactionsDeserializationState.complete = true;

            yield return null;
        }

        /// <summary>
        /// Deserializes the supplied serialized part enclosure into an interactable part enclosure instance
        /// </summary>
        /// <param name="serializedEnclosure">The serialized <code>EnclosureType</code> to deserialize</param>
        /// <param name="deserializationState">A <code>VersionedSerializationState</code> containing
        ///     the state of the deserialization process and the <code>InteractableEnclosure</code> on
        ///     success. Null on failure</param>
        /// <returns>An <code>IEnumerator</code> used for reentrance in coroutines</returns>
        protected virtual IEnumerator DeserializeEnclosure(EnclosureType serializedEnclosure, VersionedSerializationState<InteractableEnclosure> deserializationState)
        {
            Action<InteractableEnclosure> OnEnclosureLoadedAction = (InteractableEnclosure loadedEnclosure) =>
            {
                if (loadedEnclosure != null)
                {
                    // Assign the object
                    deserializationState.versioned = loadedEnclosure;

                    // Mark as complete
                    deserializationState.complete = true;
                }
                else
                {
                    // Record the error
                    deserializationState.Error("A problem was encountered instantiating the " +
                        nameof(InteractableEnclosure));
                }
            };

            // Instantiate and deserialize the enclosure
            MRET.ProjectManager.InstantiateObject(serializedEnclosure,
                null, gameObject.transform, OnEnclosureLoadedAction);

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            yield return null;
        }

        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.Deserialize(GT, SerializationState)"/>
        protected override IEnumerator Deserialize(PartsType serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Process this object specific deserialization
            serializedParts = serialized;

            // Perform the group deserialization

            // Assembly interaction settings deserialization (optional, but still deserialize defaults)
            {
                // Use the supplied serialized structure or a default
                InteractionSettingsType serializedInteractions = serializedParts.AssemblyInteractions ?? new InteractionSettingsType();

                // Deserialize the interactions
                SerializationState interactionsDeserializationState = new SerializationState();
                StartCoroutine(DeserializeAssemblyInteractions(serializedInteractions, interactionsDeserializationState));

                // Wait for the coroutine to complete
                while (!interactionsDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(interactionsDeserializationState);

                // If the user deserialization failed, there's no point in continuing
                if (deserializationState.IsError) yield break;

                // Clear the state
                deserializationState.Clear();
            }

            // Set the grabber values based upon our new deserialization values
            _assemblyGrabber.name = name;

            // Deserialize the enclosure
            if (serializedParts.Enclosure != null)
            {
                // Performn the deserialization
                VersionedSerializationState<InteractableEnclosure> enclosureDeserializationState = new VersionedSerializationState<InteractableEnclosure>();
                StartCoroutine(DeserializeEnclosure(serializedParts.Enclosure, enclosureDeserializationState));

                // Wait for the coroutine to complete
                while (!enclosureDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(enclosureDeserializationState);

                // Make sure the resultant enclosure type is not null
                if (enclosureDeserializationState.versioned is null)
                {
                    deserializationState.Error("Deserialized enclosure cannot be null, denoting a possible internal issue.");
                }

                // If the group deserialization failed, there's no point in continuing. Something is wrong
                if (deserializationState.IsError) yield break;

                // Save the enclosure reference
                _enclosure = enclosureDeserializationState.versioned;
            }

            // Configure the grab cube
            ConfigureGrabCube();

            // Place the grab cube
            PlaceGrabCube();

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <summary>
        /// Asynchronously serializes the assembly interaction settings into the supplied serialized interaction settings
        /// and updates the supplied state with the serialization state.
        /// </summary>
        /// <param name="serializedInteractions">The serialized <code>InteractionSettingsType</code> to
        ///     populate with the interaction settings</param>
        /// <param name="interactionsSerializationState">The <code>SerializationState</code> to populate with
        ///     the serialization state.</param>
        /// 
        /// <see cref="InteractionSettingsType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected virtual IEnumerator SerializeAssemblyInteractions(InteractionSettingsType serializedInteractions, SerializationState interactionsSerializationState)
        {
            // Interaction settings are optional
            if (serializedInteractions is null)
            {
                interactionsSerializationState.complete = true;
                yield return null;
            }

            // Serialize the interactions
            SchemaUtil.SerializeInteractions(serializedInteractions, AssemblyUsable, AssemblyGrabbable);

            // Mark as complete
            interactionsSerializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.Serialize(GT, SerializationState)"/>
        protected override IEnumerator Serialize(PartsType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the interaction settings
            InteractionSettingsType serializedInteractions = null;
            if ((AssemblyUsable != InteractableDefaults.USABLE) ||
                (AssemblyGrabbable != InteractableDefaults.INTERACTABLE))
            {
                // Serialize out the interactions
                serializedInteractions = new InteractionSettingsType();

                // Serialize out the interactions
                SerializationState interactionsSerializationState = new SerializationState();
                StartCoroutine(SerializeAssemblyInteractions(serializedInteractions, interactionsSerializationState));

                // Wait for the coroutine to complete
                while (!interactionsSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the serialization state
                serializationState.Update(interactionsSerializationState);

                // If the serialization failed, there's no point in continuing. Something is wrong
                if (serializationState.IsError) yield break;

                // Clear the state
                serializationState.Clear();
            }

            // Store the assembly interaction settings in the result
            serialized.AssemblyInteractions = serializedInteractions;

            // Serialize the enclosure
            serialized.Enclosure = null;
            if (Enclosure)
            {
                SerializationState enclosureSerializationState = new SerializationState();
                var serializedEnclosure = Enclosure.CreateSerializedType();
                StartCoroutine(Enclosure.SerializeWithLogging(serializedEnclosure, enclosureSerializationState));

                // Wait for the coroutine to complete
                while (!enclosureSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the serialization state
                serializationState.Update(enclosureSerializationState);

                // Check for an error condition
                if (serializationState.IsError) yield break;

                // Assign the enclosure
                serialized.Enclosure = serializedEnclosure;
            }

            // Save the final serialized reference
            serializedParts = serialized;

            // Record the deserialization state as complete
            serializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.InstantiateAndDeserialize(CT, GameObject, Transform, Action{CI})"/>
        protected override void InstantiateSerializable(PartType serializedChild, GameObject go, Transform parent, Action<InteractablePart> onFinished = null)
        {
            ProjectManager.PartManager.InstantiatePart(serializedChild, false, parent, onFinished);
        }

        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.InstantiateAndDeserialize(GT, GameObject, Transform, Action{GI})"/>
        protected override void InstantiateSerializable(PartsType serializedChildGroup, GameObject go, Transform parent, Action<InteractablePartGroup> onFinished = null)
        {
            ProjectManager.PartManager.InstantiateParts(serializedChildGroup, go, parent, onFinished);
        }

        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.ReadSerializedItems(GT)"/>
        protected override VersionedType[] ReadSerializedItems(PartsType serializedGroup) => serializedGroup.Items;

        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.WriteSerializedItems(GT, VersionedType[])"/>
        protected override void WriteSerializedItems(PartsType serializedGroup, VersionedType[] serializedItems)
        {
            serializedGroup.Items = serializedItems;
        }
        #endregion Serialization

        /// <summary>
        /// Configures the grab cube for the assembly. Grab cubes are only relevant for
        /// assemblies with enclosures.
        /// </summary>
        protected virtual void ConfigureGrabCube()
        {
            // Setup the grab cube
            grabCube.SetActive(false); // Start with it disabled
            grabCube.transform.localScale = TransformUtil.GetSizeAsScale(grabCube, new Vector3(0.1f, 0.1f, 0.1f));
            AssemblyGrabber grabCubeGrabber = grabCube.GetComponent<AssemblyGrabber>();
            if (grabCubeGrabber)
            {
                grabCubeGrabber.assemblyRoot = gameObject;
                if (Enclosure && _assemblyGrabber)
                {
                    // Link the grabbers
                    grabCubeGrabber.otherGrabbers.Add(_assemblyGrabber);
                    _assemblyGrabber.otherGrabbers.Add(grabCubeGrabber);

                    // Assign the assembly interactable settings
                    _assemblyGrabber.Grabbable = AssemblyGrabbable;
                    _assemblyGrabber.Usable = AssemblyUsable;

                    // Enable the assembly grabber
                    _assemblyGrabber.enabled = AssemblyGrabbable & AssemblyUsable;

                    // Activate the grab cube
                    grabCube.SetActive(AssemblyGrabbable & AssemblyUsable);
                }
            }
        }

        private const int MAXPLACEMENTITERATIONS = 1000;

        /// <summary>
        /// Places the grab cube for the assembly just outside the assembly bounds.
        /// </summary>
        private void PlaceGrabCube()
        {
            // Check assertions
            if (grabCube == null) return;

            int iterations = 0;

            // Get parent and initialize grab cube at center of assembly.
            Transform parentTransform = grabCube.transform.parent;
            if (parentTransform == null)
            {
                LogError("Grabcube parent is NULL");
                return;
            }

            // Build the bounding box of the assembly because we will place the grabcube
            // in the middle to start. Just in case there are no colliders, start with the
            // center of the assembly.
            Bounds bou = new Bounds(parentTransform.position, Vector3.zero);
            bool init = false;
            foreach (Collider col in parentTransform.GetComponentsInChildren<Collider>())
            {
                if (col.gameObject == grabCube)
                {
                    // Exclude assembly grabbers
                    continue;
                }

                if (!init)
                {
                    // We have at least one collider, so reinitialize the bounds
                    bou = new Bounds(col.bounds.center, Vector3.zero);
                    init = true;
                }
                else
                {
                    // Expand bounds
                    bou.Encapsulate(col.bounds);
                }
            }

            // Start the grabCube in the center of the assembly
            grabCube.transform.position = bou.center;

            // Continuously move out grab cube until it is not encapsulated by part of the assembly
            Collider[] assemblyColliders = parentTransform.GetComponentsInChildren<Collider>();
            foreach (Collider colliderToCheck in assemblyColliders)
            {
                // Ignore the grabCube collider
                if (colliderToCheck.gameObject != grabCube)
                {
                    // Keep moving out until the grabCube is not contained within the collider
                    while (colliderToCheck.bounds.Contains(grabCube.transform.position))
                    {
                        // The grab cube is within an assembly object, so it will be moved further out
                        grabCube.transform.position = new Vector3(grabCube.transform.position.x + 0.5f,
                            grabCube.transform.position.y + 0.5f, grabCube.transform.position.z + 0.5f);

                        // Put a cap on attempts
                        if (++iterations > MAXPLACEMENTITERATIONS)
                        {
                            LogWarning("Unable to move grab cube out of object.", nameof(PlaceGrabCube));
                            break;
                        }
                    }
                }
            }
        }
    }

    public class InteractablePartGroupDefaults
    {
        // We want to use the default values from the schema to keep in sync
        public static readonly bool ASSEMBLY_GRABBABLE = new InteractionSettingsType().EnableInteraction;
        public static readonly bool ASSEMBLY_USABLE = new InteractionSettingsType().EnableUsability;
    }
}
