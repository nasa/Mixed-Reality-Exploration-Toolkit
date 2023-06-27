// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.Utilities.Math;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public class EnvironmentRotateAction : 
        BaseAction<EnvironmentRotateActionType>,
        IEnvironmentRotateAction<EnvironmentRotateActionType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(EnvironmentRotateAction);

        // Assign this based upon the schema in case it changes
        public readonly AxisType DEFAULT_AXIS = new EnvironmentRotateActionType().Axis;

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private EnvironmentRotateActionType serializedEnvironmentRotateAction;

        #region IEnvironmentRotateAction
        /// <seealso cref="IEnvironmentRotateAction.Axis"/>
        public AxisType Axis { get; protected set; }

        /// <seealso cref="IEnvironmentRotateAction.ReferencePoint"/>
        public Vector3 ReferencePoint { get; protected set; }

        /// <seealso cref="IEnvironmentRotateAction.Degrees"/>
        public float Degrees { get; protected set; }
        #endregion IEnvironmentRotateAction

        #region Serialization
        /// <seealso cref="Versioned{T}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(EnvironmentRotateActionType serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedEnvironmentRotateAction = serialized;

            // Process this object specific deserialization
            Vector3 rotationAxis =
                (Axis == AxisType.X) ? Vector3.right :
                (Axis == AxisType.Y) ? Vector3.up :
                Vector3.forward;
            MRET.ProjectManager.projectContainer.transform.RotateAround(ReferencePoint, rotationAxis, Degrees);

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <seealso cref="Versioned{T}.Serialize(T, SerializationState)"/>
        protected override void Serialize(EnvironmentRotateActionType serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the reference point
            TransformPositionType serializedReferencePoint = serialized.ReferencePoint;
            if (serializedReferencePoint is null)
            {
                // Default to basic transform structure
                serializedReferencePoint = new TransformPositionType();
                serialized.ReferencePoint = serializedReferencePoint;
            }
            SchemaUtil.SerializeTransformPosition(serializedReferencePoint, ReferencePoint);

            // Serialize the degrees
            serialized.Degrees = (float) MathUtil.NormalizeAngle360(Degrees);

            // Serialize the axis
            if (Axis != DEFAULT_AXIS)
            {
                serialized.Axis = Axis;
            }

            // Save the final serialized reference
            serializedEnvironmentRotateAction = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serialization

        /// <seealso cref="Versioned{T}.Initialize"/>
        protected override void Initialize()
        {
            base.Initialize();

            // Set the defaults
            Axis = DEFAULT_AXIS;
            ReferencePoint = Vector3.zero;
            Degrees = 0f;
        }

        /// <summary>
        /// Constructor for the <code>EnvironmentRotateAction</code>
        /// </summary>
        /// <seealso cref="BaseAction{T}.BaseAction()"/>
        public EnvironmentRotateAction() : base()
        {
        }

        /// <summary>
        /// Constructor for the <code>EnvironmentRotateAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="BaseAction{T}.BaseAction(T)"/>
        public EnvironmentRotateAction(EnvironmentRotateActionType serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>EnvironmentRotateAction</code>
        /// </summary>
        /// <param name="referencePoint">The rotation reference point for the action</param>
        /// <param name="degrees">The number of degrees to rotate for the action</param>
        /// <param name="axis">The rotation axis for the action</param>
        /// <seealso cref="BaseAction{T}.BaseAction(T)"/>
        public EnvironmentRotateAction(Vector3 referencePoint, float degrees, AxisType axis = AxisType.Y) : base()
        {
            // Assign the unique settings for this action
            ReferencePoint = referencePoint;
            Degrees = degrees;
            Axis = axis;
        }
    }
}