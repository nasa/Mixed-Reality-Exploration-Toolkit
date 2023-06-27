// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GOV.NASA.GSFC.XR.XRC
{
    #region Enumerations

    public enum LogLevel
    {
        DEBUG,
        FINEST,
        FINER,
        FINE,
        CONFIG,
        INFO,
        WARNING,
        SEVERE
    };

    public enum ConnectionTypes
    {
        bolt,
        mb,
        amq383,
        amq384,
        ws71,
        ws75,
        ws80
    };

    public enum MessageBusLogLevel
    {
        NONE,
        ERROR,
        SECURE,
        WARNING,
        INFO,
        VERBOSE,
        DEBUG,
        NLEVEL
    };

    public enum EntityType
    {
        entity,
        user
    };

    public enum UnitType
    {
        unitless,
        millimeter,
        centimeter,
        meter,
        kilometer,
        inch,
        foot,
        yard,
        mile,
        radians,
        degrees
    };

    public enum ReferenceSpaceType
    {
        relative,
        global
    };

    public enum AttributeType
    {
        // Binaries
        binaryBlob,

        // Primitives
        primitiveString,
        primitiveBoolean,
        primitiveInteger,
        primitiveLong,
        primitiveFloat,
        primitiveDouble,

        // Quantities
        quantityInteger,
        quantityLong,
        quantityFloat,
        quantityDouble,

        // Transforms
        transformPosition,
        transformScale,
        transformEulerRotation,
        transformQRotation,
    };

    #endregion

    public class XRCInterface
    {
        public const string UNDEFINED = "UNSET";
        public const int ID_LEN = 128;
        public const int NAME_LEN = 256;
        public const int DATA_LEN = 512;
        public const char NULL_CHAR = '\0';

        #region Remote Session Callback Delegates
#if !HOLOLENS_BUILD
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
#endif
        public delegate void RemoteSessionFunctionCallBack(string sessionId);
        #endregion

        #region Active Session Callback Delegates
#if !HOLOLENS_BUILD
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
#endif
        public delegate void ActiveSessionFunctionCallBack();

#if !HOLOLENS_BUILD
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
#endif
        public delegate void ActiveSessionJoinedFunctionCallBack();

#if !HOLOLENS_BUILD
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
#endif
        public delegate void ActiveSessionParticipantFunctionCallBack(string participantId);
        #endregion

        #region Entity Callback delegates
#if !HOLOLENS_BUILD
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
#endif
        public delegate void EntityFunctionCallBack(string entityId);
        #endregion

#if !HOLOLENS_BUILD
        #region CLI DLL Imports
        /********************************************************************
         * STAGED ATTRIBUTE FUNCTIONS (MANAGED OBJECT BRIDGE)
         ********************************************************************/
        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEntityAttributeCount")]
        private static extern long I_GetEntityAttributeCount(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEntityAttributeIds")]
        private static extern bool I_GetEntityAttributeIds(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            out IntPtr attributeIdsArrayPtr,
            out long attributeIdsCount);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEntityAttributeType")]
        private static extern bool I_GetEntityAttributeType(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out int type);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEntityAttributeAsString")]
        private static extern bool I_GetEntityAttributeAsString(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
            long bufLen);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEntityBlobAttribute")]
        private static extern bool I_GetEntityBlobAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] dest,
            long destsz,
            out int dataLen);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "SetEntityBlobAttribute")]
        private static extern bool I_SetEntityBlobAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] data,
            int dataLen);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEntityStringAttribute")]
        private static extern bool I_GetEntityStringAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
            long bufLen);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "SetEntityStringAttribute")]
        private static extern bool I_SetEntityStringAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] value);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEntityBooleanAttribute")]
        private static extern bool I_GetEntityBooleanAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out bool value);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "SetEntityBooleanAttribute")]
        private static extern bool I_SetEntityBooleanAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            bool value);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEntityIntegerAttribute")]
        private static extern bool I_GetEntityIntegerAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out int value);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "SetEntityIntegerAttribute")]
        private static extern bool I_SetEntityIntegerAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            int value);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEntityLongAttribute")]
        private static extern bool I_GetEntityLongAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out long value);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "SetEntityLongAttribute")]
        private static extern bool I_SetEntityLongAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            long value);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEntityFloatAttribute")]
        private static extern bool I_GetEntityFloatAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out float value);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "SetEntityFloatAttribute")]
        private static extern bool I_SetEntityFloatAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            float value);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEntityDoubleAttribute")]
        private static extern bool I_GetEntityDoubleAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out double value);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "SetEntityDoubleAttribute")]
        private static extern bool I_SetEntityDoubleAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            double value);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEntityIntegerQuantityAttribute")]
        private static extern bool I_GetEntityIntegerQuantityAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out int value, out int units);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "SetEntityIntegerQuantityAttribute")]
        private static extern bool I_SetEntityIntegerQuantityAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            int value, int units = (int)UnitType.unitless);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEntityLongQuantityAttribute")]
        private static extern bool I_GetEntityLongQuantityAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out long value, out int units);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "SetEntityLongQuantityAttribute")]
        private static extern bool I_SetEntityLongQuantityAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            long value, int units = (int)UnitType.unitless);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEntityFloatQuantityAttribute")]
        private static extern bool I_GetEntityFloatQuantityAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out float value, out int units);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "SetEntityFloatQuantityAttribute")]
        private static extern bool I_SetEntityFloatQuantityAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            float value, int units = (int)UnitType.unitless);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEntityDoubleQuantityAttribute")]
        private static extern bool I_GetEntityDoubleQuantityAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out double value, out int units);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "SetEntityDoubleQuantityAttribute")]
        private static extern bool I_SetEntityDoubleQuantityAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            double value, int units = (int)UnitType.unitless);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEntityPosition")]
        private static extern bool I_GetEntityPosition(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            out double x, out double y, out double z,
            out int units, out int refSpace);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "SetEntityPosition")]
        private static extern bool I_SetEntityPosition(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            double x, double y, double z,
            int units = (int)UnitType.meter, int refSpace = (int)ReferenceSpaceType.global);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEntityScale")]
        private static extern bool I_GetEntityScale(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            out double x, out double y, out double z,
            out int units, out int refSpace);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "SetEntityScale")]
        private static extern bool I_SetEntityScale(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            double x, double y, double z,
            int units = (int)UnitType.meter, int refSpace = (int)ReferenceSpaceType.global);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEntityEulerRotation")]
        private static extern bool I_GetEntityEulerRotation(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            out double x, out double y, out double z,
            out int units, out int refSpace);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "SetEntityEulerRotation")]
        private static extern bool I_SetEntityEulerRotation(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            double x, double y, double z,
            int units = (int)UnitType.meter, int refSpace = (int)ReferenceSpaceType.global);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEntityQRotation")]
        private static extern bool I_GetEntityQRotation(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            out double x, out double y, out double z, out double w,
            out int units, out int refSpace);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "SetEntityQRotation")]
        private static extern bool I_SetEntityQRotation(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            double x, double y, double z, double w,
            int units = (int)UnitType.meter, int refSpace = (int)ReferenceSpaceType.global);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEntityTransform")]
        private static extern bool I_GetEntityTransform(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            out double xPos, out double yPos, out double zPos, out int unitsPos, out int refPos,
            out double xScl, out double yScl, out double zScl, out int unitsScl, out int refScl,
            out double xRot, out double yRot, out double zRot, out int unitsRot, out int refRot);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "SetEntityTransform")]
        private static extern bool I_SetEntityTransform(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            double xPos, double yPos, double zPos, int unitsPos, int refPos,
            double xScl, double yScl, double zScl, int unitsScl, int refScl,
            double xRot, double yRot, double zRot, int unitsRot, int refRot);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEntityQTransform")]
        private static extern bool I_GetEntityQTransform(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            out double xPos, out double yPos, out double zPos, out int unitsPos, out int refPos,
            out double xScl, out double yScl, out double zScl, out int unitsScl, out int refScl,
            out double xRot, out double yRot, out double zRot, out double wRot, out int unitsRot, out int refRot);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "SetEntityQTransform")]
        private static extern bool I_SetEntityQTransform(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            double xPos, double yPos, double zPos, int unitsPos, int refPos,
            double xScl, double yScl, double zScl, int unitsScl, int refScl,
            double xRot, double yRot, double zRot, double wRot, int unitsRot, int refRot);

        /********************************************************************
         * STAGED OBJECT FUNCTIONS (MANAGED OBJECT BRIDGE)
         ********************************************************************/
        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetStagedEntityType")]
        private static extern bool I_GetStagedEntityType(
            [MarshalAs(UnmanagedType.LPArray)] byte[] objectId,
            out int type);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetStagedEntityIds")]
        private static extern bool I_GetStagedEntityIds(
            out IntPtr entityIdsArrayPtr,
            out long entityIdsCount);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetStagedEntityAsString")]
        private static extern bool I_GetStagedEntityAsString(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
            long bufLen);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "StagedEntityCreate")]
        private static extern void I_StagedEntityCreate(
            [MarshalAs(UnmanagedType.LPArray)] byte[] objectId,
            int objectType = (int)EntityType.entity);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "StagedEntityDelete")]
        private static extern void I_StagedEntityDelete(
            [MarshalAs(UnmanagedType.LPArray)] byte[] objectId);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "StagedEntitiesClear")]
        private static extern void I_StagedEntitiesClear();

        /********************************************************************
         * XRC CONFIGURATION FUNCTIONS
         ********************************************************************/
        [DllImport("XRCEngineCLI.dll", EntryPoint = "ConfigureSystem")]
        private static extern bool I_ConfigureSystem(
            int logLevel);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "ConfigureSession")]
        private static extern bool I_ConfigureSession(
            [MarshalAs(UnmanagedType.LPArray)] byte[] project,
            [MarshalAs(UnmanagedType.LPArray)] byte[] group,
            [MarshalAs(UnmanagedType.LPArray)] byte[] sessionName,
            int participantCheckin,
            int cleanupThreadSleep,
            int inquiryThreadSleep,
            bool enableAutoCleanup);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "ConfigureEvents")]
        private static extern bool I_ConfigureEvents(
            int heartbeatThreadSleep,
            int incomingThreadSleep,
            int outgoingThreadSleep,
            bool enableHeartbeat);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "ConfigureMessageBus")]
        private static extern bool I_ConfigureMessageBus(
            [MarshalAs(UnmanagedType.LPArray)] byte[] server,
            int port,
            int connectionType,
            int directiveTimeout,
            int replyTimeout,
            [MarshalAs(UnmanagedType.LPArray)] byte[] mission,
            [MarshalAs(UnmanagedType.LPArray)] byte[] satellite,
            [MarshalAs(UnmanagedType.LPArray)] byte[] facility,
            [MarshalAs(UnmanagedType.LPArray)] byte[] domain1,
            [MarshalAs(UnmanagedType.LPArray)] byte[] domain2,
            [MarshalAs(UnmanagedType.LPArray)] byte[] component,
            [MarshalAs(UnmanagedType.LPArray)] byte[] subcomponent1,
            [MarshalAs(UnmanagedType.LPArray)] byte[] subcomponent2,
            int logLevel,
            [MarshalAs(UnmanagedType.LPArray)] byte[] logFile);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "IsStarted")]
        private static extern bool I_IsStarted();

        [DllImport("XRCEngineCLI.dll", EntryPoint = "StartUp")]
        private static extern bool I_StartUp();

        [DllImport("XRCEngineCLI.dll", EntryPoint = "ShutDown")]
        private static extern bool I_ShutDown();

        /********************************************************************
         * XRC REMOTE SESSION FUNCTIONS
         ********************************************************************/
        [DllImport("XRCEngineCLI.dll", EntryPoint = "InitializeRemoteSessionList")]
        private static extern long I_InitializeRemoteSessionList();

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetRemoteSessionNumUsers")]
        private static extern long I_GetRemoteSessionNumUsers(int index);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetRemoteSessionID")]
        private static extern bool I_GetRemoteSessionID(
            int index,
            [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
            long bufLen);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetRemoteSessionGroup")]
        private static extern bool I_GetRemoteSessionGroup(
            int index,
            [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
            long bufLen);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetRemoteSessionProject")]
        private static extern bool I_GetRemoteSessionProject(
            int index,
            [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
            long bufLen);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetRemoteSessionName")]
        private static extern bool I_GetRemoteSessionName(
            int index,
            [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
            long bufLen);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetRemoteSessionPlatform")]
        private static extern bool I_GetRemoteSessionPlatform(
            int index,
            [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
            long bufLen);

        /********************************************************************
         * XRC SESSION MANAGEMENT FUNCTIONS
         ********************************************************************/
        [DllImport("XRCEngineCLI.dll", EntryPoint = "IsSessionActive")]
        private static extern bool I_IsSessionActive();

        [DllImport("XRCEngineCLI.dll", EntryPoint = "IsMaster")]
        private static extern bool I_IsMaster();

        [DllImport("XRCEngineCLI.dll", EntryPoint = "StartSession")]
        private static extern bool I_StartSession(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedUserId);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "EndSession")]
        private static extern bool I_EndSession();

        [DllImport("XRCEngineCLI.dll", EntryPoint = "JoinSession")]
        private static extern bool I_JoinSession(
            [MarshalAs(UnmanagedType.LPArray)] byte[] sessionId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] userName);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "CancelJoinSession")]
        private static extern bool I_CancelJoinSession();

        [DllImport("XRCEngineCLI.dll", EntryPoint = "LeaveSession")]
        private static extern bool I_LeaveSession();

        /********************************************************************
         * XRC ACTIVE SESSION MANAGEMENT FUNCTIONS
         ********************************************************************/
        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetSessionNumUsers")]
        private static extern long I_GetSessionNumUsers();

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetSessionID")]
        private static extern bool I_GetSessionID(
            [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
            int bufLen);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetSessionGroup")]
        private static extern bool I_GetSessionGroup(
            [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
            long bufLen);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetSessionProject")]
        private static extern bool I_GetSessionProject(
            [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
            long bufLen);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetSessionName")]
        private static extern bool I_GetSessionName(
            [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
            long bufLen);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetSessionPlatform")]
        private static extern bool I_GetSessionPlatform(
            [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
            long bufLen);

        /********************************************************************
         * XRC ACTIVE SESSION ENTITY MANAGEMENT FUNCTIONS
         ********************************************************************/
        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetSessionEntityCount")]
        private static extern long I_GetSessionEntityCount();

        [DllImport("XRCEngineCLI.dll", EntryPoint = "SessionEntityExists")]
        private static extern bool I_SessionEntityExists(
            [MarshalAs(UnmanagedType.LPArray)] byte[] entityId);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetSessionEntity")]
        private static extern bool I_GetSessionEntity(
            [MarshalAs(UnmanagedType.LPArray)] byte[] entityId);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetSessionEntities")]
        private static extern bool I_GetSessionEntities();

        [DllImport("XRCEngineCLI.dll", EntryPoint = "AddSessionEntity")]
        private static extern bool I_AddSessionEntity(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "RemoveSessionEntity")]
        private static extern bool I_RemoveSessionEntity(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "UpdateSessionEntity")]
        private static extern bool I_UpdateSessionEntity(
            [MarshalAs(UnmanagedType.LPArray)] byte[] stagedEntityId);

        /********************************************************************
         * XRC EVENT HANDLING
         ********************************************************************/
        [DllImport("XRCEngineCLI.dll", EntryPoint = "AddRemoteSessionEventListeners", CallingConvention = CallingConvention.Cdecl)]
        private static extern void I_AddRemoteSessionEventListeners(
            RemoteSessionFunctionCallBack rsAddCB,
            RemoteSessionFunctionCallBack rsUpdateCB,
            RemoteSessionFunctionCallBack rsDeleteCB);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "RemoveRemoteSessionEventListeners", CallingConvention = CallingConvention.Cdecl)]
        private static extern void I_RemoveRemoteSessionEventListeners();

        [DllImport("XRCEngineCLI.dll", EntryPoint = "AddActiveSessionEventListeners", CallingConvention = CallingConvention.Cdecl)]
        private static extern void I_AddActiveSessionEventListeners(
            ActiveSessionJoinedFunctionCallBack asJoinCB,
            ActiveSessionParticipantFunctionCallBack asPAddCB,
            ActiveSessionParticipantFunctionCallBack asPResyncCB,
            ActiveSessionParticipantFunctionCallBack asPDeleteCB);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "RemoveActiveSessionEventListeners", CallingConvention = CallingConvention.Cdecl)]
        private static extern void I_RemoveActiveSessionEventListeners();

        [DllImport("XRCEngineCLI.dll", EntryPoint = "AddEntityEventListeners")]
        private static extern void I_AddEntityEventListeners(
            [MarshalAs(UnmanagedType.FunctionPtr)] EntityFunctionCallBack entityCreateCB,
            [MarshalAs(UnmanagedType.FunctionPtr)] EntityFunctionCallBack entityDestroyCB,
            [MarshalAs(UnmanagedType.FunctionPtr)] EntityFunctionCallBack entityReinitCB,
            [MarshalAs(UnmanagedType.FunctionPtr)] EntityFunctionCallBack entityUpdateCB,
            [MarshalAs(UnmanagedType.FunctionPtr)] EntityFunctionCallBack entityEditCB);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "RemoveEntityEventListeners", CallingConvention = CallingConvention.Cdecl)]
        private static extern void I_RemoveEntityEventListeners();

        /********************************************************************
         * EVENT ENTITY ATTRIBUTE FUNCTIONS (MANAGED OBJECT BRIDGE)
         ********************************************************************/
        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityAttributeCount")]
        private static extern long I_GetEventEntityAttributeCount();

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityAttributeIds")]
        private static extern bool I_GetEventEntityAttributeIds(
            out IntPtr attributeIdsArrayPtr,
            out long attributeIdsCount);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityAttributeType")]
        private static extern bool I_GetEventEntityAttributeType(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out int type);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityAttributeAsString")]
        private static extern bool I_GetEventEntityAttributeAsString(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
            long bufLen);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityBlobAttribute")]
        private static extern bool I_GetEventEntityBlobAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] dest,
            long destsz,
            out int dataLen);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityStringAttribute")]
        private static extern bool I_GetEventEntityStringAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
            long bufLen);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityBooleanAttribute")]
        private static extern bool I_GetEventEntityBooleanAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out bool value);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityIntegerAttribute")]
        private static extern bool I_GetEventEntityIntegerAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out int value);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityLongAttribute")]
        private static extern bool I_GetEventEntityLongAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out long value);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityFloatAttribute")]
        private static extern bool I_GetEventEntityFloatAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out float value);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityDoubleAttribute")]
        private static extern bool I_GetEventEntityDoubleAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out double value);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityIntegerQuantityAttribute")]
        private static extern bool I_GetEventEntityIntegerQuantityAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out int value, out int units);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityLongQuantityAttribute")]
        private static extern bool I_GetEventEntityLongQuantityAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out long value, out int units);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityFloatQuantityAttribute")]
        private static extern bool I_GetEventEntityFloatQuantityAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out float value, out int units);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityDoubleQuantityAttribute")]
        private static extern bool I_GetEventEntityDoubleQuantityAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out double value, out int units);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityPosition")]
        private static extern bool I_GetEventEntityPosition(
            out double x, out double y, out double z,
            out int units, out int refSpace);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityScale")]
        private static extern bool I_GetEventEntityScale(
            out double x, out double y, out double z,
            out int units, out int refSpace);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityEulerRotation")]
        private static extern bool I_GetEventEntityEulerRotation(
            out double x, out double y, out double z,
            out int units, out int refSpace);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityQRotation")]
        private static extern bool I_GetEventEntityQRotation(
            out double x, out double y, out double z, out double w,
            out int units, out int refSpace);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityTransform")]
        private static extern bool I_GetEventEntityTransform(
            out double xPos, out double yPos, out double zPos, out int unitsPos, out int refPos,
            out double xScl, out double yScl, out double zScl, out int unitsScl, out int refScl,
            out double xRot, out double yRot, out double zRot, out int unitsRot, out int refRot);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityQTransform")]
        private static extern bool I_GetEventEntityQTransform(
            out double xPos, out double yPos, out double zPos, out int unitsPos, out int refPos,
            out double xScl, out double yScl, out double zScl, out int unitsScl, out int refScl,
            out double xRot, out double yRot, out double zRot, out double wRot, out int unitsRot, out int refRot);

        /********************************************************************
         * EVENT ENTITY FUNCTIONS (MANAGED OBJECT BRIDGE)
         ********************************************************************/
        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityId")]
        private static extern bool I_GetEventEntityId(
            [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
            long bufLen);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityType")]
        private static extern bool I_GetEventEntityType(
            out int type);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventEntityAsString")]
        private static extern bool I_GetEventEntityAsString(
            [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
            long bufLen);

        /*******************************************************************************
         * EVENT ACTIVE SESSION PARTICIPANT ATTRIBUTE FUNCTIONS (MANAGED OBJECT BRIDGE)
         *******************************************************************************/
        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantAttributeCount")]
        private static extern long I_GetEventActiveSessionParticipantAttributeCount();

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantAttributeIds")]
        private static extern bool I_GetEventActiveSessionParticipantAttributeIds(
            out IntPtr attributeIdsArrayPtr,
            out long attributeIdsCount);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantAttributeType")]
        private static extern bool I_GetEventActiveSessionParticipantAttributeType(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out int type);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantAttributeAsString")]
        private static extern bool I_GetEventActiveSessionParticipantAttributeAsString(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
            long bufLen);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantBlobAttribute")]
        private static extern bool I_GetEventActiveSessionParticipantBlobAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] dest,
            long destsz,
            out int dataLen);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantStringAttribute")]
        private static extern bool I_GetEventActiveSessionParticipantStringAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
            long bufLen);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantBooleanAttribute")]
        private static extern bool I_GetEventActiveSessionParticipantBooleanAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out bool value);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantIntegerAttribute")]
        private static extern bool I_GetEventActiveSessionParticipantIntegerAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out int value);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantLongAttribute")]
        private static extern bool I_GetEventActiveSessionParticipantLongAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out long value);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantFloatAttribute")]
        private static extern bool I_GetEventActiveSessionParticipantFloatAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out float value);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantDoubleAttribute")]
        private static extern bool I_GetEventActiveSessionParticipantDoubleAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out double value);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantIntegerQuantityAttribute")]
        private static extern bool I_GetEventActiveSessionParticipantIntegerQuantityAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out int value, out int units);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantLongQuantityAttribute")]
        private static extern bool I_GetEventActiveSessionParticipantLongQuantityAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out long value, out int units);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantFloatQuantityAttribute")]
        private static extern bool I_GetEventActiveSessionParticipantFloatQuantityAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out float value, out int units);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantDoubleQuantityAttribute")]
        private static extern bool I_GetEventActiveSessionParticipantDoubleQuantityAttribute(
            [MarshalAs(UnmanagedType.LPArray)] byte[] attributeId,
            out double value, out int units);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantPosition")]
        private static extern bool I_GetEventActiveSessionParticipantPosition(
            out double x, out double y, out double z,
            out int units, out int refSpace);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantScale")]
        private static extern bool I_GetEventActiveSessionParticipantScale(
            out double x, out double y, out double z,
            out int units, out int refSpace);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantEulerRotation")]
        private static extern bool I_GetEventActiveSessionParticipantEulerRotation(
            out double x, out double y, out double z,
            out int units, out int refSpace);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantQRotation")]
        private static extern bool I_GetEventActiveSessionParticipantQRotation(
            out double x, out double y, out double z, out double w,
            out int units, out int refSpace);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantTransform")]
        private static extern bool I_GetEventActiveSessionParticipantTransform(
            out double xPos, out double yPos, out double zPos, out int unitsPos, out int refPos,
            out double xScl, out double yScl, out double zScl, out int unitsScl, out int refScl,
            out double xRot, out double yRot, out double zRot, out int unitsRot, out int refRot);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantQTransform")]
        private static extern bool I_GetEventActiveSessionParticipantQTransform(
            out double xPos, out double yPos, out double zPos, out int unitsPos, out int refPos,
            out double xScl, out double yScl, out double zScl, out int unitsScl, out int refScl,
            out double xRot, out double yRot, out double zRot, out double wRot, out int unitsRot, out int refRot);

        /********************************************************************
         * EVENT ACTIVE SESSION PARTICIPANT FUNCTIONS (MANAGED OBJECT BRIDGE)
         ********************************************************************/
        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantId")]
        private static extern bool I_GetEventActiveSessionParticipantId(
            [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
            long bufLen);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantType")]
        private static extern bool I_GetEventActiveSessionParticipantType(
            out int type);

        [DllImport("XRCEngineCLI.dll", EntryPoint = "GetEventActiveSessionParticipantAsString")]
        private static extern bool I_GetEventActiveSessionParticipantAsString(
            [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
            long bufLen);

        #endregion
#endif

        #region C# Declarations
        static void MarshalUnmanagedStrArray2ManagedStrArray(IntPtr pUnmanagedStrArray, long count, out string[] ManagedStrArray)
        {
            IntPtr[] pIntPtrArray = new IntPtr[count];
            ManagedStrArray = new string[count];

            // Copy the unmanaged bytes into a managed IntPtr array
            Marshal.Copy(pUnmanagedStrArray, pIntPtrArray, 0, (int)count);

            for (int i = 0; i < count; i++)
            {
                // Marshal the bytes into a string, and store it into out managed string array
                ManagedStrArray[i] = Marshal.PtrToStringAnsi(pIntPtrArray[i]);

                // Free the unmanaged string memory
                Marshal.FreeCoTaskMem(pIntPtrArray[i]);
            }

            // Free the unmanaged array memory
            Marshal.FreeCoTaskMem(pUnmanagedStrArray);
        }

        /********************************************************************
         * STAGED ATTRIBUTE FUNCTIONS (MANAGED OBJECT BRIDGE)
         ********************************************************************/
        public static long GetEntityAttributeCount(string stagedEntityId)
        {
#if !HOLOLENS_BUILD
            return I_GetEntityAttributeCount(Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue));
#else
            return 0;
#endif
        }

        public static bool GetEntityAttributeIds(string stagedEntityId,
            out string[] attributeIds)
        {
            // Populate the attribute array
#if !HOLOLENS_BUILD
            long attributeIdsCount = 0;
            IntPtr attributeIdsArrayPtr = IntPtr.Zero;
            I_GetEntityAttributeIds(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                out attributeIdsArrayPtr,
                out attributeIdsCount);

            // Build the managed string array
            MarshalUnmanagedStrArray2ManagedStrArray(attributeIdsArrayPtr, attributeIdsCount, out attributeIds);
#else
            attributeIds = new string[0];
#endif

            return true;
        }

        public static bool GetEntityAttributeType(string stagedEntityId, string attributeId, out AttributeType type)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpType = 0;
            result = I_GetEntityAttributeType(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out tmpType);
            type = (AttributeType)tmpType;
#else
            type = default;
#endif

            return result;
        }

        public static bool GetEntityAttributeAsString(string stagedEntityId, string attributeId, out string value)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            byte[] valueBytes = new byte[DATA_LEN];
            result = I_GetEntityAttributeAsString(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                valueBytes, DATA_LEN);
            if (result)
            {
                value = Encoding.ASCII.GetString(valueBytes).TrimEnd(NULL_CHAR);
            }
            else
            {
                value = "";
            }
#else
            value = default;
#endif

            return result;
        }

        public static bool GetEntityBlobAttribute(string stagedEntityId, string attributeId, byte[] dest, long destsz, out int dataLen)
        {
#if !HOLOLENS_BUILD
            return I_GetEntityBlobAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                dest, destsz, out dataLen);
#else
            dataLen = 0;
            return false;
#endif
        }

        public static bool SetEntityBlobAttribute(string stagedEntityId, string attributeId, byte[] data, int dataLen)
        {
#if !HOLOLENS_BUILD
            return I_SetEntityBlobAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                data, dataLen);
#else
            return false;
#endif
        }

        public static bool GetEntityStringAttribute(string stagedEntityId, string attributeId, out string value)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            byte[] valueBytes = new byte[DATA_LEN];
            result = I_GetEntityStringAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                valueBytes, DATA_LEN);
            if (result)
            {
                value = Encoding.ASCII.GetString(valueBytes).TrimEnd(NULL_CHAR);
            }
            else
            {
                value = "";
            }
#else
            value = default;
#endif

            return result;
        }

        public static bool SetEntityStringAttribute(string stagedEntityId, string attributeId, string value)
        {
#if !HOLOLENS_BUILD
            return I_SetEntityStringAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                Encoding.ASCII.GetBytes(value + char.MinValue));
#else
            return false;
#endif
        }

        public static bool GetEntityBooleanAttribute(string stagedEntityId, string attributeId, out bool value)
        {
#if !HOLOLENS_BUILD
            return I_GetEntityBooleanAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value);
#else
            value = default;
            return false;
#endif
        }

        public static bool SetEntityBooleanAttribute(string stagedEntityId, string attributeId, bool value)
        {
#if !HOLOLENS_BUILD
            return I_SetEntityBooleanAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                value);
#else
            return false;
#endif
        }

        public static bool GetEntityIntegerAttribute(string stagedEntityId, string attributeId, out int value)
        {
#if !HOLOLENS_BUILD
            return I_GetEntityIntegerAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value);
#else
            value = default;
            return false;
#endif
        }

        public static bool SetEntityIntegerAttribute(string stagedEntityId, string attributeId, int value)
        {
#if !HOLOLENS_BUILD
            return I_SetEntityIntegerAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                value);
#else
            return false;
#endif
        }

        public static bool GetEntityLongAttribute(string stagedEntityId, string attributeId, out long value)
        {
#if !HOLOLENS_BUILD
            return I_GetEntityLongAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value);
#else
            value = default;
            return false;
#endif
        }

        public static bool SetEntityLongAttribute(string stagedEntityId, string attributeId, long value)
        {
#if !HOLOLENS_BUILD
            return I_SetEntityLongAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                value);
#else
            return false;
#endif
        }

        public static bool GetEntityFloatAttribute(string stagedEntityId, string attributeId, out float value)
        {
#if !HOLOLENS_BUILD
            return I_GetEntityFloatAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value);
#else
            value = default;
            return false;
#endif
        }

        public static bool SetEntityFloatAttribute(string stagedEntityId, string attributeId, float value)
        {
#if !HOLOLENS_BUILD
            return I_SetEntityFloatAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                value);
#else
            return false;
#endif
        }

        public static bool GetEntityDoubleAttribute(string stagedEntityId, string attributeId, out double value)
        {
#if !HOLOLENS_BUILD
            return I_GetEntityDoubleAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value);
#else
            value = default;
            return false;
#endif
        }

        public static bool SetEntityDoubleAttribute(string stagedEntityId, string attributeId, double value)
        {
#if !HOLOLENS_BUILD
            return I_SetEntityDoubleAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                value);
#else
            return false;
#endif
        }

        public static bool GetEntityIntegerQuantityAttribute(string stagedEntityId, string attributeId, out int value, out UnitType units)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            result = I_GetEntityIntegerQuantityAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value, out tmpUnits);
            units = (UnitType)tmpUnits;
#else
            value = default;
            units = default;
#endif

            return result;
        }

        public static bool SetEntityIntegerQuantityAttribute(string stagedEntityId, string attributeId, int value, UnitType units = UnitType.unitless)
        {
#if !HOLOLENS_BUILD
            return I_SetEntityIntegerQuantityAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                value, (int)units);
#else
            return false;
#endif
        }

        public static bool GetEntityLongQuantityAttribute(string stagedEntityId, string attributeId, out long value, out UnitType units)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            result = I_GetEntityLongQuantityAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value, out tmpUnits);
            units = (UnitType)tmpUnits;
#else
            value = default;
            units = default;
#endif

            return result;
        }

        public static bool SetEntityLongQuantityAttribute(string stagedEntityId, string attributeId, long value, UnitType units = UnitType.unitless)
        {
#if !HOLOLENS_BUILD
            return I_SetEntityLongQuantityAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                value, (int)units);
#else
            return false;
#endif
        }

        public static bool GetEntityFloatQuantityAttribute(string stagedEntityId, string attributeId, out float value, out UnitType units)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            result = I_GetEntityFloatQuantityAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value, out tmpUnits);
            units = (UnitType)tmpUnits;
#else
            value = default;
            units = default;
#endif

            return result;
        }

        public static bool SetEntityFloatQuantityAttribute(string stagedEntityId, string attributeId, float value, UnitType units = UnitType.unitless)
        {
#if !HOLOLENS_BUILD
            return I_SetEntityFloatQuantityAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                value, (int)units);
#else
            return false;
#endif
        }

        public static bool GetEntityDoubleQuantityAttribute(string stagedEntityId, string attributeId, out double value, out UnitType units)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            result = I_GetEntityDoubleQuantityAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value, out tmpUnits);
            units = (UnitType)tmpUnits;
#else
            value = default;
            units = default;
#endif

            return result;
        }

        public static bool SetEntityDoubleQuantityAttribute(string stagedEntityId, string attributeId, double value, UnitType units = UnitType.unitless)
        {
#if !HOLOLENS_BUILD
            return I_SetEntityDoubleQuantityAttribute(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                value, (int)units);
#else
            return false;
#endif
        }

        public static bool GetEntityPosition(string stagedEntityId,
            out double x, out double y, out double z,
            out UnitType units, out ReferenceSpaceType refSpace)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            int tmpRefSpace = 0;
            result = I_GetEntityPosition(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                out x, out y, out z,
                out tmpUnits, out tmpRefSpace);
            units = (UnitType)tmpUnits;
            refSpace = (ReferenceSpaceType)tmpRefSpace;
#else
            x = y = z = default;
            units = default;
            refSpace = default;
#endif

            return result;
        }

        public static bool SetEntityPosition(string stagedEntityId,
            double x, double y, double z,
            UnitType units = UnitType.meter, ReferenceSpaceType refSpace = ReferenceSpaceType.global)
        {
#if !HOLOLENS_BUILD
            return I_SetEntityPosition(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                x, y, z,
                (int)units, (int)refSpace);
#else
            return false;
#endif
        }

        public static bool GetEntityScale(string stagedEntityId,
            out double x, out double y, out double z,
            out UnitType units, out ReferenceSpaceType refSpace)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            int tmpRefSpace = 0;
            result = I_GetEntityScale(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                out x, out y, out z,
                out tmpUnits, out tmpRefSpace);
            units = (UnitType)tmpUnits;
            refSpace = (ReferenceSpaceType)tmpRefSpace;
#else
            x = y = z = default;
            units = default;
            refSpace = default;
#endif

            return result;
        }

        public static bool SetEntityScale(string stagedEntityId,
            double x, double y, double z,
            UnitType units = UnitType.meter, ReferenceSpaceType refSpace = ReferenceSpaceType.global)
        {
#if !HOLOLENS_BUILD
            return I_SetEntityScale(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                x, y, z,
                (int)units, (int)refSpace);
#else
            return false;
#endif
        }

        public static bool GetEntityEulerRotation(string stagedEntityId,
            out double x, out double y, out double z,
            out UnitType units, out ReferenceSpaceType refSpace)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            int tmpRefSpace = 0;

            result = I_GetEntityEulerRotation(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                out x, out y, out z,
                out tmpUnits, out tmpRefSpace);
            units = (UnitType)tmpUnits;
            refSpace = (ReferenceSpaceType)tmpRefSpace;
#else
            x = y = z = default;
            units = default;
            refSpace = default;
#endif

            return result;
        }

        public static bool SetEntityEulerRotation(string stagedEntityId,
            double x, double y, double z,
            UnitType units = UnitType.meter, ReferenceSpaceType refSpace = ReferenceSpaceType.global)
        {
#if !HOLOLENS_BUILD
            return I_SetEntityEulerRotation(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                x, y, z,
                (int)units, (int)refSpace);
#else
            return false;
#endif
        }

        public static bool GetEntityQRotation(string stagedEntityId,
            out double x, out double y, out double z, out double w,
            out UnitType units, out ReferenceSpaceType refSpace)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            int tmpRefSpace = 0;
            result = I_GetEntityQRotation(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                out x, out y, out z, out w,
                out tmpUnits, out tmpRefSpace);
            units = (UnitType)tmpUnits;
            refSpace = (ReferenceSpaceType)tmpRefSpace;
#else
            x = y = z = w = default;
            units = default;
            refSpace = default;
#endif

            return result;
        }

        public static bool SetEntityQRotation(string stagedEntityId,
            double x, double y, double z, double w,
            UnitType units = UnitType.meter, ReferenceSpaceType refSpace = ReferenceSpaceType.global)
        {
#if !HOLOLENS_BUILD
            return I_SetEntityQRotation(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                x, y, z, w,
                (int)units, (int)refSpace);
#else
            return false;
#endif
        }

        public static bool GetEntityTransform(string stagedEntityId,
            out double xPos, out double yPos, out double zPos, out UnitType unitsPos, out ReferenceSpaceType refPos,
            out double xScl, out double yScl, out double zScl, out UnitType unitsScl, out ReferenceSpaceType refScl,
            out double xRot, out double yRot, out double zRot, out UnitType unitsRot, out ReferenceSpaceType refRot)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnitsPos = 0;
            int tmpUnitsScl = 0;
            int tmpUnitsRot = 0;
            int tmpRefPos = 0;
            int tmpRefScl = 0;
            int tmpRefRot = 0;
            result = I_GetEntityTransform(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                out xPos, out yPos, out zPos, out tmpUnitsPos, out tmpRefPos,
                out xScl, out yScl, out zScl, out tmpUnitsScl, out tmpRefScl,
                out xRot, out yRot, out zRot, out tmpUnitsRot, out tmpRefRot);
            unitsPos = (UnitType)tmpUnitsPos;
            unitsScl = (UnitType)tmpUnitsScl;
            unitsRot = (UnitType)tmpUnitsRot;
            refPos = (ReferenceSpaceType)tmpRefPos;
            refScl = (ReferenceSpaceType)tmpRefScl;
            refRot = (ReferenceSpaceType)tmpRefRot;
#else
            xPos = yPos = zPos = default;
            xScl = yScl = zScl = default;
            xRot = yRot = zRot = default;
            unitsPos = unitsScl = unitsRot = default;
            refPos = refScl = refRot = default;
#endif

            return result;
        }

        public static bool SetEntityTransform(string stagedEntityId,
            double xPos, double yPos, double zPos, UnitType unitsPos, ReferenceSpaceType refPos,
            double xScl, double yScl, double zScl, UnitType unitsScl, ReferenceSpaceType refScl,
            double xRot, double yRot, double zRot, UnitType unitsRot, ReferenceSpaceType refRot)
        {
#if !HOLOLENS_BUILD
            return I_SetEntityTransform(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                xPos, yPos, zPos, (int)unitsPos, (int)refPos,
                xScl, yScl, zScl, (int)unitsScl, (int)refScl,
                xRot, yRot, zRot, (int)unitsRot, (int)refRot);
#else
            return false;
#endif
        }

        public static bool GetEntityQTransform(string stagedEntityId,
            out double xPos, out double yPos, out double zPos, out UnitType unitsPos, out ReferenceSpaceType refPos,
            out double xScl, out double yScl, out double zScl, out UnitType unitsScl, out ReferenceSpaceType refScl,
            out double xRot, out double yRot, out double zRot, out double wRot, out UnitType unitsRot, out ReferenceSpaceType refRot)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnitsPos = 0;
            int tmpUnitsScl = 0;
            int tmpUnitsRot = 0;
            int tmpRefPos = 0;
            int tmpRefScl = 0;
            int tmpRefRot = 0;
            result = I_GetEntityQTransform(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                out xPos, out yPos, out zPos, out tmpUnitsPos, out tmpRefPos,
                out xScl, out yScl, out zScl, out tmpUnitsScl, out tmpRefScl,
                out xRot, out yRot, out zRot, out wRot, out tmpUnitsRot, out tmpRefRot);
            unitsPos = (UnitType)tmpUnitsPos;
            unitsScl = (UnitType)tmpUnitsScl;
            unitsRot = (UnitType)tmpUnitsRot;
            refPos = (ReferenceSpaceType)tmpRefPos;
            refScl = (ReferenceSpaceType)tmpRefScl;
            refRot = (ReferenceSpaceType)tmpRefRot;
#else
            xPos = yPos = zPos = default;
            xScl = yScl = zScl = default;
            xRot = yRot = zRot = wRot = default;
            unitsPos = unitsScl = unitsRot = default;
            refPos = refScl = refRot = default;
#endif

            return result;
        }

        public static bool SetEntityQTransform(string stagedEntityId,
            double xPos, double yPos, double zPos, UnitType unitsPos, ReferenceSpaceType refPos,
            double xScl, double yScl, double zScl, UnitType unitsScl, ReferenceSpaceType refScl,
            double xRot, double yRot, double zRot, double wRot, UnitType unitsRot, ReferenceSpaceType refRot)
        {
#if !HOLOLENS_BUILD
            return I_SetEntityQTransform(
                Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue),
                xPos, yPos, zPos, (int)unitsPos, (int)refPos,
                xScl, yScl, zScl, (int)unitsScl, (int)refScl,
                xRot, yRot, zRot, wRot, (int)unitsRot, (int)refRot);
#else
            return false;
#endif
        }

        /********************************************************************
         * STAGED OBJECT FUNCTIONS (MANAGED OBJECT BRIDGE)
         ********************************************************************/
        public static bool GetStagedEntityType(string entityId, out EntityType type)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpType = 0;
            result = I_GetStagedEntityType(
                Encoding.ASCII.GetBytes(entityId + char.MinValue),
                out tmpType);
            type = (EntityType)tmpType;
#else
            type = default;
#endif

            return result;
        }

        public static bool GetStagedEntityAsString(string entityId, out string value)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            byte[] valueBytes = new byte[DATA_LEN];
            result = I_GetStagedEntityAsString(
                Encoding.ASCII.GetBytes(entityId + char.MinValue),
                valueBytes, DATA_LEN);
            if (result)
            {
                value = Encoding.ASCII.GetString(valueBytes).TrimEnd(NULL_CHAR);
            }
            else
            {
                value = "";
            }
#else
            value = default;
#endif

            return result;
        }

        public static bool GetStagedEntityIds(out string[] entityIds)
        {
#if !HOLOLENS_BUILD
            long entityIdsCount = 0;
            IntPtr entityIdsArrayPtr = IntPtr.Zero;

            // Populate the entity ID array
            I_GetStagedEntityIds(
                out entityIdsArrayPtr,
                out entityIdsCount);

            // Build the managed string array
            MarshalUnmanagedStrArray2ManagedStrArray(entityIdsArrayPtr, entityIdsCount, out entityIds);
#else
            entityIds = new string[0];
#endif

            return true;
        }

        public static void StagedEntityCreate(string entityId,
            EntityType objectType = EntityType.entity)
        {
#if !HOLOLENS_BUILD
            I_StagedEntityCreate(
                Encoding.ASCII.GetBytes(entityId + char.MinValue),
                (int)objectType);
#endif
        }

        public static void StagedEntityDelete(string entityId)
        {
#if !HOLOLENS_BUILD
            I_StagedEntityDelete(Encoding.ASCII.GetBytes(entityId + char.MinValue));
#endif
        }

        public static void StagedEntitiesClear()
        {
#if !HOLOLENS_BUILD
            I_StagedEntitiesClear();
#endif
        }

        /********************************************************************
         * XRC CONFIGURATION FUNCTIONS
         ********************************************************************/
        public static bool ConfigureSystem(LogLevel logLevel)
        {
#if !HOLOLENS_BUILD
            return I_ConfigureSystem((int)logLevel);
#else
            return false;
#endif
        }

        public static bool ConfigureSession(string project, string group, string sessionName,
            int participantCheckin, int cleanupThreadSleep, int inquiryThreadSleep,
            bool enableAutoCleanup = true)
        {
#if !HOLOLENS_BUILD
            return I_ConfigureSession(
                Encoding.ASCII.GetBytes(project + char.MinValue),
                Encoding.ASCII.GetBytes(group + char.MinValue),
                Encoding.ASCII.GetBytes(sessionName + char.MinValue),
                participantCheckin,
                cleanupThreadSleep,
                inquiryThreadSleep,
                enableAutoCleanup);
#else
            return false;
#endif
        }

        public static bool ConfigureEvents(int heartbeatThreadSleep, int incomingThreadSleep, int outgoingThreadSleep,
            bool enableHeartbeat = true)
        {
#if !HOLOLENS_BUILD
            return I_ConfigureEvents(
                heartbeatThreadSleep,
                incomingThreadSleep,
                outgoingThreadSleep,
                enableHeartbeat);
#else
            return false;
#endif
        }

        public static bool ConfigureMessageBus(string server, int port, ConnectionTypes connectionType,
            int directiveTimeout, int replyTimeout,
            string mission = UNDEFINED, string satellite = UNDEFINED, string facility = UNDEFINED,
            string domain1 = UNDEFINED, string domain2 = UNDEFINED, string component = UNDEFINED,
            string subcomponent1 = UNDEFINED, string subcomponent2 = UNDEFINED,
            MessageBusLogLevel logLevel = MessageBusLogLevel.NONE, string logFile = "STDERR")
        {
#if !HOLOLENS_BUILD
            return I_ConfigureMessageBus(
                Encoding.ASCII.GetBytes(server + char.MinValue),
                port,
                (int)connectionType,
                directiveTimeout,
                replyTimeout,
                Encoding.ASCII.GetBytes(mission + char.MinValue),
                Encoding.ASCII.GetBytes(satellite + char.MinValue),
                Encoding.ASCII.GetBytes(facility + char.MinValue),
                Encoding.ASCII.GetBytes(domain1 + char.MinValue),
                Encoding.ASCII.GetBytes(domain2 + char.MinValue),
                Encoding.ASCII.GetBytes(component + char.MinValue),
                Encoding.ASCII.GetBytes(subcomponent1 + char.MinValue),
                Encoding.ASCII.GetBytes(subcomponent2 + char.MinValue),
                (int)logLevel,
                Encoding.ASCII.GetBytes(logFile + char.MinValue));
#else
            return false;
#endif
        }

        public static bool IsStarted
        {
            get
            {
#if !HOLOLENS_BUILD
                return I_IsStarted();
#else
                return false;
#endif
            }
        }

        public static bool StartUp()
        {
#if !HOLOLENS_BUILD
            return I_StartUp();
#else
            return false;
#endif
        }

        public static bool ShutDown()
        {
#if !HOLOLENS_BUILD
            return I_ShutDown();
#else
            return false;
#endif
        }

        /********************************************************************
         * XRC REMOTE SESSION FUNCTIONS
         ********************************************************************/
        public static long InitializeRemoteServerSessionList()
        {
#if !HOLOLENS_BUILD
            return I_InitializeRemoteSessionList();
#else
            return 0;
#endif
        }

        public static XRCSessionInfo[] GetRemoteSessions()
        {
            List<XRCSessionInfo> sessInfos = new List<XRCSessionInfo>();
            long numSessions = InitializeRemoteServerSessionList();
            for (int i = 0; i < numSessions; i++)
            {
#if !HOLOLENS_BUILD
                // Extract all of the session information
                byte[] id = new byte[ID_LEN];
                if (!I_GetRemoteSessionID(i, id, ID_LEN)) continue;

                byte[] group = new byte[NAME_LEN];
                if (!I_GetRemoteSessionGroup(i, group, NAME_LEN)) continue;

                byte[] proj = new byte[NAME_LEN];
                if (!I_GetRemoteSessionProject(i, proj, NAME_LEN)) continue;

                byte[] sess = new byte[NAME_LEN];
                if (!I_GetRemoteSessionName(i, sess, NAME_LEN)) continue;

                byte[] plat = new byte[NAME_LEN];
                if (!I_GetRemoteSessionPlatform(i, plat, NAME_LEN)) continue;

                // Build the session information object
                sessInfos.Add(new XRCSessionInfo(
                    I_GetRemoteSessionNumUsers(i),
                    Encoding.ASCII.GetString(id).TrimEnd(NULL_CHAR),
                    Encoding.ASCII.GetString(group).TrimEnd(NULL_CHAR),
                    Encoding.ASCII.GetString(proj).TrimEnd(NULL_CHAR),
                    Encoding.ASCII.GetString(sess).TrimEnd(NULL_CHAR),
                    Encoding.ASCII.GetString(plat).TrimEnd(NULL_CHAR)));
#endif
            }

            return sessInfos.ToArray();
        }

        /********************************************************************
         * XRC SESSION MANAGEMENT FUNCTIONS
         ********************************************************************/
        public static bool IsSessionActive
        {
            get
            {
#if !HOLOLENS_BUILD
                return I_IsSessionActive();
#else
                return false;
#endif
            }
        }

        public static bool IsMaster
        {
            get
            {
#if !HOLOLENS_BUILD
                return I_IsMaster();
#else
                return false;
#endif
            }
        }

        public static bool StartSession(string stagedUserId)
        {
#if !HOLOLENS_BUILD
            return I_StartSession(Encoding.ASCII.GetBytes(stagedUserId + char.MinValue));
#else
            return false;
#endif
        }

        public static bool EndSession()
        {
#if !HOLOLENS_BUILD
            return I_EndSession();
#else
            return false;
#endif
        }

        public static bool JoinSession(string sessionId, string stagedUserId)
        {
#if !HOLOLENS_BUILD
            return I_JoinSession(
                Encoding.ASCII.GetBytes(sessionId + char.MinValue),
                Encoding.ASCII.GetBytes(stagedUserId + char.MinValue));
#else
            return false;
#endif
        }

        public static bool CancelJoinSession()
        {
#if !HOLOLENS_BUILD
            return I_CancelJoinSession();
#else
            return false;
#endif
        }

        public static bool LeaveSession()
        {
#if !HOLOLENS_BUILD
            return I_LeaveSession();
#else
            return false;
#endif
        }

        /********************************************************************
         * XRC ACTIVE SESSION MANAGEMENT FUNCTIONS
         ********************************************************************/
        public static XRCSessionInfo GetCurrentSession()
        {
            XRCSessionInfo sessInfo;

#if !HOLOLENS_BUILD
            // Extract all of the session information
            byte[] id = new byte[ID_LEN];
            I_GetSessionID(id, ID_LEN);

            byte[] group = new byte[NAME_LEN];
            I_GetSessionGroup(group, NAME_LEN);

            byte[] proj = new byte[NAME_LEN];
            I_GetSessionProject(proj, NAME_LEN);

            byte[] sess = new byte[NAME_LEN];
            I_GetSessionName(sess, NAME_LEN);

            byte[] plat = new byte[NAME_LEN];
            I_GetSessionPlatform(proj, NAME_LEN);

            // Build the session information object
            sessInfo = new XRCSessionInfo(
                I_GetSessionNumUsers(),
                Encoding.ASCII.GetString(id).TrimEnd(NULL_CHAR),
                Encoding.ASCII.GetString(group).TrimEnd(NULL_CHAR),
                Encoding.ASCII.GetString(proj).TrimEnd(NULL_CHAR),
                Encoding.ASCII.GetString(sess).TrimEnd(NULL_CHAR),
                Encoding.ASCII.GetString(plat).TrimEnd(NULL_CHAR));
#else
            sessInfo = new XRCSessionInfo(0, "", "", "", "", "");
#endif
            return sessInfo;
        }

        /********************************************************************
         * XRC ACTIVE SESSION ENTITY MANAGEMENT FUNCTIONS
         ********************************************************************/
        public static long GetSessionEntityCount()
        {
#if !HOLOLENS_BUILD
            return I_GetSessionEntityCount();
#else
            return 0;
#endif
        }

        public static bool SessionEntityExists(string entityId)
        {
#if !HOLOLENS_BUILD
            return I_SessionEntityExists(Encoding.ASCII.GetBytes(entityId + char.MinValue));
#else
            return false;
#endif
        }

        public static bool GetSessionEntity(string entityId)
        {
#if !HOLOLENS_BUILD
            return I_GetSessionEntity(Encoding.ASCII.GetBytes(entityId + char.MinValue));
#else
            return false;
#endif
        }

        public static bool GetSessionEntities()
        {
#if !HOLOLENS_BUILD
            return I_GetSessionEntities();
#else
            return false;
#endif
        }

        public static bool AddSessionEntity(string stagedEntityId)
        {
#if !HOLOLENS_BUILD
            return I_AddSessionEntity(Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue));
#else
            return false;
#endif
        }

        public static bool RemoveSessionEntity(string stagedEntityId)
        {
#if !HOLOLENS_BUILD
            return I_RemoveSessionEntity(Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue));
#else
            return false;
#endif
        }

        public static bool UpdateSessionEntity(string stagedEntityId)
        {
#if !HOLOLENS_BUILD
            return I_UpdateSessionEntity(Encoding.ASCII.GetBytes(stagedEntityId + char.MinValue));
#else
            return false;
#endif
        }

        public static void AddRemoteSessionEventListeners(
            RemoteSessionFunctionCallBack rsAddCB,
            RemoteSessionFunctionCallBack rsUpdateCB,
            RemoteSessionFunctionCallBack rsDeleteCB)
        {
#if !HOLOLENS_BUILD
            I_AddRemoteSessionEventListeners(rsAddCB, rsUpdateCB, rsDeleteCB);
#endif
        }

        public static void RemoveRemoteSessionEventListeners()
        {
#if !HOLOLENS_BUILD
            I_RemoveRemoteSessionEventListeners();
#endif
        }

        public static void AddActiveSessionEventListeners(
            ActiveSessionJoinedFunctionCallBack asJoinCB,
            ActiveSessionParticipantFunctionCallBack asPAddCB,
            ActiveSessionParticipantFunctionCallBack asPResyncCB,
            ActiveSessionParticipantFunctionCallBack asPDeleteCB)
        {
#if !HOLOLENS_BUILD
            I_AddActiveSessionEventListeners(asJoinCB, asPAddCB, asPResyncCB, asPDeleteCB);
#endif
        }

        public static void RemoveActiveSessionEventListeners()
        {
#if !HOLOLENS_BUILD
            I_RemoveActiveSessionEventListeners();
#endif
        }

        public static void AddEntityEventListeners(
            EntityFunctionCallBack entityCreateCB,
            EntityFunctionCallBack entityDestroyCB,
            EntityFunctionCallBack entityReinitCB,
            EntityFunctionCallBack entityUpdateCB,
            EntityFunctionCallBack entityEditCB)
        {
#if !HOLOLENS_BUILD
            I_AddEntityEventListeners(entityCreateCB, entityDestroyCB, entityReinitCB, entityUpdateCB, entityEditCB);
#endif
        }

        public static void RemoveEntityEventListeners()
        {
#if !HOLOLENS_BUILD
            I_RemoveEntityEventListeners();
#endif
        }

        /********************************************************************
         * EVENT ENTITY ATTRIBUTE FUNCTIONS (MANAGED OBJECT BRIDGE)
         ********************************************************************/
        public static long GetEventEntityAttributeCount()
        {
#if !HOLOLENS_BUILD
            return I_GetEventEntityAttributeCount();
#else
            return 0;
#endif
        }

        public static bool GetEventEntityAttributeIds(out string[] attributeIds)
        {
#if !HOLOLENS_BUILD
            long attributeIdsCount = 0;
            IntPtr attributeIdsArrayPtr = IntPtr.Zero;

            // Populate the attribute array
            I_GetEventEntityAttributeIds(
                out attributeIdsArrayPtr,
                out attributeIdsCount);

            // Build the managed string array
            MarshalUnmanagedStrArray2ManagedStrArray(attributeIdsArrayPtr, attributeIdsCount, out attributeIds);
#else
            attributeIds = new string[0];
#endif

            return true;
        }

        public static bool GetEventEntityAttributeType(string attributeId, out AttributeType type)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpType = 0;
            result = I_GetEventEntityAttributeType(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out tmpType);
            type = (AttributeType)tmpType;
#else
            type = default;
#endif

            return result;
        }

        public static bool GetEventEntityAttributeAsString(string attributeId, out string value)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            byte[] valueBytes = new byte[DATA_LEN];

            result = I_GetEventEntityAttributeAsString(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                valueBytes, DATA_LEN);
            if (result)
            {
                value = Encoding.ASCII.GetString(valueBytes).TrimEnd(NULL_CHAR);
            }
            else
            {
                value = "";
            }
#else
            value = default;
#endif

            return result;
        }

        public static bool GetEventEntityBlobAttribute(string attributeId, byte[] dest, long destsz, out int dataLen)
        {
#if !HOLOLENS_BUILD
            return I_GetEventEntityBlobAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                dest, destsz, out dataLen);
#else
            dataLen = 0;
            return false;
#endif
        }

        public static bool GetEventEntityStringAttribute(string attributeId, out string value)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            byte[] valueBytes = new byte[DATA_LEN];
            result = I_GetEventEntityStringAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                valueBytes, DATA_LEN);
            if (result)
            {
                value = Encoding.ASCII.GetString(valueBytes).TrimEnd(NULL_CHAR);
            }
            else
            {
                value = "";
            }
#else
            value = default;
#endif

            return result;
        }

        public static bool GetEventEntityBooleanAttribute(string attributeId, out bool value)
        {
#if !HOLOLENS_BUILD
            return I_GetEventEntityBooleanAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value);
#else
            value = default;
            return false;
#endif
        }

        public static bool GetEventEntityIntegerAttribute(string attributeId, out int value)
        {
#if !HOLOLENS_BUILD
            return I_GetEventEntityIntegerAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value);
#else
            value = default;
            return false;
#endif
        }

        public static bool GetEventEntityLongAttribute(string attributeId, out long value)
        {
#if !HOLOLENS_BUILD
            return I_GetEventEntityLongAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value);
#else
            value = default;
            return false;
#endif
        }

        public static bool GetEventEntityFloatAttribute(string attributeId, out float value)
        {
#if !HOLOLENS_BUILD
            return I_GetEventEntityFloatAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value);
#else
            value = default;
            return false;
#endif
        }

        public static bool GetEventEntityDoubleAttribute(string attributeId, out double value)
        {
#if !HOLOLENS_BUILD
            return I_GetEventEntityDoubleAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value);
#else
            value = default;
            return false;
#endif
        }

        public static bool GetEventEntityIntegerQuantityAttribute(string attributeId, out int value, out UnitType units)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            result = I_GetEventEntityIntegerQuantityAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value, out tmpUnits);
            units = (UnitType)tmpUnits;
#else
            units = default;
            value = default;
#endif

            return result;
        }

        public static bool GetEventEntityLongQuantityAttribute(string attributeId, out long value, out UnitType units)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            result = I_GetEventEntityLongQuantityAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value, out tmpUnits);
            units = (UnitType)tmpUnits;
#else
            units = default;
            value = default;
#endif

            return result;
        }

        public static bool GetEventEntityFloatQuantityAttribute(string attributeId, out float value, out UnitType units)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            result = I_GetEventEntityFloatQuantityAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value, out tmpUnits);
            units = (UnitType)tmpUnits;
#else
            units = default;
            value = default;
#endif

            return result;
        }

        public static bool GetEventEntityDoubleQuantityAttribute(string attributeId, out double value, out UnitType units)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            result = I_GetEventEntityDoubleQuantityAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value, out tmpUnits);
            units = (UnitType)tmpUnits;
#else
            units = default;
            value = default;
#endif

            return result;
        }

        public static bool GetEventEntityPosition(
            out double x, out double y, out double z,
            out UnitType units, out ReferenceSpaceType refSpace)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            int tmpRefSpace = 0;
            result = I_GetEventEntityPosition(
                out x, out y, out z,
                out tmpUnits, out tmpRefSpace);
            units = (UnitType)tmpUnits;
            refSpace = (ReferenceSpaceType)tmpRefSpace;
#else
            x = y = z = default;
            units = default;
            refSpace = default;
#endif

            return result;
        }

        public static bool GetEventEntityScale(
            out double x, out double y, out double z,
            out UnitType units, out ReferenceSpaceType refSpace)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            int tmpRefSpace = 0;
            result = I_GetEventEntityScale(
                out x, out y, out z,
                out tmpUnits, out tmpRefSpace);
            units = (UnitType)tmpUnits;
            refSpace = (ReferenceSpaceType)tmpRefSpace;
#else
            x = y = z = default;
            units = default;
            refSpace = default;
#endif

            return result;
        }

        public static bool GetEventEntityEulerRotation(
            out double x, out double y, out double z,
            out UnitType units, out ReferenceSpaceType refSpace)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            int tmpRefSpace = 0;
            result = I_GetEventEntityEulerRotation(
                out x, out y, out z,
                out tmpUnits, out tmpRefSpace);
            units = (UnitType)tmpUnits;
            refSpace = (ReferenceSpaceType)tmpRefSpace;
#else
            x = y = z = default;
            units = default;
            refSpace = default;
#endif

            return result;
        }

        public static bool GetEventEntityQRotation(
            out double x, out double y, out double z, out double w,
            out UnitType units, out ReferenceSpaceType refSpace)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            int tmpRefSpace = 0;
            result = I_GetEventEntityQRotation(
                out x, out y, out z, out w,
                out tmpUnits, out tmpRefSpace);
            units = (UnitType)tmpUnits;
            refSpace = (ReferenceSpaceType)tmpRefSpace;
#else
            x = y = z = w = default;
            units = default;
            refSpace = default;
#endif

            return result;
        }

        public static bool GetEventEntityTransform(
            out double xPos, out double yPos, out double zPos, out UnitType unitsPos, out ReferenceSpaceType refPos,
            out double xScl, out double yScl, out double zScl, out UnitType unitsScl, out ReferenceSpaceType refScl,
            out double xRot, out double yRot, out double zRot, out UnitType unitsRot, out ReferenceSpaceType refRot)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnitsPos = 0;
            int tmpUnitsScl = 0;
            int tmpUnitsRot = 0;
            int tmpRefPos = 0;
            int tmpRefScl = 0;
            int tmpRefRot = 0;
            result = I_GetEventEntityTransform(
                out xPos, out yPos, out zPos, out tmpUnitsPos, out tmpRefPos,
                out xScl, out yScl, out zScl, out tmpUnitsScl, out tmpRefScl,
                out xRot, out yRot, out zRot, out tmpUnitsRot, out tmpRefRot);
            unitsPos = (UnitType)tmpUnitsPos;
            unitsScl = (UnitType)tmpUnitsScl;
            unitsRot = (UnitType)tmpUnitsRot;
            refPos = (ReferenceSpaceType)tmpRefPos;
            refScl = (ReferenceSpaceType)tmpRefScl;
            refRot = (ReferenceSpaceType)tmpRefRot;
#else
            xPos = yPos = zPos = default;
            xScl = yScl = zScl = default;
            xRot = yRot = zRot = default;
            unitsPos = unitsScl = unitsRot = default;
            refPos = refScl = refRot = default;
#endif

            return result;
        }

        public static bool GetEventEntityQTransform(
            out double xPos, out double yPos, out double zPos, out UnitType unitsPos, out ReferenceSpaceType refPos,
            out double xScl, out double yScl, out double zScl, out UnitType unitsScl, out ReferenceSpaceType refScl,
            out double xRot, out double yRot, out double zRot, out double wRot, out UnitType unitsRot, out ReferenceSpaceType refRot)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnitsPos = 0;
            int tmpUnitsScl = 0;
            int tmpUnitsRot = 0;
            int tmpRefPos = 0;
            int tmpRefScl = 0;
            int tmpRefRot = 0;

            result = I_GetEventEntityQTransform(
                out xPos, out yPos, out zPos, out tmpUnitsPos, out tmpRefPos,
                out xScl, out yScl, out zScl, out tmpUnitsScl, out tmpRefScl,
                out xRot, out yRot, out zRot, out wRot, out tmpUnitsRot, out tmpRefRot);
            unitsPos = (UnitType)tmpUnitsPos;
            unitsScl = (UnitType)tmpUnitsScl;
            unitsRot = (UnitType)tmpUnitsRot;
            refPos = (ReferenceSpaceType)tmpRefPos;
            refScl = (ReferenceSpaceType)tmpRefScl;
            refRot = (ReferenceSpaceType)tmpRefRot;
#else
            xPos = yPos = zPos = default;
            xScl = yScl = zScl = default;
            xRot = yRot = zRot = wRot = default;
            unitsPos = unitsScl = unitsRot = default;
            refPos = refScl = refRot = default;
#endif

            return result;
        }

        /********************************************************************
         * EVENT ENTITY FUNCTIONS (MANAGED OBJECT BRIDGE)
         ********************************************************************/
        public static bool GetEventEntityId(out string value)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            byte[] valueBytes = new byte[ID_LEN];

            result = I_GetEventEntityId(valueBytes, ID_LEN);
            if (result)
            {
                value = Encoding.ASCII.GetString(valueBytes).TrimEnd(NULL_CHAR);
            }
            else
            {
                value = "";
            }
#else
            value = default;
#endif

            return result;
        }

        public static bool GetEventEntityType(out EntityType type)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpType = 0;
            result = I_GetEventEntityType(
                out tmpType);
            type = (EntityType)tmpType;
#else
            type = default;
#endif

            return result;
        }

        public static bool GetEventEntityAsString(out string value)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            byte[] valueBytes = new byte[DATA_LEN];

            result = I_GetEventEntityAsString(valueBytes, DATA_LEN);
            if (result)
            {
                value = Encoding.ASCII.GetString(valueBytes).TrimEnd(NULL_CHAR);
            }
            else
            {
                value = "";
            }
#else
            value = default;
#endif

            return result;
        }

        /*******************************************************************************
         * EVENT ACTIVE SESSION PARTICIPANT ATTRIBUTE FUNCTIONS (MANAGED OBJECT BRIDGE)
         *******************************************************************************/
        public static long GetEventActiveSessionParticipantAttributeCount()
        {
#if !HOLOLENS_BUILD
            return I_GetEventActiveSessionParticipantAttributeCount();
#else
            return 0;
#endif
        }

        public static bool GetEventActiveSessionParticipantAttributeIds(out string[] attributeIds)
        {
#if !HOLOLENS_BUILD
            long attributeIdsCount = 0;
            IntPtr attributeIdsArrayPtr = IntPtr.Zero;

            // Populate the attribute array
            I_GetEventActiveSessionParticipantAttributeIds(
                out attributeIdsArrayPtr,
                out attributeIdsCount);

            // Build the managed string array
            MarshalUnmanagedStrArray2ManagedStrArray(attributeIdsArrayPtr, attributeIdsCount, out attributeIds);
#else
            attributeIds = new string[0];
#endif

            return true;
        }

        public static bool GetEventActiveSessionParticipantAttributeType(string attributeId, out AttributeType type)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpType = 0;
            result = I_GetEventActiveSessionParticipantAttributeType(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out tmpType);
            type = (AttributeType)tmpType;
#else
            type = default;
#endif

            return result;
        }

        public static bool GetEventActiveSessionParticipantAttributeAsString(string attributeId, out string value)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            byte[] valueBytes = new byte[DATA_LEN];
            result = I_GetEventActiveSessionParticipantAttributeAsString(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                valueBytes, DATA_LEN);
            if (result)
            {
                value = Encoding.ASCII.GetString(valueBytes).TrimEnd(NULL_CHAR);
            }
            else
            {
                value = "";
            }
#else
            value = default;
#endif

            return result;
        }

        public static bool GetEventActiveSessionParticipantBlobAttribute(string attributeId, byte[] dest, long destsz, out int dataLen)
        {
#if !HOLOLENS_BUILD
            return I_GetEventActiveSessionParticipantBlobAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                dest, destsz, out dataLen);
#else
            dataLen = 0;
            return false;
#endif
        }

        public static bool GetEventActiveSessionParticipantStringAttribute(string attributeId, out string value)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            byte[] valueBytes = new byte[DATA_LEN];
            result = I_GetEventActiveSessionParticipantStringAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                valueBytes, DATA_LEN);
            if (result)
            {
                value = Encoding.ASCII.GetString(valueBytes).TrimEnd(NULL_CHAR);
            }
            else
            {
                value = "";
            }
#else
            value = default;
#endif

            return result;
        }

        public static bool GetEventActiveSessionParticipantBooleanAttribute(string attributeId, out bool value)
        {
#if !HOLOLENS_BUILD
            return I_GetEventActiveSessionParticipantBooleanAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value);
#else
            value = default;
            return false;
#endif
        }

        public static bool GetEventActiveSessionParticipantIntegerAttribute(string attributeId, out int value)
        {
#if !HOLOLENS_BUILD
            return I_GetEventActiveSessionParticipantIntegerAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value);
#else
            value = default;
            return false;
#endif
        }

        public static bool GetEventActiveSessionParticipantLongAttribute(string attributeId, out long value)
        {
#if !HOLOLENS_BUILD
            return I_GetEventActiveSessionParticipantLongAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value);
#else
            value = default;
            return false;
#endif
        }

        public static bool GetEventActiveSessionParticipantFloatAttribute(string attributeId, out float value)
        {
#if !HOLOLENS_BUILD
            return I_GetEventActiveSessionParticipantFloatAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value);
#else
            value = default;
            return false;
#endif
        }

        public static bool GetEventActiveSessionParticipantDoubleAttribute(string attributeId, out double value)
        {
#if !HOLOLENS_BUILD
            return I_GetEventActiveSessionParticipantDoubleAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value);
#else
            value = default;
            return false;
#endif
        }

        public static bool GetEventActiveSessionParticipantIntegerQuantityAttribute(string attributeId, out int value, out UnitType units)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            result = I_GetEventActiveSessionParticipantIntegerQuantityAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value, out tmpUnits);
            units = (UnitType)tmpUnits;
#else
            value = default;
            units = default;
#endif

            return result;
        }

        public static bool GetEventActiveSessionParticipantLongQuantityAttribute(string attributeId, out long value, out UnitType units)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            result = I_GetEventActiveSessionParticipantLongQuantityAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value, out tmpUnits);
            units = (UnitType)tmpUnits;
#else
            value = default;
            units = default;
#endif

            return result;
        }

        public static bool GetEventActiveSessionParticipantFloatQuantityAttribute(string attributeId, out float value, out UnitType units)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            result = I_GetEventActiveSessionParticipantFloatQuantityAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value, out tmpUnits);
            units = (UnitType)tmpUnits;
#else
            value = default;
            units = default;
#endif

            return result;
        }

        public static bool GetEventActiveSessionParticipantDoubleQuantityAttribute(string attributeId, out double value, out UnitType units)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            result = I_GetEventActiveSessionParticipantDoubleQuantityAttribute(
                Encoding.ASCII.GetBytes(attributeId + char.MinValue),
                out value, out tmpUnits);
            units = (UnitType)tmpUnits;
#else
            value = default;
            units = default;
#endif

            return result;
        }

        public static bool GetEventActiveSessionParticipantPosition(
            out double x, out double y, out double z,
            out UnitType units, out ReferenceSpaceType refSpace)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            int tmpRefSpace = 0;
            result = I_GetEventActiveSessionParticipantPosition(
                out x, out y, out z,
                out tmpUnits, out tmpRefSpace);
            units = (UnitType)tmpUnits;
            refSpace = (ReferenceSpaceType)tmpRefSpace;
#else
            x = y = z = default;
            units = default;
            refSpace = default;
#endif

            return result;
        }

        public static bool GetEventActiveSessionParticipantScale(
            out double x, out double y, out double z,
            out UnitType units, out ReferenceSpaceType refSpace)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            int tmpRefSpace = 0;
            result = I_GetEventActiveSessionParticipantScale(
                out x, out y, out z,
                out tmpUnits, out tmpRefSpace);
            units = (UnitType)tmpUnits;
            refSpace = (ReferenceSpaceType)tmpRefSpace;
#else
            x = y = z = default;
            units = default;
            refSpace = default;
#endif

            return result;
        }

        public static bool GetEventActiveSessionParticipantEulerRotation(
            out double x, out double y, out double z,
            out UnitType units, out ReferenceSpaceType refSpace)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            int tmpRefSpace = 0;
            result = I_GetEventActiveSessionParticipantEulerRotation(
                out x, out y, out z,
                out tmpUnits, out tmpRefSpace);
            units = (UnitType)tmpUnits;
            refSpace = (ReferenceSpaceType)tmpRefSpace;
#else
            x = y = z = default;
            units = default;
            refSpace = default;
#endif

            return result;
        }

        public static bool GetEventActiveSessionParticipantQRotation(
            out double x, out double y, out double z, out double w,
            out UnitType units, out ReferenceSpaceType refSpace)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnits = 0;
            int tmpRefSpace = 0;
            result = I_GetEventActiveSessionParticipantQRotation(
                out x, out y, out z, out w,
                out tmpUnits, out tmpRefSpace);
            units = (UnitType)tmpUnits;
            refSpace = (ReferenceSpaceType)tmpRefSpace;
#else
            x = y = z = w = default;
            units = default;
            refSpace = default;
#endif

            return result;
        }

        public static bool GetEventActiveSessionParticipantTransform(
            out double xPos, out double yPos, out double zPos, out UnitType unitsPos, out ReferenceSpaceType refPos,
            out double xScl, out double yScl, out double zScl, out UnitType unitsScl, out ReferenceSpaceType refScl,
            out double xRot, out double yRot, out double zRot, out UnitType unitsRot, out ReferenceSpaceType refRot)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnitsPos = 0;
            int tmpUnitsScl = 0;
            int tmpUnitsRot = 0;
            int tmpRefPos = 0;
            int tmpRefScl = 0;
            int tmpRefRot = 0;

            result = I_GetEventActiveSessionParticipantTransform(
                out xPos, out yPos, out zPos, out tmpUnitsPos, out tmpRefPos,
                out xScl, out yScl, out zScl, out tmpUnitsScl, out tmpRefScl,
                out xRot, out yRot, out zRot, out tmpUnitsRot, out tmpRefRot);
            unitsPos = (UnitType)tmpUnitsPos;
            unitsScl = (UnitType)tmpUnitsScl;
            unitsRot = (UnitType)tmpUnitsRot;
            refPos = (ReferenceSpaceType)tmpRefPos;
            refScl = (ReferenceSpaceType)tmpRefScl;
            refRot = (ReferenceSpaceType)tmpRefRot;
#else
            xPos = yPos = zPos = default;
            xScl = yScl = zScl = default;
            xRot = yRot = zRot = default;
            unitsPos = unitsScl = unitsRot = default;
            refPos = refScl = refRot = default;
#endif

            return result;
        }

        public static bool GetEventActiveSessionParticipantQTransform(
            out double xPos, out double yPos, out double zPos, out UnitType unitsPos, out ReferenceSpaceType refPos,
            out double xScl, out double yScl, out double zScl, out UnitType unitsScl, out ReferenceSpaceType refScl,
            out double xRot, out double yRot, out double zRot, out double wRot, out UnitType unitsRot, out ReferenceSpaceType refRot)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpUnitsPos = 0;
            int tmpUnitsScl = 0;
            int tmpUnitsRot = 0;
            int tmpRefPos = 0;
            int tmpRefScl = 0;
            int tmpRefRot = 0;
            result = I_GetEventActiveSessionParticipantQTransform(
                out xPos, out yPos, out zPos, out tmpUnitsPos, out tmpRefPos,
                out xScl, out yScl, out zScl, out tmpUnitsScl, out tmpRefScl,
                out xRot, out yRot, out zRot, out wRot, out tmpUnitsRot, out tmpRefRot);
            unitsPos = (UnitType)tmpUnitsPos;
            unitsScl = (UnitType)tmpUnitsScl;
            unitsRot = (UnitType)tmpUnitsRot;
            refPos = (ReferenceSpaceType)tmpRefPos;
            refScl = (ReferenceSpaceType)tmpRefScl;
            refRot = (ReferenceSpaceType)tmpRefRot;
#else
            xPos = yPos = zPos = default;
            xScl = yScl = zScl = default;
            xRot = yRot = zRot = wRot = default;
            unitsPos = unitsScl = unitsRot = default;
            refPos = refScl = refRot = default;
#endif

            return result;
        }

        /********************************************************************
         * EVENT ACTIVE SESSION PARTICIPANT FUNCTIONS (MANAGED OBJECT BRIDGE)
         ********************************************************************/
        public static bool GetEventActiveSessionParticipantId(out string value)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            byte[] valueBytes = new byte[ID_LEN];
            result = I_GetEventActiveSessionParticipantId(
                valueBytes, ID_LEN);
            if (result)
            {
                value = Encoding.ASCII.GetString(valueBytes).TrimEnd(NULL_CHAR);
            }
            else
            {
                value = "";
            }
#else
            value = default;
#endif

            return result;
        }

        public static bool GetEventActiveSessionParticipantType(out EntityType type)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            int tmpType = 0;
            result = I_GetEventActiveSessionParticipantType(
                out tmpType);
            type = (EntityType)tmpType;
#else
            type = default;
#endif

            return result;
        }

        public static bool GetEventActiveSessionParticipantAsString(out string value)
        {
            bool result = false;

#if !HOLOLENS_BUILD
            byte[] valueBytes = new byte[DATA_LEN];

            result = I_GetEventActiveSessionParticipantAsString(
                valueBytes, DATA_LEN);
            if (result)
            {
                value = Encoding.ASCII.GetString(valueBytes).TrimEnd(NULL_CHAR);
            }
            else
            {
                value = "";
            }
#else
            value = default;
#endif

            return result;
        }
        #endregion

    }
}
