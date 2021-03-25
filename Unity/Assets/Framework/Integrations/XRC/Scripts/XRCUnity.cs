// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.Events;
using GSFC.ARVR.XRC;
using GSFC.ARVR.UTILITIES;

namespace GSFC.ARVR.MRET.XRC
{
    public class XRCUnity : MonoBehaviour
    {
        public static readonly int RESOURCEBUFSIZE = 65536;

        private static readonly string TYPESTRING = "TYPE",
            RESOURCESTRING = "RESOURCE",
            CATEGORYSTRING = "CATEGORY",
            SUBCATEGORYSTRING = "SUBCATEGORY",
            BUNDLESTRING = "BUNDLE",
            TAGSTRING = "TAG",
            USERTYPESTRING = "USER.TYPE",
            COLORSTRING = "COLOR",
            PARENTUUIDSTRING = "PARENTUUID",
            INTERACTIONSTRING = "INTERACTION",
            COLLISIONSTRING = "COLLISION",
            GRAVITYSTRING = "GRAVITY",
            LEFTCONTROLLERSTRING = "LCONTROLLER",
            RIGHTCONTROLLERSTRING = "RCONTROLLER",
            LEFTPOINTERSTRING = "LPOINTER",
            RIGHTPOINTERSTRING = "RPOINTER",
            TITLESTRING = "TITLE",
            TEXTSTRING = "TEXT";

        public LogLevel logLevel;

        public UnityEvent entityCreatedUnityEvent;
        public UnityEvent entityDestroyedUnityEvent;
        public UnityEvent entityReinitializedUnityEvent;
        public UnityEvent entityUpdatedUnityEvent;
        public UnityEvent entityEditedUnityEvent;
        public UnityEvent sessionJoinedUnityEvent;
        public UnityEvent sessionParticipantAddedUnityEvent;
        public UnityEvent sessionParticipantResyncedUnityEvent;
        public UnityEvent sessionParticipantDeletedUnityEvent;
        public UnityEvent remoteSessionAddedUnityEvent;
        public UnityEvent remoteSessionUpdatedUnityEvent;
        public UnityEvent remoteSessionDeletedUnityEvent;

        public static Queue<EntityEventParameters> entityCreatedEventQueue = new Queue<EntityEventParameters>();
        public static Queue<EntityEventParameters> entityDestroyedEventQueue = new Queue<EntityEventParameters>();
        public static Queue<EntityEventParameters> entityReinitializedEventQueue = new Queue<EntityEventParameters>();
        public static Queue<EntityEventParameters> entityUpdatedEventQueue = new Queue<EntityEventParameters>();
        public static Queue<EntityEventParameters> entityEditedEventQueue = new Queue<EntityEventParameters>();
        public static Queue<ActiveSessionEventParameters> asParticipantAddedEventQueue = new Queue<ActiveSessionEventParameters>();
        public static Queue<ActiveSessionEventParameters> asParticipantResyncedEventQueue = new Queue<ActiveSessionEventParameters>();
        public static Queue<ActiveSessionEventParameters> asParticipantDeletedEventQueue = new Queue<ActiveSessionEventParameters>();
        public static Queue<RemoteSessionEventParameters> rsAddedEventQueue = new Queue<RemoteSessionEventParameters>();
        public static Queue<RemoteSessionEventParameters> rsUpdatedEventQueue = new Queue<RemoteSessionEventParameters>();
        public static Queue<RemoteSessionEventParameters> rsDeletedEventQueue = new Queue<RemoteSessionEventParameters>();

        private static XRCUnity xrcUnity;
        private static int maxInvokesPerFrame = 3;

        private ConcurrentQueue<UnityEvent> invocationQueue = new ConcurrentQueue<UnityEvent>();

#region Entity callback handlers.
        static XRCInterface.EntityFunctionCallBack entityCreateEvent =
        (entityId) =>
        {
            double xPos, yPos, zPos, xRot, yRot, zRot, wRot, xScl, yScl, zScl;
            UnitType posUnits, rotUnits, sclUnits;
            ReferenceSpaceType posRef, rotRef, sclRef;
            int type;
            string category, subcategory, bundle, tag, parUUID, title, text;
            bool interaction, collision, gravity;
            byte[] resource = new byte[RESOURCEBUFSIZE];

            XRCInterface.GetEventEntityQTransform(out xPos, out yPos, out zPos, out posUnits, out posRef,
                out xScl, out yScl, out zScl, out sclUnits, out sclRef,
                out xRot, out yRot, out zRot, out wRot, out rotUnits, out rotRef);
            
            XRCInterface.GetEventEntityIntegerAttribute(TYPESTRING, out type);

            int resLen = 0;
            XRCInterface.GetEventEntityBlobAttribute(RESOURCESTRING, resource, RESOURCEBUFSIZE, out resLen);
            
            XRCInterface.GetEventEntityStringAttribute(CATEGORYSTRING, out category);

            XRCInterface.GetEventEntityStringAttribute(SUBCATEGORYSTRING, out subcategory);

            XRCInterface.GetEventEntityStringAttribute(BUNDLESTRING, out bundle);
            
            XRCInterface.GetEventEntityStringAttribute(TAGSTRING, out tag);
            
            XRCInterface.GetEventEntityStringAttribute(PARENTUUIDSTRING, out parUUID);

            XRCInterface.GetEventEntityBooleanAttribute(INTERACTIONSTRING, out interaction);

            XRCInterface.GetEventEntityBooleanAttribute(COLLISIONSTRING, out collision);

            XRCInterface.GetEventEntityBooleanAttribute(GRAVITYSTRING, out gravity);

            XRCInterface.GetEventEntityStringAttribute(TITLESTRING, out title);

            XRCInterface.GetEventEntityStringAttribute(TEXTSTRING, out text);

            entityCreatedEventQueue.Enqueue(new EntityEventParameters(tag, type, resource,
                category, subcategory, bundle, entityId, parUUID,
                new InteractablePart.InteractablePartSettings(interaction, collision, gravity),
                title, text,
                xPos, yPos, zPos, xRot, yRot, zRot, wRot, xScl, yScl, zScl));
            
            xrcUnity.invocationQueue.Enqueue(xrcUnity.entityCreatedUnityEvent);
        };

        static XRCInterface.EntityFunctionCallBack entityDestroyEvent =
        (entityId) =>
        {
            string category, subcategory, tag, parUUID;
            
            XRCInterface.GetEntityStringAttribute(entityId, CATEGORYSTRING, out category);
            XRCInterface.GetEntityStringAttribute(entityId, SUBCATEGORYSTRING, out subcategory);
            XRCInterface.GetEntityStringAttribute(entityId, TAGSTRING, out tag);
            XRCInterface.GetEntityStringAttribute(entityId, PARENTUUIDSTRING, out parUUID);

            entityDestroyedEventQueue.Enqueue(new EntityEventParameters(
                tag, category, subcategory, entityId, parUUID, null));
            xrcUnity.invocationQueue.Enqueue(xrcUnity.entityDestroyedUnityEvent);
        };

        static XRCInterface.EntityFunctionCallBack entityReinitializeEvent =
        (entityId) =>
        {
            double xPos, yPos, zPos, xRot, yRot, zRot, wRot, xScl, yScl, zScl;
            UnitType posUnits, rotUnits, sclUnits;
            ReferenceSpaceType posRef, rotRef, sclRef;
            int type;
            string category, subcategory, bundle, tag, parUUID, title, text;
            bool interaction, collision, gravity;
            byte[] resource = new byte[RESOURCEBUFSIZE];

            XRCInterface.GetEventEntityQTransform(out xPos, out yPos, out zPos, out posUnits, out posRef,
                out xScl, out yScl, out zScl, out sclUnits, out sclRef,
                out xRot, out yRot, out zRot, out wRot, out rotUnits, out rotRef);
            XRCInterface.GetEventEntityIntegerAttribute(TYPESTRING, out type);
            int resLen = 0;
            XRCInterface.GetEventEntityBlobAttribute(RESOURCESTRING, resource, RESOURCEBUFSIZE, out resLen);
            XRCInterface.GetEventEntityStringAttribute(CATEGORYSTRING, out category);
            XRCInterface.GetEventEntityStringAttribute(SUBCATEGORYSTRING, out subcategory);
            XRCInterface.GetEventEntityStringAttribute(BUNDLESTRING, out bundle);
            XRCInterface.GetEventEntityStringAttribute(TAGSTRING, out tag);
            XRCInterface.GetEventEntityStringAttribute(PARENTUUIDSTRING, out parUUID);
            XRCInterface.GetEventEntityBooleanAttribute(INTERACTIONSTRING, out interaction);
            XRCInterface.GetEventEntityBooleanAttribute(COLLISIONSTRING, out collision);
            XRCInterface.GetEventEntityBooleanAttribute(GRAVITYSTRING, out gravity);
            XRCInterface.GetEventEntityStringAttribute(TITLESTRING, out title);
            XRCInterface.GetEventEntityStringAttribute(TEXTSTRING, out text);

            entityReinitializedEventQueue.Enqueue(new EntityEventParameters(tag, type, resource,
                category, subcategory, bundle, entityId, parUUID,
                new InteractablePart.InteractablePartSettings(interaction, collision, gravity),
                title, text,
                xPos, yPos, zPos, xRot, yRot, zRot, wRot, xScl, yScl, zScl,
                posUnits, rotUnits, sclUnits));
            xrcUnity.invocationQueue.Enqueue(xrcUnity.entityReinitializedUnityEvent);
        };

        static XRCInterface.EntityFunctionCallBack entityUpdateEvent =
        (entityId) =>
        {
            double xPos, yPos, zPos, xRot, yRot, zRot, wRot, xScl, yScl, zScl;
            UnitType posUnits, rotUnits, sclUnits;
            ReferenceSpaceType posRef, rotRef, sclRef;
            int type;
            string category, subcategory, tag, parUUID, title, text;
            bool interaction, collision, gravity;
            byte[] resource = new byte[RESOURCEBUFSIZE];

            XRCInterface.GetEventEntityQTransform(out xPos, out yPos, out zPos, out posUnits, out posRef,
                out xScl, out yScl, out zScl, out sclUnits, out sclRef,
                out xRot, out yRot, out zRot, out wRot, out rotUnits, out rotRef);
            XRCInterface.GetEventEntityIntegerAttribute(TYPESTRING, out type);
            int resLen = 0;
            XRCInterface.GetEventEntityBlobAttribute(RESOURCESTRING, resource, RESOURCEBUFSIZE, out resLen);
            XRCInterface.GetEventEntityStringAttribute(CATEGORYSTRING, out category);
            XRCInterface.GetEventEntityStringAttribute(SUBCATEGORYSTRING, out subcategory);
            XRCInterface.GetEventEntityStringAttribute(TAGSTRING, out tag);
            XRCInterface.GetEventEntityStringAttribute(PARENTUUIDSTRING, out parUUID);
            XRCInterface.GetEventEntityBooleanAttribute(INTERACTIONSTRING, out interaction);
            XRCInterface.GetEventEntityBooleanAttribute(COLLISIONSTRING, out collision);
            XRCInterface.GetEventEntityBooleanAttribute(GRAVITYSTRING, out gravity);
            XRCInterface.GetEventEntityStringAttribute(TITLESTRING, out title);
            XRCInterface.GetEventEntityStringAttribute(TEXTSTRING, out text);

            entityUpdatedEventQueue.Enqueue(new EntityEventParameters(tag, type, resource,
                category, subcategory, null, entityId, parUUID,
                new InteractablePart.InteractablePartSettings(interaction, collision, gravity),
                title, text,
                xPos, yPos, zPos, xRot, yRot, zRot, wRot, xScl, yScl, zScl,
                posUnits, rotUnits, sclUnits));
            xrcUnity.invocationQueue.Enqueue(xrcUnity.entityUpdatedUnityEvent);
        };

        static XRCInterface.EntityFunctionCallBack entityEditEvent =
        (entityId) =>
        {
            double xPos, yPos, zPos, xRot, yRot, zRot, wRot, xScl, yScl, zScl;
            UnitType posUnits, rotUnits, sclUnits;
            ReferenceSpaceType posRef, rotRef, sclRef;
            int type;
            string category, subcategory, tag, parUUID, title, text;
            bool interaction, collision, gravity;
            byte[] resource = new byte[RESOURCEBUFSIZE];

            XRCInterface.GetEventEntityQTransform(out xPos, out yPos, out zPos, out posUnits, out posRef,
                out xScl, out yScl, out zScl, out sclUnits, out sclRef,
                out xRot, out yRot, out zRot, out wRot, out rotUnits, out rotRef);
            XRCInterface.GetEventEntityIntegerAttribute(TYPESTRING, out type);
            int resLen = 0;
            XRCInterface.GetEventEntityBlobAttribute(RESOURCESTRING, resource, RESOURCEBUFSIZE, out resLen);
            XRCInterface.GetEventEntityStringAttribute(CATEGORYSTRING, out category);
            XRCInterface.GetEventEntityStringAttribute(SUBCATEGORYSTRING, out subcategory);
            XRCInterface.GetEventEntityStringAttribute(TAGSTRING, out tag);
            XRCInterface.GetEventEntityStringAttribute(PARENTUUIDSTRING, out parUUID);
            XRCInterface.GetEventEntityBooleanAttribute(INTERACTIONSTRING, out interaction);
            XRCInterface.GetEventEntityBooleanAttribute(COLLISIONSTRING, out collision);
            XRCInterface.GetEventEntityBooleanAttribute(GRAVITYSTRING, out gravity);
            XRCInterface.GetEventEntityStringAttribute(TITLESTRING, out title);
            XRCInterface.GetEventEntityStringAttribute(TEXTSTRING, out text);

            entityEditedEventQueue.Enqueue(new EntityEventParameters(tag, type, resource,
                category, subcategory, null, entityId, parUUID,
                new InteractablePart.InteractablePartSettings(interaction, collision, gravity),
                title, text,
                xPos, yPos, zPos, xRot, yRot, zRot, wRot, xScl, yScl, zScl,
                posUnits, rotUnits, sclUnits));
            xrcUnity.invocationQueue.Enqueue(xrcUnity.entityEditedUnityEvent);
        };
#endregion

#region Active session callback handlers.
        static XRCInterface.ActiveSessionJoinedFunctionCallBack joinedEvent =
        () =>
        {
            xrcUnity.invocationQueue.Enqueue(xrcUnity.sessionJoinedUnityEvent);
        };

        static XRCInterface.ActiveSessionParticipantFunctionCallBack participantAddedEvent =
        (participantId) =>
        {
            string tag, color, lc, rc, lp, rp;
            int type;
            XRCInterface.GetSessionEntity(participantId);
            XRCInterface.GetEntityStringAttribute(participantId, TAGSTRING, out tag);
            XRCInterface.GetEntityIntegerAttribute(participantId, USERTYPESTRING, out type);
            XRCInterface.GetEntityStringAttribute(participantId, COLORSTRING, out color);
            XRCInterface.GetEntityStringAttribute(participantId, LEFTCONTROLLERSTRING, out lc);
            XRCInterface.GetEntityStringAttribute(participantId, RIGHTCONTROLLERSTRING, out rc);
            XRCInterface.GetEntityStringAttribute(participantId, LEFTPOINTERSTRING, out lp);
            XRCInterface.GetEntityStringAttribute(participantId, RIGHTPOINTERSTRING, out rp);

            asParticipantAddedEventQueue.Enqueue(new ActiveSessionEventParameters(participantId, tag,
                type, color, lc, rc, lp, rp));
            xrcUnity.invocationQueue.Enqueue(xrcUnity.sessionParticipantAddedUnityEvent);
        };

        static XRCInterface.ActiveSessionParticipantFunctionCallBack participantResyncEvent =
        (participantId) =>
        {
            string tag, color, lc, rc, lp, rp;
            int type;
            XRCInterface.GetSessionEntity(participantId);
            XRCInterface.GetEntityStringAttribute(participantId, TAGSTRING, out tag);
            XRCInterface.GetEntityIntegerAttribute(participantId, USERTYPESTRING, out type);
            XRCInterface.GetEntityStringAttribute(participantId, COLORSTRING, out color);
            XRCInterface.GetEntityStringAttribute(participantId, LEFTCONTROLLERSTRING, out lc);
            XRCInterface.GetEntityStringAttribute(participantId, RIGHTCONTROLLERSTRING, out rc);
            XRCInterface.GetEntityStringAttribute(participantId, LEFTPOINTERSTRING, out lp);
            XRCInterface.GetEntityStringAttribute(participantId, RIGHTPOINTERSTRING, out rp);

            asParticipantResyncedEventQueue.Enqueue(new ActiveSessionEventParameters(participantId, tag,
                type, color, lc, rc, lp, rp));
            xrcUnity.invocationQueue.Enqueue(xrcUnity.sessionParticipantResyncedUnityEvent);
        };

        static XRCInterface.ActiveSessionParticipantFunctionCallBack participantDeletedEvent =
        (participantId) =>
        {
            string tag, color, lc, rc, lp, rp;
            int type;
            XRCInterface.GetSessionEntity(participantId);
            XRCInterface.GetEntityStringAttribute(participantId, TAGSTRING, out tag);
            XRCInterface.GetEntityIntegerAttribute(participantId, USERTYPESTRING, out type);
            XRCInterface.GetEntityStringAttribute(participantId, COLORSTRING, out color);
            XRCInterface.GetEntityStringAttribute(participantId, LEFTCONTROLLERSTRING, out lc);
            XRCInterface.GetEntityStringAttribute(participantId, RIGHTCONTROLLERSTRING, out rc);
            XRCInterface.GetEntityStringAttribute(participantId, LEFTPOINTERSTRING, out lp);
            XRCInterface.GetEntityStringAttribute(participantId, RIGHTPOINTERSTRING, out rp);

            asParticipantDeletedEventQueue.Enqueue(new ActiveSessionEventParameters(participantId, tag,
                type, color, lc, rc, lp, rp));
            xrcUnity.invocationQueue.Enqueue(xrcUnity.sessionParticipantDeletedUnityEvent);
        };
#endregion

#region Remote session callback handlers.
        static XRCInterface.RemoteSessionFunctionCallBack remoteSessionAddedEvent =
        (sessionId) =>
        {
            rsAddedEventQueue.Enqueue(new RemoteSessionEventParameters(sessionId));
            xrcUnity.invocationQueue.Enqueue(xrcUnity.remoteSessionAddedUnityEvent);
        };

        static XRCInterface.RemoteSessionFunctionCallBack remoteSessionUpdatedEvent =
        (sessionId) =>
        {
            rsUpdatedEventQueue.Enqueue(new RemoteSessionEventParameters(sessionId));
            xrcUnity.invocationQueue.Enqueue(xrcUnity.remoteSessionUpdatedUnityEvent);
        };

        static XRCInterface.RemoteSessionFunctionCallBack remoteSessionDeletedEvent =
        (sessionId) =>
        {
            rsDeletedEventQueue.Enqueue(new RemoteSessionEventParameters(sessionId));
            xrcUnity.invocationQueue.Enqueue(xrcUnity.remoteSessionDeletedUnityEvent);
        };
#endregion

        public static bool XRCStarted
        {
            get
            {
                return XRCInterface.IsStarted;
            }
        }

        public void SessAdded()
        {
            Debug.Log("session added");
        }

        public static bool IsSessionActive
        {
            get
            {
                return XRCInterface.IsSessionActive;
            }
        }

        public static bool IsMaster
        {
            get
            {
                return XRCInterface.IsMaster;
            }
        }

        public static long NumUsers
        {
            get
            {
                return XRCInterface.GetCurrentSession().numUsers;
            }
        }

        public static string SessionID
        {
            get
            {
                return XRCInterface.GetCurrentSession().id;
            }
        }

        public static void Initialize(string server, int port, ConnectionTypes connType, string miss, string sat, string proj = "XRC", string group = "MRET", string sess = "UNSET")
        {
            if (!XRCInterface.IsStarted)
            {
                // Register for remote session events.
                XRCInterface.AddRemoteSessionEventListeners(remoteSessionAddedEvent, remoteSessionUpdatedEvent, remoteSessionDeletedEvent);

                // Register for active session events.
                XRCInterface.AddActiveSessionEventListeners(joinedEvent, participantAddedEvent, participantResyncEvent, participantDeletedEvent);

                // Register for entity events.
                XRCInterface.AddEntityEventListeners(entityCreateEvent, entityDestroyEvent, entityReinitializeEvent, entityUpdateEvent, entityEditEvent);

                // Configure Debug Level.
                XRCInterface.ConfigureSystem(xrcUnity.logLevel);

                // Configure Session Information.
                XRCInterface.ConfigureSession(proj, group, sess, 120000, 5000, 5000);

                // Configure Events.
                XRCInterface.ConfigureEvents(30000, 250, 500);

                // Configure Message Bus.
                XRCInterface.ConfigureMessageBus(server, port, connType, 1000, 10000, miss, sat);
            }
            else
            {
                WarnAlreadyStarted("XRCUnity->Initialize()");
            }
        }

        public static void Destroy()
        {
            XRCInterface.ShutDown();
            XRCInterface.RemoveEntityEventListeners();
            XRCInterface.RemoveActiveSessionEventListeners();
            XRCInterface.RemoveRemoteSessionEventListeners();
        }

        public static bool ConfigureMessageBus(string server, int port, ConnectionTypes connectionType)
        {
            if (!XRCInterface.IsStarted) return XRCInterface.ConfigureMessageBus(server, port, connectionType, 1000, 10000);

            WarnAlreadyStarted("XRCUnity->ConfigureMessageBus()");
            return false;
        }

        public static bool StartUp()
        {
            if (!XRCInterface.IsStarted) return XRCInterface.StartUp();

            WarnAlreadyStarted("XRCUnity->StartUp()");
            return false;
        }

        public static bool ShutDown()
        {
            if (XRCInterface.IsStarted) return XRCInterface.ShutDown();

            WarnNotStarted("XRCUnity->ShutDown()");
            return false;
        }

        public static bool ConfigureSession(string miss, string sat, string proj, string group, string sessName)
        {
            if (!XRCInterface.IsStarted) return XRCInterface.ConfigureSession(proj, group, sessName, 120000, 5000, 5000);

            WarnAlreadyStarted("XRCUnity->ConfigureSession()");
            return false;
        }

        public static bool StartSession(string userUUID, string userName, int userType,
            string userLabelColor, string lcUUID, string rcUUID, string lpUUID, string rpUUID)
        {
            if (XRCInterface.IsStarted)
            {
                // Create a staged user.
                XRCInterface.StagedEntityCreate(userUUID, EntityType.user);
                XRCInterface.SetEntityPosition(userUUID, 0.1, 0.2, 0.3, UnitType.meter, ReferenceSpaceType.global);
                XRCInterface.SetEntityScale(userUUID, 1.0, 1.0, 1.0, UnitType.unitless, ReferenceSpaceType.global);
                XRCInterface.SetEntityQRotation(userUUID, 1.0, 1.0, 1.0, 0.0, UnitType.degrees, ReferenceSpaceType.global);
                XRCInterface.SetEntityStringAttribute(userUUID, CATEGORYSTRING, XRCManager.USERCATEGORY);
                XRCInterface.SetEntityIntegerAttribute(userUUID, USERTYPESTRING, userType);
                XRCInterface.SetEntityStringAttribute(userUUID, BUNDLESTRING, null);
                XRCInterface.SetEntityStringAttribute(userUUID, TAGSTRING, userName);
                XRCInterface.SetEntityStringAttribute(userUUID, COLORSTRING, userLabelColor);
                XRCInterface.SetEntityStringAttribute(userUUID, PARENTUUIDSTRING, null);
                XRCInterface.SetEntityStringAttribute(userUUID, LEFTCONTROLLERSTRING, lcUUID);
                XRCInterface.SetEntityStringAttribute(userUUID, RIGHTCONTROLLERSTRING, rcUUID);
                XRCInterface.SetEntityStringAttribute(userUUID, LEFTPOINTERSTRING, lpUUID);
                XRCInterface.SetEntityStringAttribute(userUUID, RIGHTPOINTERSTRING, rpUUID);

                // Start the session.
                return XRCInterface.StartSession(userUUID);
            }
            else
            {
                WarnNotStarted("XRCUnity->StartSession()");
                return false;
            }
        }

        public static bool StartSession(string miss, string sat, string proj, string group,
            string sessName, string userUUID, string userName, int userType, string userLabelColor,
            string lcUUID, string rcUUID, string lpUUID, string rpUUID)
        {
            if (XRCInterface.IsStarted)
            {
                // Configure Session Information.
                if (XRCInterface.ConfigureSession(proj, group, sessName, 120000, 5000, 5000) == false) return false;

                // Create a staged user.
                XRCInterface.StagedEntityCreate(userUUID, EntityType.user);
                XRCInterface.SetEntityPosition(userUUID, 0.1, 0.2, 0.3, UnitType.meter, ReferenceSpaceType.global);
                XRCInterface.SetEntityScale(userUUID, 1.0, 1.0, 1.0, UnitType.unitless, ReferenceSpaceType.global);
                XRCInterface.SetEntityQRotation(userUUID, 1.0, 1.0, 1.0, 0.0, UnitType.degrees, ReferenceSpaceType.global);
                XRCInterface.SetEntityStringAttribute(userUUID, CATEGORYSTRING, XRCManager.USERCATEGORY);
                XRCInterface.SetEntityStringAttribute(userUUID, BUNDLESTRING, null);
                XRCInterface.SetEntityStringAttribute(userUUID, TAGSTRING, userName);
                XRCInterface.SetEntityIntegerAttribute(userUUID, USERTYPESTRING, userType);
                XRCInterface.SetEntityStringAttribute(userUUID, COLORSTRING, userLabelColor);
                XRCInterface.SetEntityStringAttribute(userUUID, PARENTUUIDSTRING, null);
                XRCInterface.SetEntityStringAttribute(userUUID, LEFTCONTROLLERSTRING, lcUUID);
                XRCInterface.SetEntityStringAttribute(userUUID, RIGHTCONTROLLERSTRING, rcUUID);
                XRCInterface.SetEntityStringAttribute(userUUID, LEFTPOINTERSTRING, lpUUID);
                XRCInterface.SetEntityStringAttribute(userUUID, RIGHTPOINTERSTRING, rpUUID);

                // Start the session.
                return XRCInterface.StartSession(userUUID);
            }
            else
            {
                WarnNotStarted("XRCUnity->StartSession()");
                return false;
            }
        }

        public static bool EndSession()
        {
            if (XRCInterface.IsSessionActive)
            {
                return XRCInterface.EndSession();
            }
            else
            {
                WarnNoSession("XRCUnity->EndSession()");
                return false;
            }
        }

        public static XRCSessionInfo[] GetRemoteSessions()
        {
            if (XRCInterface.IsStarted)
            {
                return XRCInterface.GetRemoteSessions();
            }
            else
            {
                WarnNotStarted("XRCUnity->GetRemoteSessions()");
                return null;
            }
        }

        public static bool JoinSession(string sessionID, string uuid, string userName, int userType,
            string userLabelColor, string lcUUID, string rcUUID, string lpUUID, string rpUUID)
        {
            if (XRCInterface.IsSessionActive)
            {
                WarnInSession("XRCUnity->JoinSession()");
                return false;
            }
            else if (!XRCInterface.IsStarted)
            {
                WarnNotStarted("XRCUnity->JoinSession()");
                return false;
            }
            else
            {
                // Create a staged user.
                XRCInterface.StagedEntityCreate(uuid, EntityType.user);
                XRCInterface.SetEntityPosition(uuid, 0.1, 0.2, 0.3, UnitType.meter, ReferenceSpaceType.global);
                XRCInterface.SetEntityScale(uuid, 1.0, 1.0, 1.0, UnitType.unitless, ReferenceSpaceType.global);
                XRCInterface.SetEntityQRotation(uuid, 1.0, 1.0, 1.0, 0.0, UnitType.degrees, ReferenceSpaceType.global);
                XRCInterface.SetEntityStringAttribute(uuid, CATEGORYSTRING, XRCManager.USERCATEGORY);
                XRCInterface.SetEntityStringAttribute(uuid, BUNDLESTRING, null);
                XRCInterface.SetEntityStringAttribute(uuid, TAGSTRING, userName);
                XRCInterface.SetEntityIntegerAttribute(uuid, USERTYPESTRING, userType);
                XRCInterface.SetEntityStringAttribute(uuid, COLORSTRING, userLabelColor);
                XRCInterface.SetEntityStringAttribute(uuid, PARENTUUIDSTRING, null);
                XRCInterface.SetEntityStringAttribute(uuid, LEFTCONTROLLERSTRING, lcUUID);
                XRCInterface.SetEntityStringAttribute(uuid, RIGHTCONTROLLERSTRING, rcUUID);
                XRCInterface.SetEntityStringAttribute(uuid, LEFTPOINTERSTRING, lpUUID);
                XRCInterface.SetEntityStringAttribute(uuid, RIGHTPOINTERSTRING, rpUUID);

                // Join the session.
                return XRCInterface.JoinSession(sessionID, uuid);
            }
        }

        public static bool CancelJoinSession()
        {
            if (XRCInterface.IsStarted)
            {
                return XRCInterface.CancelJoinSession();
            }
            else
            {
                WarnNotStarted("XRCUnity->CancelJoinSession()");
                return false;
            }
        }

        public static bool LeaveSession()
        {
            if (XRCInterface.IsSessionActive)
            {
                return XRCInterface.LeaveSession();
            }
            else
            {
                WarnNoSession("XRCUnity->LeaveSession()");
                return false;
            }
        }

        public static bool SessionEntityExists(string id)
        {
            if (XRCInterface.IsSessionActive)
            {
                return XRCInterface.SessionEntityExists(id);
            }
            else
            {
                WarnNoSession("XRCUnity->SessionEntityExists()");
                return false;
            }
        }

        public static bool AddSessionEntity(string tag, string category, string subcategory,
            string bundle, string objectID, string parentID,
            InteractablePart.InteractablePartSettings settings, byte[] resource, string title, string text,
            Vector3 pos, Quaternion rot, Vector3 scl,
            UnitType posUnits = UnitType.unitless, UnitType rotUnits = UnitType.unitless, UnitType sclUnits = UnitType.unitless)
        {
            return AddSessionEntity(tag, category, subcategory, bundle, objectID, parentID, settings, resource, title, text,
                new Vector3d(pos.x, pos.y, pos.z),
                new Quaterniond(rot.x, rot.y, rot.z, rot.w),
                new Vector3d(scl.x, scl.y, scl.z),
                posUnits, rotUnits, sclUnits);
        }

        public static bool AddSessionEntity(string tag, string category, string subcategory,
            string bundle, string objectID, string parentID, InteractablePart.InteractablePartSettings settings, byte[] resource,
            string title, string text,
            Vector3d pos, Quaterniond rot, Vector3d scl,
            UnitType posUnits = UnitType.unitless, UnitType rotUnits = UnitType.unitless, UnitType sclUnits = UnitType.unitless)
        {
            if (XRCInterface.IsSessionActive)
            {
                XRCInterface.StagedEntityCreate(objectID);
                
                if (!XRCInterface.SetEntityStringAttribute(objectID, CATEGORYSTRING, category)) return false;

                if (!XRCInterface.SetEntityStringAttribute(objectID, SUBCATEGORYSTRING, subcategory)) return false;

                if (!XRCInterface.SetEntityStringAttribute(objectID, BUNDLESTRING, bundle)) return false;
                
                if (!XRCInterface.SetEntityStringAttribute(objectID, TAGSTRING, tag)) return false;
                
                if (!XRCInterface.SetEntityStringAttribute(objectID, COLORSTRING, null)) return false;

                if (!XRCInterface.SetEntityStringAttribute(objectID, PARENTUUIDSTRING, parentID)) return false;
                
                if (settings == null)
                {
                    settings = InteractablePart.InteractablePartSettings.defaultSettings;
                }

                if (!XRCInterface.SetEntityBooleanAttribute(objectID,
                    INTERACTIONSTRING, settings.interactionEnabled)) return false;

                if (!XRCInterface.SetEntityBooleanAttribute(objectID,
                    COLLISIONSTRING, settings.collisionEnabled)) return false;

                if (!XRCInterface.SetEntityBooleanAttribute(objectID,
                    GRAVITYSTRING, settings.gravityEnabled)) return false;
                
                if (resource != null)
                {
                    if (!XRCInterface.SetEntityBlobAttribute(objectID, RESOURCESTRING, resource, resource.Length)) return false;
                }
                else
                {
                    byte[] temp = new byte[1] { 1 };
                    if (!XRCInterface.SetEntityBlobAttribute(objectID, RESOURCESTRING, temp, temp.Length)) return false;
                }

                if (!XRCInterface.SetEntityStringAttribute(objectID, TITLESTRING, title)) return false;

                if (!XRCInterface.SetEntityStringAttribute(objectID, TEXTSTRING, text)) return false;

                if (!XRCInterface.SetEntityQTransform(objectID, pos.x, pos.y, pos.z, posUnits, ReferenceSpaceType.global,
                    scl.x, scl.y, scl.z, sclUnits, ReferenceSpaceType.global,
                    rot.x, rot.y, rot.z, rot.w, rotUnits, ReferenceSpaceType.global)) return false;
                
                if (!XRCInterface.AddSessionEntity(objectID)) return false;
                
                return XRCInterface.UpdateSessionEntity(objectID);
            }
            else
            {
                WarnNoSession("XRCUnity->AddSessionEntity()");
                return false;
            }
        }

        public static long GetSessionEntityCount()
        {
            if (XRCInterface.IsSessionActive)
            {
                return XRCInterface.GetSessionEntityCount();
            }
            else
            {
                WarnNoSession("XRCUnity->GetSessionEntityCount()");
                return -1;
            }
        }

        public static SessionEntity GetSessionEntity(string entityId)
        {
            if (XRCInterface.IsSessionActive)
            {
                if (!XRCInterface.GetSessionEntity(entityId)) return null;
                
                EntityType eType = EntityType.entity;
                if (!XRCInterface.GetStagedEntityType(entityId, out eType)) return null;

                byte[] res = new byte[RESOURCEBUFSIZE];
                int resLen = 0;
                XRCInterface.GetEntityBlobAttribute(entityId, RESOURCESTRING, res, RESOURCEBUFSIZE, out resLen);

                string cat = "";
                XRCInterface.GetEntityStringAttribute(entityId, CATEGORYSTRING, out cat);

                string scat = "";
                XRCInterface.GetEntityStringAttribute(entityId, SUBCATEGORYSTRING, out scat);

                string bdl = "";
                XRCInterface.GetEntityStringAttribute(entityId, BUNDLESTRING, out bdl);

                int uType = 0;
                XRCInterface.GetEntityIntegerAttribute(entityId, USERTYPESTRING, out uType);

                string color = "";
                XRCInterface.GetEntityStringAttribute(entityId, COLORSTRING, out color);

                string tag = "";
                XRCInterface.GetEntityStringAttribute(entityId, TAGSTRING, out tag);

                string parUUID = "";
                XRCInterface.GetEntityStringAttribute(entityId, PARENTUUIDSTRING, out parUUID);

                bool interaction = false;
                XRCInterface.GetEntityBooleanAttribute(entityId, INTERACTIONSTRING, out interaction);

                bool collision = false;
                XRCInterface.GetEntityBooleanAttribute(entityId, COLLISIONSTRING, out collision);

                bool gravity = false;
                XRCInterface.GetEntityBooleanAttribute(entityId, GRAVITYSTRING, out gravity);

                string title = "";
                XRCInterface.GetEntityStringAttribute(entityId, TITLESTRING, out title);

                string text = "";
                XRCInterface.GetEntityStringAttribute(entityId, TEXTSTRING, out text);

                string lc = "";
                XRCInterface.GetEntityStringAttribute(entityId, LEFTCONTROLLERSTRING, out lc);

                string rc = "";
                XRCInterface.GetEntityStringAttribute(entityId, RIGHTCONTROLLERSTRING, out rc);

                string lp = "";
                XRCInterface.GetEntityStringAttribute(entityId, LEFTPOINTERSTRING, out lp);

                string rp = "";
                XRCInterface.GetEntityStringAttribute(entityId, RIGHTPOINTERSTRING, out rp);

                double xPos, yPos, zPos, xRot, yRot, zRot, wRot, xScl, yScl, zScl;
                UnitType posUnits, rotUnits, sclUnits;
                ReferenceSpaceType posRef, rotRef, sclRef;
                XRCInterface.GetEntityQTransform(entityId, out xPos, out yPos, out zPos, out posUnits, out posRef,
                out xScl, out yScl, out zScl, out sclUnits, out sclRef,
                out xRot, out yRot, out zRot, out wRot, out rotUnits, out rotRef);

                return new SessionEntity(tag, EntityEventParameters.EntityTypeFromXRCEntityType(eType),
                    res, cat, scat, color, uType, bdl, entityId, parUUID,
                    new InteractablePart.InteractablePartSettings(interaction, collision, gravity),
                    title, text, lc, rc, lp, rp, new Vector3d(xPos, yPos, zPos),
                    new Quaterniond(xRot, yRot, zRot, wRot), new Vector3d(xScl, yScl, zScl),
                    posUnits, posRef, rotUnits, rotRef, sclUnits, sclRef);
            }
            else
            {
                WarnNoSession("XRCUnity->GetSessionEntity()");
            }

            return null;
        }

        public static SessionEntity[] GetAllSessionEntities()
        {
            if (XRCInterface.IsSessionActive)
            {
                XRCInterface.StagedEntitiesClear();
                XRCInterface.GetSessionEntities();
                List<SessionEntity> sessionEntities = new List<SessionEntity>();
                string[] entityIds;
                XRCInterface.GetStagedEntityIds(out entityIds);
                foreach (string entityId in entityIds)
                {
                    SessionEntity sessionEntity = GetSessionEntity(entityId);
                    if (sessionEntity != null)
                    {
                        sessionEntities.Add(sessionEntity);
                    }
                }
                return sessionEntities.ToArray();
            }
            else
            {
                WarnNoSession("XRCUnity->GetAllSessionEntities()");
            }
            return null;
        }

        public static bool RemoveSessionEntity(string tag, string category, string objectID)
        {
            if (XRCInterface.IsSessionActive)
            {
                if (!XRCInterface.RemoveSessionEntity(objectID)) return false;

                return XRCInterface.UpdateSessionEntity(objectID);
            }
            else
            {
                WarnNoSession("XRCUnity->RemoveSessionEntity()");
                return false;
            }
        }

        public static bool UpdateEntityPosition(string tag, string category, Vector3d pos,
            string id, string parentID = null, UnitType units = UnitType.unitless)
        {
            if (XRCInterface.IsSessionActive)
            {
                if (!XRCInterface.GetSessionEntity(id)) return false;

                if (!XRCInterface.SetEntityPosition(id, pos.x, pos.y, pos.z, units)) return false;

                return XRCInterface.UpdateSessionEntity(id);
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateEntityPosition()");
                return false;
            }
        }

        public static bool UpdateEntityPosition(string tag, string category, Vector3 pos,
            string id, string parentID = null, UnitType units = UnitType.unitless)
        {
            return UpdateEntityPosition(tag, category, new Vector3d(pos.x, pos.y, pos.z), id, parentID, units);
        }

        public static bool UpdateEntityRotation(string tag, string category, Quaterniond rot,
            string id, string parentID = null, UnitType units = UnitType.unitless)
        {
            if (XRCInterface.IsSessionActive)
            {
                if (!XRCInterface.GetSessionEntity(id)) return false;

                if (!XRCInterface.SetEntityQRotation(id, rot.x, rot.y, rot.z, rot.w, units)) return false;

                return XRCInterface.UpdateSessionEntity(id);
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateEntityRotation()");
                return false;
            }
        }

        public static bool UpdateEntityRotation(string tag, string category, Quaternion rot,
            string id, string parentID = null, UnitType units = UnitType.unitless)
        {
            return UpdateEntityRotation(id, category, new Quaterniond(rot.x, rot.y, rot.z, rot.w), id, parentID, units);
        }

        public static bool UpdateEntityScale(string tag, string category, Vector3d scl,
            string id, string parentID = null, UnitType units = UnitType.unitless)
        {
            if (XRCInterface.IsSessionActive)
            {
                if (!XRCInterface.GetSessionEntity(id)) return false;

                if (!XRCInterface.SetEntityScale(id, scl.x, scl.y, scl.z, units)) return false;

                return XRCInterface.UpdateSessionEntity(id);
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateEntityScale()");
                return false;
            }
        }

        public static bool UpdateEntityScale(string tag, string category, Vector3 scl,
            string id, string parentID = null, UnitType units = UnitType.unitless)
        {
            return UpdateEntityScale(tag, category, new Vector3d(scl.x, scl.y, scl.z), id, parentID, units);
        }

        public static bool UpdateEntitySettings(string id, InteractablePart.InteractablePartSettings settings)
        {
            if (XRCInterface.IsSessionActive)
            {
                if (!XRCInterface.GetSessionEntity(id)) return false;

                if (settings == null)
                {
                    settings = InteractablePart.InteractablePartSettings.defaultSettings;
                }

                if (!XRCInterface.SetEntityBooleanAttribute(id, INTERACTIONSTRING, settings.interactionEnabled)) return false;

                if (!XRCInterface.SetEntityBooleanAttribute(id, COLLISIONSTRING, settings.collisionEnabled)) return false;

                if (!XRCInterface.SetEntityBooleanAttribute(id, GRAVITYSTRING, settings.gravityEnabled)) return false;

                return XRCInterface.UpdateSessionEntity(id);
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateEntitySettings()");
                return false;
            }
        }

        public static bool UpdateEntityResource(string id, byte[] resource)
        {
            if (XRCInterface.IsSessionActive)
            {
                if (!XRCInterface.GetSessionEntity(id)) return false;

                if (!XRCInterface.SetEntityBlobAttribute(id, RESOURCESTRING, resource, resource.Length)) return false;

                return XRCInterface.UpdateSessionEntity(id);
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateEntityResource()");
                return false;
            }
        }

        public static bool UpdateEntityParent(string id, string parentID)
        {
            if (XRCInterface.IsSessionActive)
            {
                if (!XRCInterface.GetSessionEntity(id)) return false;

                if (!XRCInterface.SetEntityStringAttribute(id, PARENTUUIDSTRING, parentID)) return false;

                return XRCInterface.UpdateSessionEntity(id);
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateEntityResource()");
                return false;
            }
        }

        public static bool UpdateEntityTitleandText(string id, string title, string text)
        {
            if (XRCInterface.IsSessionActive)
            {
                if (!XRCInterface.GetSessionEntity(id)) return false;

                if (!XRCInterface.SetEntityStringAttribute(id, TITLESTRING, title)) return false;

                if (!XRCInterface.SetEntityStringAttribute(id, TEXTSTRING, text)) return false;

                return XRCInterface.UpdateSessionEntity(id);
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateEntityTitleandText()");
                return false;
            }
        }

        void Start()
        {
            xrcUnity = this;
        }

        void Update()
        {
            int invoked = 0;
            while (invocationQueue.Count > 0 && invoked++ < maxInvokesPerFrame)
            {
                UnityEvent eventToPerform;
                if (!invocationQueue.TryDequeue(out eventToPerform)) return;

                if (eventToPerform != null)
                {
                    eventToPerform.Invoke();
                }
            }
        }

        void OnDestroy()
        {
            Destroy();
        }

        private static void WarnNotStarted(string location)
        {
            Debug.LogWarning(location + ": XRC Not Started");
        }

        private static void WarnAlreadyStarted(string location)
        {
            Debug.LogWarning(location + ": XRC Already Started");
        }

        private static void WarnNoSession(string location)
        {
            Debug.LogWarning(location + ": No Session Active");
        }

        private static void WarnInSession(string location)
        {
            Debug.LogWarning(location + ": Session Already Active");
        }
    }
}