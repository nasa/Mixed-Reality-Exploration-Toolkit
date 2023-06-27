// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.Utilities.Renderer;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.Tools.Selection;
using GOV.NASA.GSFC.XR.MRET.Integrations.CPIS.Transforms;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects
{
    /// <remarks>
    /// History:
    /// 17 November 2020: Created
    /// 18 November 2020: Inherit from SceneObject
    /// 9 August 2021: Added Motion Constraints (D. Chheda)
    /// 4 January 2022: Fixed bug with parent get (DZB)
    /// 11 October 2022: Renamed to SceneObject in preparation for the new schema hierarchy
    /// </remarks>
    /// 
    /// <summary>
    /// Base class for all MRET Scene Objects.
    /// 
    /// Author: Dylan Z. Baker
    /// Author: Jeffrey Hosler (refactored)
    /// </summary>
    public abstract class InteractableSceneObject<T> : SceneObject<T>, IInteractable<T>, ISelectable
        where T : InteractableSceneObjectType, new()
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(InteractableSceneObject<T>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedInteractableSceneObject;

        [Tooltip("Threshold to delay before considered IsTouching.")]
        public float touchHoldThreshold = 1f;

        /// <summary>
        /// Click timer tracking
        /// </summary>
        private bool clicked = false;
        private int clickTimer = -1;

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

        protected InputHand touchingHand;

        // Saved transform
        private Vector3 lastSavedPosition;
        private Quaternion lastSavedRotation;
        private Vector3 lastSavedScale;

        //TODO:Reenable anchored
        private bool anchored;


        protected bool selected { get; private set; }

        /// <seealso cref="IInteractable.interactableParent"/>
        public IInteractable interactableParent
        {
            get => (ProjectManager.SceneObjectManager != null)
                ? ProjectManager.SceneObjectManager.GetParent<IInteractable>(gameObject)
                : null;
        }

        /// <seealso cref="IInteractable.interactableChildren"/>
        public IInteractable[] interactableChildren
        {
            get => (ProjectManager.SceneObjectManager != null)
                ? ProjectManager.SceneObjectManager.GetChildren<IInteractable>(gameObject)
                : new IInteractable[0];
        }

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
            // Take the inherited behavior
            base.MRETAwake();

            // Setup telemetry listener
            if (dataPointChangeEvent == null)
            {
                dataPointChangeEvent = new DataManager.DataValueEvent();
                dataPointChangeEvent.AddListener(HandleDataPointChange);
            }

            // Set the defaults
            selected = false;
//            Grabbable = InteractableDefaults.INTERACTABLE;
//            Usable = InteractableDefaults.USABLE;
//            GrabBehavior = IInteractable.GrabBehaviors.Attach;
//            TouchBehavior = IInteractable.TouchBehaviors.Highlight;
            Visible = InteractableDefaults.VISIBLE;
            Opacity = InteractableDefaults.OPACITY;
            ShadeForLimitViolations = InteractableDefaults.SHADE_FOR_LIMIT_VIOLATIONS;
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            // Make sure we have materials
            if (HighlightMaterial == null)
            {
                HighlightMaterial = MRET.HighlightMaterial;
            }
            if (SelectionMaterial == null)
            {
                SelectionMaterial = MRET.SelectMaterial;
            }

            // Save the transform at the start
            lastSavedPosition = transform.position;
            lastSavedRotation = transform.rotation;
            lastSavedScale = transform.localScale;
            //TODO: Only set anchored for objects that should be locked in place
            #if HOLOLENS_BUILD
            anchored = !Grabbable;
            if (anchored)
            {
                ProjectManager.AnchorManager.AttachAnchor(gameObject);
            }
            #endif
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            // Click timer tracking
            if (clicked)
            {
                if (clickTimer < 0)
                {
                    clickTimer = 10;
                }
                else if (clickTimer == 0)
                {
                    clicked = false;
                    clickTimer = -1;
                }
                else
                {
                    clickTimer--;
                }
            }

            if (IsTouching && (TouchBehavior == IInteractable.TouchBehaviors.Hold))
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
        #endregion MRETUpdateBehaviour

        #region Serializable
        /// <summary>
        /// Asynchronously Deserializes the supplied serialized material and updates the supplied state
        /// with the resulting material.
        /// </summary>
        /// <param name="serializedMaterial">The serialized <code>MaterialType</code> material</param>
        /// <param name="materialDeserializationState">The <code>ObjectSerializationState</code> to populate
        ///     with the resultant <code>Material</code>, or null on error.</param>
        /// 
        /// <see cref="MaterialType"/>
        /// <see cref="ObjectSerializationState{O}"/>
        /// 
        protected virtual IEnumerator DeserializeMaterial(MaterialType serializedMaterial, ObjectSerializationState<Material> materialDeserializationState)
        {
            void DeserializeMaterialAction(Material material)
            {
                // Assign the material
                materialDeserializationState.obj = material;

                // Update the material state
                if (material == null)
                {
                    materialDeserializationState.Error("Material falied to deserialize");
                }

                // Mark as complete
                materialDeserializationState.complete = true;
            };

            // Load the material
            SchemaUtil.DeserializeMaterial(serializedMaterial, DeserializeMaterialAction);

            // Wait for the deserializing to complete
            while (!materialDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the deserialization failed, there's no point in continuing
            if (materialDeserializationState.IsError) yield break;

            yield return null;
        }

        /// <summary>
        /// Asynchronously deserializes the supplied serialized interaction settings and updates the supplied
        /// state with the result.
        /// </summary>
        /// <param name="serializedInteractions">The serialized <code>InteractionSettingsType</code> to deserialize</param>
        /// <param name="interactionsDeserializationState">The <code>SerializationState</code> to populate
        ///     with the result.</param>
        /// 
        /// <see cref="InteractionSettingsType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected virtual IEnumerator DeserializeInteractions(InteractionSettingsType serializedInteractions, SerializationState interactionsDeserializationState)
        {
            // Interaction settings are optional, but we still need to deserialize the default settings
            if (serializedInteractions is null)
            {
                // Create the defaults
                serializedInteractions = new InteractionSettingsType();
            }

            // Deserialize the interactions
            bool usable = Usable;
            bool grabbable = Grabbable;
            SchemaUtil.DeserializeInteractions(serializedInteractions, ref usable, ref grabbable);
            Usable = usable;
            Grabbable = grabbable;

            // Mark as complete
            interactionsDeserializationState.complete = true;

            yield return null;
        }

        /// <summary>
        /// Asynchronously deserializes the supplied serialized telemetry settings and updates the supplied
        /// state with the result.
        /// </summary>
        /// <param name="serializedTelemetrySettings">The serialized <code>TelemetrySettingsType</code> to deserialize</param>
        /// <param name="deserializationState">The <code>SerializationState</code> to populate
        ///     with the result.</param>
        /// 
        /// <see cref="TelemetrySettingsType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected virtual IEnumerator DeserializeTelemetrySettings(TelemetrySettingsType serializedTelemetrySettings, SerializationState deserializationState)
        {
            // Telemetry settings are optional, but we still need to deserialize the default settings
            if (serializedTelemetrySettings is null)
            {
                // Create the defaults
                serializedTelemetrySettings = new TelemetrySettingsType();
            }

            // Remove any existing data points
            foreach (string dataPoint in DataPoints)
            {
                RemoveDataPoint(dataPoint);
            }

            // Deserialize the new telemetry settings

            // Data point from telemetry key
            if (serializedTelemetrySettings.TelemetryKey != null)
            {
                foreach(string telemetryKey in serializedTelemetrySettings.TelemetryKey)
                {
                    // Create the data point from the telemetry key
                    string dataPointKey = DATA_POINT_KEY_PREFIX + telemetryKey.Trim().ToUpper().Replace('/', '.');
                    AddDataPoint(dataPointKey);
                }
            }

            // Limit violations
            ShadeForLimitViolations = serializedTelemetrySettings.ShadeForLimitViolations;

            // Mark as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="SceneObject{T}.Deserialize(T, SerializationState)"/>
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
            serializedInteractableSceneObject = serialized;

            // Perform the highlight material deserialization
            Material highlightMaterial = null;
            if (serializedInteractableSceneObject.HighlightMaterial != null)
            {
                ObjectSerializationState<Material> materialDeserializationState = new ObjectSerializationState<Material>();
                StartCoroutine(DeserializeMaterial(serializedInteractableSceneObject.HighlightMaterial, materialDeserializationState));

                // Wait for the coroutine to complete
                while (!materialDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // If the material deserialized, assign the highlight material
                if (!materialDeserializationState.IsError)
                {
                    highlightMaterial = materialDeserializationState.obj;
                }
                else
                {
                    LogWarning(materialDeserializationState.ErrorMessage + "; Defaulting to standard highlight material");
                }
            }

            // Assign the highlight material
            HighlightMaterial = (highlightMaterial != null) ? highlightMaterial : MRET.HighlightMaterial;

            // Perform the selection material deserialization
            Material selectionMaterial = null;
            if (serializedInteractableSceneObject.SelectionMaterial != null)
            {
                ObjectSerializationState<Material> materialDeserializationState = new ObjectSerializationState<Material>();
                StartCoroutine(DeserializeMaterial(serializedInteractableSceneObject.SelectionMaterial, materialDeserializationState));

                // Wait for the coroutine to complete
                while (!materialDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // If the material deserialized, assign the selection material
                if (!materialDeserializationState.IsError)
                {
                    selectionMaterial = materialDeserializationState.obj;
                }
                else
                {
                    LogWarning(materialDeserializationState.ErrorMessage + "; Defaulting to standard selection material");
                }
            }

            // Assign the selection material
            SelectionMaterial = (selectionMaterial != null) ? selectionMaterial : MRET.SelectMaterial;

            // Interaction settings deserialization (optional, but still deserialize defaults)
            {
                // Use the supplied serialized structure or a default
                InteractionSettingsType serializedInteractions = serializedInteractableSceneObject.Interactions ?? new InteractionSettingsType();

                // Deserialize the interactions
                SerializationState interactionsDeserializationState = new SerializationState();
                StartCoroutine(DeserializeInteractions(serializedInteractions, interactionsDeserializationState));

                // Wait for the coroutine to complete
                while (!interactionsDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(interactionsDeserializationState);

                // If the user deserialization failed, there's no point in continuing
                if (deserializationState.IsError) yield break;

                // Clear the state
                deserializationState.Clear();
            }

            // Telemetry settings deserialization (optional, but still deserialize defaults)
            {
                // Use the supplied serialized structure or a default
                TelemetrySettingsType serializedTelemetrySettings = serializedInteractableSceneObject.Telemetry ?? new TelemetrySettingsType();

                // Deserialize the telemetry settings
                SerializationState telemetrySettingsDeserializationState = new SerializationState();
                StartCoroutine(DeserializeTelemetrySettings(serializedTelemetrySettings, telemetrySettingsDeserializationState));

                // Wait for the coroutine to complete
                while (!telemetrySettingsDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(telemetrySettingsDeserializationState);

                // If the user deserialization failed, there's no point in continuing
                if (deserializationState.IsError) yield break;

                // Clear the state
                deserializationState.Clear();
            }

            // Visual settings
            Visible = serializedInteractableSceneObject.Visible;
            Opacity = serializedInteractableSceneObject.Opacity;

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <summary>
        /// Asynchronously serializes the interaction settings into the supplied serialized interaction settings
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
        protected virtual IEnumerator SerializeInteractions(InteractionSettingsType serializedInteractions, SerializationState interactionsSerializationState)
        {
            // Interaction settings are optional
            if (serializedInteractions != null)
            {
                // Serialize the interactions
                SchemaUtil.SerializeInteractions(serializedInteractions, Usable, Grabbable);
            }

            // Mark as complete
            interactionsSerializationState.complete = true;

            yield return null;
        }

        /// <summary>
        /// Asynchronously serializes the telemetry settings into the supplied serialized telemetry settings
        /// and updates the supplied state with the serialization state.
        /// </summary>
        /// <param name="serializedTelemetrySettings">The serialized <code>TelemetrySettingsType</code> to
        ///     populate with the telemetry settings</param>
        /// <param name="interactionsSerializationState">The <code>SerializationState</code> to populate with
        ///     the serialization state.</param>
        /// 
        /// <see cref="TelemetrySettingsType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected virtual IEnumerator SerializeTelemetrySettings(TelemetrySettingsType serializedTelemetrySettings, SerializationState serializationState)
        {
            // Interaction settings are optional
            if (serializedTelemetrySettings != null)
            {
                // Telemetry keys (optional)
                serializedTelemetrySettings.TelemetryKey = null;
                List<string> telemetryKeys = new List<string>();
                if (DataPoints.Length > 0)
                {
                    foreach (string dataPoint in DataPoints)
                    {
                        // Create the telemetry key from the data point
                        string telemetryKey = dataPoint.Replace(DATA_POINT_KEY_PREFIX, "").Replace('.', '/');
                        telemetryKeys.Add(telemetryKey);
                    }
                    serializedTelemetrySettings.TelemetryKey = telemetryKeys.ToArray();
                }

                // Violation shading
                serializedTelemetrySettings.ShadeForLimitViolations = ShadeForLimitViolations;
            }

            // Mark as complete
            serializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="SceneObject{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(T serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Start with our internal serialized material to serialize out the material
            // using the original deserialized structure (if was provided during deserialization)
            MaterialType serializedHighlightMaterial = null;
            if (serializedInteractableSceneObject != null)
            {
                // Use this material structure
                serializedHighlightMaterial = serializedInteractableSceneObject.HighlightMaterial;

                // Only serialize if we have a valid serialized material reference
                if (serializedHighlightMaterial != null)
                {
                    // TODO: Serialize the material? Not sure what this means since we really just want
                    // to retain the structure defined in the original serialized form. We don't want to
                    // generate an entirely new serialized material without context to how it was defined.
                    //SchemaUtil.SerializeMaterial(ref serializedHighlightMaterial);
                }
            }
            serialized.HighlightMaterial = (HighlightMaterial != MRET.HighlightMaterial) ? serializedHighlightMaterial : null;

            // Start with our internal serialized material to serialize out the material
            // using the original deserialized structure (if was provided during deserialization)
            MaterialType serializedSelectionMaterial = null;
            if (serializedInteractableSceneObject != null)
            {
                // Use this material structure
                serializedSelectionMaterial = serializedInteractableSceneObject.SelectionMaterial;

                // Only serialize if we have a valid serialized material reference
                if (serializedSelectionMaterial != null)
                {
                    // TODO: Serialize the material? Not sure what this means since we really just want
                    // to retain the structure defined in the original serialized form. We don't want to
                    // generate an entirely new serialized material without context to how it was defined.
                    //SchemaUtil.SerializeMaterial(ref serializedSelectionMaterial);
                }
            }
            serialized.SelectionMaterial = (SelectionMaterial != MRET.SelectMaterial) ? serializedSelectionMaterial : null;

            // Serialize the interaction settings
            {
                // Serialize out the interactions
                InteractionSettingsType serializedInteractions = new InteractionSettingsType();

                // Serialize out the interactions
                SerializationState interactionsSerializationState = new SerializationState();
                StartCoroutine(SerializeInteractions(serializedInteractions, interactionsSerializationState));

                // Wait for the coroutine to complete
                while (!interactionsSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the serialization state
                serializationState.Update(interactionsSerializationState);

                // If the serialization failed, there's no point in continuing. Something is wrong
                if (serializationState.IsError) yield break;

                // Clear the state
                serializationState.Clear();

                // Store the interaction settings in the result if any of the settings are other than default
                if ((serializedInteractions.EnableInteraction != InteractableDefaults.INTERACTABLE) ||
                    (serializedInteractions.EnableUsability != InteractableDefaults.USABLE))
                {
                    serialized.Interactions = serializedInteractions;
                }
            }

            // Serialize the telemetry settings
            {
                // Serialize out the interactions
                TelemetrySettingsType serializedTelemetrySettings = new TelemetrySettingsType();

                // Serialize out the interactions
                SerializationState telemetrySerializationState = new SerializationState();
                StartCoroutine(SerializeTelemetrySettings(serializedTelemetrySettings, telemetrySerializationState));

                // Wait for the coroutine to complete
                while (!telemetrySerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the serialization state
                serializationState.Update(telemetrySerializationState);

                // If the serialization failed, there's no point in continuing. Something is wrong
                if (serializationState.IsError) yield break;

                // Clear the state
                serializationState.Clear();

                // Store the telemetry settings in the result if any of the settings are other than default
                if ((serializedTelemetrySettings.ShadeForLimitViolations != InteractableDefaults.SHADE_FOR_LIMIT_VIOLATIONS) ||
                    ((serializedTelemetrySettings.TelemetryKey != null) && (serializedTelemetrySettings.TelemetryKey.Length > 0)))
                {
                    serialized.Telemetry = serializedTelemetrySettings;
                }
            }

            // Visual settings
            serialized.Visible = Visible;
            serialized.Opacity = Opacity != InteractableDefaults.OPACITY ? Opacity : InteractableDefaults.OPACITY;

            // Save the final serialized reference
            serializedInteractableSceneObject = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

        /// <seealso cref="Identifiable{T}.CreateSynchronizedObject"/>
        protected override ISynchronized CreateSynchronizedObject()
        {
            return gameObject.AddComponent<SynchronizedInteractable>();
        }

        /// <summary>
        /// Finds the InteractableSceneObject that the provided GameObject is part of.
        /// </summary>
        /// <param name="go">GameObject to perform the search for.</param>
        /// <returns>
        /// If InteractableSceneObject found, the InteractableSceneObject that the GameObject is
        /// part of. Otherwise, null.
        /// </returns>
        public static IInteractable GetInteractableSceneObjectForGameObject(GameObject go)
        {
            // Start with the current object and search each of the parents. When a SceneObject
            // is found on the parent, return that SceneObject. If no parent is found, return null.
            Transform currentTransform = go.transform;
            while (currentTransform != null)
            {
                IInteractable foundSceneObject = currentTransform.GetComponent<IInteractable>();
                if (foundSceneObject != null)
                {
                    return foundSceneObject;
                }
                currentTransform = currentTransform.parent;
            }
            return null;
        }

        /// <summary>
        /// Gets all the GameObjects that are part of this SceneObject.
        /// </summary>
        /// <param name="includeChildSceneObjects"></param>
        /// <returns>Array of GameObjects that are part of this SceneObject.</returns>
        protected virtual GameObject[] GetAllGameObjects(bool includeChildSceneObjects)
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
                else
                {
                    IInteractable interactable = GetInteractableSceneObjectForGameObject(t.gameObject);
                    if ((interactable as UnityEngine.Object) == this)
                    {
                        rtnObjects.Add(t.gameObject);
                    }
                }
            }
            return rtnObjects.ToArray();
        }

        /// <summary>
        /// Gets all the Renderers that are part of this SceneObject.
        /// </summary>
        /// <param name="includeChildSceneObjects"></param>
        /// <returns>Array of Renderers that are part of this SceneObject.</returns>
        protected virtual Renderer[] GetAllRenderers(bool includeChildSceneObjects)
        {
            List<Renderer> rtnObjects = new List<Renderer>();

            // Go through each child.
            foreach (Renderer m in GetComponentsInChildren<Renderer>())
            {
                // If child SceneObject are included, automatically add.
                if (includeChildSceneObjects)
                {
                    rtnObjects.Add(m);
                }
                // If not, check if it belongs to this InteractableSceneObject.
                else
                {
                    IInteractable interactable = GetInteractableSceneObjectForGameObject(m.gameObject);
                    if ((interactable as UnityEngine.Object) == this)
                    {
                        rtnObjects.Add(m);
                    }
                }
            }
            return rtnObjects.ToArray();
        }

        #region IInteractable
        /// <seealso cref="IInteractable.Visible"/>
        public virtual bool Visible
        {
            get
            {
                bool result = false;

                // If any renderer is enabled we are visible
                foreach (Renderer rend in GetComponentsInChildren<Renderer>())
                {
                    result |= rend.enabled;
                }
                return result;
            }
            set
            {
                RendererUtil.Show(gameObject, value);
            }
        }

        /// <seealso cref="IInteractable.Opacity"/>
        public virtual byte Opacity
        {
            get
            {
                return _opacity;
            }
            set
            {
                // Save the opacity
                _opacity = value;

                // Apply the opacity
                RendererUtil.ApplyOpacity(gameObject, _opacity);
            }
        }
        private byte _opacity = 255;

        #region Material Adjustment
        /// <seealso cref="IInteractable.HighlightMaterial"/>
        public Material HighlightMaterial { get; set; }

        /// <seealso cref="IInteractable.SelectionMaterial"/>
        public Material SelectionMaterial { get; set; }

        // Information about saved materials (for material swapping).
        protected Tuple<Renderer, Material[]>[] savedMaterialInfo;

        protected virtual void SaveObjectMaterials(bool includeChildInteractables = false)
        {
            List<Tuple<Renderer, Material[]>> rendMatList = new List<Tuple<Renderer, Material[]>>();
            foreach (Renderer rend in GetAllRenderers(includeChildInteractables))
            {
                List<Material> mats = new List<Material>();
                foreach (Material mat in rend.materials)
                {
                    mats.Add(mat);
                }
                rendMatList.Add(new Tuple<Renderer, Material[]>(rend, mats.ToArray()));
            }
            savedMaterialInfo = rendMatList.ToArray();
        }

        protected virtual void RestoreObjectMaterials()
        {
            if (savedMaterialInfo == null)
            {
                return;
            }

            foreach (Tuple<Renderer, Material[]> rendMatInfo in savedMaterialInfo)
            {
                // FIXME: Checking for null here may be masking another issue, but in fact during
                // line drawing editing this can become null for some reason.
                if (rendMatInfo.Item1 != null)
                {
                    rendMatInfo.Item1.materials = rendMatInfo.Item2;
                }
                else
                {
                    LogWarning("Unexpected state: Renderer for \'" + name + "\' was null while " +
                        "attempting to restore the object materials.", nameof(RestoreObjectMaterials));
                }
            }

            savedMaterialInfo = null;
        }

        protected virtual void ReplaceObjectMaterials(Material matToUse, bool includeChildInteractables = false)
        {
            foreach (Renderer rend in GetAllRenderers(includeChildInteractables))
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
        #endregion Material Adjustment

        #region Using
        /// <seealso cref="IInteractable.Usable"/>
        public bool Usable
        {
            get => usable;
            set => usable = value;
        }

        [SerializeField]
        [Tooltip("Whether or not the interactable is usable.")]
        private bool usable = InteractableDefaults.USABLE;

        /// <summary>
        /// Called when the interactable is 'clicked'
        /// </summary>
        /// <param name="hand">The <code>InputHand</code> that triggered the click</param>
        protected virtual void DoClick(InputHand hand)
        {
        }

        /// <summary>
        /// Called when the interactable is 'double-clicked'
        /// </summary>
        /// <param name="hand">The <code>InputHand</code> that triggered the double-click</param>
        protected virtual void DoDoubleClick(InputHand hand)
        {
        }

        // FIXME: Rename these methods to something better? MRET uses this method to popup an object configuration panel.
        /// <seealso cref="IInteractable.Use(InputHand)"/>
        public virtual void Use(InputHand hand)
        {
            if (!IsPlacing)
            {
                if (clicked == true)
                {
                    // Already clicked, so register as double-click
                    DoDoubleClick(hand);
                }
                else
                {
                    clicked = true;

                    // Register as a click
                    DoClick(hand);
                }
            }
        }

        /// <summary>
        /// Unuse performed on the provided hand.
        /// </summary>
        /// <param name="hand">Hand that performed the unuse.</param>
        public virtual void Unuse(InputHand hand)
        {
        }
        #endregion Using

        #region Touching
        /// <seealso cref="IInteractable.IsTouching"/>
        public bool IsTouching { get; private set; }

        /// <seealso cref="IInteractable.TouchBehavior"/>
        public IInteractable.TouchBehaviors TouchBehavior
        {
            get => touchBehavior;
            set => touchBehavior = value;
        }

        [SerializeField]
        [Tooltip("Behavior to use on touch.")]
        private IInteractable.TouchBehaviors touchBehavior = IInteractable.TouchBehaviors.Highlight;

        protected void OnTriggerEnter(Collider other)
        {
            InputHand inputHand = other.GetComponent<InputHand>();
            if (inputHand != null)
            {
                BeginTouch(inputHand);
            }
        }

        protected void OnTriggerExit(Collider other)
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
        /// Begin to touch hold the provided hand.
        /// </summary>
        protected virtual void BeginTouchHold(InputHand hand)
        {

        }

        /// <summary>
        /// End touch hold on the provided hand.
        /// </summary>
        protected virtual void EndTouchHold(InputHand hand)
        {

        }

        /// <summary>
        /// Provided for subclasses to provide behavior before this class begins
        /// the begin touch operation
        /// </summary>
        /// <param name="hand"></param>
        protected virtual void BeforeBeginTouch(InputHand hand)
        {

        }

        /// <summary>
        /// Provided for subclasses to provide behavior after this class begins
        /// the begin touch operation
        /// </summary>
        /// <param name="hand"></param>
        protected virtual void AfterBeginTouch(InputHand hand)
        {

        }

        /// <summary>
        /// Provided for subclasses to provide behavior before this class begins
        /// the end touch operation
        /// </summary>
        /// <param name="hand"></param>
        protected virtual void BeforeEndTouch(InputHand hand)
        {

        }

        /// <summary>
        /// Provided for subclasses to provide behavior after this class begins
        /// the end touch operation
        /// </summary>
        /// <param name="hand"></param>
        protected virtual void AfterEndTouch(InputHand hand)
        {

        }

        /// <seealso cref="IInteractable.BeginTouch(InputHand)"/>
        public void BeginTouch(InputHand hand)
        {
            if (!Usable || selected || IsTouching)
            {
                return;
            }

            // Allow subclasses the opportunity to add behavior
            BeforeBeginTouch(hand);

            // Determine the IsTouching behavior we need to perform
            switch (TouchBehavior)
            {
                case IInteractable.TouchBehaviors.Hold:
                    lastTime = DateTime.UtcNow;
                    holdCount = 0; // Reset the hold timer
                    goto case IInteractable.TouchBehaviors.Highlight;

                case IInteractable.TouchBehaviors.Highlight:
                    // Alter this object's materials.
                    if (savedMaterialInfo != null)
                    {
                        RestoreObjectMaterials();
                    }
                    SaveObjectMaterials(true);
                    ReplaceObjectMaterials(HighlightMaterial, true);

                    // Save the IsTouching hand.
                    touchingHand = hand;
                    break;

                case IInteractable.TouchBehaviors.Custom:
                default:
                    LogWarning("Touch behavior not implemented", nameof(BeginTouch));
                    break;
            }

            // Mark as touching
            IsTouching = (touchingHand != null);
            if (IsTouching)
            {
                // Allow subclasses the opportunity to add behavior
                AfterBeginTouch(hand);
            }
        }

        /// <seealso cref="IInteractable.EndTouch()"/>
        public void EndTouch()
        {
            if (!IsTouching)
            {
                return;
            }

            // Allow subclasses the opportunity to add behavior
            BeforeEndTouch(touchingHand);

            // Determine which IsTouching behavior we were performing
            switch (TouchBehavior)
            {
                case IInteractable.TouchBehaviors.Hold:
                    EndTouchHold(touchingHand);
                    touchHolding = false;
                    goto case IInteractable.TouchBehaviors.Highlight;

                case IInteractable.TouchBehaviors.Highlight:
                    // Restore this object's materials.
                    RestoreObjectMaterials();
                    break;

                case IInteractable.TouchBehaviors.Custom:
                default:
                    LogWarning("Touch behavior not implemented", nameof(EndTouch));
                    break;
            }

            // Turn off IsTouching
            IsTouching = false;

            // Allow subclasses the opportunity to add behavior
            AfterEndTouch(touchingHand);
            touchingHand = null;
        }
        #endregion Touching

        #region Grabbing
        /// <seealso cref="IInteractable.IsGrabbing"/>
        public bool IsGrabbing { get; private set; }

        /// <seealso cref="IInteractable.Grabbable"/>
        public bool Grabbable
        {
            get => grabbable;
            set => grabbable = value;
        }

        [SerializeField]
        [Tooltip("Whether or not the interactable is grabbable.")]
        private bool grabbable = InteractableDefaults.INTERACTABLE;

        /// <seealso cref="IInteractable.GrabBehavior"/>
        public IInteractable.GrabBehaviors GrabBehavior
        {
            get => grabBehavior;
            set => grabBehavior = value;
        }

        [SerializeField]
        [Tooltip("Behavior to use on grab.")]
        private IInteractable.GrabBehaviors grabBehavior = IInteractable.GrabBehaviors.Attach;

        /// <summary>
        /// Provided for subclasses to provide behavior before this class begins
        /// the begin grab operation
        /// </summary>
        /// <param name="hand"></param>
        protected virtual void BeforeBeginGrab(InputHand hand)
        {

        }

        /// <summary>
        /// Provided for subclasses to provide behavior after this class begins
        /// the begin grab operation
        /// </summary>
        /// <param name="hand"></param>
        protected virtual void AfterBeginGrab(InputHand hand)
        {

        }

        /// <summary>
        /// Provided for subclasses to provide behavior before this class begins
        /// the end grab operation
        /// </summary>
        /// <param name="hand"></param>
        protected virtual void BeforeEndGrab(InputHand hand)
        {

        }

        /// <summary>
        /// Provided for subclasses to provide behavior after this class begins
        /// the end grab operation
        /// </summary>
        /// <param name="hand"></param>
        protected virtual void AfterEndGrab(InputHand hand)
        {

        }

        /// <seealso cref="IInteractable.BeginGrab(InputHand)"/>
        public void BeginGrab(InputHand hand)
        {
            if (!Usable || !Grabbable || IsGrabbing)
            {
                return;
            }

            // Allow subclasses the opportunity to add behavior
            BeforeBeginGrab(hand);

            // Turn on grabbing
            IsGrabbing = true;

            switch (GrabBehavior)
            {
                case IInteractable.GrabBehaviors.Attach:
                    AttachTo(hand.transform);
                    break;

                case IInteractable.GrabBehaviors.Custom:
                default:
                    LogWarning("Grab behavior not implemented", nameof(BeginGrab));
                    break;
            }

            // Allow subclasses the opportunity to add behavior
            AfterBeginGrab(hand);

            // FIXME: All locomotion is paused by the handInteractor that begins the grab. This shouldn't be necesarry anymore.
            // Disable scaling
            DisableAllEnvironmentScaling();
        }

        /// <seealso cref="IInteractable.EndGrab(InputHand)"/>
        public void EndGrab(InputHand hand)
        {
            if (!IsGrabbing)
            {
                return;
            }

            // Allow subclasses the opportunity to add behavior
            BeforeEndGrab(hand);

            switch (GrabBehavior)
            {
                case IInteractable.GrabBehaviors.Attach:
                    Detach();
                    break;

                case IInteractable.GrabBehaviors.Custom:
                default:
                    LogWarning("Grab behavior not implemented", nameof(EndGrab));
                    break;
            }

            // Determine if the transform actually changed
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;
            Vector3 scale = transform.localScale;
            if (!lastSavedPosition.Equals(position) ||
                !lastSavedRotation.Equals(rotation) ||
                !lastSavedScale.Equals(scale))
            {
                // Record the transform action
                ProjectManager.UndoManager.AddAction(
                    new SceneObjectTransformAction(this, position, rotation, scale),
                    new SceneObjectTransformAction(this, lastSavedPosition, lastSavedRotation, lastSavedScale));

                // Record the new transform
                lastSavedPosition = transform.position;
                lastSavedRotation = transform.rotation;
                lastSavedScale = transform.localScale;
            }

            // Turn off grabbing
            IsGrabbing = false;

            // Allow subclasses the opportunity to add behavior
            AfterEndGrab(hand);

            // FIXME: All locomotion is resumed by the handInteractor that begins the grab. This shouldn't be necesarry anymore.
            // Enable scaling
            EnableAnyEnvironmentScaling();
        }
        #endregion Grabbing

        #region Placement
        /// <seealso cref="IInteractable.IsPlacing"/>
        public bool IsPlacing { get; private set; }

        /// <summary>
        /// Provided for subclasses to provide behavior before this class begins
        /// the begin placing operation
        /// </summary>
        /// <param name="placingParent">The <code>GameObject</code> parent for this placing interactable</param>
        protected virtual void BeforeBeginPlacing(GameObject placingParent = null)
        {

        }

        /// <summary>
        /// Provided for subclasses to provide behavior after this class begins
        /// the begin placing operation
        /// </summary>
        /// <param name="placingParent">The <code>GameObject</code> parent for this placing interactable</param>
        protected virtual void AfterBeginPlacing(GameObject placingParent = null)
        {

        }

        /// <summary>
        /// Provided for subclasses to provide behavior before this class begins
        /// the end placing operation
        /// </summary>
        /// <param name="hand"></param>
        protected virtual void BeforeEndPlacing()
        {

        }

        /// <summary>
        /// Provided for subclasses to provide behavior after this class begins
        /// the end placing operation
        /// </summary>
        protected virtual void AfterEndPlacing()
        {

        }

        private Transform oldParent = null;
        private bool wasGrabbable = true, wasUsable = true;

        /// <seealso cref="IInteractable.BeginPlacing(GameObject)"/>
        public void BeginPlacing(GameObject placingParent = null)
        {
            if (IsPlacing)
            {
                return;
            }

            // Allow subclasses the opportunity to add behavior
            BeforeBeginPlacing(placingParent);

            IsPlacing = true;

            foreach (IInteractable interactable in gameObject.GetComponentsInChildren<IInteractable>())
            {
                if ((UnityEngine.Object)interactable != this)
                {
                    interactable.BeginPlacing();
                }
            }

            oldParent = transform.parent;
            if (placingParent != null)
            {
                transform.SetParent(placingParent.transform);
                transform.localPosition = Vector3.zero;
            }

            // Change these for grabbing purposes.
            wasGrabbable = Grabbable;
            Grabbable = false;
            wasUsable = Usable;
            Usable = true;

            // Allow subclasses the opportunity to add behavior
            AfterBeginPlacing(placingParent);
        }

        /// <seealso cref="IInteractable.EndPlacing"/>
        public void EndPlacing()
        {
            if (!IsPlacing)
            {
                return;
            }

            // Allow subclasses the opportunity to add behavior
            BeforeEndPlacing();

            IsPlacing = false;

            foreach (IInteractable interactable in gameObject.GetComponentsInChildren<IInteractable>())
            {
                if ((UnityEngine.Object)interactable != this)
                {
                    interactable.EndPlacing();
                }
            }

            if (oldParent != null)
            {
                transform.SetParent(oldParent);
            }

            // Change these back.
            Grabbable = wasGrabbable;
            Usable = wasUsable;

            // If this is the root object, add an undo action.
            var serializedInteractable = CreateSerializedType();
            Serialize(serializedInteractable);
            ProjectManager.UndoManager.AddAction(
                new AddSceneObjectAction(serializedInteractable),
                new DeleteIdentifiableObjectAction(serializedInteractable));

            lastSavedPosition = transform.position;
            lastSavedRotation = transform.rotation;
            lastSavedScale = transform.localScale;

            // Allow subclasses the opportunity to add behavior
            AfterEndPlacing();
        }
        #endregion Placement

        #region Selection
        protected IInteractable GetInteractableSceneObjectRoot(IInteractable interactableSceneObject)
        {
            IInteractable result = interactableSceneObject;

            IInteractable[] newInteractableSceneObject;
            while ((newInteractableSceneObject = result.gameObject.transform.GetComponentsInParent<IInteractable>(true)) != null &&
                newInteractableSceneObject[0] != result)
            {
                result = newInteractableSceneObject[0];
            }

            return result;
        }

        protected virtual bool IsSceneObjectHierarchySelectable(ISelectable sceneObject)
        {
            return true;
        }

        protected virtual void SelectHierarchy()
        {
            ISelectable[] sels = GetComponentsInParent<ISelectable>();
            if ((sels.Length == 0 || (sels.Length == 1 && sels[0] == (ISelectable)this)))
            {
                foreach (ISelectable selChild in GetInteractableSceneObjectRoot(this).gameObject.GetComponentsInChildren<ISelectable>(true))
                {
                    // Allow decendants to control hierarchical selection for this child
                    selChild.Select(IsSceneObjectHierarchySelectable(selChild));
                }
            }
        }

        protected virtual void DeselectHierarchy()
        {
            ISelectable[] sels = GetComponentsInParent<ISelectable>();
            if ((sels.Length == 0 || (sels.Length == 1 && sels[0] == (ISelectable)this)))
            {
                foreach (ISelectable selChild in GetInteractableSceneObjectRoot(this).gameObject.GetComponentsInChildren<ISelectable>(true))
                {
                    // Allow decendants to control hierarchical selection for this child
                    selChild.Deselect(IsSceneObjectHierarchySelectable(selChild));
                }
            }
        }

        /// <summary>
        /// Provided for subclasses to provide behavior before this class begins
        /// the select operation
        /// </summary>
        /// <param name="hierarchical">Indicated whether the hierarchy should be selected</param>
        protected virtual void BeforeSelect(bool hierarchical = true)
        {

        }

        /// <summary>
        /// Provided for subclasses to provide behavior after this class begins
        /// the select operation
        /// </summary>
        /// <param name="hierarchical">Indicated whether the hierarchy should be selected</param>
        protected virtual void AfterSelect(bool hierarchical = true)
        {

        }

        /// <seealso cref="IInteractable.Select(bool)"/>
        public void Select(bool hierarchical = true)
        {
            if (!Usable || IsTouching || selected)
            {
                return;
            }

            // Allow subclasses the opportunity to add behavior
            BeforeSelect(hierarchical);

            // Mark as selected
            selected = true;

            // Select the entire hierachy
            if (hierarchical)
            {
                SelectHierarchy();
            }

            // Alter this object's materials
            if (savedMaterialInfo != null)
            {
                RestoreObjectMaterials();
            }
            SaveObjectMaterials(true);
            ReplaceObjectMaterials(SelectionMaterial, true);

            // Allow subclasses the opportunity to add behavior
            AfterSelect(hierarchical);
        }

        /// <summary>
        /// Provided for subclasses to provide behavior before this class begins
        /// the deselect operation
        /// </summary>
        /// <param name="hierarchical">Indicated whether the hierarchy should be selected</param>
        protected virtual void BeforeDeselect(bool hierarchical = true)
        {

        }

        /// <summary>
        /// Provided for subclasses to provide behavior after this class begins
        /// the deselect operation
        /// </summary>
        /// <param name="hierarchical">Indicated whether the hierarchy should be selected</param>
        protected virtual void AfterDeselect(bool hierarchical = true)
        {

        }

        /// <seealso cref="IInteractable.Deselect(bool)"/>
        public void Deselect(bool hierarchical = true)
        {
            if (!selected)
            {
                return;
            }

            // Allow subclasses the opportunity to add behavior
            BeforeDeselect(hierarchical);

            // Select the entire hierachy
            if (hierarchical)
            {
                DeselectHierarchy();
            }

            // Restore this object's materials.
            RestoreObjectMaterials();

            // Turn off selection
            selected = false;

            // Allow subclasses the opportunity to add behavior
            AfterDeselect(hierarchical);
        }
        #endregion Selection

        /// <seealso cref="IInteractable.CreateSerializedType"/>
        InteractableSceneObjectType IInteractable.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="IInteractable.Synchronize(InteractableSceneObjectType, Action{bool, string})"/>
        void IInteractable.Synchronize(InteractableSceneObjectType serialized, Action<bool, string> onFinished)
        {
            Synchronize(serialized as T, onFinished);
        }

        /// <seealso cref="IInteractable.Deserialize(InteractableSceneObjectType, Action{bool, string})"/>
        void IInteractable.Deserialize(InteractableSceneObjectType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="IInteractable.Serialize(InteractableSceneObjectType, Action{bool, string})"/>
        void IInteractable.Serialize(InteractableSceneObjectType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion IInteractable

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

        #region CONTEXTAWARECONTROL
        private bool previousScalingState = false;
        protected void DisableAllEnvironmentScaling()
        {
            foreach (InputHand hand in MRET.InputRig.hands)
            {
                ScaleObjectTransform sot = hand.GetComponentInChildren<ScaleObjectTransform>(true);
                if (sot)
                {
                    previousScalingState = sot.enabled;
                    sot.enabled = false;
                }
            }
        }

        protected void EnableAnyEnvironmentScaling()
        {
            foreach (InputHand hand in MRET.InputRig.hands)
            {
                ScaleObjectTransform sot = hand.GetComponentInChildren<ScaleObjectTransform>(true);
                if (sot)
                {
                    sot.enabled = previousScalingState;
                }
                previousScalingState = false;
            }
        }
        #endregion CONTEXTAWARECONTROL

        #region TELEMETRY
        public const string DATA_POINT_KEY_PREFIX = "GOV.NASA.GSFC.XR.MRET.IOT.PAYLOAD.";
        private enum PartShadingMode { MeshDefault, MeshLimits }
        private PartShadingMode shadingMode = PartShadingMode.MeshDefault;
        public bool ShadeForLimitViolations { get; set; }
        public string[] DataPoints { get => _dataPoints.ToArray(); }

        private List<string> _dataPoints = new List<string>();
        private DataManager.DataValueEvent dataPointChangeEvent;

        public bool AddDataPoint(string dataPoint)
        {
            if (_dataPoints.Contains(dataPoint))
            {
                LogWarning("Point " + dataPoint + " already exists on " + name + ".", nameof(AddDataPoint));
                return false;
            }

            // Add the data point
            _dataPoints.Add(dataPoint);
            MRET.DataManager.SubscribeToPoint(dataPoint, dataPointChangeEvent);
            Log("Registered for telemetry point: " + dataPoint);
            return true; //FIXME: at somepoint after subscribing and before the event triggers, the thresholds are cleared
        }

        public bool RemoveDataPoint(string dataPoint)
        {
            if (_dataPoints.Contains(dataPoint))
            {
                _dataPoints.Remove(dataPoint);
                MRET.DataManager.UnsubscribeFromPoint(dataPoint, dataPointChangeEvent);
                return true;
            }

            LogWarning("Point " + dataPoint + " does not exist on " + name + ".", nameof(RemoveDataPoint));
            return false;
        }

        //TODO:Fix limitstate handling for arbitrary colors, because it no longer has 'nominalstate'
        public void HandleDataPointChange(DataManager.DataValue dataValue)
        {
            DataManager.DataValue.LimitState limitState = dataValue.limitState;
            if (limitState == DataManager.DataValue.LimitState.Undefined)
            {
                HandleUndefinedLimitState();
            }
            else
            {
                HandleLimitViolation(limitState);
            }
        }

        private void HandleLimitViolation(DataManager.DataValue.LimitState limitState)
        {
            if (ShadeForLimitViolations)
            {
                if (shadingMode == PartShadingMode.MeshLimits)
                {
                    RevertMeshLimitShader();
                }
                shadingMode = PartShadingMode.MeshLimits;
                ApplyMeshLimitShader(limitState);
            }
        }

        private void HandleNominalLimitState()
        {
            if (shadingMode == PartShadingMode.MeshLimits)
            {
                shadingMode = PartShadingMode.MeshDefault;
                RevertMeshLimitShader();
            }
        }

        private void HandleUndefinedLimitState()
        {
            if (shadingMode == PartShadingMode.MeshLimits)
            {
                shadingMode = PartShadingMode.MeshDefault;
                RevertMeshLimitShader();
            }
        }

        private Dictionary<Renderer, Material[]> revertMaterials = null;
        private void ApplyMeshLimitShader(DataManager.DataValue.LimitState limitType)
        {
            if (revertMaterials != null)
            {
                LogError("Revert materials not empty. Will not apply mesh limit shader.", nameof(ApplyMeshLimitShader));
                return;
            }

            // RestoreObjectMaterials();
            SaveObjectMaterials();

            revertMaterials = new Dictionary<Renderer, Material[]>();
            foreach (Renderer rend in gameObject.GetComponentsInChildren<Renderer>())
            {
                revertMaterials.Add(rend, rend.materials);
                rend.materials[0].color = MRET.LimitMaterial.color;
                if (!rend.materials[0].name.Contains(MRET.LimitMaterial.name))
                {
                    rend.materials = new Material[] { MRET.LimitMaterial };
                }

                //TODO: revisit this switch statement with Unity Color if we can replace DataManager.DataValue.LimitState
                Color limitColor = Color.black;
                switch (limitType)
                {
                    case DataManager.DataValue.LimitState.Blue:
                        limitColor = Color.blue;
                        break;
                    case DataManager.DataValue.LimitState.Cyan:
                        limitColor = Color.cyan;
                        break;
                    case DataManager.DataValue.LimitState.DarkGray:
                        limitColor = new Color(0.25f, 0.25f, 0.25f);
                        break;
                    case DataManager.DataValue.LimitState.Gray:
                        limitColor = Color.gray;
                        break;
                    case DataManager.DataValue.LimitState.Green:
                        limitColor = Color.green;
                        break;
                    case DataManager.DataValue.LimitState.LightGray:
                        limitColor = new Color(0.75f, 0.75f, 0.75f);
                        break;
                    case DataManager.DataValue.LimitState.Magenta:
                        limitColor = Color.magenta;
                        break;
                    case DataManager.DataValue.LimitState.Orange:
                        limitColor = new Color(1f, 0.647058824f, 0f);
                        break;
                    case DataManager.DataValue.LimitState.Pink:
                        limitColor = new Color(1f, 0.752941176f, 0.796078431f);
                        break;
                    case DataManager.DataValue.LimitState.Red:
                        limitColor = Color.red;
                        break;
                    case DataManager.DataValue.LimitState.White:
                        limitColor = Color.white;
                        break;
                    case DataManager.DataValue.LimitState.Yellow:
                        limitColor = Color.yellow;
                        break;
                    case DataManager.DataValue.LimitState.Undefined:
                    case DataManager.DataValue.LimitState.Black:
                    default:
                        limitColor = Color.black;
                        break;
                }
                rend.materials[0].color = limitColor;
            }
        }

        private void RevertMeshLimitShader()
        {
            if (revertMaterials == null)
            {
                LogError("No revert materials. Will not revert materials.", nameof(RevertMeshLimitShader));
                return;
            }

            //RestoreObjectMaterials();
            //foreach (KeyValuePair<MeshRenderer, Material[]> rendMat in revertMaterials)
            //{
            //    rendMat.Key.materials = rendMat.Value;
            //}

            revertMaterials = null;
        }
        #endregion TELEMETRY
    }

    /// <summary>
    /// Provides an implementation for the abstract InteractableSceneObject class
    /// </summary>
    public class InteractableSceneObject : InteractableSceneObject<InteractableSceneObjectType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(InteractableSceneObject);
    }

    /// <summary>
    /// Used to keep the default values from the schema in sync
    /// </summary>
    public class InteractableDefaults : SceneObjectDefaults
    {
        public static readonly bool INTERACTABLE = new InteractionSettingsType().EnableInteraction;
        public static readonly bool USABLE = new InteractionSettingsType().EnableUsability;
        public static readonly bool VISIBLE = new InteractableSceneObjectType().Visible;
        public static readonly byte OPACITY = new InteractableSceneObjectType().Opacity;
        public static readonly bool SHADE_FOR_LIMIT_VIOLATIONS = new TelemetrySettingsType().ShadeForLimitViolations;
    }
}