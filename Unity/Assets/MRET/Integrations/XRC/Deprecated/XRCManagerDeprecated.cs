// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.XRC;
using GOV.NASA.GSFC.XR.Utilities;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_1;
using GOV.NASA.GSFC.XR.MRET.Integrations.GMSEC;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.XRC
{
    [Obsolete("Refer to " + nameof(GOV.NASA.GSFC.XR.MRET.Integrations.XRC.XRCManager))]

    public class XRCManagerDeprecated : MonoBehaviour
    {
        static string XRC_PACKAGE_NAME = "XRCEngine";

        public List<SynchronizedUserDeprecated> synchedUsers = new List<SynchronizedUserDeprecated>();

        public static readonly string OBJECTCATEGORY = "OBJECT",
            DRAWINGCATEGORY = "DRAWING", NOTECATEGORY = "NOTE", NOTEDRAWINGCATEGORY = "NOTEDRAWING",
            USERCATEGORY = "USER", LCONTROLLERCATEGORY = "USER.L", RCONTROLLERCATEGORY = "USER.R",
            LPOINTERCATEGORY = "USER.POINTER.L", RPOINTERCATEGORY = "USER.POINTER.R";
        
        private string currentServer;
        private int currentPort;
        private ConnectionTypes currentConnType;

        public CollaborationManager collaborationManager;
        public static XRCManagerDeprecated instance;

        public void Initialize()
        {
            instance = this;

            string xrcPath;

            // Get the XRC package path
            xrcPath = PackageLoader.GetPackagePath(Application.dataPath, XRC_PACKAGE_NAME);

            // Initialize the XRC DLL
            PackageLoader.InitializePackagePlugin(xrcPath);

            if (collaborationManager == null)
            {
                collaborationManager = FindObjectOfType<CollaborationManager>();
            }
        }

        public void StartXRC()
        {
            XRCUnityDeprecated.StartUp();
        }

        public void StopXRC()
        {
            XRCUnityDeprecated.ShutDown();
        }

        public void CleanUp()
        {
            // Delete all SychronizedUser scripts in the scene.
            foreach (SynchronizedUserDeprecated sU in FindObjectsOfType<SynchronizedUserDeprecated>())
            {
                Destroy(sU);
            }

            // Delete all SynchronizedUserController scripts in the scene.
            foreach (SynchronizedController sC in FindObjectsOfType<SynchronizedController>())
            {
                Destroy(sC);
            }

            // Delete all SynchronizedUserPointer scripts in the scene.
            foreach (SynchronizedPointer sP in FindObjectsOfType<SynchronizedPointer>())
            {
                Destroy(sP);
            }

            synchedUsers = new List<SynchronizedUserDeprecated>();
        }

        public void RecordAction(ProjectActionDeprecated actionPerformed)
        {
            if (!XRCUnityDeprecated.IsSessionActive)
            {
                return;
            }

            ActionType sAction = actionPerformed.Serialize();
            switch (sAction.Type)
            {
                case ActionTypeType.AddDrawing:
                    if (sAction.Drawing == null || string.IsNullOrEmpty(sAction.Drawing.Name)
                        || string.IsNullOrEmpty(sAction.Drawing.RenderType) || string.IsNullOrEmpty(sAction.Drawing.GUID)
                        || sAction.Drawing.Points == null)
                    {
                        return;
                    }
                    
                    string pointsStr = TypeConversionDeprecated.Vector3TypeToCSV(sAction.Drawing.Points);
                    byte[] pointsBytes = System.Text.Encoding.UTF8.GetBytes(pointsStr);
                    if (pointsBytes.Length > XRCUnityDeprecated.RESOURCEBUFSIZE)
                    {
                        System.Array.Resize(ref pointsBytes, XRCUnityDeprecated.RESOURCEBUFSIZE);
                    }
                    /*if (string.IsNullOrEmpty(pointsStr))
                    {
                        return;
                    }*/

                    XRCUnityDeprecated.AddSessionEntity(sAction.Drawing.Name, DRAWINGCATEGORY, sAction.Drawing.RenderType,
                        null, sAction.Drawing.GUID, null, null, pointsBytes, null, null,
                        Vector3.zero, Quaternion.identity, new Vector3(sAction.Drawing.Width, 0, 0),
                        TypeConversionDeprecated.LDToXRCUnits(sAction.Drawing.DesiredUnits),
                        UnitType.unitless, UnitType.meter);
                    break;

                case ActionTypeType.AddNote:
                    if (sAction.NoteName == null || sAction.Note == null || sAction.Position == null || sAction.Rotation == null)
                    {
                        return;
                    }

                    XRCUnityDeprecated.AddSessionEntity(sAction.NoteName, NOTECATEGORY, null, null, sAction.Note.GUID, null, null, null,
                        sAction.Note.Title, sAction.Note.Details,
                        DeserializeVector3(sAction.Position), DeserializeQuaternion(sAction.Rotation),
                        new Vector3(1, 1, 1), UnitType.meter, UnitType.degrees, UnitType.meter);
                    break;

                case ActionTypeType.AddNoteDrawing:

                    break;

                case ActionTypeType.AddObject:
                    if (sAction.Part == null || sAction.Part.PartName == null || sAction.Part.AssetBundle == null
                        || sAction.Position == null || sAction.Rotation == null || sAction.Scale == null)
                    {
                        return;
                    }

                    XRCUnityDeprecated.AddSessionEntity(sAction.Part.PartName, OBJECTCATEGORY, null, sAction.Part.AssetBundle, sAction.UUID,
                        null, new InteractablePartDeprecated.InteractablePartSettings(sAction.Part.EnableInteraction,
                        sAction.Part.EnableCollisions, sAction.Part.EnableGravity), null, null, null,
                        DeserializeVector3(sAction.Position), DeserializeQuaternion(sAction.Rotation),
                        DeserializeVector3(sAction.Scale), UnitType.meter, UnitType.degrees, UnitType.meter);
                    break;

                case ActionTypeType.AddPointToLine:
                    break;

                case ActionTypeType.ChangeNoteState:
                    break;

                case ActionTypeType.ChangeNoteText:
                    break;

                case ActionTypeType.DeleteDrawing:
                    if (string.IsNullOrEmpty(sAction.DrawingName) || string.IsNullOrEmpty(sAction.UUID))
                    {
                        return;
                    }

                    XRCUnityDeprecated.RemoveSessionEntity(sAction.DrawingName, DRAWINGCATEGORY, sAction.UUID);
                    break;

                case ActionTypeType.DeleteNote:
                    if (sAction.NoteName == null)
                    {
                        return;
                    }

                    XRCUnityDeprecated.RemoveSessionEntity(sAction.NoteName, NOTECATEGORY, sAction.UUID);
                    break;

                case ActionTypeType.DeleteNoteDrawing:
                    break;

                case ActionTypeType.DeleteObject:
                    if (sAction.PartName == null)
                    {
                        return;
                    }

                    XRCUnityDeprecated.RemoveSessionEntity(sAction.PartName, OBJECTCATEGORY, sAction.UUID);
                    break;

                case ActionTypeType.DeletePointFromLine:
                    break;

                case ActionTypeType.MoveNote:
                    if (sAction.NoteName == null || sAction.Position == null || sAction.Rotation == null)
                    {
                        return;
                    }

                    XRCUnityDeprecated.UpdateEntityPosition(sAction.NoteName, NOTECATEGORY, DeserializeVector3(sAction.Position), sAction.UUID, null, UnitType.meter);
                    XRCUnityDeprecated.UpdateEntityRotation(sAction.NoteName, NOTECATEGORY, DeserializeQuaternion(sAction.Rotation), sAction.UUID, null, UnitType.degrees);
                    break;

                case ActionTypeType.MoveObject:
                    if (sAction.PartName == null || sAction.Position == null || sAction.Rotation == null)
                    {
                        return;
                    }

                    XRCUnityDeprecated.UpdateEntityPosition(sAction.PartName, OBJECTCATEGORY, DeserializeVector3(sAction.Position), sAction.UUID, null, UnitType.meter);
                    XRCUnityDeprecated.UpdateEntityRotation(sAction.PartName, OBJECTCATEGORY, DeserializeQuaternion(sAction.Rotation), sAction.UUID, null, UnitType.degrees);
                    break;

                case ActionTypeType.SetFinalIKPos:
                    break;

                case ActionTypeType.SetMatlabIKPos:
                    break;

                case ActionTypeType.Unset:
                default:
                    Debug.LogError("[XRCManager->RecordAction] Unknown project action type.");
                    break;
            }
        }

        public void EntityCreationEventManager()
        {
            EntityEventParametersDeprecated evParams = XRCUnityDeprecated.entityCreatedEventQueue.Dequeue();
            if (evParams != null)
            {
                if (evParams.category == OBJECTCATEGORY)
                {
                    PartType pt = new PartType();
                    pt.PartName = evParams.tag;
                    pt.AssetBundle = evParams.bundle;
                    pt.GUID = evParams.uuid;
                    ProjectActionDeprecated actionToPerform = ProjectActionDeprecated.AddObjectAction(pt,
                        evParams.pos.ToVector3(), evParams.rot.ToQuaternion(), evParams.scale.ToVector3(),
                        evParams.settings);
                    actionToPerform.PerformAction();
                }
                else if (evParams.category == DRAWINGCATEGORY)
                {
                    DrawingType drw = new DrawingType();
                    drw.DesiredUnits = TypeConversionDeprecated.XRCToLDUnits(evParams.posUnits);
                    drw.GUID = evParams.uuid;
                    drw.Name = evParams.tag;
                    drw.Points = TypeConversionDeprecated.CSVToVector3Type(System.Text.Encoding.UTF8.GetString(evParams.resource));
                    drw.RenderType = evParams.subcategory;
                    drw.Width = (float) evParams.scale.x;
                    ProjectActionDeprecated actionToPerform = ProjectActionDeprecated.AddDrawingAction(drw);
                    actionToPerform.PerformAction();
                }
                else if (evParams.category == NOTECATEGORY)
                {
                    ProjectActionDeprecated actionToPerform = ProjectActionDeprecated.AddNoteAction(new NoteType(),
                        evParams.tag, evParams.pos.ToVector3(), evParams.rot.ToQuaternion(), evParams.uuid);
                    actionToPerform.PerformAction();
                    actionToPerform = ProjectActionDeprecated.ChangeNoteTextAction(
                        evParams.tag, evParams.title, evParams.text);
                    actionToPerform.PerformAction();
                }
                else if (evParams.category == NOTEDRAWINGCATEGORY)
                {
                    ProjectActionDeprecated actionToPerform = ProjectActionDeprecated.AddNoteDrawingAction(evParams.tag, evParams.tag,
                        TypeConversionDeprecated.CSVToVector3Type(System.Text.Encoding.UTF8.GetString(evParams.resource)));
                    actionToPerform.PerformAction();
                }
                else if (evParams.category == LCONTROLLERCATEGORY)
                {
                    if (!string.IsNullOrEmpty(evParams.uuid))
                    {
                        SynchronizedUserDeprecated userToAddTo = synchedUsers.Find(
                            x => x.uuid == System.Guid.Parse(evParams.parentUUID));
                        if (userToAddTo)
                        {
                            userToAddTo.leftController.uuid = System.Guid.Parse(evParams.uuid);
                        }
                    }
                }
                else if (evParams.category == RCONTROLLERCATEGORY)
                {
                    if (!string.IsNullOrEmpty(evParams.uuid))
                    {
                        SynchronizedUserDeprecated userToAddTo = synchedUsers.Find(
                            x => x.uuid == System.Guid.Parse(evParams.parentUUID));
                        if (userToAddTo)
                        {
                            userToAddTo.rightController.uuid = System.Guid.Parse(evParams.uuid);
                        }
                    }
                }
                else if (evParams.category == LPOINTERCATEGORY)
                {
                    if (!string.IsNullOrEmpty(evParams.uuid))
                    {
                        SynchronizedUserDeprecated userToAddTo = synchedUsers.Find(
                            x => x.uuid == System.Guid.Parse(evParams.parentUUID));
                        if (userToAddTo)
                        {
                            userToAddTo.leftController.pointer.uuid = System.Guid.Parse(evParams.uuid);
                        }
                    }
                }
                else if (evParams.category == RPOINTERCATEGORY)
                {
                    if (!string.IsNullOrEmpty(evParams.uuid))
                    {
                        SynchronizedUserDeprecated userToAddTo = synchedUsers.Find(
                            x => x.uuid == System.Guid.Parse(evParams.parentUUID));
                        if (userToAddTo)
                        {
                            userToAddTo.rightController.pointer.uuid = System.Guid.Parse(evParams.uuid);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("[XRCManager] Unknown entity creation event category.");
                }
            }
        }

        public void EntityDestructionEventManager()
        {
            EntityEventParametersDeprecated evParams = XRCUnityDeprecated.entityDestroyedEventQueue.Dequeue();
            if (evParams != null)
            {
                if (evParams.category == OBJECTCATEGORY)
                {
                    ProjectActionDeprecated actionToPerform = ProjectActionDeprecated.DeleteObjectAction(evParams.tag, evParams.uuid);
                    actionToPerform.PerformAction();

                }
                else if (evParams.category == DRAWINGCATEGORY)
                {
                    ProjectActionDeprecated actionToPerform = ProjectActionDeprecated.DeleteDrawingAction(evParams.tag, evParams.uuid);
                    actionToPerform.PerformAction();
                }
                else if (evParams.category == NOTECATEGORY)
                {
                    ProjectActionDeprecated actionToPerform = ProjectActionDeprecated.DeleteNoteAction(evParams.tag, evParams.uuid);
                    actionToPerform.PerformAction();
                }
                else
                {
                    Debug.LogWarning("[XRCManager] Unknown entity deletion event category.");
                }
            }
        }

        public void EntityReinitializationEventManager()
        {
            EntityEventParametersDeprecated evParams = XRCUnityDeprecated.entityReinitializedEventQueue.Dequeue();
            if (evParams != null)
            {
                Debug.LogWarning("[XRCManager] Entity reinitialization not yet implemented.");
            }
        }

        public void EntityUpdatingEventManager()
        {
            EntityEventParametersDeprecated evParams = XRCUnityDeprecated.entityUpdatedEventQueue.Dequeue();
            if (evParams != null)
            {
                if (evParams.category == OBJECTCATEGORY)
                {
                    ProjectActionDeprecated actionToPerform = ProjectActionDeprecated.MoveObjectAction(
                        evParams.tag, evParams.pos.ToVector3(), evParams.rot.ToQuaternion(), evParams.uuid);
                    actionToPerform.PerformAction();
                    actionToPerform = ProjectActionDeprecated.SetParentAction(evParams.tag, evParams.parentUUID, evParams.uuid);
                    actionToPerform.PerformAction();
                }
                else if (evParams.category == NOTECATEGORY)
                {
                    ProjectActionDeprecated actionToPerform = ProjectActionDeprecated.MoveNoteAction(
                        evParams.tag, evParams.pos.ToVector3(), evParams.rot.ToQuaternion());
                    actionToPerform.PerformAction();
                    actionToPerform = ProjectActionDeprecated.ChangeNoteTextAction(
                        evParams.tag, evParams.title, evParams.text);
                    actionToPerform.PerformAction();
                }
                else if (evParams.category == USERCATEGORY)
                {
                    // TODO: Should I make these actions?
                    if (!string.IsNullOrEmpty(evParams.uuid)
                        && evParams.pos != null && evParams.rot != null && evParams.scale != null)
                    {
                        SynchronizedUserDeprecated userToMove = synchedUsers.Find(
                            x => x.uuid == System.Guid.Parse(evParams.uuid));
                        if (userToMove)
                        {
                            userToMove.transform.position = evParams.pos.ToVector3();
                            userToMove.transform.rotation = evParams.rot.ToQuaternion();
                            userToMove.transform.localScale = evParams.scale.ToVector3();
                        }
                    }
                }
                else if (evParams.category == LCONTROLLERCATEGORY)
                {
                    // TODO: Should I make these actions?
                    if (!string.IsNullOrEmpty(evParams.uuid)
                        && evParams.pos != null && evParams.rot != null && evParams.scale != null)
                    {
                        SynchronizedUserDeprecated userToMove = synchedUsers.Find(
                            x => x.leftController.uuid == System.Guid.Parse(evParams.uuid));
                        if (userToMove)
                        {
                            userToMove.leftController.transform.position = evParams.pos.ToVector3();
                            userToMove.leftController.transform.rotation = evParams.rot.ToQuaternion();
                            userToMove.leftController.transform.localScale = evParams.scale.ToVector3();
                        }
                    }
                }
                else if (evParams.category == RCONTROLLERCATEGORY)
                {
                    // TODO: Should I make these actions?
                    if (!string.IsNullOrEmpty(evParams.uuid)
                        && evParams.pos != null && evParams.rot != null && evParams.scale != null)
                    {
                        // Needed for right controller
                        // since not all users have a right controller.
                        SynchronizedUserDeprecated userToMove = null;
                        foreach (SynchronizedUserDeprecated user in synchedUsers)
                        {
                            if (user.rightController)
                            {
                                if (user.rightController.uuid
                                    == System.Guid.Parse(evParams.uuid))
                                {
                                    userToMove = user;
                                }
                            }
                        }
                            
                        if (userToMove)
                        {
                            userToMove.rightController.transform.position = evParams.pos.ToVector3();
                            userToMove.rightController.transform.rotation = evParams.rot.ToQuaternion();
                            userToMove.rightController.transform.localScale = evParams.scale.ToVector3();
                        }
                    }
                }
                else if (evParams.category == LPOINTERCATEGORY)
                {
                    // TODO: Should I make these actions?
                    if (!string.IsNullOrEmpty(evParams.uuid)
                        && evParams.pos != null && evParams.rot != null && evParams.scale != null)
                    {
                        SynchronizedUserDeprecated userToMove = synchedUsers.Find(
                            x => x.leftController.pointer.uuid == System.Guid.Parse(evParams.uuid));
                        if (userToMove)
                        {
                            userToMove.leftController.pointer.SetPosition(evParams.pos.ToVector3());
                        }
                    }
                }
                else if (evParams.category == RPOINTERCATEGORY)
                {
                    // TODO: Should I make these actions?
                    if (!string.IsNullOrEmpty(evParams.uuid)
                        && evParams.pos != null && evParams.rot != null && evParams.scale != null)
                    {
                        // Needed for right controller
                        // since not all users have a right controller.
                        SynchronizedUserDeprecated userToMove = null;
                        foreach (SynchronizedUserDeprecated user in synchedUsers)
                        {
                            if (user.rightController)
                            {
                                if (user.rightController.pointer.uuid
                                    == System.Guid.Parse(evParams.uuid))
                                {
                                    userToMove = user;
                                }
                            }
                        }
                            
                        if (userToMove)
                        {
                            userToMove.rightController.pointer.SetPosition(evParams.pos.ToVector3());
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("[XRCManager] Unknown entity updating event category " + evParams.category + ".");
                }
            }
        }

        public void EntityEditingEventManager()
        {
            EntityEventParametersDeprecated evParams = XRCUnityDeprecated.entityEditedEventQueue.Dequeue();
            if (evParams != null)
            {
                Debug.LogError("[XRCManager] Entity editing not yet implemented.");
            }
        }

        public void SessionJoiningEventManager()
        {
            InitializeSessionEntities();
            InitializeSessionUsers();
            AddControllersToSession();
        }

        public void SessionParticipantAddedEventManager()
        {
            ActiveSessionEventParametersDeprecated sessParams = XRCUnityDeprecated.asParticipantAddedEventQueue.Dequeue();
            if (sessParams != null)
            {
                synchedUsers.Add(SynchronizedUserDeprecated.Create(sessParams.tag, "default",
                    (SynchronizedUserDeprecated.UserType) sessParams.type, sessParams.color, sessParams.id,
                    sessParams.lcID, sessParams.rcID, sessParams.lpID, sessParams.rpID, false));
            }
        }

        public void SessionParticipantResyncedEventManager()
        {
            ActiveSessionEventParametersDeprecated sessParams = XRCUnityDeprecated.asParticipantResyncedEventQueue.Dequeue();
            if (sessParams != null)
            {

            }
        }

        public void SessionParticipantDeletedEventManager()
        {
            ActiveSessionEventParametersDeprecated sessParams = XRCUnityDeprecated.asParticipantDeletedEventQueue.Dequeue();
            if (sessParams != null)
            {
                SynchronizedUserDeprecated removedParticipant = synchedUsers.Find(x => x.uuid.ToString() == sessParams.id);
                if (removedParticipant)
                {
                    Debug.Log("[XRCManager] " + sessParams.tag + " left.");
                    synchedUsers.Remove(removedParticipant);
                    Destroy(removedParticipant.gameObject);
                }
                else
                {
                    Debug.LogWarning("[XRCManager] Unknown user left: " + sessParams.tag);
                }
            }
        }

        public void RemoteSessionAddedEventManager()
        {
            RemoteSessionEventParameters sessParams = XRCUnityDeprecated.rsAddedEventQueue.Dequeue();
            if (sessParams != null)
            {

            }
        }

        public void RemoteSessionUpdatedEventManager()
        {
            RemoteSessionEventParameters sessParams = XRCUnityDeprecated.rsUpdatedEventQueue.Dequeue();
            if (sessParams != null)
            {

            }
        }

        public void RemoteSessionDeletedEventManager()
        {
            RemoteSessionEventParameters sessParams = XRCUnityDeprecated.rsDeletedEventQueue.Dequeue();
            if (sessParams != null)
            {

            }
        }

        public void InitializeSessionUsers()
        {
            // Remove existing ones.
            SynchronizedUserDeprecated controlledUser = null;
            foreach (SynchronizedUserDeprecated user in synchedUsers)
            {
                if (user == GetControlledUser())
                {
                    controlledUser = user;
                }
                else
                {
                    Destroy(user.userObject);
                }
            }
            synchedUsers = new List<SynchronizedUserDeprecated>();
            synchedUsers.Add(controlledUser);

            // Add all users.
            foreach (SessionEntityDeprecated userEntity in GetSessionUsers())
            {
                if (controlledUser)
                {
                    if (userEntity.uuid == controlledUser.uuid.ToString()) continue;
                }

                synchedUsers.Add(SynchronizedUserDeprecated.Create(userEntity.tag, "default",
                    (SynchronizedUserDeprecated.UserType) userEntity.userType, userEntity.color, userEntity.uuid,
                    userEntity.lcUUID, userEntity.rcUUID, userEntity.lpUUID, userEntity.rpUUID, false));
            }
        }

        public void InitializeSessionEntities()
        {
            foreach (SessionEntityDeprecated sessionEntity in XRCUnityDeprecated.GetAllSessionEntities())
            {
                if (sessionEntity.category == USERCATEGORY ||
                    sessionEntity.category == LCONTROLLERCATEGORY || sessionEntity.category == RCONTROLLERCATEGORY ||
                    sessionEntity.category == LPOINTERCATEGORY || sessionEntity.category == RPOINTERCATEGORY)
                {
                    // User-associated entity: skip.
                    continue;
                }

                XRCUnityDeprecated.entityCreatedEventQueue.Enqueue(new EntityEventParametersDeprecated(
                    sessionEntity.tag, (int) sessionEntity.type, sessionEntity.resource,
                    sessionEntity.category, sessionEntity.subcategory, sessionEntity.bundle, sessionEntity.uuid, sessionEntity.parentUUID,
                    sessionEntity.settings, sessionEntity.position, sessionEntity.rotation, sessionEntity.scale));
                EntityCreationEventManager();
            }
        }

        public SynchronizedUserDeprecated GetControlledUser()
        {
            foreach (SynchronizedUserDeprecated user in synchedUsers)
            {
                if (user.isControlled) return user;
            }
            return null;
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

        public static void AddControllersToSession(string userTag, string userUUID,
            string lcUUID, string rcUUID, string lpUUID, string rpUUID)
        {
            XRCUnityDeprecated.AddSessionEntity(userTag + ".L.CONTROLLER", LCONTROLLERCATEGORY, null, null,
                lcUUID, userUUID, null, null, null, null,
                Vector3.zero, Quaternion.identity, Vector3.one,
                UnitType.meter, UnitType.degrees, UnitType.meter);
            XRCUnityDeprecated.AddSessionEntity(userTag + ".R.CONTROLLER", RCONTROLLERCATEGORY, null, null,
                rcUUID, userUUID, null, null, null, null,
                Vector3.zero, Quaternion.identity, Vector3.one,
                UnitType.meter, UnitType.degrees, UnitType.meter);
            XRCUnityDeprecated.AddSessionEntity(userTag + ".L.POINTER", LPOINTERCATEGORY, null, null,
                lpUUID, userUUID, null, null, null, null,
                Vector3.zero, Quaternion.identity, Vector3.one,
                UnitType.meter, UnitType.degrees, UnitType.meter);
            XRCUnityDeprecated.AddSessionEntity(userTag + ".R.POINTER", RPOINTERCATEGORY, null, null,
                rpUUID, userUUID, null, null, null, null,
                Vector3.zero, Quaternion.identity, Vector3.one,
                UnitType.meter, UnitType.degrees, UnitType.meter);
        }

        public SessionEntityDeprecated[] GetSessionUsers()
        {
            SessionEntityDeprecated[] sessionEntities = XRCUnityDeprecated.GetAllSessionEntities();
            if (sessionEntities != null)
            {
                List<SessionEntityDeprecated> sessionUsers = new List<SessionEntityDeprecated>();
                foreach (SessionEntityDeprecated sessionEntity in sessionEntities)
                {
                    if (sessionEntity.category == USERCATEGORY)
                    {
                        sessionUsers.Add(sessionEntity);
                    }
                }
                return sessionUsers.ToArray();
            }
            return null;
        }

        private void AddControllersToSession()
        {
            SynchronizedUserDeprecated sU = GetControlledUser();
            if (sU)
            {
                XRCUnityDeprecated.AddSessionEntity(sU.tag + ".L.CONTROLLER", LCONTROLLERCATEGORY, null, null,
                    sU.leftController.uuid.ToString(), sU.uuid.ToString(), null, null, null, null,
                    sU.leftController.transform.position, sU.leftController.transform.rotation,
                    sU.leftController.transform.localScale,
                    UnitType.meter, UnitType.degrees, UnitType.meter);
                XRCUnityDeprecated.AddSessionEntity(sU.tag + ".L.POINTER", LPOINTERCATEGORY, null, null,
                    sU.leftController.pointer.uuid.ToString(), sU.uuid.ToString(), null, null, null, null,
                    sU.leftController.pointer.transform.position, sU.leftController.pointer.transform.rotation,
                    sU.leftController.pointer.transform.localScale, UnitType.meter, UnitType.degrees, UnitType.meter);

                XRCUnityDeprecated.AddSessionEntity(sU.tag + ".R.CONTROLLER", RCONTROLLERCATEGORY, null, null,
                    sU.rightController.uuid.ToString(), sU.uuid.ToString(), null, null, null, null,
                    sU.rightController.transform.position, sU.rightController.transform.rotation,
                    sU.rightController.transform.localScale,
                    UnitType.meter, UnitType.degrees, UnitType.meter);
                XRCUnityDeprecated.AddSessionEntity(sU.tag + ".R.POINTER", RPOINTERCATEGORY, null, null,
                    sU.rightController.pointer.uuid.ToString(), sU.uuid.ToString(), null, null, null, null,
                    sU.rightController.pointer.transform.position, sU.rightController.pointer.transform.rotation,
                    sU.rightController.pointer.transform.localScale, UnitType.meter, UnitType.degrees, UnitType.meter);
            }
        }

#region TypeDeserialization
        protected List<Vector3> DeserializeVector3ArrayToList(Vector3Type[] input)
        {
            List<Vector3> outVec = new List<Vector3>();

            foreach (Vector3Type vec in input)
            {
                outVec.Add(DeserializeVector3(vec));
            }

            return outVec;
        }

        protected Vector3[] DeserializeVector3Array(Vector3Type[] input)
        {
            Vector3[] outList = new Vector3[input.Length];

            for (int i = 0; i < input.Length; i++)
                foreach (Vector3Type vec in input)
                {
                    outList[i] = DeserializeVector3(input[i]);
                }

            return outList;
        }

        protected Vector3 DeserializeVector3(Vector3Type input)
        {
            return new Vector3(input.X, input.Y, input.Z);
        }

        protected Vector3 DeserializeVector3(NonNegativeFloat3Type input)
        {
            return new Vector3(input.X, input.Y, input.Z);
        }

        protected Quaternion DeserializeQuaternion(QuaternionType input)
        {
            return new Quaternion(input.X, input.Y, input.Z, input.W);
        }

        protected static List<Vector3> DeserializeVector3ArrayToList_s(Vector3Type[] input)
        {
            List<Vector3> outVec = new List<Vector3>();

            foreach (Vector3Type vec in input)
            {
                outVec.Add(DeserializeVector3_s(vec));
            }

            return outVec;
        }

        protected static Vector3[] DeserializeVector3Array_s(Vector3Type[] input)
        {
            Vector3[] outList = new Vector3[input.Length];

            for (int i = 0; i < input.Length; i++)
                foreach (Vector3Type vec in input)
                {
                    outList[i] = DeserializeVector3_s(input[i]);
                }

            return outList;
        }

        protected static Vector3 DeserializeVector3_s(Vector3Type input)
        {
            return new Vector3(input.X, input.Y, input.Z);
        }

        protected static Quaternion DeserializeQuaternion_s(QuaternionType input)
        {
            return new Quaternion(input.X, input.Y, input.Z, input.W);
        }
#endregion
    }
}