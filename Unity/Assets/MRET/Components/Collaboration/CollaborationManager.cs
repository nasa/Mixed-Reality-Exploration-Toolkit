// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.XRC;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Note;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.User;
using GOV.NASA.GSFC.XR.MRET.Integrations.GMSEC;
using GOV.NASA.GSFC.XR.MRET.Integrations.GMSEC.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Integrations.XRC;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    public class CollaborationManager : MRETManager<CollaborationManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(CollaborationManager);

        public enum LabelColor { red = 1, yellow = 2, blue = 3, black = 4, green = 5, purple = 6 }

        public enum EngineType { XRC, LegacyGMSEC };

        [Tooltip("The middleware type to use.")]
        public GMSECDefs.ConnectionTypes connectionType = GMSECDefs.ConnectionTypes.bolt;

        [Tooltip("The GMSEC server address and port number.")]
        public string server = "localhost:9100";

        [Tooltip("The mission name to use.")]
        public string missionName = "UNSET";

        [Tooltip("The satellite name to use.")]
        public string satName = "UNSET";

        public SynchronizationManagerDeprecated synchManager;
        public SessionMasterNode masterNode;
        public SessionSlaveNode slaveNode;
        public ModeNavigator modeNavigator;
        public EngineType engineType = EngineType.XRC;
        public GameObject avatarPrefab, vrAvatarPrefab, desktopAvatarPrefab;
        public GameObject userContainer;

        private SessionAdvertiser sessionAdvertiser;
        private XRCManager xrcManager;

        public bool CollaborationEnabled => xrcManager.IsSessionActive;

        public void RefreshSynchronization()
        {
            // Enable collaboration for all session entities
            bool sessionActive = CollaborationEnabled;
            foreach (IIdentifiable identifiable in MRET.UuidRegistry.IdentifiableObjects)
            {
                identifiable.SynchronizationEnabled = sessionActive;
            }
        }

        #region MRETBehaviour
        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Make sure we have a user container
            if (userContainer == null)
            {
                userContainer = new GameObject("Users");
                userContainer.transform.parent = transform;
            }
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            base.MRETOnDestroy();

            // Stop the collaboration engine
            if (IsEngineStarted)
            {
                StopEngine();
            }
        }
        #endregion MRETBehaviour

        /// <seealso cref="MRETManager{M}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();

            Log("Initializing XRC Manager...", nameof(Initialize));
            if (xrcManager == null)
            {
                xrcManager = GetComponent<XRCManager>();
                if (xrcManager == null)
                {
                    LogError("Fatal Error. Unable to initialize XRC manager. Aborting...", nameof(Initialize));
                    MRET.Quit();
                }
            }
            xrcManager.Initialize();
            Log("XRC Manager initialized.", nameof(Initialize));
        }

        #region Engine
        /// <summary>
        /// Indicates if a session is active
        /// </summary>
        public bool IsEngineStarted => xrcManager.IsXRCStarted;

        /// <summary>
        /// Starts the collaboration engine
        /// </summary>
        /// <returns>A boolean value indicating success</returns>
        public bool StartEngine() => xrcManager.StartXRC();

        /// <summary>
        /// Stops the collaboration engine
        /// </summary>
        /// <returns>A boolean value indicating success</returns>
        public bool StopEngine()
        {
            if (IsSessionActive)
            {
                if (IsMaster)
                {
                    EndSession();
                }
                else
                {
                    LeaveSession();
                }
            }
            return xrcManager.StopXRC();
        }

        /// <summary>
        /// Initializes the engine
        /// </summary>
        /// <param name="server">The server name/IP</param>
        /// <param name="port">The port</param>
        /// <param name="projectName">Name of the project</param>
        /// <param name="groupName">Name of the group</param>
        /// <param name="sessionName">Name of the session</param>
        public void InitializeEngine(string server, int port,
            string projectName = "XRC", string groupName = "MRET", string sessionName = "UNSET")
        {
            xrcManager.InitializeXRC(server, port,
                XRCManager.GMSECToXRCConnType(connectionType),
                missionName, satName, projectName, groupName, sessionName);
        }

        /// <summary>
        /// Indicates if a session is active
        /// </summary>
        public bool IsSessionActive => xrcManager.IsSessionActive;

        /// <summary>
        /// Indicates if this is a master session
        /// </summary>
        public bool IsMaster => xrcManager.IsMaster;

        /// <summary>
        /// Starts a collaboration session
        /// </summary>
        /// <param name="user">The local <code>IUser</code></param>
        /// <returns>A boolean value indicating success</returns>
        public bool StartSession(IUser user)
        {
            bool started = xrcManager.StartSession(user);
            RefreshSynchronization();
            return started;
        }

        /// <summary>
        /// Ends a collaboration session
        /// </summary>
        /// <returns>A boolean value indicating success</returns>
        public bool EndSession()
        {
            bool ended = xrcManager.EndSession();
            RefreshSynchronization();
            return ended;
        }

        /// <summary>
        /// Starts a collaboration session
        /// </summary>
        /// <param name="sessionId">The ID of the session to join</param>
        /// <param name="user">The local <code>IUser</code></param>
        /// <returns>A boolean value indicating success</returns>
        public bool JoinSession(string sessionId, IUser user) => xrcManager.JoinSession(sessionId, user);

        /// <summary>
        /// leaves a collaboration session
        /// </summary>
        /// <returns>A boolean value indicating success</returns>
        public bool LeaveSession()
        {
            bool leftSession = xrcManager.LeaveSession();
            RefreshSynchronization();
            return leftSession;
        }

        /// <summary>
        /// Gets the remote sessions
        /// </summary>
        /// <returns>An array of <code>IRemoteSession</code> interfaces describing the remote sessions</returns>
        public IRemoteSession[] GetRemoteSessions() => xrcManager.GetRemoteSessions();
        #endregion Engine

        /// <summary>
        /// Queries the current session for the supplied entity
        /// </summary>
        /// <param name="entity">The <code>IIdentifiable</code> to query</param>
        /// <returns>A boolean value indicating whether or not the entity exists in the session</returns>
        public bool SessionEntityExists(IIdentifiable entity) => xrcManager.SessionEntityExists(entity);

        /// <summary>
        /// Adds an entity to the session.
        /// </summary>
        /// <param name="entity">The <code>IIdentifiable</code> to add</param>
        /// <param name="parent">The entity parent <code>IIdentifiable</code></param>
        /// <returns>An indicator the entity was added to the session</returns>
        public bool AddSessionEntity(IIdentifiable entity, IIdentifiable parent) => xrcManager.AddSessionEntity(entity, parent);

        /// <summary>
        /// Removes an entity from the session.
        /// </summary>
        /// <param name="entity">The <code>IIdentifiable</code> to remove</param>
        /// <returns>An indicator the entity was removed from the session</returns>
        public bool RemoveSessionEntity(IIdentifiable entity) => xrcManager.RemoveSessionEntity(entity);

        /// <summary>
        /// Updates an entity in the session.
        /// </summary>
        /// <param name="entity">The <code>IIdentifiable</code> to update</param>
        /// <param name="parent">The entity parent <code>IIdentifiable</code></param>
        /// <returns>An indicator the entity was updated in the session</returns>
        public bool UpdateSessionEntity(IIdentifiable entity, IIdentifiable parent) => xrcManager.UpdateSessionEntity(entity, parent);

        /// <summary>
        /// Updates an entity parent in the session.
        /// </summary>
        /// <param name="entity">The <code>IIdentifiable</code> to update</param>
        /// <param name="parent">The entity parent <code>IIdentifiable</code></param>
        /// <returns>An indicator the entity was updated in the session</returns>
        public bool UpdateSessionEntityParent(ISceneObject entity, IIdentifiable parent) =>
            xrcManager.UpdateSessionEntityParent(entity, parent);

        /// <summary>
        /// Updates an entity transform in the session.
        /// </summary>
        /// <param name="entity">The <code>IIdentifiable</code> to update</param>
        /// <returns>An indicator the entity transform was updated in the session</returns>
        public bool UpdateSessionEntityTransform(ISceneObject entity) =>
            xrcManager.UpdateSessionEntityTransform(entity);

        /// <summary>
        /// Updates an entity position in the session.
        /// </summary>
        /// <param name="entity">The <code>IIdentifiable</code> to update</param>
        /// <returns>An indicator the entity position was updated in the session</returns>
        public bool UpdateSessionEntityPosition(ISceneObject entity) =>
            xrcManager.UpdateSessionEntityPosition(entity);

        /// <summary>
        /// Updates an entity scale in the session.
        /// </summary>
        /// <param name="entity">The <code>IIdentifiable</code> to update</param>
        /// <returns>An indicator the entity scale was updated in the session</returns>
        public bool UpdateSessionEntityScale(ISceneObject entity) =>
            xrcManager.UpdateSessionEntityScale(entity);

        /// <summary>
        /// Updates an entity rotation in the session.
        /// </summary>
        /// <param name="entity">The <code>IIdentifiable</code> to update</param>
        /// <returns>An indicator the entity rotation was updated in the session</returns>
        public bool UpdateSessionEntityRotation(ISceneObject entity) =>
            XRCUnity.UpdateSessionEntityRotation(entity);

        public IIdentifiable LookupEntity(IdentifiableType serializedEntity)
        {
            IIdentifiable result = null;

            // Lookup the UUID
            try
            {
                Guid uuid = new Guid(serializedEntity.UUID);
                result = MRET.UuidRegistry.Lookup(serializedEntity.UUID);
            }
            catch (Exception)
            {
                // Assume it's not a valid GUID
            }

            return result;
        }

        #region CreateRemoteEntity
        /// <summary>
        /// Adds a text annotation from a remote client
        /// </summary>
        /// <param name="serializedEntity"></param>
        public void CreateRemoteEntity(TextAnnotationType serializedEntity)
        {
            // Make sure the UUID doesn't already exist
            IIdentifiable identifiable = LookupEntity(serializedEntity);
            if (identifiable == null)
            {
                // Use the action system to create the entity
                var action = new AddIdentifiableObjectAction(serializedEntity);
                action.PerformAction();
            }
            else
            {
                LogWarning("UUID clash. A registered UUID already exists for the entity UUID being created: " +
                    identifiable.uuid, nameof(CreateRemoteEntity));
            }
        }

        /// <summary>
        /// Adds a source annotation from a remote client
        /// </summary>
        /// <param name="serializedEntity"></param>
        public void CreateRemoteEntity(SourceAnnotationType serializedEntity)
        {
            // Make sure the UUID doesn't already exist
            IIdentifiable identifiable = LookupEntity(serializedEntity);
            if (identifiable == null)
            {
                // Use the action system to create the entity
                var action = new AddIdentifiableObjectAction(serializedEntity);
                action.PerformAction();
            }
            else
            {
                LogWarning("UUID clash. A registered UUID already exists for the entity UUID being created: " +
                    identifiable.uuid, nameof(CreateRemoteEntity));
            }
        }

        /// <summary>
        /// Adds a part from a remote client
        /// </summary>
        /// <param name="serializedEntity"></param>
        public void CreateRemoteEntity(PartType serializedEntity, string parentId)
        {
            // Make sure the UUID doesn't already exist
            IIdentifiable identifiable = LookupEntity(serializedEntity);
            if (identifiable == null)
            {
                // Get the parent reference. Don't assume it's UUID. It could be ID.
                IIdentifiable parent = MRET.UuidRegistry.Lookup(parentId);
                parentId = (parent != null) ? parent.id : "";

                // Use the action system to create the entity
                var action = new AddSceneObjectAction(serializedEntity, parentId);
                action.PerformAction();
            }
            else
            {
                LogWarning("UUID clash. A registered UUID already exists for the entity UUID being created: " +
                    identifiable.uuid, nameof(CreateRemoteEntity));
            }
        }

        /// <summary>
        /// Adds a note from a remote client
        /// </summary>
        /// <param name="serializedEntity"></param>
        /// <param name="parentId"></param>
        public void CreateRemoteEntity(NoteType serializedEntity, string parentId)
        {
            // Make sure the UUID doesn't already exist
            IIdentifiable identifiable = LookupEntity(serializedEntity);
            if (identifiable == null)
            {
                // Get the parent reference. Don't assume it's UUID. It could be ID.
                IIdentifiable parent = MRET.UuidRegistry.Lookup(parentId);
                parentId = (parent != null) ? parent.id : "";

                // Use the action system to create the entity
                var action = new AddSceneObjectAction(serializedEntity, parentId);
                action.PerformAction();
            }
            else
            {
                LogWarning("UUID clash. A registered UUID already exists for the entity UUID being created: " +
                    identifiable.uuid, nameof(CreateRemoteEntity));
            }
        }

        /// <summary>
        /// Adds a 2D drawing from a remote client
        /// </summary>
        /// <param name="serializedEntity"></param>
        /// <param name="parentId"></param>
        public void CreateRemoteEntity(Drawing2dType serializedEntity, string parentId)
        {
            // Make sure the UUID doesn't already exist
            IIdentifiable identifiable = LookupEntity(serializedEntity);
            if (identifiable == null)
            {
                // Get the parent reference. Don't assume it's UUID. It could be ID.
                IIdentifiable parent = MRET.UuidRegistry.Lookup(parentId);
                parentId = (parent != null) ? parent.id : "";
                if (parent is InteractableNote)
                {
                    // Use the action system to create the entity
                    var action = new AddDrawingToNoteAction(parentId, serializedEntity);
                    action.PerformAction();
                }
                else
                {
                    Debug.LogWarning("[" + ClassName + "] Note parent not valid for note drawing: " + parentId);
                }
            }
            else
            {
                LogWarning("UUID clash. A registered UUID already exists for the entity UUID being created: " +
                    identifiable.uuid, nameof(CreateRemoteEntity));
            }
        }

        /// <summary>
        /// Adds a 3D drawing from a remote client
        /// </summary>
        /// <param name="serializedEntity"></param>
        public void CreateRemoteEntity(Drawing3dType serializedEntity, string parentId)
        {
            // Make sure the UUID doesn't already exist
            IIdentifiable identifiable = LookupEntity(serializedEntity);
            if (identifiable == null)
            {
                IIdentifiable parent = MRET.UuidRegistry.Lookup(parentId);
                parentId = (parent != null) ? parent.id : "";

                // Use the action system to create the entity
                var action = new AddSceneObjectAction(serializedEntity, parentId);
                action.PerformAction();
            }
            else
            {
                LogWarning("UUID clash. A registered UUID already exists for the entity UUID being created: " +
                    identifiable.uuid, nameof(CreateRemoteEntity));
            }
        }

        /// <summary>
        /// Adds a physical scene object from a remote client
        /// </summary>
        /// <param name="serializedEntity"></param>
        public void CreateRemoteEntity(PhysicalSceneObjectType serializedEntity, string parentId)
        {
            // Make sure the UUID doesn't already exist
            IIdentifiable identifiable = LookupEntity(serializedEntity);
            if (identifiable == null)
            {
                // Get the parent reference. Don't assume it's UUID. It could be ID.
                IIdentifiable parent = MRET.UuidRegistry.Lookup(parentId);
                parentId = (parent != null) ? parent.id : "";

                // Use the action system to create the entity
                var action = new AddSceneObjectAction(serializedEntity, parentId);
                action.PerformAction();
            }
            else
            {
                LogWarning("UUID clash. A registered UUID already exists for the entity UUID being created: " +
                    identifiable.uuid, nameof(CreateRemoteEntity));
            }
        }

        /// <summary>
        /// Adds a interactable scene object from a remote client
        /// </summary>
        /// <param name="serializedEntity"></param>
        public void CreateRemoteEntity(InteractableSceneObjectType serializedEntity, string parentId)
        {
            // Make sure the UUID doesn't already exist
            IIdentifiable identifiable = LookupEntity(serializedEntity);
            if (identifiable == null)
            {
                // Get the parent reference. Don't assume it's UUID. It could be ID.
                IIdentifiable parent = MRET.UuidRegistry.Lookup(parentId);
                parentId = (parent != null) ? parent.id : "";

                // Use the action system to create the entity
                var action = new AddSceneObjectAction(serializedEntity, parentId);
                action.PerformAction();
            }
            else
            {
                LogWarning("UUID clash. A registered UUID already exists for the entity UUID being created: " +
                    identifiable.uuid, nameof(CreateRemoteEntity));
            }
        }

        /// <summary>
        /// Adds a scene object from a remote client
        /// </summary>
        /// <param name="serializedEntity"></param>
        public void CreateRemoteEntity(SceneObjectType serializedEntity, string parentId)
        {
            // Make sure the UUID doesn't already exist
            IIdentifiable identifiable = LookupEntity(serializedEntity);
            if (identifiable == null)
            {
                // Get the parent reference. Don't assume it's UUID. It could be ID.
                IIdentifiable parent = MRET.UuidRegistry.Lookup(parentId);
                parentId = (parent != null) ? parent.id : "";

                // Use the action system to create the entity
                var action = new AddSceneObjectAction(serializedEntity, parentId);
                action.PerformAction();
            }
            else
            {
                LogWarning("UUID clash. A registered UUID already exists for the entity UUID being created: " +
                    identifiable.uuid, nameof(CreateRemoteEntity));
            }
        }

        /// <summary>
        /// Adds a identifiable object from a remote client
        /// </summary>
        /// <param name="serializedEntity"></param>
        public void CreateRemoteEntity(IdentifiableType serializedEntity)
        {
            // Make sure the UUID doesn't already exist
            IIdentifiable identifiable = LookupEntity(serializedEntity);
            if (identifiable == null)
            {
                // Use the action system to create the entity
                var action = new AddIdentifiableObjectAction(serializedEntity);
                action.PerformAction();
            }
            else
            {
                LogWarning("UUID clash. A registered UUID already exists for the entity UUID being created: " +
                    identifiable.uuid, nameof(CreateRemoteEntity));
            }
        }

        /// <summary>
        /// Adds a user head from a remote client
        /// </summary>
        /// <param name="serializedEntity"></param>
        public void CreateRemoteUserHead(UserHeadType serializedEntity, string userId)
        {
            // Make sure the UUID doesn't already exist
            IIdentifiable identifiable = LookupEntity(serializedEntity);
            if (identifiable == null)
            {
                // Get the user reference
                IIdentifiable user = MRET.UuidRegistry.Lookup(userId);
                if ((user is IUser) && !(user as IUser).IsLocal)
                {
                    IUser remoteUser = user as IUser;
                    remoteUser.SynchronizeHead(serializedEntity);
                }
                else
                {
                    LogWarning("Invalid user reference for remote head: " + userId,
                        nameof(CreateRemoteUserHead));
                }
            }
            else
            {
                LogWarning("UUID clash. A registered UUID already exists for the entity UUID being created: " +
                    identifiable.uuid, nameof(CreateRemoteUserHead));
            }
        }

        /// <summary>
        /// Adds a user torso from a remote client
        /// </summary>
        /// <param name="serializedEntity"></param>
        public void CreateRemoteUserTorso(UserTorsoType serializedEntity, string userId)
        {
            // Make sure the UUID doesn't already exist
            IIdentifiable identifiable = LookupEntity(serializedEntity);
            if (identifiable == null)
            {
                // Get the user reference
                IIdentifiable user = MRET.UuidRegistry.Lookup(userId);
                if ((user is IUser) && !(user as IUser).IsLocal)
                {
                    IUser remoteUser = user as IUser;
                    remoteUser.SynchronizeTorso(serializedEntity);
                }
                else
                {
                    LogWarning("Invalid user reference for remote torso: " + userId,
                        nameof(CreateRemoteUserTorso));
                }
            }
            else
            {
                LogWarning("UUID clash. A registered UUID already exists for the entity UUID being created: " +
                    identifiable.uuid, nameof(CreateRemoteUserTorso));
            }
        }

        /// <summary>
        /// Adds a user hand from a remote client
        /// </summary>
        /// <param name="serializedEntity"></param>
        public void CreateRemoteUserHand(UserHandType serializedEntity, string userId)
        {
            // Make sure the UUID doesn't already exist
            IIdentifiable identifiable = LookupEntity(serializedEntity);
            if (identifiable == null)
            {
                // Get the user reference
                IIdentifiable user = MRET.UuidRegistry.Lookup(userId);
                if ((user is IUser) && !(user as IUser).IsLocal)
                {
                    IUser remoteUser = user as IUser;
                    remoteUser.SynchronizeHand(serializedEntity);
                }
                else
                {
                    LogWarning("Invalid user reference for remote hand: " + userId,
                        nameof(CreateRemoteUserHand));
                }
            }
            else
            {
                LogWarning("UUID clash. A registered UUID already exists for the entity UUID being created: " +
                    identifiable.uuid, nameof(CreateRemoteUserHand));
            }
        }

        /// <summary>
        /// Adds a user hand controller from a remote client
        /// </summary>
        /// <param name="serializedEntity"></param>
        public void CreateRemoteUserHandController(UserControllerType serializedEntity, string handId)
        {
            // Make sure the UUID doesn't already exist
            IIdentifiable identifiable = LookupEntity(serializedEntity);
            if (identifiable == null)
            {
                // Get the hand reference
                IIdentifiable hand = MRET.UuidRegistry.Lookup(handId);
                if ((hand is IUserHand) && !(hand as IUserHand).IsLocal)
                {
                    IUserHand remoteHand = hand as IUserHand;
                    remoteHand.SynchronizeController(serializedEntity);
                }
                else
                {
                    LogWarning("Invalid hand reference for remote hand controller: " + handId,
                        nameof(CreateRemoteUserHandController));
                }
            }
            else
            {
                LogWarning("UUID clash. A registered UUID already exists for the entity UUID being created: " +
                    identifiable.uuid, nameof(CreateRemoteUserHandController));
            }
        }

        /// <summary>
        /// Adds a user hand pointer from a remote client
        /// </summary>
        /// <param name="serializedEntity"></param>
        public void CreateRemoteUserHandPointer(UserPointerType serializedEntity, string handId)
        {
            // Make sure the UUID doesn't already exist
            IIdentifiable identifiable = LookupEntity(serializedEntity);
            if (identifiable == null)
            {
                // Get the hand reference
                IIdentifiable hand = MRET.UuidRegistry.Lookup(handId);
                if ((hand is IUserHand) && !(hand as IUserHand).IsLocal)
                {
                    IUserHand remoteHand = hand as IUserHand;
                    remoteHand.SynchronizePointer(serializedEntity);
                }
                else
                {
                    LogWarning("Invalid hand reference for remote hand pointer: " + handId,
                        nameof(CreateRemoteUserHandController));
                }
            }
            else
            {
                LogWarning("UUID clash. A registered UUID already exists for the entity UUID being created: " +
                    identifiable.uuid, nameof(CreateRemoteUserHandPointer));
            }
        }

        /// <summary>
        /// Adds a session user
        /// </summary>
        /// <param name="serializedUser"></param>
        public void CreateRemoteUser(UserType serializedUser)
        {
            // Make sure the UUID doesn't already exist
            IIdentifiable identifiable = LookupEntity(serializedUser);
            if (identifiable == null)
            {
                // Instantiate the user rig
                GameObject newUserRigGO = Instantiate(avatarPrefab, userContainer.transform);
                InputRig newUserRig = newUserRigGO.GetComponentInChildren<InputRig>();
                if (newUserRig != null)
                {
                    User user = newUserRig.GetComponent<User>();
                    if (user == null)
                    {
                        user = newUserRig.gameObject.AddComponent<User>();
                    }
                    user.Initialize(false, newUserRig);

                    // Action delegate on load
                    Action<bool, string> OnUserLoaded = (bool loaded, string message) =>
                    {
                        // Make sure the user loaded
                        if (!loaded)
                        {
                            LogError("A problem was encountered deserializing the remote user: " + serializedUser.Alias,
                                nameof(CreateRemoteUser));
                        }
                    };

                    // Synchronize the user
                    user.Synchronize(serializedUser, OnUserLoaded);
                }
                else
                {
                    LogWarning("User rig prefab does not contain an InputRig", nameof(CreateRemoteUser));
                }
            }
            else
            {
                LogWarning("UUID clash. A registered UUID already exists for the user UUID being created: " +
                    identifiable.uuid, nameof(CreateRemoteUser));
            }
        }
        #endregion CreateRemoteEntity

        #region DeleteRemoteEntity
        /// <summary>
        /// Deletes a remote session user
        /// </summary>
        /// <param name="serializedEntity"></param>
        public void DeleteRemoteEntity(IdentifiableType serializedEntity, string parentId)
        {
            // Make sure the UUID exists
            IIdentifiable identifiable = LookupEntity(serializedEntity);
            if (identifiable != null)
            {
                if (serializedEntity is Drawing2dType)
                {
                    // Get the parent reference. Don't assume it's UUID. It could be ID.
                    IIdentifiable parent = MRET.UuidRegistry.Lookup(parentId);
                    parentId = (parent != null) ? parent.id : "";

                    if (parent is InteractableNote)
                    {
                        var action = new DeleteDrawingFromNoteAction(parentId, serializedEntity as Drawing2dType);
                        action.PerformAction();
                    }
                    else
                    {
                        Debug.LogWarning("[" + ClassName + "] Note parent not valid for note drawing: " + parentId);
                    }
                }
                else
                {
                    var action = new DeleteIdentifiableObjectAction(serializedEntity);
                    action.PerformAction();
                }
            }
            else
            {
                LogWarning("Entity being deleted is not registered as a valid entity", nameof(DeleteRemoteEntity));
            }
        }

        /// <summary>
        /// Deletes a remote session user
        /// </summary>
        /// <param name="serializedUser"></param>
        public void DeleteRemoteUser(UserType serializedUser)
        {
            // Make sure the UUID exists
            IIdentifiable identifiable = LookupEntity(serializedUser);
            if (identifiable is IUser)
            {
                IUser user = identifiable as IUser;

                // Ignore the local user
                if (!user.IsLocal)
                {
                    DeleteRemoteUser(user);
                }
                else
                {
                    LogWarning("Attempt to delete local user ignored", nameof(DeleteRemoteUser));
                }
            }
            else
            {
                LogWarning("User being deleted is not registered as a valid user", nameof(DeleteRemoteUser));
            }
        }

        /// <summary>
        /// Deletes a remote session user
        /// </summary>
        /// <param name="user">The <code>IUser</code> to delete</param>
        public void DeleteRemoteUser(IUser user)
        {
            // Make sure the user exists
            if (user != null)
            {
                // Ignore the local user
                if (!user.IsLocal)
                {
                    Destroy(user.gameObject);
                }
                else
                {
                    LogWarning("Attempt to delete local user ignored", nameof(DeleteRemoteUser));
                }
            }
            else
            {
                LogWarning("Supplied user is null", nameof(DeleteRemoteUser));
            }
        }

        /// <summary>
        /// Deletes all the remote session users
        /// </summary>
        public void DeleteRemoteUsers()
        {
            foreach (IUser user in MRET.UuidRegistry.Users)
            {
                // Ignore the local user
                if (!user.IsLocal)
                {
                    Destroy(user.gameObject);
                }
            }
        }
        #endregion DeleteRemoteEntity

        #region UpdateRemoteEntity
        /// <summary>
        /// Updates a remote session entity
        /// </summary>
        /// <param name="serializedEntity"></param>
        public void UpdateRemoteEntity(IdentifiableType serializedEntity)
        {
            // Make sure the UUID exists
            IIdentifiable entity = LookupEntity(serializedEntity);
            if (entity != null)
            {
                // Synchronize the entity
                entity.Synchronize(serializedEntity);
            }
            else
            {
                LogWarning("Entity being updated is not registered as a valid entity", nameof(UpdateRemoteEntity));
            }
        }
        #endregion UpdateRemoteEntity

        /// <summary>
        /// Gets the local user from the list of session users.
        /// </summary>
        /// <returns>The <code>IUser</code> representing the first local user in the list of session users, or null</returns>
        public IUser GetLocalUser()
        {
            IUser result = null;

            foreach (IUser user in MRET.UuidRegistry.Users)
            {
                // Ignore the local user
                if (user.IsLocal)
                {
                    result = user;
                    break;
                }
            }

            return result;
        }

        public static string GetRandomColor()
        {
            System.Random rand = new System.Random();

            LabelColor color = (LabelColor)rand.Next(1, 6);
            return color.ToString();
        }

        public static LabelColor LabelColorFromString(string colorName)
        {
            switch (colorName)
            {
                case "red":
                    return LabelColor.red;

                case "yellow":
                    return LabelColor.yellow;

                case "blue":
                    return LabelColor.blue;

                case "black":
                    return LabelColor.black;

                case "green":
                    return LabelColor.green;

                case "purple":
                    return LabelColor.purple;

                default:
                    return LabelColor.black;
            }
        }

        public void EnterMasterMode(SessionInformation sessionInfo, string alias)
        {
            if (engineType == EngineType.LegacyGMSEC)
            {
                synchManager.enabled = true;
                slaveNode.enabled = false;
                masterNode.enabled = true;

                masterNode.StartRunning(sessionInfo, alias);

                sessionAdvertiser = gameObject.AddComponent<SessionAdvertiser>();
                synchManager.sessionAdvertiser = sessionAdvertiser;
                synchManager.masterNode = masterNode;
                sessionAdvertiser.sessionInformation = sessionInfo;
                sessionAdvertiser.connectionType = connectionType;
                sessionAdvertiser.server = server;
                sessionAdvertiser.missionName = missionName;
                sessionAdvertiser.satName = satName;
                sessionAdvertiser.Initialize();

                MRET.ProjectDeprecated.userAlias = alias;
                modeNavigator.OpenProject(sessionInfo.projectName, true);
            }
        }

        public void EnterSlaveMode(SessionInformation sessionInfo, string alias)
        {
            if (engineType == EngineType.LegacyGMSEC)
            {
                Destroy(sessionAdvertiser);

                synchManager.enabled = true;
                masterNode.enabled = false;
                slaveNode.enabled = true;

                slaveNode.Connect(sessionInfo, alias);

                MRET.ProjectDeprecated.userAlias = alias;
                modeNavigator.OpenProject(Application.dataPath + "/Projects/" + sessionInfo.projectName, true);
            }
        }
    }
}