// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public abstract class SceneObjectTransformAction<T,I> : SceneObjectAction<T,I>, ISceneObjectTransformAction<T,I>
        where T : TransformSceneObjectActionType, new()
        where I : ISceneObject
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SceneObjectTransformAction<T,I>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedTransformAction;

        #region ISceneObjectTransformAction
        /// <seealso cref="ISceneObjectTransformAction.CreateSerializedType"/>
        TransformSceneObjectActionType ISceneObjectTransformAction.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="ISceneObjectTransformAction.Position"/>
        public Vector3 Position { get; private set; }

        /// <seealso cref="ISceneObjectTransformAction.Rotation"/>
        public Quaternion Rotation { get; private set; }

        /// <seealso cref="ISceneObjectTransformAction.Scale"/>
        public Vector3 Scale { get; private set; }

        /// <seealso cref="ISceneObjectTransformAction.Reference"/>
        public ReferenceSpaceType Reference { get; private set; }

        /// <seealso cref="ISceneObjectTransformAction.SerializedAction"/>
        TransformSceneObjectActionType ISceneObjectTransformAction.SerializedAction => SerializedAction;

        /// <seealso cref="ISceneObjectTransformAction.Deserialize(TransformSceneObjectActionType, System.Action{bool, string})"/>
        void ISceneObjectTransformAction.Deserialize(TransformSceneObjectActionType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="ISceneObjectTransformAction.Serialize(TransformSceneObjectActionType, Action{bool, string})"/>
        void ISceneObjectTransformAction.Serialize(TransformSceneObjectActionType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion ISceneObjectTransformAction

        #region Serialization
        /// <seealso cref="SceneObjectAction{T,I}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(T serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedTransformAction = serialized;

            // Process this object specific deserialization
            SchemaUtil.DeserializeTransform(serialized.Transform, ActionObject.gameObject);

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <seealso cref="SceneObjectAction{T,I}.Serialize(T, SerializationState)"/>
        protected override void Serialize(T serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Start with our internal serialized transform to serialize out the transform
            // using the original deserialized structure (if was provided during deserialization)
            TransformType serializedTransform = null;
            if (serializedTransformAction != null)
            {
                // Use this transform structure
                serializedTransform = serializedTransformAction.Transform;
            }

            // Make sure we have a valid serialized transform reference
            if (serializedTransform is null)
            {
                // Default to basic transform structure
                serializedTransform = new TransformType();
            }

            // Serialize the position transform
            TransformPositionType serializedTransformPosition = serializedTransform.Position;
            if (serializedTransformPosition is null)
            {
                // Default to basic transform structure
                serializedTransformPosition = new TransformPositionType();
                serializedTransform.Position = serializedTransformPosition;
            }
            SchemaUtil.SerializeTransformPosition(serializedTransformPosition, Position, Reference);

            // Serialize the rotation transform
            TransformRotationType serializedTransformRotation = serializedTransform.Item;
            if (serializedTransformRotation is null)
            {
                // Default to basic transform structure
                serializedTransformRotation = new TransformQRotationType();
                serializedTransform.Item = serializedTransformRotation;
            }
            if (serializedTransformRotation is TransformEulerRotationType)
            {
                // Euler
                SchemaUtil.SerializeTransformRotation(
                    serializedTransformRotation as TransformEulerRotationType,
                    Rotation.eulerAngles, Reference);
            }
            else if (serializedTransformRotation is TransformQRotationType)
            {
                // Quaternion
                SchemaUtil.SerializeTransformRotation(
                    serializedTransformRotation as TransformQRotationType,
                    Rotation, Reference);
            }

            // Serialize the scale transform
            TransformScaleType serializedTransformScale = serializedTransform.Item1;
            if (serializedTransformScale is null)
            {
                // Default to basic transform structure
                serializedTransformScale = new TransformScaleType(); // FIXME: What if we should use size?
                serializedTransform.Item1 = serializedTransformScale;
            }
            SchemaUtil.SerializeTransformScale(serializedTransformScale, Scale, Reference);

            // Serialize out the transform
            serialized.Transform = serializedTransform;

            // Save the final serialized reference
            serializedTransformAction = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serialization

        /// <seealso cref="Versioned{T}.Initialize"/>
        protected override void Initialize()
        {
            base.Initialize();

            // Set the defaults
            Position = Vector3.zero;
            Rotation = new Quaternion();
            Scale = Vector3.zero;
            Reference = ReferenceSpaceType.Global;
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectTransformAction</code>
        /// </summary>
        /// <seealso cref="SceneObjectAction{T,I}.SceneObjectAction()"/>
        public SceneObjectTransformAction() : base()
        {
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectTransformAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="SceneObjectAction{T,I}.SceneObjectAction(T)"/>
        public SceneObjectTransformAction(T serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectTransformAction</code>
        /// </summary>
        /// <param name="sceneObject">The <code>ISceneObject</code> associated with this action</param>
        /// <seealso cref="SceneObjectAction{T,I}.SceneObjectAction(I,ISceneObject)"/>
        public SceneObjectTransformAction(I sceneObject) : base(sceneObject)
        {
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectTransformAction</code>
        /// </summary>
        /// <param name="sceneObjectId">The scene object ID associated with this action</param>
        /// <param name="parentId">The optional scene object parent ID associated with this action</param>
        public SceneObjectTransformAction(string sceneObjectId, string parentId = "") : base(sceneObjectId, parentId)
        {
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectTransformAction</code>
        /// </summary>
        /// <param name="sceneObject">The <code>ISceneObject</code> associated with this action</param>
        /// <param name="position">The <code>Vector3</code> position associate with this action</param>
        /// <param name="reference">The <code>ReferenceSpaceType</code> of the transform</param>
        /// <seealso cref="SceneObjectAction{T,I}.SceneObjectAction(I,ISceneObject)"/>
        public SceneObjectTransformAction(I sceneObject,
            Vector3 position, ReferenceSpaceType reference = ReferenceSpaceType.Global) : base(sceneObject)
        {
            // Assign the unique settings for this action
            Position = position;
            Reference = reference;
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectTransformAction</code>
        /// </summary>
        /// <param name="sceneObjectId">The scene object ID associated with this action</param>
        /// <param name="position">The <code>Vector3</code> position associate with this action</param>
        /// <param name="reference">The <code>ReferenceSpaceType</code> of the transform</param>
        public SceneObjectTransformAction(string sceneObjectId,
            Vector3 position, ReferenceSpaceType reference = ReferenceSpaceType.Global) : base(sceneObjectId)
        {
            // Assign the unique settings for this action
            Position = position;
            Reference = reference;
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectTransformAction</code>
        /// </summary>
        /// <param name="sceneObject">The <code>ISceneObject</code> associated with this action</param>
        /// <param name="rotation">The <code>Quaternion</code> rotation associate with this action</param>
        /// <param name="reference">The <code>ReferenceSpaceType</code> of the transform</param>
        /// <seealso cref="SceneObjectAction{T,I}.SceneObjectAction(I,ISceneObject)"/>
        public SceneObjectTransformAction(I sceneObject,
            Quaternion rotation, ReferenceSpaceType reference = ReferenceSpaceType.Global) : base(sceneObject)
        {
            // Assign the unique settings for this action
            Rotation = rotation;
            Reference = reference;
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectTransformAction</code>
        /// </summary>
        /// <param name="sceneObjectId">The scene object ID associated with this action</param>
        /// <param name="rotation">The <code>Quaternion</code> rotation associate with this action</param>
        /// <param name="reference">The <code>ReferenceSpaceType</code> of the transform</param>
        public SceneObjectTransformAction(string sceneObjectId,
            Quaternion rotation, ReferenceSpaceType reference = ReferenceSpaceType.Global) : base(sceneObjectId)
        {
            // Assign the unique settings for this action
            Rotation = rotation;
            Reference = reference;
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectTransformAction</code>
        /// </summary>
        /// <param name="sceneObject">The <code>ISceneObject</code> associated with this action</param>
        /// <param name="position">The <code>Vector3</code> position associate with this action</param>
        /// <param name="rotation">The <code>Quaternion</code> rotation associate with this action</param>
        /// <param name="scale">The <code>Vector3</code> scale associate with this action</param>
        /// <param name="reference">The <code>ReferenceSpaceType</code> of the transform</param>
        /// <seealso cref="SceneObjectAction{T,I}.SceneObjectAction(I,ISceneObject)"/>
        public SceneObjectTransformAction(I sceneObject,
            Vector3 position, Quaternion rotation, Vector3 scale,
            ReferenceSpaceType reference = ReferenceSpaceType.Global) : base(sceneObject)
        {
            // Assign the unique settings for this action
            Position = position;
            Rotation = rotation;
            Scale = scale;
            Reference = reference;
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectTransformAction</code>
        /// </summary>
        /// <param name="sceneObjectId">The scene object ID associated with this action</param>
        /// <param name="position">The <code>Vector3</code> position associate with this action</param>
        /// <param name="rotation">The <code>Quaternion</code> rotation associate with this action</param>
        /// <param name="scale">The <code>Vector3</code> scale associate with this action</param>
        /// <param name="reference">The <code>ReferenceSpaceType</code> of the transform</param>
        public SceneObjectTransformAction(string sceneObjectId,
            Vector3 position, Quaternion rotation, Vector3 scale,
            ReferenceSpaceType reference = ReferenceSpaceType.Global) : base(sceneObjectId)
        {
            // Assign the unique settings for this action
            Position = position;
            Rotation = rotation;
            Scale = scale;
            Reference = reference;
        }
    }

    /// <summary>
    /// Provides an implementation for the generic SceneObjectTransformAction class
    /// </summary>
    public class SceneObjectTransformAction : SceneObjectTransformAction<TransformSceneObjectActionType, ISceneObject>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SceneObjectTransformAction);

        /// <summary>
        /// Constructor for the <code>SceneObjectTransformAction</code>
        /// </summary>
        /// <seealso cref="SceneObjectTransformAction{T,I}.SceneObjectTransformAction()"/>
        public SceneObjectTransformAction() : base()
        {
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectTransformAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="SceneObjectTransformAction{T,I}.SceneObjectTransformAction(T)"/>
        public SceneObjectTransformAction(TransformSceneObjectActionType serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectTransformAction</code>
        /// </summary>
        /// <param name="sceneObject">The <code>ISceneObject</code> associated with this action</param>
        /// <seealso cref="SceneObjectTransformAction{T,I}.SceneObjectTransformAction(I)"/>
        public SceneObjectTransformAction(ISceneObject sceneObject) : base(sceneObject)
        {
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectTransformAction</code>
        /// </summary>
        /// <param name="sceneObjectId">The scene object ID associated with this action</param>
        /// <param name="parentId">The optional scene object parent ID associated with this action</param>
        /// <seealso cref="SceneObjectTransformAction{T,I}.SceneObjectTransformAction(string,string)"/>
        public SceneObjectTransformAction(string sceneObjectId, string parentId = "") : base(sceneObjectId, parentId)
        {
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectTransformAction</code>
        /// </summary>
        /// <param name="sceneObject">The <code>ISceneObject</code> associated with this action</param>
        /// <param name="position">The <code>Vector3</code> position associate with this action</param>
        /// <param name="reference">The <code>ReferenceSpaceType</code> of the transform</param>
        /// <seealso cref="SceneObjectTransformAction{T,I}.SceneObjectTransformAction(ISceneObject, Vector3, ReferenceSpaceType)"/>
        public SceneObjectTransformAction(ISceneObject sceneObject,
            Vector3 position, ReferenceSpaceType reference = ReferenceSpaceType.Global) : base(sceneObject)
        {
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectTransformAction</code>
        /// </summary>
        /// <param name="sceneObjectId">The scene object ID associated with this action</param>
        /// <param name="position">The <code>Vector3</code> position associate with this action</param>
        /// <param name="reference">The <code>ReferenceSpaceType</code> of the transform</param>
        /// <seealso cref="SceneObjectTransformAction{T,I}.SceneObjectTransformAction(string, Vector3, ReferenceSpaceType)"/>
        public SceneObjectTransformAction(string sceneObjectId,
            Vector3 position, ReferenceSpaceType reference = ReferenceSpaceType.Global) : base(sceneObjectId)
        {
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectTransformAction</code>
        /// </summary>
        /// <param name="sceneObject">The <code>ISceneObject</code> associated with this action</param>
        /// <param name="rotation">The <code>Quaternion</code> rotation associate with this action</param>
        /// <param name="reference">The <code>ReferenceSpaceType</code> of the transform</param>
        /// <seealso cref="SceneObjectTransformAction{T,I}.SceneObjectTransformAction(I, Quaternion, ReferenceSpaceType)"/>
        public SceneObjectTransformAction(ISceneObject sceneObject,
            Quaternion rotation, ReferenceSpaceType reference = ReferenceSpaceType.Global) : base(sceneObject)
        {
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectTransformAction</code>
        /// </summary>
        /// <param name="sceneObjectId">The scene object ID associated with this action</param>
        /// <param name="rotation">The <code>Quaternion</code> rotation associate with this action</param>
        /// <param name="reference">The <code>ReferenceSpaceType</code> of the transform</param>
        /// <seealso cref="SceneObjectTransformAction{T,I}.SceneObjectTransformAction(string, Quaternion, ReferenceSpaceType)"/>
        public SceneObjectTransformAction(string sceneObjectId,
            Quaternion rotation, ReferenceSpaceType reference = ReferenceSpaceType.Global) : base(sceneObjectId)
        {
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectTransformAction</code>
        /// </summary>
        /// <param name="sceneObject">The <code>ISceneObject</code> associated with this action</param>
        /// <param name="position">The <code>Vector3</code> position associate with this action</param>
        /// <param name="rotation">The <code>Quaternion</code> rotation associate with this action</param>
        /// <param name="scale">The <code>Vector3</code> scale associate with this action</param>
        /// <param name="reference">The <code>ReferenceSpaceType</code> of the transform</param>
        /// <seealso cref="SceneObjectTransformAction{T,I}.SceneObjectTransformAction(I, Vector3, Quaternion, Vector3, ReferenceSpaceType)"/>
        public SceneObjectTransformAction(ISceneObject sceneObject,
            Vector3 position, Quaternion rotation, Vector3 scale,
            ReferenceSpaceType reference = ReferenceSpaceType.Global) : base(sceneObject)
        {
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectTransformAction</code>
        /// </summary>
        /// <param name="sceneObjectId">The scene object ID associated with this action</param>
        /// <param name="position">The <code>Vector3</code> position associate with this action</param>
        /// <param name="rotation">The <code>Quaternion</code> rotation associate with this action</param>
        /// <param name="scale">The <code>Vector3</code> scale associate with this action</param>
        /// <param name="reference">The <code>ReferenceSpaceType</code> of the transform</param>
        /// <seealso cref="SceneObjectTransformAction{T,I}.SceneObjectTransformAction(string, Vector3, Quaternion, Vector3, ReferenceSpaceType)"/>
        public SceneObjectTransformAction(string sceneObjectId,
            Vector3 position, Quaternion rotation, Vector3 scale,
            ReferenceSpaceType reference = ReferenceSpaceType.Global) : base(sceneObjectId)
        {
        }
    }
}