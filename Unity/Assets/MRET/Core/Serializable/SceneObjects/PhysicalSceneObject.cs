// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using GOV.NASA.GSFC.XR.Utilities.Collider;
using GOV.NASA.GSFC.XR.Utilities.Renderer;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.UI.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects
{
    /// <remarks>
    /// History:
    /// 17 Nov 2020: Created
    /// 18 Nov2020: Inherit from Interactable
    /// 10 Sep 2021: Inherit from InteractableSceneObject (J. Hosler)
    /// </remarks>
    /// <summary>
    /// Base class for all physical MRET Scene Objects.
    /// Author: Dylan Z. Baker
    /// </summary>
    public abstract class PhysicalSceneObject<T> : InteractableSceneObject<T>, IPhysicalSceneObject<T>
        where T : PhysicalSceneObjectType, new()
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(PhysicalSceneObject<T>);

        // Multipliers for the explode calculations
        public const float DEFAULT_EXPLODE_MIN_FACTOR = 0.1f;
        public const float DEFAULT_EXPLODE_MAX_FACTOR = 0.2f;

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedPhysicalSceneObject;

        protected CollisionHandler collisionHandler;

        private ModelContainer _modelContainer;

        private bool lastEnableCollisions;
        private bool lastEnableGravity;

        #region IPhysicalSceneObject
        /// <seealso cref="IPhysicalSceneObject.CreateSerializedType"/>
        PhysicalSceneObjectType IPhysicalSceneObject.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="IPhysicalSceneObject.Rigidbody"/>
        public Rigidbody Rigidbody { get; private set; }

        /// <seealso cref="IPhysicalSceneObject.Mass"/>
        public float Mass
        {
            get => Rigidbody.mass;
            set => Rigidbody.mass = value;
        }

        /// <seealso cref="IPhysicalSceneObject.AngularDrag"/>
        public float AngularDrag
        {
            get => Rigidbody.angularDrag;
            set => Rigidbody.angularDrag = value;
        }

        /// <seealso cref="IPhysicalSceneObject.Drag"/>
        public float Drag
        {
            get => Rigidbody.drag;
            set => Rigidbody.drag = value;
        }

        /// <seealso cref="IPhysicalSceneObject.EnableCollisions"/>
        public bool EnableCollisions
        {
            get => !Rigidbody.isKinematic;
            set => Rigidbody.isKinematic = !value;
        }

        /// <seealso cref="IPhysicalSceneObject.EnableGravity"/>
        public bool EnableGravity
        {
            get => Rigidbody.useGravity;
            set =>
                // NOTE: Only useful if kinematic is off, but let's not assume order of operations
                Rigidbody.useGravity = value;
        }

        /// <seealso cref="IPhysicalSceneObject.Specifications"/>
        public PhysicalSpecifications Specifications { get => specifications; }
        private PhysicalSpecifications specifications = null;

        /// <seealso cref="IPhysicalSceneObject.RandomizeTextures"/>
        public bool RandomizeTextures
        {
            get => randomizeTextures;
            set => randomizeTextures = value;
        }

        [SerializeField]
        [Tooltip("Indicates whether randomized textures are applied to the physical scene object.")]
        private bool randomizeTextures = PhysicalSceneObjectDefaults.RANDOMIZE_TEXTURES;

        /// <seealso cref="IPhysicalSceneObject.Exploding"/>
        public bool Exploding { get; private set; }

        /// <seealso cref="IPhysicalSceneObject.ExplodeMode"/>
        public Preferences.ExplodeMode ExplodeMode
        {
            get => explodeMode;
            set => explodeMode = value;
        }

        [SerializeField]
        [Tooltip("The mode when exploding.")]
        private Preferences.ExplodeMode explodeMode = Preferences.ExplodeMode.Relative;

        /// <seealso cref="IPhysicalSceneObject.ExplodeScale"/>
        public float ExplodeScale
        {
            get => explodeScale;
            set => explodeScale = value;
        }

        [SerializeField]
        [Tooltip("The scale of effect when exploding.")]
        private float explodeScale = 1f;

        /// <seealso cref="IPhysicalSceneObject.Synchronize(PhysicalSceneObjectType, Action{bool, string})"/>
        void IPhysicalSceneObject.Synchronize(PhysicalSceneObjectType serialized, Action<bool, string> onFinished)
        {
            Synchronize(serialized as T, onFinished);
        }

        /// <seealso cref="IPhysicalSceneObject.Deserialize(PhysicalSceneObjectType, Action{bool, string})"/>
        void IPhysicalSceneObject.Deserialize(PhysicalSceneObjectType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="IPhysicalSceneObject.Serialize(PhysicalSceneObjectType, Action{bool, string})"/>
        void IPhysicalSceneObject.Serialize(PhysicalSceneObjectType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion IPhysicalSceneObject

        #region MRETUpdateBehaviour
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)

                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETAwake"/>
        protected override void MRETAwake()
        {
            base.MRETAwake();

            // Make sure we have a rigid body
            Rigidbody = gameObject.GetComponent<Rigidbody>();
            if (Rigidbody == null)
            {
                Rigidbody = gameObject.AddComponent<Rigidbody>();

                // Set the rigid body defaults
                EnableGravity = PhysicalSceneObjectDefaults.GRAVITY_ENABLED;
                EnableCollisions = PhysicalSceneObjectDefaults.COLLISIONS_ENABLED;

                // PhysX defaults
                Rigidbody.mass = PhysicalSceneObjectDefaults.MASS_MAX;
                Rigidbody.angularDrag = PhysicalSceneObjectDefaults.ANGULARDRAG;
                Rigidbody.drag = PhysicalSceneObjectDefaults.DRAG;
            }

            // Set the defaults
            specifications = new PhysicalSpecifications();
//            RandomizeTextures = PhysicalSceneObjectDefaults.RANDOMIZE_TEXTURES;
            Exploding = false;
//            ExplodeMode = Preferences.ExplodeMode.Relative;
//            ExplodeScale = 1f;
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Assign a default configuration panel
            configurationPanelPrefab = GetDefaultConfigurationPanelPrefab();

            // Enable any attached colliders
            EnableColliders(true);
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            // Take the inherited behavior
            base.MRETOnDestroy();

            // Destroy the model
            _modelContainer = null;

            // Destroy the configuration panel
            DestroyConfigurationPanel();
        }
        #endregion MRETUpdateBehaviour

        /// <summary>
        /// Destroys the model reference
        /// </summary>
        private void DestroyModel()
        {
            // Only destroy the model game object if it's a true child of this transform
            if (_modelContainer != null)
            {
                GameObject model = _modelContainer.Model;
                if ((model != null) && model.transform.IsChildOf(transform) && (model.transform != transform))
                {
                    Destroy(model);
                }
            }

            // Destroy the model container
            _modelContainer = null;
        }

        #region Serialization
        /// <summary>
        /// Asynchronously Deserializes the supplied serialized model and updates the supplied state
        /// with the resulting model.
        /// </summary>
        /// <param name="serializedModel">The serialized <code>ModelType</code> model</param>
        /// <param name="modelDeserializationState">The <code>GameObjectSerializationState</code> to populate with the result.</param>
        /// 
        /// <see cref="ModelType"/>
        /// <see cref="GameObjectSerializationState"/>
        /// 
        protected virtual IEnumerator DeserializeModel(ModelType serializedModel, ObjectSerializationState<GameObject> modelDeserializationState)
        {
            void DeserializeModelAction(GameObject model)
            {
                // Assign the model
                modelDeserializationState.obj = model;

                // TODO: Resize the mesh to match the dimensions

                // Update the model state
                if (model == null)
                {
                    modelDeserializationState.Error("There was a problem deserializing the model");
                }

                // Mark as complete
                modelDeserializationState.complete = true;
            };

            // Load the model
            SchemaUtil.DeserializeModel(serializedModel, DeserializeModelAction);

            // Wait for the deserialization to complete
            while (!modelDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the deserialization failed, there's no point in continuing
            if (modelDeserializationState.IsError) yield break;

            yield return null;
        }

        /// <summary>
        /// Asynchronously deserializes the supplied serialized physics settings and updates the supplied
        /// state with the result.
        /// </summary>
        /// <param name="serializedPhysics">The serialized <code>PhysicsSettingsType</code> to deserialize</param>
        /// <param name="physicsDeserializationState">The <code>SerializationState</code> to populate
        ///     with the result.</param>
        /// 
        /// <see cref="PhysicsSettingsType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected virtual IEnumerator DeserializePhysics(PhysicsSettingsType serializedPhysics, SerializationState physicsDeserializationState)
        {
            // Physics settings are optional, but we still need to deserialize the default settings
            if (serializedPhysics is null)
            {
                // Create the defaults
                serializedPhysics = new PhysicsSettingsType();
            }

            // Deserialize the physics
            bool enableCollisions = EnableCollisions;
            bool enableGravity = EnableGravity;
            SchemaUtil.DeserializePhysics(serializedPhysics, ref enableCollisions, ref enableGravity);
            EnableCollisions = enableCollisions;
            EnableGravity = enableGravity;

            // Mark as complete
            physicsDeserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="InteractableSceneObject{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(T serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Process this object specific deserialization

            // Save the serialized reference
            serializedPhysicalSceneObject = serialized;

            // Specifications deserialization (optional, but still deserialize defaults)
            {
                // Use the supplied serialized structure or a default
                PhysicalSpecificationsType serializedSpecs = serializedPhysicalSceneObject.Specifications ?? new PhysicalSpecificationsType();

                // Deserialize the specs
                SerializationState specsDeserializationState = new SerializationState();
                StartCoroutine(specifications.Deserialize(serializedSpecs, specsDeserializationState));

                // Wait for the coroutine to complete
                while (!specsDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(specsDeserializationState);

                // If the deserialization failed, abort
                if (deserializationState.IsError) yield break;

                // Clear the state
                deserializationState.Clear();
            }

            // Destroy the old model
            DestroyModel();

            // Perform the model deserialization
            if (serializedPhysicalSceneObject.Model != null)
            {
                ObjectSerializationState<GameObject> modelDeserializationState = new ObjectSerializationState<GameObject>();
                StartCoroutine(DeserializeModel(serializedPhysicalSceneObject.Model, modelDeserializationState));

                // Wait for the coroutine to complete
                while (!modelDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(modelDeserializationState);

                // If the model loading failed, exit with an error
                if (deserializationState.IsError) yield break;

                // Set the model reference
                SetModel(modelDeserializationState.obj, serializedPhysicalSceneObject.Model);

                // Reapply the transform to factor in the child model
                if (serializedPhysicalSceneObject.Transform != null)
                {
                    SchemaUtil.DeserializeTransform(serializedPhysicalSceneObject.Transform, gameObject);
                }

                // Clear the state
                deserializationState.Clear();
            }

            // Make sure we have a Rigidbody reference
            CreateRigidbody(gameObject);

            // Update the rigid body settings
            Mass = specifications.massMax;
            AngularDrag = specifications.angularDrag;
            Drag = specifications.drag;

            // Physics settings deserialization (optional, but still deserialize defaults)
            {
                // Use the supplied serialized structure or a default
                PhysicsSettingsType serializedPhysics = serializedPhysicalSceneObject.Physics ?? new PhysicsSettingsType();

                // Deserialize the Physics
                SerializationState physicsDeserializationState = new SerializationState();
                StartCoroutine(DeserializePhysics(serializedPhysics, physicsDeserializationState));

                // Wait for the coroutine to complete
                while (!physicsDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(physicsDeserializationState);

                // If the user deserialization failed, there's no point in continuing
                if (deserializationState.IsError) yield break;

                // Clear the state
                deserializationState.Clear();
            }

            // TODO: Do we need to adjust the bounds to encapsulate children?
            //Bounds bounds = TransformUtil.GetBounds(gameObject);

            // Deserialize the flags
            RandomizeTextures = serializedPhysicalSceneObject.RandomizeTextures;
            if (RandomizeTextures)
            {
                // Apply the randomized textures
                int texturesToApply = Random.Range(0, ProjectManager.DefaultPartMaterials.Count-1);
                RendererUtil.ApplyMaterial(gameObject, ProjectManager.DefaultPartMaterials[texturesToApply]);
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <summary>
        /// Asynchronously serializes the physics settings into the supplied serialized physics settings
        /// and updates the supplied state with the serialization state.
        /// </summary>
        /// <param name="serializedPhysics">The serialized <code>PhysicsSettingsType</code> to populate with the physics settings</param>
        /// <param name="physicsSerializationState">The <code>SerializationState</code> to populate with the serialization state.</param>
        /// 
        /// <see cref="PhysicsSettingsType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected virtual IEnumerator SerializePhysics(PhysicsSettingsType serializedPhysics, SerializationState physicsSerializationState)
        {
            // Physics settings are optional
            if (serializedPhysics != null)
            {
                // Serialize the physics
                SchemaUtil.SerializePhysics(serializedPhysics, EnableCollisions, EnableGravity);
            }

            // Mark as complete
            physicsSerializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="InteractableSceneObject{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(T serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Serialize the object specific configuration

            // Start with our internal serialized model to serialize out the model
            // using the original deserialized structure (if was provided during deserialization)
            ModelType serializedModel = null;
            if (_modelContainer != null)
            {
                // Use the model container structure
                serializedModel = _modelContainer.SerializedModel;
            }

            // Serialize out the model
            serialized.Model = serializedModel;

            // Serialize the physics settings
            PhysicsSettingsType serializedPhysics = null;
            if ((EnableCollisions != PhysicalSceneObjectDefaults.COLLISIONS_ENABLED) ||
                (EnableGravity != PhysicalSceneObjectDefaults.GRAVITY_ENABLED))
            {
                // Serialize out the physics
                serializedPhysics = new PhysicsSettingsType();

                // Serialize out the physics
                SerializationState physicsSerializationState = new SerializationState();
                StartCoroutine(SerializePhysics(serializedPhysics, physicsSerializationState));

                // Wait for the coroutine to complete
                while (!physicsSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the serialization state
                serializationState.Update(physicsSerializationState);

                // If the serialization failed, there's no point in continuing. Something is wrong
                if (serializationState.IsError) yield break;

                // Clear the state
                serializationState.Clear();
            }

            // Store the physics settings in the result
            serialized.Physics = serializedPhysics;

            // Assign over the Rigidbody settings before we serialize.
            // FIXME: The linkage between physical specs and the Rigidbody should be cleaner,
            // i.e. The physical specs should always reflect the actual settings.
            specifications.massMax = Mass;
            specifications.angularDrag = AngularDrag;
            specifications.drag = Drag;

            // Physical specifications (optional)
            PhysicalSpecificationsType serializedSpecifications = null;
            if ((!specifications.MassIsDefaults) ||
                (specifications.notes != PhysicalSceneObjectDefaults.NOTES) ||
                (specifications.reference.ToString() != PhysicalSceneObjectDefaults.REFERENCE))
            {
                serializedSpecifications = new PhysicalSpecificationsType();

                // Perform the serialization
                SerializationState specsSerializationState = new SerializationState();
                StartCoroutine(specifications.Serialize(serializedSpecifications, specsSerializationState));

                // Wait for the coroutine to complete
                while (!specsSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the serialization state
                serializationState.Update(specsSerializationState);

                // If the serialization failed, exit with an error
                if (serializationState.IsError) yield break;

                // Clear the state
                serializationState.Clear();
            }

            // Store the specs in the result
            serialized.Specifications = serializedSpecifications;

            // Serialize the remaining fields
            serialized.RandomizeTextures = RandomizeTextures;

            // Save the final serialized reference
            serializedPhysicalSceneObject = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serialization

        #region Using
        /// <seealso cref="InteractableSceneObject{T}.DoClick(InputHand)"/>
        protected override void DoClick(InputHand hand)
        {
            base.DoClick(hand);

            if (configurationPanel == null)
            {
                // We don't have a configuration panel instance, so create it
                LoadConfigurationPanel(hand, false);
            }
            else
            {
                // Destroy the panel, but save the position in a dummy gameobject
                configurationPanelInfo = new GameObject();
                configurationPanelInfo.transform.position = configurationPanel.transform.position;
                DestroyConfigurationPanel();
            }
        }

        /// <seealso cref="InteractableSceneObject{T}.DoDoubleClick(InputHand)"/>
        protected override void DoDoubleClick(InputHand hand)
        {
            base.DoDoubleClick(hand);

            if (configurationPanel != null)
            {
                // Reinitialize the configuration panel
                LoadConfigurationPanel(hand, true);
            }
        }
        #endregion Using

        #region EXPLODE
        protected enum ExplodeDirection
        {
            Expanding = 1,
            Collapsing = -1
        }

        private float _explodeFactor = 1f;
        public float ExplodeFactor
        {
            get => _explodeFactor;
        }

        protected class ExplodeChild
        {
            public GameObject gameObject = null;
            public Vector3 originalPosition = Vector3.zero;
            public float originalDistanceFromParent = 0f;
            public Vector3 explodeVector = Vector3.zero;
            public Space explodeVectorSpace = Space.World;

            public void RestorePosition()
            {
                if (gameObject)
                {
                    gameObject.transform.position = originalPosition;
                }
            }
        }

        private List<ExplodeChild> explodeChildren = new List<ExplodeChild>();

        /// <summary>
        /// Called to initialize the exploding. This must be called before <code>Explode</code>
        /// or <code>Unexplode</code>.
        /// </summary>
        /// <see cref="StopExplode"/>
        public void StartExplode()
        {
            if (Exploding)
            {
                return;
            }

            // Save the state of the explode children because we are going to alter their positions
            // and need to restore them later.

            // Start with an empty list
            explodeChildren.Clear();

            // We only care about renderable children
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            {
                // Make sure we have a renderer and skip this gameobject. We only want the children.
                if (renderer && (renderer.gameObject != this.gameObject))
                {
                    // Save the child position
                    ExplodeChild explodeChild = new ExplodeChild()
                    {
                        gameObject = renderer.gameObject,
                        originalPosition = renderer.gameObject.transform.position,
                        originalDistanceFromParent = Vector3.Distance(renderer.gameObject.transform.position, transform.position),

                        // Initialize the explode vector based upon the explode mode
                        explodeVector = (ExplodeMode == Preferences.ExplodeMode.Relative) ?
                            renderer.gameObject.transform.position - transform.position :
                            RandomExplodeVector(DEFAULT_EXPLODE_MIN_FACTOR * ExplodeScale, DEFAULT_EXPLODE_MAX_FACTOR * ExplodeScale),
                        explodeVectorSpace = (ExplodeMode == Preferences.ExplodeMode.Relative) ? Space.World : Space.Self
                    };

                    // Add explode child info to our master array
                    explodeChildren.Add(explodeChild);
                }
            }

            // Mark exploded as being active
            Exploding = true;
        }

        /// <summary>
        /// Translates all the explode children in the supplied direction
        /// </summary>
        /// <param name="direction">The <code>ExplodeDirection</code> defining the direction of the translation</param>
        protected void ExplodeTranslate(ExplodeDirection direction)
        {
            // Move each child along a positive random vector. 
            // Ensure that the object moving is not just the collider child.
            foreach (ExplodeChild explodeChild in explodeChildren)
            {
                // Check assertions
                if (explodeChild.gameObject)
                {
                    // Translate the child along the explode vector in the specified direction
                    Vector3 translateVector = explodeChild.explodeVector * UnityEngine.Time.deltaTime * (float)direction;

                    // Perform the translate
                    explodeChild.gameObject.transform.Translate(translateVector, explodeChild.explodeVectorSpace);

                    // Calculate the new distance from parent
                    float distanceFromParent = Vector3.Distance(explodeChild.gameObject.transform.position, transform.position);

                    // Calculate the explosion scale
                    _explodeFactor = distanceFromParent / explodeChild.originalDistanceFromParent;
                }
            }
        }

        /// <summary>
        /// Called to explode the children in the expanding direction
        /// </summary>
        public void Explode()
        {
            if (!Exploding)
            {
                return;
            }

            // Expand the children
            ExplodeTranslate(ExplodeDirection.Expanding);
        }

        /// <summary>
        /// Called to explode the children in the collapsing direction
        /// </summary>
        public void Unexplode()
        {
            if (!Exploding)
            {
                return;
            }

            // Collapse the children
            ExplodeTranslate(ExplodeDirection.Collapsing);
        }

        /// <summary>
        /// Called to stop the exploding.
        /// </summary>
        /// <see cref="StartExplode"/>
        public void StopExplode()
        {
            if (!Exploding)
            {
                return;
            }

            // Set each child to it's original position and clear the list
            foreach (ExplodeChild explodeChild in explodeChildren)
            {
                explodeChild.RestorePosition();
            }

            // Clear the list
            explodeChildren.Clear();

            // Mark explode as inactive
            Exploding = false;
        }

        /// <summary>
        /// Generates a random explode vector. Y value is set to 0 so explosion only happens horizontally
        /// </summary>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>A random explode <code>Vector3</code></returns>
        protected virtual Vector3 RandomExplodeVector(float min, float max)
        {
            var x = Random.Range(min, max);
            var y = Random.Range(0, 0);
            var z = Random.Range(min, max);

            return new Vector3(x, y, z);
        }

        #endregion
        
        /// <seealso cref="Identifiable{T}.CreateSynchronizedObject"/>
        protected override ISynchronized CreateSynchronizedObject()
        {
            return gameObject.AddComponent<SynchronizedPhysicalSceneObject>();
        }

        /// <seealso cref="InteractableSceneObject{T}.AfterBeginGrab(InputHand)"/>
        protected override void AfterBeginGrab(InputHand hand)
        {
            base.AfterBeginGrab(hand);

            collisionHandler = gameObject.AddComponent<CollisionHandler>();
            collisionHandler.enabled = false;
            collisionHandler.grabbingObj = hand.gameObject;
            collisionHandler.enabled = true;

            // Save the physics state
            lastEnableGravity = EnableGravity;
            lastEnableCollisions = EnableCollisions;

            // Temporarily disable gravity (ensure non-convex colliders and zero mass objects remain kinematic or errors will result in Unity 5+)
            MeshCollider meshCollider = gameObject.GetComponentInChildren<MeshCollider>();
            EnableCollisions = (!Mathf.Approximately(Mass, default) && (meshCollider != null))
                ? !meshCollider.convex  // We have a mass *and* a mesh collider, so use the existing value
                : false;                // No mass or no mesh collider, so set to false
            EnableGravity = false;
            //Mass = 0f;
            //AngularDrag = 0f;
            //Drag = 0f;
        }

        /// <seealso cref="InteractableSceneObject{T}.BeforeEndGrab(InputHand)"/>
        protected override void BeforeEndGrab(InputHand hand)
        {
            base.BeforeEndGrab(hand);

            if (collisionHandler != null)
            {
                if (!selected)
                {
                    collisionHandler.ResetTextures();
                }
                Destroy(collisionHandler);
            }
        }

        /// <seealso cref="InteractableSceneObject{T}.AfterEndGrab(InputHand)"/>
        protected override void AfterEndGrab(InputHand hand)
        {
            base.AfterEndGrab(hand);

            // Restore the physics state
            EnableGravity = lastEnableGravity;
            EnableCollisions = lastEnableCollisions;
        }

        /// <summary>
        /// Assigns a model to this physical scene object. If a previous model exists, it
        /// will be destroyed before assigning the new model.
        /// </summary>
        /// <param name="model">The <code>GameObject</code> containing the model</param>
        /// <param name="serializedModel">The optional <code>ModelType</code> containing
        ///     the serialized representation of the model</param>
        protected void SetModel(GameObject model, ModelType serializedModel = null)
        {
            // Destroy the old model reference
            DestroyModel();

            // Remove the collider because we want to recreate it based upon the supplied model
            RemovePreviewColliders(gameObject);

            // Setup the model parent
            model.transform.SetParent(transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;

            // Create the new container
            _modelContainer = new ModelContainer(model, serializedModel);

            // Remove all rigid bodies in the model, because we don't want nested rigid bodies
            RemoveNestedRigidBodies(gameObject);

            // Make sure we have a collider
            CreatePreviewCollider();

            // TODO: Raycast colliders were implemented for the old line drawing system and they were
            // added to handle better raycasting with point clouds. In the current implementation of
            // MRET, they cause problems with the HandInteractor collider triggers. The concept needs
            // evaluated for purpose.
            //AddRaycastMeshColliders();

            // Enable the colliders
            EnableColliders(true);
        }

        /// <summary>
        /// Removes all of the nestedrigid bodies in the supplied game object. Nested rigid
        /// bodies cause problems with PhysX so this method is useful for destroying nested
        /// rigid bodies.
        /// </summary>
        /// <param name="go">The <code>GameObject</code> containing the nested rigid bodies
        ///     to destroy</param>
        protected void RemoveNestedRigidBodies(GameObject go)
        {
            foreach (Rigidbody rigidBody in go.GetComponentsInChildren<Rigidbody>())
            {
                // We want to keep the rigid body on our transform
                if (rigidBody.transform != transform)
                {
                    Destroy(rigidBody);
                }
            }
        }

        /// <summary>
        /// Removes all of the preview colliders in the supplied game object.
        /// </summary>
        /// <param name="go">The <code>GameObject</code> containing the colliders to destroy</param>
        protected void RemovePreviewColliders(GameObject go)
        {
            foreach (Collider collider in go.GetComponentsInChildren<Collider>())
            {
                if (collider.gameObject.layer == MRET.previewLayer)
                {
                    Destroy(collider);
                }
            }
        }

        /// <summary>
        /// Creates a preview collider for the supplied object. The ConfigurationManager will
        /// be used to determine the type of collider to create.
        /// </summary>
        /// 
        /// <seealso cref="Collider"/>
        /// <seealso cref="ConfigurationManager"/>
        /// 
        protected void CreatePreviewCollider()
        {
            // Use the collider utility in case an alternate solution to Collider is used for non-convex
            // colliders
            if (!ColliderUtil.HasColliderInChildren(gameObject))
            {
                ConfigurationManager.ColliderMode mode = MRET.ConfigurationManager.colliderMode;

                // Log a message
                Log("No collider detected. Generating collider type: " + mode, nameof(CreatePreviewCollider));

                // Create the collider based upon the configuration manager settings
                switch (mode)
                {
                    case ConfigurationManager.ColliderMode.Box:

                        // Create the box collider
                        ColliderUtil.CreateBoxCollider(gameObject);
                        gameObject.layer = MRET.previewLayer;
                        break;

                    case ConfigurationManager.ColliderMode.NonConvex:

                        // Create the NonConvexMeshColliders for each mesh making up the object
                        ColliderUtil.CreateNonConvexMeshColliders(gameObject);
                        foreach (MeshFilter mesh in gameObject.GetComponentsInChildren<MeshFilter>())
                        {
                            mesh.gameObject.layer = MRET.previewLayer;
                        }
                        break;

                    case ConfigurationManager.ColliderMode.None:
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Temporary fix for a bug occurring with colliders being disabled.
        /// </summary>
        protected void EnableColliders(bool enabled)
        {
            ColliderUtil.Enable(gameObject, enabled);
        }

        /// <summary>
        /// Adds the raycast mesh colliders to this physical scene object
        /// </summary>
        protected void AddRaycastMeshColliders()
        {
            foreach (MeshFilter mesh in gameObject.GetComponentsInChildren<MeshFilter>())
            {
                // Check for existing mesh collider
                MeshCollider mcoll = mesh.gameObject.GetComponentInChildren<MeshCollider>();
                if (mcoll == null)
                {
                    // Create a child gameobject to hold the collider
                    GameObject raycastColliderObj = new GameObject("RaycastCollider");
                    raycastColliderObj.transform.parent = mesh.transform;
                    raycastColliderObj.transform.localPosition = Vector3.zero;
                    raycastColliderObj.transform.localRotation = Quaternion.identity;
                    raycastColliderObj.transform.localScale = Vector3.one;
                    raycastColliderObj.layer = MRET.raycastLayer;

                    // Add a mesh collider
                    mcoll = raycastColliderObj.gameObject.AddComponent<MeshCollider>();
                    mcoll.convex = true;
                }

                // Share the mesh
                mcoll.sharedMesh = mesh.mesh;
            }
        }

        /// <summary>
        /// Updates mesh colliders to the current collisions enabled state
        /// </summary>
/* Remove
        protected void UpdateMeshColliders()
        {
            foreach (MeshCollider mcoll in gameObject.GetComponentsInChildren<MeshCollider>())
            {
                mcoll.convex = !EnableCollisions;
            }
        }
*/

        /// <summary>
        /// Creates a Rigidbody and initializes to defaults. If the RigidBody already exists,
        /// this method does nothing.
        /// </summary>
        /// <returns name="toRigidbody">The <code>Rigidbody</code> to copy the settings to</returns>
        protected Rigidbody CreateRigidbody(GameObject gameObject)
        {
            Rigidbody result = gameObject.GetComponentInChildren<Rigidbody>();
            if (result == null)
            {
                result = gameObject.AddComponent<Rigidbody>();

                // Set the rigid body defaults
                EnableGravity = PhysicalSceneObjectDefaults.GRAVITY_ENABLED;
                EnableCollisions = PhysicalSceneObjectDefaults.COLLISIONS_ENABLED;

                // PhysX defaults
                result.mass = PhysicalSceneObjectDefaults.MASS_MAX;
                result.angularDrag = PhysicalSceneObjectDefaults.ANGULARDRAG;
                result.drag = PhysicalSceneObjectDefaults.DRAG;
            }

            return result;
        }

        /// <summary>
        /// Helper function to copy the Rigidbody settings of this PhysicalSceneObject to
        /// another Rigidbody. This is available to ensure that "relevant" rigid body settings
        /// are centralized.
        /// </summary>
        /// <param name="toRigidbody">The <code>Rigidbody</code> to copy the settings to</param>
        protected void CopyRigidbodySettings(Rigidbody toRigidbody)
        {
            if (toRigidbody != null)
            {
                // Copy the rigid body settings
                toRigidbody.mass = Rigidbody.mass;
                toRigidbody.isKinematic = Rigidbody.isKinematic;
                toRigidbody.useGravity = Rigidbody.useGravity;
                toRigidbody.angularDrag = Rigidbody.angularDrag;
                toRigidbody.drag = Rigidbody.drag;
            }
        }

        #region Configuration Panel
        [Tooltip("Configuration Panel")]
        public GameObject configurationPanelPrefab;
        private GameObject configurationPanel;
        private GameObject configurationPanelInfo = null;

        /// <summary>
        /// Obtains a reference to the default configuration panel prefab for this physical
        /// scene object. Available for subclasses to override the default configuration
        /// panel prefab used for this physical scene object.
        /// </summary>
        /// <returns>A <code>GameObject</code> reference to the default configuration
        ///     panel</returns>
        protected virtual GameObject GetDefaultConfigurationPanelPrefab()
        {
            return ProjectManager.ObjectConfigurationPanelPrefab;
        }

        /// <summary>
        /// Obtains a reference to the default configuration panel prefab for this physical
        /// scene object. Available for subclasses to alter the interactable associated with
        /// the configuration panel.
        /// </summary>
        /// <returns>A <code>GameObject</code> reference to the default configuration
        ///     panel</returns>
        protected virtual IInteractable GetConfigurationPanelInteractable()
        {
            return this;
        }

        /// <summary>
        /// Loads the object configuration panel
        /// </summary>
        /// <param name="hand">The <code>InputHand</code> loading the panel</param>
        /// <param name="reinitialize">Indicated whether or not to reinitialize the panel</param>
        public void LoadConfigurationPanel(InputHand hand, bool reinitialize)
        {
            if (!ProjectManager.ObjectConfigurationPanelEnabled || (configurationPanelPrefab == null))
            {
                return;
            }

            if (configurationPanel == null)
            {
                // Instantiate the configuration panel
                configurationPanel = Instantiate(configurationPanelPrefab);
            }

            // Get the controller and initialize
            IInteractableConfigurationPanelController configurationPanelController =
                configurationPanel.GetComponent<IInteractableConfigurationPanelController>();
            if (configurationPanelController != null)
            {
                // Ask subclasses for the interactable to use
                IInteractable configPanelInteractable = GetConfigurationPanelInteractable();
                if (configPanelInteractable == null)
                {
                    // Make sure we have a valid interactable
                    configPanelInteractable = this;
                }
                configurationPanelController.Initialize(configPanelInteractable);
            }

            // Determine if we have a collider because we will use it to interpolate the position of the panel
            Collider objectCollider = GetComponentInChildren<Collider>();
            if (objectCollider)
            {
                // Get our collider bounds (world space)
                Bounds colliderBounds = ColliderUtil.GetBounds(gameObject);

                // If position hasn't been set or reinitializing, initialize panel position.
                if ((configurationPanelInfo == null) || reinitialize)
                {
                    // Find the closest point to the hand position on the collider
                    Vector3 selectedPosition = objectCollider.ClosestPointOnBounds(hand.transform.position);

                    // Move panel between selected point and headset.
                    configurationPanel.transform.position = Vector3.Lerp(selectedPosition, MRET.InputRig.head.transform.position, 0.1f);
                }
                else
                {
                    // Use the last saved position
                    configurationPanel.transform.position = configurationPanelInfo.transform.position;
                }

                // Reposition the panel if the panel position is inside of our collider
                if (colliderBounds.Contains(configurationPanel.transform.position))
                {
                    // Try to move panel outside of our collider. If the headset is contained
                    // within our collider, there is nothing we can do because we need to
                    // interpolate between the panel and the head.
                    if (!colliderBounds.Contains(MRET.InputRig.head.transform.position))
                    {
                        while (colliderBounds.Contains(configurationPanel.transform.position))
                        {
                            configurationPanel.transform.position = Vector3.Lerp(configurationPanel.transform.position,
                                MRET.InputRig.head.transform.position, 0.1f);
                        }
                    }

                    // Move the panel out just a little more
                    configurationPanel.transform.position = Vector3.Lerp(configurationPanel.transform.position,
                        MRET.InputRig.head.transform.position, 0.1f);
                }
            }
            else
            {
                // No collider, so position the panel close to the hand
                configurationPanel.transform.position = hand.transform.position;
            }

            // Finally, with the panel position set, point the panel at the camera, and make it a child
            configurationPanel.transform.rotation = Quaternion.LookRotation(MRET.InputRig.head.transform.forward);
            configurationPanel.transform.SetParent(transform.parent);
        }

        /// <summary>
        /// Destroys the object configuration panel
        /// </summary>
        protected void DestroyConfigurationPanel()
        {
            if (configurationPanel)
            {
                Destroy(configurationPanel);
            }
        }
        #endregion Configuration Panel

        public class ModelContainer
        {
            public GameObject Model { get => _model; }
            private GameObject _model = null;

            public ModelType SerializedModel { get => _serializedModel; }
            private ModelType _serializedModel = null;

            /// <summary>
            /// Constructor for the physical scene object model container
            /// </summary>
            /// <param name="model">The <code>GameObject</code> representing the game object of the model</param>
            /// <param name="serializedModel">The optional <code>ModelType</code> containing the serialized
            ///     representation of the model</param>
            public ModelContainer(GameObject model, ModelType serializedModel = null)
            {
                _model = model;
                _serializedModel = serializedModel;
            }

            /// <summary>
            /// Destroys the model tracked by this class instance
            /// </summary>
            public void DestroyModel()
            {
                _model = null;
            }

            /// <summary>
            /// Destructor
            /// </summary>
            ~ModelContainer()
            {
                DestroyModel();
            }
        }
    }

    /// <summary>
    /// Provides an implementation for the abstract PhysicalSceneObject class
    /// </summary>
    public class PhysicalSceneObject : PhysicalSceneObject<PhysicalSceneObjectType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(PhysicalSceneObject);
    }

    /// <summary>
    /// Defines physical specifications, i.e. mass, notes, reference
    /// </summary>
    public class PhysicalSpecifications
    {
        // TODO: RigidBody
        public float drag = PhysicalSceneObjectDefaults.DRAG;
        public float angularDrag = PhysicalSceneObjectDefaults.ANGULARDRAG;

        // Mass
        public float massMin = PhysicalSceneObjectDefaults.MASS_MIN;
        public float massMax = PhysicalSceneObjectDefaults.MASS_MAX;
        public float massContingency = PhysicalSceneObjectDefaults.MASS_CONTINGENCY;
        public MassUnitType massMinUnits = PhysicalSceneObjectDefaults.MASS_MIN_UNITS;
        public MassUnitType massMaxUnits = PhysicalSceneObjectDefaults.MASS_MAX_UNITS;
        public MassUnitType massContingencyUnits = PhysicalSceneObjectDefaults.MASS_CONTINGENCY_UNITS;

        // Supplementary information
        public Uri reference = new Uri(PhysicalSceneObjectDefaults.REFERENCE);
        public string notes = PhysicalSceneObjectDefaults.NOTES;

        /// <summary>
        /// Helper property to check if any mass settings are different than the defaults
        /// </summary>
        public bool MassIsDefaults =>
            Mathf.Approximately(massMin, PhysicalSceneObjectDefaults.MASS_MIN) &&
            (massMinUnits == PhysicalSceneObjectDefaults.MASS_MIN_UNITS) &&
            Mathf.Approximately(massMax, PhysicalSceneObjectDefaults.MASS_MAX) &&
            (massMaxUnits == PhysicalSceneObjectDefaults.MASS_MAX_UNITS) &&
            Mathf.Approximately(massContingency, PhysicalSceneObjectDefaults.MASS_CONTINGENCY) &&
            (massContingencyUnits == PhysicalSceneObjectDefaults.MASS_CONTINGENCY_UNITS);

        /// <summary>
        /// Performs deserialization of the supplied serialized mass class instance into the
        /// resultant arguments.
        /// </summary>
        /// <param name="serialized">The serialized <code>MassType</code> class instance to read</param>
        /// <param name="mass">The resultant mass converted to default Unity units</param>
        /// <param name="units">The original mass units before conversion</param>
        public void DeserializeMass(MassType serialized, ref float mass, ref MassUnitType units)
        {
            mass = default;
            MassType serializedMass = serialized ?? new MassType();
            SchemaUtil.DeserializeMass(serializedMass, ref mass);
            units = serializedMass.units;
        }

        /// <summary>
        /// Performs deserialization of the supplied serialized specifications into this class instance
        /// </summary>
        /// <param name="serializedSpecs">The serialized <code>PhysicalSpecificationsType</code> class
        ///     instance to read</param>
        /// <param name="deserializationState">The <code>SerializationState</code> to assign the
        ///     results of the deserialization.</param>
        /// <returns>An <code>IEnumerator</code> that can be used for asynchronous reentance</returns>
        public virtual IEnumerator Deserialize(PhysicalSpecificationsType serializedSpecs, SerializationState deserializationState)
        {
            if (serializedSpecs == null)
            {
                // Record the deserialization state
                deserializationState.Error("Specifications not defined");
                yield break;
            }

            // Mass (optional)
            if (serializedSpecs.Mass != null)
            {
                // Min (optional)
                DeserializeMass(serializedSpecs.Mass.Min, ref massMin, ref massMinUnits);

                // Max (optional)
                DeserializeMass(serializedSpecs.Mass.Max, ref massMax, ref massMaxUnits);

                // Min (optional)
                DeserializeMass(serializedSpecs.Mass.Contingency, ref massContingency, ref massContingencyUnits);
            }

            // Notes
            notes = serializedSpecs.Notes;

            // Reference (optional)
            try
            {
                reference = (!string.IsNullOrEmpty(serializedSpecs.Reference)) ?
                    new Uri(serializedSpecs.Reference) :
                    reference;
            }
            catch (Exception e)
            {
                // Don't report as error
                Debug.LogWarning("Reference is invalid: " + e);
                reference = new Uri(PhysicalSceneObjectDefaults.REFERENCE);
            }

            // Mark as complete
            deserializationState.complete = true;
        }

        /// <summary>
        /// Performs serialization of the supplied arguments into the supplied serialized mass
        /// class instance.
        /// </summary>
        /// <param name="mass">The mass in default Unity units</param>
        /// <param name="units">The mass units to use for serialization conversion</param>
        /// <param name="serialized">The serialized <code>MassType</code> class instance to write</param>
        public void SerializeMass(float mass, MassUnitType units, MassType serialized)
        {
            // Only assign the units if the serialized type contains the default
            // units. Assignment could still be the default, but this handles the
            // case where it's not.
            if (units != new MassType().units)
            {
                serialized.units = units;
            }
            SchemaUtil.SerializeMass(mass, serialized);
        }

        /// <summary>
        /// Performs serialization of this class instance into the supplied serialized specifications.
        /// </summary>
        /// <param name="serializedSpecs">The serialized <code>PhysicalSpecificationsType</code> class
        ///     instance to write</param>
        /// <param name="serializationState">The <code>SerializationState</code> to assign the
        ///     results of the serialization.</param>
        /// <returns>An <code>IEnumerator</code> that can be used for asynchronous reentance</returns>
        public IEnumerator Serialize(PhysicalSpecificationsType serializedSpecs, SerializationState serializationState)
        {
            if (serializedSpecs == null)
            {
                // Record the serialization state
                serializationState.Error("Specifications reference is not defined");
                yield break;
            }

            // Mass
            if (!MassIsDefaults)
            {
                // Make sure we have a serializable structure
                serializedSpecs.Mass = serializedSpecs.Mass ?? new MassSpecificationsType();

                // Min (optional)
                if (!Mathf.Approximately(massMin, PhysicalSceneObjectDefaults.MASS_MIN) ||
                    (massMinUnits != PhysicalSceneObjectDefaults.MASS_MIN_UNITS))
                {
                    serializedSpecs.Mass.Min = serializedSpecs.Mass.Min ?? new MassType();
                    SerializeMass(massMin, massMinUnits, serializedSpecs.Mass.Min);
                }

                // Max (optional)
                if (!Mathf.Approximately(massMax, PhysicalSceneObjectDefaults.MASS_MAX) ||
                    (massMaxUnits != PhysicalSceneObjectDefaults.MASS_MAX_UNITS))
                {
                    serializedSpecs.Mass.Max = serializedSpecs.Mass.Max ?? new MassType();
                    SerializeMass(massMax, massMaxUnits, serializedSpecs.Mass.Max);
                }

                // Contingency (optional)
                if (!Mathf.Approximately(massContingency, PhysicalSceneObjectDefaults.MASS_CONTINGENCY) ||
                    (massContingencyUnits != PhysicalSceneObjectDefaults.MASS_CONTINGENCY_UNITS))
                {
                    serializedSpecs.Mass.Contingency = serializedSpecs.Mass.Contingency ?? new MassType();
                    SerializeMass(massContingency, massContingencyUnits, serializedSpecs.Mass.Contingency);
                }
            }

            // Reference
            serializedSpecs.Notes = notes;
            serializedSpecs.Reference = (reference != null) ? reference.ToString() : PhysicalSceneObjectDefaults.REFERENCE;

            // Mark as complete
            serializationState.complete = true;
        }
    }

    public class PhysicalSceneObjectDefaults : InteractableDefaults
    {
        // We want to use the default values from the schema to keep in sync
        public static readonly bool RANDOMIZE_TEXTURES = new PhysicalSceneObjectType().RandomizeTextures;
        public static readonly bool COLLISIONS_ENABLED = new PhysicsSettingsType().EnableCollisions;
        public static readonly bool GRAVITY_ENABLED = new PhysicsSettingsType().EnableGravity;

        // Specifications
        public static readonly string REFERENCE = new PhysicalSpecificationsType().Reference;
        public static readonly string NOTES = new PhysicalSpecificationsType().Notes;
        public static readonly float MASS_MIN = 1.0f;       // Default from RigidBody
        public static readonly float MASS_MAX = 1.0f;       // Default from RigidBody
        public static readonly float MASS_CONTINGENCY = default;
        public static readonly MassUnitType MASS_MIN_UNITS = new MassType().units;
        public static readonly MassUnitType MASS_MAX_UNITS = new MassType().units;
        public static readonly MassUnitType MASS_CONTINGENCY_UNITS = new MassType().units;

        // TODO: Should these be in the schema?
        public static readonly float ANGULARDRAG = 0.99f;   // Default from RigidBody is 0.05f
        public static readonly float DRAG = 0.99f;          // Default from RigidBody is 0f
    }
}