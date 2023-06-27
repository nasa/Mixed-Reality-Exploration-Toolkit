// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET.Animation
{
    /// <summary>
    /// The MRETActionAnimation class encapsulates a BaseAction and an inverse BaseAction. 
    /// When the animation is started or the currentTime is set to a value > 0 the PerformAction method 
    /// on the Action will be called. If the currentTime is set to 0 the PerformAction will be called on
    /// the Inverse action.
    /// </summary>
    public class MRETActionAnimation : MRETBaseAnimation<ActionSequenceFrameType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(MRETActionAnimation);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private ActionSequenceFrameType serializedFrame;

        /// <summary>
        /// The Action property that references the <code>IAction</code> to perform by this animation.
        /// </summary>
        public IAction Action { get; set; }

        /// <summary>
        /// The Inverse property that references the <code>IAction</code> to undo this animation.
        /// </summary>
        public IAction Inverse { get; set; }

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

            // Set the defaults
            Action = null;
            Inverse = null;
        }
        #endregion MRETUpdateBehaviour

        #region Serializable
        /// <summary>
        /// Asynchronously deserializes the supplied serialized action set field into the supplied action
        /// and updates the supplied state with the serialization state.
        /// </summary>
        /// <param name="action">The <code>IAction</code> to assign</param>
        /// <param name="serializedActionSet">The serialized <code>ActionSetType</code> field to deserialize</param>
        /// <param name="actionDeserializationState">The <code>SerializationState</code> to populate with the deserialization state.</param>
        /// 
        /// <see cref="IAction"/>
        /// <see cref="ActionSetType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected virtual IEnumerator DeserializeActionSet(IAction action, ActionSetType serializedActionSet, SerializationState actionDeserializationState)
        {
            // Setup the deserialized action
            void DeserializedAction(bool successful, string message)
            {
                if (!successful)
                {
                    // Error condition
                    actionDeserializationState.Error("There was a problem deserializing the action: " +
                        message);

                    // Clear the reference
                    action = null;
                }

                // Mark as complete
                actionDeserializationState.complete = true;
            };

            // Deserialize the correct action type
            ActionType serializedAction = serializedActionSet.Item;

            // SceneObject
            if (serializedAction is AddSceneObjectActionType)
            {
                // FIXME: This may be wrong. What should be the generic type params be to the constructor?
                action = new AddSceneObjectAction(serializedAction as AddSceneObjectActionType);
            }
            else if (serializedAction is DeleteIdentifiableObjectActionType)
            {
                action = new DeleteIdentifiableObjectAction(serializedAction as DeleteIdentifiableObjectActionType);
            }
            // This check must occur after the AddSceneObjectActionType because it's a subclass
            else if (serializedAction is TransformSceneObjectActionType)
            {
                action = new SceneObjectTransformAction(serializedAction as TransformSceneObjectActionType);
            }
            else if (serializedAction is UpdateIdentifiableObjectActionType)
            {
                // FIXME: This may be wrong. What should be the generic type params be to the constructor?
                action = new IdentifiableObjectUpdateAction(serializedAction as UpdateIdentifiableObjectActionType);
            }

            // InteractableSceneObject
            else if (serializedAction is UpdateInteractionsActionType)
            {
                action = new UpdateInteractionsAction(serializedAction as UpdateInteractionsActionType);
            }

            // PhysicalSceneObject
            else if (serializedAction is UpdatePhysicsActionType)
            {
                action = new UpdatePhysicsAction(serializedAction as UpdatePhysicsActionType);
            }

            // Drawing
            else if (serializedAction is UpdateDrawingActionType)
            {
                action = new UpdateDrawingAction(serializedAction as UpdateDrawingActionType);
            }
            else if (serializedAction is AddPointsToDrawingActionType)
            {
                action = new AddPointsToDrawingAction(serializedAction as AddPointsToDrawingActionType);
            }
            else if (serializedAction is DeletePointsFromDrawingActionType)
            {
                action = new DeletePointsFromDrawingAction(serializedAction as DeletePointsFromDrawingActionType);
            }

            // Environment
            else if (serializedAction is EnvironmentRotateActionType)
            {
                action = new EnvironmentRotateAction(serializedAction as EnvironmentRotateActionType);
            }
            else if (serializedAction is EnvironmentScaleActionType)
            {
                action = new EnvironmentScaleAction(serializedAction as EnvironmentScaleActionType);
            }

            // Annotation
            else if (serializedAction is TextAnnotationActionType)
            {
                action = new TextAnnotationAction(serializedAction as TextAnnotationActionType);
            }
            else if (serializedAction is SourceAnnotationActionType)
            {
                action = new SourceAnnotationAction(serializedAction as SourceAnnotationActionType);
            }

            // Error condition
            else
            {
                actionDeserializationState.Error("Unknown action type encountered: " +
                    nameof(serializedAction));

                // Clear the reference
                action = null;

                yield break;
            }

            // Perform the deserialization
            action.Deserialize(serializedAction, DeserializedAction);

            // If the action deserialization failed, exit with an error
            if (actionDeserializationState.IsError) yield break;

            yield return null;
        }

        /// <seealso cref="MRETBaseAnimation{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(ActionSequenceFrameType serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedFrame = serialized;

            // Process this object specific deserialization
            // Deserialize the action
            SerializationState actionDeserializationState = new SerializationState();
            StartCoroutine(DeserializeActionSet(Action, serialized.Action, actionDeserializationState));

            // Wait for the coroutine to complete
            while (!actionDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // Record the deserialization state
            deserializationState.Update(actionDeserializationState);

            // If the action serialization failed, exit with an error
            if (deserializationState.IsError) yield break;

            // Deserialize the inverse action (optional)
            Inverse = null;
            if (serialized.Inverse != null)
            {
                SerializationState inverseDeserializationState = new SerializationState();
                StartCoroutine(DeserializeActionSet(Inverse, serialized.Inverse, inverseDeserializationState));

                // Wait for the coroutine to complete
                while (!inverseDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(inverseDeserializationState);

                // If the inverse action deserialization failed, exit with an error
                if (deserializationState.IsError) yield break;
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <summary>
        /// Asynchronously serializes the supplied action into the supplied serialized action set field
        /// and updates the supplied state with the serialization state.
        /// </summary>
        /// <param name="action">The <code>IAction</code> to serialize</param>
        /// <param name="serializedActionSet">The serialized <code>ActionSetType</code> field to populate with the serialized action</param>
        /// <param name="actionSerializationState">The <code>SerializationState</code> to populate with the serialization state.</param>
        /// 
        /// <see cref="ActionSetType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected virtual IEnumerator SerializeActionSet(IAction action, ActionSetType serializedActionSet, SerializationState actionSerializationState)
        {
            ActionType serializedAction = null;

            // Setup the serialized action
            void SerializedAction(bool successful, string message)
            {
                if (!successful)
                {
                    // Error condition
                    actionSerializationState.Error("There was a problem serializing the action: " +
                        message);

                    // Clear the reference
                    serializedAction = null;
                }

                // Mark as complete
                actionSerializationState.complete = true;
            };

            // Perform the serialization
            Action.Serialize(serializedAction, SerializedAction);

            // If the action serialization failed, exit with an error
            if (actionSerializationState.IsError) yield break;

            // Serialize out the correct action type

            // SceneObject
            if (serializedAction is AddSceneObjectActionType)
            {
                serializedActionSet.Item = serializedAction as AddSceneObjectActionType;
            }
            else if (serializedAction is DeleteIdentifiableObjectActionType)
            {
                serializedActionSet.Item = serializedAction as DeleteIdentifiableObjectActionType;
            }
            // This check must occur after the AddSceneObjectActionType because it's a subclass
            else if (serializedAction is TransformSceneObjectActionType)
            {
                serializedActionSet.Item = serializedAction as TransformSceneObjectActionType;
            }
            else if (serializedAction is UpdateIdentifiableObjectActionType)
            {
                serializedActionSet.Item = serializedAction as UpdateIdentifiableObjectActionType;
            }

            // PhysicalSceneObject
            else if (serializedAction is UpdateInteractionsActionType)
            {
                serializedActionSet.Item = serializedAction as UpdateInteractionsActionType;
            }

            // PhysicalSceneObject
            else if (serializedAction is UpdatePhysicsActionType)
            {
                serializedActionSet.Item = serializedAction as UpdatePhysicsActionType;
            }

            // Drawing
            else if (serializedAction is UpdateDrawingActionType)
            {
                serializedActionSet.Item = serializedAction as UpdateDrawingActionType;
            }
            else if (serializedAction is AddPointsToDrawingActionType)
            {
                serializedActionSet.Item = serializedAction as AddPointsToDrawingActionType;
            }
            else if (serializedAction is DeletePointsFromDrawingActionType)
            {
                serializedActionSet.Item = serializedAction as DeletePointsFromDrawingActionType;
            }

            // Environment
            else if (serializedAction is EnvironmentRotateActionType)
            {
                serializedActionSet.Item = serializedAction as EnvironmentRotateActionType;
            }
            else if (serializedAction is EnvironmentScaleActionType)
            {
                serializedActionSet.Item = serializedAction as EnvironmentScaleActionType;
            }

            // Annotation
            else if (serializedAction is TextAnnotationActionType)
            {
                serializedActionSet.Item = serializedAction as TextAnnotationActionType;
            }
            else if (serializedAction is SourceAnnotationActionType)
            {
                serializedActionSet.Item = serializedAction as SourceAnnotationActionType;
            }

            // Error condition
            else
            {
                actionSerializationState.Error("Unknown action type encountered: " +
                    nameof(serializedAction));

                // Clear the reference
                serializedActionSet.Item = null;

                yield break;
            }

            // Mark as complete
            actionSerializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="MRETBaseAnimation{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(ActionSequenceFrameType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the action
            SerializationState actionSerializationState = new SerializationState();
            StartCoroutine(SerializeActionSet(Action, serialized.Action, actionSerializationState));

            // Wait for the coroutine to complete
            while (!actionSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // Record the serialization state
            serializationState.Update(actionSerializationState);

            // If the action serialization failed, exit with an error
            if (serializationState.IsError) yield break;

            // Serialize the inverse action (optional)
            serialized.Inverse = null;
            if (Inverse != null)
            {
                SerializationState inverseSerializationState = new SerializationState();
                StartCoroutine(SerializeActionSet(Inverse, serialized.Inverse, inverseSerializationState));

                // Wait for the coroutine to complete
                while (!inverseSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the serialization state
                serializationState.Update(inverseSerializationState);

                // If the inverse action serialization failed, exit with an error
                if (serializationState.IsError) yield break;
            }

            // Save the final serialized reference
            serializedFrame = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

        /// <summary>
        /// Overrides inherited updateCurrentTime to perform the action or inverse action.  
        /// </summary>
        /// <param name="currentTime"></param>
        protected override void UpdateCurrentTime(float currentTime)
        {
            //Debug.Log("[MRETActionAnimation->updateCurrentTime] " + currentTime);
            if (currentTime > 0)
            {
                Action.PerformAction();
            }
            else if (Inverse != null)
            {
                Inverse.PerformAction();
            }
        }
    }
}