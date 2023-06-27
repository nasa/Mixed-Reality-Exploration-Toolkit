// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public abstract class BaseAction<T> : Versioned<T>, IAction<T>
        where T : ActionType, new()
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(BaseAction<T>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedAction;

        #region IAction
        /// <seealso cref="IAction.CreateSerializedType"/>
        ActionType IAction.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="IAction.SerializedAction"/>
        public T SerializedAction
        {
            get
            {
                T result = _serializedAction;
                if (result == null)
                {
                    result = GetSerializedAction();
                }
                return result;
            }
            private set => _serializedAction = value;
        }
        private T _serializedAction;

        /// <seealso cref="IAction.SerializedAction"/>
        ActionType IAction.SerializedAction => SerializedAction;

        /// <seealso cref="IAction.Deserialize(ActionType, System.Action{bool, string})"/>
        void IAction.Deserialize(ActionType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="IAction.Serialize(ActionType, System.Action{bool, string})"/>
        void IAction.Serialize(ActionType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion IAction

        #region Serializable
        /// <seealso cref="Versioned{T}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(T serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedAction = serialized;

            // Process this object specific deserialization

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <seealso cref="Versioned{T}.Serialize(T, SerializationState)"/>
        protected override void Serialize(T serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Save the final serialized reference
            serializedAction = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serializable

        #region IAction
        /// <seealso cref="IAction.PerformAction"/>
        public void PerformAction()
        {
            // Trigger the serialization of the action
            T serializedAction = SerializedAction;
            if (serializedAction == null)
            {
                LogWarning("The serialized action is not defined.", nameof(PerformAction));
                return;
            }

            // Perform the action by deserializing ourself with the serialized action
            SerializationState derializedState = new SerializationState();
            DeserializeWithLogging(serializedAction, derializedState);
        }
        #endregion IAction

        /// <summary>
        /// Called to obtain the serialized version of the action.<br>
        /// </summary>
        private T GetSerializedAction()
        {
            var serializedAction = CreateSerializedType();
            SerializationState serializationState = new SerializationState();
            SerializeWithLogging(serializedAction, serializationState);

            // Assign the serialized action
            if (serializationState.IsError)
            {
                // Log a warning
                serializedAction = null;
                LogWarning("There was a problem serializing the action: " +
                    serializationState.ErrorMessage, nameof(GetSerializedAction));
            }

            return serializedAction;
        }

        /// <seealso cref="Versioned{T}.Initialize"/>
        protected override void Initialize()
        {
            base.Initialize();

            // Set the defaults
            SerializedAction = null;
        }

        /// <summary>
        /// Constructor for the <code>BaseAction</code>
        /// </summary>
        public BaseAction() : base()
        {
        }

        /// <summary>
        /// Constructor for the <code>BaseAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        public BaseAction(T serializedAction) : base()
        {
            SerializedAction = serializedAction;
        }

        /**
        public static BaseAction Deserialize(ActionType serializedAction)
        {
            GameObject foundObj;
            Rigidbody foundRBody;

            switch (serializedAction.Type)
            {
                case ActionTypeType.AddDrawing:
                    return ProjectAction.AddDrawingAction(serializedAction.Drawing);

                case ActionTypeType.AddNote:
                    return ProjectAction.AddNoteAction(serializedAction.NoteDeprecated, serializedAction.NoteName, serializedAction.Position, serializedAction.Rotation);

                case ActionTypeType.AddNoteDrawing:
                    return ProjectAction.AddNoteDrawingAction(serializedAction.NoteName, serializedAction.NoteDrawingName, serializedAction.NoteDrawing);

                case ActionTypeType.AddObject:
                    return ProjectAction.AddObjectAction(serializedAction.Part, serializedAction.Position, serializedAction.Rotation, serializedAction.Scale,
                        new InteractablePart.InteractablePartSettings(serializedAction.Part.EnableInteraction,
                        serializedAction.Part.EnableCollisions, serializedAction.Part.EnableGravity));

                case ActionTypeType.AddPointToLine:
                    return ProjectAction.AddPointToLine();

                case ActionTypeType.ApplyForce:
                    foundObj = GameObject.Find("LoadedProject/GameObjects/" + serializedAction.RigidbodyName);
                    if (foundObj)
                    {
                        foundRBody = foundObj.GetComponent<Rigidbody>();
                        if (foundRBody)
                        {
                            return RigidbodyActionDeprecated.ApplyForceAction(foundRBody, serializedAction.Force, serializedAction.ForceMode);
                        }
                    }
                    return null;

                case ActionTypeType.ChangeNoteState:
                    return ProjectAction.ChangeNoteState();

                case ActionTypeType.ChangeNoteText:
                    return ProjectAction.ChangeNoteTextAction(serializedAction.NoteName, serializedAction.Title, serializedAction.Content);

                case ActionTypeType.ChangeParent:
                    GameObject parentObj = GameObject.Find("LoadedProject/GameObjects/" + serializedAction.ParentObjectName);
                    if (parentObj)
                    {
                        GameObject childObj = GameObject.Find("LoadedProject/GameObjects/" + serializedAction.ChildObjectName);
                        if (childObj)
                        {
                            return RigidbodyActionDeprecated.ChangeParentAction(childObj.transform, parentObj.transform);
                        }
                    }
                    return null;

                case ActionTypeType.DeleteDrawing:
                    return ProjectAction.DeleteDrawingAction(serializedAction.DrawingName);

                case ActionTypeType.DeleteNote:
                    return ProjectAction.DeleteNoteAction(serializedAction.NoteName);

                case ActionTypeType.DeleteNoteDrawing:
                    return ProjectAction.DeleteNoteDrawingAction(serializedAction.NoteName, serializedAction.NoteDrawingName);

                case ActionTypeType.DeleteObject:
                    return ProjectAction.DeleteObjectAction(serializedAction.PartName);

                case ActionTypeType.DeletePointFromLine:
                    return ProjectAction.DeletePointFromLine();

                case ActionTypeType.MoveNote:
                    return ProjectAction.MoveNoteAction(serializedAction.NoteName, serializedAction.Position, serializedAction.Rotation);

                case ActionTypeType.MoveObject:
                    return ProjectAction.MoveObjectAction(serializedAction.PartName, serializedAction.Position, serializedAction.Rotation);

                case ActionTypeType.MoveUser:
                    return ViewActionDeprecated.MoveUserAction(serializedAction.UserName, serializedAction.Position, serializedAction.Rotation, serializedAction.Scale);

                case ActionTypeType.RotateEnvironment:
                    return ViewActionDeprecated.RotateEnvironmentAction(serializedAction.Axis, serializedAction.ReferencePoint, serializedAction.RotationInDegrees);

                case ActionTypeType.ScaleEnvironment:
                    return ViewActionDeprecated.ScaleEnvironmentAction(serializedAction.ScaleToApply);

                case ActionTypeType.SetFinalIKPos:
                    return ProjectAction.SetFinalIKPosAction(serializedAction.PartName, serializedAction.Position);

                case ActionTypeType.SetMatlabIKPos:
                    return ProjectAction.SetMatlabIKPosAction();

                case ActionTypeType.SwitchGravityState:
                    foundObj = GameObject.Find("LoadedProjects/GameObjects/" + serializedAction.RigidbodyName);
                    if (foundObj)
                    {
                        foundRBody = foundObj.GetComponent<Rigidbody>();
                        if (foundRBody)
                        {
                            return RigidbodyActionDeprecated.SwitchGravityStateAction(foundRBody, serializedAction.IsOn);
                        }
                    }
                    return null;

                case ActionTypeType.SwitchPhysicsState:
                    foundObj = GameObject.Find("LoadedProjects/GameObjects/" + serializedAction.RigidbodyName);
                    if (foundObj)
                    {
                        foundRBody = foundObj.GetComponent<Rigidbody>();
                        if (foundRBody)
                        {
                            return RigidbodyActionDeprecated.SwitchPhysicsStateAction(foundRBody, serializedAction.IsOn);
                        }
                    }
                    return null;

                case ActionTypeType.AnnotationGoTo:
                    return AnnotationAction.AnnotationGoToAction(serializedAction.AnnotationType, serializedAction.PartName, serializedAction.Index);

                case ActionTypeType.EndAnnotation:
                    return AnnotationAction.EndAnnotationAction(serializedAction.AnnotationType, serializedAction.PartName);

                case ActionTypeType.PauseAnnotation:
                    return AnnotationAction.PauseAnnotationAction(serializedAction.AnnotationType, serializedAction.PartName);

                case ActionTypeType.PlayAnnotation:
                    return AnnotationAction.PlayAnnotationAction(serializedAction.AnnotationType, serializedAction.PartName);

                case ActionTypeType.StartAnnotation:
                    return AnnotationAction.StartAnnotationAction(serializedAction.AnnotationType, serializedAction.FileName, serializedAction.PartName);

                case ActionTypeType.StopAnnotation:
                    return AnnotationAction.StopAnnotationAction(serializedAction.AnnotationType, serializedAction.PartName);

                case ActionTypeType.Unset:
                default:
                    return null;
            }
        }

        public virtual ActionType Serialize()
        {
            return new ActionType();
        }
        **/
    }
}