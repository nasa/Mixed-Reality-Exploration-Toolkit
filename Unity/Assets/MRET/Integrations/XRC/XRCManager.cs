// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.XRC;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.User;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Integrations.GMSEC;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.XRC
{
    public class XRCManager : MRETManager<XRCManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(XRCManager);

        static string XRC_PACKAGE_NAME = "XRCEngine";

        public List<IUser> users = new List<IUser>();

        public enum Category
        {
            // Core Objects
            IDENTIFIABLEOBJECT,
            SCENEOBJECT,
            INTERACTABLESCENEOBJECT,
            PHYSICALSCENEOBJECT,

            // Part
            PART,

            // Notes
            NOTE,

            // Drawings
            DRAWING2D,
            DRAWING3D,

            // Annotations
            TEXTANNOTATION,
            SOURCEANNOTATION,

            // User
            USER,
            USER_LCONTROLLER,
            USER_RCONTROLLER,
            USER_LPOINTER,
            USER_RPOINTER
        }

        public static readonly string
            OBJECTCATEGORY = "OBJECT",
            DRAWINGCATEGORY = "DRAWING",
            NOTECATEGORY = "NOTE",
            NOTEDRAWINGCATEGORY = "NOTEDRAWING",
            USERCATEGORY = "USER",
            LCONTROLLERCATEGORY = "USER.L",
            RCONTROLLERCATEGORY = "USER.R",
            LPOINTERCATEGORY = "USER.POINTER.L",
            RPOINTERCATEGORY = "USER.POINTER.R";
        
        private string currentServer;
        private int currentPort;
        private ConnectionTypes currentConnType;

        public CollaborationManager collaborationManager;

        #region MRETBehaviour
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

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Set the defaults
        }
        #endregion MRETBehaviour

        /// <seealso cref="MRETManager{M}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();

            string xrcPath;

            // Get the XRC package path
            xrcPath = PackageLoader.GetPackagePath(Application.dataPath, XRC_PACKAGE_NAME);

            // Initialize the XRC DLL
            PackageLoader.InitializePackagePlugin(xrcPath);

            if (collaborationManager == null)
            {
                collaborationManager = GOV.NASA.GSFC.XR.MRET.MRET.CollaborationManager;
            }
        }

        public bool IsXRCStarted => XRCUnity.XRCStarted;

        public bool StartXRC() => XRCUnity.StartUp();

        public bool StopXRC() => XRCUnity.ShutDown();

        public void InitializeXRC(string server, int port, ConnectionTypes connType,
            string missionName, string satName, string projectName = "XRC", string groupName = "MRET", string sessionName = "UNSET")
        {
            XRCUnity.Initialize(server, port,
                GMSECToXRCConnType(collaborationManager.connectionType),
                collaborationManager.missionName,
                collaborationManager.satName, projectName, groupName, sessionName);
        }

        public IRemoteSession[] GetRemoteSessions() => XRCUnity.GetRemoteSessions();

        public bool IsSessionActive => XRCUnity.IsSessionActive;

        public bool IsMaster => XRCUnity.IsMaster;

        public bool StartSession(IUser user)
        {
            bool result = false;

            if (user != null)
            {
                // Serialize out the user
                UserType serializedUser = user.CreateSerializedType();
                user.Serialize(serializedUser);

                // Start the session
                result = XRCUnity.StartSession(serializedUser);

                // Add the user components
                if (result)
                {
                    AddUserComponentsToSession(user);
                }
            }
            return result;
        }

        public bool EndSession() => XRCUnity.EndSession();

        public bool JoinSession(string sessionID, IUser user)
        {
            bool result = false;

            if (user != null)
            {
                // Serialize out the user
                UserType serializedUser = user.CreateSerializedType();
                user.Serialize(serializedUser);

                // Join the session
                result = XRCUnity.JoinSession(sessionID, serializedUser);
            }
            return result;
        }

        public bool LeaveSession() => XRCUnity.LeaveSession();

        public bool SessionEntityExists(IIdentifiable entity) => XRCUnity.SessionEntityExists(entity);

        public bool AddSessionEntity(IIdentifiable entity, IIdentifiable parent)
        {
            var serializedEntity = entity.CreateSerializedType();
            entity.Serialize(serializedEntity);
            return XRCUnity.AddSessionEntity(serializedEntity, parent);
        }

        public bool RemoveSessionEntity(IIdentifiable entity) => XRCUnity.RemoveSessionEntity(entity);

        public bool UpdateSessionEntity(IIdentifiable entity, IIdentifiable parent)
        {
            var serializedEntity = entity.CreateSerializedType();
            entity.Serialize(serializedEntity);
            return XRCUnity.UpdateSessionEntity(serializedEntity, parent);
        }

        public bool UpdateSessionEntityParent(ISceneObject entity, IIdentifiable parent) =>
            XRCUnity.UpdateSessionEntityParent(entity, parent);

        public bool UpdateSessionEntityTransform(ISceneObject entity) =>
            XRCUnity.UpdateSessionEntityTransform(entity);

        public bool UpdateSessionEntityPosition(ISceneObject entity) =>
            XRCUnity.UpdateSessionEntityPosition(entity);

        public bool UpdateSessionEntityScale(ISceneObject entity) =>
            XRCUnity.UpdateSessionEntityScale(entity);

        public bool UpdateSessionEntityRotation(ISceneObject entity) =>
            XRCUnity.UpdateSessionEntityRotation(entity);

        #region XRC Event Handlers
        /// <summary>
        /// Called by an entity created event trigger from the XRCUnity script.
        /// </summary>
        /// <seealso cref="XRCUnity.entityCreatedUnityEvent"/>
        public void EntityCreationEventManager()
        {
            IEntityParameters evParams = XRCUnity.entityCreatedEventQueue.Dequeue();

            // Create the entity based upon the supplied entity parameters
            if (evParams.SerializedEntity is TextAnnotationType)
            {
                collaborationManager.CreateRemoteEntity(evParams.SerializedEntity as TextAnnotationType);
            }
            else if (evParams.SerializedEntity is SourceAnnotationType)
            {
                collaborationManager.CreateRemoteEntity(evParams.SerializedEntity as SourceAnnotationType);
            }
            else if (evParams.SerializedEntity is PartType)
            {
                collaborationManager.CreateRemoteEntity(evParams.SerializedEntity as PartType);
            }
            else if (evParams.SerializedEntity is NoteType)
            {
                collaborationManager.CreateRemoteEntity(evParams.SerializedEntity as NoteType, evParams.ParentID);
            }
            else if (evParams.SerializedEntity is Drawing2dType)
            {
                collaborationManager.CreateRemoteEntity(evParams.SerializedEntity as Drawing2dType, evParams.ParentID);
            }
            else if (evParams.SerializedEntity is Drawing3dType)
            {
                collaborationManager.CreateRemoteEntity(evParams.SerializedEntity as Drawing3dType, evParams.ParentID);
            }
            else if (evParams.SerializedEntity is UserType)
            {
                collaborationManager.CreateRemoteUser(evParams.SerializedEntity as UserType);
            }
            else if (evParams.SerializedEntity is UserHeadType)
            {
                collaborationManager.CreateRemoteUserHead(evParams.SerializedEntity as UserHeadType, evParams.ParentID);
            }
            else if (evParams.SerializedEntity is UserTorsoType)
            {
                collaborationManager.CreateRemoteUserTorso(evParams.SerializedEntity as UserTorsoType, evParams.ParentID);
            }
            else if (evParams.SerializedEntity is UserHandType)
            {
                collaborationManager.CreateRemoteUserHand(evParams.SerializedEntity as UserHandType, evParams.ParentID);
            }
            else if (evParams.SerializedEntity is UserControllerType)
            {
                collaborationManager.CreateRemoteUserHandController(evParams.SerializedEntity as UserControllerType, evParams.ParentID);
            }
            else if (evParams.SerializedEntity is UserPointerType)
            {
                collaborationManager.CreateRemoteUserHandPointer(evParams.SerializedEntity as UserPointerType, evParams.ParentID);
            }
            else if (evParams.SerializedEntity is PhysicalSceneObjectType)
            {
                collaborationManager.CreateRemoteEntity(evParams.SerializedEntity as PhysicalSceneObjectType, evParams.ParentID);
            }
            else if (evParams.SerializedEntity is InteractableSceneObjectType)
            {
                collaborationManager.CreateRemoteEntity(evParams.SerializedEntity as InteractableSceneObjectType, evParams.ParentID);
            }
            else if (evParams.SerializedEntity is SceneObjectType)
            {
                collaborationManager.CreateRemoteEntity(evParams.SerializedEntity as SceneObjectType, evParams.ParentID);
            }
            else if (evParams.SerializedEntity is IdentifiableType)
            {
                collaborationManager.CreateRemoteEntity(evParams.SerializedEntity);
            }
            else
            {
                LogWarning("Serialized type unknown: " + evParams.SerializedEntity?.GetType(), nameof(EntityCreationEventManager));
            }
        }

        /// <summary>
        /// Called by an entity destroyed event trigger from the XRCUnity script.
        /// </summary>
        /// <seealso cref="XRCUnity.entityDestroyedUnityEvent"/>
        public void EntityDestructionEventManager()
        {
            IEntityParameters evParams = XRCUnity.entityDestroyedEventQueue.Dequeue();

            collaborationManager.DeleteRemoteEntity(evParams.SerializedEntity, evParams.ParentID);
        }

        /// <summary>
        /// Called by an entity reinitialize event trigger from the XRCUnity script.
        /// </summary>
        /// <seealso cref="XRCUnity.entityReinitializedUnityEvent"/>
        public void EntityReinitializationEventManager()
        {
            IEntityParameters evParams = XRCUnity.entityReinitializedEventQueue.Dequeue();

            Debug.LogWarning("[" + ClassName + "] Entity reinitialization not yet implemented.");
        }

        /// <summary>
        /// Called by an entity updated event trigger from the XRCUnity script.
        /// </summary>
        /// <seealso cref="XRCUnity.entityUpdatedUnityEvent"/>
        public void EntityUpdatingEventManager()
        {
            IEntityParameters evParams = XRCUnity.entityUpdatedEventQueue.Dequeue();

            collaborationManager.UpdateRemoteEntity(evParams.SerializedEntity);
        }

        /// <summary>
        /// Called by an entity edited event trigger from the XRCUnity script.
        /// </summary>
        /// <seealso cref="XRCUnity.entityEditedUnityEvent"/>
        public void EntityEditingEventManager()
        {
            IEntityParameters evParams = XRCUnity.entityReinitializedEventQueue.Dequeue();

            Debug.LogWarning("[" + ClassName + "] Entity editing not yet implemented.");
        }

        /// <summary>
        /// Called by an session joined event trigger from the XRCUnity script.
        /// </summary>
        /// <seealso cref="XRCUnity.sessionJoinedUnityEvent"/>
        public void SessionJoinedEventManager()
        {
            IEntityParameters[] sessionEntities = XRCUnity.GetAllSessionEntities();
            if (sessionEntities != null)
            {
                // Note: Must call InitializeSessionUsers first so that user components can get
                // added to user in call to InitializeSessionEntities
                InitializeSessionUsers(sessionEntities);
                InitializeSessionEntities(sessionEntities);
            }

            // Add the local user components, which couldn't be done until we joined
            IUser localUser = collaborationManager.GetLocalUser();
            if (localUser != null)
            {
                AddUserComponentsToSession(localUser);
            }

            // Refresh the collaboration synchronization
            collaborationManager.RefreshSynchronization();
        }

        /// <summary>
        /// Called by an session participant added event trigger from the XRCUnity script.
        /// </summary>
        /// <seealso cref="XRCUnity.sessionParticipantAddedUnityEvent"/>
        public void SessionParticipantAddedEventManager()
        {
            IUserParameters userParameters = XRCUnity.asParticipantAddedEventQueue.Dequeue();
            if (userParameters != null)
            {
                collaborationManager.CreateRemoteUser(userParameters.SerializedEntity);
            }
        }

        /// <summary>
        /// Called by an session participant resynch event trigger from the XRCUnity script.
        /// </summary>
        /// <seealso cref="XRCUnity.sessionParticipantResyncedUnityEvent"/>
        public void SessionParticipantResyncedEventManager()
        {
            IUserParameters sessParams = XRCUnity.asParticipantResyncedEventQueue.Dequeue();
            if (sessParams != null)
            {
                // TODO: Not implemented
            }
        }

        /// <summary>
        /// Called by an session participant deleted event trigger from the XRCUnity script.
        /// </summary>
        /// <seealso cref="XRCUnity.sessionParticipantDeletedUnityEvent"/>
        public void SessionParticipantDeletedEventManager()
        {
            IUserParameters userParameters = XRCUnity.asParticipantAddedEventQueue.Dequeue();
            if (userParameters != null)
            {
                collaborationManager.DeleteRemoteUser(userParameters.SerializedEntity);
            }
        }

        /// <summary>
        /// Called by a remote session added event trigger from the XRCUnity script.
        /// </summary>
        /// <seealso cref="XRCUnity.remoteSessionAddedUnityEvent"/>
        public void RemoteSessionAddedEventManager()
        {
            RemoteSessionEventParameters sessParams = XRCUnity.rsAddedEventQueue.Dequeue();
            if (sessParams != null)
            {
                // TODO: Not implemented
            }
        }

        /// <summary>
        /// Called by a remote session updated event trigger from the XRCUnity script.
        /// </summary>
        /// <seealso cref="XRCUnity.remoteSessionUpdatedUnityEvent"/>
        public void RemoteSessionUpdatedEventManager()
        {
            RemoteSessionEventParameters sessParams = XRCUnity.rsUpdatedEventQueue.Dequeue();
            if (sessParams != null)
            {
                // TODO: Not implemented
            }
        }

        /// <summary>
        /// Called by a remote session deleted event trigger from the XRCUnity script.
        /// </summary>
        /// <seealso cref="XRCUnity.remoteSessionDeletedUnityEvent"/>
        public void RemoteSessionDeletedEventManager()
        {
            RemoteSessionEventParameters sessParams = XRCUnity.rsDeletedEventQueue.Dequeue();
            if (sessParams != null)
            {
                // TODO: Not implemented
            }
        }
        #endregion XRC Event Handlers

        public void InitializeSessionUsers(IEntityParameters[] sessionEntityParameters)
        {
            // Remove existing session users
            collaborationManager.DeleteRemoteUsers();

            // Add all users
            if (sessionEntityParameters != null)
            {
                foreach (IEntityParameters entityParameters in sessionEntityParameters)
                {
                    if (entityParameters is IUserParameters)
                    {
                        collaborationManager.CreateRemoteUser((entityParameters as IUserParameters).SerializedEntity);
                    }
                }
            }
        }

        public void InitializeSessionEntities(IEntityParameters[] sessionEntityParameters)
        {
            if (sessionEntityParameters != null)
            {
                foreach (IEntityParameters entityParameters in sessionEntityParameters)
                {
                    if (entityParameters.Type == EntityType.user)
                    {
                        // Skip user entities
                        continue;
                    }

                    XRCUnity.entityCreatedEventQueue.Enqueue(entityParameters);
                    EntityCreationEventManager();
                }
            }
        }

        public static void AddUserComponentsToSession(IUser user)
        {
            // Add the head to the session
            if (user.Head != null)
            {
                var serializedHead = user.Head.CreateSerializedType();
                user.Head.Serialize(serializedHead);
                XRCUnity.AddSessionEntity(serializedHead, user);
            }

            // Add the torso to the session
            if (user.Torso != null)
            {
                var serializedTorso = user.Torso.CreateSerializedType();
                user.Torso.Serialize(serializedTorso);
                XRCUnity.AddSessionEntity(serializedTorso, user);
            }

            // Add the hands to the session
            foreach (IUserHand userHand in user.Hands)
            {
                // Add the hand to the session
                var serializedHand = userHand.CreateSerializedType();
                userHand.Serialize(serializedHand);
                XRCUnity.AddSessionEntity(serializedHand, user);

                // Add the hand controller to the session
                if (userHand.Controller != null)
                {
                    var serializedHandController = userHand.Controller.CreateSerializedType();
                    userHand.Controller.Serialize(serializedHandController);
                    XRCUnity.AddSessionEntity(serializedHandController, userHand);
                }

                // Add the hand pointer to the session
                if (userHand.Pointer != null)
                {
                    var serializedHandPointer = userHand.Pointer.CreateSerializedType();
                    userHand.Pointer.Serialize(serializedHandPointer);
                    XRCUnity.AddSessionEntity(serializedHandPointer, userHand);
                }
            }
        }

        public IEntityParameters[] GetSessionUsers()
        {
            IEntityParameters[] result = null;

            IEntityParameters[] sessionEntities = XRCUnity.GetAllSessionEntities();
            if (sessionEntities != null)
            {
                List<IEntityParameters> sessionUsers = new List<IEntityParameters>();
                foreach (IEntityParameters sessionEntity in sessionEntities)
                {
                    if (sessionEntity.Type == EntityType.user)
                    {
                        sessionUsers.Add(sessionEntity);
                    }
                }
                result = sessionUsers.ToArray();
            }

            return result;
        }

#region TypeDeserialization0
        public static string ToCSV<T>(T[] input)
            where T : Vector3Type
        {
            string output = "";
            foreach (T v3 in input)
            {
                output = output + v3.X + "," + v3.Y + "," + v3.Z + ";";
            }

            return output;
        }

        public static T[] FromCSV<T>(string input)
            where T : Vector3Type, new()
        {
            List<T> output = new List<T>();
            foreach (string coordPoint in input.Split(new char[] { ';' }))
            {
                string[] coordVals = coordPoint.Split(new char[] { ',' });
                if (coordVals.Length != 3)
                {
                    continue;
                }
                else
                {
                    output.Add(new T()
                    {
                        X = float.Parse(coordVals[0]),
                        Y = float.Parse(coordVals[1]),
                        Z = float.Parse(coordVals[2])
                    });
                }
            }

            return output.ToArray();
        }

        public static EntityType EntityTypeFromInt(int intVal)
        {
            switch (intVal)
            {
                case 0:
                    return EntityType.entity;

                case 1:
                    return EntityType.user;

                default:
                    Debug.LogError("[XRCManager->EntityTypeFromInt] Invalid Integer Value.");
                    return EntityType.entity;
            }
        }
        #endregion

#region TypeMapping
        public static XR.XRC.ReferenceSpaceType ReferenceSpaceToXRCReferenceSpace(Schema.v0_9.ReferenceSpaceType referenceSpace)
        {
            switch (referenceSpace)
            {
                case Schema.v0_9.ReferenceSpaceType.Relative:
                    return XR.XRC.ReferenceSpaceType.relative;

                case Schema.v0_9.ReferenceSpaceType.Global:
                default:
                    return XR.XRC.ReferenceSpaceType.global;
            }
        }

        public static Schema.v0_9.ReferenceSpaceType XRCReferenceSpaceToReferenceSpace(XR.XRC.ReferenceSpaceType referenceSpace)
        {
            switch (referenceSpace)
            {
                case XR.XRC.ReferenceSpaceType.relative:
                    return Schema.v0_9.ReferenceSpaceType.Relative;

                case XR.XRC.ReferenceSpaceType.global:
                default:
                    return Schema.v0_9.ReferenceSpaceType.Global;
            }
        }

        public static UnitType LengthUnitsToXRCUnits(LengthUnitType units)
        {
            switch (units)
            {
                case LengthUnitType.Millimeter:
                    return UnitType.millimeter;

                case LengthUnitType.Centimeter:
                    return UnitType.centimeter;

                case LengthUnitType.Kilometer:
                    return UnitType.kilometer;

                case LengthUnitType.Inch:
                    return UnitType.inch;

                case LengthUnitType.Foot:
                    return UnitType.foot;

                case LengthUnitType.Yard:
                    return UnitType.yard;

                case LengthUnitType.Mile:
                    return UnitType.mile;

                case LengthUnitType.Meter:
                default:
                    return UnitType.meter;
            }
        }

        public static LengthUnitType XRCUnitsToLengthUnits(UnitType units)
        {
            switch (units)
            {
                case UnitType.millimeter:
                    return LengthUnitType.Millimeter;

                case UnitType.centimeter:
                    return LengthUnitType.Centimeter;

                case UnitType.kilometer:
                    return LengthUnitType.Kilometer;

                case UnitType.inch:
                    return LengthUnitType.Inch;

                case UnitType.foot:
                    return LengthUnitType.Foot;

                case UnitType.yard:
                    return LengthUnitType.Yard;

                case UnitType.mile:
                    return LengthUnitType.Mile;

                case UnitType.meter:
                default:
                    return LengthUnitType.Meter;
            }
        }

        public static ConnectionTypes GMSECToXRCConnType(GMSECDefs.ConnectionTypes gmsecType)
        {
            switch (gmsecType)
            {
                case GMSECDefs.ConnectionTypes.amq383:
                    return ConnectionTypes.amq383;

                case GMSECDefs.ConnectionTypes.amq384:
                    return ConnectionTypes.amq384;

                case GMSECDefs.ConnectionTypes.bolt:
                    return ConnectionTypes.bolt;

                case GMSECDefs.ConnectionTypes.mb:
                    return ConnectionTypes.mb;

                case GMSECDefs.ConnectionTypes.ws71:
                    return ConnectionTypes.ws71;

                case GMSECDefs.ConnectionTypes.ws75:
                    return ConnectionTypes.ws75;

                case GMSECDefs.ConnectionTypes.ws80:
                    return ConnectionTypes.ws80;

                default:
                    return ConnectionTypes.bolt;
            }
        }

        public static GMSECDefs.ConnectionTypes XRCToGMSECConnType(ConnectionTypes gmsecType)
        {
            switch (gmsecType)
            {
                case ConnectionTypes.amq383:
                    return GMSECDefs.ConnectionTypes.amq383;

                case ConnectionTypes.amq384:
                    return GMSECDefs.ConnectionTypes.amq384;

                case ConnectionTypes.bolt:
                    return GMSECDefs.ConnectionTypes.bolt;

                case ConnectionTypes.mb:
                    return GMSECDefs.ConnectionTypes.mb;

                case ConnectionTypes.ws71:
                    return GMSECDefs.ConnectionTypes.ws71;

                case ConnectionTypes.ws75:
                    return GMSECDefs.ConnectionTypes.ws75;

                case ConnectionTypes.ws80:
                    return GMSECDefs.ConnectionTypes.ws80;

                default:
                    return GMSECDefs.ConnectionTypes.bolt;
            }
        }

        #endregion
    }
}