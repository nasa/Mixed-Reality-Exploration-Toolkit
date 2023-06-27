// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Tools.Selection;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part.Alignment;
using GOV.NASA.GSFC.XR.MRET.Extensions.EasyBuildSystem.Alignment;
using GOV.NASA.GSFC.XR.MRET.Extensions.Ros;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Part
{
    public class InteractablePart : PhysicalSceneObject<PartType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(InteractablePart);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private PartType serializedPart;

        // ROS
        public RosConnector RosInterface { get => _rosInterface; }
        private RosConnector _rosInterface = null;

        /// <summary>
        /// Part specifications
        /// </summary>
        new public PartVendorSpecifications Specifications { get => specifications; }
        private PartVendorSpecifications specifications = null;

        /// <summary>
        /// Part category
        /// </summary>
        public PartCategoryType Category { get; set; }

        /// <summary>
        /// Part category
        /// </summary>
        public PartSubsystemType Subsystem { get; set; }

        /// <summary>
        /// The InteractablePartGroup of child parts.
        /// </summary>
        public IGroup ChildPartGroup { get => _childPartGroup; }
        private InteractablePartGroup _childPartGroup = null;

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

            // Default to using size when serializing the transform
            useSizeForDefaultOnSerialize = true;

            // Initialize the part specifications
            specifications = new PartVendorSpecifications();
            Category = InteractablePartDefaults.CATEGORY;
            Subsystem = InteractablePartDefaults.SUBSYSTEM;
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Override the default serialization for the scale transform
            useSizeForDefaultOnSerialize = true;
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            // Take the inherited behavior
            base.MRETOnDestroy();

            Destroy(_rosInterface);
            Destroy(_childPartGroup);
        }
        #endregion MRETUpdateBehaviour

        #region Serialization
        /// <seealso cref="PhysicalSceneObject{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(PartType serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Process the object specific configuration

            // Save the serialized reference
            serializedPart = serialized;

            // Part Specifications (optional, but still deserialize defaults)
            {
                // Use the supplied serialized structure or a default
                PartSpecificationsType serializedSpecs = serializedPart.PartSpecifications ?? new PartSpecificationsType();

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

            // Update the rigid body settings
            Mass = specifications.massMax;

            // Deserialize the configuration
            Category = serializedPart.Category;
            Subsystem = serializedPart.Subsystem;

            // Destroy the old ROS connector
            if (_rosInterface != null) Destroy(_rosInterface);

            // ROS Interface
            if (serializedPart.ROSInterface != null)
            {
                // Get the ROS connector reference
                _rosInterface = gameObject.GetComponent<RosConnector>();
                if (_rosInterface == null)
                {
                    // Create one
                    _rosInterface = gameObject.AddComponent<RosConnector>();
                }

                // Perform the ROS deserialization
                SerializationState rosDeserializationState = new SerializationState();
                StartCoroutine(_rosInterface.DeserializeWithLogging(serializedPart.ROSInterface, rosDeserializationState));

                // Wait for the coroutine to complete
                while (!rosDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(rosDeserializationState);

                // If the ROS deserialization failed, abort
                if (deserializationState.IsError) yield break;

                // Clear the state
                deserializationState.Clear();
            }

            // Destroy the old child part group
            if (_childPartGroup != null)
            {
                Destroy(_childPartGroup);
                _childPartGroup = null;
            }

            // Deserialize the child parts
            if (serializedPart.ChildParts != null)
            {
                // Perform the group deserialization
                VersionedSerializationState<InteractablePartGroup> groupDeserializationState = new VersionedSerializationState<InteractablePartGroup>();
                StartCoroutine(DeserializeVersioned(serializedPart.ChildParts, gameObject, gameObject.transform, groupDeserializationState));

                // Wait for the coroutine to complete
                while (!groupDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(groupDeserializationState);

                // Make sure the resultant child parts type is not null
                if (groupDeserializationState.versioned is null)
                {
                    deserializationState.Error("Deserialized child parts group cannot be null, denoting a possible internal issue.");
                }

                // If the group deserialization failed, there's no point in continuing. Something is wrong
                if (deserializationState.IsError) yield break;

                // Assign the group
                _childPartGroup = groupDeserializationState.versioned;

                // Clear the state
                deserializationState.Clear();
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="PhysicalSceneObject{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(PartType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Serialize the object specific configuration

            // Serialize the interactable part

            // Part specifications (required)
            {
                PartSpecificationsType serializedSpecs = new PartSpecificationsType();

                // Perform the serialization
                SerializationState specsSerializationState = new SerializationState();
                StartCoroutine(specifications.Serialize(serializedSpecs, specsSerializationState));

                // Wait for the coroutine to complete
                while (!specsSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the serialization state
                serializationState.Update(specsSerializationState);

                // If the serialization failed, exit with an error
                if (serializationState.IsError) yield break;

                // Clear the state
                serializationState.Clear();

                // Store the vendor specs in the result
                serialized.PartSpecifications = serializedSpecs;
            }

            // Serialize the part categorization
            serialized.Category = Category;
            serialized.Subsystem = Subsystem;

            // Serialize the ROS interface settings (optional)
            ROSInterfaceType serializedROSInterface = null;
            if (_rosInterface)
            {
                serializedROSInterface = _rosInterface.CreateSerializedType();

                // Perform the ROS interface serialization
                SerializationState rosSerializationState = new SerializationState();
                StartCoroutine(_rosInterface.SerializeWithLogging(serializedROSInterface, rosSerializationState));

                // Wait for the coroutine to complete
                while (!rosSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the serialization state
                serializationState.Update(rosSerializationState);

                // If the ROS serialization failed, exit with an error
                if (serializationState.IsError) yield break;

                // Clear the state
                serializationState.Clear();
            }
            serialized.ROSInterface = serializedROSInterface;

            // Serialize the child part group
            PartsType serializedParts = null;
            if (_childPartGroup != null)
            {
                serializedParts = _childPartGroup.CreateSerializedType();

                // Perform the group serialization
                SerializationState groupSerializationState = new SerializationState();
                StartCoroutine(_childPartGroup.SerializeWithLogging(serializedParts, groupSerializationState));

                // Wait for the coroutine to complete
                while (!groupSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the serialization state
                serializationState.Update(groupSerializationState);

                // If the group serialization failed, exit with an error
                if (serializationState.IsError) yield break;

                // Clear the state
                serializationState.Clear();
            }
            serialized.ChildParts = serializedParts;

            // Save the final serialized reference
            serializedPart = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serialization

        /// <summary>
        /// Creates an interactable part
        /// </summary>
        /// <param name="partName">The name of the part</param>
        /// <param name="partSpecifications">The optional <code>PartSpecifications</code></param>
        /// <returns>A <code>InteractablePart</code> instance</returns>
        public static InteractablePart Create(string partName, PartVendorSpecifications partSpecifications = null)
        {
            GameObject interactablePartGameObject = new GameObject(partName);
            InteractablePart interactablePart = interactablePartGameObject.AddComponent<InteractablePart>();
            if (partSpecifications != null)
            {
                interactablePart.specifications = partSpecifications;
            }
            return interactablePart;
        }

        /// <seealso cref="Identifiable{T}.CreateSynchronizedObject"/>
        protected override ISynchronized CreateSynchronizedObject()
        {
            return gameObject.AddComponent<SynchronizedPart>();
        }

        /// <seealso cref="PhysicalSceneObject{T}.AfterBeginGrab(InputHand)"/>
        protected override void AfterBeginGrab(InputHand hand)
        {
            base.AfterBeginGrab(hand);

            StartAligning(hand.gameObject);
        }

        /// <seealso cref="PhysicalSceneObject{T}.BeforeEndGrab(InputHand)"/>
        protected override void BeforeEndGrab(InputHand hand)
        {
            base.BeforeEndGrab(hand);

            StopAligning();

            SnapToConnector();
        }

        #region SNAPPING
        private SnappingConnector connectorToSnapTo = null;

        public void SetCurrentSnappingConnector(SnappingConnector snappingConnector)
        {
            connectorToSnapTo = snappingConnector;
        }

        public void UnsetSnappingConnector(SnappingConnector snappingConnector)
        {
            if (connectorToSnapTo == snappingConnector)
            {
                connectorToSnapTo = null;
            }
        }

        public void SnapToConnector()
        {
            if (connectorToSnapTo)
            {
                connectorToSnapTo.Snap();
            }
        }

        public void StartAligning(GameObject controller)
        {
            if (MRET.ConfigurationManager.config.PlacementMode == false)
            {
                return;
            }

#if MRET_EXTENSION_EASYBUILDSYSTEM
            VRBuilderBehaviour bb = controller.AddComponent<VRBuilderBehaviour>();
            bb.OutOfRangeDistance = 0.5f;
            //bb.PreviewLayer = MRET.previewLayer;

            EasyBuildSystem.Features.Scripts.Core.Base.Piece.PieceBehaviour pb = gameObject.AddComponent<EasyBuildSystem.Features.Scripts.Core.Base.Piece.PieceBehaviour>();

            pb.ChangeState(EasyBuildSystem.Features.Scripts.Core.Base.Piece.Enums.StateType.Preview);

            bb.ChangeMode(EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums.BuildMode.Placement);
            bb.placingObject = gameObject;

            foreach (Collider coll in GetComponentsInChildren<Collider>())
            {
                coll.enabled = false;
            }
#else
            LogWarning("Easy Build System is unavailable", nameof(StartAligning));
#endif
        }

        public void StopAligning()
        {
            if (MRET.ConfigurationManager.config.PlacementMode == false)
            {
                return;
            }

#if MRET_EXTENSION_EASYBUILDSYSTEM
            foreach (VRBuilderBehaviour bb in FindObjectsOfType<VRBuilderBehaviour>())
            {
                if (bb.placingObject)
                {
                    if (bb.CurrentPreview)
                    {
                        bb.placingObject.transform.position = bb.CurrentPreview.transform.position;
                        bb.placingObject.transform.rotation = bb.CurrentPreview.transform.rotation;
                        bb.ChangeMode(EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums.BuildMode.None);
                        Destroy(bb.CurrentPreview);
                    }
                    EasyBuildSystem.Features.Scripts.Core.Base.Piece.PieceBehaviour pb = bb.placingObject.GetComponent<EasyBuildSystem.Features.Scripts.Core.Base.Piece.PieceBehaviour>();
                    if (pb)
                    {
                        Destroy(pb);
                    }

                    bb.placingObject = null;
                    Destroy(bb);
                }
            }

            foreach (Collider coll in GetComponentsInChildren<Collider>())
            {
                coll.enabled = true;
                if (coll.gameObject.layer == MRET.previewLayer)
                {
                    coll.gameObject.layer = MRET.defaultLayer;
                }
            }
#else
            LogWarning("Easy Build System is unavailable", nameof(StopAligning));
#endif
        }
        #endregion

        #region PLACEMENT

        protected override void BeforeBeginPlacing(GameObject placingParent = null)
        {
            base.BeforeBeginPlacing(placingParent);

            StartAligning(placingParent);
        }

        protected override void BeforeEndPlacing()
        {
            StopAligning();
        }
#endregion

#region Selection
        protected override bool IsSceneObjectHierarchySelectable(ISelectable sceneObject)
        {
            // Disallow AssemblyGrabber hierarchy selection
            return !(sceneObject is AssemblyGrabber);
        }

#endregion
    }

    public class InteractablePartDefaults : PhysicalSceneObjectDefaults
    {
        // We want to use the default values from the schema to keep in sync
        public static readonly PartCategoryType CATEGORY = new PartType().Category;
        public static readonly PartSubsystemType SUBSYSTEM = new PartType().Subsystem;

        // Part Specifications
        new public static readonly string REFERENCE = new PartSpecificationsType().Reference1;
        new public static readonly string NOTES = new PartSpecificationsType().Notes1;
        public static readonly string VENDOR = "";
        public static readonly string VERSION = "";
        public static readonly float POWER_MIN = default;
        public static readonly float POWER_MAX = default;
        public static readonly float POWER_IDLE = default;
        public static readonly float POWER_AVERAGE = default;
        public static readonly float POWER_CONTINGENCY = default;
        public static readonly PowerUnitType POWER_MIN_UNITS = new PowerType().units;
        public static readonly PowerUnitType POWER_MAX_UNITS = new PowerType().units;
        public static readonly PowerUnitType POWER_IDLE_UNITS = new PowerType().units;
        public static readonly PowerUnitType POWER_AVERAGE_UNITS = new PowerType().units;
        public static readonly PowerUnitType POWER_CONTINGENCY_UNITS = new PowerType().units;
    }

    /// <summary>
    /// Defines part vendor specifications
    /// </summary>
    public class PartVendorSpecifications : PhysicalSpecifications
    {
        // Part information
        public string vendor = InteractablePartDefaults.VENDOR;
        public string version = InteractablePartDefaults.VERSION;

        // Power
        public float powerMin = InteractablePartDefaults.POWER_MIN;
        public float powerMax = InteractablePartDefaults.POWER_MAX;
        public float powerIdle = InteractablePartDefaults.POWER_IDLE;
        public float powerAverage = InteractablePartDefaults.POWER_AVERAGE;
        public float powerContingency = InteractablePartDefaults.POWER_CONTINGENCY;
        public PowerUnitType powerMinUnits = InteractablePartDefaults.POWER_MIN_UNITS;
        public PowerUnitType powerMaxUnits = InteractablePartDefaults.POWER_MAX_UNITS;
        public PowerUnitType powerIdleUnits = InteractablePartDefaults.POWER_IDLE_UNITS;
        public PowerUnitType powerAverageUnits = InteractablePartDefaults.POWER_AVERAGE_UNITS;
        public PowerUnitType powerContingencyUnits = InteractablePartDefaults.POWER_CONTINGENCY_UNITS;

        new public Uri reference = new Uri(InteractablePartDefaults.REFERENCE);
        new public string notes = InteractablePartDefaults.NOTES;

        /// <summary>
        /// Helper property to check if any power settings are different than the defaults
        /// </summary>
        public bool PowerIsDefaults =>
            Mathf.Approximately(powerMin, InteractablePartDefaults.POWER_MIN) &&
            (powerMinUnits == InteractablePartDefaults.POWER_MIN_UNITS) &&
            Mathf.Approximately(powerMax, InteractablePartDefaults.POWER_MAX) &&
            (powerMaxUnits == InteractablePartDefaults.POWER_MAX_UNITS) &&
            Mathf.Approximately(powerIdle, InteractablePartDefaults.POWER_IDLE) &&
            (powerIdleUnits == InteractablePartDefaults.POWER_IDLE_UNITS) &&
            Mathf.Approximately(powerAverage, InteractablePartDefaults.POWER_AVERAGE) &&
            (powerAverageUnits == InteractablePartDefaults.POWER_AVERAGE_UNITS) &&
            Mathf.Approximately(powerContingency, InteractablePartDefaults.POWER_CONTINGENCY) &&
            (powerContingencyUnits == InteractablePartDefaults.POWER_CONTINGENCY_UNITS);

        /// <summary>
        /// Performs deserialization of the supplied serialized power class instance into the
        /// resultant arguments.
        /// </summary>
        /// <param name="serialized">The serialized <code>PowerType</code> class instance to read</param>
        /// <param name="power">The resultant power converted to default Unity units</param>
        /// <param name="units">The original power units before conversion</param>
        public void DeserializePower(PowerType serialized, ref float power, ref PowerUnitType units)
        {
            power = default;
            PowerType serializedPower = serialized ?? new PowerType();
            SchemaUtil.DeserializePower(serializedPower, ref power);
            units = serializedPower.units;
        }

        /// <summary>
        /// Performs deserialization of the supplied serialized specifications into this class instance
        /// </summary>
        /// <param name="serializedSpecs">The serialized <code>PartSpecificationsType</code> class
        ///     instance to read</param>
        /// <param name="deserializationState">The <code>SerializationState</code> to assign the
        ///     results of the deserialization.</param>
        /// <returns>An <code>IEnumerator</code> that can be used for asynchronous reentance</returns>
        public IEnumerator Deserialize(PartSpecificationsType serializedSpecs, SerializationState deserializationState)
        {
            if (serializedSpecs == null)
            {
                // Record the deserialization state
                deserializationState.Error("Part specifications not defined");
                yield break;
            }

            // Vendor info (required)
            vendor = serializedSpecs.Vendor;
            version = serializedSpecs.Version;

            // Mass (optional)
            if (serializedSpecs.Mass1 != null)
            {
                // Min (optional)
                DeserializeMass(serializedSpecs.Mass1.Min, ref massMin, ref massMinUnits);

                // Max (optional)
                DeserializeMass(serializedSpecs.Mass1.Max, ref massMax, ref massMaxUnits);

                // Min (optional)
                DeserializeMass(serializedSpecs.Mass1.Contingency, ref massContingency, ref massContingencyUnits);
            }

            // Power (optional)
            if (serializedSpecs.Power != null)
            {
                // Min (optional)
                DeserializePower(serializedSpecs.Power.Min, ref powerMin, ref powerMinUnits);

                // Max (optional)
                DeserializePower(serializedSpecs.Power.Max, ref powerMax, ref powerMaxUnits);

                // Idle (optional)
                DeserializePower(serializedSpecs.Power.Idle, ref powerIdle, ref powerIdleUnits);

                // Average (optional)
                DeserializePower(serializedSpecs.Power.Average, ref powerAverage, ref powerAverageUnits);

                // Contigency (optional)
                DeserializePower(serializedSpecs.Power.Contingency, ref powerContingency, ref powerContingencyUnits);
            }

            // Notes
            notes = serializedSpecs.Notes1;

            // Reference (optional)
            try
            {
                reference = (!string.IsNullOrEmpty(serializedSpecs.Reference1)) ?
                    new Uri(serializedSpecs.Reference1) :
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
        /// Performs serialization of the supplied arguments into the supplied serialized power
        /// class instance.
        /// </summary>
        /// <param name="power">The power in default Unity units</param>
        /// <param name="units">The power units to use for serialization conversion</param>
        /// <param name="serialized">The serialized <code>PowerType</code> class instance to write</param>
        public void SerializePower(float power, PowerUnitType units, PowerType serialized)
        {
            // Only assign the units if the serialized type contains the default
            // units. Assignment could still be the default, but this handles the
            // case where it's not.
            if (units != new PowerType().units)
            {
                serialized.units = units;
            }
            serialized.units = units;
            SchemaUtil.SerializePower(power, serialized);
        }

        /// <summary>
        /// Performs serialization of this class instance into the supplied serialized specifications.
        /// </summary>
        /// <param name="serializedSpecs">The serialized <code>PartSpecificationsType</code> class
        ///     instance to write</param>
        /// <param name="serializationState">The <code>SerializationState</code> to assign the
        ///     results of the serialization.</param>
        /// <returns>An <code>IEnumerator</code> that can be used for asynchronous reentance</returns>
        public IEnumerator Serialize(PartSpecificationsType serializedSpecs, SerializationState serializationState)
        {
            if (serializedSpecs == null)
            {
                // Record the deserialization state
                serializationState.Error("Part specifications not defined");
                yield break;
            }

            // Vendor info (required)
            serializedSpecs.Vendor = vendor;
            serializedSpecs.Version = version;

            // Mass
            if (!MassIsDefaults)
            {
                // Make sure we have a serializable structure
                serializedSpecs.Mass1 = serializedSpecs.Mass1 ?? new MassSpecificationsType();

                // Min (optional)
                if (!Mathf.Approximately(massMin, PhysicalSceneObjectDefaults.MASS_MIN) ||
                    (massMinUnits != PhysicalSceneObjectDefaults.MASS_MIN_UNITS))
                {
                    serializedSpecs.Mass1.Min = serializedSpecs.Mass1.Min ?? new MassType();
                    SerializeMass(massMin, massMinUnits, serializedSpecs.Mass1.Min);
                }

                // Max (optional)
                if (!Mathf.Approximately(massMax, PhysicalSceneObjectDefaults.MASS_MAX) ||
                    (massMaxUnits != PhysicalSceneObjectDefaults.MASS_MAX_UNITS))
                {
                    serializedSpecs.Mass1.Max = serializedSpecs.Mass1.Max ?? new MassType();
                    SerializeMass(massMax, massMaxUnits, serializedSpecs.Mass1.Max);
                }

                // Contingency (optional)
                if (!Mathf.Approximately(massContingency, PhysicalSceneObjectDefaults.MASS_CONTINGENCY) ||
                    (massContingencyUnits != PhysicalSceneObjectDefaults.MASS_CONTINGENCY_UNITS))
                {
                    serializedSpecs.Mass1.Contingency = serializedSpecs.Mass1.Contingency ?? new MassType();
                    SerializeMass(massContingency, massContingencyUnits, serializedSpecs.Mass1.Contingency);
                }
            }

            // Power
            if (!PowerIsDefaults)
            {
                // Make sure we have a serializable structure
                serializedSpecs.Power ??= new PowerSpecificationsType();

                // Min (optional)
                if (!Mathf.Approximately(powerMin, InteractablePartDefaults.POWER_MIN) ||
                    (powerMinUnits != InteractablePartDefaults.POWER_MIN_UNITS))
                {
                    serializedSpecs.Power.Min ??= new PowerType();
                    SerializePower(powerMin, powerMinUnits, serializedSpecs.Power.Min);
                }

                // Max (optional)
                if (!Mathf.Approximately(powerMax, InteractablePartDefaults.POWER_MAX) ||
                    (powerMaxUnits != InteractablePartDefaults.POWER_MAX_UNITS))
                {
                    serializedSpecs.Power.Max ??= new PowerType();
                    SerializePower(powerMax, powerMaxUnits, serializedSpecs.Power.Max);
                }

                // Idle (optional)
                if (!Mathf.Approximately(powerIdle, InteractablePartDefaults.POWER_IDLE) ||
                    (powerIdleUnits != InteractablePartDefaults.POWER_IDLE_UNITS))
                {
                    serializedSpecs.Power.Idle ??= new PowerType();
                    SerializePower(powerIdle, powerIdleUnits, serializedSpecs.Power.Idle);
                }

                // Average (optional)
                if (!Mathf.Approximately(powerAverage, InteractablePartDefaults.POWER_AVERAGE) ||
                    (powerAverageUnits != InteractablePartDefaults.POWER_AVERAGE_UNITS))
                {
                    serializedSpecs.Power.Average ??= new PowerType();
                    SerializePower(powerAverage, powerAverageUnits, serializedSpecs.Power.Average);
                }

                // Contingency (optional)
                if (!Mathf.Approximately(powerContingency, InteractablePartDefaults.POWER_CONTINGENCY) ||
                    (powerContingencyUnits != InteractablePartDefaults.POWER_CONTINGENCY_UNITS))
                {
                    serializedSpecs.Power.Contingency ??= new PowerType();
                    SerializePower(powerContingency, powerContingencyUnits, serializedSpecs.Power.Contingency);
                }
            }

            // Reference
            serializedSpecs.Notes1 = notes;
            serializedSpecs.Reference1 = (reference != null) ? reference.ToString() : PhysicalSceneObjectDefaults.REFERENCE;

            // Mark as complete
            serializationState.complete = true;
        }
    }
}